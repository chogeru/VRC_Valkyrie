//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// InputType. Defines a type of input that the player can use. Usually held in the Inputs object.
    /// </summary>
    [System.Serializable]
    public struct InputType
    {
        #region PROPERTIES
        
        /// <summary>
        /// Type.
        /// </summary>
        public string Type => type;
        /// <summary>
        /// KeyCode.
        /// </summary>
        public KeyCode KeyCode => keyCode;
        
        #endregion
        
        #region FIELDS SERIALIZED
        
        [Title(label: "Type")]
        
        [Tooltip("Type of input. This functionally acts as a name for this specific input so we can recognize it, and search for it.")]
        [SerializeField]
        private string type;

        [Title(label: "KeyCode")]
        
        [Tooltip("Key to map to the type.")]
        [SerializeField, SearchableEnum]
        private KeyCode keyCode;
        
        #endregion
    }
}