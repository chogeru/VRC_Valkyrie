//Copyright 2022, Infima Games. All Rights Reserved.

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Game Mode Service.
    /// </summary>
    public class GameStartService : IGameStartService
    {
        #region FIELDS

        /// <summary>
        /// True if the game has started.
        /// </summary>
        private bool hasStarted;
        /// <summary>
        /// True if the player has clicked to start the game.
        /// </summary>
        private bool hasClickedToStart;
        
        #endregion
        
        #region METHODS
        
        /// <summary>
        /// SetStarted.
        /// </summary>
        public void SetStarted(bool value)
        {
            //Set.
            hasStarted = value;
        }
        /// <summary>
        /// SetClickedToStart.
        /// </summary>
        public void SetClickedToStart(bool value)
        {
            //Set.
            hasClickedToStart = value;
        }

        /// <summary>
        /// Reset.
        /// </summary>
        public void Reset()
        {
            //Reset.
            hasStarted = hasClickedToStart = false;
        }

        #endregion
        
        #region FUNCTIONS

        /// <summary>
        /// HasStarted.
        /// </summary>
        public bool HasStarted()
        {
            //Return.
            return hasStarted;
        }
        /// <summary>
        /// HasClickedToStart.
        /// </summary>
        public bool HasClickedToStart()
        {
            //Return.
            return hasClickedToStart;
        }

        #endregion
    }
}