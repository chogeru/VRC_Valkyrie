//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// This component simply helps us hide the player’s entire interface with the press of a button. There’s not much to this one really as it is only a few lines long and they just set an object active or inactive based on input.
    /// </summary>
    public class HideInterface : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Tooltip("The player’s interface object. This is the actual Game Object that we’re going to be messing around with in this script.")]
        [SerializeField]
        private GameObject interfaceObject;
        
        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Toggle the interface's visibility.
            if(Input.GetKeyDown(KeyCode.O))
                interfaceObject.SetActive(!interfaceObject.activeInHierarchy);
        }
        
        #endregion
    }    
}
