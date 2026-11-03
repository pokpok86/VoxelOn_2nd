using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;
public static class WispRigInspect {
 public static void Build(){Run();VoxBuild.BuildWindows();}
 public static void Run(){
 AssetDatabase.Refresh();
 var importer=(ModelImporter)AssetImporter.GetAtPath("Assets/Art/Robots/WISP/WISP_v003.fbx");
 importer.isReadable=true; importer.preserveHierarchy=true; importer.optimizeGameObjects=false; importer.SaveAndReimport();
 var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Robots/WISP/WISP_v003.fbx"));
 var s=new StringBuilder();
 foreach(var t in go.GetComponentsInChildren<Transform>(true))
  if(t.GetComponent<Renderer>()==null)s.AppendLine(t.name+" parent="+(t.parent!=null?t.parent.name:"")+" local="+t.localPosition.ToString("F4")+" rot="+t.localEulerAngles.ToString("F2")+" scale="+t.localScale);
 foreach(var r in go.GetComponentsInChildren<Renderer>(true))s.AppendLine("RENDER "+r.name+" "+r.GetType().Name+" "+r.bounds);
 File.WriteAllText("Logs/WISP-v003-UnityRig.txt",s.ToString());
 Debug.Log("WISP_V003_IMPORT_PASS "+go.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length+" skinned meshes");
 Object.DestroyImmediate(go);
 }
}

