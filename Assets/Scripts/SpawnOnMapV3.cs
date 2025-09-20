using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Mapbox.BaseModule.Data.Vector2d;   // For LatitudeLongitude
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using Mapbox.Example.Scripts.Map;
using UnityEngine.SceneManagement;

public class SpawnOnMapV3 : MonoBehaviour
{
    [SerializeField] private MapboxMapBehaviour _mapCore;
    private MapboxMap _map;

    private void Start()
    {
        if (_mapCore == null)
        {
            Debug.LogError("MapboxMapBehaviour is not assigned!");
            return;
        }

        _mapCore.Initialized += (map) =>
        {
            _map = map;

            // Ask TargetManager to initialize the map state
            if (TargetManager.Instance != null)
            {
                TargetManager.Instance.SetMap(_map);
                TargetManager.Instance.InitializeMap(this);
            }
        };
    }

    public void InitializeAndSpawn(List<Target> targets, int currentIndex)
    {
        StartCoroutine(WaitForMapReady(targets, currentIndex));
    }

    private IEnumerator WaitForMapReady(List<Target> targets, int currentIndex)
    {
        while (_map.Status < InitializationStatus.ReadyForUpdates)
        {
            yield return null; // wait for next frame
        }

        Debug.Log("Map is ready! Spawning targets now.");
        SpawnTargets(targets, currentIndex);
    }


    public void SpawnTargets(List<Target> targets, int currentIndex)
    {
        for (int i = 0; i <= currentIndex; i++)
        {
            SpawnTargetOnMap(targets[i], targets[i].visited);
        }
    }

    private void SpawnTargetOnMap(Target target, bool asDiscovered)
    {
        var latLng = Conversions.StringToLatLon(target.locationString);
        Vector3 localPos = _map.MapInformation.ConvertLatLngToPosition(latLng);

        var prefab = asDiscovered ? target.discoveredPrefab : target.undiscoveredPrefab;
        var spawnScale = asDiscovered ? target.D_SpawnScale : target.UD_SpawnScale;

        var instance = Instantiate(
            prefab,
            localPos,
            Quaternion.identity,
            _mapCore.UnityContext.MapRoot
        );
        instance.transform.localScale = Vector3.one * spawnScale;

        target.currentInstance = instance;
    }

    public void CloseMapClick()
    {
        SceneManager.LoadScene("MainScene");
    }
}
