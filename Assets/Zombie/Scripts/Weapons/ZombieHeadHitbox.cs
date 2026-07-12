using UdonSharp;

// Empty marker component: attach to a zombie's head collider (a child
// object of the main hit collider) so Gun.cs can detect headshots without
// depending on Unity's project-wide Tag system.
public class ZombieHeadHitbox : UdonSharpBehaviour
{
}
