using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Panel")]
    public GameObject quizPanel;

    public static QuizManager Instance { get; private set; }

    [SerializeField] private Button answerBtn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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
