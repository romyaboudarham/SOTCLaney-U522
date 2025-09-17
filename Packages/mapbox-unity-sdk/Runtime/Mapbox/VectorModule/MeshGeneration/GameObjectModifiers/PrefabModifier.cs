using System;
using System.Collections.Generic;
using System.Linq;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Unity;
using Mapbox.BaseModule.Utilities;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.GameObjectModifiers
{
	[Serializable]
	public class PrefabModifierSettings
	{
		public float Range = 0.01f;
		public GameObject Prefab;
		public bool ScaleDownWithWorld = false;
	}

	[Serializable]
	public class PrefabModifier : GameObjectModifier
	{
		public Action<GameObject> PrefabCreated = (s) => { };

		private UnityContext _unityContext;
		private Dictionary<GameObject, GameObject> _objects;
		private readonly PrefabModifierSettings _settings;
		private  GameObject _objectContainer;
		
		public PrefabModifier(UnityContext unityContext, PrefabModifierSettings settings)
		{
			_unityContext = unityContext;
			_settings = settings;
			if (_objects == null)
			{
				_objects = new Dictionary<GameObject, GameObject>();
			}

			_objectContainer = new GameObject("Prefab Container");
			if (_unityContext != null)
			{
				_objectContainer.transform.SetParent(_unityContext.RuntimeGenerationRoot);
			}
		}
		
		public override void Run(VectorEntity ve, IMapInformation mapInformation)
		{
			var rectd = Conversions.TileBoundsInUnitySpace(ve.Feature.TileId, mapInformation.CenterMercator, mapInformation.Scale);
			var tilePos = new Vector3((float) rectd.Center.x, 0, (float) rectd.Center.y);
			var tileScale = (float) rectd.Size.x;
			int selpos = ve.Feature.Points[0].Count / 2;
			var met = tilePos + (ve.Feature.Points[0][selpos] * tileScale);

			if (_objects.Any(x => Vector3.Distance(x.Value.transform.position, met) < _settings.Range))
				return;

			GameObject go;
			
			if (_objects.ContainsKey(ve.GameObject))
			{
				go = _objects[ve.GameObject];
				go.name = ve.Feature.Data.Id.ToString();
				go.transform.localPosition = met;
				return;
			}
			
			go = GameObject.Instantiate(_settings.Prefab, _objectContainer.transform);
			_objects.Add(ve.GameObject, go);
			go.name = ve.Feature.Data.Id.ToString();
			go.transform.position = met;

			PrefabCreated(go);
		}
	}
}
