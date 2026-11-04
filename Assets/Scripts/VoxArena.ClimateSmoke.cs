using System;
using System.Collections;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator ClimateSmoke()
    {
        bool pass=true;int checks=0;string map=arenaMap.ToString();string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","PD14-Climate",map);Directory.CreateDirectory(folder);
        void Check(bool ok,string label){checks++;pass&=ok;Debug.Log("CLIMATE_CHECK "+label+" "+(ok?"PASS":"FAIL"));}
        TestSetupSelectable(0,1);smoke=false;inMenu=false;spectatorMode=false;targetLock=false;muted=true;player.root.position=new Vector3(0,FighterGroundY,0);enemy.root.position=new Vector3(35,FighterGroundY,35);player.facing=Vector3.forward;
        cameraYaw=45;cameraPitch=DefaultThirdPersonPitch;Animate(player);Animate(enemy);
        Check(arenaMap!=ArenaMapKind.IndoorFactory&&skyDome!=null,"outdoor authored sky exists");Check(ponds.Count==3&&rainDrops.Count==96&&ripples.Count==24,"bounded pond rain and ripple pools built");
        for(int weather=0;weather<4;weather++)for(int day=0;day<2;day++)
        {
            forcedWeather=weather;forcedDay=day;SelectRoundClimate();
            for(int frame=0;frame<18;frame++)yield return null;
            Check((int)roundWeather==weather&&evening==(day==1),"forced weather/day "+weather+"/"+day);
            Check(rainDrops[0].gameObject.activeSelf==(weather>=2),"rain visibility "+weather+"/"+day);
            yield return new WaitForEndOfFrame();Texture2D screen=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,roundWeather+"-"+(evening?"Evening":"Morning")+".png"),screen.EncodeToPNG());Destroy(screen);
        }
        ApplyClimate(WeatherKind.Clear,false);
        if(arenaMap==ArenaMapKind.OutdoorCanyon)Check(debris.Count==0&&terrainSteps.Count==0&&cameraSolids.Count>0,"PD outdoor arena has no loose block obstacles and retains boundaries");
        foreach(int id in new[]{0,1,2})
        {
            TestSetupSelectable(id,(id+1)%3);smoke=false;inMenu=false;player.root.position=new Vector3(arenaMap==ArenaMapKind.OpenPlains?110:0,0,arenaMap==ArenaMapKind.OpenPlains?75:0);Vector3 p=player.root.position;p.y=SurfaceHeight(p)+FighterSupportOffset;player.root.position=p;player.facing=Vector3.forward;player.boostHeld=false;
            for(int i=0;i<100;i++)Move(player,Vector3.forward,.02f);Animate(player);
            Check(Mathf.Abs(player.root.position.y-FighterSupportOffset-SurfaceHeight(player.root.position))<.05f,"actual terrain contact while walking frame "+id);
            Check(Finite(player.root.position),"finite hilly movement frame "+id);
        }
        if(arenaMap==ArenaMapKind.OpenPlains)
        {
            float maxError=0;int samples=0;
            foreach(MeshFilter filter in starRoot.GetComponentsInChildren<MeshFilter>())
            {
                Vector3[] vertices=filter.sharedMesh.vertices;
                for(int i=0;i<vertices.Length;i+=37){Vector3 point=filter.transform.TransformPoint(vertices[i]);maxError=Mathf.Max(maxError,Mathf.Abs(point.y-SurfaceHeight(point)));samples++;}
            }
            Check(samples>100&&maxError<.02f,"authored Blender surface matches landing height, error="+maxError.ToString("F4"));
            int elevations=0;
            foreach(float sign in new[]{-1f,1f})
            {
                bool found=false;Vector3 anchor=Vector3.zero;
                foreach(var pair in plainsFaces)
                {
                    Vector3 at=(pair.Value[0].a+pair.Value[0].b+pair.Value[0].c)/3;
                    if(Mathf.Abs(at.x)>320||Mathf.Abs(at.z)>320||at.y*sign<12)continue;
                    Vector3 root=at+Vector3.up*FighterSupportOffset;if(Occupied(root,player.Radius))continue;anchor=root;found=true;break;
                }
                if(found)
                {
                    player.root.position=anchor;player.jumpHeight=0;player.jumpVelocity=0;player.jumpHeld=false;player.boostHeld=false;
                    for(int tick=0;tick<80;tick++)Move(player,Vector3.forward,.02f);
                    for(int tick=0;tick<40;tick++)Move(player,Vector3.zero,.02f);
                    Check(Mathf.Abs(player.root.position.y-FighterSupportOffset-SurfaceHeight(player.root.position))<.06f,"high or low terrain stays grounded after travel sign="+sign);
                    elevations++;
                }
            }
            Check(elevations==2,"both high hills and deep low areas are walkable return routes");
            Check(cameraSolids.Count==0,"plains has no artificial perimeter walls");
            Check(debris.Count>=30,"plains has distributed destructible natural cover");
        }
        player.root.position=new Vector3(ponds[0].x,FighterGroundY,ponds[0].z);player.jumpHeight=player.jumpVelocity=0;player.jumpHeld=player.boostHeld=false;player.guarding=false;player.velocity=Vector3.zero;
        previouslyWet[0]=false;splashClock=.3f;int waterFree=fxPools["WaterDrop"].Count,priorRipple=nextRipple;UpdateClimate(.02f);
        Check(previouslyWet[0]&&fxPools["WaterDrop"].Count<waterFree&&nextRipple!=priorRipple,"real shallow water entry emits pooled splash and ripple");
        Vector3 wetStart=player.root.position;Move(player,Vector3.right,.02f);float wetTravel=Vector3.Distance(wetStart,player.root.position);
        player.root.position=new Vector3(30,FighterGroundY,30);player.velocity=Vector3.zero;UpdateClimate(.02f);Check(!previouslyWet[0],"leaving shallow water clears contact state");
        Vector3 dryStart=player.root.position;Move(player,Vector3.right,.02f);Check(Mathf.Abs(wetTravel-Vector3.Distance(dryStart,player.root.position))<.01f,"shallow water preserves normal movement speed");
        forcedWeather=forcedDay=-1;Debug.Log(pass?"CLIMATE_SMOKE_PASS "+map+" "+checks+" checks":"CLIMATE_SMOKE_FAIL "+map+" "+checks+" checks");Application.Quit(pass?0:2);
    }
}
