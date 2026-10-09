using UnityEngine;

public class CharacterSelectionSceneEntry : MonoBehaviour
{
    [SerializeField] private Transform characterListContent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameSessionManager.Instance == null)
        {
            Debug.LogError("[CharacterSelectionSceneEntry] Không tìm thấy GameSessionManager trong scene này. Hãy chắc chắn rằng có 1 GameSessionManager trong scene.");
            return;
        }
        GameSessionManager.Instance.RegisterCharacterList(characterListContent);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
