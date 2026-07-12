using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

// Put a Collider on this GameObject (Is Trigger not required) so VRChat
// shows an "Use" interact prompt. Any player can press it; GameManager
// itself decides whether the request is honored (only from the Lobby state).
public class GameStartButton : UdonSharpBehaviour
{
    public GameManager gameManager;

    public override void Interact()
    {
        if (gameManager != null)
        {
            gameManager.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(GameManager.RequestStartGame));
        }
    }
}
