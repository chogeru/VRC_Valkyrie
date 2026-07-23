using System.Collections.Generic;
#if UNITY_6000_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.PlayerInputManager;

namespace MalbersAnimations.InputSystem
{
    [AddComponentMenu("Malbers/Input/MInput Player Manager")]
    [RequireComponent(typeof(PlayerInputManager))]
    public class MInputPlayerManager : MonoBehaviour
    {
        public PlayerInputManager Manager;

#if UNITY_6000_0_OR_NEWER
        [SerializeField] private List<OutputChannels> playerOutputChannels;
#else
        // Cinemachine 2.x has no per-camera output-channel/split-screen concept
        // (that's a Cinemachine 3.x-only feature) - this project's VRChat SDK
        // pins Cinemachine 2.9.7, and VRChat worlds don't do local split-screen
        // multiplayer anyway, so channel routing below is a no-op on this version.
        [SerializeField] private List<int> playerOutputChannels;
#endif

        public List<PlayerInput> players;

        public List<Transform> SpawnPoints = new();

        private int NextPoint;

        public PlayerJoinedEvent OnPlayerJoined = new();
        public PlayerJoinedEvent OnPlayerLeft = new();

        private void OnEnable()
        {
            if (Manager == null)
                Manager = FindAnyObjectByType<PlayerInputManager>();

            if (Manager != null)
            {
                Manager.onPlayerJoined += PlayerJoined;
                Manager.onPlayerLeft += PlayerLeft;
            }
        }


        private void OnDisable()
        {
            if (Manager != null)
            {
                Manager.onPlayerJoined -= PlayerJoined;
                Manager.onPlayerLeft -= PlayerLeft;
            }
        }


        /// <summary> Check when the Player has Joined </summary>
        public void PlayerJoined(PlayerInput player)
        {
            Debug.Log($"Player Joined {player.name}", this);
            players.Add(player);
            var Player = player.transform;
            //Position the Player in a spawn point
            Player.position = SpawnPoints[NextPoint].position;
            CameraLayerSettings(player);
            NextPoint = (NextPoint + 1) % SpawnPoints.Count;
            OnPlayerJoined.Invoke(player);
        }

        private void CameraLayerSettings(PlayerInput player)
        {
            player.name += $"[{player.playerIndex}]";

#if UNITY_6000_0_OR_NEWER
            //It can have multiple Virtual Cameras
            var VirtualCams = player.transform.root.GetComponentsInChildren<CinemachineVirtualCameraBase>();

            foreach (var v in VirtualCams)
            {
                v.OutputChannel = playerOutputChannels[player.playerIndex];
            }
#endif

            var Camera = player.GetComponentInChildren<Camera>();

            Camera.name += $"[{player.playerIndex}]";

#if UNITY_6000_0_OR_NEWER
            var CMBrain = player.GetComponentInChildren<CinemachineBrain>();
            CMBrain.ChannelMask = playerOutputChannels[player.playerIndex];
#endif
        }


        //Check when the player has left
        public void PlayerLeft(PlayerInput input)
        {
            OnPlayerLeft.Invoke(input);
        }

        private void Reset()
        {
#if UNITY_6000_0_OR_NEWER
            playerOutputChannels = new List<OutputChannels>
            {
                OutputChannels.Channel01,
                OutputChannels.Channel02,
                OutputChannels.Channel03,
                OutputChannels.Channel04,
            };
#else
            playerOutputChannels = new List<int> { 0, 1, 2, 3 };
#endif
        }
    }
}
