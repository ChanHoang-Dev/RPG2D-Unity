using UnityEngine;
using UnityEngine.UI;

public class AvatarSlot : MonoBehaviour
{
    public Image avatarImage;       
    public GameObject checkmark;    

    public Image AvatarImage => avatarImage;
    private UIStartScene uiStartScene;

    public void Initialize(UIStartScene panel)
    {
        uiStartScene = panel;
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        uiStartScene.SelectSlot(this);
    }

    public void SetSelected(bool isSelected)
    {
        checkmark.SetActive(isSelected);
    }
}