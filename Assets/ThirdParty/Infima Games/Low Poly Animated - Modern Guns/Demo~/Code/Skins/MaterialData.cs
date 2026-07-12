//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// MaterialData. Maps a Material to a specific name for a Material Slot. These names are usually matched with a SkinnedMesh Component's list of names.
    /// </summary>
    [System.Serializable]
    public struct MaterialData
    {
        #region PROPERTIES
        
        /// <summary>
        /// Link.
        /// </summary>
        public string Link => link;
        /// <summary>
        /// Material.
        /// </summary>
        public Material Material => material;
      
        #endregion
        
        #region FIELDS SERIALIZED
        
        [Title(label: "Link")]
        
        [Tooltip("This link represents what material slot to apply this Material to. Basically needs to match the MaterialSlot on the SkinnedMesh that you want to change.")]
        [SerializeField]
        public string link;

        [Title(label: "Material")]
        
        [Tooltip("Material Reference.")]
        [SerializeField]
        public Material material;
        
        #endregion
    }
}