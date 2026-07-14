//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// The BipodData component holds the information needed for a weapon to know whether its bipod should be open or closed. The reason there is a script for this specifically is that bipods get destroyed when randomizing so they cannot keep track of this data themselves, thus this script is needed.
    /// </summary>
    public class BipodData : MonoBehaviour
    {
        #region FIELDS
        
        /// <summary>
        /// This value is true whenever the weapon’s bipod needs to be folded.
        /// </summary>
        private bool isFolded;

        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// Returns the value of isFolded.
        /// </summary>
        public bool IsFolded() => isFolded;
        
        #endregion
        
        #region METHODS
        
        /// <summary>
        /// Gets called whenever the weapon's attachments are randomized.
        /// </summary>
        public void OnRandomize()
        {
            //Toggle Fold.
            isFolded = !isFolded;
        }
        
        #endregion
    }
}