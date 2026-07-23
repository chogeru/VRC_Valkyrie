using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// DogWorld シーンのヒエラルキーを整理するエディタツール。
///
/// 目標構造:
/// VRCWorld          ← VRC SDK必須、ルートに残す
/// Main Camera       ← Unity必須、ルートに残す
/// EventSystem       ← ルートに残す
/// DogWorld
///   Lighting
///     Directional Light
///     PostProcessVolume
///   Environment
///     Ground               ← Plane をリネーム
///     Park
///       Kouenn
///       ParkGround
///     Roads
///       Straight           ← Road_N/S/E/W_*
///       Corners            ← Corner_*
///       Sidewalks          ← SW_*
///       StreetLights       ← SL_*
///       UtilityPoles       ← Pole_*
///       Intersections      ← TrafficLight_*, Crosswalk_*
///       Props              ← BusStop_*, Vending_*, VendingBox_*
///     Buildings
///       North              ← Bldg_N_*
///       South              ← Bldg_S_*
///       East               ← Bldg_E_*
///       West               ← Bldg_W_*
///       Corners            ← Bldg_Corner_*
///   Dog
///     ShibaInu
///     DogConfig
///   Gameplay
///     Agility
///       AgilityWaypoints
///     Toys
///     Feeding
/// </summary>
public static class OrganizeHierarchy
{
    [MenuItem("DogWorld/Organize Hierarchy")]
    public static void Run()
    {
        var dogWorld = GameObject.Find("DogWorld");
        if (dogWorld == null) { Debug.LogError("DogWorld not found"); return; }

        // ── 0. 重複・空グループを削除 ─────────────────────────────────
        DestroyEmptyDuplicates(dogWorld.transform, "Gameplay");
        DestroyEmptyDuplicates(dogWorld.transform, "Lighting");

        // ── 1. Lighting ──────────────────────────────────────────────
        var lighting = GetOrCreate("Lighting", dogWorld.transform);
        ReparentFind("Directional Light", lighting.transform);
        ReparentFind("PostProcessVolume",  lighting.transform);

        // ── 2. Environment ───────────────────────────────────────────
        var env = GetOrCreate("Environment", dogWorld.transform);

        // Ground (旧 Plane)
        var ground = FindInHierarchy("Plane") ?? FindInHierarchy("Ground");
        if (ground != null)
        {
            ground.name = "Ground";
            ground.transform.SetParent(env.transform, true);
        }

        // Park
        var park = FindChildRecursive(dogWorld.transform, "Park");
        if (park != null) park.transform.SetParent(env.transform, true);

        // ── 3. Roads サブグループ ─────────────────────────────────────
        var roadsGo = env.transform.Find("Roads")?.gameObject ?? GetOrCreate("Roads", env.transform);

        var grpStraight  = GetOrCreate("Straight",      roadsGo.transform);
        var grpCorners   = GetOrCreate("Corners",       roadsGo.transform);
        var grpSidewalks = GetOrCreate("Sidewalks",     roadsGo.transform);
        var grpLights    = GetOrCreate("StreetLights",  roadsGo.transform);
        var grpPoles     = GetOrCreate("UtilityPoles",  roadsGo.transform);
        var grpIntersect = GetOrCreate("Intersections", roadsGo.transform);
        var grpProps     = GetOrCreate("Props",         roadsGo.transform);

        foreach (var go in SnapshotChildren(roadsGo.transform))
        {
            if (IsAnyOf(go, grpStraight, grpCorners, grpSidewalks, grpLights,
                            grpPoles, grpIntersect, grpProps)) continue;
            string n = go.name;
            if      (n.StartsWith("Road_"))           go.transform.SetParent(grpStraight.transform,  true);
            else if (n.StartsWith("Corner_"))         go.transform.SetParent(grpCorners.transform,   true);
            else if (n.StartsWith("SW_"))             go.transform.SetParent(grpSidewalks.transform, true);
            else if (n.StartsWith("SL_"))             go.transform.SetParent(grpLights.transform,    true);
            else if (n.StartsWith("Pole_"))           go.transform.SetParent(grpPoles.transform,     true);
            else if (n.StartsWith("TrafficLight_") ||
                     n.StartsWith("Crosswalk_"))      go.transform.SetParent(grpIntersect.transform, true);
            else if (n.StartsWith("BusStop")  ||
                     n.StartsWith("Vending")  ||
                     n.StartsWith("VendingBox"))      go.transform.SetParent(grpProps.transform,     true);
        }

        SetChildOrder(roadsGo.transform,
            "Straight", "Corners", "Sidewalks", "StreetLights", "UtilityPoles", "Intersections", "Props");

        // ── 4. Buildings サブグループ ─────────────────────────────────
        var bldgsGo = env.transform.Find("Buildings")?.gameObject ?? GetOrCreate("Buildings", env.transform);

        var bNorth   = GetOrCreate("North",   bldgsGo.transform);
        var bSouth   = GetOrCreate("South",   bldgsGo.transform);
        var bEast    = GetOrCreate("East",    bldgsGo.transform);
        var bWest    = GetOrCreate("West",    bldgsGo.transform);
        var bCorners = GetOrCreate("Corners", bldgsGo.transform);

        foreach (var go in SnapshotChildren(bldgsGo.transform))
        {
            if (IsAnyOf(go, bNorth, bSouth, bEast, bWest, bCorners)) continue;
            string n = go.name;
            if      (n.StartsWith("Bldg_N_"))      go.transform.SetParent(bNorth.transform,   true);
            else if (n.StartsWith("Bldg_S_"))      go.transform.SetParent(bSouth.transform,   true);
            else if (n.StartsWith("Bldg_E_"))      go.transform.SetParent(bEast.transform,    true);
            else if (n.StartsWith("Bldg_W_"))      go.transform.SetParent(bWest.transform,    true);
            else if (n.StartsWith("Bldg_Corner_")) go.transform.SetParent(bCorners.transform, true);
        }

        SetChildOrder(bldgsGo.transform, "North", "South", "East", "West", "Corners");

        SetChildOrder(env.transform, "Ground", "Park", "Roads", "Buildings");

        // ── 5. Dog ───────────────────────────────────────────────────
        var dog = GetOrCreate("Dog", dogWorld.transform);
        ReparentFind("ShibaInu",  dog.transform);
        ReparentFind("DogConfig", dog.transform);

        // ── 6. Gameplay ──────────────────────────────────────────────
        var gameplay = GetOrCreate("Gameplay", dogWorld.transform);
        ReparentFind("Agility", gameplay.transform);
        ReparentFind("Toys",    gameplay.transform);
        ReparentFind("Feeding", gameplay.transform);

        // ── 7. DogWorld 直下の順序 ───────────────────────────────────
        SetChildOrder(dogWorld.transform, "Lighting", "Environment", "Dog", "Gameplay");

        // ── 8. シーンルートの順序 ────────────────────────────────────
        MoveRootSibling("VRCWorld",    0);
        MoveRootSibling("Main Camera", 1);
        MoveRootSibling("EventSystem", 2);
        MoveRootSibling("DogWorld",    3);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[OrganizeHierarchy] 完了！");
    }

