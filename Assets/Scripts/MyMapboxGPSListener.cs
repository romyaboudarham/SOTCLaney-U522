using UnityEngine;
using Mapbox.LocationModule;
using TMPro;

using System.Collections;
using System.Collections.Generic;
using System;
using Mapbox.BaseModule.Data.Vector2d;   // ✅ For LatitudeLongitude
using Mapbox.Example.Scripts.LocationBehaviours;
using Mapbox.BaseModule.Utilities;

public class MyMapboxGPSListener : MonoBehaviour
{
    public GameObject questionMarker;
    bool createdMarker = false;

    private DeviceLocationProvider _locationProvider;
    public TMP_Text debugTxt;

    [SerializeField] public string[] _locationStrings;

    void Start()
    {
        // Find the Mapbox location provider in the scene
        _locationProvider = FindObjectOfType<DeviceLocationProvider>();
        if (_locationProvider == null)
        {
            Debug.LogError("No DeviceLocationProvider found!");
            debugTxt.text = "FAILED: No provider";
            return;
        }

        // Subscribe to updates
        _locationProvider.OnLocationUpdated += HandleLocationUpdated;
        debugTxt.text = "Subscribed to GPS updates...";
    }

    private void HandleLocationUpdated(Location location)
    {
        // Debug.Log($"[GPS] Lat: {location.LatitudeLongitude.Latitude}, Lon: {location.LatitudeLongitude.Longitude}, Acc: {location.Accuracy}");

        debugTxt.text =
            "Location: " +
            "\nLat: " + location.LatitudeLongitude.Latitude +
            "\nLon: " + location.LatitudeLongitude.Longitude +
            "\nAcc: " + location.Accuracy;
        double distanceBetween = distance((double)location.LatitudeLongitude.Latitude, (double)location.LatitudeLongitude.Longitude, (double)37.845677, (double)-122.266856, 'K');

        debugTxt.text += "\n\nDistance: " + distanceBetween;

            if (distanceBetween < 0.01)
            {
                if (createdMarker == false)
                {
                    createdMarker = true;
                    Instantiate(questionMarker, new Vector3(), Quaternion.identity);
                }
                debugTxt.text += "\nFOUND OBJECT 1 ";
            }
    }

    void OnDestroy()
    {
        if (_locationProvider != null)
        {
            _locationProvider.OnLocationUpdated -= HandleLocationUpdated;
        }
    }

    // ** DISTANCE CALCULATIONS **

    //https://www.geodatasource.com/resources/tutorials/how-to-calculate-the-distance-between-2-locations-using-c/
    private double distance(double lat1, double lon1, double lat2, double lon2, char unit)
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

    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    //::  This function converts decimal degrees to radians             :::
    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    private double deg2rad(double deg)
    {
        return (deg * Math.PI / 180.0);
    }

    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    //::  This function converts radians to decimal degrees             :::
    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    private double rad2deg(double rad)
    {
        return (rad / Math.PI * 180.0);
    }
}
