//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;
using UnityEngine.Serialization;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Socket. Represents a mesh socket, basically maps a name to a Transform.
    /// </summary>
    [System.Serializable]
    public struct Socket
    {
        #region PROPERTIES

        /// <summary>
        /// Type.
        /// </summary>
        public string Type => type;
        /// <summary>
        /// Transform.
        /// </summary>
        public Transform Transform => transform;
        
        #endregion
        
        #region FIELDS SERIALIZED
        
        [Title(label: "Type")]
        
        [Tooltip("Type of the socket. Mostly acts like a name.")]
        [SerializeField]
        private string type;

        [Title(label: "Transform")]
        
        [Tooltip("Transform of the Socket.")]
        [SerializeField]
        private Transform transform;
        
        #endregion
    }
}