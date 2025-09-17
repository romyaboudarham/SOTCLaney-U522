using System;
using Mapbox.BaseModule.Data.Vector2d;
using Mapbox.BaseModule.Utilities;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities.Attributes;
using Mapbox.Utils;
using UnityEngine;
using System.Collections.Generic;

namespace Mapbox.Example.Scripts.LocationBehaviours
{
    public class SpawnOnMapV3 : MonoBehaviour
    {
        [SerializeField] 
        private MapBehaviourCore _mapCore;

        [SerializeField]
        [Geocode]
        private string[] _locationStrings;

        private LatitudeLongitude[] _locations;
        private List<GameObject> _spawnedObjects;

        [SerializeField] 
        private GameObject _markerPrefab;

        [SerializeField] 
        private float _spawnScale = 1f;

        private MapboxMap _map;

        private void Start()
        {
            _locations = new LatitudeLongitude[_locationStrings.Length];
            _spawnedObjects = new List<GameObject>();

            // Wait until map initializes before spawning
            _mapCore.Initialized += (map) =>
            {
                _map = map;
                SpawnMarkers();
            };
        }

        private void SpawnMarkers()
        {
            for (int i = 0; i < _locationStrings.Length; i++)
            {
                _locations[i] = Conversions.StringToLatLon(_locationStrings[i]);
                var worldPos = _map.MapInformation.ConvertLatLngToPosition(_locations[i]);
                Debug.Log($"Spawning marker {i} at {worldPos}");
                var instance = Instantiate(_markerPrefab, worldPos, Quaternion.identity, transform);
                instance.transform.localScale = Vector3.one * _spawnScale;
                _spawnedObjects.Add(instance);
            }
        }

        private void Update()
        {
            if (_map == null || _map.Status < InitializationStatus.ReadyForUpdates) return;

            for (int i = 0; i < _spawnedObjects.Count; i++)
            {
                var worldPos = _map.MapInformation.ConvertLatLngToPosition(_locations[i]);
                _spawnedObjects[i].transform.localPosition = worldPos;
                _spawnedObjects[i].transform.localScale = Vector3.one * _spawnScale;
            }
        }
    }
}
