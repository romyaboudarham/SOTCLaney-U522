using System;
using Mapbox.BaseModule.Data.Tiles;
using Mapbox.BaseModule.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mapbox.BaseModule.Unity
{
	public class UnityMapTile : MonoBehaviour
	{
		public UnwrappedTileId UnwrappedTileId { get; private set; }
		public CanonicalTileId CanonicalTileId { get; private set; }
		public float TileScale { get; private set; }

		//change this with list T : containers?
		public UnityTileTerrainContainer TerrainContainer;
		public UnityTileImageContainer ImageContainer;
		public UnityTileVectorContainer VectorContainer;
		
		private string _tileScaleFieldNameID = "_TileScale";
		
		private MeshRenderer _meshRenderer;
		public MeshRenderer MeshRenderer
		{
			get
			{
				if (_meshRenderer == null)
				{
					_meshRenderer = GetComponent<MeshRenderer>();
					if (_meshRenderer == null)
					{
						_meshRenderer = gameObject.AddComponent<MeshRenderer>();
					}
				}
				return _meshRenderer;
			}
		}

		public Material Material;
		private MeshFilter _meshFilter;
		public MeshFilter MeshFilter
		{
			get
			{
				if (_meshFilter == null)
				{
					_meshFilter = GetComponent<MeshFilter>();
					if (_meshFilter == null)
					{
						_meshFilter = gameObject.AddComponent<MeshFilter>();
						_meshFilter.sharedMesh = new Mesh();
					}
				}
				return _meshFilter;
			}
		}

		public bool IsTemporary = false;

		public UnityMapTile()
		{
			TerrainContainer = new UnityTileTerrainContainer(this);
			ImageContainer = new UnityTileImageContainer(this);
			VectorContainer = new UnityTileVectorContainer(this);
		}
		
		public void Initialize(UnwrappedTileId tileId, float scale)
		{
			TileScale = 1 / scale;
			//var test = (float) (1 / Conversions.TileBoundsInWebMercator(tileId).Size.x); // * scaleCurve;
			//var latCompensation = 1 / Mathf.Cos(Mathf.Deg2Rad * (float)Conversions.TileIdToCenterLatitudeLongitude(tileId.X, tileId.Y, tileId.Z).y);
			//TileScale *= latCompensation;

			UnwrappedTileId = tileId;
			CanonicalTileId = tileId.Canonical;
#if UNITY_EDITOR
			gameObject.name = tileId.ToString();
#endif
			
			Material.SetFloat(_tileScaleFieldNameID, TileScale);
		}
		
		public void Recycle()
		{
			gameObject.SetActive(false);
			ImageContainer.GetAndClearImageData();
			TerrainContainer.GetAndClearTerrainData();
			VectorContainer.GetAndClearVectorData();
		}
		
		private void OnDestroy()
		{
			ImageContainer.OnDestroy();
			TerrainContainer.OnDestroy();
			VectorContainer.OnDestroy();
		}
	}
}
