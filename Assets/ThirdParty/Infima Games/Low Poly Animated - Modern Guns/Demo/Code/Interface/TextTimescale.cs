//Copyright 2022, Infima Games. All Rights Reserved.

using System;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns.Interface
{
    /// <summary>
    /// Component that changes a text to match the current time scale.
    /// </summary>
    public class TextTimescale : ElementText
    {
        #region METHODS

        /// <summary>
        /// Tick.
        /// </summary>
        protected override void Tick()
        {
            //Change text to match the time scale!
            textMesh.text = "Timescale : " + Math.Round(Time.timeScale, 2);
        }        

        #endregion
    }
}