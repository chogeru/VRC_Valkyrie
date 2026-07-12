using System.IO;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;

// Editor-only helpers (never included in the uploaded VRChat build).
// Mirrors WeaponSetupTool.cs but for zombie models (e.g. NewPunch's
// "ShirtlessZombieFree" pack): generates a starter ZombieConfig and wires
// NavMeshAgent / Collider / ZombieAI / voice AudioSource onto a selected
// zombie model so it's ready to drop into WaveManager's pool.
public static class ZombieSetupTool
{
    private const string ZombieDataFolder = "Assets/_Project/Data/Zombies";

    [MenuItem("Zombie Game/Zombies/1. Generate Starter ZombieConfig")]
    private static void GenerateStarterConfig()
    {
        EnsureFolder(ZombieDataFolder);

        string path = ZombieDataFolder + "/ZombieConfig_Walker.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[ZombieSetupTool] ZombieConfig_Walker already exists at " + path);
            return;
        }

        GameObject go = new GameObject("ZombieConfig_Walker");
        ZombieConfig config = go.AddComponent<ZombieConfig>();
        config.zombieName = "Walker";
        config.maxHealth = 100f;
        config.moveSpeed = 2.2f;
        config.attackDamage = 10f;
        config.attackRange = 1.6f;
        config.attackCooldown = 1.2f;
        config.scoreValue = 10;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ZombieSetupTool] Generated " + path + ". Assign voice clips (Assets/ThirdParty/Zombie Voices Audio Pack) and tune stats in the Inspector.");
    }

    // Works on either:
    //  - a scene instance (e.g. a dragged-in copy of NewPunch's
    //    ShirtlessZombie_FREE prefab placed in the Hierarchy), or
    //  - the prefab ASSET itself selected in the Project window - in that
    //    case the wiring is saved directly onto the prefab, so every future
    //    (and already-placed, unmodified) instance gets it automatically.
    [MenuItem("Zombie Game/Zombies/2. Wire Selected GameObject As Zombie")]
    private static void WireSelectedAsZombie()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("[ZombieSetupTool] Select a zombie model first - either a scene instance in the Hierarchy, or the prefab asset itself in the Project window.");
            return;
        }

        if (PrefabUtility.IsPartOfPrefabAsset(selected))
        {
            string assetPath = AssetDatabase.GetAssetPath(selected);
            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);
            try
            {
                if (WireGameObject(contentsRoot))
                {
                    PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
                    Debug.Log("[ZombieSetupTool] Wired and saved directly onto the prefab asset at " + assetPath + ". Every instance you drag from it (and already-placed unmodified instances) now has this wiring.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }
        }
        else
        {
            if (WireGameObject(selected))
            {
                EditorUtility.SetDirty(selected);
                Debug.Log("[ZombieSetupTool] Wired '" + selected.name + "' (scene instance only - select the prefab asset in the Project window instead if you want this saved onto the prefab itself).");
            }
        }

        Debug.Log("[ZombieSetupTool] Now: assign ZombieAI.config (a ZombieConfig), ZombieAI.settings (GameSettings), ZombieAI.waveManager, add a VRC Object Sync (or Continuous transform sync), then duplicate/place this for the rest of the pool and register every instance in WaveManager.zombiePool.");
    }

    // Scans every prefab under Assets/ThirdParty/NewPunch for full zombie
    // characters (skips body-part/prop sub-prefabs and HDRP/URP variants,
    // since this project is Built-in Render Pipeline). Already-wired
    // prefabs (has a ZombieAI already) are skipped, so safe to re-run.
    [MenuItem("Zombie Game/Zombies/4. Auto-Wire ALL Known Zombie Prefabs")]
    private static void AutoWireAllZombiePrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/ThirdParty/NewPunch" });
        int wired = 0;
        int skipped = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("/Prefabs/")) continue;

            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.Contains("BodyParts")) continue; // separated limb/prop pieces, not a full character
            if (fileName.EndsWith("_HDRP") || fileName.EndsWith("_URP")) continue; // this project is Built-in RP

            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (contentsRoot.GetComponent<ZombieAI>() != null)
                {
                    skipped++;
                    continue;
                }
                if (!WireGameObject(contentsRoot))
                {
                    Debug.LogWarning("[ZombieSetupTool] Skipped " + path + " - looked wrong for a zombie (see previous error).");
                    continue;
                }

                PrefabUtility.SaveAsPrefabAsset(contentsRoot, path);
                wired++;
                Debug.Log("[ZombieSetupTool] Auto-wired: " + path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ZombieSetupTool] Auto-wire complete: " + wired + " prefab(s) wired, " + skipped + " already had ZombieAI. " +
            "Each still needs ZombieAI.config/settings/waveManager assigned by hand, plus a VRC Object Sync component.");
    }

    // Returns false (and logs why, without modifying anything) if the
    // selection is clearly not a zombie model.
    private static bool WireGameObject(GameObject go)
    {
        if (go.GetComponentInChildren<Camera>(true) != null)
        {
            Debug.LogError("[ZombieSetupTool] Refusing to wire '" + go.name + "' - it has a Camera component, which is never a zombie model. Select the correct zombie prefab/instance instead.");
            return false;
        }

        if (go.GetComponent<Collider>() == null)
        {
            CapsuleCollider capsule = go.AddComponent<CapsuleCollider>();
            capsule.height = 1.9f;
            capsule.radius = 0.35f;
            capsule.center = new Vector3(0f, 0.95f, 0f);
            Debug.Log("[ZombieSetupTool] Added a default CapsuleCollider to " + go.name + " - adjust to fit the model.");
        }

        NavMeshAgent agent = go.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = go.AddComponent<NavMeshAgent>();
            agent.radius = 0.35f;
            agent.height = 1.9f;
            agent.baseOffset = 0f;
        }

        AudioSource voice = go.GetComponent<AudioSource>();
        if (voice == null)
        {
            voice = go.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 1f; // 3D sound
            voice.maxDistance = 25f;
            voice.rolloffMode = AudioRolloffMode.Linear;
        }

        ZombieAI ai = go.GetComponent<ZombieAI>();
        if (ai == null) ai = go.AddComponent<ZombieAI>();
        ai.agent = agent;
        ai.hitCollider = go.GetComponent<Collider>();
        ai.voiceAudioSource = voice;
        if (ai.animator == null) ai.animator = go.GetComponentInChildren<Animator>();

        // Pooled zombies stay inactive until WaveManager activates them.
        go.SetActive(false);
        return true;
    }

    [MenuItem("Zombie Game/Zombies/3. Bake NavMesh For Current Scene")]
    private static void BakeNavMesh()
    {
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("[ZombieSetupTool] NavMesh baked for the active scene.");
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
