using System;
using System.Collections;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator CaptureShellComparison()
    {
        string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","NacchinStyle-20261105","Iteration06","Unity");Directory.CreateDirectory(folder);
        RenderTexture target=new RenderTexture(768,768,24);Texture2D image=new Texture2D(768,768,TextureFormat.RGB24,false);
        view.targetTexture=target;view.orthographic=true;view.orthographicSize=2.5f;view.cullingMask=1<<9;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.13f,.16f,.20f);
        Transform studio=SpawnPDPart("Pond",null,new Vector3(0,GroundSurfaceY-.008f,0),new Vector3(5,1,5));SetPartMaterial(studio,Mat(new Color(.18f,.20f,.23f)));
        foreach(Transform t in studio.GetComponentsInChildren<Transform>())t.gameObject.layer=9;
        Vector3[] directions={Vector3.forward,Vector3.right,Vector3.back,Vector3.left,new Vector3(5,2.4f,8).normalized};string[] names={"front","right","rear","left","three-quarter"};int count=0;
        for(int frame=0;frame<(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--wisp-only")>=0?1:3);frame++)
        {
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--bastion-only")>=0&&frame!=1)continue;
            TestSetupSelectable(frame,0);inMenu=false;smoke=false;paused=true;spectatorMode=false;player.root.position=new Vector3(0,FighterGroundY,0);player.facing=player.chassisForward=Vector3.forward;player.velocity=Vector3.zero;Animate(player);enemy.root.gameObject.SetActive(false);enemy.shadow.gameObject.SetActive(false);
            foreach(Transform t in player.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;
            foreach(Transform weapon in player.weaponVisuals)weapon.gameObject.SetActive(false);
            if(player.guardShield!=null)player.guardShield.gameObject.SetActive(false);if(player.boostVisual!=null)player.boostVisual.gameObject.SetActive(false);
            Vector3 center=player.root.position+Vector3.down*.2f;
            foreach(int pose in new[]{0,1,2,3,4})
            {
                view.orthographic=true;view.orthographicSize=2.5f;view.cullingMask=1<<9;
                view.transform.position=center+directions[pose]*8+Vector3.up*.18f;view.transform.LookAt(center);view.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,768,768),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,(frame+1)+"-"+player.spec.name+"-"+names[pose]+".png"),image.EncodeToPNG());count++;
                if(frame==2)
                {
                    view.orthographicSize=3.6f;view.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,768,768),0,0);image.Apply();
                    File.WriteAllBytes(Path.Combine(folder,(frame+1)+"-"+player.spec.name+"-"+names[pose]+"-wide.png"),image.EncodeToPNG());count++;
                }
            }
            yield return null;
        }
        RenderTexture.active=null;view.targetTexture=null;target.Release();Destroy(target);Destroy(image);Debug.Log("SHELL_COMPARISON_CAPTURE_PASS "+count+" actual Unity images");Application.Quit(0);
    }
}
