using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DifficultyPanel : MonoBehaviour
{
    [System.Serializable]
    public class DifficultyEntry
    {
        public Difficulty difficulty;
        public Button selectButton;
        public GameObject detailPanel;
        public Button playButton;

    }

    [Header("3 độ khó: Easy, Medium, Hard")]
    [SerializeField] private DifficultyEntry[] entries = new DifficultyEntry[3];

    [Header("Khác")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI levelNameText; // đổi sang TMP_Text nếu dùng TextMeshPro

    private LevelData currentLevel;
    private Difficulty selectedDifficulty = Difficulty.Easy;

    private void Awake()
    {
        foreach (var entry in entries)
        {
            Difficulty d = entry.difficulty;

            if (entry.selectButton != null)
            {
                entry.selectButton.onClick.RemoveAllListeners();
                entry.selectButton.onClick.AddListener(() => SelectDifficulty(d));
            }

            if (entry.playButton != null)
            {
                entry.playButton.onClick.RemoveAllListeners();
                entry.playButton.onClick.AddListener(() => OnPlayPressed(d));
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        gameObject.SetActive(false);
    }

    /// <summary>Mở panel cho 1 level, mặc định chọn Easy.</summary>
    public void Open(LevelData level)
    {
        currentLevel = level;
        gameObject.SetActive(true);

        if (levelNameText != null)
            levelNameText.text = level.levelName;

        // Khóa / mở từng nút theo tiến độ
        foreach (var entry in entries)
        {
            bool unlocked = level.IsDifficultyUnLocked(entry.difficulty);

            if (entry.selectButton != null) entry.selectButton.interactable = unlocked;
        }

        SelectDifficulty(Difficulty.Easy); // mặc định Easy
    }

    public void Close()
    {
        gameObject.SetActive(false);
        currentLevel = null;
    }

    private void SelectDifficulty(Difficulty difficulty)
    {
        if (currentLevel == null || !currentLevel.IsDifficultyUnLocked(difficulty)) return;

        selectedDifficulty = difficulty;

        // Chỉ bật sub-panel của độ khó được chọn
        foreach (var entry in entries)
        {
            if (entry.detailPanel != null)
                entry.detailPanel.SetActive(entry.difficulty == difficulty);
        }
    }

    private void OnPlayPressed(Difficulty difficulty)
    {
        if (currentLevel == null) return;

        // Chặn thêm 1 lớp phòng khi UI bị lỗi
        if (!currentLevel.IsDifficultyUnLocked(difficulty)) return;

        if (string.IsNullOrEmpty(currentLevel.sceneNameToLoad))
        {
            Debug.LogWarning($"[DifficultyPanel] Level '{currentLevel.levelName}' chưa gán sceneNameToLoad.");
            return;
        }

        LevelSessionManager.Instance.SetCurrentSession(currentLevel, difficulty);
        SceneManager.LoadScene(currentLevel.sceneNameToLoad);
    }
}