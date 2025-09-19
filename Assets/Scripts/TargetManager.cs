using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private List<Target> targets;
    [SerializeField] private UIManager uiManager;

    private int currentTargetIndex = 0;

    public static TargetManager Instance { get; private set; }

    void Awake()
    {
        // Singleton-style persistence
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ActivateTarget(currentTargetIndex);
        // uiManager.ShowIntro(() =>
        // {
        //     ActivateTarget(currentTargetIndex);
        // });
    }

    public void ActivateTarget(int index)
    {
        if (index >= targets.Count)
        {
            uiManager.ShowCompletionScreen();
            return;
        }

        uiManager.ShowTargetInstruction(targets[index].targetName);
    }

    public void TargetReached(GameObject marker)
    {
        Target target = targets.Find(t => t.currentInstance == marker);

        if (target != null && !target.visited)
        {
            target.visited = true;

            // Advance quest step
            currentTargetIndex++;
            ActivateTarget(currentTargetIndex);
        }
    }

    // Called when MapScene loads
    public void InitializeMap(SpawnOnMapV3 spawner)
    {
        if (spawner == null) return;

        spawner.InitializeAndSpawn(targets, currentTargetIndex);
    }
}
