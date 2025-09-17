using System;
using Mapbox.BaseModule.Data.Interfaces;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Unity;
using Mapbox.ImageModule.Terrain.Settings;
using Mapbox.ImageModule.Terrain.TerrainStrategies;
using UnityEngine;

namespace Mapbox.Example.Scripts.ModuleBehaviours
{
    public class TileCreatorBehaviour : MonoBehaviour
    {
        [NonSerialized] private ITileCreator _tileCreator;
        
        [Tooltip("Materials for base map tile mesh and gameobject")]
        public Material[] TileMaterials;
        
        [Tooltip("Settings for base map tile mesh")]
        public ElevationLayerProperties Settings;

        public int CacheSize = 25;
    
        public ITileCreator GetTileCreator(UnityContext unityContext)
        {
            if (_tileCreator != null) return _tileCreator;

            _tileCreator = new TileCreator(unityContext, new ElevatedTerrainStrategy(), Settings, TileMaterials, CacheSize);
            return _tileCreator;
        }
    }
}
