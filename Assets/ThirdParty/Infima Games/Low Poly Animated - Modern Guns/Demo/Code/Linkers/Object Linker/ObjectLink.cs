//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// ObjectLink. Data type that defines a link to some sort of object, either in a scene or directly an asset.
    /// </summary>
    [System.Serializable]
    public struct ObjectLink
    {
        #region PROPERTIES

        /// <summary>
        /// Type.
        /// </summary>
        public string Type => type;
        /// <summary>
        /// GameObject.
        /// </summary>
        public GameObject GameObject => gameObject;
        
        #endregion
        
        #region FIELDS SERIALIZED
        
        [Tooltip("Type of the reference.")]
        [SerializeField]
        public string type;
        
        [Tooltip("Referenced Game Object.")]
        [SerializeField]
        public GameObject gameObject;
        
        #endregion
    }
}