//Copyright 2022, Infima Games. All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// ReloadBehaviour. This StateMachineBehaviour is used to make a weapon's magazines appear and disappear
    /// at appropriate times.
    /// </summary>
    public class ReloadBehaviour : StateMachineBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Tooltip("The type of reload we're apply this behaviour to. We have two types, the Reload type, and the Reload Empty type.")]
        [SerializeField]
        private string reloadType = "Reload";

        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// TimeLink Array. Contains all TimeLink values used to show/hide magazines.
        /// </summary>
        private List<TimeLink> timeLinks;

        /// <summary>
        /// Magazine GameObject.
        /// </summary>
        private GameObject magazine;
        /// <summary>
        /// Magazine Reserve GameObject.
        /// </summary>
        private GameObject magazineReserve;

        #endregion
        
        #region METHODS
        
        /// <summary>
        /// Gets all required references.
        /// </summary>
        private void GetReferences()
        {
            //Get Equipped Weapon.
            WeaponBehaviour weaponBehaviour = ServiceLocator.Current.Get<IGameModeService>().GetEquippedWeapon();
            //Check Reference.
            if (weaponBehaviour == null)
                return;

            //Get DataLinker.
            var dataLinker = weaponBehaviour.GetComponent<DataLinker>();
            //Check Reference.
            if (dataLinker == null)
                return;

            //Get TimeLinker.
            var time = dataLinker.Get<TimeLinker>("Time Linker");
            //Check Reference.
            if (time == null)
                return;
            
            //Get All Reload Time Links.
            timeLinks = time.GetLinks(reloadType).ToList();

            //Get AttachmentManager.
            var manager = weaponBehaviour.GetComponent<AttachmentBehaviour>();
            //Check Reference.
            if (manager == null)
                return;
            
            //Get Magazine.
            magazine = manager.GetVariant("Magazine");
            //Get Reserve Magazine.
            magazineReserve = manager.GetVariant("Magazine_Reserve");
        }
        
        #endregion
        
        #region UNITY
        
        /// <summary>
        /// OnStateEnter.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Base.
            base.OnStateEnter(animator, stateInfo, layerIndex);
            //Get References.
            GetReferences();
            
            //Enable Magazine.
            Show(magazine);
            
            //Enable Magazine Reserve.
            Show(magazineReserve);
        }

        /// <summary>
        /// OnStateUpdate.
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Base.
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            //Check References.
            if (timeLinks == null || timeLinks.Count == 0)
                return;
            
            //Get References.
            GetReferences();

            //Show Magazine At This Delay.
            UpdateMagazine(magazine, $"{reloadType}/Show/Magazine", false, stateInfo);
            
            //Show Magazine Reserve At This Delay.
            UpdateMagazine(magazineReserve, $"{reloadType}/Show/Magazine_Reserve", false, stateInfo);

            //Hide Reserve Magazine.
            UpdateMagazine(magazineReserve, $"{reloadType}/Hide/Magazine_Reserve", true, stateInfo);

            //Hide Magazine At This Delay.
            UpdateMagazine(magazine, $"{reloadType}/Hide/Magazine", true, stateInfo);
        }

        /// <summary>
        /// OnStateExit.
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Get References.
            GetReferences();
            
            //Determine.
            DetermineEndVisibility(magazine);
            
            //Determine.
            DetermineEndVisibility(magazineReserve, false);
        }
        
        #endregion
        
        #region METHODS

        /// <summary>
        /// Updates the visibility of a specific magazine object if it needs to based on the current play
        /// state of the animation.
        /// </summary>
        private void UpdateMagazine(GameObject magazineObject, string typeSearch, bool hide, AnimatorStateInfo stateInfo)
        {
            //Check Reference.
            if (magazineObject == null)
                return;
            
            //Link Search.
            bool Predicate(TimeLink link) => link.Type == typeSearch;

            //Return if we cannot find such an item.
            if (!timeLinks.Exists(Predicate))
                return;
            
            //Returns the current necessary time link.
            TimeLink timeLink = timeLinks.First(Predicate);
            //We check whether we actually need to change anything based on this TimeLink.
            if(Mathf.Abs(stateInfo.normalizedTime - timeLink.Delay) > 0.005f)
                return;
            
            //We update the object.
            if(hide)
                Hide(magazineObject);
            else
                Show(magazineObject);
        }

        /// <summary>
        /// Makes a specific magazine object visible and takes care of raising certain events if needed.
        /// </summary>
        private static void Show(GameObject magazineObject)
        {
            //Check Reference.
            if (magazineObject == null)
                return;
            
            //Activate.
            magazineObject.SetActive(true);
            
            //We do this so we can run functions when the magazine gets shown.
            var magazineManager = magazineObject.GetComponent<IMagazine>();
            if(magazineManager != null)
                magazineObject.GetComponent<IMagazine>().Shown();
        }

        /// <summary>
        /// Makes a specific magazine object invisible and takes care of raising certain events if needed.
        /// </summary>
        private static void Hide(GameObject magazineObject)
        {
            //Check Reference.
            if (magazineObject == null)
                return;
            
            //We do this so we can run functions when the magazine gets hidden.
            var magazineManager = magazineObject.GetComponent<IMagazine>();
            if(magazineManager != null)
                magazineObject.GetComponent<IMagazine>().Hidden();
            
            //De-activate.
            magazineObject.SetActive(false);
        }

        /// <summary>
        /// Determines, and updates, the visibility of a magazine object at the end of this animation state.
        /// </summary>
        private static void DetermineEndVisibility(GameObject magazineObject, bool defaultVisibility = true)
        {
            //Check Reference.
            if (magazineObject == null)
                return;
            
            //This little variable determines whether we keep the magazine visible after this state finishes (now).
            bool keepVisible = defaultVisibility;
            
            //We do this so we can change this based on the magazine, which we only do for specific weapons.
            var magazineManager = magazineObject.GetComponent<IMagazine>();
            if (magazineManager != null)
                keepVisible = magazineManager.KeepVisibleAtEndOfReload();
            
            //Hide/Show.
            if(keepVisible)
                Show(magazineObject);
            else
                Hide(magazineObject);
        }
        
        #endregion
    }
}