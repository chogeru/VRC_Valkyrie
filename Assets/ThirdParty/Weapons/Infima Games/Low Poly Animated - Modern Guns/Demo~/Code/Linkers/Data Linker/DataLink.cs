//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// DataLink. Defines what a data link really is. This is just a simple data type that contains some sort of name, and a reference to a ScriptableObject.
    /// </summary>
    [System.Serializable]
    public struct DataLink
    {
        #region PROPERTIES
        
        /// <summary>
        /// Type.
        /// </summary>
        public string Type => type;

        /// <summary>
        /// Scriptable.
        /// </summary>
        public ScriptableObject Scriptable => scriptable;
        
        #endregion
        
        #region FIELDS SERIALIZED
        
        [Tooltip("Type of the Data Asset being linked.")]
        [SerializeField]
        private string type;
        
        [Tooltip("Reference to the Data Asset.")]
        [SerializeField]
        private ScriptableObject scriptable;
        
        #endregion
    }
}