using UnityEngine;

public class LevelSelectionSceneEntry : MonoBehaviour
{
    [SerializeField] private LevelSlotUI[] levelSlots;

    private void Start()
    {
        if (LevelSessionManager.Instance == null)
        {
            Debug.LogError("[LevelSelectionSceneEntry] Không tìm thấy LevelSessionManager trong scene.");
            return;
        }

        foreach (var slot in levelSlots)
        {
            slot.RefreshState();
        }

        LevelSessionManager.OnLevelStateChanged += HandleLevelStateChanged;
    }

    private void HandleLevelStateChanged(LevelData data)
    {
        foreach (var slot in levelSlots)
        {
            if (slot.levelData != null && slot.levelData.levelId == data.levelId)
            {
                slot.RefreshState();
            }
        }
    }

    private void OnDestroy()
    {
        LevelSessionManager.OnLevelStateChanged -= HandleLevelStateChanged;
    }
}