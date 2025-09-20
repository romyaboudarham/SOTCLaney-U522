using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject QuestUnlockedPanel;
    [SerializeField] private GameObject QuestCompletedPanel;

    // Updated to take an Action callback
    public void ShowIntro(Action onComplete)
    {
        introPanel.SetActive(true);

        // Example: close after 3 seconds and then call back
        // (replace with your actual button or flow)
        Invoke(nameof(CloseIntro), 3f);

        void CloseIntro()
        {
            introPanel.SetActive(false);
            onComplete?.Invoke();
        }
    }

    public void ShowQuestUnlocked(string targetName)
    {
        Debug.Log($"Go find {targetName}!");
        // update UI text here

        QuestUnlockedPanel.SetActive(true);

        Invoke(nameof(CloseIntro), 3f);

        void CloseIntro()
        {
            QuestUnlockedPanel.SetActive(false);
        }
    }

    public void ShowQuestComplete()
    {
        Debug.Log("Quest complete!");
        // show some completion UI here

        QuestCompletedPanel.SetActive(true);

        Invoke(nameof(CloseIntro), 3f);

        void CloseIntro()
        {
            QuestCompletedPanel.SetActive(false);
        }
    }
}
