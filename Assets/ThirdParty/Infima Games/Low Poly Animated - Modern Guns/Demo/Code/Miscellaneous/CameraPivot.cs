//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// CameraPivot. This component is used to allow the camera to rotate around the character.
    /// </summary>
    public class CameraPivot : MonoBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Tooltip("Distance the camera starts at.")]
        [SerializeField]
        private float distance = -3.0f;
        
        #endregion
        
        #region FIELDS
        
        /// <summary>
        /// Keeps track of the state the camera is in.
        /// </summary>
        private bool lockedInFirstPerson = true;

        /// <summary>
        /// The camera transform’s current location for this frame.
        /// </summary>
        private Vector3 location;
        /// <summary>
        /// The rotation around the pivot point that we’re currently at,
        /// </summary>
        private Vector3 lerpedLocation;

        /// <summary>
        /// Mouse X.
        /// </summary>
        private float mouseX;
        /// <summary>
        /// Mouse Y.
        /// </summary>
        private float mouseY;

        #endregion

        #region UNITY

        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Update camera state based on input press.
            if (Cursor.visible == false && Input.GetKeyDown(KeyCode.C))
                lockedInFirstPerson = !lockedInFirstPerson;

            //Mouse X.
            mouseX += Cursor.visible ? 0 : (lockedInFirstPerson ? 0 : Input.GetAxisRaw("Mouse X"));
            //Mouse Y.
            mouseY -= Cursor.visible ? 0 : (lockedInFirstPerson ? 0 : Input.GetAxisRaw("Mouse Y"));

            //Interpolate Distance. This also adds the scrolwheel into account and allows us to scroll the camera in/out.
            distance = Mathf.Lerp(distance , distance + (Cursor.visible ? 0 : lockedInFirstPerson ? 0 : Input.GetAxisRaw("Mouse ScrollWheel") * 20), Time.deltaTime * 15);
            //Clamp Distance.
            distance = Mathf.Clamp(distance, -3, -1);
            
            //Update the location based on whether we're locked in the first person camera or not.
            location.z = lockedInFirstPerson ? 0 : distance;
            //Interpolated Location.
            lerpedLocation = Vector3.Lerp(lerpedLocation, new Vector3(mouseY, mouseX, 0), Time.deltaTime * 15);
            
            //Updates Location.
            transform.GetChild(0).localPosition = location;
            //Updates Rotation.
            transform.localEulerAngles = lockedInFirstPerson ? default : lerpedLocation;
        }
        
        #endregion
    }
}