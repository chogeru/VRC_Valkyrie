//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Useful when attached to a weapon’s scope attachment so we can make sure that it is in fact a scope. It contains lots of variables that developers can edit to make sure each scope is different from the other! 
    /// </summary>
    public class Scope : ScopeBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Tooltip("Offset applied to the weapon bone when aiming through this scope.")]
        [SerializeField]
        private Vector3 offset;

        [Title(label: "Fade Setup")]

        [Tooltip("Setting this value to true will make sure that this Scope has a fade material applied over it when not being aimed through, effectively darkening it.")]
        [SerializeField]
        private bool fadeWhenNotAiming;

        [EnableIf("fadeWhenNotAiming", true)]
        [Tooltip("Material used to represent a Scope that the player cannot see through. This gets applied to the renderer when the player isn’t aiming through this Scope.")]
        [SerializeField]
        private Material fadeMaterial;

        [EnableIf("fadeWhenNotAiming", true)]
        [Tooltip("Index in the attachmentRenderer’s materials array of the Material that we want to replace for fadeMaterial when not aiming through this scope.")]
        [SerializeField]
        private int fadeIndex;
        
        [Title(label: "Hide When Scope Equipped")]

        [Tooltip("If this value is true we will try to hide the material at fadeIndex by using fadeMaterial when the character isn’t aiming. This is very useful for things like sniper scopes that need to be kept dark whilst not aimed through.")]
        [SerializeField]
        private bool hideWhenScopeEquipped;
        
        [Title(label: "References")]

        [Tooltip("Reference to the MeshRenderer component that renders this scope mesh! It’d be very weird for us to have a scope with no mesh, so this basically always exists.")]
        [SerializeField]
        private MeshRenderer attachmentRenderer;

        #endregion
        
        #region FIELDS

        /// <summary>
        /// Represents the Material that is normally at fadeIndex in the attachmentRenderer’s sharedMaterials array.
        /// </summary>
        private Material nonFadeMaterial;

        /// <summary>
        /// Reference to the game's IGameModeService service.
        /// </summary>
        private IGameModeService gameModeService;
        
        #endregion

        #region UNITY

        /// <summary>
        /// Start.
        /// </summary>
        private void Start()
        {
            //Grab IGameModeService.
            gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            
            //Cache the nonFadeMaterial.
            if(attachmentRenderer != null)
                nonFadeMaterial = attachmentRenderer.sharedMaterials[fadeIndex];
        }

        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Check Reference.
            if (attachmentRenderer == null)
                return;
            
            //Reference to the currently-equipped weapon.
            WeaponBehaviour weaponBehaviour = gameModeService.GetEquippedWeapon();
            //Grab CharacterBehaviour reference.
            CharacterBehaviour characterBehaviour = gameModeService.GetPlayerCharacter();

            //Checks if we need to hide this attachment whenever there is a Scope equipped too.
            if (hideWhenScopeEquipped && weaponBehaviour != null)
            {
                //Get Attachments component.
                var attachments = weaponBehaviour.GetComponent<Attachments>();
                //Check Reference.
                if (attachments != null)
                {
                    //Disable renderer based on whether there is a scope or not.
                    AttachmentSpawned attachmentSpawned = attachments.Get("Scope");
                    attachmentRenderer.enabled = attachmentSpawned.Index == 0;
                }
            }
            
            //This part handles whether there should be a fade material replacing a certain material when not aiming or not.
            if(fadeWhenNotAiming && fadeMaterial != null)
            {
                //Get Materials.
                Material[] sharedMaterials = attachmentRenderer.sharedMaterials;

                //Get ObjectLinker.
                var objectLinker = characterBehaviour.GetComponent<ObjectLinker>();
                //Check Reference.
                if (objectLinker == null)
                    return;
                
                //Get Aiming Value.
                float aiming = objectLinker.Get<Animator>("Animator").GetFloat(AHashes.AimingAlpha);
                //Update Material.
                sharedMaterials[fadeIndex] = aiming > 0 ? nonFadeMaterial : fadeMaterial;

                //Set Materials.
                attachmentRenderer.sharedMaterials = sharedMaterials;
            }
        }

        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// GetOffset.
        /// </summary>
        public override Vector3 GetOffset() => offset;
        /// <summary>
        /// HideScopeWhenEquipped.
        /// </summary>
        public override bool HideWhenScopeEquipped() => hideWhenScopeEquipped;

        #endregion
    }
}