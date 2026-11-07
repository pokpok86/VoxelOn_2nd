using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    readonly string WeaponArtVersion=Array.IndexOf(Environment.GetCommandLineArgs(),"--weapon-art-v001")>=0?"v001":"v003";
    static readonly string[] EquippedArtIds={"Military_AR","Military_GL","Military_RL","Future_Sniper"};
    [Serializable] sealed class WeaponArtEntry { public string id,title,family,kind,resource;public int version,vertexCount,meshCount;public float[] min,max; }
    [Serializable] sealed class WeaponArtCatalog {public WeaponArtEntry[] models;}
    WeaponArtCatalog weaponArtCatalog;
    bool weaponGalleryOpen;int weaponPreviewIndex;Vector2 weaponGalleryScroll;
    GameObject weaponStudioRoot,weaponPreviewModel;Camera weaponStudioCamera;RenderTexture weaponPreviewTexture;float weaponPreviewYaw;Quaternion weaponPreviewRestRotation;
    readonly Dictionary<Material,Material> weaponArtMaterials=new Dictionary<Material,Material>();
    WeaponArtCatalog WeaponCatalog()
    {
        if(weaponArtCatalog==null){TextAsset data=Resources.Load<TextAsset>("PDParts/Weapons20261105/catalog_"+WeaponArtVersion);if(data==null)throw new InvalidOperationException("Weapon catalog missing");weaponArtCatalog=JsonUtility.FromJson<WeaponArtCatalog>(data.text);}
        return weaponArtCatalog;
    }
    string WeaponArtResource(string id)
    {
        foreach(WeaponArtEntry e in WeaponCatalog().models)if(e.id==id)return e.resource;
        if(id!=null&&id.StartsWith("CUSTOM_",StringComparison.Ordinal))
            foreach(WeaponArtEntry e in WeaponCatalog().models)if(e.id=="Military_AR")return e.resource;
        throw new InvalidOperationException("Unknown weapon art "+id);
    }
    static Transform FindArtSocket(Transform root,string socket)
    {
        foreach(Transform t in root.GetComponentsInChildren<Transform>(true))if(t.name.EndsWith("__"+socket,StringComparison.Ordinal))return t;
        return null;
    }
    void PrepareWeaponMaterials(Transform root)
    {
        foreach(Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] source=renderer.sharedMaterials;
            for(int i=0;i<source.Length;i++)
            {
                Material original=source[i];if(original==null)continue;
                if(!weaponArtMaterials.TryGetValue(original,out Material copy))
                {
                    copy=new Material(original);WeaponArtSurface.Configure(copy);weaponArtMaterials.Add(original,copy);
                }
                source[i]=copy;
            }
            renderer.sharedMaterials=source;
        }
    }
    void EnsureWeaponStudio()
    {
        if(weaponStudioRoot!=null)return;
        weaponStudioRoot=new GameObject("Weapon preview isolated studio");weaponStudioRoot.transform.position=new Vector3(10000,100,0);
        weaponPreviewTexture=new RenderTexture(768,512,24);weaponPreviewTexture.Create();
        weaponStudioCamera=new GameObject("Weapon preview camera").AddComponent<Camera>();weaponStudioCamera.transform.SetParent(weaponStudioRoot.transform,false);weaponStudioCamera.transform.localPosition=new Vector3(-5,2.5f,8);weaponStudioCamera.transform.LookAt(weaponStudioRoot.transform.position+Vector3.forward*.12f);
        weaponStudioCamera.orthographic=true;weaponStudioCamera.orthographicSize=1.2f;weaponStudioCamera.aspect=1.5f;weaponStudioCamera.cullingMask=1<<10;weaponStudioCamera.clearFlags=CameraClearFlags.SolidColor;weaponStudioCamera.backgroundColor=new Color(.045f,.06f,.085f);weaponStudioCamera.targetTexture=weaponPreviewTexture;weaponStudioCamera.enabled=false;
        view.cullingMask&=~(1<<10);
        foreach(Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))light.cullingMask&=~(1<<10);
        for(int i=0;i<2;i++)
        {
            Light light=new GameObject("Weapon studio light "+i).AddComponent<Light>();light.transform.SetParent(weaponStudioRoot.transform,false);light.type=LightType.Directional;light.cullingMask=1<<10;light.intensity=i==0?1.1f:.55f;light.transform.localRotation=Quaternion.Euler(i==0?35:25,i==0?-35:130,0);light.shadows=LightShadows.None;
        }
    }
    void ShowWeaponPreview(int index,string legacy=null,GameObject supplied=null)
    {
        EnsureWeaponStudio();WeaponArtEntry[] entries=WeaponCatalog().models;weaponPreviewIndex=Mathf.Clamp(index,0,entries.Length-1);
        if(weaponPreviewModel!=null){weaponPreviewModel.SetActive(false);Destroy(weaponPreviewModel);}
        string path=legacy??entries[weaponPreviewIndex].resource;
        GameObject prefab=supplied!=null?supplied:Resources.Load<GameObject>(path);if(prefab==null)throw new InvalidOperationException("Missing preview "+path);
        weaponPreviewModel=Instantiate(prefab,weaponStudioRoot.transform,false);weaponPreviewModel.transform.localPosition=Vector3.zero;
        foreach(Transform t in weaponPreviewModel.GetComponentsInChildren<Transform>(true))t.gameObject.layer=10;
        if(supplied!=null)ApplySimpleShellMaterials(weaponPreviewModel.transform);else if(legacy==null)PrepareWeaponMaterials(weaponPreviewModel.transform);
        weaponPreviewRestRotation=prefab.transform.localRotation;
        weaponPreviewYaw=0;RenderWeaponPreview();
    }
    void RenderWeaponPreview()
    {
        weaponPreviewModel.transform.localRotation=Quaternion.Euler(0,weaponPreviewYaw,0)*weaponPreviewRestRotation;
        weaponStudioCamera.Render();
    }
    string GalleryEquipmentStatus(string id)
    {
        var slots=new List<string>();
        if(player!=null&&player.weaponArtIds!=null)
            for(int i=0;i<player.weaponArtIds.Length;i++)if(player.weaponArtIds[i]==id)slots.Add((i+1).ToString());
        return slots.Count==0?"ART PREVIEW / NOT EQUIPPED ON CURRENT FRAME":"CURRENT FRAME / EQUIPPED SLOTS "+string.Join(", ",slots);
    }
    void CloseWeaponGallery(){weaponGalleryOpen=false;}
    void DrawWeaponGallery()
    {
        Panel(new Rect(0,0,1280,720),new Color(.025f,.045f,.075f,.99f));Text(30,22,1000,"WEAPON ART LIBRARY / BLENDER",title);
        if(GUI.Button(new Rect(1110,22,140,35),"BACK / ESC")){CloseWeaponGallery();return;}
        WeaponArtEntry[] entries=WeaponCatalog().models;
        weaponGalleryScroll=GUI.BeginScrollView(new Rect(25,85,310,580),weaponGalleryScroll,new Rect(0,0,285,entries.Length*36));
        for(int i=0;i<entries.Length;i++)if(GUI.Button(new Rect(0,i*36,280,32),(i==weaponPreviewIndex?"> ":"")+entries[i].id.Replace('_',' ')))ShowWeaponPreview(i);
        GUI.EndScrollView();
        if(weaponPreviewTexture!=null)GUI.DrawTexture(new Rect(350,90,900,485),weaponPreviewTexture,ScaleMode.ScaleToFit);
        WeaponArtEntry selected=entries[weaponPreviewIndex];
        Text(365,578,880,selected.id+" / "+selected.family+" / "+WeaponArtVersion,label);
        Text(365,611,880,GalleryEquipmentStatus(selected.id),small);
        Text(365,640,880,"Weapon art does not change damage, reload, energy or combat rules.",small);
        if(GUI.Button(new Rect(900,668,150,32),"ROTATE LEFT")){weaponPreviewYaw-=30;RenderWeaponPreview();}
        if(GUI.Button(new Rect(1060,668,150,32),"ROTATE RIGHT")){weaponPreviewYaw+=30;RenderWeaponPreview();}
    }
    void DisposeWeaponGallery()
    {
        if(weaponStudioRoot!=null)Destroy(weaponStudioRoot);if(weaponPreviewTexture!=null){weaponPreviewTexture.Release();Destroy(weaponPreviewTexture);}
        foreach(Material m in weaponArtMaterials.Values)if(m!=null)Destroy(m);
    }
    IEnumerator CaptureWeaponLibrary()
    {
        string[] args=Environment.GetCommandLineArgs();bool legacy=Array.IndexOf(args,"--legacy-weapon-art")>=0;
        string dir=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","Weapons-20261105",legacy?"Before":"Unity-"+WeaponArtVersion+"-EquipmentFinal");Directory.CreateDirectory(dir);
        int checks=0,captures=0;bool pass=true;void Check(bool ok,string name){checks++;pass&=ok;Debug.Log("WEAPON_ART_CHECK "+name+" "+(ok?"PASS":"FAIL"));}
        WeaponArtEntry[] entries=WeaponCatalog().models;
        Check(entries.Length==32,"catalog contains 32 distinct model IDs");var ids=new HashSet<string>();
        foreach(WeaponArtEntry entry in entries)
        {
            if(legacy&&Array.IndexOf(EquippedArtIds,entry.id)<0)continue;
            int index=Array.IndexOf(entries,entry);string[] old={"WeaponRifle","WeaponCannon","WeaponRocket","WeaponRail"};int equipped=Array.IndexOf(EquippedArtIds,entry.id);
            ShowWeaponPreview(index,legacy?"PDParts/"+old[equipped]:null);yield return null;yield return new WaitForEndOfFrame();weaponStudioCamera.Render();
            if(!legacy)
            {
                Check(ids.Add(entry.id)&&entry.vertexCount>8&&entry.vertexCount<15000,"unique ID and geometry budget "+entry.id);
                Check(FindArtSocket(weaponPreviewModel.transform,"GRIP_MAIN")!=null,"grip socket "+entry.id);
                Bounds bounds=new Bounds();bool first=true;bool valid=true;int vertices=0;
                foreach(MeshFilter mesh in weaponPreviewModel.GetComponentsInChildren<MeshFilter>()){vertices+=mesh.sharedMesh.vertexCount;valid&=mesh.sharedMesh.isReadable;}
                foreach(Renderer r in weaponPreviewModel.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
                Check(valid&&!first&&Finite(bounds.size)&&bounds.size.magnitude>.1f&&bounds.size.magnitude<5&&vertices>8,"readable finite meter-scale import "+entry.id+" "+bounds.size.ToString("F3"));
                if(entry.kind=="firearm")
                {
                    Transform muzzle=FindArtSocket(weaponPreviewModel.transform,"MUZZLE");Transform grip=FindArtSocket(weaponPreviewModel.transform,"GRIP_MAIN");
                    Check(muzzle!=null&&Vector3.Dot(muzzle.position-grip.position,Vector3.forward)>.5f,"forward muzzle axis "+entry.id);
                }
            }
            SaveWeaponImage(Path.Combine(dir,entry.id+".png"));captures++;
        }
        if(!legacy)
        {
            for(int frame=0;frame<3;frame++)
            {
                TestSetupSelectable(frame,(frame+1)%3);smoke=false;inMenu=false;muted=true;targetLock=false;aimDirection=Vector3.forward;
                player.root.position=new Vector3(80,FighterGroundY,70);player.facing=player.chassisForward=Vector3.forward;enemy.root.position=player.root.position+Vector3.forward*30;
                foreach(Transform t in player.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;
                Animate(player);yield return null;yield return new WaitForEndOfFrame();
                for(int i=0;i<4;i++)
                {
                    player.weaponSlot=i+1;Animate(player);Vector3 muzzle=player.weaponMuzzlePoints[i].position;
                    player.weaponTimer=player.reloadTime=player.lockTime=player.rushTime=0;player.magazine=player.weaponProfiles[i].capacity;int shotCount=shots.Count;
                    FireEquippedWeapon(player,enemy,Vector3.forward);
                    Check(shots.Count==shotCount+1&&Vector3.Distance(shots[shots.Count-1].visual.position,muzzle)<.001f&&player.magazine==player.weaponProfiles[i].capacity-1,"live projectile and independent ammo slot "+i+" frame "+frame);
                    if(i<2&&player.gripRig[i]!=null)Check(Vector3.Distance(player.weaponGripPoints[i].position,HandGripPosition(player.gripRig[i]))<.01f,"hand socket alignment "+i+" frame "+frame);
                }
                player.weaponSlot=1;Animate(player);
                for(int side=0;side<2;side++)
                {
                    Transform joint=side==0?player.leftArm:player.rightArm,mount=player.weaponVisuals[side+2];Quaternion rest=joint.rotation,oldRotation=mount.rotation;Vector3 oldOffset=mount.position-joint.position;
                    joint.rotation=Quaternion.AngleAxis(30,Vector3.up)*rest;
                    Check(mount.parent==joint&&Quaternion.Angle(oldRotation,mount.rotation)>29&&Vector3.Distance(mount.position-joint.position,Quaternion.AngleAxis(30,Vector3.up)*oldOffset)<.002f,"shoulder mount directly inherits joint rotation side "+side+" frame "+frame);joint.rotation=rest;
                }
                player.weaponSlot=4;Animate(player);Check(Vector3.Dot(player.weaponVisuals[3].forward,Vector3.forward)>.999f,"selected shoulder barrel aligns with actual requested aim frame "+frame);
                player.weaponTimer=player.reloadTime=player.lockTime=player.rushTime=0;player.magazine=4;
                Bounds sideCover=new Bounds(player.root.position+Vector3.right*(player.Radius+.60f),new Vector3(1,8,8));obstacles.Add(sideCover);Animate(player);
                int previousShots=shots.Count;FireEquippedWeapon(player,enemy,Vector3.forward);
                Check(shots.Count==previousShots+1,"rail remains usable beside nonintersecting side cover frame "+frame);obstacles.RemoveAt(obstacles.Count-1);
                Bounds frontCover=new Bounds(player.root.position+Vector3.forward*.65f,new Vector3(8,8,.25f));obstacles.Add(frontCover);
                Shot through=Fire(player,Vector3.forward,1,100,.2f,30,loadoutVisual:true);
                Check(through==null,"front cover still blocks the real barrel frame "+frame);obstacles.RemoveAt(obstacles.Count-1);
                Camera camera=weaponStudioCamera;int mask=camera.cullingMask;Vector3 position=camera.transform.position;Quaternion rotation=camera.transform.rotation;float size=camera.orthographicSize;
                camera.cullingMask=1<<9;camera.orthographicSize=3.3f;camera.transform.position=player.root.position+new Vector3(6,3,9);camera.transform.LookAt(player.root.position+Vector3.up*.6f);camera.Render();SaveWeaponImage(Path.Combine(dir,"Mounted-"+(frame+1)+".png"));captures++;
                camera.cullingMask=mask;camera.orthographicSize=size;camera.transform.SetPositionAndRotation(position,rotation);
            }
            inMenu=true;weaponGalleryOpen=true;ShowWeaponPreview(0);yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,"Gallery-UI.png"));captures++;yield return null;yield return null;
        }
        Debug.Log((pass?"WEAPON_LIBRARY_CAPTURE_PASS ":"WEAPON_LIBRARY_CAPTURE_FAIL ")+checks+" checks; "+captures+" images");Application.Quit(pass?0:2);
    }
    void SaveWeaponImage(string path)
    {
        RenderTexture previous=RenderTexture.active;RenderTexture.active=weaponPreviewTexture;Texture2D image=new Texture2D(768,512,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,768,512),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=previous;Destroy(image);
    }
}
