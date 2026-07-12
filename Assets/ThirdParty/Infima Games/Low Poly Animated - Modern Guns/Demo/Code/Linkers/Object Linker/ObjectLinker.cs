//Copyright 2022, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// The Object Linker ScriptableObject contains an array of ObjectLink values which we can very easily reference from other scripts. This is useful to access objects that may be relevant to other ones in a more direct manner.
    /// </summary>
    public class ObjectLinker : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Object Links")]
        
        [Tooltip("Object Links.")]
        [SerializeField, ReorderableList(Foldable = true)]
        private ObjectLink[] objectLinks;

        #endregion
        
        #region FUNCTIONS
        
        /// <summary>
        /// Returns the object that the given link type refers to.
        /// </summary>
        public GameObject Get(string type) => objectLinks.First(link => link.Type == type).GameObject;
        /// <summary>
        /// Returns a reference to the first component of type T found attached to the GameObject that the given link type refers to.
        /// </summary>
        public T Get<T>(string type) => Get(type).GetComponent<T>();

        #endregion
    }
}