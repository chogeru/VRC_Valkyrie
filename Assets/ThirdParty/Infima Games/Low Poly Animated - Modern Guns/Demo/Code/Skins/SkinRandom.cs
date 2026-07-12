//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// SkinRandom. This ScriptableObject helps a lot with storing multiple skins and getting a random one easily.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Skin_Random", menuName = "Infima Games/Low Poly/Animated/Modern Guns Pack/Skin Random", order = 0)]
    public class SkinRandom : ScriptableObject
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Skins")]
        
        [Tooltip("Skins that this ScriptableObject can hold.")]
        [ReorderableList(Foldable = true)]
        public Skin[] skins;
        
        #endregion
        
        #region FUNCTIONS

        /// <summary>
        /// Gets a random skin.
        /// </summary>
        public Skin Get()
        {
            //Return.
            return skins[Random.Range(0, skins.Length)];
        }
        
        #endregion
    }
}