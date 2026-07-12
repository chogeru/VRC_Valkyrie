using System.IO;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;

// Editor-only helpers (never included in the uploaded VRChat build).
// Speeds up wiring third-party weapon models (e.g. Infima Games packs) onto
// the Zombie game's Gun.cs / WeaponConfig system.
public static class WeaponSetupTool
{
    private const string WeaponDataFolder = "Assets/_Project/Data/Weapons";
    private const string ThirdPartyFolder = "Assets/ThirdParty";
    private const string SourcePackagesFolder = "Assets/ThirdParty/_SourcePackages";

    // Raw ".unitypackage" files that were dropped straight into Assets root
    // (not yet imported). Add new pack folder names here as they're added.
    private static readonly string[] RawWeaponPackFolders = new string[]
    {
        "Low Poly AR Weapon Pack 1",
        "Low Poly AR Weapon Pack 3",
        "Low Poly Optic Pack 1",
        "Low Poly Pistol Weapon Pack 1",
        "Low Poly Pistol Weapon Pack 2",
        "Low Poly SMG Weapon Pack 2",
        "Low Poly SMG Weapon Pack 3",
        "Low Poly ShotGun Weapon Pack 1",
        "Low Poly ShotGun Weapon Pack 2",
        "Low Poly Weapon Pack 4_WWII_1",
    };

