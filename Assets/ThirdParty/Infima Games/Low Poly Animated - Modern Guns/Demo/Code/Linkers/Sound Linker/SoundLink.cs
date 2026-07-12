//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Sound. Defines what a sound is. These can be accessed quickly via the SoundLinker.
    /// </summary>
    [System.Serializable]
    public struct SoundLink
    {
        #region PROPERTIES

        /// <summary>
        /// Type.
        /// </summary>
        public string Type => type;
        /// <summary>
        /// Clips.
        /// </summary>
        public AudioClip[] Clips => clips;
        
        #endregion
        
        #region FIELDS SERIALIZED
        
        [Title(label: "Type")]

        [Tooltip("Type of sound. This acts functionally as a name.")]
        [SerializeField]
        private string type;

        [Title(label: "Clips")]

        [Tooltip("Clips that can be played for this type.")]
        [SerializeField]
        private AudioClip[] clips;
        
        #endregion
    }
}