using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    public QuestManager questManager  { get; set; }

    public bool IsMapOpen { get; set; }
    
    [SerializeField] private GameObject locationReachedPanel;

    public void ShowLocationReachedPanel()
    {
        locationReachedPanel.SetActive(true);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void OnMapClose()
    {
        if (locationReachedPanel.activeSelf)
        {
            locationReachedPanel.SetActive(false);
        }

        // questManager.ClearUnreachedTargetInAR();
        //questManager.SpawnUnreachedTargetInAR();
        CameraUIManager.Instance.ShowAR();
    }
}
