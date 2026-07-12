//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Grip Abstract Class.
    /// </summary>
    public abstract class GripBehaviour : MonoBehaviour
    {
        #region GETTERS
        
        /// <summary>
        /// Returns the Grip Index value.
        /// </summary>
        public abstract int GetIndex();
        
        #endregion
    }
}