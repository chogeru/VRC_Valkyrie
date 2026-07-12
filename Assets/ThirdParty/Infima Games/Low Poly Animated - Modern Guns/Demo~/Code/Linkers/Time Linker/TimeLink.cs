//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// ObjectLink. Data type that defines a delay for a type of entry that we will want to reference at some point during our game.
    /// </summary>
    [System.Serializable]
    public struct TimeLink
    {
        #region PROPERTIES

        /// <summary>
        /// Type.
        /// </summary>
        public string Type => type;
        /// <summary>
        /// Delay.
        /// </summary>
        public float Delay => delay;
        
        #endregion
        
        #region FIELDS SERIALIZED
        
        [Title(label: "Type")]
        
        [Tooltip("Type of delay. Basically acts like a name.")]
        [SerializeField]
        private string type;

        [Title(label: "Delay")]
        
        [Tooltip("Amount of time to delay for when calling on this specific type of TimeLink.")]
        [SerializeField]
        private float delay;
        
        #endregion
    }
}