using UnityEngine;
using UnityEditor;
using System.Linq;

public static class OrganizeHierarchy
{
    [MenuItem("DogWorld/Organize Hierarchy")]
    public static void Run()
    {
        var dogWorld = GameObject.Find("DogWorld");
        var env      = GameObject.Find("DogWorld/Environment");
        var roads    = GameObject.Find("DogWorld/Environment/Roads");

        if (dogWorld == null || env == null || roads == null)
        { Debug.LogError("DogWorld/Environment/Roads not found"); return; }

        // ── 1. Lighting group ─────────────────────────────────────────
        var lighting = new GameObject("Lighting");
        lighting.transform.SetParent(dogWorld.transform, false);

        var dirLight = GameObject.Find("Directional Light");
        if (dirLight != null) dirLight.transform.SetParent(lighting.transform, true);

        var ppVol = GameObject.Find("PostProcessVolume");
        if (ppVol != null) ppVol.transform.SetParent(lighting.transform, true);

        // ── 2. Move Plane → Environment/Ground ───────────────────────
        var plane = GameObject.Find("Plane");
        if (plane != null)
        {
            plane.name = "Ground";
            plane.transform.SetParent(env.transform, true);
            plane.transform.SetAsFirstSibling(); // put Ground at top
        }

        // ── 3. Sub-group Roads ────────────────────────────────────────
        var roadStraight = new GameObject("Straight");
        roadStraight.transform.SetParent(roads.transform, false);

        var roadCorners  = new GameObject("Corners");
        roadCorners.transform.SetParent(roads.transform, false);

        var roadSidewalks = new GameObject("Sidewalks");
        roadSidewalks.transform.SetParent(roads.transform, false);

        var roadLights = new GameObject("StreetLights");
        roadLights.transform.SetParent(roads.transform, false);

        // Reparent road children by prefix
        var toMove = Enumerable.Range(0, roads.transform.childCount)
            .Select(i => roads.transform.GetChild(i).gameObject)
            .Where(go => go != roadStraight && go != roadCorners && go != roadSidewalks && go != roadLights)
            .ToList(); // snapshot to avoid iterator invalidation

        foreach (var go in toMove)
        {
            string n = go.name;
            if      (n.StartsWith("Road_"))    go.transform.SetParent(roadStraight.transform, true);
            else if (n.StartsWith("Corner_"))  go.transform.SetParent(roadCorners.transform, true);
            else if (n.StartsWith("SW_"))      go.transform.SetParent(roadSidewalks.transform, true);
            else if (n.StartsWith("SL_"))      go.transform.SetParent(roadLights.transform, true);
            // BusStop stays in Roads
        }

        // ── 4. Gameplay group ─────────────────────────────────────────
        var gameplay = new GameObject("Gameplay");
        gameplay.transform.SetParent(dogWorld.transform, false);

        var agility = GameObject.Find("DogWorld/Agility");
        var toys    = GameObject.Find("DogWorld/Toys");
        var feeding = GameObject.Find("DogWorld/Feeding");
        if (agility != null) agility.transform.SetParent(gameplay.transform, true);
        if (toys    != null) toys.transform.SetParent(gameplay.transform, true);
        if (feeding != null) feeding.transform.SetParent(gameplay.transform, true);

        // ── 5. Re-order DogWorld children ────────────────────────────
        // Order: Lighting, Environment, Dog, Gameplay, DogConfig
        SetChildOrder(dogWorld.transform, new[]{ "Lighting", "Environment", "Dog", "Gameplay", "DogConfig" });

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[DogWorld] Hierarchy organized!");
    }

    static void SetChildOrder(Transform parent, string[] order)
    {
        for (int i = 0; i < order.Length; i++)
        {
            var child = parent.Find(order[i]);
            if (child != null) child.SetSiblingIndex(i);
        }
    }
}
