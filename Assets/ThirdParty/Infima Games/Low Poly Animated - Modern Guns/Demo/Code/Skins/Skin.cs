//Copyright 2022, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Skin. This ScriptableObject contains all the data necessary to define a SkinnedMesh's Skin.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_LPAMG_Skin", menuName = "Infima Games/Low Poly/Animated/Modern Guns Pack/Skin", order = 0)]
    public class Skin : ScriptableObject
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Skin Props")]
        
        [Tooltip("List of entries that define the Skin.")]
        [SerializeField, ReorderableList(Foldable = true)]
        private SkinProp[] skinProps;
        
        #endregion
        
        #region METHODS

        /// <summary>
        /// Tries to apply the Skin to the SkinnedMesh using the Link.
        /// </summary>
        public void Apply(string link, SkinnedMesh skinnedMesh)
        {
            //Check Reference.
            if (skinnedMesh == null)
                return;

            //Find Matching Skin Prop For Link.
            SkinProp skinProp = skinProps.FirstOrDefault(prop => prop.Item == link);

            //Get MaterialData Array For Link.
            MaterialData[] materialDatas = skinProp.MaterialDatas;
            //Check Reference.
            if (materialDatas == null || materialDatas.Length == 0)
                return;
            
            //Apply Skin To SkinnedMesh.
            skinnedMesh.Apply(materialDatas);
        }
        
        #endregion
    }
}