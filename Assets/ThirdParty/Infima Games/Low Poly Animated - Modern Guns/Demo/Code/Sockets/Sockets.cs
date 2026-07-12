//Copyright 2022, Infima Games. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Sockets. Holds all Socket values for a specific GameObject. Very useful when we want to have certain snap points to move or spawn things to.
    /// </summary>
    public class Sockets : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Sockets")]
        
        [Tooltip("List of Socket values.")]
        [SerializeField, ReorderableList(Foldable = true)]
        private List<Socket> sockets;

        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// Returns the Transform value for the given Socket type.
        /// </summary>
        public Transform GetSocketTransform(string socket)
        {
            //Return.
            return sockets.Find(x => x.Type == socket).Transform;
        }
        
        #endregion
    }
}