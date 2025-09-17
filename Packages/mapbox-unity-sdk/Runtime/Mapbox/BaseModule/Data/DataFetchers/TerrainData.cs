using System;
using Mapbox.BaseModule.Data.Tiles;
using UnityEngine;

namespace Mapbox.BaseModule.Data.DataFetchers
{
    [Serializable]
    public class TerrainData : RasterData
    {
        [HideInInspector] public float[] ElevationValues;
        public bool IsElevationDataReady = false;
        public Action ElevationValuesUpdated = () => { };
        
        public override void Clear()
        {
            base.Clear();
            IsElevationDataReady = false;
        }

        public void SetElevationValues(float[] elevationArray)
        {
            ElevationValues = elevationArray;
            IsElevationDataReady = true;
            ElevationValuesUpdated();
        }
        
        public float QueryHeightData(CanonicalTileId requestingSubTileId, float x, float y)
        {
            if (ElevationValues?.Length > 0)
            {
                var _terrainTextureScaleOffset = requestingSubTileId.CalculateScaleOffsetAtZoom(TileId.Z);
                return ReadElevation(x, y, _terrainTextureScaleOffset);
            }
            return 0;
        }
        
        public float QueryHeightData(Vector2 point)
        {
            return ReadElevation(point.x, point.y, new Vector4(1, 1, 0, 0));
        }
        
        public float QueryHeightData(float x, float y)
        {
            return ReadElevation(x, y, new Vector4(1, 1, 0, 0));
        }

        private float ReadElevation(float x, float y, Vector4 terrainTextureScaleOffset)
        {
            var width = (int) Mathf.Sqrt(ElevationValues.Length);
            var sectionWidth = width * terrainTextureScaleOffset.x - 1;
            var padding = width * new Vector2(terrainTextureScaleOffset.z, terrainTextureScaleOffset.w);

            var xx = padding.x + (x * sectionWidth);
            var yy = padding.y + (y * sectionWidth);

            var index = (int) yy * width
                        + (int) xx;
            if (ElevationValues.Length <= index)
            {
                return 0;
            }
            else
            {
                return ElevationValues[(int) yy * width + (int) xx];
            }
        }

        
    }
}