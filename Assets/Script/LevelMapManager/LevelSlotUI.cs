using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSlotUI : MonoBehaviour
{
    [Header("Data")]
    public LevelData levelData;

    [Header("4 State GameObjects (mỗi cái tự có Button riêng, lồng nhau, chỉ 1 cái active)")]
    [SerializeField] private GameObject stateZeroStars;
    [SerializeField] private GameObject stateOneStar;
    [SerializeField] private GameObject stateTwoStars;
    [SerializeField] private GameObject stateThreeStars;

    [SerializeField] private DifficultyPanel difficultyPanel;

    private void Start()
    {
        BindButton(stateZeroStars);
        BindButton(stateOneStar);
        BindButton(stateTwoStars);
        BindButton(stateThreeStars);
    }

    private void BindButton(GameObject stateObj)
    {
        if (stateObj == null) return;

        Button btn = stateObj.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogWarning($"[LevelSlotUI] '{stateObj.name}' chưa có component Button.");
            return;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClicked);
    }

    public void RefreshState()
    {
        if (levelData == null) return;

        bool isUnlocked = levelData.isUnlocked;

        if (stateZeroStars != null) stateZeroStars.SetActive(false);
        if (stateOneStar != null) stateOneStar.SetActive(false);
        if (stateTwoStars != null) stateTwoStars.SetActive(false);
        if (stateThreeStars != null) stateThreeStars.SetActive(false);

        GameObject activeState = null;

        if (!isUnlocked)
        {
            activeState = stateZeroStars;
        }
        else
        {
            switch (levelData.starsEarned)
            {
                case 0: activeState = stateZeroStars; break;
                case 1: activeState = stateOneStar; break;
                case 2: activeState = stateTwoStars; break;
                case 3: activeState = stateThreeStars; break;
            }
        }

        if (activeState != null)
        {
            activeState.SetActive(true);

            Button btn = activeState.GetComponent<Button>();
            if (btn != null) btn.interactable = isUnlocked;
        }
    }

    public void OnClicked()
    {
        if (levelData == null || !levelData.isUnlocked) return;

        if (difficultyPanel == null)
        {
            Debug.LogError("[LevelSlotUI] Chưa gán DifficultyPanel.");
            return;
        }

        difficultyPanel.Open(levelData);
    }
}