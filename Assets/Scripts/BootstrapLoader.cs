using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

using Mapbox.BaseModule.Map;
using Mapbox.Example.Scripts.Map;

public class BootstrapLoader : MonoBehaviour
{
    public static BootstrapLoader Instance;

    private MapboxMapBehaviour _mapCore;
    private MapboxMap _map;

    public MapboxMap GetMap() => _map;
    public MapboxMapBehaviour GetMapCore() => _mapCore;

    public event System.Action<MapboxMap> OnMapReady;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.LoadSceneAsync("MapScene", LoadSceneMode.Additive).completed += (op) =>
        {
            Debug.Log("MapScene loaded.");
            StartCoroutine(WaitForMapcoreAndMap());
        };
    }

    private IEnumerator WaitForMapcoreAndMap()
    {
        // Wait for the mapCore object
        while (_mapCore == null)
        {
            _mapCore = FindObjectOfType<MapboxMapBehaviour>();
            yield return null; // wait a frame
        }
        Debug.Log("mapCore found.");

        // Wait for MapboxMap component inside mapcore
        _mapCore.Initialized += (map) =>
        {
            StartCoroutine(WaitUntilReady(map));
        };
    }

    private IEnumerator WaitUntilReady(MapboxMap map)
    {
        while (map.Status < InitializationStatus.ReadyForUpdates)
            yield return null;

        _map = map;
        Debug.Log("Map is fully ready for updates!");

        SceneManager.LoadSceneAsync("MainARScene", LoadSceneMode.Additive).completed += (op) =>
        {
            Debug.Log("MainARScene loaded.");
            CameraUIManager.Instance.ShowAR();
            OnMapReady?.Invoke(_map); // 🔑 notify listeners
        };
    }
}

