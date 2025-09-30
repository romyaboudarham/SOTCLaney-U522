using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Collections.Generic;

public class MultiTimelinePlayerUI : MonoBehaviour
{
    [Header("UI")]
    public Button playPauseButton;
    public Slider progressSlider;

    [Header("Timelines")]
    public List<PlayableDirector> timelines = new List<PlayableDirector>();

    private int activeIndex = -1;
    private bool isPlaying = false;

    void Start()
    {
        playPauseButton.onClick.AddListener(TogglePlayPause);
        progressSlider.onValueChanged.AddListener(OnSliderChanged);

        progressSlider.minValue = 0;
        progressSlider.maxValue = 1;
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
        var old = GetActiveTimeline();
        if (old != null) old.Stop();

        activeIndex = index;
        isPlaying = false;
        progressSlider.value = 0;
    }

    void TogglePlayPause()
    {
        var active = GetActiveTimeline();
        if (active == null) return;

        if (!isPlaying)
        {
            Debug.Log("Playing timeline: " + active.name);
            active.Play();
            isPlaying = true;
        }
        else
        {
            Debug.Log("Pausing timeline: " + active.name);
            active.Pause();
            isPlaying = false;
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
}
