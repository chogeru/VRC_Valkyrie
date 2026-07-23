using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.PostProcessing;

public static class DogWorldPostProcess
{
    [MenuItem("DogWorld/Setup Post-Processing")]
    public static void Setup()
    {
        // ── Create Profile Asset ──────────────────────────────────────
        var profile = ScriptableObject.CreateInstance<PostProcessProfile>();

        // Bloom – soft glow on bright surfaces & lights
        var bloom = profile.AddSettings<Bloom>();
        bloom.enabled.Override(true);
        bloom.intensity.Override(1.2f);
        bloom.threshold.Override(0.9f);
        bloom.softKnee.Override(0.5f);
        bloom.diffusion.Override(7f);
        bloom.anamorphicRatio.Override(0f);

        // Color Grading – warm, slightly cinematic tone
        var cg = profile.AddSettings<ColorGrading>();
        cg.enabled.Override(true);
        cg.gradingMode.Override(GradingMode.LowDefinitionRange);
        cg.tonemapper.Override(Tonemapper.Neutral);
        cg.temperature.Override(8f);      // slightly warm
        cg.tint.Override(1f);
        cg.postExposure.Override(0f);
        cg.saturation.Override(15f);      // richer colors
        cg.contrast.Override(10f);        // mild contrast

        // Vignette – very subtle
        var vig = profile.AddSettings<Vignette>();
        vig.enabled.Override(true);
        vig.intensity.Override(0.15f);
        vig.smoothness.Override(0.6f);
        vig.roundness.Override(1f);

        // Ambient Occlusion – subtle contact shadows
        var ao = profile.AddSettings<AmbientOcclusion>();
        ao.enabled.Override(true);
        ao.intensity.Override(0.5f);
        ao.radius.Override(0.4f);

        // ── Save Profile ──────────────────────────────────────────────
        string dir = "Assets/_Project/PostProcessing";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/_Project", "PostProcessing");

        string path = dir + "/DogWorldProfile.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(profile, path);
        AssetDatabase.SaveAssets();

        // ── Create Global Volume in Scene ─────────────────────────────
        var old = GameObject.Find("PostProcessVolume");
        if (old != null) Object.DestroyImmediate(old);

        var go = new GameObject("PostProcessVolume");
        go.layer = 0; // Default
        var vol = go.AddComponent<PostProcessVolume>();
        vol.isGlobal = true;
        vol.weight   = 1f;
        vol.priority = 1f;
        vol.sharedProfile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(path);

        // Ensure camera layer mask includes Default
        var ppLayer = Object.FindObjectOfType<PostProcessLayer>();
        if (ppLayer != null)
            ppLayer.volumeLayer = LayerMask.GetMask("Default", "Water");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[DogWorld] Post-processing applied!");
    }
}
