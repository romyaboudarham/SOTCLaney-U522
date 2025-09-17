using Mapbox.BaseModule.Unity;
using Mapbox.VectorModule.MeshGeneration.Unity;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.MeshModifiers.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Height Modifier")]
	public class HeightModifierObject : ScriptableMeshModifierObject
	{
		public GeometryExtrusionOptions ExtrusionOptions;
		private HeightModifier _heightModifierImplementation;
		protected override MeshModifier _meshModifierImplementation => _heightModifierImplementation;

		public override void ConstructModifier(UnityContext unityContext)
		{
			_heightModifierImplementation = new HeightModifier(ExtrusionOptions);
		}
	}
}