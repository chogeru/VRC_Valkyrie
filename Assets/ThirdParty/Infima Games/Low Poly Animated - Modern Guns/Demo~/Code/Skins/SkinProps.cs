//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// SkinProp. Contains information on all the materials needed for a specific prop on a SkinnedMesh.
    /// </summary>
    [System.Serializable]
    public struct SkinProp
    {
        #region PROPERTIES
        
        /// <summary>
        /// Item.
        /// </summary>
        public string Item => item;
        /// <summary>
        /// Material Datas.
        /// </summary>
        public MaterialData[] MaterialDatas => materialDatas;

        #endregion
        
        #region FIELDS SERIALIZED
        
        [Tooltip("Item Name.")]
        [SerializeField]
        private string item;

        [Tooltip("Array of material data for each link on this prop.")]
        [SerializeField]
        private MaterialData[] materialDatas;
        
        #endregion
    }
}