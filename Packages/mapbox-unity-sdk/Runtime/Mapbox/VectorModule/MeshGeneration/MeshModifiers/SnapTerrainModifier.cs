using System;
using Mapbox.BaseModule.Data;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Utilities;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.MeshModifiers
{
    public interface ISnapTerrainModifier
    {
        void Run(VectorFeatureUnity feature, MeshData md, IMapInformation mapInfo);
    }

    [Serializable]
    public class SnapTerrainModifier : MeshModifier, ISnapTerrainModifier
    {
        public override void Run(VectorFeatureUnity feature, MeshData md, IMapInformation mapInfo)
        {
            var hcore = new SnapTerrainModifierCore();
            hcore.Run(feature, md, mapInfo);
        }
    }
    
    public class SnapTerrainModifierCore
    {
        public void Run(VectorFeatureUnity feature, MeshData md, IMapInformation mapInformation)
        {
            var rectd = Conversions.TileBoundsInUnitySpace(feature.TileId, mapInformation.CenterMercator, mapInformation.Scale);
            var _counter = md.Vertices.Count;
            if (_counter > 0)
            {
                for (int i = 0; i < _counter; i++)
                {
                    var h = mapInformation.QueryElevation(
                        feature.TileId,
                        (float)(md.Vertices[i].x) + .5f,
                        (float)(md.Vertices[i].z) + .5f)
                        / mapInformation.Scale
                        / rectd.Size.x;
                    md.Vertices[i] += new Vector3(0, (float) h, 0);
                }
            }
            else
            {
                foreach (var sub in feature.Points)
                {
                    _counter = sub.Count;
                    for (int i = 0; i < _counter; i++)
                    {
                        var h = 0f;
                        if (mapInformation.QueryElevation != null)
                        {
                            h = (float)(mapInformation.QueryElevation(
                                    feature.TileId,
                                    (float) (sub[i].x),
                                    (float) (sub[i].z + 1)) / mapInformation.Scale
                                                            / rectd.Size.x);
                        }

                        sub[i] += new Vector3(0, h, 0);

                    }
                }
            }
        }
    }
}