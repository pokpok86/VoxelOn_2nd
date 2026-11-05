using UnityEngine;
using UnityEditor;
public static class ReferenceHandsBuild
{
    public static void BuildWindows()
    {
        foreach(string path in new[]{"Assets/Art/Robots/WISP/WISP_v014.fbx","Assets/Art/Robots/BASTION/BASTION_v009.fbx","Assets/Art/Robots/TARANTULA/TARANTULA_v008.fbx"})
        {
            ModelImporter importer=AssetImporter.GetAtPath(path) as ModelImporter;if(importer==null)throw new System.InvalidOperationException(path);importer.isReadable=true;importer.preserveHierarchy=true;importer.optimizeGameObjects=false;importer.importBlendShapes=true;importer.SaveAndReimport();
            GameObject prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);int vertices=0;
            foreach(SkinnedMeshRenderer skin in prefab.GetComponentsInChildren<SkinnedMeshRenderer>()){vertices+=skin.sharedMesh.vertexCount;if(skin.sharedMesh.blendShapeCount>0)for(int i=0;i<skin.sharedMesh.blendShapeCount;i++)Debug.Log("REFERENCE_HAND_IMPORT "+skin.name+" shape="+skin.sharedMesh.GetBlendShapeName(i)+" vertices="+skin.sharedMesh.vertexCount);}
            Debug.Log("REFERENCE_MODEL_IMPORT "+path+" skins="+prefab.GetComponentsInChildren<SkinnedMeshRenderer>().Length+" vertices="+vertices);
        }
        BalanceAuthoringBuild.BuildWindows();
    }
}
