using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Mapbox.BaseModule.Map;
using Mapbox.Example.Scripts.Map;

public class CameraUIManager : MonoBehaviour
{
    public static CameraUIManager Instance;

    [Header("Scene Cameras / AR")]
    [SerializeField] private GameObject arRig; // contains ARCamera

    [Header("UI Roots")]
    [SerializeField] private GameObject arUI;
    [SerializeField] private GameObject mapUI;
    [SerializeField] private GameObject mapTag;

    [Header("Loading Overlay")]
    [SerializeField] private CanvasGroup loadingCanvas; // assign in inspector

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Listen for scene load events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainARScene")
        {
            arRig = GameObject.FindGameObjectWithTag("ARRig");
            arUI = GameObject.FindGameObjectWithTag("ARUI");

            if (arRig == null) Debug.LogWarning("ARRig not found!");
            if (arUI == null) Debug.LogWarning("ARUI not found!");
        }
        else if (scene.name == "MapScene")
        {
            mapUI = GameObject.FindGameObjectWithTag("MapUI");
            if (mapUI == null) Debug.LogWarning("MapUI not found!");

            mapTag = GameObject.FindGameObjectWithTag("Map");
            if (mapTag == null) Debug.LogWarning("Map Tag not found!");
        }
    }

    // Show map scene content
    public void ShowMap()
    {
        //Debug.Log("SHOW MAP");
        MapManager.Instance.IsMapOpen = true;
        // Hide AR rig/UI
        if (arRig) arRig.SetActive(false);
        if (arUI) arUI.SetActive(false);

        // Show Map UI immediately
        if (mapUI) mapUI.SetActive(true);
        EnableAllMapChildren(mapTag);
    }

    private void EnableAllMapChildren(GameObject mapRoot)
    {
        if (mapRoot == null) return;

        // Enable all top-level children of Map
        foreach (Transform child in mapRoot.transform)
        {
            child.gameObject.SetActive(true);
        }

        // Enable all children of MapUtility
        Transform mapUtility = mapRoot.transform.Find("MapUtility");
        if (mapUtility != null)
        {
            foreach (Transform child in mapUtility)
            {
                child.gameObject.SetActive(true);
            }
        }

        Debug.Log("All Map children re-enabled.");
    }

    // Show AR scene content
    public void ShowAR()
    {
        //Debug.Log("SHOW AR");
        MapManager.Instance.IsMapOpen = false;
        // Hide Map runtime objects 
        if (mapUI) mapUI.SetActive(false);
        DisableMapExceptLocationModule(mapTag);
        // if (mapTag) mapTag.SetActive(false);

        // Show AR rig + UI
        if (arRig) arRig.SetActive(true);
        if (arUI) arUI.SetActive(true);

        // Fade out the loading overlay now that AR is ready
        if (loadingCanvas != null && loadingCanvas.gameObject.activeSelf)
            StartCoroutine(FadeAwayLoading());
    }

    private void DisableMapExceptLocationModule(GameObject mapRoot)
    {
        if (mapRoot == null) return;

        // Find MapUtility and LocationModule
        Transform mapUtility = mapRoot.transform.Find("MapUtility");
        if (mapUtility == null)
        {
            Debug.LogError("MapUtility not found under Map root!");
            return;
        }

        Transform locationProvider = mapUtility.Find("LocationModule");
        if (locationProvider == null)
        {
            Debug.LogError("LocationModule not found under MapUtility!");
            return;
        }

        // 1. Disable all top-level children of Map except MapUtility
        foreach (Transform child in mapRoot.transform)
        {
            if (child != mapUtility)
                child.gameObject.SetActive(false);
        }

        // 2️. Disable all children of MapUtility except LocationProviderFactory
        foreach (Transform child in mapUtility)
        {
            if (child != locationProvider)
                child.gameObject.SetActive(false);
        }

        //Debug.Log("Disabled all Map visuals except LocationProviderFactory.");
    }

    private IEnumerator FadeAwayLoading()
    {
        while (loadingCanvas.alpha > 0f)
        {
            loadingCanvas.alpha -= Time.deltaTime; // adjust speed if needed
            yield return null;
        }

        loadingCanvas.gameObject.SetActive(false);
    }
}
