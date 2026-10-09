using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CharacterSlotUI : MonoBehaviour
{
    [Header("Data")]
    public CharacterData characterData;

    [Header("Reference")]
    public CharacterPreviewController previewController;
    public CharacterUpgradeController upgradeController;

    [Header("Visual State")]
    [SerializeField] private Image FrameImage;
    [SerializeField] private Image IconImage;
    [SerializeField] private GameObject selectedFrame;

    void Start()
    {
    }

    public void OnSlotClicked()
    {
        previewController.ShowCharacter(characterData);
        upgradeController.SelectCharacter(characterData);
    }
    public void RefreshState(bool isCurrentlySelected)
    {
        bool isUnlocked = characterData.isUnlocked;
        Color targetColor = isUnlocked ? Color.white : Color.black;

        if(FrameImage != null)
            FrameImage.color = targetColor;
        if(IconImage != null)
            IconImage.color = targetColor;
        if(selectedFrame != null)
            selectedFrame.SetActive(isUnlocked && isCurrentlySelected);
    }
}