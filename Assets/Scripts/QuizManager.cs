using UnityEngine;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Panel")]
    public GameObject quizPanel;

    public static QuizManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Ensure quiz panel starts inactive
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }
    }

    public void ShowQuiz()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
            Debug.Log("Quiz panel activated");
        }
        else
        {
            Debug.LogWarning("Quiz panel not assigned in QuizManager!");
        }
    }

    public void HideQuiz()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
            Debug.Log("Quiz panel deactivated");
        }
    }
}
