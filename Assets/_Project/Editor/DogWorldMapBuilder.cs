using UnityEngine;
using UnityEditor;

public static class DogWorldMapBuilder
{
    private const string ROAD_BASE   = "Assets/ThirdParty/Environment/TsubokuLab/Models/JapaneseStreetPack/RoadUnit/Prefabs/RoadUnit/";
    private const string LIGHT_PATH  = "Assets/ThirdParty/Environment/TsubokuLab/Models/JapaneseStreetPack/StreetLight/Prefabs/StreetLightPrefab.prefab";
    private const string BUSSTOP     = "Assets/ThirdParty/Environment/TsubokuLab/Models/JapaneseStreetPack/BusStop/Prefabs/BusStopPrefab.prefab";
    private const string FUN_BASE    = "Assets/ThirdParty/FUNSET/shouwatownmodel/swt_prefab/mall_set/prfb_building/";
    private const string KOUENN_FBX  = "Assets/ThirdParty/Environment/kouenn/kouenn.fbx";

    private const string TLAB_BASE   = "Assets/ThirdParty/Environment/TsubokuLab/Models/JapaneseStreetPack/";
    private const string VENDING     = "VendingMachine/Prefabs/VendingMachinePrefab.prefab";
    private const string VEND_BOX    = "VendingMachine/Prefabs/VendingMachineDustBoxPrefab.prefab";
    private const string TRAFFIC_XRD = "TrafficLight/Prefabs/TrafficLightContainer_Crossroad_TimerType.prefab";
    private const string CROSSWALK   = "RoadMarkings/Prefabs/RoadMarking_Crosswalk_Set.prefab";
    private const string UTIL_POLE   = "UtilityPole/Prefabs/UtilityPolePrefab.prefab";

    // Road unit is 20m long × ~8m wide (half-width = 4m from center line to curb)
    // Park area: 40×40m at origin
    // Roads run at roadDist from center (park edge + half road width)
    private const float ROAD_DIST   = 30f;   // center of road from world center
    private const float SWALK_DIST  = 35.5f; // sidewalk (road_dist + 5.5)
    private const float LIGHT_DIST  = 37.5f; // street light (sidewalk + 2)
    private const float BLDG_DIST   = 52f;   // buildings outside sidewalk

    // ─────────────────────────────────────────────────────
    [MenuItem("DogWorld/Build Map")]
    public static void BuildMap()
    {
        var env = GameObject.Find("DogWorld/Environment");
        if (env == null) { Debug.LogError("DogWorld/Environment not found"); return; }

        // ─── GROUND PLANE ───────────────────────────────
        // "Ground" (旧 Plane) を Environment 直下に確保する。
        // ClearMap でも削除しないため、ここで再作成または参照を保持する。
        var ground = env.transform.Find("Ground")?.gameObject
                  ?? GameObject.Find("Ground")
                  ?? GameObject.Find("Plane");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            // Default grey material is fine – it represents the road/pavement base
        }
        ground.name = "Ground";
        ground.transform.SetParent(env.transform, false);
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale    = new Vector3(22f, 1f, 22f); // 220×220m
        ground.transform.SetAsFirstSibling();

        // ─── PARK (kouenn) ──────────────────────────────
        var parkParent = new GameObject("Park");
        parkParent.transform.SetParent(env.transform, false);

        var kouennFbx = AssetDatabase.LoadAssetAtPath<GameObject>(KOUENN_FBX);
        if (kouennFbx != null)
        {
            var park = Object.Instantiate(kouennFbx, parkParent.transform);
            park.name = "Kouenn";
            park.transform.localPosition = new Vector3(0f, -0.8f, 0f); // sink terrain under ground plane
            park.transform.localScale    = new Vector3(2.5f, 2.5f, 2.5f); // scale up playground
        }
        else Debug.LogWarning("kouenn.fbx not found");

        // Green park ground (covers any terrain bleed-through)
        var parkGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        parkGround.name = "ParkGround";
        parkGround.transform.SetParent(parkParent.transform, false);
        parkGround.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        parkGround.transform.localScale    = new Vector3(2.4f, 1f, 2.4f); // 24×24m green patch
        // Find any available shader for the green ground
        Shader greenShader = Shader.Find("Standard")
                          ?? Shader.Find("Diffuse")
                          ?? Shader.Find("Legacy Shaders/Diffuse");
        // Fallback: copy from existing ground material
        if (greenShader == null && ground != null)
            greenShader = ground.GetComponent<Renderer>()?.sharedMaterial?.shader;
        var greenMat = greenShader != null
            ? new Material(greenShader)
            : new Material(ground != null ? ground.GetComponent<Renderer>().sharedMaterial : null);
        if (greenMat != null)
            greenMat.color = new Color(0.35f, 0.65f, 0.25f); // grass green
        parkGround.GetComponent<Renderer>().material = greenMat;
        // Disable collider so it doesn't interfere
        Object.DestroyImmediate(parkGround.GetComponent<MeshCollider>());

