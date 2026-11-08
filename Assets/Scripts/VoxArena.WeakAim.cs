using System;
using UnityEngine;
public sealed partial class VoxArena
{
    float assistEcho;
    float WeakAimStrength(Fighter f)
    {
        if(f.boosting||f.dashRemaining>0)return .25f;
        if(f.jumpHeight>.08f||Mathf.Abs(f.jumpVelocity)>1)return .5f;
        if(Vector3.ProjectOnPlane(f.velocity,Vector3.up).magnitude>1)return .65f;
        return Mathf.Lerp(.65f,1,Mathf.Clamp01(f.aimSettled/.4f));
    }
    bool CanWeakAssist(Fighter shooter,Fighter target,Vector3 requested)
    {
        if(shooter==null)return false;
        bool Reject(){shooter.assistTrackedTarget=null;return false;}
        if(!aimAssist||weakAssistDegrees<=0||target==null||target.hp<=0||requested.sqrMagnitude<.001f||!Finite(requested))return Reject();
        bool retained=shooter.assistTrackedTarget==target;
        Vector3 delta=target.root.position-(!smoke&&shooter.weaponVisuals!=null?WeaponMuzzle(shooter):shooter.root.position);
        // Head radar provides soft-assist awareness without granting or requiring RMB lock-on.
        float sensorRange=Mathf.Max(0,shooter.spec.radarRange);
        if(delta.magnitude>Mathf.Min(EquippedProfile(shooter).range,sensorRange)||Vector3.Angle(requested,delta)>(retained?2.3f:1.8f))return Reject();
        if(ObstacleOnSegment(shooter.root.position,target.root.position,.08f,out _))return Reject();
        if(shooter==player&&!spectatorMode)
        {
            Vector3 p=view.WorldToViewportPoint(target.root.position);
            if(p.z<=0||Mathf.Abs(p.x-.5f)>(retained?.06f:.045f)||Mathf.Abs(p.y-.5f)>(retained?.08f:.065f)||ObstacleOnSegment(view.transform.position,target.root.position,.02f,out _))return Reject();
        }
        shooter.assistTrackedTarget=target;return true;
    }

    Vector3 WeakAssist(Fighter shooter,Fighter target,Vector3 requested)
    {
        if(!CanWeakAssist(shooter,target,requested))return requested;
        return Vector3.RotateTowards(requested.normalized,(target.root.position-(!smoke&&shooter.weaponVisuals!=null?WeaponMuzzle(shooter):shooter.root.position)).normalized,
            weakAssistDegrees*Mathf.Deg2Rad*WeakAimStrength(shooter),0);
    }
    void UpdateAssistFeedback(float dt)
    {
        assistEcho=Mathf.MoveTowards(assistEcho,assistEngaged?1:0,dt/.3f);
    }
    void DrawAssistEcho()
    {
        if(inMenu||assistEcho<=0)return;
        Color c=Cyan;c.a=assistEcho*.35f;float r=8+(1-assistEcho)*28;
        for(int i=0;i<4;i++)
        {
            float a=(45+i*90)*Mathf.Deg2Rad;Panel(new Rect(640+Mathf.Cos(a)*r-2,360+Mathf.Sin(a)*r-2,4,4),c);
        }
        if(assistEcho>.95f)Panel(new Rect(637,345,6,2),Cyan);
    }
    void AimRevisionChecks(Action<bool,string> check)
    {
        bool prior=smoke,oldAssist=aimAssist;float oldDegrees=weakAssistDegrees;smoke=false;aimAssist=true;weakAssistDegrees=1;
        TestSetupSelectable(0,1);player.root.position=new Vector3(100,FighterGroundY,100);enemy.root.position=player.root.position+Vector3.forward*30;
        view.orthographic=false;view.transform.position=player.root.position-Vector3.forward*10;view.transform.rotation=Quaternion.identity;
        Vector3 requested=Quaternion.AngleAxis(1.4f,Vector3.up)*(enemy.root.position-WeaponMuzzle(player)).normalized;
        player.velocity=Vector3.zero;player.jumpHeight=0;player.jumpVelocity=0;player.boosting=false;player.dashRemaining=0;player.aimSettled=.5f;
        Quaternion rotation=view.transform.rotation;Vector3 a=WeakAssist(player,enemy,requested);
        check(Vector3.Angle(a,requested)>.9f&&Vector3.Angle(a,requested)<1.1f&&view.transform.rotation==rotation,"weak assist caps correction at one degree without camera pull");
        player.velocity=Vector3.forward*10;float moving=WeakAimStrength(player);player.jumpHeight=2;float airborne=WeakAimStrength(player);player.boosting=true;float boosting=WeakAimStrength(player);
        check(moving==.65f&&airborne==.5f&&boosting==.25f,"weak aim strength decreases for travel air and boost");
        check(!CanWeakAssist(player,enemy,Vector3.right),"weak aim rejects targets far from reticle");
        targetLock=false;enemy.root.position=player.root.position+Vector3.forward*50;player.spec.radarRange=45;
        check(!CanWeakAssist(player,enemy,Vector3.forward),"head radar range limits soft aim assistance without lock-on");
        player.spec.radarRange=60;
        check(CanWeakAssist(player,enemy,Vector3.forward)&&!targetLock,"head radar enables soft aim assistance without acquiring camera lock");
        assistEngaged=true;check(ShouldDrawSoftAssistReticle(true)&&!ShouldDrawSoftAssistReticle(false),"soft-assist cue requires an eligible on-screen target");
        targetLock=true;check(!ShouldDrawSoftAssistReticle(true),"soft-assist cue does not replace the full lock reticle");targetLock=false;
        enemy.root.position=player.root.position+Vector3.forward*30;
        Bounds wall=new Bounds(player.root.position+Vector3.forward*15,new Vector3(5,6,2));obstacles.Add(wall);
        check(!CanWeakAssist(player,enemy,requested),"weak aim rejects occluded target");obstacles.RemoveAt(obstacles.Count-1);
        aimAssist=false;check(WeakAssist(player,enemy,requested)==requested,"weak aim toggle fully disables correction");
        aimAssist=oldAssist;weakAssistDegrees=oldDegrees;smoke=prior;TestSetup();
    }
}
