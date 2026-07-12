//Copyright 2022, Infima Games. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// AttachmentData. Contains information on the complete set of attachments that any given weapon in the asset
    /// can have.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_WEP_Attachments", menuName = "Infima Games/Low Poly/Animated/Modern Guns Pack/Attachment Data", order = 0)]
    public class AttachmentData : ScriptableObject
    {
        #region PROPERTIES
        
        /// <summary>
        /// Attachments.
        /// </summary>
        public List<Attachment> Attachments => attachments;

        #endregion

        #region FIELDS SERIALIZED
        
        [Title(label: "Attachments")]

        [Tooltip("The list of attachments to spawn.")]
        [SerializeField, ReorderableList]
        private List<Attachment> attachments;
        
        #endregion
    }
}