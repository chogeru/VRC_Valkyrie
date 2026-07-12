//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Muzzle Abstract Class.
    /// </summary>
    public abstract class MuzzleBehaviour : MonoBehaviour
    {
        #region METHODS
        
        /// <summary>
        /// Fire. Called when we want the weapon to fire.
        /// </summary>
        public abstract void Fire();
        
        #endregion
    }
}