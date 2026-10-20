using UnityEngine;
using UnityEngine.UI;

public class ExpBorder : MonoBehaviour
{
    [SerializeField] Image expBorderImage;      
    [SerializeField] float smoothSpeed = 5f;

    float targetProgress;   // 0..1 theo chu vi
    float currentProgress;

    public void SetExp(float current, float max)
    {
        targetProgress = Mathf.Clamp01(current / max);
    }

    void Update()
    {
        currentProgress = Mathf.MoveTowards(currentProgress, targetProgress,
                                            smoothSpeed * Time.deltaTime);
        expBorderImage.fillAmount = PerimeterToAngle01(currentProgress);
    }

    // Quy đổi % chu vi -> % góc (bắt đầu từ giữa cạnh trên, quay thuận chiều kim đồng hồ)
    float PerimeterToAngle01(float p)
    {
        if (p <= 0f) return 0f;
        if (p >= 1f) return 1f;

        Rect r = ((RectTransform)expBorderImage.transform).rect;
        float W = r.width, H = r.height;
        float hw = W * 0.5f, hh = H * 0.5f;
        float d = p * 2f * (W + H);   // quãng đường đã đi
        float x, y;

        if (d <= hw)                 { x = d;       y = hh; }            // giữa trên -> góc phải trên
        else if (d <= hw + H)        { x = hw;      y = hh - (d - hw); } // cạnh phải
        else if (d <= hw + H + W)    { x = hw - (d - hw - H); y = -hh; } // cạnh dưới
        else if (d <= hw + 2*H + W)  { x = -hw;     y = -hh + (d - hw - H - W); } // cạnh trái
        else                         { x = -hw + (d - hw - 2*H - W); y = hh; }    // cạnh trên (nửa trái)

        float angle = Mathf.Atan2(x, y);              // góc tính từ trục hướng lên, thuận chiều kim đồng hồ
        if (angle < 0f) angle += 2f * Mathf.PI;
        return angle / (2f * Mathf.PI);
    }
}