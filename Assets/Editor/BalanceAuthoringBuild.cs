using System.IO;
using UnityEditor;
using UnityEngine;
public static class BalanceAuthoringBuild
{
    [MenuItem("Voxel-On/Build Windows with balance template")]
    public static void BuildWindows()
    {
        BalanceDocument defaults=BalanceData.Defaults();string root=Directory.GetCurrentDirectory();string template=Path.Combine(root,"Docs","Balance","PD-Balance-Template.json");
        string previousTemplate=File.Exists(template)?File.ReadAllText(template):null;
        string editable=Path.Combine(root,"Data","Balance","PD-Balance.json");
        // Refresh only our untouched generated template; preserve any producer edits.
        if(!File.Exists(editable)||previousTemplate!=null&&File.ReadAllText(editable)==previousTemplate)BalanceFiles.Save(editable,defaults);
        Directory.CreateDirectory(Path.GetDirectoryName(template));File.WriteAllText(template,JsonUtility.ToJson(defaults,true),new System.Text.UTF8Encoding(false));
        VoxBuild.BuildWindows();
    }
}
