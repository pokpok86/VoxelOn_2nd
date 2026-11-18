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
        string[] droneKinds={"ATTACK","INTERCEPT","JAMMER"};arena.droneBackpackSources=new GameObject[3];arena.droneModelSources=new GameObject[3];arena.droneDockedPreviewSources=new GameObject[3];
        for(int i=0;i<3;i++){string stem="Assets/Art/DroneBackpacks/Concept_v003/VXDR_"+droneKinds[i]+"_v003";arena.droneBackpackSources[i]=AssetDatabase.LoadAssetAtPath<GameObject>(stem+"_BACKPACK.fbx");arena.droneModelSources[i]=AssetDatabase.LoadAssetAtPath<GameObject>(stem+"_DRONE.fbx");arena.droneDockedPreviewSources[i]=AssetDatabase.LoadAssetAtPath<GameObject>(stem+"_DOCKED.fbx");if(arena.droneBackpackSources[i]==null||arena.droneModelSources[i]==null||arena.droneDockedPreviewSources[i]==null)throw new Exception("Drone art missing "+stem);}

        arena.auxiliaryModelSources=new[]{AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/AuxiliaryWeapons/v002/VXAUX_MISSILE_POD_v002.fbx"),AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/AuxiliaryWeapons/v002/VXAUX_PULSE_GUN_v002.fbx")};
        arena.auxiliaryProjectileSources=new[]{AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/AuxiliaryWeapons/v002/VXAUX_HOMING_MISSILE_v002.fbx"),AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/AuxiliaryWeapons/v002/VXAUX_PULSE_BOLT_v002.fbx")};
        arena.auxiliaryBracketSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/AuxiliaryWeapons/v002/VXAUX_MOUNT_BRACKET_v002.fbx");if(arena.auxiliaryBracketSource==null)throw new Exception("Auxiliary bracket missing");
        foreach(GameObject asset in arena.auxiliaryModelSources)if(asset==null)throw new Exception("Auxiliary model missing");foreach(GameObject asset in arena.auxiliaryProjectileSources)if(asset==null)throw new Exception("Auxiliary projectile missing");
        string[] effectIds={"MUZZLE","MISSILE_BURST","INTERCEPT","JAM_FIELD","THRUSTER"};arena.combatEffectSources=new GameObject[effectIds.Length];for(int i=0;i<effectIds.Length;i++){arena.combatEffectSources[i]=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/CombatEffects/v002/VXFX_"+effectIds[i]+"_v002.fbx");if(arena.combatEffectSources[i]==null)throw new Exception("Combat FX missing "+effectIds[i]);}
        arena.skyTextureShader=Shader.Find("VoxelOn/SkyTint");if(arena.skyTextureShader==null)throw new Exception("Sky shader unavailable");
        arena.shallowWaterShader=Shader.Find("VoxelOn/ShallowWater");if(arena.shallowWaterShader==null)throw new Exception("Shallow water shader unavailable");
        arena.cameraFadeShader=Shader.Find("VoxelOn/CameraHeadFade");
        if(arena.cameraFadeShader==null)throw new Exception("Camera head fade shader unavailable");
        if (arena.hullShader == null) throw new Exception("Standard shader unavailable");
        var modelImporter=AssetImporter.GetAtPath("Assets/Art/Robots/WISP/WISP_v014.fbx") as ModelImporter;
        if(modelImporter!=null&&!modelImporter.isReadable) { modelImporter.isReadable=true;modelImporter.preserveHierarchy=true;modelImporter.optimizeGameObjects=false;modelImporter.SaveAndReimport(); }
        arena.referenceHandsBaseline=new[]{AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/WISP/WISP_v012.fbx"),AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/BASTION/BASTION_v007.fbx"),AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/TARANTULA/TARANTULA_v006.fbx")};
        arena.gloveReviewBaseline=new[]{AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/WISP/WISP_v011.fbx"),AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/BASTION/BASTION_v006.fbx"),AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/TARANTULA/TARANTULA_v005.fbx")};
        arena.wispModelSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/WISP/WISP_v014.fbx");
        foreach(string name in new[]{"BASTION","TARANTULA"})
        {
            string path="Assets/Art/Robots/"+name+"/"+name+(name=="BASTION"?"_v010.fbx":"_v008.fbx");
            var importer=AssetImporter.GetAtPath(path) as ModelImporter;
            if(importer!=null&&(!importer.isReadable||!importer.preserveHierarchy)){importer.isReadable=true;importer.preserveHierarchy=true;importer.optimizeGameObjects=false;importer.SaveAndReimport();}
        }
        foreach(string name in new[]{"BASTION","TARANTULA"})
        {
            string folder="Assets/Art/Robots/"+name;string version=name=="BASTION"?"_v010":"_v008";string path=folder+"/"+name+version+".prefab";
            if(AssetDatabase.LoadAssetAtPath<GameObject>(path)==null){GameObject source=AssetDatabase.LoadAssetAtPath<GameObject>(folder+"/"+name+version+".fbx");if(source!=null){GameObject instance=UnityEngine.Object.Instantiate(source);instance.name=name+version;PrefabUtility.SaveAsPrefabAsset(instance,path);UnityEngine.Object.DestroyImmediate(instance);}}
        }
        arena.bastionModelSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/BASTION/BASTION_v010.fbx");
        arena.tarantulaModelSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/TARANTULA/TARANTULA_v008.fbx");
        arena.wispSwordSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/WISP/WISP_Sword_v001.fbx");
        arena.wispShieldSource=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/WISP/WISP_Shield_v003.fbx");
        arena.wispBaseColorTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Robots/WISP/Textures/WISP_BaseColor_v004.png");
        arena.wispEmissionTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Robots/WISP/Textures/WISP_Emission_v004.png");
        if(arena.wispModelSource==null) Debug.LogWarning("WISP FBX is not installed; procedural WISP fallback will be used.");
        foreach(string name in new[]{"PlainsTerrain","NaturalRock"})
        {
            var importer=AssetImporter.GetAtPath("Assets/Resources/PDParts/"+name+".fbx") as ModelImporter;
            if(importer!=null&&!importer.isReadable){importer.isReadable=true;importer.SaveAndReimport();}
        }
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Arena.unity");
        EditorBuildSettings.scenes = new [] { new EditorBuildSettingsScene("Assets/Scenes/Arena.unity", true) };
        PlayerSettings.productName = "Voxel-On";
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
            locationPathName = "Builds/Windows/Voxel-On.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Windows build failed: " + report.summary.result);
        Debug.Log("VOXSKY_BUILD_PASS " + report.summary.totalSize + " bytes");
    }
}
