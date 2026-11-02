using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VoxBuild
{
    [MenuItem("VoxSky/Create prototype scene")]
    public static void CreateScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var arena = new GameObject("VoxSky Arena").AddComponent<VoxArena>();
        arena.hullShader = Shader.Find("Standard");
        if (arena.hullShader == null) throw new Exception("Standard shader unavailable");
        arena.wispModelSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/WISP/WISP_v001.fbx");
        arena.wispSwordSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/WISP/WISP_Sword_v001.fbx");
        arena.wispShieldSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/WISP/WISP_Shield_v001.fbx");
        arena.wispBaseColorTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Robots/WISP/Textures/WISP_BaseColor.png");
        arena.wispEmissionTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Robots/WISP/Textures/WISP_Emission.png");
        if(arena.wispModelSource==null) Debug.LogWarning("WISP FBX is not installed; procedural WISP fallback will be used.");
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Arena.unity");
        EditorBuildSettings.scenes = new [] { new EditorBuildSettingsScene("Assets/Scenes/Arena.unity", true) };
        PlayerSettings.productName = "VoxSky";
        PlayerSettings.companyName = "VoxSky";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        Debug.Log("VOXSKY_SCENE_READY WISP_FBX="+(arena.wispModelSource!=null));
    }

    public static void BuildWindows()
    {
        CreateScene();
        Directory.CreateDirectory("Builds/Windows");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new [] { "Assets/Scenes/Arena.unity" },
            locationPathName = "Builds/Windows/VoxSky.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Windows build failed: " + report.summary.result);
        Debug.Log("VOXSKY_BUILD_PASS " + report.summary.totalSize + " bytes");
    }
}
