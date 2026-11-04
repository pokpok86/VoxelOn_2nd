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
        if(!aimAssist||weakAssistDegrees<=0||shooter==null||target==null||target.hp<=0)return false;
        Vector3 delta=target.root.position-(!smoke&&shooter.weaponVisuals!=null?WeaponMuzzle(shooter):shooter.root.position);
        if(delta.magnitude>EquippedProfile(shooter).range||Vector3.Angle(requested,delta)>1.8f)return false;
        if(ObstacleOnSegment(shooter.root.position,target.root.position,.08f,out _))return false;
        if(shooter==player&&!spectatorMode)
        {
            Vector3 p=view.WorldToViewportPoint(target.root.position);
            if(p.z<=0||Mathf.Abs(p.x-.5f)>.045f||Mathf.Abs(p.y-.5f)>.065f||ObstacleOnSegment(view.transform.position,target.root.position,.02f,out _))return false;
        }
        return true;
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
        Bounds wall=new Bounds(player.root.position+Vector3.forward*15,new Vector3(5,6,2));obstacles.Add(wall);
        check(!CanWeakAssist(player,enemy,requested),"weak aim rejects occluded target");obstacles.RemoveAt(obstacles.Count-1);
        aimAssist=false;check(WeakAssist(player,enemy,requested)==requested,"weak aim toggle fully disables correction");
        aimAssist=oldAssist;weakAssistDegrees=oldDegrees;smoke=prior;TestSetup();
    }
}
