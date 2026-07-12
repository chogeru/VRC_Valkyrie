//Copyright 2022, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Inputs. ScriptableObject containing all the possible inputs for a given player. These could theoretically be swapped at runtime, though we have absolutely no reason to do so for this asset. It appears we've built a somewhat nicer system than needed.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_LPAMG_Inputs", menuName = "Infima Games/Low Poly/Animated/Modern Guns Pack/Inputs", order = 0)]
    public class Inputs : ScriptableObject
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Values")]
        
        [Tooltip("All Input Values.")]
        [SerializeField, ReorderableList(Foldable = true)]
        private InputType[] values;

        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// Get. Returns the correct KeyCode for the given Type value.
        /// </summary>
        public KeyCode Get(string type) => values.FirstOrDefault(value => value.Type == type).KeyCode;
        
        #endregion
    }
}