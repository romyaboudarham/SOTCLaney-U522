using Mapbox.BaseModule.Unity;
using Mapbox.VectorModule.MeshGeneration.Unity;
using UnityEngine;

namespace Mapbox.VectorModule.MeshGeneration.MeshModifiers.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mapbox/Modifiers/Chamfer Height Modifier")]
    public class ChamferHeightModifierObject : ScriptableMeshModifierObject
    {
        public ChamferModifierSettings ExtrusionOptions;
        private ChamferHeightModifier _heightModifierImplementation;
        protected override MeshModifier _meshModifierImplementation => _heightModifierImplementation;

        public override void ConstructModifier(UnityContext unityContext)
        {
            _heightModifierImplementation = new ChamferHeightModifier(ExtrusionOptions);
        }
    }
}