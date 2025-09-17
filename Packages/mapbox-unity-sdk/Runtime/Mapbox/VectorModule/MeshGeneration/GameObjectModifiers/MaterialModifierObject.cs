using Mapbox.BaseModule.Unity;
using Mapbox.VectorModule.MeshGeneration.Unity;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.GameObjectModifiers
{
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Material Modifier")]
	public class MaterialModifierObject : ScriptableGameObjectModifierObject
	{
		public Material[] Materials;
		private MaterialModifier _materialModifierImplementation;
		protected override GameObjectModifier _gameObjectModifierImplementation => _materialModifierImplementation;
		
		public override void ConstructModifier(UnityContext unityContext)
		{
			_materialModifierImplementation = new MaterialModifier(Materials);
		}
	}
}