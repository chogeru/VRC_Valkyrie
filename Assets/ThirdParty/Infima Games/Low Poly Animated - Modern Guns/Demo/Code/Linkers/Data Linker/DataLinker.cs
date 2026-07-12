//Copyright 2022, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// This component allows us to keep references to as many ScriptableObjects as we want. This is very
    /// useful for quickly getting data.
    /// </summary>
    public class DataLinker : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Links")]
        
        [Tooltip("Links to all the data assets.")]
        [SerializeField, ReorderableList(Foldable = true)]
        private DataLink[] dataLinks;
        
        #endregion
        
        #region FUNCTIONS

        /// <summary>
        /// This function returns a data asset with a specific type/name. 
        /// </summary>
        public T Get<T>(string type) where T : ScriptableObject => dataLinks.First(link => link.Type == type).Scriptable as T;
        
        #endregion
    }
}