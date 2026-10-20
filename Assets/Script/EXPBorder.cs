using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Viền EXP chạy quanh khung chữ nhật + hiển thị Level của nhân vật đang chọn.
/// Dữ liệu lấy tự động từ GameSessionManager.SelectedCharacter:
///   - EXP đã lưu của nhân vật (PlayerStatsRuntime.TotalExp)
///   - cộng thêm EXP đang chờ chốt trong màn (StageExpTracker.GetFinalExp), nên đánh quái là viền chạy ngay.
/// Khi EXP vượt ngưỡng: viền chạy đầy -> Level tăng, hiệu ứng nảy -> viền reset về 0 rồi chạy tiếp.
///
/// Inspector: expBorderImage = Image (Type: Filled, Radial 360, Origin: Top, Clockwise bật).
/// </summary>
public class EXPBorder : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image expBorderImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [Tooltip("Tùy chọn: hiện 'EXP hiện tại / EXP cần' của level. Để trống nếu không dùng.")]
    [SerializeField] private TextMeshProUGUI expText;

    [Header("Text Format")]
    [SerializeField] private string levelFormat = "Lv.{0}";
    [SerializeField] private string expFormat = "{0}/{1}";
    [SerializeField] private string maxLevelText = "MAX";

    [Header("Animation")]
    [Tooltip("Tốc độ chạy của viền: phần trăm chu vi mỗi giây (1 = đầy một vòng trong 1 giây).")]
    [SerializeField] private float fillSpeed = 1.2f;
    [SerializeField] private float levelUpPulseScale = 1.3f;
    [SerializeField] private float levelUpPulseTime = 0.35f;

    [Header("Events")]
    [Tooltip("Bắn ra mỗi khi viền chạy đầy và Level hiển thị tăng (gắn VFX / âm thanh ở đây).")]
    public UnityEvent onLevelUp;

    private CharacterData boundCharacter;
    private bool initialized;

    // Giá trị đang HIỂN THỊ (chạy đuổi theo giá trị thật)
    private int shownLevel;
    private float shownProgress;   // 0..1 theo chu vi

    // Cache để không tạo string mỗi frame
    private int drawnLevel = -1;
    private int drawnExp = -1;

    private float pulseTimer;
    private Vector3 levelTextBaseScale = Vector3.one;

    private void Awake()
    {
        if (levelText != null) levelTextBaseScale = levelText.transform.localScale;
    }

    private void OnEnable()
    {
        initialized = false;
        drawnLevel = -1;
        drawnExp = -1;
    }

    private void Update()
    {
        GameSessionManager gsm = GameSessionManager.Instance;
        CharacterData character = gsm != null ? gsm.SelectedCharacter : null;
        if (character == null || character.playerStatsRuntime == null || expBorderImage == null) return;

        ReadTarget(character, out int targetLevel, out float targetProgress);

        if (!initialized || character != boundCharacter)
        {
            // Lần đầu hoặc đổi nhân vật: gán thẳng, không chạy animation
            boundCharacter = character;
            initialized = true;
            shownLevel = targetLevel;
            shownProgress = targetProgress;
        }
        else
        {
            Animate(targetLevel, targetProgress, Time.unscaledDeltaTime);
        }

        expBorderImage.fillAmount = PerimeterToAngle01(shownProgress);
        RefreshTexts();
        UpdatePulse(Time.unscaledDeltaTime);
    }

    // ------------------------------------------------------------------
    // Lấy level + tiến độ THẬT (đã gồm EXP đang chờ chốt của màn hiện tại)
    // ------------------------------------------------------------------
    private static void ReadTarget(CharacterData character, out int level, out float progress)
    {
        long total = character.playerStatsRuntime.TotalExp;

        // Sau Commit(), EXP đã nằm trong TotalExp rồi -> không cộng thêm nữa (tránh tính 2 lần)
        if (!StageExpTracker.IsCommitted)
            total += StageExpTracker.GetFinalExp();

        total = System.Math.Min(total, CharacterProgression.MaxTotalExp);

        level = CharacterProgression.LevelFromTotalExp((int)total);
        if (level >= CharacterProgression.MaxLevel)
        {
            progress = 1f;
            return;
        }

        int into = (int)total - CharacterProgression.TotalExpForLevel(level);
        int need = Mathf.Max(1, CharacterProgression.ExpToNextLevel(level));
        progress = Mathf.Clamp01((float)into / need);
    }

    // ------------------------------------------------------------------
    // Animation: chạy đầy vòng -> lên level -> reset 0 -> chạy tiếp
    // ------------------------------------------------------------------
    private void Animate(int targetLevel, float targetProgress, float dt)
    {
        if (shownLevel > targetLevel)
        {
            // EXP bị giảm / reset save -> gán thẳng
            shownLevel = targetLevel;
            shownProgress = targetProgress;
            return;
        }

        if (shownLevel < targetLevel)
        {
            // Nhảy nhiều level cùng lúc thì chạy nhanh hơn cho đỡ lâu
            float speed = fillSpeed * (1 + targetLevel - shownLevel);
            shownProgress = Mathf.MoveTowards(shownProgress, 1f, speed * dt);

            if (shownProgress >= 1f)
            {
                shownLevel++;
                shownProgress = 0f;
                pulseTimer = levelUpPulseTime;
                onLevelUp?.Invoke();
            }
            return;
        }

        // Cùng level
        if (targetProgress < shownProgress)
            shownProgress = targetProgress; // giảm (reset) -> không chạy lùi
        else
            shownProgress = Mathf.MoveTowards(shownProgress, targetProgress, fillSpeed * dt);
    }

    // ------------------------------------------------------------------
    // Text
    // ------------------------------------------------------------------
    private void RefreshTexts()
    {
        if (levelText != null && drawnLevel != shownLevel)
        {
            drawnLevel = shownLevel;
            levelText.text = string.Format(levelFormat, shownLevel);
        }

        if (expText != null)
        {
            bool isMax = shownLevel >= CharacterProgression.MaxLevel;
            int need = isMax ? 0 : CharacterProgression.ExpToNextLevel(shownLevel);
            int into = isMax ? 0 : Mathf.RoundToInt(shownProgress * need);

            // drawnExp gộp level + into để đổi level cũng vẽ lại
            int key = into + shownLevel * 1000003;
            if (drawnExp != key)
            {
                drawnExp = key;
                expText.text = isMax ? maxLevelText : string.Format(expFormat, into, need);
            }
        }
    }

    private void UpdatePulse(float dt)
    {
        if (levelText == null || pulseTimer <= 0f) return;

        pulseTimer -= dt;
        float t = 1f - Mathf.Clamp01(pulseTimer / Mathf.Max(0.01f, levelUpPulseTime));
        levelText.transform.localScale = levelTextBaseScale * Mathf.Lerp(levelUpPulseScale, 1f, t);
    }

    // ------------------------------------------------------------------
    // Quy đổi % chu vi -> % góc (Radial 360 chạy theo góc nên cạnh dài/ngắn sẽ lệch nếu không đổi)
    // Bắt đầu từ giữa cạnh trên, quay thuận chiều kim đồng hồ.
    // ------------------------------------------------------------------
    private float PerimeterToAngle01(float p)
    {
        if (p <= 0f) return 0f;
        if (p >= 1f) return 1f;

        Rect r = ((RectTransform)expBorderImage.transform).rect;
        float W = r.width, H = r.height;
        float hw = W * 0.5f, hh = H * 0.5f;
        float d = p * 2f * (W + H);
        float x, y;

        if (d <= hw)                { x = d;                         y = hh; }                       // giữa trên -> góc phải trên
        else if (d <= hw + H)       { x = hw;                        y = hh - (d - hw); }            // cạnh phải
        else if (d <= hw + H + W)   { x = hw - (d - hw - H);         y = -hh; }                      // cạnh dưới
        else if (d <= hw + 2*H + W) { x = -hw;                       y = -hh + (d - hw - H - W); }   // cạnh trái
        else                        { x = -hw + (d - hw - 2*H - W);  y = hh; }                       // nửa trái cạnh trên

        float angle = Mathf.Atan2(x, y);
        if (angle < 0f) angle += 2f * Mathf.PI;
        return angle / (2f * Mathf.PI);
    }
}