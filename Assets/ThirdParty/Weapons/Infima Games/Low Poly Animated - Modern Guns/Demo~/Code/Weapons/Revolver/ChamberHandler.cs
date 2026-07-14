//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Has some super useful functions for hiding different bullets inside of a revolver’s chamber and its speedloader. This is very useful when trying to keep certain things hidden while reloading, for instance.
    /// </summary>
    public class ChamberHandler : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Chamber")]
        
        [Tooltip("Casing GameObjects inside of this Weapon's chamber.")]
        [SerializeField]
        private GameObject[] chamberedCasings;

        [Title(label: "Speedloader")]
        
        [Tooltip("Casing GameObjects inside of this Weapon's speedloader.")]
        [SerializeField]
        private GameObject[] speedloaderCasings;

        #endregion
        
        #region METHODS
        
        /// <summary>
        /// Makes the chamberedCasings all either active or inactive.
        /// </summary>
        public void SetChamberedCasings(bool active)
        {
            //Set Active Value.
            chamberedCasings.ForEach((casing => casing.SetActive(active)));
        }

        /// <summary>
        /// Makes the speedloaderCasings all either active or inactive.
        /// </summary>
        public void SetSpeedloaderCasings(bool active)
        {
            //Set Active Value.
            speedloaderCasings.ForEach((casing => casing.SetActive(active)));
        }
        
        #endregion
    }   
}