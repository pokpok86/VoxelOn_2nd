using System.Collections;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    bool legacyProjectileReview;
    static readonly string[] ProjectileArtNames={"ProjectileRifle_v002","ProjectileCannon_v002","ProjectileRocket_v002","ProjectileRail_v002"};
    Transform BuildImportedProjectile(Transform root,Shot shot,Color shell,Color core)
    {
        Transform art=SpawnPDPart(ProjectileArtNames[shot.artSlot],root,Vector3.zero,Vector3.one*shot.radius);SetPartMaterial(art,fxSolid);
        foreach(Renderer renderer in art.GetComponentsInChildren<Renderer>())
        {
            bool bright=renderer.name.Contains("Core");fxProperties.Clear();fxProperties.SetColor("_Color",bright?core:shell);fxProperties.SetColor("_EmissionColor",bright?core*2:Color.black);renderer.SetPropertyBlock(fxProperties);
        }
        return root;
    }
    bool AuthoredProjectileFits(Shot shot)
    {
        bool authored=false;foreach(Transform child in shot.visual)if(child.name=="Blender PD / "+ProjectileArtNames[shot.artSlot])authored=true;
        if(!authored)return false;
        foreach(MeshFilter filter in shot.visual.GetComponentsInChildren<MeshFilter>())
        {
            Bounds b=filter.sharedMesh.bounds;
            for(int i=0;i<8;i++)
            {
                Vector3 p=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));p=shot.visual.InverseTransformPoint(filter.transform.TransformPoint(p));
                if(Mathf.Abs(p.x)>shot.radius*1.02f||Mathf.Abs(p.y)>shot.radius*1.02f)return false;
            }
        }
        return true;
    }
    IEnumerator CaptureProjectileGallery()
    {
        string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","PD14-Projectiles",legacyProjectileReview?"Before":"After");Directory.CreateDirectory(folder);
        TestSetupSelectable(0,1);smoke=false;inMenu=false;paused=true;spectatorMode=false;player.facing=Vector3.forward;
        RenderTexture target=new RenderTexture(768,768,24);Texture2D image=new Texture2D(768,768,TextureFormat.RGB24,false);view.targetTexture=target;view.orthographic=true;view.orthographicSize=.8f;view.cullingMask=1<<9;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.045f,.06f,.08f);view.transform.position=new Vector3(3,1,3);view.transform.LookAt(Vector3.zero);
        for(int slot=0;slot<4;slot++)
        {
            foreach(Shot old in shots)old.visual.gameObject.SetActive(false);ClearCombat();player.weaponSlot=slot+1;player.weaponTimer=player.reloadTime=0;player.magazine=EquippedProfile(player).capacity;FireEquippedWeapon(player,enemy,Vector3.forward);
            if(shots.Count!=1){Debug.LogError("PROJECTILE_GALLERY_FAIL missing shot");Application.Quit(2);yield break;}
            Shot shot=shots[0];shot.visual.position=Vector3.zero;shot.visual.rotation=Quaternion.identity;foreach(Transform t in shot.visual.GetComponentsInChildren<Transform>())t.gameObject.layer=9;foreach(TrailRenderer t in shot.visual.GetComponentsInChildren<TrailRenderer>())t.enabled=false;
            view.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,768,768),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,(slot+1)+"-"+EquippedProfile(player).name+".png"),image.EncodeToPNG());yield return null;
        }
        RenderTexture.active=null;view.targetTexture=null;target.Release();Destroy(target);Destroy(image);Debug.Log("PROJECTILE_GALLERY_PASS 4 actual fired mesh closeups / "+(legacyProjectileReview?"before":"after"));Application.Quit(0);
    }
}
