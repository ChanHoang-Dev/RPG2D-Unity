using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePreviewUi : MonoBehaviour
{
    [SerializeField] private GameObject upgradePreviewPanel;
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI levelCurrenttxt;
    [SerializeField] private TextMeshProUGUI levelNexttxt;
    [SerializeField] private TextMeshProUGUI valueCurrenttxt;
    [SerializeField] private TextMeshProUGUI valueNexttxt;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private GameObject arrowIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ShowUpgradePreview(string statDisplayName, int currentLevel, int maxLevel, string currentValue, string nextValue)
    {
        if (upgradePreviewPanel != null)
        {
            upgradePreviewPanel.SetActive(true);
        }
        bool isMax = currentLevel >= maxLevel;
        if (statNameText != null)
        {
            statNameText.text = statDisplayName;
        }
        if (levelCurrenttxt != null)
        {
            levelCurrenttxt.text = $"Level:{currentLevel}/{maxLevel}";
        }
        if (levelNexttxt != null)
        {
            levelNexttxt.gameObject.SetActive(!isMax);
            if (!isMax)
                levelNexttxt.text = $"Level: {currentLevel + 1}/{maxLevel}";
        }
        if (valueCurrenttxt != null)
        {
            valueCurrenttxt.text = $"{currentValue}";
        }
        if (valueNexttxt != null)
        {
            valueNexttxt.gameObject.SetActive(!isMax);
            if (!isMax)
                valueNexttxt.text = $"{nextValue}";
        }
        if (arrowIcon != null)
        {
            arrowIcon.SetActive(!isMax);
        }
        if (upgradeButton != null)
        {
            upgradeButton.interactable = !isMax;
        }
        if (upgradeButtonText != null)
        {
            upgradeButtonText.text = isMax ? "Max" : "Upgrade";
        }
    }
    public void HideUpgradePreview()
    {
        if (upgradePreviewPanel != null)
        {
            upgradePreviewPanel.SetActive(false);
        }
    }
}
