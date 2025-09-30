using UnityEngine;
using Mapbox.BaseModule.Data.Vector2d;
using System;

/// <summary>
/// Utility class for geographic and mathematical calculations
/// </summary>
public static class GeoMathUtils
{
    private const float EarthRadius = 6378137f; // meters

    /// <summary>
    /// Calculate distance between two geographic points using Haversine formula
    /// </summary>
    /// <param name="lat1">First point latitude</param>
    /// <param name="lon1">First point longitude</param>
    /// <param name="lat2">Second point latitude</param>
    /// <param name="lon2">Second point longitude</param>
    /// <param name="unit">Unit of measurement ('K' for kilometers, 'N' for nautical miles, default is statute miles)</param>
    /// <returns>Distance in specified units</returns>
    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2, char unit = 'M')
    {
        if ((lat1 == lat2) && (lon1 == lon2)) return 0;

        double theta = lon1 - lon2;
        double dist = Math.Sin(DegToRad(lat1)) * Math.Sin(DegToRad(lat2)) +
                      Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) * Math.Cos(DegToRad(theta));

        dist = Math.Acos(dist);
        dist = RadToDeg(dist);
        dist = dist * 60 * 1.1515; // Convert to statute miles

        if (unit == 'K') dist = dist * 1.609344; // Convert to kilometers
        else if (unit == 'N') dist = dist * 0.8684; // Convert to nautical miles

        return dist;
    }

    /// <summary>
    /// Calculate AR-space relative offset in meters between two geographic points
    /// </summary>
    /// <param name="player">Player's geographic position</param>
    /// <param name="target">Target's geographic position</param>
    /// <returns>ENU (East-North-Up) offset vector in meters</returns>
    public static Vector3 ComputeArRelativeOffsetMeters(LatitudeLongitude player, LatitudeLongitude target)
    {
        // Convert degrees to radians
        float lat1 = (float)player.Latitude * Mathf.Deg2Rad;
        float lon1 = (float)player.Longitude * Mathf.Deg2Rad;
        float lat2 = (float)target.Latitude * Mathf.Deg2Rad;
        float lon2 = (float)target.Longitude * Mathf.Deg2Rad;

        float dLat = lat2 - lat1;
        float dLon = lon2 - lon1;
        float meanLat = (lat1 + lat2) * 0.5f;

        // East-North in meters (ENU) - absolute GPS-based position
        float metersNorth = dLat * EarthRadius; // +Z
        float metersEast = dLon * EarthRadius * Mathf.Cos(meanLat); // +X
        Vector3 enu = new Vector3(metersEast, 0f, metersNorth);

        return enu;
    }

    /// <summary>
    /// Get compass heading with fallbacks
    /// </summary>
    /// <param name="camera">Camera to use as fallback if compass unavailable</param>
    /// <returns>Heading in degrees</returns>
    public static float GetCompassHeading(Camera camera = null)
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
            heading = camera != null ? camera.transform.eulerAngles.y : 0f;
            Debug.Log($"Using camera yaw fallback: {heading}");
        }
        
        return heading;
    }

    /// <summary>
    /// Clamp a vector to a maximum magnitude
    /// </summary>
    /// <param name="vector">Vector to clamp</param>
    /// <param name="maxMagnitude">Maximum magnitude</param>
    /// <returns>Clamped vector</returns>
    public static Vector3 ClampRange(Vector3 vector, float maxMagnitude)
    {
        if (vector.sqrMagnitude <= maxMagnitude * maxMagnitude) return vector;
        return vector.normalized * maxMagnitude;
    }

    /// <summary>
    /// Convert degrees to radians
    /// </summary>
    /// <param name="degrees">Angle in degrees</param>
    /// <returns>Angle in radians</returns>
    public static double DegToRad(double degrees) => (degrees * Math.PI / 180.0);

    /// <summary>
    /// Convert radians to degrees
    /// </summary>
    /// <param name="radians">Angle in radians</param>
    /// <returns>Angle in degrees</returns>
    public static double RadToDeg(double radians) => (radians / Math.PI * 180.0);
}
