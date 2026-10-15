using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float moveUpSpeed = 1.5f;
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private float fadeStartTime = 0.4f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.black;
    [SerializeField] private Color critColor = new Color(1f, 0.84f, 0f); // vàng
    [SerializeField] private Color missColor = Color.gray;
    [SerializeField] private Color trueDamageColor = Color.white; // đỏ tươi
    [SerializeField] private Color expColor = new Color(0.1f, 0.5f, 1f); // xanh dương
    [Header("EXP Popup")]
    [Tooltip("EXP hiện lâu hơn số sát thương bấy nhiêu lần để kịp đọc")]
    [SerializeField] private float expLifetimeMultiplier = 1.5f;

    private TextMeshPro textMesh;
    private float timer;
    private Color currentColor;
    private float activeLifetime;
    private float activeFadeStart;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        activeLifetime = lifetime;
        activeFadeStart = fadeStartTime;
    }

    /// <summary>Gọi ngay sau khi Instantiate để set nội dung hiển thị</summary>
    public void Setup(DamageResult result)
    {
        if (result.isMiss)
        {
            textMesh.text = "Miss";
            currentColor = missColor; // Miss màu trắng cho nổi bật
        }
        else
        {
            // -1 nghĩa là bị chặn gần hết, vẫn hiện số âm để người chơi biết đòn bị giáp chặn mạnh
            textMesh.text = "-" + result.amount.ToString();
            currentColor = result.isTrueDamage ? trueDamageColor : (result.isCrit ? critColor : normalColor);
        }

        textMesh.color = currentColor;
        textMesh.fontSize = result.isCrit ? 30f : 28f; // crit chữ to hơn cho nổi bật
        timer = 0f;
    }
    public void SetupExp(int exp)
    {
        textMesh.text = $"+{exp} EXP";
        currentColor = expColor;
        textMesh.color = currentColor;
 
        activeLifetime = lifetime * expLifetimeMultiplier;
        activeFadeStart = fadeStartTime * expLifetimeMultiplier;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;

        if (timer >= fadeStartTime)
        {
            float fadeProgress = (timer - fadeStartTime) / (lifetime - fadeStartTime);
            currentColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
            textMesh.color = currentColor;
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}