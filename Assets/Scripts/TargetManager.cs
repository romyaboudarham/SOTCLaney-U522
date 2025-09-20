using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.BaseModule.Data.Vector2d;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;

using TMPro;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private List<Target> targets;
    [SerializeField] private UIManager uiManager;

    private int currentTargetIndex = 0;

    public static TargetManager Instance { get; private set; }

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
        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        uiManager.ShowQuestUnlocked(targets[currentTargetIndex].targetName);
        // uiManager.ShowIntro(() =>
        // {
        //     ActivateTarget(currentTargetIndex);
        // });
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

    // Called when MapScene loads
    public void InitializeMap(SpawnOnMapV3 spawner)
    {
        if (spawner == null) return;

        spawner.InitializeAndSpawn(targets, currentTargetIndex);
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
