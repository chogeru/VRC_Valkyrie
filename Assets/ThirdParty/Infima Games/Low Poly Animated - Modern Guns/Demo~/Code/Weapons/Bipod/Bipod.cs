//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// The Bipod component takes care of updating a bipod attachment’s Animator component with the information needed for it to fold/unfold as requested.
    /// </summary>
    public class Bipod : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Tooltip("Reference to the bipod's Animator.")]
        [SerializeField]
        private Animator animator;
        
        #endregion
        
        #region UNITY

        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Get the player's equipped weapon reference.
            WeaponBehaviour weaponBehaviour = ServiceLocator.Current.Get<IGameModeService>().GetEquippedWeapon();
            //Check Reference.
            if (weaponBehaviour == null)
                return;
            
            //Get the bipod's current BipodData.
            var bipodData = weaponBehaviour.GetComponent<BipodData>();
            //Check Reference.
            if (bipodData == null)
                return;
            
            //Update the folding value!
            animator.SetBool("Folded", bipodData.IsFolded());
        }
        
        #endregion
    }
}