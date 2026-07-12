//Copyright 2022, Infima Games. All Rights Reserved.

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// The Game Service Interface is the interface that defines what a Game Service needs to have in order to be usable in the project. Having this here basically allows us to swap Game Services at runtime if we really want to for a similar one with a completely different implementation!
    /// This specific interface is implemented by every single service in the asset, and it helps us know that all of them are in fact services. We also use this to neatly store them and it might even be helpful in case we wanted everything to have certain functions or methods.
    /// </summary>
    public interface IGameService
    {
        
    }
}