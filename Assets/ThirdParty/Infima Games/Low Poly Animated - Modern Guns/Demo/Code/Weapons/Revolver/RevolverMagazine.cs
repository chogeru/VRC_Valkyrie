//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// 
    /// </summary>
    public class RevolverMagazine : MonoBehaviour, IMagazine
    {
        /// <summary>
        /// Type of casings. Just a helper enum to set some stuff inside this class.
        /// </summary>
        private enum CasingsType
        {
            Chamber, Speedloader
        }

        [Tooltip("Type of casings that this revolver magazine is currently pointing to. We basically have the normal magazine hold one type of casing, and the reserve (which is just an empty GameObject) hold the other.")]
        [SerializeField]
        private CasingsType casingsType;

        [Tooltip("If true, this magazine will stay visible at the end of reloads.")]
        [SerializeField]
        private bool keepVisible;
        
        #region FIELDS
        
        /// <summary>
        /// Reference to the currently-equipped weapon, which we will assume is a revolver for the sake of brevity.
        /// </summary>
        private WeaponBehaviour revolver;
        
        #endregion
        
        #region IMagazine
        
        /// <summary>
        /// Shown.
        /// </summary>
        public void Shown()
        {
            //Enable Casings.
            SetCasingsActive(true);
        }

        /// <summary>
        /// Hidden.
        /// </summary>
        public void Hidden()
        {
            //Disable Casings.
            SetCasingsActive(false);
        }

        /// <summary>
        /// KeepVisibleAtEndOfReload.
        /// </summary>
        public bool KeepVisibleAtEndOfReload() => keepVisible;
        
        #endregion
        
        #region METHODS

        /// <summary>
        /// Updates the state of the casings.
        /// </summary>
        private void SetCasingsActive(bool active)
        {
            //Get the actual revolver reference so we can access its chamber.
            revolver ??= ServiceLocator.Current.Get<IGameModeService>().GetEquippedWeapon();
            //Check Reference.
            if (revolver == null)
                return;

            //Get ChamberHandler.
            var chamberHandler = revolver.GetComponent<ChamberHandler>();
            //Check Reference.
            if (chamberHandler == null)
                return;

            //Switch Casings.
            switch (casingsType)
            {
                //Chamber.
                case CasingsType.Chamber:
                    //Update Chambered.
                    chamberHandler.SetChamberedCasings(active);
                    break;
                //Speedloader.
                case CasingsType.Speedloader:
                    //Update Speedloader.
                    chamberHandler.SetSpeedloaderCasings(active);
                    break;
            }
        }
        
        #endregion
    }   
}