//Copyright 2022, Infima Games. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// InitialUnholster. This component takes care of playing an unholster animation on the player character whenever the game starts. It waits a little bit before doing so, this way we can get our fade animation to play properly.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class InitialUnholster : MonoBehaviour
    {
        #region FIELDS SERIALIZED

        [Tooltip("Delay after the player has clicked to start at which the play the unholster.")]
        [SerializeField]
        private float delay = 0.5f;

        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// IGameStartService.
        /// </summary>
        private IGameStartService gameStartService;

        /// <summary>
        /// If true, that means we've finished with using this component.
        /// </summary>
        private bool isDone;
        
        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Awake.
        /// </summary>
        private void Awake()
        {
            //Cache IGameStartService.
            gameStartService = ServiceLocator.Current.Get<IGameStartService>();
        }

        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Check if the game has started.
            if (gameStartService.HasClickedToStart() && !isDone)
                StartCoroutine(nameof(Wait));
        }

        #endregion
        
        #region FUNCTIONS

        /// <summary>
        /// Waits for a few seconds before playing the unholster animation on the character’s Animator component.
        /// </summary>
        private IEnumerator Wait()
        {
            //Yield.
            yield return new WaitForSeconds(delay);
            //Play.
            GetComponent<Animator>().Play("Unholster");
            //Set isDone.
            isDone = true;
        }
        
        #endregion
    }
}