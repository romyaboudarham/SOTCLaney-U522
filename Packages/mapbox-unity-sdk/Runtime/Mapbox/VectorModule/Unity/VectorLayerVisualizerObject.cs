using System;
using System.Collections.Generic;
using System.Linq;
using Mapbox.BaseModule.Map;
using Mapbox.BaseModule.Unity;
using Mapbox.VectorModule.MeshGeneration.Unity;
using UnityEngine;

namespace Mapbox.VectorModule.Unity
{
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Layer Visualizer")]
	public class VectorLayerVisualizerObject : ScriptableObject
	{
		[SerializeField] private string _vectorLayerName;
		public string VectorLayerName => _vectorLayerName;
		public bool IsActive = true;

		[SerializeField] private VectorLayerVisualizerSettings _settings;
		[SerializeField] private List<ModifierStackObject> _modifierStackObjects;
		private VectorLayerVisualizer _layerVisualizer;
		
		public IVectorLayerVisualizer GetLayerVisualizer()
		{
			return _layerVisualizer;
		}
		
		public IVectorLayerVisualizer ConstructLayerVisualizer(IMapInformation mapInformation, UnityContext unityContext)
		{
			_layerVisualizer = new VectorLayerVisualizer(VectorLayerName, mapInformation, unityContext, _settings);
			_layerVisualizer.Active = IsActive;
			
			foreach (var modifierStackObject in _modifierStackObjects.Where(x => x != null))
			{
				modifierStackObject.Initialize(unityContext);
			}
			_layerVisualizer.AddModifierStack(_modifierStackObjects.Select(x => x.GetModifierStack).ToList());

			_layerVisualizer.OnVectorMeshCreated += OnVectorMeshCreated;
			_layerVisualizer.OnVectorMeshDestroyed += OnVectorMeshDestroyed;
			
			return _layerVisualizer;
		}

		public Action<GameObject> OnVectorMeshCreated = list => { };
		public Action<GameObject> OnVectorMeshDestroyed = go => { };
	}
}