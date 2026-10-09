using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UISelectCharacter : MonoBehaviour
{
    [SerializeField] private Button ReturnStartSceneButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(ReturnStartSceneButton != null)
        {
            ReturnStartSceneButton.onClick.RemoveAllListeners();
            ReturnStartSceneButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("StartScene");
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
