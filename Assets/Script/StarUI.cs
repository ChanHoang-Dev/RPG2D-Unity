using UnityEngine;
using UnityEngine.UI;

public class StarUI : MonoBehaviour
{
    [Tooltip("Kéo lần lượt Image FILL VÀNG (con thứ 2) của FrameStar, FrameStar (1)...FrameStar (4) vào đây theo đúng thứ tự trái -> phải. Không cần kéo phần khung vì khung luôn hiện đủ 100%.")]
    [SerializeField] private Image[] starFills = new Image[5];

    [Tooltip("Làm tròn tới bội số này, VD 0.5f = làm tròn tới nửa sao (4.3 -> 4.5)")]
    [SerializeField] private float roundStep = 0.5f;

    public void SetRating(float rating)
    {
        rating = Mathf.Clamp(rating, 0f, starFills.Length);

        if (roundStep > 0f)
        {
            rating = Mathf.Round(rating / roundStep) * roundStep;
        }

        for (int i = 0; i < starFills.Length; i++)
        {
            if (starFills[i] == null) continue;

            // Mỗi sao thứ i phụ trách khoảng [i, i+1) trên thang điểm rating
            float fill = Mathf.Clamp01(rating - i);

            starFills[i].fillAmount = fill;
        }
    }
}