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
        public GameObject panel;          // The panel for this target (before reaching)
        public GameObject reachedPanel;   // The panel to show when target is reached
        public GameObject prefab;         // Prefab to spawn on the map

        [HideInInspector] public bool isReached;
        [HideInInspector] public bool isCompleted;
    }

    [SerializeField] private GameObject timelinePlayerUI;
    [SerializeField] private GameObject completedPanel;
    [SerializeField] private GameObject greetingPanel;
    [SerializeField] private GameObject locationReachedPanel;
    [SerializeField] private List<QuestStep> questSteps;
    [SerializeField] private List<GameObject> backpackItems;

    private int currentStepIndex = -1; // -1 = greeting not done yet

    private TargetManager targetManager;
    private NavBarUIManager navBarUIManager;
    private MapManager mapManager;
    private TimelinePlayerManager timelinePlayerManager;

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

        if (MapManager.Instance != null)
        {
           MapManager.Instance.questManager = this; 
        }

        if (targetManager != null)
        {
            Debug.Log("TargetManager ready!");
        }
        else
        {
            Debug.LogError("No TargetManager found in the scene!");
        }

        ShowGreeting();
    }

    private void Update()
    {
    }

    public void ShowLocationReachedPanel()
    {
        Debug.Log("Showing location reached panel");
        locationReachedPanel.SetActive(true);
    }

    public void HideLocationReachedPanel()
    {
        Debug.Log("Hiding location reached panel");
        locationReachedPanel.SetActive(false);
    }

    public void SpawnUnreachedTargetInAR()
    {
        //targetManager.SpawnUnreachedTargetInAR(currentStepIndex);
    }

    #region Greeting
    private void ShowGreeting()
    {
        StartCoroutine(FadeInCanvas(greetingPanel.GetComponent<CanvasGroup>()));
    }

    public void OnBeginButtonClicked()
    {
        greetingPanel.SetActive(false);
        targetManager.EnableLocationUpdates();
        StartNextQuestStep();
    }
    #endregion

    #region Completed
    private void ShowCompletedButton()
    {
        StartCoroutine(FadeInCanvas(completedPanel.GetComponent<CanvasGroup>()));
    }

    public void OnCompletedButtonClicked()
    {
        OnTargetCompleted();
    }
    #endregion

    public void HideAllQuestPanels()
    {
        foreach (var step in questSteps)
        {
            if (step.panel != null) step.panel.SetActive(false);
            if (step.reachedPanel != null) step.reachedPanel.SetActive(false);
        }
    }

    private void StartNextQuestStep()
    {
        currentStepIndex++;

        timelinePlayerManager.SetTimeline(currentStepIndex); // sets player to first timeline

        if (currentStepIndex >= questSteps.Count)
        {
            Debug.Log("All quests completed!");
            return;
        }

        var step = questSteps[currentStepIndex];
        step.isReached = false;
        step.isCompleted = false;

        Debug.Log($"Starting quest step {currentStepIndex}: {step.questName}");

        if (step.panel != null)
        {
            step.panel.SetActive(true);
            navBarUIManager.MapNewQuest(); // flash map button
            StartCoroutine(FadeAwayCanvas(step.panel.GetComponent<CanvasGroup>()));
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

        backpackItems[currentStepIndex].SetActive(true);
        ShowLocationReachedPanel();
        //StartCoroutine(FadeAwayCanvas(locationReachedPanel.GetComponent<CanvasGroup>()));
        navBarUIManager.BackpackNewItem();
        mapManager.ShowLocationReachedPanel();

        //ShowCompletedButton();

        // if (step.reachedPanel != null)
        //     step.reachedPanel.SetActive(true);

        // if (step.panel != null)
        //     step.panel.SetActive(false);
    }

    // Called when player does the required interaction to fully complete the quest
    public void OnTargetCompleted()
    {
        if (currentStepIndex < 0 || currentStepIndex >= questSteps.Count) return;
        var step = questSteps[currentStepIndex];

        if (step.isCompleted) return;
        step.isCompleted = true;

        Debug.Log($"Quest step {currentStepIndex} COMPLETED!");
        targetManager.MarkCurrentTargetCompleted();
        StartNextQuestStep();

        // if (step.reachedPanel != null)
        //     step.reachedPanel.SetActive(false);

        // OnTargetReachedPanelClosed();
    }

    private void OnTargetReachedPanelClosed()
    {
        StartNextQuestStep();
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
    }
}
