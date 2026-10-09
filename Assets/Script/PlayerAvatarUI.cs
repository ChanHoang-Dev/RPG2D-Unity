using UnityEngine;
using UnityEngine.UI;

public class PlayerAvatarUI : MonoBehaviour
{
    [SerializeField] private Image avatarImage;
    private void OnEnable()
    {
        GameSessionManager.OnCharacterChanged += HandleCharacterChanged;
        RefreshDisplay();
    }
    private void OnDisable()
    {
        GameSessionManager.OnCharacterChanged -= HandleCharacterChanged;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void RefreshDisplay()
    {

        var manager = GameSessionManager.Instance;
        if (manager == null)
        {
            Debug.LogError("[PlayerAvatarUI] Không tìm thấy GameSessionManager.");
            return;
        }

        Debug.Log($"[PlayerAvatarUI] SelectedCharacter = {manager.SelectedCharacter}, thumbnail = {manager.SelectedCharacter?.thumbnail}");

        if (avatarImage != null)
            avatarImage.sprite = manager.SelectedCharacter.thumbnail;
        else
            Debug.LogError("[PlayerAvatarUI] avatarImage chưa được gán trong Inspector!");
    }
    private void HandleCharacterChanged(CharacterData data)
    {
        RefreshDisplay();
    }
}
