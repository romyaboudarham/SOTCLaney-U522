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
    [SerializeField] private GameObject Prefab_AR_Undiscovered;
    [SerializeField] private List<Target> targets;
    [SerializeField] private Transform arTargetsRoot;
    [SerializeField] private ARAnchorManager anchorManager;
    public TMP_Text debugTxt;

    [Header("AR Spawn Settings")]
    [SerializeField] private float maxArSpawnRangeMeters = 50f;
    [SerializeField] private float arSpawnScale = 0.15f;
    private MapboxMapBehaviour _mapCore;
    private MapboxMap _map;
    private QuestManager questManager;
    private MapManager mapManager;
    private BootstrapLoader bootstrapLoader;
    private ILocationProvider _locationProvider; // GPS

    private int currentTargetIndex = 0;
    private double thresholdKm = 0.01; // 10 meters
    private double thresholdSpawnKm = 0.02; // 10 meters

    private void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
        mapManager = FindObjectOfType<MapManager>();
        bootstrapLoader = FindObjectOfType<BootstrapLoader>();
        _mapCore = bootstrapLoader.GetMapCore();
        _map = bootstrapLoader.GetMap();

        MapManager.Instance.targetManager = this;

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
        if (target == null)
        {
            Debug.LogError("SpawnTargetOnMap called with null target");
            return;
        }

        if (_map == null || _mapCore == null)
        {
            Debug.LogWarning("Map not ready in SpawnTargetOnMap. Skipping spawn for target: " + target.targetName);
            return;
        }

        if (_map.Status < InitializationStatus.ReadyForUpdates)
        {
            Debug.LogWarning("Map status not ReadyForUpdates. Skipping spawn for target: " + target.targetName);
            return;
        }

        var latLng = Conversions.StringToLatLon(target.locationString);
        Vector3 localPos = _map.MapInformation.ConvertLatLngToPosition(latLng);

        var prefab = target.completed ? target.Prefab_Completed : target.Prefab_Undiscovered;
        if (prefab == null)
        {
            Debug.LogError($"Prefab missing for target '{target.targetName}'. Completed={target.completed}. Assign in inspector.");
            return;
        }

        var spawnScale = target.completed ? target.SpawnScale_Completed : target.SpawnScale_Undiscovered;

        Transform parent = _mapCore.UnityContext != null ? _mapCore.UnityContext.MapRoot : null;
        if (parent == null)
        {
            Debug.LogWarning("MapRoot is null. Spawning without parent for target: " + target.targetName);
        }

        var instance = parent != null
            ? Instantiate(prefab, localPos, Quaternion.identity, parent)
            : Instantiate(prefab, localPos, Quaternion.identity);

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

    public GameObject SpawnTestInFront(GameObject prefab, float meters = 3f)
    {
        var fwd = arCamera.transform.forward;
        var worldPos = arCamera.transform.position + fwd.normalized * meters;
        var pose = new Pose(worldPos, Quaternion.LookRotation(fwd));
        return Instantiate(prefab, pose.position, pose.rotation, arTargetsRoot);
    }

    public GameObject SpawnCurrentTargetTestInFront(float meters = 3f, float scale = 1f)
    {
        if (currentTargetIndex < 0 || currentTargetIndex >= targets.Count)
        {
            Debug.LogWarning("SpawnCurrentTargetTestInFront: invalid currentTargetIndex");
            return null;
        }
        var currentTarget = targets[currentTargetIndex];
        if (currentTarget == null || currentTarget.Prefab_Undiscovered == null)
        {
            Debug.LogWarning("SpawnCurrentTargetTestInFront: current target or prefab is null");
            return null;
        }
        var go = SpawnTestInFront(currentTarget.Prefab_Undiscovered, meters);
        go.transform.localScale = Vector3.one * (Mathf.Approximately(scale, 0f) ? 1f : scale);
        currentTarget.arInstance = go;
        return go;
    }

    private Vector3 ComputeArRelativeOffsetMeters(LatitudeLongitude player, LatitudeLongitude target)
    {
        // Convert degrees to radians
        float lat1 = (float)player.Latitude * Mathf.Deg2Rad;
        float lon1 = (float)player.Longitude * Mathf.Deg2Rad;
        float lat2 = (float)target.Latitude * Mathf.Deg2Rad;
        float lon2 = (float)target.Longitude * Mathf.Deg2Rad;

        const float EarthRadius = 6378137f; // meters
        float dLat = lat2 - lat1;
        float dLon = lon2 - lon1;
        float meanLat = (lat1 + lat2) * 0.5f;

        // East-North in meters (ENU) - absolute GPS-based position
        float metersNorth = dLat * EarthRadius; // +Z
        float metersEast = dLon * EarthRadius * Mathf.Cos(meanLat); // +X
        Vector3 enu = new Vector3(metersEast, 0f, metersNorth);

        // DON'T rotate position by compass - keep absolute GPS position
        // Compass rotation is applied separately in SpawnAtGeoPosition for object orientation only
        return enu;
    }

    private Vector3 ClampRange(Vector3 v, float maxMagnitude)
    {
        if (v.sqrMagnitude <= maxMagnitude * maxMagnitude) return v;
        return v.normalized * maxMagnitude;
    }

    private float GetCompassHeading()
    {
        // Start compass if not already started
        if (!Input.compass.enabled)
        {
            Input.compass.enabled = true;
        }
        
        float heading = 0f;
        
        // Wait a moment for compass to initialize
        if (Input.compass.enabled)
        {
            // Use true heading if available (more accurate)
            if (Input.compass.trueHeading != 0f)
            {
                heading = Input.compass.trueHeading;
                Debug.Log($"Using compass trueHeading: {heading}");
            }
            // Fallback to magnetic heading
            else if (Input.compass.magneticHeading != 0f)
            {
                heading = Input.compass.magneticHeading;
                Debug.Log($"Using compass magneticHeading: {heading}");
            }
        }
        
        // Final fallback to camera yaw if compass not available
        if (heading == 0f)
        {
            heading = arCamera != null ? arCamera.transform.eulerAngles.y : 0f;
            Debug.Log($"Using camera yaw fallback: {heading}");
        }
        
        return heading;
    }

    // Called by QuestManager when we are in AR scene and new quest started
    public void SpawnUnreachedTargetInAR(Target currentTarget, LatitudeLongitude targetLatLng, Vector2d playerVec)
    {
        if (currentTarget == null || Prefab_AR_Undiscovered == null)
        {
            Debug.LogWarning("SpawnUnreachedTargetInAR: target or prefab missing");
            return;
        }

        var playerLatLng = Vector2dToLatLon(playerVec);

        // Compute AR-space relative offset in meters using ENU
        Vector3 relativePos = ComputeArRelativeOffsetMeters(playerLatLng, targetLatLng);
        relativePos = ClampRange(relativePos, maxArSpawnRangeMeters);

        currentTarget.arInstance = SpawnAtGeoPosition(Prefab_AR_Undiscovered, relativePos, arSpawnScale);
    }

     public GameObject SpawnAtGeoPosition(GameObject prefab, Vector3 relativePos, float scale)
    {
        // Calculate where in AR space the object should go (absolute GPS position)
        Vector3 worldPos = arCamera.transform.position + relativePos;
        
        // Apply compass rotation only to object orientation, not position
        float compassHeading = GetCompassHeading();
        Quaternion compassRotation = Quaternion.Euler(0f, compassHeading, 0f);
        Pose pose = new Pose(worldPos, compassRotation);

        // Create an anchor GameObject at the pose and add ARAnchor component
        GameObject anchorObject = new GameObject("ARAnchor");
        anchorObject.transform.SetParent(arTargetsRoot, false);
        anchorObject.transform.SetPositionAndRotation(pose.position, pose.rotation);
        ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();

        if (anchor == null)
        {
            // Fallback to plain spawn if anchoring not available
            var go = Instantiate(prefab, pose.position, pose.rotation, arTargetsRoot);
            go.transform.localScale = Vector3.one * scale;
            return go;
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

    // Called when map is opened to refresh AR target positions with current camera orientation
    public void RefreshARTargetOrientations()
    {
        Debug.Log("Refreshing AR target orientations with current camera heading");
        
        // Get current player position
        if (_locationProvider == null) return;
        
        var currentLocation = _locationProvider.CurrentLocation;
        
        Vector2d playerPos = new Vector2d(
            currentLocation.LatitudeLongitude.Latitude,
            currentLocation.LatitudeLongitude.Longitude
        );

        // Update AR target positions for all active targets
        for (int i = 0; i <= currentTargetIndex && i < targets.Count; i++)
        {
            var target = targets[i];
            if (target.arInstance != null && !target.completed)
            {
                // Destroy existing AR instance
                Destroy(target.arInstance);
                target.arInstance = null;
                
                // Respawn with current orientation
                var targetLatLng = Conversions.StringToLatLon(target.locationString);
                SpawnUnreachedTargetInAR(target, targetLatLng, playerPos);
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


      
