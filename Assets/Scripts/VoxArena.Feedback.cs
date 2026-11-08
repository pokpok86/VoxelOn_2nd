using UnityEngine;

public sealed partial class VoxArena
{
    bool assistEngaged;
    float hitConfirmTime,hitConfirmDamage,damageDealt,damageTaken;
    bool lastHitShield;
    float DisplayedPrimaryRange()=>smoke?player.spec.range:EquippedProfile(player).range;
    string AimStatus()
    {
        if(!canShoot) return "OUTSIDE FIRING ARC";
        float distance=Vector3.Distance(player.root.position,enemy.root.position);
        float range=DisplayedPrimaryRange();
        if(distance>range) return "RIVAL OUT OF PRIMARY RANGE";
        if(ObstacleOnSegment(player.root.position,enemy.root.position,.1f,out _)) return "RIVAL BEHIND COVER";
        if(!smoke&&player.reloadTime>0) return "RELOADING  "+player.reloadTime.ToString("0.0")+"s";
        if(!smoke&&player.magazine<=0) return "AUTO RELOAD PENDING";
        if(player.weaponTimer>0) return "PRIMARY COOLDOWN  "+player.weaponTimer.ToString("0.0")+"s";
        return assistEngaged?"AIM ASSIST ACTIVE / PRIMARY READY":"MANUAL AIM / PRIMARY READY";
    }
    void RecordDamage(Fighter target,float amount,bool shield)
    {
        if(amount<=0) return;
        if(target==enemy)
        {
            damageDealt+=amount;
            hitConfirmDamage=hitConfirmTime>0?hitConfirmDamage+amount:amount;
            hitConfirmTime=.35f; lastHitShield=shield;
        }
        else damageTaken+=amount;
    }
    void ResetFeedback()
    {
        assistEngaged=false; hitConfirmTime=hitConfirmDamage=damageDealt=damageTaken=0;
    }
    void DrawCombatFeedback()
    {
        if(paused||Finished) return;
        float distance=Vector3.Distance(player.root.position,enemy.root.position);
        Text(28,574,370,(HasSensorContact(player,enemy)?"DIST "+distance.ToString("0"):"DIST --")+" / RANGE "+DisplayedPrimaryRange().ToString("0"),small);
        DrawOccludedPilot();
        if(targetLock&&player.spec.supportsLockOn&&TryGetTargetScreenRect(out Rect targetRect))
        {
            if(TryGetTargetScreenPoint(out Vector2 lockPoint))targetRect.center=lockPoint;
            float x=targetRect.center.x,y=targetRect.center.y;
            Color color=hitConfirmTime>0?Color.white:assistEngaged?Cyan:new Color(1,.55f,.28f);
            float corner=Mathf.Clamp(Mathf.Min(targetRect.width,targetRect.height)*.20f,8,18),thick=2;
            for(int side=-1;side<=1;side+=2)for(int vertical=-1;vertical<=1;vertical+=2)
            {
                float cx=side<0?targetRect.xMin:targetRect.xMax-corner,cy=vertical<0?targetRect.yMin:targetRect.yMax-thick;
                Panel(new Rect(cx,cy,corner,thick),color);
                Panel(new Rect(side<0?targetRect.xMin:targetRect.xMax-thick,vertical<0?targetRect.yMin:targetRect.yMax-corner,thick,corner),color);
            }
            if(assistEngaged){Panel(new Rect(x-33,Mathf.Max(100,targetRect.yMin-22),66,20),new Color(.025f,.045f,.075f,.86f));GUI.color=Cyan;Text(x-25,Mathf.Max(99,targetRect.yMin-23),80,"ASSIST",small);GUI.color=Color.white;}
        }
        if(hitConfirmTime>0)
        {
            GUI.color=lastHitShield?Cyan:Color.white;
            Text(1035,290,230,(lastHitShield?"SHIELD HIT  ":"HIT  ")+hitConfirmDamage.ToString("0.0"),label);
            GUI.color=Color.white;
        }
    }
    bool CameraOccludes(Fighter fighter)
    {
        Vector3 center=fighter.root.position+Vector3.up*.6f;
        // Orthographic camera rays are parallel; a ray from camera.position would be incorrect.
        Vector3 origin=view.orthographic?center-view.transform.forward*120:view.transform.position;
        return ObstacleOnSegment(origin,center,0,out _);
    }
    void DrawOccludedPilot()
    {
        if(!CameraOccludes(player)) return;
        Vector3 p=view.WorldToViewportPoint(player.root.position+Vector3.up*.6f);
        float x=p.x*1280,y=(1-p.y)*720;
        if(p.z<=0||x<65||x>1215||y<115||y>540) return;
        Panel(new Rect(x-3,y-3,6,6),Cyan);
        Panel(new Rect(x-57,y+13,114,21),new Color(.025f,.045f,.075f,.9f));
        GUI.color=Cyan; Text(x-48,y+12,110,"PILOT / COVER",small); GUI.color=Color.white;
    }
}