        // ─── ROADS ──────────────────────────────────────
        var roadParent = new GameObject("Roads");
        roadParent.transform.SetParent(env.transform, false);

        // 3 segments × 20m = 60m per side, spanning from -30 to +30 so ends meet corners at ±30
        for (int i = -1; i <= 1; i++)
        {
            float off = i * 20f;
            Place(ROAD_BASE + "Road_TwoLane_20m.prefab", roadParent.transform,
                  new Vector3(off, 0f,  ROAD_DIST), Quaternion.Euler(0, 90, 0), "Road_N_" + i);
            Place(ROAD_BASE + "Road_TwoLane_20m.prefab", roadParent.transform,
                  new Vector3(off, 0f, -ROAD_DIST), Quaternion.Euler(0, 90, 0), "Road_S_" + i);
            Place(ROAD_BASE + "Road_TwoLane_20m.prefab", roadParent.transform,
                  new Vector3( ROAD_DIST, 0f, off), Quaternion.identity, "Road_E_" + i);
            Place(ROAD_BASE + "Road_TwoLane_20m.prefab", roadParent.transform,
                  new Vector3(-ROAD_DIST, 0f, off), Quaternion.identity, "Road_W_" + i);
        }

        // Corners
        Place(ROAD_BASE + "RoadUnit_Corner.prefab", roadParent.transform,
              new Vector3( ROAD_DIST, 0f,  ROAD_DIST), Quaternion.Euler(0, 180, 0), "Corner_NE");
        Place(ROAD_BASE + "RoadUnit_Corner.prefab", roadParent.transform,
              new Vector3(-ROAD_DIST, 0f,  ROAD_DIST), Quaternion.Euler(0,  90, 0), "Corner_NW");
        Place(ROAD_BASE + "RoadUnit_Corner.prefab", roadParent.transform,
              new Vector3( ROAD_DIST, 0f, -ROAD_DIST), Quaternion.Euler(0, 270, 0), "Corner_SE");
        Place(ROAD_BASE + "RoadUnit_Corner.prefab", roadParent.transform,
              new Vector3(-ROAD_DIST, 0f, -ROAD_DIST), Quaternion.identity,          "Corner_SW");

        // Sidewalks (3 per side, same offsets as roads)
        for (int i = -1; i <= 1; i++)
        {
            float off = i * 20f;
            Place(ROAD_BASE + "Sidewalk_Default_20m.prefab", roadParent.transform,
                  new Vector3(off, 0f,  SWALK_DIST), Quaternion.Euler(0, 90, 0), "SW_N_" + i);
            Place(ROAD_BASE + "Sidewalk_Default_20m.prefab", roadParent.transform,
                  new Vector3(off, 0f, -SWALK_DIST), Quaternion.Euler(0, 90, 0), "SW_S_" + i);
            Place(ROAD_BASE + "Sidewalk_Default_20m.prefab", roadParent.transform,
                  new Vector3( SWALK_DIST, 0f, off), Quaternion.identity, "SW_E_" + i);
            Place(ROAD_BASE + "Sidewalk_Default_20m.prefab", roadParent.transform,
                  new Vector3(-SWALK_DIST, 0f, off), Quaternion.identity, "SW_W_" + i);
        }

        // Street lights (one per road segment, at mid-segment)
        for (int i = -1; i <= 1; i++)
        {
            float off = i * 20f;
            Place(LIGHT_PATH, roadParent.transform,
                  new Vector3(off, 0f,  LIGHT_DIST), Quaternion.Euler(0,  90, 0), "SL_N_" + i);
            Place(LIGHT_PATH, roadParent.transform,
                  new Vector3(off, 0f, -LIGHT_DIST), Quaternion.Euler(0, 270, 0), "SL_S_" + i);
            Place(LIGHT_PATH, roadParent.transform,
                  new Vector3( LIGHT_DIST, 0f, off), Quaternion.identity,          "SL_E_" + i);
            Place(LIGHT_PATH, roadParent.transform,
                  new Vector3(-LIGHT_DIST, 0f, off), Quaternion.Euler(0, 180, 0),  "SL_W_" + i);
        }

