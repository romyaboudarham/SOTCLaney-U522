using System;
using UnityEngine;
using System.Collections.Generic;
using Mapbox.BaseModule.Data.Vector2d;   // For LatitudeLongitude
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using Mapbox.Example.Scripts.Map;
using Mapbox.LocationModule; // GPS
using TMPro;
using UnityEngine.XR.ARFoundation;

public class TargetManager : MonoBehaviour
{
    [Serializable]
    public class Target
    {
        public string targetName;          // For UI
        public string locationString;      // "37.7749,-122.4194" format
        public GameObject Prefab_Undiscovered;
        public float SpawnScale_Undiscovered;
        public GameObject Prefab_Completed;
        public float SpawnScale_Completed;

        [HideInInspector] public bool reached;      // Player arrived at location
        [HideInInspector] public bool completed;    // Player did the action at location
        [HideInInspector] public GameObject currentInstance; // instance on the map
        [HideInInspector] public GameObject arInstance; // instance in AR
    }

    [SerializeField] Camera arCamera;
    [SerializeField] private List<Target> targets;
    [SerializeField] private Transform arTargetsRoot;
    public TMP_Text debugTxt;

    private MapboxMapBehaviour _mapCore;
    private MapboxMap _map;
    private QuestManager questManager;
    private BootstrapLoader bootstrapLoader;
    private ILocationProvider _locationProvider; // GPS

    private int currentTargetIndex = 0;
    private double thresholdKm = 0.01; // 10 meters
    private double thresholdSpawnKm = 0.02; // 10 meters

    private void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
        bootstrapLoader = FindObjectOfType<BootstrapLoader>();
        _mapCore = bootstrapLoader.GetMapCore();
        _map = bootstrapLoader.GetMap();

