//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns.Interface
{
    /// <summary>
    /// This component updates whatever text is on the object to match the animation being played in the character's Actions layer in the Animator.
    /// </summary>
    public class TextAnimationPlaying : ElementText
    {
        #region METHODS
        
        /// <summary>
        /// Tick.
        /// </summary>
        protected override void Tick()
        {
            //Base.
            base.Tick();

            //Get ObjectLinker.
            var objectLinker = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<ObjectLinker>();

            //Get Animator GameObject.
            GameObject animatorObject = objectLinker.Get("Animator");
            //Check Reference.
            if (animatorObject == null)
                return;

            //Get Animator.
            var animator = animatorObject.GetComponent<Animator>();
            //Check Reference.
            if (animator == null)
                return;

            //Get ClipInfo.
            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(1);
            //Check Validity.
            if (clipInfos == null || clipInfos.Length == 0)
                return;

            //Get AnimationClip.
            AnimationClip animationClip = clipInfos[0].clip;
            //Check Reference.
            if (animationClip == null)
                return;
            
            //Get currently played clip name.
            string clipName = animationClip.name;
            //Update text to match the clip name.
            textMesh.text = $"Animation Playing: <b>{clipName}</b>";
        }
        
        #endregion
    }
}