using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class QuizData
{
    public string question;
    public string[] answers = new string[4];
    public int correctAnswerIndex;
}

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Panel")]
    public GameObject quizPanel;

    [Header("Quiz Data")]
    [SerializeField] private QuizData[] quizzes = new QuizData[3];

    [Header("UI References")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button[] answerButtons = new Button[4];
    [SerializeField] private TMP_Text[] answerTexts = new TMP_Text[4];

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

    private void OnAnswerSelected(int selectedAnswerIndex, int correctAnswerIndex)
    {
        bool isCorrect = selectedAnswerIndex == correctAnswerIndex;
        
        if (isCorrect)
        {
            Debug.Log("Correct answer selected!");
            // You can add visual feedback here (e.g., green highlight)
        }
        else
        {
            Debug.Log($"Incorrect answer selected. Correct answer was index {correctAnswerIndex}");
            // You can add visual feedback here (e.g., red highlight)
        }
        
        // Hide quiz and complete the quest step
        HideQuiz();
        QuestManager.Instance.OnQuizCompleted();
    }

    // Legacy method for backward compatibility
    public void OnAnswerBtnClick() {
        HideQuiz();
        QuestManager.Instance.OnQuizCompleted();
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
            // Get the current quiz index from QuestManager
            int quizIndex = QuestManager.Instance.GetCurrentStepIndex();
            
            // Validate quiz index
            if (quizIndex < 0 || quizIndex >= quizzes.Length)
            {
                Debug.LogError($"Invalid quiz index: {quizIndex}. Must be between 0 and {quizzes.Length - 1}");
                return;
            }
            
            // Get the current quiz data
            QuizData currentQuiz = quizzes[quizIndex];
            
            // Populate the UI with quiz data
            if (questionText != null)
            {
                questionText.text = currentQuiz.question;
            }
            
            // Populate answer buttons and texts
            for (int i = 0; i < answerButtons.Length && i < currentQuiz.answers.Length; i++)
            {
                if (answerButtons[i] != null)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    // Store the answer index for click handling
                    answerButtons[i].onClick.RemoveAllListeners();
                    int answerIndex = i; // Capture for closure
                    answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex, currentQuiz.correctAnswerIndex));
                }
                
                if (answerTexts[i] != null)
                {
                    answerTexts[i].text = currentQuiz.answers[i];
                }
            }
            
            // Hide any unused answer buttons
            for (int i = currentQuiz.answers.Length; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] != null)
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
            
            quizPanel.SetActive(true);
            Debug.Log($"Quiz panel activated with quiz {quizIndex}: {currentQuiz.question}");
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
