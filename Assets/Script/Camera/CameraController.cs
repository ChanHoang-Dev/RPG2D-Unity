using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Camera Limit")]
    [SerializeField] private float left;
    [SerializeField] private float right;
    [SerializeField] private float bottom;
    [SerializeField] private float top;

    [Header("Camera")]
    [SerializeField] private float orthographicSize = 8f;
    [SerializeField] private float smoothTime = 0.15f; // thời gian làm mượt (giây)

    private Camera cam;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        SetupCamera();
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindPlayer();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnValidate()
    {
        if (left > right)
        {
            Debug.LogWarning($"[CameraController] 'left' ({left}) đang lớn hơn 'right' ({right}). Hãy kiểm tra lại giá trị trong Inspector.");
        }

        if (bottom > top)
        {
            Debug.LogWarning($"[CameraController] 'bottom' ({bottom}) đang lớn hơn 'top' ({top}). Hãy kiểm tra lại giá trị trong Inspector.");
        }
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        Vector3 playerPosition = player.transform.position;

        float targetX = Mathf.Clamp(playerPosition.x, left, right);
        float targetY = Mathf.Clamp(playerPosition.y, bottom, top);

        Vector3 targetPosition = new Vector3(
            targetX,
            targetY,
            transform.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name);

        SetupCamera();
        player = null;
        velocity = Vector3.zero; // reset để tránh giật khi đổi scene
        FindPlayer();
    }

    private void SetupCamera()
    {
        cam = Camera.main;

        if (cam != null)
        {
            cam.rect = new Rect(0, 0, 1, 1);
            cam.orthographicSize = orthographicSize;
        }
        else
        {
            Debug.LogWarning("[CameraController] Không tìm thấy Camera.main trong scene này.");
        }
    }

    private void FindPlayer()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

        if (foundPlayer != null)
        {
            player = foundPlayer;
            Debug.Log("Camera found Player: " + player.name);
        }
    }

    public void SetPlayer(GameObject newPlayer)
    {
        player = newPlayer;

        if (player != null)
        {
            velocity = Vector3.zero; // reset khi gán player mới thủ công
            Debug.Log("Camera now following: " + player.name);
        }
    }
}