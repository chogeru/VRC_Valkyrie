//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// FireBehaviour.
    /// </summary>
    public class FireBehaviour : StateMachineBehaviour
    {
        #region FIELDS

        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        /// <summary>
        /// Player Inventory.
        /// </summary>
        private InventoryBehaviour playerInventoryBehaviour;

        #endregion
        
        #region UNITY
        
        /// <summary>
        /// OnStateEnter.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            //We need to get the character component.
            playerCharacter ??= ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
            //Check Reference.
            if (playerCharacter == null)
                return;
            
            //Get Inventory.
            playerInventoryBehaviour ??= playerCharacter.GetInventory();

            //Get Equipped.
            WeaponBehaviour weaponBehaviour = playerInventoryBehaviour.GetEquipped();
            //Check Reference.
            if (weaponBehaviour == null)
                return;
            
            //Fire.
            weaponBehaviour.Fire();
        }
        
        #endregion
    }
}