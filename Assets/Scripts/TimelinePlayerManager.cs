using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Collections.Generic;
using Mapbox.Examples;

public class TimelinePlayerManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject timelinePlayerUI;
    public Button playPauseButton;
    [SerializeField] RawImage playIcon;
    [SerializeField] RawImage pauseIcon;
    private bool isPlaying = false;
    public Slider progressSlider;

    [Header("Timelines")]
    public List<PlayableDirector> timelines = new List<PlayableDirector>();

    private int activeIndex = -1;
    private bool isUpdatingSlider = false; // Prevent recursive Play() calls

    private MapManager mapManager;
    private QuestManager questManager;
    
    // Performance optimization references
    private TargetManager targetManager;
    private CharacterMovement characterMovement;

    void Awake()
    {

    }

    void Start()
    {
        mapManager = FindObjectOfType<MapManager>();
        questManager = FindObjectOfType<QuestManager>();
        
        // Get performance optimization references
        targetManager = FindObjectOfType<TargetManager>();
        characterMovement = FindObjectOfType<CharacterMovement>();

        if (MapManager.Instance != null)
        {
            MapManager.Instance.timelinePlayerManager = this;
        }

        if (playPauseButton != null)
        {
            playPauseButton.onClick.AddListener(TogglePlayPause);
            Debug.Log("PlayPause button listener added");
        }
        else
        {
            Debug.LogError("PlayPause button is null!");
        }
        
        if (progressSlider != null)
        {
            progressSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        progressSlider.minValue = 0;
        progressSlider.maxValue = 1;

        UpdatePlayPauseIcon();
    }

    void Update()
    {
        var active = GetActiveTimeline();
        if (active != null && active.playableAsset != null && active.duration > 0 && isPlaying)
        {
            // Prevent OnSliderChanged from calling Play() when we update the slider
            isUpdatingSlider = true;
            progressSlider.value = (float)(active.time / active.duration);
            isUpdatingSlider = false;
            
            // Debug timeline progress
            if (activeIndex == 1) // Laney timeline
            {
                Debug.Log($"Laney Timeline Progress: {active.time:F2}/{active.duration:F2} ({(active.time/active.duration)*100:F1}%)");
            }
            
            // Check if timeline has finished
            if (active.time >= active.duration)
            {
                Debug.Log($"Timeline {activeIndex} finished - time: {active.time:F2}, duration: {active.duration:F2}");
                OnTimelineFinished();
            }
        }
    }

    public void ActivateAndPlayTimeline(int index)
    {
        timelinePlayerUI.SetActive(true);
        Debug.Log("Playing timeline index: " + index);
        if (index < 0 || index >= timelines.Count) return;

        // PERFORMANCE OPTIMIZATION: Disable heavy systems during timeline
        DisablePerformanceHeavySystems();

        // stop old timeline
        var oldTimeline = GetActiveTimeline();
        if (oldTimeline != null) oldTimeline.Stop();

        activeIndex = index;
        isPlaying = false;
        progressSlider.value = 0;

        // Debug timeline info
        var newTimeline = GetActiveTimeline();
        if (newTimeline != null)
        {
            Debug.Log($"Timeline {index} info - Duration: {newTimeline.duration:F2}, State: {newTimeline.state}");
        }

        TogglePlayPause();
    }

    public void TogglePlayPause()
    {
        Debug.Log("TogglePlayPause called!");
        var activeTimeline = GetActiveTimeline();
        if (activeTimeline == null) 
        {
            Debug.Log("No active timeline found");
            return;
        }

        if (!isPlaying)
        {
            Debug.Log("Playing timeline: " + activeTimeline.name);
            Debug.Log($"Timeline duration: {activeTimeline.duration}, current time: {activeTimeline.time}");
            activeTimeline.Play();
            isPlaying = true;
            Debug.Log($"Timeline state after Play(): {activeTimeline.state}");
        }
        else
        {
            Debug.Log("Pausing timeline: " + activeTimeline.name);
            activeTimeline.Pause();
            isPlaying = false;
        }

        UpdatePlayPauseIcon();
    }

    private void UpdatePlayPauseIcon()
    {
        if (isPlaying == true)
        {
            playIcon.enabled = false;
            pauseIcon.enabled = true;
        }
        else
        {
            playIcon.enabled = true;
            pauseIcon.enabled = false;
        }
    }

    void OnSliderChanged(float value)
    {
        // Don't call Play() if we're updating the slider programmatically
        if (isUpdatingSlider) return;
        
        var active = GetActiveTimeline();
        if (active == null || active.playableAsset == null) return;

        double newTime = value * active.duration;
        active.time = newTime;

        if (isPlaying)
            active.Play();
        else
            active.Evaluate();
    }

    PlayableDirector GetActiveTimeline()
    {
        if (activeIndex >= 0 && activeIndex < timelines.Count)
            return timelines[activeIndex];
        return null;
    }

    public void SaveAndPause()
    {
        var active = GetActiveTimeline();
        if (active != null)
        {
            // Pause
            active.Pause();
            isPlaying = false;
            UpdatePlayPauseIcon();
        }
    }

    public void Restore()
    {
        var active = GetActiveTimeline();
        if (active != null)
        {
            // Keep timeline at paused time
            active.Evaluate();
            progressSlider.value = (float)(active.time / active.duration);
            UpdatePlayPauseIcon();
        }
    }

    private void OnTimelineFinished()
    {
        Debug.Log($"OnTimelineFinished called for timeline index: {activeIndex}");
        
        // PERFORMANCE OPTIMIZATION: Re-enable heavy systems after timeline
        EnablePerformanceHeavySystems();
        
        // Stop the timeline
        var active = GetActiveTimeline();
        if (active != null)
        {
            Debug.Log($"Stopping timeline {activeIndex}, final time: {active.time:F2}, duration: {active.duration:F2}");
            active.Stop();
            isPlaying = false;
            UpdatePlayPauseIcon();
        }
        
        // Deactivate timeline player UI
        if (timelinePlayerUI != null)
        {
            timelinePlayerUI.SetActive(false);
        }

        // Handle different timeline completions
        if (activeIndex == 0) {
            // Intro timeline - start next quest step
            Debug.Log("Intro timeline finished - starting next quest step");
            questManager.StartNextQuestStep();
        }
        else if (activeIndex == 1) {
            // Laney timeline - cleanup tapped artifact and show quiz
            Debug.Log("Laney timeline finished - cleaning up artifact and showing quiz");
            
            // Clean up the tapped artifact and disable tap-to-place
            if (targetManager != null)
            {
                targetManager.CleanupTappedArtifact();
            }
            
            if (questManager != null)
            {
                questManager.ShowQuiz();
            }
            else
            {
                Debug.LogError("QuestManager is null when trying to show quiz!");
            }
        }
        else
        {
            Debug.LogWarning($"Unknown timeline index {activeIndex} finished");
        }
    }
    
    // PERFORMANCE OPTIMIZATION METHODS
    private void DisablePerformanceHeavySystems()
    {
        Debug.Log("Disabling performance-heavy systems for timeline playback");
        
        // Disable GPS/location updates
        if (targetManager != null)
        {
            targetManager.DisableLocationUpdates();
        }
        
        // Disable character movement (includes compass, GPS, terrain snapping)
        if (characterMovement != null)
        {
            characterMovement.enabled = false;
        }
        
        // Disable AR target animations
        var arTargetAnimations = FindObjectsOfType<ARTarget_Animation>();
        foreach (var anim in arTargetAnimations)
        {
            anim.enabled = false;
        }
    }
    
    private void EnablePerformanceHeavySystems()
    {
        Debug.Log("Re-enabling performance-heavy systems after timeline");
        
        // Re-enable GPS/location updates
        if (targetManager != null)
        {
            targetManager.EnableLocationUpdates();
        }
        
        // Re-enable character movement
        if (characterMovement != null)
        {
            characterMovement.enabled = true;
        }
        
        // Re-enable AR target animations
        var arTargetAnimations = FindObjectsOfType<ARTarget_Animation>();
        foreach (var anim in arTargetAnimations)
        {
            anim.enabled = true;
        }
    }
}
