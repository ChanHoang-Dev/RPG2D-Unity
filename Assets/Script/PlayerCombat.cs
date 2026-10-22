using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private WeaponHitbox weaponHitbox;

    [System.Serializable]
    public class AttackData
    {
        [Tooltip("Tên STATE trong Animator Controller (không phải tên clip). Ví dụ: Attack1, Attack2...")]
        public string stateName;

        [Tooltip("Thời lượng khoá hành động (giây). Thường để = độ dài animation thật của đòn này. " +
                 "Kéo clip vào ô bên dưới để tool tự điền, hoặc tự nhập tay nếu build ra ngoài Editor.")]
        public float lockDuration = 0.3f;

#if UNITY_EDITOR
        [Tooltip("CHỈ dùng trong Editor để tự động điền lockDuration ở trên. Không ảnh hưởng lúc runtime/build.")]
        public AnimationClip editorClipRef;
#endif
    }

    [Header("Combo Attack")]
    [Tooltip("Danh sách các đòn combo theo thứ tự 1,2,3... Nhân vật chỉ có 1 hoặc 2 đòn thì để đúng số phần tử tương ứng.")]
    public AttackData[] attacks = new AttackData[]
    {
        new AttackData { stateName = "Attack1", lockDuration = 0.3f },
        new AttackData { stateName = "Attack2", lockDuration = 0.3f },
        new AttackData { stateName = "Attack3", lockDuration = 0.3f },
    };

    [Tooltip("Từ % thời lượng animation này trở đi mới cho phép ghi nhận input combo tiếp theo (0-1)")]
    [Range(0f, 1f)] public float comboBufferOpenAt = 0.6f;
    [Tooltip("Sau % thời lượng này thì không nhận buffer nữa (animation gần kết thúc, tránh giật)")]
    [Range(0f, 1f)] public float comboBufferCloseAt = 0.95f;

    [Tooltip("Đánh hết đòn cuối rồi bấm tiếp có quay vòng lại đòn 1 không, hay phải thả tay ra đánh lại từ đầu")]
    public bool loopComboAtEnd = false;
    [Header("Attack Speed")]
    [Tooltip("Lấy attack speed của nhân vật từ CombatEntity (level càng cao càng nhanh). Tự tìm nếu để trống.")]
    [SerializeField] private CombatEntity combatEntity;
    [SerializeField] private float minAttackSpeed = 0.5f;
    [SerializeField] private float maxAttackSpeed = 3f;
    private float attackSpeed = 1f;
    public float AttackSpeed => attackSpeed;
    private PlayerController controller;
    private Animator animator;

    private int currentComboIndex = 0;       // 1-based: 1 = attacks[0], 2 = attacks[1] ...
    private bool comboInputBuffered = false; // đã bấm J trong lúc combo đang mở, chờ chuyển đòn tiếp theo
    private bool isAttacking = false;

    // Số lần StartComboAttack đã được gọi, dùng để phân biệt "đòn hiện tại" với "đòn mới hơn"
    // trong callback ForceResetAttackState (tránh reset nhầm đè lên đòn đã StartComboAttack sau đó).
    private int attackToken = 0;
    private int _pendingResetToken = -1;

    public bool IsAttacking => isAttacking;

    void Awake()
    {
        controller = GetComponent<PlayerController>();

        if (weaponHitbox == null) weaponHitbox = GetComponentInChildren<WeaponHitbox>();
        if (combatEntity == null) combatEntity = GetComponent<CombatEntity>();
    }

    void OnEnable()
    {
        CharacterProgression.OnCharacterLevelChanged += HandleLevelChanged;
    }

    void OnDisable()
    {
        CharacterProgression.OnCharacterLevelChanged -= HandleLevelChanged;
    }

    // Lên level giữa màn -> tốc độ đánh mới áp dụng từ đòn kế tiếp
    private void HandleLevelChanged(CharacterData data, int oldLevel, int newLevel)
    {
        GameSessionManager gsm = GameSessionManager.Instance;
        if (gsm != null && data == gsm.SelectedCharacter)
            RefreshAttackSpeed();
    }

    void Start()
    {
        animator = controller.Animator;
        RefreshAttackSpeed();
    }

    /// <summary>Gọi từ PlayerController.Update() mỗi frame — xử lý input J.</summary>
    public void TickInput()
    {
        if (attacks == null || attacks.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (!isAttacking && !controller.IsActionLocked)
            {
                // Chưa đánh gì cả (hoặc rảnh tay) -> bắt đầu combo từ đòn 1
                StartComboAttack(1);
            }
            else if (isAttacking)
            {
                // Đang giữa 1 đòn đánh -> ghi nhận buffer.
                comboInputBuffered = true;
                // Thử xử lý ngay trong frame này luôn (không đợi tới TickComboTimer của frame sau),
                // để tránh trường hợp bấm J đúng frame combo window vừa mở nhưng bị lệch thứ tự gọi hàm.
                TryConsumeComboBuffer();
            }
        }
    }

    /// <summary>Gọi từ PlayerController.Update() mỗi frame, sau TickInput() — theo dõi tiến trình đòn đánh.</summary>
    public void TickComboTimer()
    {
        if (!isAttacking) return;
        if (currentComboIndex <= 0 || currentComboIndex > attacks.Length) return;

        TryConsumeComboBuffer();

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string expectedState = attacks[currentComboIndex - 1].stateName;

        // Animator có thể còn đang crossfade từ state trước sang state này (transition duration > 0),
        // trong lúc đó IsName có thể chưa true ngay -> đợi tới khi thực sự vào đúng state rồi mới tính progress.
        if (!stateInfo.IsName(expectedState)) return;

        float progress = stateInfo.normalizedTime; // 0 -> 1 (có thể > 1 nếu animation loop, nên clamp khi so sánh)

        if (progress >= 1f)
        {
            // Đòn đánh kết thúc mà không có input buffer nào kịp (hoặc đã là đòn cuối, không loop) -> reset combo
            isAttacking = false;
            currentComboIndex = 0;
            comboInputBuffered = false;
            ResetAnimatorSpeed();

            // FAIL-SAFE: đảm bảo hitbox đã tắt khi animation Attack thực sự kết thúc, phòng trường hợp
            // Animation Event DisableHitbox() bị bỏ lỡ (ví dụ animation kết thúc đúng lúc frame đó bị skip).
            DisableWeaponHitbox();
        }
    }

    void StartComboAttack(int index)
    {
        // Kẹp index trong khoảng hợp lệ theo số đòn thực tế của nhân vật này (1 hoặc 2 đòn cũng chạy đúng)
        index = Mathf.Clamp(index, 1, attacks.Length);

        currentComboIndex = index;
        isAttacking = true;
        comboInputBuffered = false;

        AttackData data = attacks[index - 1];

        animator.SetInteger("AttackIndex", index);
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

        animator.speed = attackSpeed;
        float scaledLock = data.lockDuration / attackSpeed;
        controller.StartActionLock(scaledLock);

        // --- FAIL-SAFE ---
        // Nếu vì lý do gì đó (state trùng tên với nhân vật khác, Any State transition đá lệch,
        // transition duration làm IsName() không bao giờ khớp...) mà TickComboTimer() không bao giờ
        // detect được progress >= 1f, isAttacking sẽ bị kẹt "true" vĩnh viễn -> J không còn tác dụng nữa.
        // Đoạn này đảm bảo combo luôn được giải phóng trễ nhất là sau lockDuration, bất kể Animator ra sao.
        attackToken++;
        int myToken = attackToken;
        CancelInvoke(nameof(ForceResetAttackStateInvokeWrapper));
        Invoke(nameof(ForceResetAttackStateInvokeWrapper), scaledLock + 0.1f);
        _pendingResetToken = myToken;
    }

    // Invoke() không cho truyền tham số trực tiếp nên dùng field trung gian _pendingResetToken
    void ForceResetAttackStateInvokeWrapper()
    {
        ForceResetAttackState(_pendingResetToken);
    }

    void ForceResetAttackState(int token)
    {
        // Chỉ reset nếu token này vẫn là đòn "mới nhất" (chưa bị StartComboAttack nào khác ghi đè)
        // và vẫn đang kẹt ở trạng thái isAttacking. Nếu combo đã tự kết thúc bình thường qua
        // TickComboTimer() thì isAttacking đã là false từ trước, đoạn này không làm gì cả.
        if (token != attackToken) return;
        if (!isAttacking) return;

        isAttacking = false;
        currentComboIndex = 0;
        comboInputBuffered = false;
        ResetAnimatorSpeed();

        // FAIL-SAFE: đây đúng là trường hợp Animator không bao giờ chạy tới Animation Event
        // DisableHitbox() (state trùng tên, transition đá lệch...). Tắt cứng tại đây để hitbox
        // không bị kẹt bật vĩnh viễn cho tới đòn chém kế tiếp.
        DisableWeaponHitbox();
    }

    // Kiểm tra và xử lý buffer nếu đang trong cửa sổ hợp lệ. Được gọi cả từ TickInput (ngay khi bấm)
    // lẫn từ TickComboTimer (mỗi frame), để không bỏ lỡ khung hình nào.
    void TryConsumeComboBuffer()
    {
        if (!isAttacking || !comboInputBuffered) return;
        if (currentComboIndex <= 0 || currentComboIndex > attacks.Length) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string expectedState = attacks[currentComboIndex - 1].stateName;
        if (!stateInfo.IsName(expectedState)) return;

        float clamped = Mathf.Min(stateInfo.normalizedTime, 1f);

        // Đã bấm sớm hơn comboBufferOpenAt -> chưa xử lý vội, giữ nguyên buffer chờ tới khi cửa sổ mở.
        if (clamped < comboBufferOpenAt) return;

        // Bỏ giới hạn trên (comboBufferCloseAt) khỏi điều kiện HUỶ buffer: dù bấm hơi trễ (qua mốc close)
        // vẫn cho ăn đòn tiếp theo, miễn animation hiện tại chưa thật sự kết thúc (progress < 1f).
        // Điều này tránh tình trạng bấm J nhưng bị "nuốt" input do bấm đúng vùng [close, 1.0).
        if (clamped >= 1f) return; // animation đã xong, để TickComboTimer xử lý reset bên ngoài

        bool isLastAttack = currentComboIndex >= attacks.Length;

        if (!isLastAttack)
        {
            StartComboAttack(currentComboIndex + 1);
        }
        else if (loopComboAtEnd)
        {
            StartComboAttack(1);
        }
        else
        {
            // Đòn cuối, không loop -> không tiêu buffer, để animation tự kết thúc và reset combo bình thường.
        }
    }

    /// <summary>
    /// Huỷ combo đang dở ngay lập tức. Gọi bởi PlayerHealth khi bị đánh trúng (Hurt) hoặc chết (Die),
    /// tránh animation Attack đè lên Hurt/Death.
    /// </summary>
    public void CancelCombo()
    {
        isAttacking = false;
        currentComboIndex = 0;
        comboInputBuffered = false;
        attackToken++; // vô hiệu hoá mọi ForceResetAttackState đang chờ từ đòn trước đó
        CancelInvoke(nameof(ForceResetAttackStateInvokeWrapper));
        ResetAnimatorSpeed();

        // FAIL-SAFE: nếu animation Attack bị cắt ngang giữa chừng (ví dụ do Hurt/Death chen vào)
        // trước khi kịp chạy tới Animation Event DisableHitbox(), collider sẽ bị kẹt bật mãi mãi.
        // Luôn tắt cứng tại đây để đảm bảo không còn hitbox nào sót lại sau khi combo bị huỷ.
        DisableWeaponHitbox();
    }
    public void RefreshAttackSpeed()
    {
        float value = combatEntity != null ? combatEntity.GetAttackSpeed() : 1f;
        attackSpeed = Mathf.Clamp(value, minAttackSpeed, maxAttackSpeed);
    }

    // animator.speed chỉ được tăng trong lúc đánh. Ngoài lúc đánh (chạy, nhảy, hurt, chết) luôn là 1.
    private void ResetAnimatorSpeed()
    {
        if (animator != null) animator.speed = 1f;
    }
    public void EnableWeaponHitbox()
    {
        if (weaponHitbox != null)
            weaponHitbox.EnableHitbox();
    }

    public void DisableWeaponHitbox()
    {
        if (weaponHitbox != null)
            weaponHitbox.DisableHitbox();
    }
}