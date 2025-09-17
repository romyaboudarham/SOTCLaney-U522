using Mapbox.BaseModule.Unity;
using Mapbox.VectorModule.MeshGeneration.Unity;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.MeshModifiers.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Line Modifier")]
	public class LineMeshModifierObject : ScriptableMeshModifierObject
	{
		public LineMeshParameters LineMeshParameters;
		private LineMeshForPolygonsModifier _lineModifierImplementation;
		protected override MeshModifier _meshModifierImplementation => _lineModifierImplementation;

		public override void ConstructModifier(UnityContext unityContext)
		{
			_lineModifierImplementation = new LineMeshForPolygonsModifier(LineMeshParameters);
		}
	}
}