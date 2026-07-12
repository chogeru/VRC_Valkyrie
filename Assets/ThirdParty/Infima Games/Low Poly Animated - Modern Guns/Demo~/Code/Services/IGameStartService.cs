//Copyright 2022, Infima Games. All Rights Reserved.

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Game Start Service.
    /// </summary>
    public interface IGameStartService : IGameService
    {
        #region METHODS
        
        /// <summary>
        /// Sets the value of Started.
        /// </summary>
        void SetStarted(bool value);
        /// <summary>
        /// Sets the value of ClickedToStart.
        /// </summary>
        void SetClickedToStart(bool value);

        /// <summary>
        /// Resets everything here. This is used when restarting the game.
        /// </summary>
        void Reset();

        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// Returns true if the game has started.
        /// </summary>
        bool HasStarted();
        /// <summary>
        /// Returns true if the player has clicked to start the game.
        /// </summary>
        /// <returns></returns>
        bool HasClickedToStart();

        #endregion
    }
}