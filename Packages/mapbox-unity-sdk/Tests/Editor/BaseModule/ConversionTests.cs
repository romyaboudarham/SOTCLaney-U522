using Mapbox.BaseModule.Data.Tiles;
using Mapbox.BaseModule.Utilities;
using NUnit.Framework;

namespace Mapbox.BaseModuleTests
{
    public class ConversionTests
    {
        [Test]
        public void TileIdToBoundsCenterEqualsToLatlngToTileId()
        {
            var tileId = new CanonicalTileId(16, 37309, 18968);
            var bounds = Conversions.TileIdToBounds(tileId);
            var center = bounds.Center;
            var newTileId = Conversions.LatitudeLongitudeToTileId(center, 16);
            Assert.AreEqual(tileId.ToString(), newTileId.ToString());
        }
        
        [Test]
        public void TileIdToCenterLatLngToTile01ToLatlng()
        {
            var tileId = new CanonicalTileId(16, 37309, 18968);
            var bounds = Conversions.TileIdToBounds(tileId);
            var center = bounds.Center;
            var zeroOne = Conversions.LatitudeLongitudeToInTile01(center, tileId);
            var newLatLng = Conversions.Tile01ToLatitudeLongitude(zeroOne, tileId);
            Assert.AreEqual(center.ToString(), newLatLng.ToString());
        }
        
        [Test]
        public void StringToLatLngToMercatorToLatLng()
        {
            var str = "-77.0295,38.9165";
            var latlng = Conversions.StringToLatLon(str);
            var mercator = Conversions.LatitudeLongitudeToWebMercator(latlng);
            var newLatlng = Conversions.WebMercatorToLatLon(mercator);
            Assert.AreEqual(latlng.Latitude, newLatlng.Latitude, 0.001d);
            Assert.AreEqual(latlng.Longitude, newLatlng.Longitude, 0.001d);
        }
    }
}