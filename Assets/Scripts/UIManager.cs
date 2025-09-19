using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject introPanel;

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

    public void ShowTargetInstruction(string targetName)
    {
        Debug.Log($"Go find {targetName}!");
        // update UI text here
    }

    public void ShowCompletionScreen()
    {
        Debug.Log("Quest complete!");
        // show some completion UI here
    }
}
