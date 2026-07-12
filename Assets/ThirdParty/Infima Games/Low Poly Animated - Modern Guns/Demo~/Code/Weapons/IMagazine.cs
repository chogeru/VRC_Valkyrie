//Copyright 2022, Infima Games. All Rights Reserved.

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Interface that represents a weapon's magazine. Super useful to get some helper functions in there.
    /// </summary>
    public interface IMagazine
    {
        /// <summary>
        /// Basically an event raised whenever a magazine GameObject gets set to active.
        /// </summary>
        void Shown();
        /// <summary>
        /// Basically an event raised whenever a magazine GameObject gets set to inactive.
        /// </summary>
        void Hidden();

        /// <summary>
        /// Setting this to true will make it so that the magazine stays visible when a weapon finishes
        /// reloading with it.
        /// </summary>
        bool KeepVisibleAtEndOfReload();
    }
}