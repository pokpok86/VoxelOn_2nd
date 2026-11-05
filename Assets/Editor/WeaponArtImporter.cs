using UnityEditor;
using UnityEngine;
public sealed class WeaponArtImporter:AssetPostprocessor
{
    bool IsWeapon=>assetPath.StartsWith("Assets/Resources/PDParts/Weapons20261105/");
    void OnPreprocessModel(){if(!IsWeapon)return;ModelImporter importer=(ModelImporter)assetImporter;importer.isReadable=true;importer.preserveHierarchy=true;importer.importAnimation=false;importer.optimizeGameObjects=false;}
    void OnPostprocessMaterial(Material material){if(IsWeapon)WeaponArtSurface.Configure(material);}
}
