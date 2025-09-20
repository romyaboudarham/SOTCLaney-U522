using UnityEngine;
using Mapbox.LocationModule;
using TMPro;

using System.Collections;
using System.Collections.Generic;
using System;
using Mapbox.BaseModule.Data.Vector2d;
using Mapbox.Example.Scripts.LocationBehaviours;
using Mapbox.BaseModule.Utilities;

public class MyMapboxGPSListener : MonoBehaviour
{
    public TMP_Text debugTxt;

    //private DeviceLocationProvider _locationProvider;
    private DeviceLocationProvider _locationProvider; // <-- works for both

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        StartCoroutine(InitializeGPSProvider());
    }

    void OnDisable()
    {
        if (_locationProvider != null)
        {
            _locationProvider.OnLocationUpdated -= HandleLocationUpdated;
        }
    }

    private IEnumerator InitializeGPSProvider()
    {
        debugTxt.text = "Waiting for GPS provider...";

        float timeout = 10f; // wait up to 10 seconds
        while (_locationProvider == null && timeout > 0f)
        {
            _locationProvider = FindObjectOfType<DeviceLocationProvider>();
            if (_locationProvider != null) break;

            timeout -= Time.deltaTime;
            yield return null;
        }

        if (_locationProvider == null)
        {
            debugTxt.text = "No DeviceLocationProvider found!";
            yield break;
        }

        // Subscribe to location updates
        _locationProvider.OnLocationUpdated += HandleLocationUpdated;
        debugTxt.text = $"Subscribed to GPS updates ({_locationProvider.name})";
    }

    // void Start()
    // {
    //     // Find the Mapbox location provider in the scene
    //     _locationProvider = FindObjectOfType<DeviceLocationProvider>();
    //     if (_locationProvider == null)
    //     {
    //         Debug.LogError("No DeviceLocationProvider found!");
    //         debugTxt.text = "FAILED: No provider";
    //         return;
    //     }

    //     // Subscribe to updates
    //     _locationProvider.OnLocationUpdated += HandleLocationUpdated;
    //     debugTxt.text = "Subscribed to GPS updates...";
    // }

    private void HandleLocationUpdated(Location location)
    {
        // Debug.Log($"[GPS] Lat: {location.LatitudeLongitude.Latitude}, Lon: {location.LatitudeLongitude.Longitude}, Acc: {location.Accuracy}");
        // debugTxt.text =
        //     "Location: " +
        //     "\nLat: " + location.LatitudeLongitude.Latitude +
        //     "\nLon: " + location.LatitudeLongitude.Longitude +
        //     "\nAcc: " + location.Accuracy;
        // double distanceBetween = distance((double)location.LatitudeLongitude.Latitude, (double)location.LatitudeLongitude.Longitude, (double)37.845677, (double)-122.266856, 'K');

        // debugTxt.text += "\n\nDistance: " + distanceBetween;

        //     if (distanceBetween < 0.01)
        //     {
        //         if (createdMarker == false)
        //         {
        //             createdMarker = true;
        //             Instantiate(questionMarker, new Vector3(), Quaternion.identity);
        //         }
        //         debugTxt.text += "\nFOUND OBJECT 1 ";
        //     }

        if (TargetManager.Instance != null)
        {
            Vector2d playerPos = new Vector2d(
                location.LatitudeLongitude.Latitude,
                location.LatitudeLongitude.Longitude
            );

            TargetManager.Instance.HandlePlayerNearCurrentTarget(playerPos, 0.02);
        }
    }

    void OnDestroy()
    {
        if (_locationProvider != null)
        {
            _locationProvider.OnLocationUpdated -= HandleLocationUpdated;
        }
    }
}
