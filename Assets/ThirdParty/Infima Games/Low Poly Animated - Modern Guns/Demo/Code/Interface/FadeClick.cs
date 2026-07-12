//Copyright 2022, Infima Games. All Rights Reserved.

using System.Collections;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns.Interface
{
    /// <summary>
    /// FadeClick.
    /// </summary>
    public class FadeClick : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "References")]
        
        [Tooltip("Animator Component.")]
        [SerializeField, NotNull]
        private Animator animator;

        [Title(label: "Settings")]

        [Tooltip("Delay after clicking to continue at which the game starts.")]
        [SerializeField]
        private float startDelay = 1.0f;
        
        #endregion

        #region FIELDS

        /// <summary>
        /// IGameStartService.
        /// </summary>
        private IGameStartService gameStartService;

        /// <summary>
        /// Fade Hash.
        /// </summary>
        private static readonly int fadeHash = Animator.StringToHash("Fade");

        #endregion
        
        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        private void Awake()
        {
            //Get Start Service.
            gameStartService = ServiceLocator.Current.Get<IGameStartService>();
        }

        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Check Reference.
            if (animator == null)
            {
                //ReferenceError.
                Log.ReferenceError(this, gameObject);

                //Return.
                return;
            }
            
            //Click.
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //Clicked to start.
                gameStartService.SetClickedToStart(true);
                
                //Trigger Fade.
                animator.SetTrigger(fadeHash);
                //Wait.
                StartCoroutine(nameof(Wait));
            }
        }
        
        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// Small delay to action inputs.
        /// </summary>
        private IEnumerator Wait()
        {
            //Yield.
            yield return new WaitForSeconds(startDelay);
            //Start Game.
            gameStartService.SetStarted(true);
        }
        
        #endregion
    }
}