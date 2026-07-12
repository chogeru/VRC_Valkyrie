//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Helper StateMachineBehaviour that allows us to more easily play a specific character sound.
    /// </summary>
    public class PlaySoundCharacterBehaviour : StateMachineBehaviour
    {
        #region FIELDS SERIALIZED

        [Title(label: "Setup")]
        
        [Tooltip("Delay at which the audio is played.")]
        [SerializeField]
        private float delay;
        
        [Tooltip("Sound Type. Determines the type of sound we're trying to play.")]
        [SerializeField]
        private string type;
        
        [Title(label: "Audio Settings")]

        [Tooltip("Settings applied to the AudioClip played when the Animation State this component is on starts playing.")]
        [SerializeField]
        private AudioSettings audioSettings = new AudioSettings(1.0f, 0.0f, true);

        #endregion

        #region FIELDS

        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        /// <summary>
        /// Player Inventory.
        /// </summary>
        private InventoryBehaviour playerInventory;
        /// <summary>
        /// The service that handles sounds.
        /// </summary>
        private IAudioManagerService audioManagerService;

        /// <summary>
        /// Last AudioClip played through this component.
        /// </summary>
        private AudioClip lastClip;
        
        #endregion
        
        #region UNITY

        /// <summary>
        /// On State Enter.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //We need to get the character component.
            playerCharacter ??= ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
            //Check Reference.
            if (playerCharacter == null)
                return;
            
            //Get Inventory.
            playerInventory ??= playerCharacter.GetInventory();
            
            //Try to get the equipped weapon's Weapon component.
            if (!(playerInventory.GetEquipped() is {} weaponBehaviour))
                return;
            
            //Try grab a reference to the sound managing service.
            audioManagerService ??= ServiceLocator.Current.Get<IAudioManagerService>();
            
            //Get DataLinker.
            var dataLinker = weaponBehaviour.GetComponent<DataLinker>();
            //Check Reference.
            if (dataLinker == null)
                return;
            
            //Get SoundLinker.
            var soundLinker = dataLinker.Get<SoundLinker>("Sounds");
            //Check Reference.
            if (soundLinker == null)
                return;
            
            #region Select Correct Clip To Play

            //Get Correct Audio Clip.
            lastClip = soundLinker.Get(type);
            //Check Reference.
            if (lastClip == null)
                return;
            
            #endregion
            
            //Play with some delay. Granted, if the delay is set to zero, this will just straight-up play!
            audioManagerService.PlayOneShotDelayed(lastClip, audioSettings, delay);
        }

        /// <summary>
        /// OnStateExit.
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Base.
            base.OnStateExit(animator, stateInfo, layerIndex);

            //Check Reference.
            if (lastClip == null)
                return;
            
            //Try to find the last audio clip that we've played through one of these behaviours.
            GameObject found = GameObject.Find($"Audio Source -> {lastClip.name}");
            //Destroy that clip if there is one.
            if(found != null)
                Destroy(found);
        }

        #endregion
    }
}