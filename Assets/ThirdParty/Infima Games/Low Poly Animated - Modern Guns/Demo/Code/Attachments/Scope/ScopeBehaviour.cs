//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Exists with the sole purpose of acting as a base class for the Scope script. It defines what a Scope class needs and allows us to override its functions with whatever we want, in our case we do this in that aforementioned Scope class.
    /// </summary>
    public abstract class ScopeBehaviour : MonoBehaviour
    {
        #region FUNCTIONS
        
        /// <summary>
        /// Returns the offset this scope requires in order to be properly aimed.
        /// </summary>
        public abstract Vector3 GetOffset();
        /// <summary>
        /// If true, this scope default should be hidden when there is a scope equipped. Only makes sense for default scopes.
        /// </summary>
        public abstract bool HideWhenScopeEquipped();

        #endregion
    }
}