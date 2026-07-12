//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Play Sound Behaviour. Plays an AudioClip using our custom AudioManager!
    /// </summary>
    public class PlaySoundBehaviour : StateMachineBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Header("Setup")]
        
        [Tooltip("Array of AudioClip values to pick from when trying to play a sound through this component.")]
        [SerializeField]
        private AudioClip[] clips;
        
        [Header("Settings")]

        [Tooltip("Settings applied to the AudioClip played when the Animation State this component is on starts playing.")]
        [SerializeField]
        private AudioSettings settings = new AudioSettings(1.0f, 0.0f, true);
        
        #endregion
        
        #region FIELDS

        /// <summary>
        /// Audio Manager Service. Handles all game audio.
        /// </summary>
        private IAudioManagerService audioManagerService;

        /// <summary>
        /// Last clip played through this component. This is very important so we can delete that last GameObject we created when exiting the state.
        /// </summary>
        private AudioClip lastClip;

        #endregion

        #region UNITY

        /// <summary>
        /// OnStateEnter.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Randomize Clip To Play.
            lastClip = clips[Random.Range(0, clips.Length)];
            
            //Try grab a reference to the sound managing service.
            audioManagerService ??= ServiceLocator.Current.Get<IAudioManagerService>();
            //Play!
            audioManagerService?.PlayOneShot(lastClip, settings);
        }
        
        /// <summary>
        /// OnStateExit.
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Base.
            base.OnStateExit(animator, stateInfo, layerIndex);
            
            //Find the last Audio Source we spawned through the manager.
            GameObject found = GameObject.Find($"Audio Source -> {lastClip.name}");
            //If there is one, destroy it.
            if(found != null)
                Destroy(found);
        }

        #endregion
    }
}