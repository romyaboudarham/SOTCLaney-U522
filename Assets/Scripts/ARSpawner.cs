using UnityEngine;
using Mapbox.BaseModule.Data.Vector2d;
using Mapbox.Example.Scripts.LocationBehaviours;
using Mapbox.BaseModule.Utilities;

public class ARSpawner : MonoBehaviour
{
    private SpawnOnMapV3 mapSpawner;

    private LatitudeLongitude[] _locations;

    void Start()
    {
        mapSpawner = FindObjectOfType<SpawnOnMapV3>(); // finds the persistent object

        if (mapSpawner != null)
        {
            _locations = new LatitudeLongitude[mapSpawner._locationStrings.Length];
            for (int i = 0; i < mapSpawner._locationStrings.Length; i++)
            {
                _locations[i] = Conversions.StringToLatLon(mapSpawner._locationStrings[i]);
                Debug.Log("Spawn AR object at: " + _locations[i]);
                // You can now spawn your AR object at this coordinate
            }
        }
    }
}
