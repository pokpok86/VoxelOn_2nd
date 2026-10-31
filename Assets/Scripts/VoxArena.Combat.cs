using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    sealed class Shot
    {
        public Transform visual; public Fighter owner; public Vector3 velocity,heading;
        public float life,damage,radius,age,tracking,blast,fuse=-1,arm,wave;
        public bool stopAndExplode,mine;
        public LineRenderer warning;
    }
    sealed class Pending { public float delay; public Action action; }
    sealed class Beam
    {
        public Fighter owner; public Vector3 direction; public float age,charge,duration,width,length,dps,sweep;
        public LineRenderer visual; public bool sounded;
    }
    sealed class RingEffect { public LineRenderer line; public Vector3 center; public float radius,age,life; }
    readonly List<Shot> shots=new List<Shot>();
    readonly List<Pending> pending=new List<Pending>();
    readonly List<Beam> beams=new List<Beam>();
    readonly List<RingEffect> rings=new List<RingEffect>();
    int projectilesCreated;

    void ClearCombat()
    {
        foreach(Shot s in shots) { Destroy(s.visual.gameObject); if(s.warning!=null) Destroy(s.warning.gameObject); }
        foreach(Beam b in beams) Destroy(b.visual.gameObject);
        foreach(RingEffect r in rings) Destroy(r.line.gameObject);
        shots.Clear(); pending.Clear(); beams.Clear(); rings.Clear();
        foreach(FlashEffect fx in flashes) Destroy(fx.visual.gameObject); flashes.Clear();
    }
    void Schedule(float delay,Action action)
    {
        if(delay<=0) action(); else pending.Add(new Pending{delay=delay,action=action});
    }
    Shot Fire(Fighter owner,Vector3 direction,float damage,float speed,float radius,float range,float tracking=0,float blast=0,bool stop=false,float wave=0)
    {
        direction=direction.normalized;
        Vector3 muzzle=owner.root.position+direction*(owner.Radius+.25f);
        float physicalRadius=radius*bulletSizeScale;
        if(ObstacleOnSegment(owner.root.position,muzzle,physicalRadius,out _)) return null;
        speed*=bulletSpeedScale;
        Shot s=new Shot{owner=owner,velocity=direction*speed,heading=direction,damage=damage,
            radius=physicalRadius,life=range/speed,tracking=tracking,blast=blast,stopAndExplode=stop,wave=wave};
        s.visual=BuildProjectileVisual(s,muzzle);
        s.visual.rotation=Quaternion.LookRotation(direction);
        if(!stop)
        {
            var trail=s.visual.gameObject.AddComponent<TrailRenderer>();
            trail.sharedMaterial=Glow(TeamColor(owner));
            trail.time=tracking>0?.18f:owner.spec.primary==PrimaryKind.Rail?.11f:.065f;
            trail.startWidth=physicalRadius*.8f; trail.endWidth=.015f; trail.minVertexDistance=.15f;
        }
        Flash(muzzle,TeamColor(owner),.45f,.08f);
        shots.Add(s); projectilesCreated++; return s;
    }
    void Attack(Fighter owner,Fighter target,Vector3 direction)
    {
        if(!smoke)
        {
            if(Finished||owner.lockTime>0||owner.rushTime>0) return;
            if(owner.weaponSlot!=1) return;
            if(owner.reloadTime>0||owner.magazine<=0||owner.weaponTimer>0||!InArc(owner,direction)) return;
            owner.weaponTimer=.105f*weaponCooldownScale/Mathf.Max(.2f,owner.spec.rightArmHandling); owner.magazine--;
            PlaySound(SoundKind.Fire,owner.root.position,.32f,1.45f);
            float spread=owner.spec.aimSpreadDegrees/Mathf.Max(.2f,owner.spec.rightArmStability);
            Vector3 fired=Quaternion.Euler(0,UnityEngine.Random.Range(-spread,spread),0)*direction;
            Fire(owner,fired,7,132,.24f,82);
            if(owner.magazine<=0) owner.reloadTime=1.65f;
            return;
        }
        if(Finished||owner.weaponTimer>0||owner.lockTime>0||owner.rushTime>0||!InArc(owner,direction)) return;
        CraftSpec spec=owner.spec; owner.weaponTimer=spec.weaponCooldown*weaponCooldownScale;
        direction=direction.normalized;
        float pitch=spec.primary==PrimaryKind.Plasma?.65f:spec.primary==PrimaryKind.Gatling?1.5f:
            spec.primary==PrimaryKind.Grenade?.72f:spec.primary==PrimaryKind.Missiles?.85f:spec.primary==PrimaryKind.Rail?1.25f:1;
        PlaySound(SoundKind.Fire,owner.root.position,.45f,pitch);
        switch(spec.primary)
        {
            case PrimaryKind.SplitCenter:
                Fire(owner,Quaternion.Euler(0,-9,0)*direction,spec.damage,spec.projectileSpeed,spec.projectileRadius,spec.range);
                Fire(owner,Quaternion.Euler(0,9,0)*direction,spec.damage,spec.projectileSpeed,spec.projectileRadius,spec.range);
                Vector3 centerAim=FollowupAim(owner,target,direction,.16f,1.15f);
                Schedule(.16f,()=>Fire(owner,centerAim,spec.damage*1.4f,spec.projectileSpeed*1.15f,spec.projectileRadius,spec.range)); break;
            case PrimaryKind.Plasma:
                Fire(owner,direction,spec.damage,spec.projectileSpeed,spec.projectileRadius,spec.range,0,3); break;
            case PrimaryKind.Gatling:
                for(int i=0;i<6;i++)
                {
                    Vector3 d=Quaternion.Euler(0,i%2==0?-1.5f:1.5f,0)*direction;
                    Schedule(i*.075f,()=>Fire(owner,d,spec.damage,spec.projectileSpeed,spec.projectileRadius,spec.range));
                }
                break;
            case PrimaryKind.Missiles:
                foreach(float a in new[]{-14f,14f}) Fire(owner,Quaternion.Euler(0,a,0)*direction,spec.damage,spec.projectileSpeed,spec.projectileRadius,spec.range,1.1f,2.3f);
                break;
            case PrimaryKind.Scatter:
                for(int i=0;i<7;i++) Fire(owner,Quaternion.Euler(0,-15+i*5,0)*direction,spec.damage,spec.projectileSpeed,spec.projectileRadius,spec.range);
                break;
            case PrimaryKind.Rail:
                Fire(owner,direction,spec.damage,spec.projectileSpeed,spec.projectileRadius,spec.range); break;
            case PrimaryKind.Wave:
                for(int i=0;i<3;i++)
                {
                    Vector3 d=Quaternion.Euler(0,-7+i*7,0)*direction;
                    Schedule(i*.12f,()=>Fire(owner,d,spec.damage,spec.projectileSpeed,spec.projectileRadius,spec.range,0,0,false,5));
                }
                break;
            case PrimaryKind.Grenade:
                Fire(owner,direction,spec.damage,spec.projectileSpeed,spec.projectileRadius,spec.range,0,4.5f); break;
        }
    }
    void Special(Fighter owner,Fighter target,Vector3 aim)
    {
        if(Finished||owner.specialTimer>0||owner.lockTime>0||owner.rushTime>0) return;
        Vector3 d=InArc(owner,aim)?aim.normalized:owner.facing;
        owner.specialTimer=owner.spec.specialCooldown/Mathf.Max(.2f,owner.spec.leftArmHandling);
        PlaySound(SoundKind.Special,owner.root.position,.7f);
        switch(owner.spec.special)
        {
            case SpecialKind.SweepBeam: AddBeam(owner,d,.16f,.65f,1.1f,55,65,48,false); break;
            case SpecialKind.SiegeBeam: AddBeam(owner,d,.30f,.85f,2.0f,75,70,0,true); break;
            case SpecialKind.RailLance: AddBeam(owner,d,.24f,.18f,.9f,95,350,0,true); break;
            case SpecialKind.SaberRush:
                owner.rushDirection=(target.root.position-owner.root.position).normalized;
                owner.rushTime=.5f; owner.rushHit=false; owner.pulseTime=0;
                owner.saber.gameObject.SetActive(true); break;
            case SpecialKind.MissileFan:
                for(int i=0;i<9;i++)
                {
                    Vector3 dir=Quaternion.Euler(0,-52+i*13,0)*d;
                    Schedule(i*.04f,()=>Fire(owner,dir,13,40,.4f,72,1.2f,3));
                }
                break;
            case SpecialKind.Nova:
                for(int i=0;i<16;i++) Fire(owner,Quaternion.Euler(0,i*22.5f,0)*Vector3.forward,12,44,.48f,30,0,1.2f);
                break;
            case SpecialKind.HomingMines:
                foreach(float a in new[]{-25f,0,25f}) Fire(owner,Quaternion.Euler(0,a,0)*d,28,38,.36f,80,.85f,5.5f,true);
                break;
            case SpecialKind.Minefield:
                for(int i=0;i<5;i++)
                {
                    Vector3 p=owner.root.position+Quaternion.Euler(0,i*72,0)*Vector3.forward*5;
                    if(Occupied(p,.5f)) continue;
                    var s=new Shot{owner=owner,damage=28,radius=.5f,life=5.5f,blast=4.3f,mine=true,arm=.8f};
                    s.visual=Shape(PrimitiveType.Sphere,"Proximity mine",null,p,Vector3.one*.75f,TeamColor(owner),true);
                    s.warning=Line("Mine warning",TeamColor(owner),.06f); shots.Add(s); projectilesCreated++;
                }
                break;
        }
    }
    void AddBeam(Fighter owner,Vector3 d,float charge,float duration,float width,float length,float dps,float sweep,bool anchored)
    {
        beams.Add(new Beam{owner=owner,direction=d,charge=charge,duration=duration,width=width,length=length,dps=dps,sweep=sweep,
            visual=Line("Beam telegraph",TeamColor(owner),.07f)});
        if(anchored) { owner.lockTime=charge+duration; owner.pulseTime=0; owner.velocity=Vector3.zero; }
    }
    void CheckSaber(Fighter f,Vector3 from,Vector3 to)
    {
        if(f.rushHit) return;
        Fighter target=Opponent(f);
        if(SegmentSphere(from,to,target.root.position,target.Radius+2.5f,out _))
        {
            if(!ObstacleOnSegment(to,target.root.position,.1f,out _))
            { Hit(target,to,58); f.rushHit=true; PlaySound(SoundKind.Hit,to,.8f); }
        }
        f.saber.localRotation=Quaternion.Euler(0,Mathf.Lerp(-65,70,1-f.rushTime/.5f),0);
    }
    bool SegmentSphere(Vector3 from,Vector3 to,Vector3 center,float radius,out float distance)
    {
        Vector3 d=to-from,m=from-center; float length=d.magnitude; distance=float.PositiveInfinity;
        if(m.sqrMagnitude<=radius*radius) { distance=0; return true; }
        if(length<.0001f) return false;
        Vector3 direction=d/length; float b=Vector3.Dot(m,direction),c=m.sqrMagnitude-radius*radius;
        if(b>0) return false; float discriminant=b*b-c;
        if(discriminant<0) return false;
        distance=-b-Mathf.Sqrt(discriminant); return distance>=0&&distance<=length;
    }
    void Circle(LineRenderer line,Vector3 p,float radius)
    {
        line.positionCount=41;
        for(int i=0;i<=40;i++) { float a=i*Mathf.PI*2/40; line.SetPosition(i,p+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius)); }
    }
    void Explode(Shot s,Vector3 position)
    {
        Fighter target=Opponent(s.owner);
        if(Vector3.Distance(position,target.root.position)<=s.blast+target.Radius &&
            !ObstacleOnSegment(position,target.root.position,.02f,out _)) Hit(target,position,s.damage);
        var ring=new RingEffect{line=Line("Explosion",TeamColor(s.owner),.22f),center=position,radius=s.blast,life=.42f};
        rings.Add(ring); Flash(position,TeamColor(s.owner),s.blast*.6f,.22f); Burst(position,TeamColor(s.owner),8); PlaySound(SoundKind.Explosion,position,.6f);
    }
    void UpdateCombat(float dt)
    {
        if(Finished) return;
        for(int i=pending.Count-1;i>=0;i--)
        {
            Pending p=pending[i]; p.delay-=dt;
            if(p.delay<=0) { pending.RemoveAt(i); p.action(); }
        }
        UpdateShots(dt); UpdateBeams(dt); UpdateFlashes(dt);
        for(int i=rings.Count-1;i>=0;i--)
        {
            RingEffect r=rings[i]; r.age+=dt;
            Circle(r.line,r.center,r.radius*Mathf.Min(1,r.age/.18f));
            r.line.startWidth=r.line.endWidth=Mathf.Max(.01f,.22f*(1-r.age/r.life));
            if(r.age>=r.life) { Destroy(r.line.gameObject); rings.RemoveAt(i); }
        }
    }
    void UpdateShots(float dt)
    {
        for(int i=shots.Count-1;i>=0;i--)
        {
            Shot s=shots[i]; Fighter target=Opponent(s.owner); Vector3 from=s.visual.position;
            s.age+=dt; s.arm=Mathf.Max(0,s.arm-dt);
            if(s.fuse>=0)
            {
                s.fuse-=dt; if(s.warning==null) s.warning=Line("Blast warning",TeamColor(s.owner),.1f);
                Circle(s.warning,from,s.blast);
                s.visual.localScale=Vector3.one*(1+Mathf.Sin(s.age*30)*.12f);
                if(s.fuse<=0) { Explode(s,from); RemoveShot(i); }
                continue;
            }
            if(s.mine)
            {
                Circle(s.warning,from,s.blast);
                if(s.arm<=0 && Vector3.Distance(from,target.root.position)<s.blast+1) s.fuse=.5f;
                s.life-=dt;
                if(s.life<=0) { Explode(s,from); RemoveShot(i); }
                continue;
            }
            if(s.tracking>0)
            {
                // Practice missiles commit to a straight final approach, giving a lateral boost
                // a real escape window. Player missiles and higher difficulties keep their guidance.
                if(!s.stopAndExplode&&s.owner==enemy&&difficulty==0&&s.age>=.12f&&Vector3.Distance(from,target.root.position)<16)
                    s.tracking=0;
                else
                {
                    Vector3 toward=(target.root.position-from).normalized;
                    s.velocity=Vector3.RotateTowards(s.velocity.normalized,toward,3.2f*dt,0)*s.velocity.magnitude;
                    s.tracking=Mathf.Max(0,s.tracking-dt);
                    if(s.tracking<=0 && s.stopAndExplode) { s.velocity=Vector3.zero; s.fuse=.32f; continue; }
                }
            }
            Vector3 velocity=s.velocity;
            if(s.wave>0) velocity+=Vector3.Cross(s.heading,Vector3.up)*Mathf.Sin(s.age*10)*s.wave;
            Vector3 to=from+velocity*Mathf.Min(dt,s.life);
            bool hit=!s.stopAndExplode && SegmentSphere(from,to,target.root.position,target.Radius+s.radius,out float unused);
            float targetDistance=float.PositiveInfinity;
            if(hit) SegmentSphere(from,to,target.root.position,target.Radius+s.radius,out targetDistance);
            bool cover=ObstacleOnSegment(from,to,s.radius,out float coverDistance);
            if(cover && (!hit || coverDistance<=targetDistance))
            {
                Vector3 impact=from+(to-from).normalized*Mathf.Max(0,coverDistance-.12f);
                if(s.blast>0) Explode(s,impact); else Burst(impact,TeamColor(s.owner),3);
                RemoveShot(i); continue;
            }
            if(hit)
            {
                Vector3 impact=from+(to-from).normalized*targetDistance;
                if(s.blast>0) Explode(s,impact); else Hit(target,from,s.damage);
                RemoveShot(i); continue;
            }
            s.visual.position=to;
            if(velocity.sqrMagnitude>.01f) s.visual.rotation=Quaternion.LookRotation(velocity);
            s.life-=dt;
            if(s.life<=0) { if(s.blast>0) Explode(s,to); RemoveShot(i); }
        }
    }
    void RemoveShot(int i)
    {
        Shot s=shots[i]; Destroy(s.visual.gameObject);
        if(s.warning!=null) Destroy(s.warning.gameObject); shots.RemoveAt(i);
    }
    void UpdateBeams(float dt)
    {
        for(int i=beams.Count-1;i>=0;i--)
        {
            Beam b=beams[i]; float oldAge=b.age; b.age+=dt;
            float progress=Mathf.Clamp01((b.age-b.charge)/b.duration);
            Vector3 d=Quaternion.Euler(0,Mathf.Lerp(-b.sweep/2,b.sweep/2,progress),0)*b.direction;
            Vector3 from=b.owner.root.position+d*(b.owner.Radius+.2f),to=from+d*b.length;
            float width=b.age<b.charge?.04f:b.width;
            // A local muzzle charge replaces the full-length advance aiming line.
            if(b.age<b.charge) to=from+d*1.1f;
            if(ObstacleOnSegment(b.owner.root.position,to,width/2,out float wall)) to=b.owner.root.position+d*Mathf.Max(0,wall-.05f);
            b.visual.SetPosition(0,from); b.visual.SetPosition(1,to); b.visual.startWidth=b.visual.endWidth=width;
            float active=Mathf.Max(0,Mathf.Min(b.age,b.charge+b.duration)-Mathf.Max(oldAge,b.charge));
            if(active>0)
            {
                if(!b.sounded) { PlaySound(SoundKind.Laser,from,.75f); b.sounded=true; }
                Fighter target=Opponent(b.owner);
                if(SegmentSphere(from,to,target.root.position,target.Radius+b.width/2,out _)) Hit(target,from,b.dps*active);
            }
            if(b.age>=b.charge+b.duration) { Destroy(b.visual.gameObject); beams.RemoveAt(i); }
        }
    }
}
