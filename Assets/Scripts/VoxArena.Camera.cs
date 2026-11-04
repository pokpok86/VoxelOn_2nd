using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed partial class VoxArena
{
    bool mouseLookCaptured;
    int discardMouseLookFrames;
    float cameraBodyOpacity=1;
    bool cameraBodyFadeApplied;
    Material cameraBodyFadeMaterial;
    readonly Dictionary<Material,Material> cameraFadeMaterials=new Dictionary<Material,Material>();
    readonly Dictionary<Renderer,Material[]> cameraFadedArrays=new Dictionary<Renderer,Material[]>();
    readonly Dictionary<Renderer,Material[]> cameraFadeOriginals=new Dictionary<Renderer,Material[]>();
    void ResetMouseLookCapture(){mouseLookCaptured=false;discardMouseLookFrames=2;}
    void OnApplicationFocus(bool focused){ResetMouseLookCapture();}
    void ApplyMouseLook(float dx,float dy,bool captured)
    {
        if(!captured){ResetMouseLookCapture();return;}
        if(!mouseLookCaptured){mouseLookCaptured=true;discardMouseLookFrames=2;}
        if(discardMouseLookFrames>0){discardMouseLookFrames--;return;}
        if(float.IsNaN(dx)||float.IsNaN(dy)||float.IsInfinity(dx)||float.IsInfinity(dy))return;
        cameraYaw=Mathf.Repeat(cameraYaw+dx*mouseSensitivity,360);
        cameraPitch=Mathf.Clamp(cameraPitch-dy*mouseSensitivity,-5,45);
    }
    void UpdateThirdPersonCamera(float dt)
    {
        view.orthographic=false;
        if(!CanStartLockOn(player))targetLock=false;
        if(targetLock)
        {
            Vector3 toward=enemy.root.position-player.root.position;
            cameraYaw=Mathf.LerpAngle(cameraYaw,Mathf.Atan2(toward.x,toward.z)*Mathf.Rad2Deg,1-Mathf.Exp(-8*dt));
            cameraPitch=Mathf.Lerp(cameraPitch,DefaultThirdPersonPitch,1-Mathf.Exp(-8*dt));
        }
        Quaternion requested=Quaternion.Euler(cameraPitch,cameraYaw,0);
        Vector3 boom=-(requested*Vector3.forward)*12.5f;
        float highPivot=2.8f;
        if(wispHeadBoundsReady&&(player.spec.name=="WISP"||player.importedChassis))
        {
            Vector3 cameraUp=requested*Vector3.up;float headTop=float.NegativeInfinity;
            for(int i=0;i<8;i++)
            {
                Vector3 corner=wispHeadBounds.center+Vector3.Scale(wispHeadBounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                headTop=Mathf.Max(headTop,Vector3.Dot(player.headPart.TransformPoint(corner)-player.root.position,cameraUp));
            }
            highPivot=Mathf.Max(highPivot,(headTop+.8f)/Mathf.Max(.7f,Vector3.Dot(cameraUp,Vector3.up)));
        }
        Vector3 highFocus=player.root.position+Vector3.up*highPivot;
        float clearance=Vector3.Distance(highFocus,ConstrainCamera(highFocus,highFocus+boom,.4f));
        float framing=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.5f,7f,clearance));
        // Lower the translation pivot near cover to keep the torso in frame.
        // Collision never selects a new yaw/pitch or an overhead camera.
        Vector3 focus=player.root.position+Vector3.up*Mathf.Lerp(.2f,highPivot,framing);
        Vector3 desired=ConstrainCamera(focus,focus+boom,.4f);
        float blend=1-Mathf.Exp(-12*dt);
        Vector3 position=desired; // Follow the current pilot position without trailing through cover.
        if(cameraShakeTime>0)
        {
            cameraShakeTime=Mathf.Max(0,cameraShakeTime-dt);
            position+=new Vector3(Mathf.Sin(Time.time*83),Mathf.Cos(Time.time*109),0)*cameraShakeStrength*(cameraShakeTime/.3f);
        }
        view.transform.position=ConstrainCamera(focus,position,.4f);
        view.transform.rotation=requested; // Mouse orientation must not lag behind the centered boom.
        view.nearClipPlane=.08f;
        float distance=Vector3.Distance(focus,view.transform.position);
        view.fieldOfView=70;
        UpdateCameraHeadFade(dt,distance);
    }
    void RegisterCameraFadeRenderers(Fighter fighter)
    {
        foreach(var pair in cameraFadeOriginals)if(pair.Key!=null)pair.Key.sharedMaterials=pair.Value;
        cameraFadeOriginals.Clear();cameraFadedArrays.Clear();cameraBodyOpacity=1;cameraBodyFadeApplied=false;
        foreach(Renderer part in fighter.body.GetComponentsInChildren<Renderer>())
        {
            Material[] source=part.sharedMaterials;bool native=fighter.simpleShell;
            foreach(Material material in source)if(material==wispRuntimeMaterial)native=true;
            if(native)
            {
                cameraFadeOriginals[part]=source;Material[] faded=new Material[source.Length];
                for(int i=0;i<source.Length;i++)
                {
                    Material material=source[i];if(material==null)continue;
                    if(!cameraFadeMaterials.TryGetValue(material,out Material clone)){clone=new Material(cameraFadeShader);clone.CopyPropertiesFromMaterial(material);clone.renderQueue=2450;cameraFadeMaterials[material]=clone;}
                    faded[i]=clone;cameraBodyFadeMaterial=clone;
                }
                cameraFadedArrays[part]=faded;
            }
        }
    }
    void UpdateCameraHeadFade(float dt,float distance)
    {
        bool obstructs=(player.spec.name=="WISP"||player.importedChassis)&&(distance<2.4f||ProjectedWispHeadCoversReticle());
        cameraBodyOpacity=Mathf.MoveTowards(cameraBodyOpacity,obstructs?.12f:1,Mathf.Max(0,dt)*10);
        if(cameraBodyOpacity>.995f)
        {
            if(cameraBodyFadeApplied)foreach(var pair in cameraFadeOriginals)if(pair.Key!=null)pair.Key.sharedMaterials=pair.Value;
            cameraBodyFadeApplied=false;return;
        }
        foreach(Material clone in cameraFadeMaterials.Values)clone.SetFloat("_CameraFade",cameraBodyOpacity);
        if(cameraBodyFadeApplied)return;cameraBodyFadeApplied=true;
        foreach(var pair in cameraFadedArrays)if(pair.Key!=null)pair.Key.sharedMaterials=pair.Value;
    }
    bool VisibleWispHeadBlocksReticle()=>HeadOverlapsCenterReticle(player)&&
        !(cameraFadeOriginals.Count>0&&cameraBodyFadeMaterial!=null&&cameraBodyOpacity<.2f);
    void CameraMotionChecks(Action<bool,string> check)
    {
        bool oldSmoke=smoke,oldOrtho=view.orthographic;Vector3 oldPosition=view.transform.position;Quaternion oldRotation=view.transform.rotation;float oldFov=view.fieldOfView,oldYaw=cameraYaw,oldPitch=cameraPitch;
        TestSetupSelectable(0,0);smoke=false;inMenu=false;
        cameraYaw=13;cameraPitch=18;targetLock=true;UpdateCamera(10);
        check(Mathf.Abs(Vector3.Dot(view.transform.position-player.root.position,Quaternion.Euler(0,cameraYaw,0)*Vector3.right))<.001f,"camera follows the center rear without shoulder offset");
        check(!targetLock&&Mathf.Abs(Mathf.DeltaAngle(cameraYaw,13))<.001f&&Mathf.Abs(cameraPitch-18)<.001f,"WISP rejects stale lock without changing mouse view");
        Bounds wall=new Bounds(new Vector3(100,3,96.5f),new Vector3(20,8,2));obstacles.Add(wall);
        cameraYaw=0;cameraPitch=DefaultThirdPersonPitch;
        for(int i=0;i<80;i++)UpdateCamera(.025f);
        check(Quaternion.Angle(view.transform.rotation,Quaternion.Euler(cameraPitch,cameraYaw,0))<.1f,"cover retracts camera without overhead pitch or automatic yaw");
        check(!CameraOccludes(player),"perspective cover marker ignores walls behind the real camera");
        bool stable=true;
        for(int i=0;i<60;i++){enemy.root.position=new Vector3(100+Mathf.Sin(i)*30,20,100+Mathf.Cos(i)*30);UpdateCamera(.025f);stable&=Quaternion.Angle(view.transform.rotation,Quaternion.Euler(cameraPitch,cameraYaw,0))<.1f;}
        check(stable,"moving enemy cannot steer WISP camera");obstacles.RemoveAt(obstacles.Count-1);
        for(int i=0;i<80;i++)UpdateCamera(.025f);
        check(cameraBodyOpacity>.995f,"head restores opaque appearance when camera leaves cover");
        cameraYaw=20;cameraPitch=10;ResetMouseLookCapture();ApplyMouseLook(9999,9999,false);ApplyMouseLook(9999,9999,true);ApplyMouseLook(9999,9999,true);
        check(cameraYaw==20&&cameraPitch==10,"focus and cursor recapture discard warp deltas");
        ApplyMouseLook(1,-1,true);
        check(Mathf.Abs(cameraYaw-20-mouseSensitivity)<.001f&&Mathf.Abs(cameraPitch-10-mouseSensitivity)<.001f,"normal mouse input remains responsive after recapture");
        float yaw=cameraYaw,pitch=cameraPitch;ApplyMouseLook(float.NaN,float.PositiveInfinity,true);
        check(cameraYaw==yaw&&cameraPitch==pitch,"invalid mouse deltas cannot corrupt view angles");
        smoke=oldSmoke;ResetMouseLookCapture();TestSetup();
        view.transform.SetPositionAndRotation(oldPosition,oldRotation);view.orthographic=oldOrtho;view.fieldOfView=oldFov;cameraYaw=oldYaw;cameraPitch=oldPitch;
    }
}
