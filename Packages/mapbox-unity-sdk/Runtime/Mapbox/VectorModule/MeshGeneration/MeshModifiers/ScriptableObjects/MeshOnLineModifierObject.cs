using Mapbox.VectorModule.MeshGeneration.Unity;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.MeshModifiers.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Mesh On Line Modifier")]
	public class MeshOnLineModifierObject : ScriptableMeshModifierObject
	{
		[SerializeField]
		private MeshOnLineModifier _meshOnLineModifierImplementation;
		protected override MeshModifier _meshModifierImplementation => _meshOnLineModifierImplementation;
	}
}