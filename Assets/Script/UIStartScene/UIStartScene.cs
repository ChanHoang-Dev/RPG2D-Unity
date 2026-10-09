using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIStartScene : MonoBehaviour
{
    [SerializeField] private GameObject PanelBackGround;
    [SerializeField] private GameObject PanelFrameAvatar;
    [SerializeField] private Button BackPanelBackGroundFromPanelFrameAvatar;
    [Header("Avatar Slots")]
    [SerializeField] private List<AvatarSlot> avatarSlots;
    [SerializeField] private Button DoneButton;
    [SerializeField] private Image targetCharacterSpriteRenderer;
    [Header("Mode")]
    [SerializeField] private Button CharacterSelectButton;

    private AvatarSlot currentSelectedSlot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PanelFrameAvatar.SetActive(false);
        BackPanelBackGroundFromPanelFrameAvatar.onClick.RemoveAllListeners();
        BackPanelBackGroundFromPanelFrameAvatar.onClick.AddListener(OnClickBack);

        DoneButton.onClick.RemoveAllListeners();
        DoneButton.onClick.AddListener(OnClickDone);

        foreach (var slot in avatarSlots)
        {
            slot.Initialize(this);
            slot.SetSelected(false);
        }
        LoadSavedAvatarSelection();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void LoadSavedAvatarSelection()
    {
        int savedIndex = PlayerProfileManager.LoadAvatarIndex();
        if (savedIndex < 0 || savedIndex >= avatarSlots.Count)
        {
            Debug.Log("[UIStartScene] Không có avatar đã lưu hoặc index không hợp lệ, chọn avatar đầu tiên.");
            return;
        }

        AvatarSlot savedSlot = avatarSlots[savedIndex];
        currentSelectedSlot = savedSlot;
        savedSlot.SetSelected(true);

        if (currentSelectedSlot != null)
        {
            targetCharacterSpriteRenderer.sprite = currentSelectedSlot.AvatarImage.sprite;
        }
    }
    public void OnClickAvatar()
    {
        PanelFrameAvatar.SetActive(true);
        PanelBackGround.SetActive(false);
    }

    public void OnClickBack()
    {
        PanelFrameAvatar.SetActive(false);
        PanelBackGround.SetActive(true);
    }
    public void OnClickDone()
    {
        if (currentSelectedSlot != null)
        {
            targetCharacterSpriteRenderer.sprite = currentSelectedSlot.AvatarImage.sprite;
            int indexSelect = avatarSlots.IndexOf(currentSelectedSlot);
            if (indexSelect >= 0)
                PlayerProfileManager.SaveAvatarIndex(indexSelect);
        }
        OnClickBack();
    }
    public void SelectSlot(AvatarSlot slot)
    {
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelected(false);
        }
        currentSelectedSlot = slot;
        currentSelectedSlot.SetSelected(true);
    }
    public void OnClickActiveSelectMap()
    {
        SceneManager.LoadScene("SelectMap");
    }
}
