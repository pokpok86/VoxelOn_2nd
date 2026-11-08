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
        int ActiveRainCount(){int count=0;foreach(Transform drop in rainDrops)if(drop!=null&&drop.gameObject.activeSelf)count++;return count;}
        TestSetupSelectable(0,1);smoke=false;inMenu=false;spectatorMode=false;targetLock=false;muted=true;player.root.position=new Vector3(0,FighterGroundY,0);enemy.root.position=new Vector3(35,FighterGroundY,35);player.facing=Vector3.forward;
        cameraYaw=45;cameraPitch=DefaultThirdPersonPitch;Animate(player);Animate(enemy);
        Check(arenaMap!=ArenaMapKind.IndoorFactory&&skyDome!=null,"outdoor authored sky exists");Check(ponds.Count==(arenaMap==ArenaMapKind.CoastalBay?1:arenaMap==ArenaMapKind.OutdoorCanyon?2:3)&&rainDrops.Count==96&&ripples.Count==24,"bounded water, rain and ripple pools built");
        if(arenaMap==ArenaMapKind.OutdoorCanyon)
        {
            float centerDepth=OutdoorCanyonWaterDepth(new Vector3(CanyonWaterCenters[0].x,0,CanyonWaterCenters[0].y));
            float maxSlope=Mathf.Atan(1.5f*CanyonWaterDepth/(CanyonWaterRadius-CanyonWaterFloorRadius))*Mathf.Rad2Deg;
            Check(Mathf.Abs(centerDepth-CanyonWaterDepth)<.01f&&ponds.Count==2,"canyon has two deep walkable water basins");
            Check(maxSlope<22&&OutdoorCanyonSurfaceHeight(new Vector3(CanyonWaterCenters[0].x+CanyonWaterRadius,0,CanyonWaterCenters[0].y))==GroundSurfaceY,"water shoreline ramps stay below a 22 degree grade");
            player.root.position=new Vector3(CanyonWaterCenters[0].x,GroundSurfaceY-CanyonWaterDepth+FighterSupportOffset,CanyonWaterCenters[0].y);
            Check(WaterGravityScale(player)==.5f,"deep underwater gravity is reduced by 50 percent");
        }
        else if(arenaMap==ArenaMapKind.CoastalBay)
        {
            float slope=Mathf.Atan(1.5f*CoastalBayDepth/(0-CoastalLandRampStartX))*Mathf.Rad2Deg;
            Check(Mathf.Abs(CoastalBayDepthAt(new Vector3(0,0,0))-CoastalBayDepth)<.01f&&Mathf.Abs(CoastalBaySurfaceHeight(new Vector3(-100,0,0))-GroundSurfaceY)<.01f,"coast separates dry land and deep sea floor");
            Check(slope<22&&InsideWaterFootprint(new Vector3(50,0,0))&&!InsideWaterFootprint(new Vector3(-75,0,0)),"shoreline ramp is walkable and dry land meets water at the bay edge");
            Check(ponds.Count==1&&coastalWaveCrests.Count==14&&debris.Count>=15&&debris.TrueForAll(item=>item.origin.x<0),"coastal map has one wavy sea and warehouse cover on land");
            player.root.position=new Vector3(50,CoastalBaySurfaceHeight(new Vector3(50,0,0))+FighterSupportOffset,0);
            Check(WaterGravityScale(player)==.5f,"deep coastal water reduces gravity by 50 percent");
            UpdateClimate(.02f);
            Check(RenderSettings.fogDensity>.02f&&view.backgroundColor.b<.2f,"submerged fighter activates blue underwater fog and lighting");
            Check(GameObject.Find("Ocean surface / non-colliding")!=null,"coastal water surface is visible without blocking movement");
            float savedRoundTime=roundTime;roundTime=3f;float towardLand=CoastalWaveShoreSpeed(player);roundTime=8.5f;float towardSea=CoastalWaveShoreSpeed(player);roundTime=savedRoundTime;
            Check(towardLand>0&&towardSea<0,"coastal wave current alternates pull toward land and push back to sea");
            Vector3 wetStep=ProjectileStep(new Vector3(50,GroundSurfaceY-3,0),Vector3.forward*100,.1f,.1f),dryStep=ProjectileStep(new Vector3(-50,GroundSurfaceY,0),Vector3.forward*100,.1f,.1f);
            Check(Mathf.Abs(wetStep.magnitude/dryStep.magnitude-.58f)<.001f,"underwater projectiles travel at reduced speed");
            player.root.position=new Vector3(-75,SurfaceHeight(new Vector3(-75,0,0))+FighterSupportOffset,0);UpdateClimate(.02f);
            Check(RenderSettings.fogDensity<.01f,"shore-side dry land restores clear air");
            forcedWeather=(int)WeatherKind.Clear;forcedDay=0;SelectRoundClimate();
            player.root.position=new Vector3(45,CoastalBaySurfaceHeight(new Vector3(45,0,0))+FighterSupportOffset,0);
            for(int frame=0;frame<3;frame++)yield return null;
            yield return new WaitForEndOfFrame();Texture2D underwater=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,"CoastalBay-Underwater.png"),underwater.EncodeToPNG());Destroy(underwater);
            player.root.position=ArenaSpawnPosition(0);
            for(int frame=0;frame<3;frame++)yield return null;
            yield return new WaitForEndOfFrame();Texture2D coast=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,"CoastalBay-Shoreline.png"),coast.EncodeToPNG());Destroy(coast);
        }
        int[] weatherCases=arenaMap==ArenaMapKind.OutdoorCanyon?new[]{0,1,(int)WeatherKind.Sandstorm}:new[]{0,1,2,3};
        foreach(int weather in weatherCases)for(int day=0;day<2;day++)
        {
            forcedWeather=weather;forcedDay=day;SelectRoundClimate();
            for(int frame=0;frame<18;frame++)yield return null;
            WeatherKind expected=arenaMap==ArenaMapKind.OutdoorCanyon&&weather>=2?WeatherKind.Sandstorm:(WeatherKind)weather;
            bool particleWeather=expected==WeatherKind.Rain||expected==WeatherKind.Thunderstorm||expected==WeatherKind.Sandstorm;
            Check(roundWeather==expected&&evening==(day==1),"forced weather/day "+weather+"/"+day);
            Check(rainDrops[0].gameObject.activeSelf==particleWeather,"rain or sand particle visibility "+weather+"/"+day);
            yield return new WaitForEndOfFrame();Texture2D screen=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,roundWeather+"-"+(evening?"Evening":"Morning")+".png"),screen.EncodeToPNG());Destroy(screen);
        }
        if(arenaMap==ArenaMapKind.OutdoorCanyon)
        {
            forcedWeather=(int)WeatherKind.Rain;forcedTemperatureC=-8;SelectRoundClimate();
            Check(roundWeather==WeatherKind.Sandstorm&&!snowing&&ActiveRainCount()==96&&rainDrops[0].localScale.x>2,"Canyon rain selection becomes visible dry sand gusts without overlapping rain or snow");
            Check(RenderSettings.fogDensity>.008f&&Mathf.Abs(RadarWeatherFactor()-.46f)<.001f&&Mathf.Abs(TargetingWeatherFactor()-.55f)<.001f&&Mathf.Abs(WeatherLockRange(player)-player.spec.lockRange*.55f)<.01f,"sandstorm fog, radar and lock-on range are degraded");
        }
        else
        {
            forcedWeather=(int)WeatherKind.Rain;forcedTemperatureC=-8;forcedHumidity=95;forcedPrecipitation=0;ApplyClimate(WeatherKind.Rain,false);
            Check(snowing&&ActiveRainCount()==27&&FireIgnitionMultiplier<=.25f,"cold rain converts to light snow and high humidity lowers future fire ignition chance");
            forcedPrecipitation=1;ApplyClimate(WeatherKind.Rain,false);Check(ActiveRainCount()==60,"moderate precipitation uses its middle intensity");
            forcedPrecipitation=2;ApplyClimate(WeatherKind.Rain,false);Check(ActiveRainCount()==96,"heavy precipitation uses its maximum intensity");
        }
        forcedTemperatureC=99;forcedHumidity=forcedPrecipitation=-1;ApplyClimate(WeatherKind.Clear,false);
        if(arenaMap==ArenaMapKind.OutdoorCanyon)Check(debris.Count>=8&&terrainSteps.Count>=100&&cameraSolids.Count>0,"canyon keeps distributed natural cover, continuous slopes, collision cover and boundaries");
        foreach(int id in new[]{0,1,2})
        {
            TestSetupSelectable(id,(id+1)%3);smoke=false;inMenu=false;player.root.position=new Vector3(arenaMap==ArenaMapKind.OpenPlains?110:0,0,arenaMap==ArenaMapKind.OpenPlains?75:0);Vector3 p=player.root.position;p.y=SurfaceHeight(p)+FighterSupportOffset;player.root.position=p;player.facing=Vector3.forward;player.boostHeld=false;
            for(int i=0;i<100;i++)Move(player,Vector3.forward,.02f);
            for(int i=0;i<10;i++)Move(player,Vector3.zero,.02f);Animate(player);
            float contactError=player.root.position.y-LandingHeight(player.root.position,player.Radius,player.root.position.y);
            Check(Mathf.Abs(contactError)<.15f,"actual terrain contact while walking frame "+id+" error="+contactError.ToString("F3"));
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
        float dryExitX=arenaMap==ArenaMapKind.CoastalBay?-75:30;player.root.position=new Vector3(dryExitX,SurfaceHeight(new Vector3(dryExitX,0,30))+FighterSupportOffset,30);player.velocity=Vector3.zero;UpdateClimate(.02f);Check(!previouslyWet[0],"leaving water clears contact state");
        if(arenaMap==ArenaMapKind.OutdoorCanyon)
        {
            player.root.position=new Vector3(CanyonWaterCenters[0].x,GroundSurfaceY-CanyonWaterDepth+FighterSupportOffset+2,CanyonWaterCenters[0].y);player.jumpHeight=2;player.jumpVelocity=0;player.velocity=Vector3.zero;Move(player,Vector3.zero,.02f);float wetFallSpeed=-player.jumpVelocity;
            player.root.position=new Vector3(30,FighterGroundY+10,30);player.jumpHeight=10;player.jumpVelocity=0;player.velocity=Vector3.zero;Move(player,Vector3.zero,.02f);float dryFallSpeed=-player.jumpVelocity;
            Check(wetFallSpeed>0&&Mathf.Abs(wetFallSpeed/dryFallSpeed-.5f)<.02f,"deep water halves gravity-driven fall acceleration");
        }
        forcedWeather=forcedDay=-1;Debug.Log(pass?"CLIMATE_SMOKE_PASS "+map+" "+checks+" checks":"CLIMATE_SMOKE_FAIL "+map+" "+checks+" checks");Application.Quit(pass?0:2);
    }
}
