using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.BaseModule.Data.Tiles;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Unity;

namespace Mapbox.BaseModule.Data.Interfaces
{
    public interface IMapVisualizer
    {
        public void Load(TileCover tileCover);
        IEnumerator Initialize();
        IEnumerator LoadTileCoverToMemory(TileCover tileCover);
        void OnDestroy();
        Dictionary<UnwrappedTileId, UnityMapTile> ActiveTiles { get; }
        bool TryGetLayerModule<T>(Type type, out T layerModule) where T : ILayerModule;
    }
}