using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using Mapbox.Example.Scripts.Map;
using Mapbox.LocationModule;
using UnityEngine;

namespace Mapbox.Examples
{
	public class CharacterMovement : MonoBehaviour
	{
		public MapBehaviourCore MapBehaviour;
		private IMapInformation _mapInformation;
		public Animator CharacterAnimator;
		public float Speed;
		private float _scale;
		private bool _readyForUpdates = false;
		
		public bool SnapToTerrain = false;
		
		// GPS tracking
		private ILocationProvider _locationProvider;
		private Vector3 _lastPosition;
		private bool _isMoving = false;

		private void Start()
		{ 
			MapBehaviour.Initialized += map =>
			{
				_mapInformation = map.MapInformation;
				_scale = map.MapInformation.Scale;
				_readyForUpdates = true;
			};
			
			// Setup GPS tracking
			_locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
			if (_locationProvider != null)
			{
				_locationProvider.OnLocationUpdated += OnLocationUpdated;
			}
		}

		void Update()
		{
			if (!_readyForUpdates)
				return;
			
			// Always rotate according to phone compass
			float compassHeading = GetCompassHeading();
			Quaternion compassRotation = Quaternion.Euler(0f, compassHeading, 0f);
			transform.rotation = compassRotation;
			
			// Move forward if GPS shows movement
			if (_isMoving)
			{
				transform.Translate(Vector3.forward * (Speed/_scale));
				if(CharacterAnimator) CharacterAnimator.SetBool("IsWalking", true);
			}
			else
			{
				if(CharacterAnimator) CharacterAnimator.SetBool("IsWalking", false);
			}

			if (SnapToTerrain)
			{
				var latlng = _mapInformation.ConvertPositionToLatLng(this.transform.position);
				var tileId = Conversions.LatitudeLongitudeToTileId(latlng, 16).Canonical;
				
				//changed this part and haven't tested...
				var tileSpace = Conversions.LatitudeLongitudeToInTile01(latlng, tileId);
				var elevation = _mapInformation.QueryElevation(tileId, tileSpace.x, tileSpace.y);
				transform.position = new Vector3(transform.position.x, elevation, transform.position.z);
			}
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
					Debug.Log($"Astronaut using compass trueHeading: {heading}");
				}
				// Fallback to magnetic heading
				else if (Input.compass.magneticHeading != 0f)
				{
					heading = Input.compass.magneticHeading;
					Debug.Log($"Astronaut using compass magneticHeading: {heading}");
				}
			}
			
			// Final fallback to camera yaw if compass not available
			if (heading == 0f)
			{
				heading = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;
				//Debug.Log($"Astronaut using camera yaw fallback: {heading}");
			}
			
			return heading;
		}
		
		private void OnLocationUpdated(Location location)
		{
			if (!_readyForUpdates || _mapInformation == null) return;
			
			// Convert GPS to map position
			var latLng = new Mapbox.BaseModule.Data.Vector2d.LatitudeLongitude(
				location.LatitudeLongitude.Latitude, 
				location.LatitudeLongitude.Longitude
			);
			Vector3 currentPosition = _mapInformation.ConvertLatLngToPosition(latLng);
			
			// Check if position changed significantly (movement threshold)
			float movementThreshold = 0.1f / _scale; // 10cm in real world
			if (Vector3.Distance(_lastPosition, currentPosition) > movementThreshold)
			{
				_isMoving = true;
				_lastPosition = currentPosition;
				
				// Update astronaut position to follow GPS
				transform.position = currentPosition;
				
				Debug.Log($"Astronaut following GPS movement to: {currentPosition}");
			}
			else
			{
				_isMoving = false;
			}
		}
		
		private void OnDestroy()
		{
			if (_locationProvider != null)
			{
				_locationProvider.OnLocationUpdated -= OnLocationUpdated;
			}
		}
	}
}