    // Imports every "<Pack>_URP.unitypackage" found under the raw pack
    // folders above, then moves the resulting content folder into
    // Assets/ThirdParty and the source .unitypackage into
    // Assets/ThirdParty/_SourcePackages so Assets root stays clean.
    // Uses the URP shader variant; this project is Built-in Render Pipeline,
    // so materials will need a shader pass afterward (see SETUP.md).
    [MenuItem("Zombie Game/Weapons/0. Import Raw Weapon Packages (URP)")]
    private static void ImportRawWeaponPackages()
    {
        EnsureFolder(ThirdPartyFolder);
        EnsureFolder(SourcePackagesFolder);

        int imported = 0;
        foreach (string packFolder in RawWeaponPackFolders)
        {
            string sourceDir = "Assets/" + packFolder;
            if (!AssetDatabase.IsValidFolder(sourceDir))
            {
                continue; // already imported/moved, or not present this project
            }

            string packageFile = FindUnityPackage(sourceDir, packFolder);
            if (packageFile == null)
            {
                Debug.LogWarning("[WeaponSetupTool] No _URP.unitypackage found in " + sourceDir);
                continue;
            }

            AssetDatabase.ImportPackage(packageFile, false);
            imported++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (imported > 0)
        {
            Debug.Log("[WeaponSetupTool] Imported " + imported + " weapon package(s). Run '0b. Move Imported Packs Into ThirdParty' next once Unity finishes importing.");
        }
        else
        {
            Debug.Log("[WeaponSetupTool] Nothing to import - either already imported, or no matching raw packages found at Assets root.");
        }
    }

    // Run this after the import above has finished (importing can take a
    // moment on large packs - watch the progress bar / console first).
    [MenuItem("Zombie Game/Weapons/0b. Move Imported Packs Into ThirdParty")]
    private static void MoveImportedPacksIntoThirdParty()
    {
        EnsureFolder(ThirdPartyFolder);
        EnsureFolder(SourcePackagesFolder);

        int moved = 0;
        foreach (string packFolder in RawWeaponPackFolders)
        {
            string sourceDir = "Assets/" + packFolder;
            if (!AssetDatabase.IsValidFolder(sourceDir)) continue;

            // Move the raw .unitypackage file(s) out of the way first.
            foreach (string pkg in Directory.GetFiles(sourceDir, "*.unitypackage"))
            {
                string pkgAssetPath = pkg.Replace('\\', '/');
                string dest = SourcePackagesFolder + "/" + Path.GetFileName(pkgAssetPath);
                if (AssetDatabase.LoadAssetAtPath<Object>(dest) == null)
                {
                    AssetDatabase.MoveAsset(pkgAssetPath, dest);
                }
            }

            // If the import created a matching "Assets/<packFolder>" content
            // tree alongside the raw package (same name), the folder already
            // *is* sourceDir - just relocate the whole thing under ThirdParty.
            if (Directory.GetDirectories(sourceDir).Length > 0)
            {
                string dest = ThirdPartyFolder + "/" + packFolder;
                if (AssetDatabase.LoadAssetAtPath<Object>(dest) == null)
                {
                    string error = AssetDatabase.MoveAsset(sourceDir, dest);
                    if (string.IsNullOrEmpty(error)) moved++;
                    else Debug.LogWarning("[WeaponSetupTool] Could not move " + sourceDir + ": " + error);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[WeaponSetupTool] Moved " + moved + " imported pack folder(s) into " + ThirdPartyFolder + ".");
    }

    private static string FindUnityPackage(string sourceDir, string packFolder)
    {
        string preferred = sourceDir + "/" + packFolder + "_URP.unitypackage";
        if (File.Exists(preferred)) return preferred;

        string[] any = Directory.GetFiles(sourceDir, "*_URP.unitypackage");
        return any.Length > 0 ? any[0] : null;
    }

    [MenuItem("Zombie Game/Weapons/1. Generate Starter WeaponConfigs")]
    private static void GenerateStarterConfigs()
    {
        EnsureFolder(WeaponDataFolder);

        // Best-guess archetypes for the Infima "Modern Guns" pack's code names.
        // Rename/retune freely in the Inspector - these are just a starting point.
        CreateConfig("AG14W", "Assault Rifle", isAutomatic: true, damage: 18f, headshotMul: 2f, fireRate: 8f, range: 60f, spread: 2.5f, mag: 30, reload: 2.0f, reserve: 180);
        CreateConfig("HVG7", "Light Machine Gun", isAutomatic: true, damage: 22f, headshotMul: 2f, fireRate: 9f, range: 55f, spread: 3.5f, mag: 75, reload: 3.2f, reserve: 300);
        CreateConfig("LRAF9", "Sniper Rifle", isAutomatic: false, damage: 90f, headshotMul: 3f, fireRate: 1.2f, range: 120f, spread: 0.2f, mag: 5, reload: 2.6f, reserve: 30);
        CreateConfig("MAK12", "Pistol", isAutomatic: false, damage: 25f, headshotMul: 2f, fireRate: 3f, range: 40f, spread: 1.0f, mag: 12, reload: 1.4f, reserve: 96);
        CreateConfig("RC425", "SMG", isAutomatic: true, damage: 14f, headshotMul: 2f, fireRate: 12f, range: 35f, spread: 3.0f, mag: 25, reload: 1.8f, reserve: 200);
        CreateConfig("SP60", "Shotgun", isAutomatic: false, damage: 70f, headshotMul: 1.5f, fireRate: 1.1f, range: 15f, spread: 6.0f, mag: 6, reload: 2.2f, reserve: 48);
        CreateConfig("X13", "Machine Pistol", isAutomatic: true, damage: 16f, headshotMul: 2f, fireRate: 10f, range: 30f, spread: 4.0f, mag: 20, reload: 1.5f, reserve: 160);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[WeaponSetupTool] Generated starter WeaponConfig prefabs in " + WeaponDataFolder + ". Names/stats are best-guess placeholders - rename and retune in the Inspector to match the actual model.");
    }

    // The newer "Low Poly AR/Pistol/SMG/Shotgun/WWII" packs use generic mesh
    // names per pack rather than one distinct codename per weapon, so these
    // are generated as reusable category archetypes instead of guessing
    // exact prefab names. Duplicate one of these per specific mesh you wire
    // up and rename it (e.g. "WeaponConfig_AR_A_1").
    [MenuItem("Zombie Game/Weapons/1b. Generate Category Archetype WeaponConfigs")]
    private static void GenerateCategoryArchetypes()
    {
        EnsureFolder(WeaponDataFolder);

        CreateConfig("AssaultRifle", "Category Archetype", isAutomatic: true, damage: 18f, headshotMul: 2f, fireRate: 8f, range: 60f, spread: 2.5f, mag: 30, reload: 2.0f, reserve: 180);
        CreateConfig("Pistol", "Category Archetype", isAutomatic: false, damage: 25f, headshotMul: 2f, fireRate: 3f, range: 40f, spread: 1.0f, mag: 12, reload: 1.4f, reserve: 96);
        CreateConfig("SMG", "Category Archetype", isAutomatic: true, damage: 14f, headshotMul: 2f, fireRate: 12f, range: 35f, spread: 3.0f, mag: 25, reload: 1.8f, reserve: 200);
        CreateConfig("Shotgun", "Category Archetype", isAutomatic: false, damage: 70f, headshotMul: 1.5f, fireRate: 1.1f, range: 15f, spread: 6.0f, mag: 6, reload: 2.2f, reserve: 48);
        CreateConfig("BoltActionRifle_WWII", "Category Archetype", isAutomatic: false, damage: 85f, headshotMul: 2.5f, fireRate: 0.8f, range: 100f, spread: 0.3f, mag: 5, reload: 2.8f, reserve: 40);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[WeaponSetupTool] Generated category archetype WeaponConfig prefabs in " + WeaponDataFolder + ". Duplicate + rename one per specific weapon mesh.");
    }

    private static void CreateConfig(string codeName, string archetypeLabel, bool isAutomatic, float damage, float headshotMul, float fireRate, float range, float spread, int mag, float reload, int reserve)
    {
        string path = WeaponDataFolder + "/WeaponConfig_" + codeName + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return; // don't clobber if it already exists

        GameObject go = new GameObject("WeaponConfig_" + codeName);
        WeaponConfig config = go.AddComponent<WeaponConfig>();
        config.weaponName = codeName + " (" + archetypeLabel + ")";
        config.isAutomatic = isAutomatic;
        config.damagePerHit = damage;
        config.headshotMultiplier = headshotMul;
        config.fireRate = fireRate;
        config.range = range;
        config.spreadDegrees = spread;
        config.magazineSize = mag;
        config.reloadTime = reload;
        config.reserveAmmoMax = reserve;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    // Works on either a scene instance, or the prefab ASSET itself selected
    // in the Project window (saved directly onto the prefab in that case -
    // see ZombieSetupTool.WireSelectedAsZombie for the same pattern).
    [MenuItem("Zombie Game/Weapons/2. Wire Selected GameObject As Gun")]
    private static void WireSelectedAsGun()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("[WeaponSetupTool] Select a weapon model first - either a scene instance in the Hierarchy, or the prefab asset itself in the Project window.");
            return;
        }

        if (PrefabUtility.IsPartOfPrefabAsset(selected))
        {
            string assetPath = AssetDatabase.GetAssetPath(selected);
            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);
            try
            {
                WireGameObject(contentsRoot);
                PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
                Debug.Log("[WeaponSetupTool] Wired and saved directly onto the prefab asset at " + assetPath + ".");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contentsRoot);
            }
        }
        else
        {
            WireGameObject(selected);
            EditorUtility.SetDirty(selected);
            Debug.Log("[WeaponSetupTool] Wired '" + selected.name + "' (scene instance only - select the prefab asset in the Project window instead if you want this saved onto the prefab itself).");
        }

        Debug.Log("[WeaponSetupTool] Now in the Inspector: assign Gun.config (a WeaponConfig from " + WeaponDataFolder + "), Gun.settings (GameSettings), and Gun.muzzle (an empty child Transform placed at the barrel tip).");
    }

    private static void WireGameObject(GameObject go)
    {
        if (go.GetComponent<Collider>() == null)
        {
            Debug.LogWarning("[WeaponSetupTool] " + go.name + " has no Collider. VRC Pickup requires one to be grabbable - add one before testing.");
        }

        if (go.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        if (go.GetComponent<VRCPickup>() == null)
        {
            go.AddComponent<VRCPickup>();
        }

        if (go.GetComponent<Gun>() == null)
        {
            go.AddComponent<Gun>();
        }
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
