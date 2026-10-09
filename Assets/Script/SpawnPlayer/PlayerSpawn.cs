using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        SpawnSelectedCharacter();
    }

    private void SpawnSelectedCharacter()
    {
        if (GameSessionManager.Instance == null)
        {
            Debug.LogError("[PlayerSpawner] Không tìm thấy GameSessionManager trong scene.");
            return;
        }

        CharacterData data = GameSessionManager.Instance.SelectedCharacter;
        if (data == null)
        {
            Debug.LogError("[PlayerSpawner] SelectedCharacter đang null -> không có gì để spawn.");
            return;
        }

        if (data.characterPrefab == null)
        {
            Debug.LogError($"[PlayerSpawner] Nhân vật '{data.characterName}' chưa gán characterPrefab trong CharacterData.");
            return;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        GameObject playerInstance = Instantiate(data.characterPrefab, spawnPos, spawnRot);

        if (!playerInstance.CompareTag("Player"))
        {
            Debug.LogWarning($"[PlayerSpawner] Prefab '{data.characterPrefab.name}' không có tag 'Player' -> CameraController có thể không tìm thấy.");
        }

        Debug.Log($"[PlayerSpawner] Đã spawn nhân vật: {data.characterName}");
    }
}