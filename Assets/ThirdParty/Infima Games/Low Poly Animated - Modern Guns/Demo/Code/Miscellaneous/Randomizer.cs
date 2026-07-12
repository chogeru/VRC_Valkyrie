//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Randomizer. This component helps us with randomizing the attachments and skins on the character/weapons.
    /// </summary>
    public class Randomizer : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "References")]
        
        [Tooltip("Inventory Reference.")]
        [SerializeField]
        private InventoryBehaviour inventoryBehaviour;

        [Tooltip("Character Skinner Reference. We use this one to randomize the character skin when pressing the correct input.")]
        [SerializeField]
        private CharacterSkinner characterSkinner;

        [Tooltip("Reference to the character's main Animator component.")]
        [SerializeField]
        private Animator animator;
        
        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Ignore any inputs while the cursor is visible. We do this because for us this means that there's some sort of menu open.
            if (Cursor.visible)
                return;

            //Get Equipped.
            WeaponBehaviour equipped = inventoryBehaviour.GetEquipped();
            //Check Reference.
            if (equipped == null)
                return;
        
            //Randomize Attachments.
            if (Input.GetKeyDown(KeyCode.J))
            {
                //Play Animation.
                Play();
                //Randomize Attachments.
                equipped.GetAttachments().Randomize();
                
                //Allows us to let every behaviour know that we're randomizing things.
                equipped.SendMessage("OnRandomize", SendMessageOptions.DontRequireReceiver);
            }
            
            //Randomize Skins.
            if (Input.GetKeyDown(KeyCode.K))
            {
                //Play Animation.
                Play();

                //Get WeaponSkinner.
                var skinner = equipped.GetComponent<WeaponSkinner>();
                //Check Reference.
                if (skinner != null)
                {
                    //Randomize.
                    skinner.Randomize();
                    //Apply.
                    skinner.Apply();
                }
                
                //Randomize.
                characterSkinner.Randomize();
                //Apply.
                characterSkinner.Apply();
            }
        }
        
        #endregion

        #region METHODS
        
        /// <summary>
        /// Plays the randomizing animation.
        /// </summary>
        private void Play()
        {
            //Crossfade Randomize Animation.
            animator.CrossFade("Randomize", 0.0f, animator.GetLayerIndex("Randomization"), 0.0f);
        }
        
        #endregion
    }
}