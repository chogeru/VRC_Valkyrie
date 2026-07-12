using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;

// Editor-only helpers (never included in the uploaded VRChat build).
// Speeds up wiring third-party weapon models (e.g. Infima Games packs) onto
// the Zombie game's Gun.cs / WeaponConfig system.
public static class WeaponSetupTool
{
    private const string WeaponDataFolder = "Assets/_Project/Data/Weapons";

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

    [MenuItem("Zombie Game/Weapons/2. Wire Selected GameObject As Gun")]
    private static void WireSelectedAsGun()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogWarning("[WeaponSetupTool] Select a weapon GameObject in the Hierarchy first (e.g. a duplicated Infima weapon prefab instance placed in your scene).");
            return;
        }

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

        EditorUtility.SetDirty(go);
        Debug.Log("[WeaponSetupTool] Wired VRCPickup + Gun on '" + go.name + "'. Now in the Inspector: assign Gun.config (a WeaponConfig from " + WeaponDataFolder + "), Gun.settings (GameSettings), and Gun.muzzle (an empty child Transform placed at the barrel tip).");
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
