//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Animator Hashes.
    /// </summary>
    public static class AHashes
    {
        /// <summary>
        /// Aiming Bool Hash.
        /// </summary>
        public static readonly int Aim = Animator.StringToHash("Aim");
        
        /// <summary>
        /// Crouching Bool Hash.
        /// </summary>
        public static readonly int Crouching = Animator.StringToHash("Crouching");
        /// <summary>
        /// Jumping Bool Hash.
        /// </summary>
        public static readonly int Jumping = Animator.StringToHash("Jumping");

        /// <summary>
        /// Running Bool Hash.
        /// </summary>
        public static readonly int Running = Animator.StringToHash("Running");
        /// <summary>
        /// Lowered Bool Hash.
        /// </summary>
        public static readonly int Lowered = Animator.StringToHash("Lowered");

        /// <summary>
        /// Aiming Alpha Value.
        /// </summary>
        public static readonly int AimingAlpha = Animator.StringToHash("Aiming");
        /// <summary>
        /// Grip Index Value.
        /// </summary>
        public static readonly int GripIndex = Animator.StringToHash("Grip Index");

        /// <summary>
        /// Hashed "Movement".
        /// </summary>
        public static readonly int Movement = Animator.StringToHash("Movement");
        
        /// <summary>
        /// Hashed "Horizontal".
        /// </summary>
        public static readonly int Horizontal = Animator.StringToHash("Horizontal");
        /// <summary>
        /// Hashed "Vertical".
        /// </summary>
        public static readonly int Vertical = Animator.StringToHash("Vertical");
        
        /// <summary>
        /// Hashed "Stop Fidgets".
        /// </summary>
        public static readonly int StopFidgets = Animator.StringToHash("Stop Fidgets");
        /// <summary>
        /// Hashed "Tactical Sprint".
        /// </summary>
        public static readonly int TacticalSprint = Animator.StringToHash("Tactical Sprint");
    }
}