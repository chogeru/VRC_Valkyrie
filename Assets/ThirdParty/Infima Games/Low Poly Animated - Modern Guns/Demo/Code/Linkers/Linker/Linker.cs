//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Linker. Base class defining a Linker. Mostly a bit of a helper class to avoid boilerplate.
    /// </summary>
    public class Linker<T> : ScriptableObject
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Links")]
        
        [Tooltip("Links.")]
        [SerializeField, ReorderableList(Foldable = true)]
        protected T[] links;
        
        #endregion
    }
}