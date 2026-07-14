//Copyright 2022, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// IconLinker. Holds an item's icons. Very useful to keep track of what icons pertain to what item.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_LPAMG_Icons", menuName = "Infima Games/Low Poly/Animated/Modern Guns Pack/Icons", order = 0)]
    public class IconLinker : Linker<Link<Sprite>>
    {
        #region FUNCTIONS
        
        /// <summary>
        /// Get. Returns the Sprite linked to the given type.
        /// </summary>
        public Sprite Get(string type, int index = 0)
        {
            //Return.
            return (from t in links where t.Type == type select t.Get(index)).FirstOrDefault();
        }
        
        #endregion
    }
}