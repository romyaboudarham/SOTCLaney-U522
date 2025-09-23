using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Mapbox.BaseModule.Data.Vector2d;   // For LatitudeLongitude
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using Mapbox.Example.Scripts.Map;
using Mapbox.LocationModule; // GPS
using TMPro;

public class SpawnOnMapV3 : MonoBehaviour
{
    [SerializeField] private MapboxMapBehaviour _mapCore;
    private MapboxMap _map;

    [SerializeField] private List<Target> targets;
    public TMP_Text debugTxt;

    private ILocationProvider  _locationProvider; // GPS

    int currentTargetIndex = 0;
    double thresholdKm = 0.02; // 20 meters

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

    void OnEnable()
    {
        // Grab the right provider (Editor vs Device chosen by Mapbox internally)
        _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;

        if (_locationProvider == null)
        {
            debugTxt.text = "No LocationProvider found!";
        }
        else
        {
            debugTxt.text = $"Using {_locationProvider.GetType().Name} for GPS";
        }

        if (_locationProvider != null)
        {
            _locationProvider.OnLocationUpdated += HandleLocationUpdated;
        }
    }

    void OnDisable()
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

    void OnDestroy()
    {
        if (_locationProvider != null)
        {
            _locationProvider.OnLocationUpdated -= HandleLocationUpdated;
        }
    }

    public void InitializeAndSpawn(int currentIndex)
    {
        currentTargetIndex = currentIndex;
        StartCoroutine(WaitForMapReady());
    }

    private IEnumerator WaitForMapReady()
    {
        while (_map.Status < InitializationStatus.ReadyForUpdates)
        {
            yield return null; // wait for next frame
        }

        Debug.Log("Map is ready! Spawning targets now.");
        SpawnTargets(currentTargetIndex);
    }


    public void SpawnTargets(int currentTargetIndex)
    {
        for (int i = 0; i <= currentTargetIndex; i++)
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

    public void HandlePlayerNearCurrentTarget(Vector2d playerPos)
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