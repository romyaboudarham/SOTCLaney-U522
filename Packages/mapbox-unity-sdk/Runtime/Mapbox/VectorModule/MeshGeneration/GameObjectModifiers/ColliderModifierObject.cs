using Mapbox.BaseModule.Unity;
using Mapbox.VectorModule.MeshGeneration.Unity;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.GameObjectModifiers
{
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Collider Modifier")]
	public class ColliderModifierObject : ScriptableGameObjectModifierObject
	{
		private ColliderModifier _prefabModifierImplementation;
		protected override GameObjectModifier _gameObjectModifierImplementation => _prefabModifierImplementation;

		public override void ConstructModifier(UnityContext unityContext)
		{
			_prefabModifierImplementation = new ColliderModifier();
		}
	}
}