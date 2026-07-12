//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// WeaponSkinner. This is the specific Skinner for Weapons. It basically by default handles all the specific attachments
    /// and applies Skins to them.
    /// </summary>
    public class WeaponSkinner : Skinner
    {
        #region FIELDS
        
        /// <summary>
        /// Inventory.
        /// </summary>
        private InventoryBehaviour playerInventory;

        /// <summary>
        /// Types of objects that this component applies to.
        /// </summary>
        private string[] types = new[]
        {
            "Grip", "Muzzle", "Forestock", "Scope", "Barrel", "Laser", "Magazine", "Magazine_Reserve", "Ironsight_B", "Ironsight_F", "Rails", "Bipod"
        };
        
        #endregion

        #region METHODS
        
        /// <summary>
        /// Apply.
        /// </summary>
        public override void Apply()
        {
            //Base.
            base.Apply();

            //Make sure that we have a skin.
            if (skin == null)
                return;
            
            //Get AttachmentBehaviour.
            var attachments = GetComponent<AttachmentBehaviour>();
            //Check Reference.
            if (attachments == null)
                return;
            
            //Loop.
            foreach (string attachment in types)
            {
                //Get Variant.
                GameObject variant = attachments.GetVariant(attachment);
                //Check Reference.
                if (variant == null)
                    continue;
                
                //Get SkinnedMesh.
                var skinnedMesh = variant.GetComponent<SkinnedMesh>();
                //Check Reference.
                if (skinnedMesh == null)
                    continue;
                
                //Apply.
                skin.Apply(attachment, skinnedMesh);
            }
        }
        
        #endregion
    }
}