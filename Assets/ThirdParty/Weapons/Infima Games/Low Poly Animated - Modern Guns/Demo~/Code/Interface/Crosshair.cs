//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns.Interface
{
    /// <summary>
    /// The Crosshair component helps with making sure that the on-screen crosshair changes its appearance based on different events in the asset.
    /// </summary>
    public class Crosshair : Element
    {
        #region FIELDS SERIALIZED
        
        [Tooltip("Reference to the RectTransform that this component needs to modify in order to make the entire crosshair look bigger//smaller.")]
        [SerializeField]
        private RectTransform holderTransform;

        [Tooltip("Reference to the CanvasGroup component needed to modify the entire crosshair’s alpha value.")]
        [SerializeField]
        private CanvasGroup canvasGroup;

        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// Reference to the player character’s Animator component that drives everything in the asset.
        /// </summary>
        private Animator characterAnimator;
        /// <summary>
        /// Represents the current size of the crosshair.
        /// </summary>
        private float currentSize = 50.0f;
        /// <summary>
        /// Represents the current opacity of the crosshair.
        /// </summary>
        private float currentOpacity = 1.0f;

        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Awake.
        /// </summary>
        protected override void Awake()
        {
            //Base.
            base.Awake();
            //Cache Animator.
            characterAnimator = characterBehaviour.GetComponent<ObjectLinker>().Get<Animator>("Animator");
        }
        
        #endregion
        
        #region METHODS

        /// <summary>
        /// Tick.
        /// </summary>
        protected override void Tick()
        {
            //Base.
            base.Tick();

            //Get AimingAlpha value from the character's animator. This is the value from [0, 1] that represents how much we're aiming.
            float aimingAlpha = characterAnimator.GetFloat(AHashes.AimingAlpha);
            //Modify the size based on that alpha so we can hide the crosshair when aiming.
            currentSize = Mathf.Lerp(50.0f, 0.0f, aimingAlpha);
            //Modify the opacity based on that alpha so we can hide the crosshair when aiming.
            currentOpacity = Mathf.Lerp(1.0f, 0.0f, aimingAlpha);
            
            //Update Horizontal Size.
            holderTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentSize);
            //Update Vertical Size.
            holderTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentSize);
            //Update Alpha.
            canvasGroup.alpha = currentOpacity;
        }
        
        #endregion
    }
}