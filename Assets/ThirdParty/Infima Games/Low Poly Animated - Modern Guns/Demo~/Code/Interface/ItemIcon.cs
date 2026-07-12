//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace InfimaGames.Animated.ModernGuns.Interface
{
    /// <summary>
    /// ItemIcon. This component updates all of its pieces (usually children) to match the sprites set up in an item's icons data asset.
    /// </summary>
    public class ItemIcon : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Body Image")]
        
        [Tooltip("All pieces to update based on an IconLinker data asset.")]
        [SerializeField, ReorderableList(Foldable = true)]
        private Link<Image>[] pieces;
        
        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// IconLinker.
        /// </summary>
        private IconLinker iconLinker;

        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Grab WeaponBehaviour.
            WeaponBehaviour weaponBehaviour = ServiceLocator.Current.Get<IGameModeService>().GetEquippedWeapon();

            //Check Reference.
            if (weaponBehaviour == null)
                return;
            
            //Get AttachmentBehaviour Component.
            var attachments = weaponBehaviour.GetComponent<AttachmentBehaviour>();
            //Get IconLinker.
            iconLinker = weaponBehaviour.GetComponent<DataLinker>().Get<IconLinker>("Icons Linker");
            
            //Get Forestock Attachment.
            AttachmentSpawned forestockAttachment = attachments.Get(AttachmentConstants.Forestock);
            //Check Reference.
            //if (forestockAttachment.GameObject == null)
            //{
            //    //Return.
            //    return;
            //}
            
            //Loop Pieces.
            foreach (Link<Image> piece in pieces)
            {
                //Get Image Component.
                Image image = piece.Get();
                //Get Attachment With Piece Type.
                AttachmentSpawned attachment = attachments.Get(piece.Type);

                //Icon Index.
                int iconIndex = attachment.Index;

                //This code here is a bit of an edge case where we want the muzzle icon to change based on the forestock we have.
                if (piece.Type == "Muzzle" && attachment.Index != 0)
                    iconIndex = forestockAttachment.Index + 1;
                //Special edge case for the front sight, which also needs to change based on whatever forestock there is.
                else if (piece.Type == "Ironsight_F")
                    iconIndex = forestockAttachment.Index;
                
                //Check Reference.
                if (attachment.GameObject != null)
                {
                    //Try to grab a ScopeBehaviour from this attachment to check whether it is some sort of scope or not.
                    var scopeBehaviour = attachment.GameObject.GetComponent<ScopeBehaviour>();
                    //Check Reference.
                    if (scopeBehaviour != null)
                    {
                        //We do this right here to check whether we need to hide it right now or not.
                        if (scopeBehaviour.HideWhenScopeEquipped() && attachments.Get("Scope").Index != 0)
                        {
                            //Hide it.
                            image.enabled = false;
                            //Continue.
                            continue;
                        }
                    }   
                }

                //Get Sprite.
                Sprite sprite = iconLinker.Get(piece.Type, iconIndex);
                //Check Reference.
                if (sprite == null)
                {
                    //Disable Piece.
                    image.enabled = false;
                    
                    //Continue.
                    continue;
                }

                //Enable Image.
                image.enabled = true;
                //Assign Sprite.
                image.sprite = sprite;
            }
        }
        
        #endregion
    }
}