        if (_map == null)
        {
            Debug.LogError("MapboxMap is null! Cannot spawn targets.");
            return;
        }
    }

    private void OnEnable()
    {
        _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
    }

    public void EnableLocationUpdates()
    {
        if (_locationProvider != null)
        {
            Debug.Log("Enabling GPS updates");
            _locationProvider.OnLocationUpdated += HandleLocationUpdated;
        }
    }

    public void DisableLocationUpdates()
    {
        if (_locationProvider != null)
        {
            Debug.Log("Disabling GPS updates");
            _locationProvider.OnLocationUpdated -= HandleLocationUpdated;
        }
    }

    private void OnDisable()
    {
        DisableLocationUpdates();
    }

    private void OnDestroy()
    {
        if (_locationProvider != null)
        {
            _locationProvider.OnLocationUpdated -= HandleLocationUpdated;
        }
    }

    private void HandleLocationUpdated(Location location)
    {
        Vector2d playerPos = new Vector2d(
            location.LatitudeLongitude.Latitude,
            location.LatitudeLongitude.Longitude
        );

        HandlePlayerNearCurrentTarget(playerPos);
    }

    public void InitializeAndSpawn(int currentIndex)
    {
        currentTargetIndex = currentIndex;
        for (int i = 0; i <= currentTargetIndex; i++)
        {
            SpawnTargetOnMap(targets[i]);
        }
    }

    private void SpawnTargetOnMap(Target target)
    {
        var latLng = Conversions.StringToLatLon(target.locationString);
        Vector3 localPos = _map.MapInformation.ConvertLatLngToPosition(latLng);

        var prefab = target.completed ? target.Prefab_Completed : target.Prefab_Undiscovered;
        var spawnScale = target.completed ? target.SpawnScale_Completed : target.SpawnScale_Undiscovered;

        var instance = Instantiate(
            prefab,
            localPos,
            Quaternion.identity,
            _mapCore.UnityContext.MapRoot
        );
        instance.transform.localScale = Vector3.one * spawnScale;

        target.currentInstance = instance;
    }
    public static LatitudeLongitude Vector2dToLatLon(Vector2d v)
    {
        if (v == null)
        {
            throw new ArgumentNullException(nameof(v), "Vector2d input cannot be null");
        }

        double latitude = v.x;
        double longitude = v.y;

        return new LatitudeLongitude(latitude, longitude);
    }

    // Called by QuestManager when we are in AR scene and new quest started
    public void SpawnUnreachedTargetInAR(Target currentTarget, LatitudeLongitude targetLatLng, Vector2d playerVec)
    {
        var playerLatLng = Vector2dToLatLon(playerVec);
        var playerPos = _map.MapInformation.ConvertLatLngToPosition(playerLatLng);
        var targetPos = _map.MapInformation.ConvertLatLngToPosition(targetLatLng);

        Vector3 relativePos = targetPos - playerPos;
        var prefab = currentTarget.Prefab_Undiscovered;
        var spawnScale = 4f;

        currentTarget.arInstance = SpawnAtGeoPosition(prefab, relativePos, spawnScale);

        // var instance = Instantiate(
        //     prefab,
        //     localPos,
        //     Quaternion.identity,
        //     arTargetsRoot
        // );
        // instance.transform.localScale = Vector3.one * spawnScale;

        // currTarget.arInstance = instance;

    }

     public GameObject SpawnAtGeoPosition(GameObject prefab, Vector3 relativePos, float scale = 1f)
    {
        // Calculate where in AR space the object should go
        Vector3 worldPos = arCamera.transform.position + relativePos;

        Pose pose = new Pose(worldPos, Quaternion.identity);

        GameObject anchorObject = new GameObject("ARAnchor");
        anchorObject.transform.SetParent(arTargetsRoot, false); // parent under ARRoot
        anchorObject.transform.position = pose.position;
        anchorObject.transform.rotation = pose.rotation;

        ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();

        if (anchor == null)
        {
            Debug.LogWarning("Could not create anchor. Falling back to plain spawn.");
            return Instantiate(prefab, worldPos, Quaternion.identity, arTargetsRoot);
        }

        // Instantiate prefab as child of the anchor
        GameObject instance = Instantiate(prefab, pose.position, pose.rotation, anchor.transform);
        instance.transform.localScale = Vector3.one * scale;

        return instance;
    }

    public void HandlePlayerNearCurrentTarget(Vector2d playerPos)
    {
        if (currentTargetIndex < 0 || currentTargetIndex >= targets.Count || _map == null) return;
        Target currentTarget = targets[currentTargetIndex];

        var targetLatLng = Conversions.StringToLatLon(currentTarget.locationString);
        Vector2d targetPos = new Vector2d(targetLatLng.Latitude, targetLatLng.Longitude);

        double distanceToTarget = Distance(
            playerPos.x, playerPos.y,
            targetPos.x, targetPos.y, 'K'
        );

        // DEBUG
        debugTxt.text =
            "Location:" +
            "\nLat: " + playerPos.x +
            "\nLon: " + playerPos.y +
            "\n\nDistance: " + distanceToTarget;

        // Player is close enough
        if (distanceToTarget <= thresholdKm && !currentTarget.reached)
        {
            targets[currentTargetIndex].reached = true;
            Debug.Log($"Quest step {currentTargetIndex} reached! Waiting for completion...");

            // Notify QuestManager but do NOT auto-complete
            questManager.OnTargetReached();
        }

        // check for AR Target Spawn
        if (distanceToTarget <= thresholdSpawnKm && !currentTarget.reached && currentTarget.arInstance == null)
        {
            Debug.Log($"Spawning AR target {currentTargetIndex}");
            SpawnUnreachedTargetInAR(currentTarget, targetLatLng, playerPos);
        }
    }

    // Called by QuestManager when the player actually does the required action (e.g. open map, scan AR, etc.)
    public void MarkCurrentTargetCompleted()
    {
        if (currentTargetIndex < 0 || currentTargetIndex >= targets.Count) return;
        Target currentTarget = targets[currentTargetIndex];

        if (!currentTarget.completed)
        {
            targets[currentTargetIndex].completed = true;
            Debug.Log($"Quest step {currentTargetIndex} marked COMPLETED!");


            // Advance QuestManager
            questManager.OnTargetCompleted();
        }
    }

    public void ClearAllTargets()
    {
        Debug.Log("Clearing all targets from map");
        foreach (var target in targets)
        {
            if (target.currentInstance != null)
            {
                Destroy(target.currentInstance);
                target.currentInstance = null;
            }
        }
    }

    // DISTANCE CALCULATIONS
    private double Distance(double lat1, double lon1, double lat2, double lon2, char unit)
    {
        if ((lat1 == lat2) && (lon1 == lon2)) return 0;

        double theta = lon1 - lon2;
        double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) +
                      Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));

        dist = Math.Acos(dist);
        dist = rad2deg(dist);
        dist = dist * 60 * 1.1515;

        if (unit == 'K') dist = dist * 1.609344;
        else if (unit == 'N') dist = dist * 0.8684;

        return dist;
    }

    private double deg2rad(double deg) => (deg * Math.PI / 180.0);
    private double rad2deg(double rad) => (rad / Math.PI * 180.0);
}


      
