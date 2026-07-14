//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames
{
    /// <summary>
    /// Player Interface.
    /// </summary>
    public class CanvasSpawner : MonoBehaviour
    {
        #region FIELDS SERIALIZED

        [Title(label: "Settings")]
        
        [Tooltip("Canvas prefab spawned at start. Displays the player's user interface.")]
        [SerializeField]
        private GameObject canvasPrefab;
        
        [Tooltip("Quality settings menu prefab spawned at start. Used for switching between different quality settings in-game.")]
        [SerializeField]
        private GameObject qualitySettingsPrefab;

        #endregion

        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        private void Awake()
        {
            //Spawn Interface.
            if(canvasPrefab != null)
                Instantiate(canvasPrefab);
            //Spawn Quality Settings Menu.
            if(qualitySettingsPrefab != null)
                Instantiate(qualitySettingsPrefab);
        }

        #endregion
    }
}