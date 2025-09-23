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
        public GameObject panel;          // The panel for this target
        public GameObject reachedPanel;   // The panel to show when target is reached
        public GameObject prefab;         // Prefab to spawn on the map
    }

    [SerializeField] private GameObject greetingPanel;
    [SerializeField] private List<QuestStep> questSteps;

    private int currentStepIndex = -1; // -1 means greeting not done yet

    private SpawnOnMapV3 mapSpawner;

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
         mapSpawner = FindObjectOfType<SpawnOnMapV3>();
        if (mapSpawner != null)
        {
            Debug.Log("SpawnOnMapV3 ready!");
        }
        else
        {
            Debug.LogError("No SpawnOnMapV3 found in the scene!");
        }
        ShowGreeting();
    }

    #region Greeting
    private void ShowGreeting()
    {
        greetingPanel.SetActive(true);
        HideAllQuestPanels();
    }

    public void OnBeginButtonClicked()
    {
        greetingPanel.SetActive(false);
        StartNextQuestStep();
    }
    #endregion

    private void HideAllQuestPanels()
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
        if (currentStepIndex >= questSteps.Count)
        {
            Debug.Log("All quests completed!");
            return;
        }

        // Show the target panel
        var step = questSteps[currentStepIndex];
        if (step.panel != null)
            step.panel.SetActive(true);

        // Spawn target prefab on map
        if (step.prefab != null)
        {
            //TargetManager.Instance.SpawnTarget(step.prefab, currentStepIndex);
            mapSpawner.InitializeAndSpawn(currentStepIndex);
        }
    }

    // Called by TargetManager when the target is reached
    public void OnTargetReached()
    {
        if (currentStepIndex < 0 || currentStepIndex >= questSteps.Count) return;

        var step = questSteps[currentStepIndex];

        // Show reached panel
        if (step.reachedPanel != null)
            step.reachedPanel.SetActive(true);

        // Optionally hide the target panel
        if (step.panel != null)
            step.panel.SetActive(false);
    }

    // Called when closing the reached panel to go to the next step
    public void OnTargetReachedPanelClosed()
    {
        StartNextQuestStep();
    }
}
