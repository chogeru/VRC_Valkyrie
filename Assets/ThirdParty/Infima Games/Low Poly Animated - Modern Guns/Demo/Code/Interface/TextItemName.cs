//Copyright 2022, Infima Games. All Rights Reserved.

namespace InfimaGames.Animated.ModernGuns.Interface
{
    /// <summary>
    /// The TextItemName component changes a text component to say the name of the player’s current equipped item. Very helpful for our asset’s display purposes.
    /// </summary>
    public class TextItemName : ElementText
    {
        #region METHODS

        /// <summary>
        /// Tick.
        /// </summary>
        protected override void Tick()
        {
            //Update the text based on the equipped item's name.
            textMesh.text = $"Item Name: <b>{equippedWeaponBehaviour.GetItemName()}</b>";
        }

        #endregion
    }
}