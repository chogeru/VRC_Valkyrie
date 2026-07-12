//Copyright 2022, Infima Games. All Rights Reserved.

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Game Mode Service.
    /// </summary>
    public interface IGameModeService : IGameService
    {
        #region FUNCTIONS
        
        /// <summary>
        /// Returns the Player Character.
        /// </summary>
        CharacterBehaviour GetPlayerCharacter();
        
        /// <summary>
        /// Returns the Player Character.
        /// </summary>
        WeaponBehaviour GetEquippedWeapon();
        
        #endregion
    }
}