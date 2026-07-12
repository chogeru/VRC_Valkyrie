//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Link. Defines what a link is. Links are very important in the context of Linkers, as they are the properties these last ones hold and reference.
    /// </summary>
    [System.Serializable]
    public struct Link<T>
    {
        #region PROPERTIES
        
        /// <summary>
        /// Type.
        /// </summary>
        public string Type => type;
        
        #endregion
        
        #region FIELDS SERIALIZED
        
        [Tooltip("Type of the link.")]
        [SerializeField]
        private string type;

        [Tooltip("Link values.")]
        [SerializeField, ReorderableList(Foldable = true)]
        private T[] values;

        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// Get. Returns the value of the link at the given index.
        /// </summary>
        public T Get(int index = 0)
        {
            //Check Reference.
            if (values == null || values.Length == 0)
                return default;

            //Check Length.
            if (index >= values.Length)
                return values[values.Length - 1];
            
            //Return.
            return values[index];
        }
        
        #endregion
    }
}