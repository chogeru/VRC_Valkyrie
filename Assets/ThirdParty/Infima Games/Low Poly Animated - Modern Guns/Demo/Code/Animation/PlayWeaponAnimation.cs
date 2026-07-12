//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// PlayWeaponAnimation. Plays an animation on the character's equipped weapon when this state starts.
    /// </summary>
    public class PlayWeaponAnimation : StateMachineBehaviour
    {
        #region FIELDS SERIALIZED

        [Title(label: "Setup")]

        [Tooltip("Name of the AnimState to play.")]
        [SerializeField]
        private string stateName;

        [Tooltip("Layer the AnimState is played on.")]
        [SerializeField]
        private int layer = 0;

        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        /// <summary>
        /// Player Inventory.
        /// </summary>
        private InventoryBehaviour playerInventory;

        #endregion
        
        #region UNITY

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //We need to get the character component.
            playerCharacter ??= ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
            //Check Reference.
            if (playerCharacter == null)
                return;
            
            //Get Inventory.
            playerInventory ??= playerCharacter.GetInventory();

            //Get WeaponBehaviour.
            WeaponBehaviour weaponBehaviour = playerInventory.GetEquipped();
            //Check Reference.
            if (weaponBehaviour == null)
                return;
            
            //Get the weapon's Animator component.
            var weaponAnimator = weaponBehaviour.GetComponent<Animator>();
            //Check Reference.
            if (weaponAnimator == null)
                return;
            
            //Try to play the corresponding state.
            if(weaponAnimator.HasState(layer, Animator.StringToHash(stateName)))
                weaponAnimator.CrossFade(stateName, 0.0f, layer, 0.0f);
            //This right here is a little hacky, but it solves some issues with certain custom animation setups.
            if(weaponAnimator.HasState(layer + 1, Animator.StringToHash(stateName)))
                weaponAnimator.CrossFade(stateName, 0.0f, layer + 1, 0.0f);
        }

        #endregion
    }
}