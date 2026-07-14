//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Skinner. Applies a random Skin to a SkinnedMesh.
    /// </summary>
    public class Skinner : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "References")]

        [Tooltip("SkinnedMesh Component To Modify.")]
        [SerializeField]
        private SkinnedMesh skinnedMesh;

        [Tooltip("ScriptableObject containing all the Skins that this component can pick from when selecting a Skin to apply.")]
        [SerializeField]
        private SkinRandom skinRandom;
        
        [Title(label: "Settings")]

        [Tooltip("Determines if the Skinner should immediately apply a Skin to the SkinnedMesh when starting.")]
        [SerializeField]
        private bool autoApplyAtStart;
        
        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// Current Skin Applied.
        /// </summary>
        protected Skin skin;

        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Awake.
        /// </summary>
        private void Awake()
        {
            //Check autoApplyAtStart.
            if (!autoApplyAtStart) 
                return;
            
            //Randomize.
            Randomize();
            //Apply.
            Apply();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Randomizes the selected skin.
        /// </summary>
        public void Randomize()
        {
            //Reference Check.
            if (skinRandom == null)
                return;
            //Get random skin.
            skin = skinRandom.Get();
        }

        /// <summary>
        /// Applies the selected skin.
        /// </summary>
        public virtual void Apply()
        {
            //Check Reference.
            if (skin == null)
                Randomize();

            //Apply Body.
            if (skin != null)
                skin.Apply("Body", skinnedMesh);
        }
        
        #endregion
    }
}