using UnityEngine;
using System.Collections.Generic;
using Mapbox.BaseModule.Data.Vector2d;   // ✅ For LatitudeLongitude
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using Mapbox.Example.Scripts.Map;

namespace Mapbox.Example.Scripts.LocationBehaviours
{
    public class SpawnOnMapV3 : MonoBehaviour
    {
        [SerializeField] private MapboxMapBehaviour _mapCore;
        [SerializeField] public GameObject[] _markerPrefabs;
        [SerializeField] public string[] _locationStrings;

        [SerializeField] private float _spawnScale = 1f;

        private MapboxMap _map;
        private LatitudeLongitude[] _locations;
        private List<GameObject> _spawnedObjects;

        void Awake()
        {
            DontDestroyOnLoad(gameObject); // keeps this object across scene loads
        }

        private void Start()
        {
            if (_mapCore == null)
            {
                Debug.LogError("MapBehaviourCore is not assigned!");
                return;
            }

            // Parse input strings into Lat/Lng
            _locations = new LatitudeLongitude[_locationStrings.Length];
            for (int i = 0; i < _locationStrings.Length; i++)
            {
                _locations[i] = Conversions.StringToLatLon(_locationStrings[i]);
            }

            _spawnedObjects = new List<GameObject>();

            // Wait for map to finish initializing
            _mapCore.Initialized += (map) =>
            {
                _map = map;

                // ✅ Recenter map root on the first location
                if (_locations.Length > 0)
                {
                    //_map.MapInformation.SetInformation(_locations[0]);
                    Debug.Log($"Map recentered on {_locations[0]}");
                }

                SpawnMarkers();
            };
        }

        private void SpawnMarkers()
        {
            for (int i = 0; i < _locations.Length; i++)
            {
                Vector3 localPos = _map.MapInformation.ConvertLatLngToPosition(_locations[i]);
                var instance = Instantiate(_markerPrefabs[i], localPos, Quaternion.identity, _mapCore.UnityContext.MapRoot);
                instance.transform.localScale = Vector3.one * _spawnScale;

                Debug.Log($"Spawned marker {i} at {localPos} (latlng: {_locations[i]})");
            }
        }

        private void Update()
        {
            if (_map == null || _map.Status < InitializationStatus.ReadyForUpdates) return;

            // Update marker positions (in case map shifts/recalculates)
            for (int i = 0; i < _spawnedObjects.Count; i++)
            {
                Vector3 localPos = _map.MapInformation.ConvertLatLngToPosition(_locations[i]);
                _spawnedObjects[i].transform.localPosition = localPos;
                _spawnedObjects[i].transform.localScale = Vector3.one * _spawnScale;
            }
        }
    }
}
