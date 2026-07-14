//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// AttachmentSpawned. Contains all information related to what an Attachment needs when spawned.
    /// </summary>
    [System.Serializable]
    public struct AttachmentSpawned
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
        /// GameObject.
        /// </summary>
        public GameObject GameObject
        {
            //Get.
            get => gameObject;
            //Set.
            set => gameObject = value;
        }
        /// <summary>
        /// Index.
        /// </summary>
        public int Index
        {
            //Get.
            get => index;
            //Set.
            set => index = value;
        }
        
        #endregion
        
        #region FIELDS SERIALIZED

        [Tooltip("Defines the type of attachment this is. The name here is actually incredibly important, as it is later referenced in quite a few places.")]
        [SerializeField]
        private string type;

        [Tooltip("Socket on an item that we want to spawn this attachment at.")]
        [SerializeField]
        private string socket;

        [Tooltip("Reference to the Game Object in the scene that actually represents this Attachment.")]
        [SerializeField]
        private GameObject gameObject;

        [Tooltip("Index of this Attachment in the Variants list used to pick what we want to spawn.")]
        [SerializeField]
        private int index;

        #endregion
    }
}