//Copyright 2022, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// TimeLinker. ScriptableObject that stores delays that we might want to use for different things in the asset.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_LPAMG_Time", menuName = "Infima Games/Low Poly/Animated/Modern Guns Pack/Time", order = 0)]
    public class TimeLinker : ScriptableObject
    {        
        #region FIELDS SERIALIZED
        
        [Title(label: "Time Links")]

        [Tooltip("Time Links.")]
        [SerializeField, ReorderableList(Foldable = true)]
        private TimeLink[] timeLinks;

        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// Returns the delay for the TimeLink of the given type.
        /// </summary>
        public float Get(string type)
        {
            //Return.
            return timeLinks.FirstOrDefault(link => link.Type.Split('/')[0] == type).Delay;
        }
        
        /// <summary>
        /// Returns a TimeLink with the given type.
        /// </summary>
        public TimeLink GetLink(string type)
        {
            //Return.
            return timeLinks.FirstOrDefault(link => link.Type.Split('/')[0] == type);
        }
        /// <summary>
        /// Returns all TimeLink values stored that have the type as their first sub-type (../..).
        /// </summary>
        public TimeLink[] GetLinks(string type)
        {
            //Return.
            return timeLinks.Where(link => link.Type.Split('/')[0] == type).ToArray();
        }
        
        #endregion
    }
}