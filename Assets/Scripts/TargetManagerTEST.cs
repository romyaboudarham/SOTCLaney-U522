using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.BaseModule.Data.Vector2d;
using Mapbox.BaseModule.Map;
using Mapbox.Example.Scripts.Map;
using Mapbox.BaseModule.Utilities;

using TMPro;

public class TargetManagerTEST : MonoBehaviour
{
    [SerializeField] private List<Target> targets;
    [SerializeField] private UIManager uiManager;

    private int currentTargetIndex = 0;

    public static TargetManagerTEST Instance { get; private set; }

    [SerializeField] private MapboxMapBehaviour _mapCore;
    private MapboxMap _map;

    public TMP_Text debugTxt;

    public void SetMap(Mapbox.BaseModule.Map.MapboxMap map)
    {
        _map = map;
    }

    void Awake()
    {
        // Singleton-style persistence
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    void Start()
    {
        if (_mapCore == null)
        {
            Debug.LogError("MapboxMapBehaviour is not assigned!");
            return;
        }

        _mapCore.Initialized += (map) =>
        {
            _map = map;
            SpawnTargets(targets, currentTargetIndex);
        };
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

    public void ActivateTarget(int index)
    {
        if (index >= targets.Count)
        {
            //uiManager.DONE();
            return;
        }

        uiManager.ShowQuestUnlocked(targets[index].targetName);
    }

    public void TargetReached(GameObject marker)
    {
        Target target = targets.Find(t => t.currentInstance == marker);

        if (target != null && !target.visited)
        {
            target.visited = true;

            // Advance quest step
            currentTargetIndex++;
            uiManager.ShowQuestComplete();
        }
    }

    public void SpawnTarget(GameObject prefab, int index)
    {
        if (index < 0 || index >= targets.Count) return;

        Target t = targets[index];
        if (prefab != null)
        {
            Vector3 worldPos = _map.MapInformation.ConvertLatLngToPosition(Conversions.StringToLatLon(t.locationString));
            GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity);
            t.currentInstance = instance;
        }
    }

    public void HandlePlayerNearCurrentTarget(Vector2d playerPos, double thresholdKm)
    {
        if (currentTargetIndex < 0 || currentTargetIndex >= targets.Count || _map == null) return;
        Target t = targets[currentTargetIndex];

        var targetLatLng = Conversions.StringToLatLon(t.locationString);
        Vector2d targetPos = new Vector2d(targetLatLng.Latitude, targetLatLng.Longitude);

        double distanceToTarget = Distance(
            playerPos.x, playerPos.y,
            targetPos.x, targetPos.y, 'K'
        );

        // DEBUG LOGS START
        debugTxt.text =
           "Location: " +
           "\nLat: " + playerPos.x +
           "\nLon: " + playerPos.y;

        debugTxt.text += "\n\nDistance: " + distanceToTarget;
        // DEBUG LOGS END


        if (distanceToTarget <= thresholdKm && !t.visited)
        {
            // Spawn marker in AR
            Vector3 worldPos = _map.MapInformation.ConvertLatLngToPosition(targetLatLng);
            GameObject instance = Instantiate(t.discoveredPrefab, worldPos, Quaternion.identity);
            t.currentInstance = instance;
            t.visited = true;

            // Advance to next target
            currentTargetIndex++;
        }
    }

    // ** DISTANCE CALCULATIONS **

    //https://www.geodatasource.com/resources/tutorials/how-to-calculate-the-distance-between-2-locations-using-c/
    private double Distance(double lat1, double lon1, double lat2, double lon2, char unit)
    {
        if ((lat1 == lat2) && (lon1 == lon2))
        {
            return 0;
        }
        else
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }
    }

    private double deg2rad(double deg)
    {
        return (deg * Math.PI / 180.0);
    }

    private double rad2deg(double rad)
    {
        return (rad / Math.PI * 180.0);
    }
}