        // Bus stop on north side
        Place(BUSSTOP, roadParent.transform,
              new Vector3(10f, 0f, SWALK_DIST), Quaternion.Euler(0, 270, 0), "BusStop_N");

        // Crosswalk markings at all 4 corners (on the road surface at corner midpoints)
        Place(TLAB_BASE + CROSSWALK, roadParent.transform,
              new Vector3( ROAD_DIST - 5f, 0f,  ROAD_DIST), Quaternion.Euler(0,  90, 0), "Crosswalk_NE_N");
        Place(TLAB_BASE + CROSSWALK, roadParent.transform,
              new Vector3( ROAD_DIST, 0f,  ROAD_DIST - 5f), Quaternion.identity,          "Crosswalk_NE_E");
        Place(TLAB_BASE + CROSSWALK, roadParent.transform,
              new Vector3(-ROAD_DIST + 5f, 0f,  ROAD_DIST), Quaternion.Euler(0,  90, 0), "Crosswalk_NW_N");
        Place(TLAB_BASE + CROSSWALK, roadParent.transform,
              new Vector3(-ROAD_DIST, 0f,  ROAD_DIST - 5f), Quaternion.identity,          "Crosswalk_NW_W");
        Place(TLAB_BASE + CROSSWALK, roadParent.transform,
              new Vector3( ROAD_DIST - 5f, 0f, -ROAD_DIST), Quaternion.Euler(0,  90, 0), "Crosswalk_SE_S");
        Place(TLAB_BASE + CROSSWALK, roadParent.transform,
              new Vector3( ROAD_DIST, 0f, -ROAD_DIST + 5f), Quaternion.identity,          "Crosswalk_SE_E");
        Place(TLAB_BASE + CROSSWALK, roadParent.transform,
              new Vector3(-ROAD_DIST + 5f, 0f, -ROAD_DIST), Quaternion.Euler(0,  90, 0), "Crosswalk_SW_S");
        Place(TLAB_BASE + CROSSWALK, roadParent.transform,
              new Vector3(-ROAD_DIST, 0f, -ROAD_DIST + 5f), Quaternion.identity,          "Crosswalk_SW_W");

        // Traffic lights at all 4 corners
        Place(TLAB_BASE + TRAFFIC_XRD, roadParent.transform,
              new Vector3( ROAD_DIST + 1f, 0f,  ROAD_DIST + 1f), Quaternion.Euler(0, 225, 0), "TrafficLight_NE");
        Place(TLAB_BASE + TRAFFIC_XRD, roadParent.transform,
              new Vector3(-ROAD_DIST - 1f, 0f,  ROAD_DIST + 1f), Quaternion.Euler(0, 135, 0), "TrafficLight_NW");
        Place(TLAB_BASE + TRAFFIC_XRD, roadParent.transform,
              new Vector3( ROAD_DIST + 1f, 0f, -ROAD_DIST - 1f), Quaternion.Euler(0, 315, 0), "TrafficLight_SE");
        Place(TLAB_BASE + TRAFFIC_XRD, roadParent.transform,
              new Vector3(-ROAD_DIST - 1f, 0f, -ROAD_DIST - 1f), Quaternion.Euler(0,  45, 0), "TrafficLight_SW");

        // Vending machines – one pair on the north sidewalk, one on the west
        Place(TLAB_BASE + VENDING,  roadParent.transform,
              new Vector3(-8f, 0f,  SWALK_DIST + 0.5f), Quaternion.Euler(0, 180, 0), "Vending_N_L");
        Place(TLAB_BASE + VEND_BOX, roadParent.transform,
              new Vector3(-6f, 0f,  SWALK_DIST + 0.5f), Quaternion.Euler(0, 180, 0), "VendingBox_N_L");
        Place(TLAB_BASE + VENDING,  roadParent.transform,
              new Vector3( SWALK_DIST + 0.5f, 0f,  8f), Quaternion.Euler(0, 270, 0), "Vending_E_L");
        Place(TLAB_BASE + VEND_BOX, roadParent.transform,
              new Vector3( SWALK_DIST + 0.5f, 0f,  6f), Quaternion.Euler(0, 270, 0), "VendingBox_E_L");

