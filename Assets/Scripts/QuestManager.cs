using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Serializable]
    public class QuestStep
    {
        public string questName;
        public GameObject prefab;

        [HideInInspector] public bool isReached;
        [HideInInspector] public bool isCompleted;
    }

    [SerializeField] private GameObject timelinePlayerUI;
    [SerializeField] private GameObject newQuestPanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject greetingPanel;
    [SerializeField] private GameObject artifactCollectedPanel;
    [SerializeField] private List<QuestStep> questSteps;
    [SerializeField] private List<GameObject> backpackItems;

    private int currentStepIndex = -1; // -1 = greeting not done yet

    private TargetManager targetManager;
    private NavBarUIManager navBarUIManager;
    private MapManager mapManager;
    private TimelinePlayerManager timelinePlayerManager;
    private QuizManager quizManager;

    public static QuestManager Instance { get; private set; }

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
        targetManager = FindObjectOfType<TargetManager>();
        navBarUIManager = FindObjectOfType<NavBarUIManager>();
        mapManager = FindObjectOfType<MapManager>();
        timelinePlayerManager = FindObjectOfType<TimelinePlayerManager>();
        quizManager = FindObjectOfType<QuizManager>();

        if (MapManager.Instance != null)
        {
           MapManager.Instance.questManager = this; 
        }

        if (targetManager != null)
        {
            Debug.Log("TargetManager ready!");
            // Wait for location provider to be ready before showing greeting
            StartCoroutine(WaitForLocationProviderAndShowGreeting());
        }
        else
        {
            Debug.LogError("No TargetManager found in the scene!");
        }
    }

    private void Update()
    {
    }

    public void ShowArtifactCollectedPanel()
    {
        Debug.Log("Showing artifact collected panel");
        artifactCollectedPanel.SetActive(true);
    }

    public void HideArtifactCollectedPanel()
    {
        Debug.Log("Hiding artifact collected panel");
        artifactCollectedPanel.SetActive(false);
    }

    public void SpawnUnreachedTargetInAR()
    {
        //targetManager.SpawnUnreachedTargetInAR(currentStepIndex);
    }

    #region Greeting
    private IEnumerator WaitForLocationProviderAndShowGreeting()
    {
        // Wait for location provider to be ready
        while (!targetManager.IsLocationProviderReady())
        {
            Debug.Log("Waiting for location provider to initialize...");
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("Location provider ready! Showing greeting panel.");
        ShowGreeting();
    }
    
    private void ShowGreeting()
    {
        StartCoroutine(FadeInCanvas(greetingPanel.GetComponent<CanvasGroup>()));
    }

    public void OnBeginButtonClicked()
    {
        greetingPanel.SetActive(false);
        timelinePlayerManager.ActivateAndPlayTimeline(0);
    }
    #endregion

    public void StartNextQuestStep()
    {
        targetManager.EnableLocationUpdates();
        currentStepIndex++;

        if (currentStepIndex >= questSteps.Count)
        {
            Debug.Log("All quests completed!");
            return;
        }

        var step = questSteps[currentStepIndex];
        step.isReached = false;
        step.isCompleted = false;

        Debug.Log($"Starting quest step {currentStepIndex}: {step.questName}");

        if (newQuestPanel != null)
        {
            newQuestPanel.SetActive(true);
            navBarUIManager.MapNewQuest(); // flash map button
            StartCoroutine(FadeAwayCanvas(newQuestPanel.GetComponent<CanvasGroup>()));
        }
    }

    // called by NavBarUIManager when open map button is pressed
    public void SpawnQuestsOnMap()
    {
        if (currentStepIndex < 0 || currentStepIndex >= questSteps.Count) return;
        {
            targetManager.ClearAllTargets();
            targetManager.InitializeAndSpawn(currentStepIndex);
        }
    }

    // Called by TargetManager when the target is reached (GPS proximity)
    public void OnTargetReached()
    {
        if (currentStepIndex < 0 || currentStepIndex >= questSteps.Count) return;
        var step = questSteps[currentStepIndex];

        if (step.isReached) return; // already handled
        step.isReached = true;

        Debug.Log($"Quest step {currentStepIndex} reached!");
        
        // Remove the undiscovered AR prefab
        targetManager.RemoveCurrentUndiscoveredARPrefab();
        
        // Enable plane detection and show tap-to-place prompt
        targetManager.EnableTapToPlaceMode(currentStepIndex);
        
        mapManager.ShowLocationReachedPanel();
    }

    public void OnQuizCompleted() {
        quizManager.HideQuiz();
        OnTargetCompleted();
    }

    // Called when player does the required interaction to fully complete the quest
    public void OnTargetCompleted()
    {
        if (currentStepIndex < 0 || currentStepIndex >= questSteps.Count) return;
        var step = questSteps[currentStepIndex];

        if (step.isCompleted) return;
        step.isCompleted = true;

        targetManager.MarkCurrentTargetCompleted();
        Debug.Log($"Quest step {currentStepIndex} COMPLETED!");

        backpackItems[currentStepIndex].SetActive(true);
        navBarUIManager.BackpackNewItem();
        ShowArtifactCollectedPanel();
        StartCoroutine(FadeAwayCanvas(artifactCollectedPanel.GetComponent<CanvasGroup>()));
    }

    public void ShowQuiz()
    {
        if (quizManager != null)
        {
            quizPanel.SetActive(true);
            StartCoroutine(FadeAwayCanvas(quizPanel.GetComponent<CanvasGroup>()));
            OnQuizCompleted();
        }
        else
        {
            Debug.LogWarning("QuizManager not found!");
        }
    }


    private IEnumerator FadeInCanvas(CanvasGroup canvas)
    {
        yield return new WaitForSeconds(1f);
        canvas.gameObject.SetActive(true);
        while (canvas.alpha < 1f)
        {
            canvas.alpha += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeAwayCanvas(CanvasGroup canvas)
    {
        yield return new WaitForSeconds(3f);
        while (canvas.alpha > 0f)
        {
            canvas.alpha -= Time.deltaTime;
            yield return null;
        }
        canvas.gameObject.SetActive(false);
        canvas.alpha = 1f;
    }
}
