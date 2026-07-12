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

    // Select a zombie model instance placed in the scene (e.g. a dragged-in
    // copy of NewPunch's ShirtlessZombie_FREE prefab) and run this to wire
    // it up as one pool entry. Duplicate the result N times for the pool.
    [MenuItem("Zombie Game/Zombies/2. Wire Selected GameObject As Zombie")]
    private static void WireSelectedAsZombie()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogWarning("[ZombieSetupTool] Select a zombie model GameObject in the Hierarchy first.");
            return;
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

        EditorUtility.SetDirty(go);
        Debug.Log("[ZombieSetupTool] Wired NavMeshAgent + Collider + AudioSource + ZombieAI on '" + go.name + "'. Now: assign ZombieAI.config (a ZombieConfig), ZombieAI.settings (GameSettings), ZombieAI.waveManager, add a VRC Object Sync (or Continuous transform sync), then duplicate this GameObject for the rest of the pool and register every instance in WaveManager.zombiePool.");
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
