using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// UdonSharpBehaviour "config" data holders (WeaponConfig, ZombieConfig,
// WaveConfig) were being assigned directly as prefab ASSET references
// (e.g. Gun.config = "Assets/.../WeaponConfig_Pistol.prefab") instead of
// scene instances. Udon only initializes UdonBehaviours that are actually
// part of the loaded scene graph, so every field read on an asset-only
// reference (config.damagePerHit, zombieConfig.maxHealth, wave.zombieCount,
// ...) silently came back as the type default (0 / false / null) at
// runtime - which is why zombies never spawned (WaveConfig.zombieCount
// read as 0) and weapons likely dealt no damage (WeaponConfig.damagePerHit
// read as 0) despite everything looking correctly wired in the Inspector.
//
// This tool instantiates one real scene copy of every distinct config
// prefab actually referenced by a Gun/ZombieAI/GameSettings in the open
// scene (parked inactive under a "_ConfigInstances" container so Udon
// still initializes them - VRC initializes all scene-placed UdonBehaviours
// regardless of active state), then repoints every reference at the new
// scene instance instead of the raw asset.
public static class ConfigInstantiationFixTool
{
    private const string ContainerName = "_ConfigInstances";

    [MenuItem("Zombie Game/5. Fix Config Prefab References (Instantiate In Scene)")]
    private static void FixConfigReferences()
    {
        Transform container = GetOrCreateContainer();
        var instanceCache = new Dictionary<Object, Object>(); // asset -> scene instance component of same type

        int gunsFixed = FixGunConfigs(container, instanceCache);
        int zombiesFixed = FixZombieConfigs(container, instanceCache);
        int wavesFixed = FixWaveConfigs(container, instanceCache);

        EditorUtility.SetDirty(container.gameObject);
        UnityEngine.SceneManagement.Scene scene = container.gameObject.scene;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log("[ConfigInstantiationFixTool] Done. Instantiated " + instanceCache.Count + " unique config prefab(s) into the scene. " +
            "Repointed: " + gunsFixed + " Gun(s), " + zombiesFixed + " ZombieAI(s), " + wavesFixed + " GameSettings.waves entrie(s). " +
            "Save the scene to persist this.");
    }

    private static Transform GetOrCreateContainer()
    {
        GameObject go = GameObject.Find(ContainerName);
        if (go == null)
        {
            go = new GameObject(ContainerName);
            go.SetActive(false); // never needs to be visible/active - Udon still inits it
        }
        return go.transform;
    }

    // Returns the scene-instantiated copy of `asset` (a prefab asset Component
    // reference), creating and caching it under `container` the first time
    // any caller asks for that particular asset.
    private static T GetOrInstantiate<T>(Object asset, Transform container, Dictionary<Object, Object> cache) where T : Component
    {
        if (asset == null) return null;
        if (cache.TryGetValue(asset, out Object cached)) return cached as T;

        // asset here is the Component (e.g. WeaponConfig) on the prefab asset.
        GameObject assetGo = (asset as Component).gameObject;
        GameObject instanceGo = (GameObject)PrefabUtility.InstantiatePrefab(assetGo, container);
        instanceGo.name = assetGo.name;

        T instanceComponent = instanceGo.GetComponent<T>();
        cache[asset] = instanceComponent;
        return instanceComponent;
    }

    private static int FixGunConfigs(Transform container, Dictionary<Object, Object> cache)
    {
        int fixedCount = 0;
        Gun[] guns = Object.FindObjectsOfType<Gun>(true);
        foreach (Gun gun in guns)
        {
            if (gun.config == null) continue;
            if (!IsAssetReference(gun.config)) continue;

            WeaponConfig sceneInstance = GetOrInstantiate<WeaponConfig>(gun.config, container, cache);
            if (sceneInstance == null) continue;

            SerializedObject so = new SerializedObject(gun);
            SerializedProperty prop = so.FindProperty("config");
            prop.objectReferenceValue = sceneInstance;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(gun);
            fixedCount++;
        }
        return fixedCount;
    }

    private static int FixZombieConfigs(Transform container, Dictionary<Object, Object> cache)
    {
        int fixedCount = 0;
        ZombieAI[] zombies = Object.FindObjectsOfType<ZombieAI>(true);
        foreach (ZombieAI zombie in zombies)
        {
            if (zombie.config == null) continue;
            if (!IsAssetReference(zombie.config)) continue;

            ZombieConfig sceneInstance = GetOrInstantiate<ZombieConfig>(zombie.config, container, cache);
            if (sceneInstance == null) continue;

            SerializedObject so = new SerializedObject(zombie);
            SerializedProperty prop = so.FindProperty("config");
            prop.objectReferenceValue = sceneInstance;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(zombie);
            fixedCount++;
        }
        return fixedCount;
    }

    private static int FixWaveConfigs(Transform container, Dictionary<Object, Object> cache)
    {
        int fixedCount = 0;
        GameSettings[] allSettings = Object.FindObjectsOfType<GameSettings>(true);
        foreach (GameSettings settings in allSettings)
        {
            if (settings.waves == null) continue;

            SerializedObject so = new SerializedObject(settings);
            SerializedProperty wavesProp = so.FindProperty("waves");
            bool changed = false;

            for (int i = 0; i < wavesProp.arraySize; i++)
            {
                SerializedProperty element = wavesProp.GetArrayElementAtIndex(i);
                WaveConfig current = element.objectReferenceValue as WaveConfig;
                if (current == null || !IsAssetReference(current)) continue;

                WaveConfig sceneInstance = GetOrInstantiate<WaveConfig>(current, container, cache);
                if (sceneInstance == null) continue;

                element.objectReferenceValue = sceneInstance;
                changed = true;
                fixedCount++;
            }

            if (changed)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
            }
        }
        return fixedCount;
    }

    // A Component reference is "asset-only" (never initialized by Udon at
    // runtime) if its GameObject doesn't belong to any loaded scene.
    private static bool IsAssetReference(Object component)
    {
        var comp = component as Component;
        if (comp == null) return false;
        return !comp.gameObject.scene.IsValid();
    }
}
