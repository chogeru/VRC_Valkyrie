//Copyright 2022, Infima Games. All Rights Reserved.

using System.Collections;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Weapon. This class handles most of the things that weapons need.
    /// </summary>
    public class Weapon : WeaponBehaviour
    {
        #region FIELDS SERIALIZED

        [Title(label: "General")]

        [Tooltip("The name of this item. Mostly just for display purposes.")]
        [SerializeField]
        private string itemName = "SP60";
        
        [Title(label: "Firing")]

        [Tooltip("Is this weapon automatic? If yes, then holding down the firing button will continuously fire.")]
        [SerializeField] 
        private bool automatic;

        [Tooltip("Amount of shots this weapon can shoot in a minute. It determines how fast the weapon shoots.")]
        [SerializeField] 
        private int roundsPerMinutes = 200;

        [Title(label: "Reloading")]
        
        [Tooltip("Determines if this weapon reloads in cycles, meaning that it inserts one bullet at a time, or not.")]
        [SerializeField]
        private bool cycledReload;

        [Tooltip("Determines if this weapon can be reloaded while being aimed.")]
        [SerializeField]
        private bool canReloadAimed = true;

        [Title(label: "Casings")]

        [Tooltip("Determines the amount of time that has to pass right after firing for a casing prefab to get ejected from the weapon!")]
        [SerializeField]
        private float casingDelay;

        [Title(label: "Resources")]
        
        [Tooltip("The AnimatorController a player character needs to use while wielding this weapon.")]
        [SerializeField] 
        public RuntimeAnimatorController controller;

        [Tooltip("Prefab spawned when firing this weapon as a casing.")]
        [SerializeField]
        private GameObject casingPrefab;

        #endregion

        #region FIELDS

        /// <summary>
        /// Weapon Animator.
        /// </summary>
        private Animator animator;
        /// <summary>
        /// Attachment Manager.
        /// </summary>
        private AttachmentBehaviour attachment;

        /// <summary>
        /// The player character's camera.
        /// </summary>
        private Transform playerCamera;
        
        #endregion

        #region UNITY
        
        /// <summary>
        /// Awake.
        /// </summary>
        protected override void Awake()
        {
            //Get Animator.
            animator = GetComponent<Animator>();
            //Get Attachment Manager.
            attachment = GetComponent<AttachmentBehaviour>();
        }

        #endregion

        #region GETTERS

        /// <summary>
        /// GetItemName.
        /// </summary>
        public override string GetItemName() => itemName;
        
        /// <summary>
        /// HasCycledReload.
        /// </summary>
        public override bool HasCycledReload() => cycledReload;
        /// <summary>
        /// IsAutomatic.
        /// </summary>
        public override bool IsAutomatic() => automatic;
        /// <summary>
        /// GetRateOfFire.
        /// </summary>
        public override float GetRateOfFire() => roundsPerMinutes;
        /// <summary>
        /// CanReloadAimed.
        /// </summary>
        public override bool CanReloadAimed() => canReloadAimed;

        /// <summary>
        /// GetAnimatorController.
        /// </summary>
        public override RuntimeAnimatorController GetAnimatorController() => controller;
        /// <summary>
        /// GetAttachmentManager.
        /// </summary>
        public override AttachmentBehaviour GetAttachments() => attachment;

        #endregion

        #region METHODS

        /// <summary>
        /// Waits for the necessary casingDelay before spawning a casing to eject from the weapon. This is very helpful to showcase weapons like a sniper rifle, which might have a bolt pull after firing that is when the casing gets ejected.
        /// </summary>
        private IEnumerator WaitForCasing()
        {
            //Yield.
            yield return new WaitForSeconds(casingDelay);
            
            //Eject.
            EjectCasing();
        }

        /// <summary>
        /// Reload.
        /// </summary>
        public override void Reload()
        {
            //Set Reloading Bool. This helps cycled reloads know when they need to stop cycling.
            const string boolName = "Reloading";
            animator.SetBool(boolName, true);
        }

        /// <summary>
        /// EjectCasing.
        /// </summary>
        public override void EjectCasing()
        {
            //Get Sockets.
            var sockets = GetComponent<Sockets>();
            //Check Reference.
            if (sockets == null)
                return;
            
            //Get Eject Socket.
            Transform ejectSocket = sockets.GetSocketTransform("SOCKET_Eject");
            //Check Reference.
            if(ejectSocket == null)
                return;

            //Instantiate.
            Instantiate(casingPrefab, ejectSocket.position, ejectSocket.rotation);
        }

        /// <summary>
        /// Fire.
        /// </summary>
        public override void Fire()
        {
            //Get Muzzle.
            var muzzleBehaviour = attachment.GetVariant<MuzzleBehaviour>("Muzzle");
            //Check Reference.
            if (muzzleBehaviour == null)
                return;
            
            //Fire Muzzle.
            muzzleBehaviour.Fire();
            
            //Make sure we're not still waiting for some other casing to spawn.
            StopCoroutine(nameof(WaitForCasing));

            //Spawn Casing.
            StartCoroutine(nameof(WaitForCasing));
        }

        #endregion
    }
}