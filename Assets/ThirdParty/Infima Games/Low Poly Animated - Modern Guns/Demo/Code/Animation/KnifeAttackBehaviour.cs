//Copyright 2022, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// KnifeAttackBehaviour. This StateMachineBehaviour is used in the Knife Attack state to change the knife mesh visibility.
    /// </summary>
    public class KnifeAttackBehaviour : StateMachineBehaviour
    {
        #region FIELDS

        /// <summary>
        /// This is a simple reference to the player character in the game.
        /// </summary>
        private CharacterBehaviour playerCharacter;

        /// <summary>
        /// The player character's equipped weapon.
        /// </summary>
        private WeaponBehaviour weaponBehaviour;
        /// <summary>
        /// The equipped weaponBehaviour's TimeLinker.
        /// </summary>
        private TimeLinker timeLinker;

        /// <summary>
        /// The character's knife GameObject.
        /// </summary>
        private GameObject knifeObject;
        
        #endregion
        
        #region UNITY
        
        /// <summary>
        /// OnStateEnter.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Base.
            base.OnStateEnter(animator, stateInfo, layerIndex);

            //Get GameModeService.
            var gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            //Check Reference.
            if (gameModeService == null)
                return;

            //Get CharacterBehaviour.
            playerCharacter ??= gameModeService.GetPlayerCharacter();
            //Check Reference.
            if (playerCharacter == null)
                return;

            //Get ObjectLinker.
            var objectLinker = playerCharacter.GetComponent<ObjectLinker>();
            //Check Reference.
            if (objectLinker == null)
                return;
            
            //Cache a reference to the character's knife GameObject if we don't already have it.
            knifeObject ??= objectLinker.Get("Knife");
            //Update Equipped Weapon Reference.
            weaponBehaviour = gameModeService.GetEquippedWeapon();
            
            //Check Reference.
            if (weaponBehaviour == null)
                return;

            //Grab DataLinker.
            var dataLinker = weaponBehaviour.GetComponent<DataLinker>();
            //Check Reference.
            if (dataLinker == null)
                return;
            
            //Cache TimeLinker.
            timeLinker = dataLinker.Get<TimeLinker>("Time Linker");
        }
        /// <summary>
        /// OnStateUpdate.
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Base.
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            //Check Reference.
            if (timeLinker == null)
                return;

            //Get all the TimeLinks related to the Knife Attack animation.
            TimeLink[] links = timeLinker.GetLinks("Knife Attack");
            
            //Get the delay required from the start of the Knife Attack animation to show the knife object.
            float showDelay = links.First(link => link.Type == $"Knife Attack/Show").Delay;
            //Get the delay required from the start of the Knife Attack animation to hide the knife object.
            float hideDelay = links.First(link => link.Type == $"Knife Attack/Hide").Delay;

            //Check if we need to enable the knife object. If we do, then we set it active.
            if(stateInfo.normalizedTime >= showDelay && stateInfo.normalizedTime < hideDelay)
                knifeObject.SetActive(true);
            //Check if we're done with the knife object, and can disable it. If so, disable it.
            else if(stateInfo.normalizedTime >= hideDelay)
                knifeObject.SetActive(false);
        }
        /// <summary>
        /// OnStateExit.
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            //Check Reference.
            if (knifeObject == null)
                return;
            
            //Disable.
            knifeObject.SetActive(false);
        }
        
        #endregion
    }
}