using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CameraUIManager : MonoBehaviour
{
    public static CameraUIManager Instance;

    [Header("Scene Cameras / AR")]
    [SerializeField] private GameObject arRig; // contains ARCamera

    [Header("UI Roots")]
    [SerializeField] private GameObject arUI;
    [SerializeField] private GameObject mapUI;

    [Header("Loading Overlay")]
    [SerializeField] private CanvasGroup loadingCanvas; // assign in inspector

    // Runtime map objects (created by Mapbox at runtime)
    private GameObject baseTiles;
    private GameObject runtimeObjectsRoot;

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

            // Begin waiting for Mapbox runtime objects
            StartCoroutine(WaitForMapObjects());
        }
    }

    private IEnumerator WaitForMapObjects()
    {
        float timeout = 10f;
        float timer = 0f;

        while ((baseTiles == null || runtimeObjectsRoot == null) && timer < timeout)
        {
            baseTiles = GameObject.Find("BaseTiles");
            runtimeObjectsRoot = GameObject.Find("RuntimeObjectsRoot");

            timer += Time.deltaTime;
            yield return null;
        }

        if (baseTiles && runtimeObjectsRoot)
            Debug.Log("BaseTiles and RuntimeObjectsRoot found!");
        else
            Debug.LogWarning("Map objects not found within timeout.");
    }

    public IEnumerator WaitForMapAndThenShowAR()
    {
        // Wait until map runtime objects exist
        while (baseTiles == null || runtimeObjectsRoot == null)
        {
            baseTiles = GameObject.Find("BaseTiles");
            runtimeObjectsRoot = GameObject.Find("RuntimeObjectsRoot");
            yield return null;
        }

        Debug.Log("Map objects ready, now switching to AR");
        ShowAR();
        SceneManager.LoadSceneAsync("MainARScene", LoadSceneMode.Additive);
    }

    // Show map scene content
    public void ShowMap()
    {
        Debug.Log("SHOW MAP");

        // Hide AR rig/UI
        if (arRig) arRig.SetActive(false);
        if (arUI) arUI.SetActive(false);

        // Show Map UI immediately
        if (mapUI) mapUI.SetActive(true);

        // Enable map runtime objects when ready
        StartCoroutine(EnableMapObjects());
    }

    private IEnumerator EnableMapObjects()
    {
        // Wait until map objects exist
        while (baseTiles == null || runtimeObjectsRoot == null)
            yield return null;

        baseTiles.SetActive(true);
        runtimeObjectsRoot.SetActive(true);
    }

    // Show AR scene content
    public void ShowAR()
    {
        Debug.Log("SHOW AR");

        // Hide Map runtime objects + UI
        if (baseTiles) baseTiles.SetActive(false);
        if (runtimeObjectsRoot) runtimeObjectsRoot.SetActive(false);
        if (mapUI) mapUI.SetActive(false);

        // Show AR rig + UI
        if (arRig) arRig.SetActive(true);
        if (arUI) arUI.SetActive(true);

        // Fade out the loading overlay now that AR is ready
        if (loadingCanvas != null && loadingCanvas.gameObject.activeSelf)
            StartCoroutine(FadeAwayLoading());
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
