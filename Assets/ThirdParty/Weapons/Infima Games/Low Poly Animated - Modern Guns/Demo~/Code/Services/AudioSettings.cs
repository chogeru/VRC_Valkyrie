//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;
using UnityEngine.Audio;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Audio Settings used to interact with the AudioManagerService.
    /// </summary>
    [System.Serializable]
    public struct AudioSettings
    {
        /// <summary>
        /// Automatic Cleanup Getter.
        /// </summary>
        public bool AutomaticCleanup => automaticCleanup;
        /// <summary>
        /// Volume Getter.
        /// </summary>
        public float Volume => volume;
        /// <summary>
        /// Spatial Blend Getter.
        /// </summary>
        public float SpatialBlend => spatialBlend;
        //Loop.
        public bool Loop => loop;

        /// <summary>
        /// Output.
        /// </summary>
        public AudioMixerGroup Output => output;

        [Header("Settings")]
        
        [Tooltip("Settings this value to true will make sure that whatever audio is played with these settings will continuously loop until manually stopped.")]
        [SerializeField]
        private bool loop;
        
        [Tooltip("If true, any AudioSource created will be removed after it has finished playing its clip.")]
        [SerializeField]
        private bool automaticCleanup;

        [Tooltip("Volume.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float volume;

        [Tooltip("Spatial Blend.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float spatialBlend;

        [Header("Mixer")]

        [Tooltip("AudioMixerGroup used for whatever AudioClip gets played with these settings. This one is important as it defines how everything in the game is mixed.")]
        [SerializeField]
        private AudioMixerGroup output;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AudioSettings(float volume = 1.0f, float spatialBlend = 0.0f, bool automaticCleanup = true, bool loop = false, AudioMixerGroup output = default)
        {
            //Volume.
            this.volume = volume;
            //Spatial Blend.
            this.spatialBlend = spatialBlend;
            //Automatic Cleanup.
            this.automaticCleanup = automaticCleanup;
            //Loop.
            this.loop = loop;
            //Output.
            this.output = output;
        }
    }
}