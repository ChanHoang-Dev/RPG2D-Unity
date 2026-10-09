using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterButtonManager : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private string characterSelectedSceneName = "CharacterSelection";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        RefreshDisplay();
    }
    private void OnEnable()
    {
        GameSessionManager.OnCharacterChanged += HandleCharacterChanged;
    }
    private void OnDisable()
    {
        GameSessionManager.OnCharacterChanged -= HandleCharacterChanged;
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
            Debug.LogError("[CharacterButtonManager] Không tìm thấy GameSessionManager trong scene này. Hãy chắc chắn rằng có 1 GameSessionManager trong scene.");
            return;
        }
        characterImage.sprite = manager.SelectedCharacter.thumbnail;
    }
    private void HandleCharacterChanged(CharacterData data)
    {
        RefreshDisplay();
    }
    public void OnCharacterButtonClicked()
    {
        SceneManager.LoadScene(characterSelectedSceneName);
    }
}
