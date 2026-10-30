using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    [Range(0,2)] public int difficulty=0;
    public bool aimAssist=true;
    string DifficultyName=>difficulty==0?"PRACTICE":difficulty==1?"STANDARD":"ACE";
    bool CanAssist(Fighter shooter,Fighter target,Vector3 requested)
    {
        if(!aimAssist || !InArc(shooter,requested)) return false;
        Vector3 delta=target.root.position-shooter.root.position;
        float range=shooter==player&&!smoke?82:shooter.spec.range;
        return Vector3.Angle(requested,delta)<=18 && delta.magnitude<=range &&
            !ObstacleOnSegment(shooter.root.position,target.root.position,.1f,out _);
    }
    Vector3 AssistAim(Fighter shooter,Fighter target,Vector3 requested,float delay=0,float speedFactor=1)
    {
        if(!CanAssist(shooter,target,requested)) return requested;
        Vector3 delta=target.root.position-shooter.root.position;
        float speed=(shooter==player&&!smoke?132:shooter.spec.projectileSpeed)*bulletSpeedScale*speedFactor;
        float flight=delta.magnitude/speed;
        // Refine travel time instead of consistently aiming behind a lateral target.
        for(int i=0;i<4;i++) flight=Mathf.Clamp(((delta+target.velocity*(delay+flight)).magnitude-shooter.Radius-.25f)/speed,0,1.2f);
        Vector3 predicted=delta+target.velocity*(delay+flight);
        Vector3 assisted=Vector3.RotateTowards(requested.normalized,predicted.normalized,18*Mathf.Deg2Rad,0);
        return InArc(shooter,assisted)?assisted:requested;
    }
    Vector3 FollowupAim(Fighter owner,Fighter target,Vector3 committed,float delay,float speedFactor)
    {
        if(owner!=player||!CanAssist(owner,target,committed)) return committed;
        Vector3 predicted=AssistAim(owner,target,committed,delay,speedFactor);
        Vector3 corrected=Vector3.RotateTowards(committed,predicted,6*Mathf.Deg2Rad,0);
        return InArc(owner,corrected)?corrected:committed;
    }
    sealed class FlashEffect { public Transform visual; public float age,life,size; }
    readonly List<FlashEffect> flashes=new List<FlashEffect>();
    void Flash(Vector3 position,Color color,float size,float life)
    {
        if(flashes.Count>=48) return;
        Transform visual=Shape(PrimitiveType.Sphere,"Energy flash",null,position,Vector3.one*size,color,true);
        flashes.Add(new FlashEffect{visual=visual,life=life,size=size});
    }
    void UpdateFlashes(float dt)
    {
        for(int i=flashes.Count-1;i>=0;i--)
        {
            FlashEffect fx=flashes[i]; fx.age+=dt;
            fx.visual.localScale=Vector3.one*fx.size*Mathf.Max(0,1-fx.age/fx.life);
            if(fx.age>=fx.life) { Destroy(fx.visual.gameObject); flashes.RemoveAt(i); }
        }
    }
}
