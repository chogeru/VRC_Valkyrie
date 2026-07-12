//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Grip.
    /// </summary>
    public class Grip : GripBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Tooltip("Index value applied on the character’s Animator to determine which grip animation pose to apply. When adding new grips, new poses have to be added to the Animator, otherwise weird issues may happen. This is, of course, unless these new grips use the same index as the old ones.")]
        [SerializeField]
        private int gripIndex;

        #endregion
        
        #region FUNCTIONS

        /// <summary>
        /// GetIndex.
        /// </summary>
        public override int GetIndex() => gripIndex;

        #endregion
    }
}