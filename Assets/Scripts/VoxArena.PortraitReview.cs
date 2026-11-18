using System;
using System.IO;
using UnityEngine;

public sealed partial class VoxArena
{
    void SavePortraitEvidence(RenderTexture texture,string name)
    {
        RenderTexture previous=RenderTexture.active;var image=new Texture2D(texture.width,texture.height,TextureFormat.RGB24,false);
        try
        {
            RenderTexture.active=texture;image.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);image.Apply();
            string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Validation","Overnight-20261119","Iteration13");Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;Destroy(image);}
    }
    void CapturePriorPortraitFraming(int frame)
    {
        // Reproduce the previous 384x512 customization target and chassis-only
        // minimum zoom for a controlled comparison with the very same assembly.
        Bounds bounds=PortraitBounds(false,out bool found);Vector3 target=bounds.center;
        portraitCamera.transform.position=portraitModel.root.position+Quaternion.Euler(0,portraitYaw,0)*new Vector3(2.5f,4.1f,9f);
        portraitCamera.transform.LookAt(target);Vector3 up=portraitCamera.transform.up,e=bounds.extents;
        portraitCamera.aspect=384f/512;portraitCamera.orthographicSize=Mathf.Max(2.6f,(Mathf.Abs(up.x)*e.x+Mathf.Abs(up.y)*e.y+Mathf.Abs(up.z)*e.z)*1.12f+.08f);
        var before=new RenderTexture(384,512,24);
        try{portraitCamera.targetTexture=before;portraitCamera.Render();SavePortraitEvidence(before,"Frame-"+frame+"-Before-Previous-Framing");}
        finally{portraitCamera.targetTexture=null;before.Release();Destroy(before);}
    }
    void PortraitFrameChecks(Action<bool,string> check)
    {
        BalanceDocument previousBalance=activeBalance;FamilyLoadoutFile previousFamily=familyLoadouts;float previousYaw=portraitYaw;bool previousMenu=inMenu;
        try
        {
            activeBalance=BalanceData.Defaults();familyLoadouts=new FamilyLoadoutFile();activeBalance.activeDesignId=activeBalance.designs[0].id;inMenu=true;
            activeBalance.designs[0].slots[0]="Military_RL";activeBalance.designs[0].slots[1]="Military_LMG";
            BuildPortraits();
            for(int frame=0;frame<CraftCatalog.Selectable.Length;frame++)
            {
                ApplySelectableCharacter(portraitModel,frame);FamilyLoadout config=FamilyConfig(portraitModel);
                config.backpack=frame==1?"B-02_CROSSLANCE":DronePackIds[frame==0?0:2];config.auxiliary=new[]{"AUX_MISSILE_POD","AUX_PULSE_GUN"};BuildRobot(portraitModel);PreparePortraitModel();
                check(customizationPortraits[frame].width==824&&customizationPortraits[frame].height==608,"customization uses landscape viewport matching actual left panel "+frame);
                check(framePortraits[frame].width==384&&framePortraits[frame].height==512,"selection card retains separate portrait texture "+frame);
                for(int turn=0;turn<4;turn++)
                {
                    portraitYaw=turn*90;RenderPortraitFrame(frame,true);
                    if(turn==0){CapturePriorPortraitFraming(frame);RenderPortraitFrame(frame,true);}
                    Bounds all=PortraitBounds(true,out bool found);bool contained=found;float closest=1,farthest=0;
                    foreach(Vector3 point in PortraitMeshCorners(true))
                    {
                        Vector3 screen=portraitCamera.WorldToViewportPoint(point);
                        contained&=screen.z>portraitCamera.nearClipPlane&&screen.x>=.02f&&screen.x<=.98f&&screen.y>=.02f&&screen.y<=.98f;
                        closest=Mathf.Min(closest,screen.y);farthest=Mathf.Max(farthest,screen.y);
                    }
                    check(contained,"body, hands and backpack weapons fit rotated customization camera "+frame+" yaw="+portraitYaw);
                    check(farthest-closest>.45f,"actual assembly occupies readable viewport height "+frame+" yaw="+portraitYaw);
                    SavePortraitEvidence(customizationPortraits[frame],"Frame-"+frame+"-Refined-Yaw-"+portraitYaw);
                    Debug.Log("PORTRAIT_FRAMING frame="+frame+" yaw="+portraitYaw+" ortho="+portraitCamera.orthographicSize+" heightFraction="+(farthest-closest));
                }
                check(compactCustomizationPortraits[frame].width==592&&compactCustomizationPortraits[frame].height==840,"numeric editor has separate portrait aspect matching its actual panel "+frame);
                for(int turn=0;turn<4;turn++)
                {
                    portraitYaw=turn*90;RenderPortraitFrame(frame,true,true);bool compactFits=true;
                    foreach(Vector3 point in PortraitMeshCorners(true))
                    {Vector3 screen=portraitCamera.WorldToViewportPoint(point);compactFits&=screen.z>portraitCamera.nearClipPlane&&screen.x>=.02f&&screen.x<=.98f&&screen.y>=.02f&&screen.y<=.98f;}
                    check(compactFits,"compact numeric preview keeps complete body and weapons inside view "+frame+" yaw="+portraitYaw);
                    SavePortraitEvidence(compactCustomizationPortraits[frame],"Compact-Frame-"+frame+"-Yaw-"+portraitYaw);
                }
                Bounds chassis=PortraitBounds(false,out bool chassisFound);check(chassisFound&&chassis.min.y>=portraitHangar.position.y+.02f,"actual mesh feet clear raised hangar floor "+frame);
            }
        }
        finally{activeBalance=previousBalance;familyLoadouts=previousFamily;portraitYaw=previousYaw;inMenu=previousMenu;BuildPortraits();}
    }
}
