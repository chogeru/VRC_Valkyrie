//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// ProceduralAnimator.
    /// </summary>
    public class ProceduralAnimator : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "References")]
        
        [Tooltip("The Character's Animator Component.")]
        [SerializeField]
        private Animator animator;

        [Tooltip("The Character's Weapon Bone Transform.")]
        [SerializeField]
        private Transform weaponBone;
        
        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Get Aiming Value.
            float aiming = animator.GetFloat(AHashes.AimingAlpha);

            //Get Equipped Weapon.
            WeaponBehaviour weaponBehaviour = ServiceLocator.Current.Get<IGameModeService>().GetEquippedWeapon();
            //Check Reference.
            if (weaponBehaviour == null)
                return;

            //Get Attachment Manager.
            var manager = weaponBehaviour.GetComponent<AttachmentBehaviour>();
            //Check Reference.
            if (manager == null)
                return;
            
            //Get Scope GameObject.
            GameObject scopeDefault = manager.GetVariant("Scope");
            //Check Reference.
            if (scopeDefault == null)
                return;
            
            //Location.
            Vector3 location = default;
            //Add Scope Offset While Aiming.
            location += Vector3.Lerp(default, scopeDefault.GetComponent<ScopeBehaviour>().GetOffset(), aiming);
            
            //Apply Location.
            weaponBone.localPosition = location;
        }
        
        #endregion
    }
}