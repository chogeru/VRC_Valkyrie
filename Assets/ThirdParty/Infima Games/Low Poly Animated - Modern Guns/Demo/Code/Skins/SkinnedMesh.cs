//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;
using System.Linq;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// SkinnedMesh. Represents a mesh that can have a Skin ScriptableObject applied to it. This is needed to designate what the name of each material slot is.
    /// </summary>
    public class SkinnedMesh : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title("Materials")]
        
        [Tooltip("Links each Material on this SkinnedMesh with a Name.")]
        [SerializeField, ReorderableList]
        private string[] links;

        [Tooltip("Renderer that this SkinnedMesh applies to.")]
        [SerializeField]
        private Renderer meshRenderer;

        #endregion
        
        #region METHODS
        
        /// <summary>
        /// Applies the given MaterialData array to this SkinnedMesh.
        /// </summary>
        public void Apply(MaterialData[] materialDatas)
        {
            //Check Reference.
            if (materialDatas == null || materialDatas.Length == 0)
                return;
            
            //Get Shared Materials.
            var materials = meshRenderer.sharedMaterials;
            
            //Loop through the links.
            for (var i = 0; i < links.Length; i++)
            {
                //We should stop this loop if we've passed the material amount, there's likely some issue.
                if (materials.Length == i)
                    break;

                //Find MaterialData 
                Material material = materialDatas.FirstOrDefault(x => x.Link == links[i]).Material;
                //Set Material.
                if(material != null)
                    materials[i] = material;
            }

            //Set SharedMaterials.
            meshRenderer.sharedMaterials = materials;
        }
        
        #endregion
    }
}