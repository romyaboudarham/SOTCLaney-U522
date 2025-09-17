using Mapbox.BaseModule.Data.Tiles;

namespace Mapbox.BaseModule.Data.Interfaces
{
	public interface ITerrainLayerModule : ILayerModule
	{
		float QueryElevation(CanonicalTileId tileId, float x, float y);
	}
}