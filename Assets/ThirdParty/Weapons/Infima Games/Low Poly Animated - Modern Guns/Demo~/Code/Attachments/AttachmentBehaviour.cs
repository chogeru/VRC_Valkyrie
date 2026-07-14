//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// AttachmentBehaviour.
    /// </summary>
    public abstract class AttachmentBehaviour : MonoBehaviour
    {
        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake(){}

        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// Returns the Attachment variant that was spawned for the given Type input.
        /// </summary>
        public abstract GameObject GetVariant(string type);
        /// <summary>
        /// Returns a component on the Attachment variant that was spawned for the given Type input.
        /// </summary>
        public abstract T GetVariant<T>(string type) where T : MonoBehaviour;

        /// <summary>
        /// Returns the Attachment value for a given Type.
        /// </summary>
        public abstract AttachmentSpawned Get(string type);
        
        #endregion

        #region METHODS

        /// <summary>
        /// Randomizes the spawned Attachment variants.
        /// </summary>
        public abstract void Randomize();
        
        #endregion
    }
}