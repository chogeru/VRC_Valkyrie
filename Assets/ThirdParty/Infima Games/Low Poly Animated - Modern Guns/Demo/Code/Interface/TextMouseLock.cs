//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns.Interface
{
    /// <summary>
    /// This component handles warning developers whether their mouse is locked or not by
    /// updating a text in the interface.
    /// </summary>
    public class TextMouseLock : ElementText
    {
        #region METHODS

        /// <summary>
        /// Tick.
        /// </summary>
        protected override void Tick()
        {
            //Update the text based on whether the cursor is locked or not.
            textMesh.text = "Cursor " + (Cursor.visible == false ? "Locked" : "Unlocked");
        }

        #endregion
    }
}