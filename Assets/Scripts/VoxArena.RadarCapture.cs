using System.Collections;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator RadarCapture()
    {
        string dir=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","PD14-Radar",System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--radar-layout-before")>=0?"StormBefore":"StormAfter");Directory.CreateDirectory(dir);bool pass=true;
        TestSetupSelectable(0,1);inMenu=false;smoke=false;muted=true;spectatorMode=false;targetLock=false;player.root.position=new Vector3(0,FighterGroundY,0);cameraYaw=0;cameraPitch=DefaultThirdPersonPitch;
        string[] names={"01-outside-unseen","02-inside-contact","03-rain-outside","04-behind-unseen","05-storm-behind-unseen"};float[] distance={120,30,75,-120,-75};
        for(int i=0;i<names.Length;i++)
        {
            roundWeather=i==4?WeatherKind.Thunderstorm:i==2?WeatherKind.Rain:WeatherKind.Clear;ApplyClimate(roundWeather,false);enemy.root.position=player.root.position+Vector3.forward*distance[i];if(i==0||i==3)radarHasSeen=false;
            Animate(player);Animate(enemy);bool contact=HasSensorContact(player,enemy);bool ok=contact==(i==1)&&outsideRadarPolicy==RadarOutsidePolicy.AlwaysDirection;pass&=ok;
            Debug.Log("RADAR_CHECK "+names[i]+" contact="+contact+" range="+SensorRange(player)+" priorSeen="+radarHasSeen+" "+(ok?"PASS":"FAIL"));
            for(int f=0;f<5;f++)yield return null;yield return new WaitForEndOfFrame();Texture2D image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(dir,names[i]+".png"),image.EncodeToPNG());Destroy(image);
        }
        Debug.Log(pass?"RADAR_CAPTURE_PASS 5 checks / 5 actual images":"RADAR_CAPTURE_FAIL");Application.Quit(pass?0:2);
    }
}
