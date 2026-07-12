//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    public abstract class WeaponBehaviour : MonoBehaviour
    {
        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake(){}

        #endregion

        #region GETTERS

        /// <summary>
        /// Returns the name of this weapon item. This is very helpful for things like displaying the name.
        /// </summary>
        public abstract string GetItemName();
        /// <summary>
        /// Determines if this Weapon reloads in cycles.
        /// </summary>
        public abstract bool HasCycledReload();
        
        /// <summary>
        /// Returns true if this weapon shoots in automatic.
        /// </summary>
        public abstract bool IsAutomatic();
        /// <summary>
        /// Returns the weapon's rate of fire.
        /// </summary>
        public abstract float GetRateOfFire();

        /// <summary>
        /// Returns true if this weapon can reload while being aimed.
        /// </summary>
        /// <returns></returns>
        public abstract bool CanReloadAimed();

        /// <summary>
        /// Returns the RuntimeAnimationController the Character needs to use when this Weapon is equipped!
        /// </summary>
        public abstract RuntimeAnimatorController GetAnimatorController();
        /// <summary>
        /// Returns the weapon's attachment manager component.
        /// </summary>
        public abstract AttachmentBehaviour GetAttachments();
        
        #endregion

        #region METHODS

        /// <summary>
        /// Fires the weapon.
        /// </summary>
        public abstract void Fire();
        /// <summary>
        /// Reloads the weapon.
        /// </summary>
        public abstract void Reload();

        /// <summary>
        /// Spawns a new casing prefab and ejects it from the weapon. Really cool stuff.
        /// </summary>
        public abstract void EjectCasing();

        #endregion
    }
}