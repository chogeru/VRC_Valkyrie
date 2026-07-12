//Copyright 2022, Infima Games. All Rights Reserved.

using TMPro;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns.Interface
{
    /// <summary>
    /// This component is in charge of setting the visibility of the different objects that make up our tutorial text. This is used in our animation showcase demos and is very helpful for us to showcase controls but likely won’t be of much help for anything else.
    /// </summary>
    public class TextTutorial : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Title")]
        
        [Tooltip("The title text for the tutorial.")]
        [SerializeField]
        private TextMeshProUGUI tutorialTitle;
        
        [Title(label: "Tutorial")]
        
        [Tooltip("A reference to the actual text.")]
        [SerializeField]
        private GameObject textTutorial;

        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// This value determines whether the tutorial text is actually visible on the screen or not.
        /// </summary>
        private bool visible;

        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Input.
            visible = Input.GetKey(KeyCode.Tab);
            
            //Activate.
            if(textTutorial)
                textTutorial.SetActive(visible);
            if(tutorialTitle)
                tutorialTitle.enabled = !visible;
        }
        
        #endregion
    }
}