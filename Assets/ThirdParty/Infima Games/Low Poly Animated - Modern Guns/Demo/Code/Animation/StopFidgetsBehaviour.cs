//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// StopFidgetsBehaviour. Adding this StateMachineBehaviour to a State will make sure that fidgets do not play while
    /// in that state.
    /// </summary>
    public class StopFidgetsBehaviour : StateMachineBehaviour
    {
        #region UNITY
        
        /// <summary>
        /// OnStateExit.
        /// </summary>
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Base.
            base.OnStateExit(animator, stateInfo, layerIndex);
            
            //Enable Fidgets Again.
            animator.SetBool(AHashes.StopFidgets, false);
        }

        /// <summary>
        /// OnStateUpdate.
        /// </summary>
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //Base.
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            //Stop Fidgets.
            animator.SetBool(AHashes.StopFidgets, true);
        }
        
        #endregion
    }
}