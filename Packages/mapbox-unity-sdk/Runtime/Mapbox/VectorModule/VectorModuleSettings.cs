using System;
using Mapbox.BaseModule.Data.Interfaces;
using Mapbox.BaseModule.Map;
using UnityEngine;

namespace Mapbox.VectorModule
{
    [Serializable]
    public class VectorModuleSettings
    {
        public VectorSourceType SourceType;
        public string CustomSourceId;
        public bool LoadBackgroundData = false;
        public VectorSourceSettings DataSettings;
        
        [Tooltip("Tile outside this range will be rejected.")]
        public Vector2 RejectTilesOutsideZoom = new Vector2(12, 16);
    }
}