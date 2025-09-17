using System;
using Mapbox.BaseModule.Unity;
using Mapbox.VectorModule.MeshGeneration.Unity;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.GameObjectModifiers
{
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Prefab Modifier")]
	public class PrefabModifierObject : ScriptableGameObjectModifierObject
	{
		public Action<GameObject> PrefabCreated = (s) => { };

		public PrefabModifierSettings Settings;
		private PrefabModifier _prefabModifierImplementation;
		protected override GameObjectModifier _gameObjectModifierImplementation => _prefabModifierImplementation;

		public override void ConstructModifier(UnityContext unityContext)
		{
			_prefabModifierImplementation = new PrefabModifier(unityContext, Settings);
			_prefabModifierImplementation.PrefabCreated += PrefabCreated;
		}
	}
}