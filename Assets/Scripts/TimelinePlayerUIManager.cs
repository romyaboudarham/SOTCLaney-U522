using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Collections.Generic;

public class TimelinePlayerManager : MonoBehaviour
{
    [Header("UI")]
    public Button playPauseButton;
    [SerializeField] RawImage playIcon;
    [SerializeField] RawImage pauseIcon;
    private bool isPlaying = false;
    public Slider progressSlider;

    [Header("Timelines")]
    public List<PlayableDirector> timelines = new List<PlayableDirector>();

    private int activeIndex = -1;

    private MapManager mapManager;

    void Awake()
    {

    }

    void Start()
    {
        mapManager = FindObjectOfType<MapManager>();

        if (MapManager.Instance != null)
        {
            MapManager.Instance.timelinePlayerManager = this;
        }

        playPauseButton.onClick.AddListener(TogglePlayPause);
        progressSlider.onValueChanged.AddListener(OnSliderChanged);

        progressSlider.minValue = 0;
        progressSlider.maxValue = 1;

        UpdatePlayPauseIcon();
    }

    void Update()
    {
        var active = GetActiveTimeline();
        if (active != null && active.playableAsset != null && active.duration > 0 && isPlaying)
        {
            progressSlider.value = (float)(active.time / active.duration);
        }
    }

    public void SetTimeline(int index)
    {
        if (index < 0 || index >= timelines.Count) return;
        Debug.Log("Switching to timeline index: " + index);

        // stop old timeline
        var oldTimeline = GetActiveTimeline();
        if (oldTimeline != null) oldTimeline.Stop();

        activeIndex = index;
        isPlaying = false;
        progressSlider.value = 0;
    }

    public void TogglePlayPause()
    {
        var activeTimeline = GetActiveTimeline();
        if (activeTimeline == null) return;

        if (!isPlaying)
        {
            Debug.Log("Playing timeline: " + activeTimeline.name);
            activeTimeline.Play();
            isPlaying = true;
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
}
