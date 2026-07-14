//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Character Abstract Behaviour.
    /// </summary>
    public abstract class CharacterBehaviour : MonoBehaviour
    {
        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake(){}
        /// <summary>
        /// Start.
        /// </summary>
        protected virtual void Start(){}

        /// <summary>
        /// Update.
        /// </summary>
        protected virtual void Update(){}

        #endregion
        
        #region GETTERS
        
        /// <summary>
        /// Returns a reference to the Inventory component.
        /// </summary>
        public abstract InventoryBehaviour GetInventory();
        
        /// <summary>
        /// Returns true if the game cursor is locked.
        /// </summary>
        public abstract bool IsCursorLocked();
        
        #endregion
    }
}