        // Utility poles along the back of sidewalks (behind the sidewalk, between bldgs and road)
        float POLE_DIST = LIGHT_DIST + 1.5f; // just behind street lights
        for (int i = -1; i <= 1; i++)
        {
            float off = i * 20f;
            Place(TLAB_BASE + UTIL_POLE, roadParent.transform,
                  new Vector3(off, 0f,  POLE_DIST), Quaternion.identity, "Pole_N_" + i);
            Place(TLAB_BASE + UTIL_POLE, roadParent.transform,
                  new Vector3(off, 0f, -POLE_DIST), Quaternion.identity, "Pole_S_" + i);
            Place(TLAB_BASE + UTIL_POLE, roadParent.transform,
                  new Vector3( POLE_DIST, 0f, off), Quaternion.identity, "Pole_E_" + i);
            Place(TLAB_BASE + UTIL_POLE, roadParent.transform,
                  new Vector3(-POLE_DIST, 0f, off), Quaternion.identity, "Pole_W_" + i);
        }

        // ─── BUILDINGS (FUNSET) ─────────────────────────
        var bldgParent = new GameObject("Buildings");
        bldgParent.transform.SetParent(env.transform, false);

        // Varied building types per side for visual interest
        string[] northTypes = { "ms_h03.prefab",    "ms_h02.prefab",    "ms_h01_1a.prefab", "ms_h01_1b.prefab" };
        string[] southTypes = { "ms_h01_1b.prefab", "ms_h03.prefab",    "ms_h02.prefab",    "ms_h01_1a.prefab" };
        string[] eastTypes  = { "ms_h02.prefab",    "ms_h01_1a.prefab", "ms_h03.prefab",    "ms_h01_1b.prefab" };
        string[] westTypes  = { "ms_h01_1a.prefab", "ms_h01_1b.prefab", "ms_h02.prefab",    "ms_h03.prefab"  };

        for (int i = 0; i < 4; i++)
        {
            float off = -30f + i * 20f;
            Place(FUN_BASE + northTypes[i], bldgParent.transform,
                  new Vector3(off, 0f,  BLDG_DIST), Quaternion.Euler(0, 180, 0), "Bldg_N_" + i);
            Place(FUN_BASE + southTypes[i], bldgParent.transform,
                  new Vector3(off, 0f, -BLDG_DIST), Quaternion.identity,          "Bldg_S_" + i);
            Place(FUN_BASE + eastTypes[i],  bldgParent.transform,
                  new Vector3( BLDG_DIST, 0f, off), Quaternion.Euler(0, 270, 0),  "Bldg_E_" + i);
            Place(FUN_BASE + westTypes[i],  bldgParent.transform,
                  new Vector3(-BLDG_DIST, 0f, off), Quaternion.Euler(0,  90, 0),  "Bldg_W_" + i);
        }

        // Corner filler buildings
        Place(FUN_BASE + "ms_small_1.prefab", bldgParent.transform,
              new Vector3( BLDG_DIST, 0f,  BLDG_DIST), Quaternion.Euler(0, 225, 0), "Bldg_Corner_NE");
        Place(FUN_BASE + "ms_small_2.prefab", bldgParent.transform,
              new Vector3(-BLDG_DIST, 0f,  BLDG_DIST), Quaternion.Euler(0, 135, 0), "Bldg_Corner_NW");
        Place(FUN_BASE + "ms_small_1.prefab", bldgParent.transform,
              new Vector3( BLDG_DIST, 0f, -BLDG_DIST), Quaternion.Euler(0, 315, 0), "Bldg_Corner_SE");
        Place(FUN_BASE + "ms_small_2.prefab", bldgParent.transform,
              new Vector3(-BLDG_DIST, 0f, -BLDG_DIST), Quaternion.Euler(0,  45, 0), "Bldg_Corner_SW");

        EditorUtility.SetDirty(env);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[DogWorldMapBuilder] Map rebuilt successfully!");
    }

    // ─────────────────────────────────────────────────────
    [MenuItem("DogWorld/Clear Map")]
    public static void ClearMap()
    {
        var env = GameObject.Find("DogWorld/Environment");
        if (env == null) return;
        for (int i = env.transform.childCount - 1; i >= 0; i--)
        {
            var child = env.transform.GetChild(i);
            if (child.name == "Ground") continue; // 地面は削除しない
            Object.DestroyImmediate(child.gameObject);
        }
        Debug.Log("[DogWorldMapBuilder] Map cleared.");
    }

    static GameObject Place(string path, Transform parent, Vector3 pos, Quaternion rot, string label)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) { Debug.LogWarning("Missing: " + path); return null; }
        var go = path.EndsWith(".prefab")
            ? (GameObject)PrefabUtility.InstantiatePrefab(asset, parent)
            : Object.Instantiate(asset, parent);
        if (go == null) return null;
        go.transform.localPosition = pos;
        go.transform.localRotation = rot;
        go.name = label;
        return go;
    }
}
