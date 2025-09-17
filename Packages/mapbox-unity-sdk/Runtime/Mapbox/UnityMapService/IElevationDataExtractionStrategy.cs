using System;
using UnityEngine;

namespace Mapbox.UnityMapService
{
    public interface IElevationDataExtractionStrategy
    {
        void ExtractHeightData(Texture2D data, Action<float[]> callback);
    }
}