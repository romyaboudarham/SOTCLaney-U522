using UnityEngine;

[System.Serializable]
public class Target
{
    public string targetName;          // For UI
    public string locationString;      // "37.7749,-122.4194" format
    public GameObject undiscoveredPrefab;
    public GameObject discoveredPrefab;

    [HideInInspector] public bool visited;
    [HideInInspector] public GameObject currentInstance;
}
