using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Mapbox.BaseModule.Data.Vector2d;   // For LatitudeLongitude
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using Mapbox.Example.Scripts.Map;
using UnityEngine.SceneManagement;
using TMPro;

public class SpawnOnMapV3 : MonoBehaviour
{
    [SerializeField] private MapboxMapBehaviour _mapCore;
    private MapboxMap _map;

    [SerializeField] private List<Target> targets;
    public TMP_Text debugTxt;

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
        };
    }

    public void InitializeAndSpawn(int currentIndex)
    {
        StartCoroutine(WaitForMapReady(currentIndex));
    }

    private IEnumerator WaitForMapReady(int currentIndex)
    {
        while (_map.Status < InitializationStatus.ReadyForUpdates)
        {
            yield return null; // wait for next frame
        }

        Debug.Log("Map is ready! Spawning targets now.");
        SpawnTargets(currentIndex);
    }


    public void SpawnTargets(int currentIndex)
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
}