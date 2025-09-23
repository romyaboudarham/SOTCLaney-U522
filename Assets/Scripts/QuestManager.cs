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

    private TargetManager targetManager;
    private NavBarUIManager navBarUIManager;

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

    #region Greeting
    private void ShowGreeting()
    {
        StartCoroutine(FadeInCanvas(greetingPanel.GetComponent<CanvasGroup>()));
        HideAllQuestPanels();
    }

    public void OnBeginButtonClicked()
    {
        greetingPanel.SetActive(false);
        StartNextQuestStep();
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
        if (currentStepIndex >= questSteps.Count)
        {
            Debug.Log("All quests completed!");
            return;
        }

        // Show the target panel
        var step = questSteps[currentStepIndex];
        if (step.panel != null)
        {
            step.panel.SetActive(true);
            navBarUIManager.MapNewQuest();
            StartCoroutine(FadeAwayCanvas(step.panel.GetComponent<CanvasGroup>()));
        }

        // Spawn target prefab on map
            if (step.prefab != null)
            {
                //TargetManager.Instance.SpawnTarget(step.prefab, currentStepIndex);
                targetManager.InitializeAndSpawn(currentStepIndex);
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

    private IEnumerator FadeInCanvas(CanvasGroup canvas)
    {
        yield return new WaitForSeconds(3f);
        while (canvas.alpha < 1f)
        {
            canvas.alpha += Time.deltaTime; // adjust speed if needed
            yield return null;
        }

        canvas.gameObject.SetActive(true);
    }

    private IEnumerator FadeAwayCanvas(CanvasGroup canvas)
    {
        // wait 3 seconds first
        yield return new WaitForSeconds(3f);

        while (canvas.alpha > 0f)
        {
            canvas.alpha -= Time.deltaTime; // adjust speed if needed
            yield return null;
        }

        canvas.gameObject.SetActive(false);
    }
}
