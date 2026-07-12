using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Editor-only helper (never included in the uploaded VRChat build).
//
// The bundled NewPunch "ShirtlessZombieFree" model ships with exactly one
// embedded animation clip on its FBX ("FreeRunning") and no walk/idle/
// attack/death clips. Rather than fight that, this tool builds a minimal
// Animator Controller that just loops whatever run/locomotion clip is
// embedded, and ZombieAI.cs covers attack/death entirely with script-driven
// procedural motion (lunge on attack, collapse on death) so the zombie
// still reads clearly even without a full animation set. Swap in a fuller
// rigged model later and this tool/ZombieAI keep working unchanged.
public static class ZombieAnimatorSetupTool
{
    private const string ControllerFolder = "Assets/_Project/Data/Zombies";
    private const string ControllerPath = ControllerFolder + "/ZombieLocomotion.controller";

    [MenuItem("Zombie Game/Zombies/4. Build Locomotion Animator Controller From Selected FBX")]
    private static void BuildFromSelection()
    {
        Object selected = Selection.activeObject;
        if (selected == null)
        {
            Debug.LogWarning("[ZombieAnimatorSetupTool] Select the zombie model's FBX (or a prefab instance using it) in the Project/Hierarchy first.");
            return;
        }

        string fbxPath = AssetDatabase.GetAssetPath(selected);
        if (string.IsNullOrEmpty(fbxPath) || !fbxPath.ToLower().EndsWith(".fbx"))
        {
            // Allow selecting a scene instance instead of the raw FBX asset.
            GameObject go = selected as GameObject;
            Animator animator = go != null ? go.GetComponentInChildren<Animator>() : null;
            if (animator == null || animator.avatar == null)
            {
                Debug.LogWarning("[ZombieAnimatorSetupTool] Select the model's .fbx asset in the Project window (or a scene instance with an Animator+Avatar) and try again.");
                return;
            }
            fbxPath = AssetDatabase.GetAssetPath(animator.avatar);
        }

        AnimationClip clip = FindLocomotionClip(fbxPath);
        if (clip == null)
        {
            Debug.LogWarning("[ZombieAnimatorSetupTool] No usable AnimationClip found inside " + fbxPath);
            return;
        }

        EnsureFolder(ControllerFolder);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        controller.RemoveLayer(0);
        controller.AddLayer("Base Layer");
        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        AnimatorState runState = sm.AddState("Run");
        runState.motion = clip;
        sm.defaultState = runState;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ZombieAnimatorSetupTool] Built " + ControllerPath + " looping clip '" + clip.name + "'. Assign it to the zombie prefab's Animator component (Controller field), then run 'Wire Selected GameObject As Zombie' as usual.");
    }

    private static AnimationClip FindLocomotionClip(string fbxPath)
    {
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        List<AnimationClip> clips = new List<AnimationClip>();
        foreach (Object obj in subAssets)
        {
            AnimationClip clip = obj as AnimationClip;
            if (clip == null) continue;
            if (clip.name.StartsWith("__preview__")) continue;
            clips.Add(clip);
        }

        if (clips.Count == 0) return null;

        foreach (AnimationClip clip in clips)
        {
            string lower = clip.name.ToLower();
            if (lower.Contains("run") || lower.Contains("walk") || lower.Contains("locomotion")) return clip;
        }

        return clips[0];
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = "Assets";
        foreach (string part in path.Substring("Assets/".Length).Split('/'))
        {
            string next = parent + "/" + part;
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(parent, part);
            parent = next;
        }
    }
}
