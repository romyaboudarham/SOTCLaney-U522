using Mapbox.BaseModule.Unity;
using Mapbox.VectorModule.MeshGeneration.Unity;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.MeshModifiers.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Polygon Mesh Modifier")]
	public class PolygonMeshModifierObject : ScriptableMeshModifierObject, IPolygonMeshModifier
	{
		public float Height = 0f;
		
		private PolygonMeshModifier _polygonMeshModifierImplementation;
		protected override MeshModifier _meshModifierImplementation => _polygonMeshModifierImplementation;

		public override void ConstructModifier(UnityContext unityContext)
		{
			_polygonMeshModifierImplementation = new PolygonMeshModifier(Height);
		}
	}
}