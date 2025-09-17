using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Unity;
using Mapbox.ImageModule.Terrain.Settings;
using UnityEngine;

namespace Mapbox.ImageModule.Terrain.TerrainStrategies
{
	public class TerrainStrategy
	{
		protected float TileSize = 0;

		[SerializeField]
		protected ElevationLayerProperties _elevationOptions = new ElevationLayerProperties();

		public virtual int RequiredVertexCount
		{
			get { return 0; }
		}

		public virtual void Initialize(ElevationLayerProperties elOptions)
		{
			if (elOptions != null)
			{
				_elevationOptions = elOptions;
				TileSize = _elevationOptions.TileMeshSize;
			}
		}


		public virtual void RegisterTile(UnityMapTile tile, bool createElevatedMesh)
		{

		}
	}
}
