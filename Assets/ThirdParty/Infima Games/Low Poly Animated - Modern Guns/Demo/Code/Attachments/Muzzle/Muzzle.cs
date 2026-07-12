//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Muzzle.
    /// </summary>
    public class Muzzle : MuzzleBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Settings")]
        
        [Tooltip("Socket at the tip of the Muzzle. Commonly used as a firing point.")]
        [SerializeField]
        private Transform socket;
        
        [Title(label: "Particles")]
        
        [Tooltip("Firing Particles.")]
        [SerializeField]
        private GameObject prefabFlashParticles;

        [Tooltip("Number of particles to emit when firing.")]
        [SerializeField]
        private int flashParticlesCount = 5;
        
        [Title(label: "Audio")]
        
        [Tooltip("Audio clip played when firing through this muzzle.")]
        [SerializeField, ReorderableList]
        private AudioClip[] audioClipsFire;

        [Tooltip("Audio Settings used when playing the firing Audio Clip.")]
        [SerializeField]
        private AudioSettings audioSettingsFire;
        
        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// Instantiated Particle System.
        /// </summary>
        private ParticleSystem particles;

        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Awake.
        /// </summary>
        private void Awake()
        {
            //Null Check.
            if(prefabFlashParticles != null)
            {
                //Instantiate Particles.
                GameObject spawnedParticlesPrefab = Instantiate(prefabFlashParticles, socket);
                //Reset the position.
                spawnedParticlesPrefab.transform.localPosition = default;
                //Reset the rotation.
                spawnedParticlesPrefab.transform.localEulerAngles = default;
                
                //Get Reference.
                particles = spawnedParticlesPrefab.GetComponent<ParticleSystem>();
            }
        }

        #endregion
        
        #region METHODS
        
        /// <summary>
        /// Fire.
        /// </summary>
        public override void Fire()
        {
            //Try to play the fire particles from the muzzle!
            if(particles != null)
                particles.Emit(flashParticlesCount);
            
            //Play One Shot.
            ServiceLocator.Current.Get<IAudioManagerService>().PlayOneShot(audioClipsFire, audioSettingsFire);
        }
        
        #endregion
    }
}