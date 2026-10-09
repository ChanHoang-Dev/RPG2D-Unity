using UnityEngine;

/// <summary>
/// Gắn component này lên GameObject "skeletonDie_Skull" (child của Skeleton chính, cùng cấp AttackPoint).
/// Mặc định object này nên ẩn/inactive từ đầu, chỉ hiện ra và văng đi khi được gọi Trigger().
///
/// Setup Animation Event trên clip Die của SKELETON CHÍNH (không phải trên clip skull):
/// - Tại đúng frame "chỉ còn thân, đầu lìa ra" -> gọi hàm forward trên SkeletonLow (xem bước dưới)
/// </summary>
public class SkullDetach : MonoBehaviour
{
    [Header("Fly Settings")]
    [SerializeField] private float flySpeed = 4f;
    [SerializeField] private float flyDuration = 1.2f; // Sau thời gian này thì tự Destroy

    [Header("References")]
    [SerializeField] private Animator skullAnimator; // Animator riêng của skull (animation xoay)

    private Vector2 direction = Vector2.right;
    private float timer = 0f;
    private bool isFlying = false;

    void Awake()
    {
        if (skullAnimator == null)
            skullAnimator = GetComponent<Animator>();

        gameObject.SetActive(false); // Ẩn sẵn từ đầu, chỉ hiện khi Trigger() được gọi
    }

    void Update()
    {
        if (!isFlying) return;

        timer += Time.deltaTime;
        if (timer >= flyDuration)
        {
            isFlying = false;
            return;
        }
        transform.position += (Vector3)(direction * flySpeed * Time.deltaTime);
    }
    public void Trigger(bool facingRight)
    {
        gameObject.SetActive(true);

        direction = facingRight ? Vector2.right : Vector2.left;
        isFlying = true;
        timer = 0f;

        if (skullAnimator != null)
        {
            skullAnimator.enabled = true;
            // Nếu animation xoay có riêng 1 state/trigger, có thể gọi thêm ở đây, ví dụ:
            // skullAnimator.SetTrigger("Spin");
        }
    }
}