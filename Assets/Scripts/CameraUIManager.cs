using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraUIManager : MonoBehaviour
{
    public static CameraUIManager Instance;

    [Header("Scene Cameras")]
    public Camera arCamera;
    public Camera mapCamera;

    [Header("UI Roots")]
    public GameObject arUI;
    public GameObject mapUI;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to sceneLoaded so we grab objects when scenes are ready
            SceneManager.sceneLoaded += OnSceneLoaded;
        } else {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainARScene")
        {
            arCamera = GameObject.FindGameObjectWithTag("ARCamera")?.GetComponent<Camera>();
            arUI = GameObject.FindGameObjectWithTag("ARUI");
        }
       else if (scene.name == "MapScene")
       {
            mapCamera = GameObject.FindGameObjectWithTag("MapCamera")?.GetComponent<Camera>();
            mapUI = GameObject.FindGameObjectWithTag("MapUI");
       }

        // Debug logs
        if (arCamera == null && scene.name == "MainARScene") Debug.LogWarning("ARCamera not found!");
        if (arUI == null && scene.name == "MainARScene") Debug.LogWarning("ARUI not found!");
        if (mapCamera == null && scene.name == "MapScene") Debug.LogWarning("MapCamera not found!");
        if (mapUI == null && scene.name == "MapScene") Debug.LogWarning("MapUI not found!");
    }


    public void ShowMap()
    {
        Debug.Log("SHOW MAP");
        if (arCamera) arCamera.gameObject.SetActive(false);
        if (arUI) arUI.SetActive(false);

        Debug.Log(mapCamera);
        if (mapCamera) mapCamera.gameObject.SetActive(true);
        if (mapUI) mapUI.SetActive(true);
    }

    public void ShowAR()
    {
        if (mapCamera) mapCamera.gameObject.SetActive(false);
        if (mapUI) mapUI.SetActive(false);

        if (arCamera) arCamera.gameObject.SetActive(true);
        if (arUI) arUI.SetActive(true);
    }
}
