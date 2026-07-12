//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Time Manager.
    /// </summary>
    public class TimeHandler : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Settings")]
        
        [Tooltip("Value the time scale gets updated by every time.")]
        [SerializeField]
        private float increment = 0.1f;
      
        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// Determines if the time is stopped.
        /// </summary>
        private bool paused;
        
        /// <summary>
        /// Current Time Scale.
        /// </summary>
        private float current = 1.0f;

        #endregion
        
        #region UNITY
        
        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Decrease Time Scale.
            if(Input.GetKeyDown(KeyCode.Alpha8))
                Increase(-increment);
            //Increase Time Scale.
            if(Input.GetKeyDown(KeyCode.Alpha9))
                Increase(increment);
            
            //Pause/Unpause Time Scale.
            if(Input.GetKeyDown(KeyCode.Alpha5))
                Toggle();
        }

        #endregion
        
        #region METHODS
        
        /// <summary>
        /// Updates The Time Scale.
        /// </summary>
        private void Scale()
        {
            //Update Time Scale.
            Time.timeScale = current;
        }
        
        /// <summary>
        /// Change Time Scale.
        /// </summary>
        private void Change(float value = 1.0f)
        {
            //Save Value.
            current = value;
            
            //Update.
            Scale();
        }

        /// <summary>
        /// Increase Time Scale Value.
        /// </summary>
        private void Increase(float value = 1.0f)
        {
            //Change.
            Change(Mathf.Clamp01(current + value));
        }

        /// <summary>
        /// Pause.
        /// </summary>
        private void Pause()
        {
            //Pause.
            paused = true;
            
            //Pause.
            Time.timeScale = 0.0f;
        }
        
        /// <summary>
        /// Toggle Pause.
        /// </summary>
        private void Toggle()
        {
            //Toggle Pause.
            if (paused)
                Unpause();
            else
                Pause();
        }

        /// <summary>
        /// Unpause.
        /// </summary>
        private void Unpause()
        {
            //Unpause.
            paused = false;
            
            //Unpause.
            Change(current);
        }
        
        #endregion
    }
}