    // ── ヘルパー ─────────────────────────────────────────────────────

    static GameObject GetOrCreate(string name, Transform parent)
    {
        var existing = parent.Find(name);
        if (existing != null) return existing.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    static List<GameObject> SnapshotChildren(Transform parent)
    {
        var list = new List<GameObject>();
        for (int i = 0; i < parent.childCount; i++)
            list.Add(parent.GetChild(i).gameObject);
        return list;
    }

    static bool IsAnyOf(GameObject go, params GameObject[] others)
    {
        foreach (var o in others) if (go == o) return true;
        return false;
    }

    /// <summary>シーン内で name に一致する最初のオブジェクトを parent に移動する。</summary>
    static void ReparentFind(string name, Transform parent)
    {
        foreach (var t in Object.FindObjectsOfType<Transform>())
        {
            if (t.name == name && t.transform.parent != parent)
            {
                t.transform.SetParent(parent, true);
                return;
            }
        }
    }

    static GameObject FindInHierarchy(string name)
    {
        foreach (var t in Object.FindObjectsOfType<Transform>())
            if (t.name == name) return t.gameObject;
        return null;
    }

    static GameObject FindChildRecursive(Transform root, string name)
    {
        if (root.name == name) return root.gameObject;
        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindChildRecursive(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    static void SetChildOrder(Transform parent, params string[] order)
    {
        for (int i = 0; i < order.Length; i++)
        {
            var child = parent.Find(order[i]);
            if (child != null) child.SetSiblingIndex(i);
        }
    }

    static void MoveRootSibling(string name, int index)
    {
        var go = GameObject.Find(name);
        if (go != null && go.transform.parent == null)
            go.transform.SetSiblingIndex(index);
    }

    static void DestroyEmptyDuplicates(Transform parent, string groupName)
    {
        var found = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            var c = parent.GetChild(i);
            if (c.name == groupName) found.Add(c);
        }
        if (found.Count <= 1) return;
        foreach (var t in found)
            if (t.childCount == 0)
                Object.DestroyImmediate(t.gameObject);
    }
}
