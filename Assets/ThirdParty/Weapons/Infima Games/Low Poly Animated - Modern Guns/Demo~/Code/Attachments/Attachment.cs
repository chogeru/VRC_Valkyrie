//Copyright 2022, Infima Games. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Attachment. Contains all information related to what an Attachment needs in order to get properly spawned.
    /// </summary>
    [System.Serializable]
    public struct Attachment
    {
        #region PROPERTIES
        
        /// <summary>
        /// Type.
        /// </summary>
        public string Type
        {
            //Get.
            get => type;
            //Set.
            set => type = value;
        }
        /// <summary>
        /// Socket.
        /// </summary>
        public string Socket
        {
            //Get.
            get => socket;
            //Set.
            set => socket = value;
        }
        /// <summary>
        /// Variants.
        /// </summary>
        public List<GameObject> Variants
        {
            //Get.
            get => variants;
            //Set.
            set => variants = value;
        }
        
        #endregion
        
        #region FIELDS SERIALIZED

        [Tooltip("Defines the type of attachment this is. The name here is actually incredibly important, as it is later referenced in quite a few places.")]
        [SerializeField]
        private string type;

        [Tooltip("Socket on an item that we want to spawn this attachment at.")]
        [SerializeField]
        private string socket;

        [Tooltip("All possible variants of this attachment. These are basically the prefabs that can be spawned for this type of attachment, for instance all of the different scopes for a weapon.")]
        [SerializeField]
        private List<GameObject> variants;
        
        #endregion
    }
}