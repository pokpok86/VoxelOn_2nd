using UnityEngine;

public sealed partial class VoxArena
{
    bool assistEngaged;
    float hitConfirmTime,hitConfirmDamage,damageDealt,damageTaken;
    bool lastHitShield;
    string AimStatus()
    {
        if(!canShoot) return "OUTSIDE FIRING ARC";
        float distance=Vector3.Distance(player.root.position,enemy.root.position);
        float range=!smoke?82:player.spec.range;
        if(distance>range) return "RIVAL OUT OF PRIMARY RANGE";
        if(ObstacleOnSegment(player.root.position,enemy.root.position,.1f,out _)) return "RIVAL BEHIND COVER";
        if(!smoke&&player.reloadTime>0) return "RELOADING  "+player.reloadTime.ToString("0.0")+"s";
        if(!smoke&&player.magazine<=0) return "MAG EMPTY / PRESS R TO RELOAD";
        if(player.weaponTimer>0) return "RELOADING  "+player.weaponTimer.ToString("0.0")+"s";
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
        Text(28,574,370,$"DIST {distance:0} / RANGE {player.spec.range:0}",small);
        DrawOccludedPilot();
        Vector3 point=view.WorldToViewportPoint(enemy.root.position);
        float x=point.x*1280,y=(1-point.y)*720;
        if(point.z>0&&x>40&&x<1240&&y>140&&y<540)
        {
            Color color=hitConfirmTime>0?Color.white:assistEngaged?Cyan:new Color(1,.55f,.28f);
            float r=24*enemy.spec.size;
            for(int side=-1;side<=1;side+=2)
            {
                Panel(new Rect(x+side*r-(side>0?8:0),y-r,8,2),color);
                Panel(new Rect(x+side*r,y-r,2,8),color);
                Panel(new Rect(x+side*r-(side>0?8:0),y+r,8,2),color);
                Panel(new Rect(x+side*r,y+r-6,2,8),color);
            }
            if(assistEngaged)
            {
                Vector3 above=view.WorldToViewportPoint(enemy.root.position+Vector3.up*(2.4f*enemy.spec.size));
                float labelY=Mathf.Max(100,(1-above.y)*720-22);
                Panel(new Rect(x-33,labelY,66,20),new Color(.025f,.045f,.075f,.86f));
                GUI.color=Cyan; Text(x-25,labelY-1,80,"ASSIST",small); GUI.color=Color.white;
            }
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
        return ObstacleOnSegment(center-view.transform.forward*120,center,0,out _);
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
