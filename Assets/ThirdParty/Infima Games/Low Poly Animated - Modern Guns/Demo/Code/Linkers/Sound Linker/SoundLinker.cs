//Copyright 2022, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// This ScriptableObject holds references to all the sounds for a weapon in the asset.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_LPAMG_Sounds", menuName = "Infima Games/Low Poly/Animated/Modern Guns Pack/Sounds", order = 0)]
    public class SoundLinker : ScriptableObject
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Sound Values")]
        
        [Tooltip("Sound Values.")]
        [SerializeField, ReorderableList(Foldable = true)]
        private SoundLink[] soundValues;

        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// Get. Returns the AudioClip linked to the given type.
        /// </summary>
        public AudioClip Get(string type)
        {
            //Return.
            return (from t in soundValues where t.Type == type select t.Clips[Random.Range(0, t.Clips.Length)]).FirstOrDefault();
        }
        
        #endregion
    }
}