using UnityEngine;

public sealed partial class VoxArena
{
    float ThreatAt(Fighter f,Vector3 velocity,float horizon)
    {
        float danger=0; Vector3 p=f.root.position;
        foreach(Shot s in shots)
        {
            if(s.owner==f) continue;
            if(s.mine || s.fuse>=0)
            {
                float distance=Vector3.Distance(p+velocity*.35f,s.visual.position);
                if(distance<s.blast+f.Radius+2) danger+=30*(s.blast+f.Radius+2-distance);
                continue;
            }
            Vector3 relative=s.visual.position-p,relativeVelocity=s.velocity-velocity;
            float t=relativeVelocity.sqrMagnitude>.01f?Mathf.Clamp(-Vector3.Dot(relative,relativeVelocity)/relativeVelocity.sqrMagnitude,0,horizon):0;
            float closest=(relative+relativeVelocity*t).magnitude;
            if(closest<f.Radius+s.radius+1.7f) danger+=(f.Radius+s.radius+1.7f-closest)*28;
        }
        foreach(Beam b in beams)
        {
            if(b.owner==f || b.age+horizon<b.charge) continue;
            Vector3 from=b.owner.root.position,to=from+b.direction*b.length;
            if(SegmentSphere(from,to,p+velocity*.3f,f.Radius+b.width+1,out _)) danger+=65;
        }
        return danger;
    }
    Vector3 AimLead(Fighter shooter,Fighter target)
    {
        Vector3 delta=target.root.position-shooter.root.position;
        float projectileSpeed=smoke?shooter.spec.projectileSpeed:132;
        float time=Mathf.Clamp(delta.magnitude/(projectileSpeed*bulletSpeedScale),0,1.2f);
        Vector3 point=target.root.position+shooter.smoothedTargetVelocity*time*(difficulty==0?.28f:difficulty==1?.52f:.65f);
        Vector3 aim=(point-shooter.root.position).normalized;
        return Quaternion.Euler(0,Mathf.Sin(shooter.aiClock*2.7f)*(difficulty==0?8:difficulty==1?4:1.7f),0)*aim;
    }
    // Legacy entry point for smoke tests and the current one-rival mode.
    void EnemyAI(float dt) => EnemyAI(enemy,player,dt);

    // Explicit actor/target pairing is independent of player/enemy field swaps.
    void EnemyAI(Fighter f,Fighter target,float dt)
    {
        if(!IsHostile(f,target)) return;
        bool testRival=testLabEnabled && f==enemy && !smoke;
        f.aiClock+=dt; f.aiReaction-=dt; f.orbitTime-=dt;
        f.smoothedTargetVelocity=Vector3.Lerp(f.smoothedTargetVelocity,target.velocity,1-Mathf.Exp(-5*dt));
        Vector3 toward=(target.root.position-f.root.position).normalized;
        float distance=Vector3.Distance(target.root.position,f.root.position);
        float personality=(f.frameIndex*0.618f+f.teamId*.37f)%1f;
        float preferred=Mathf.Clamp(f.spec.range*(.31f+personality*.24f),10,38);
        if(f.orbitTime<=0) { f.orbitTime=1.5f+personality*2.8f; f.orbitSign=-f.orbitSign; }
        if(f.velocity.magnitude<.6f && f.aiMove.sqrMagnitude>.1f) f.stuckTime+=dt; else f.stuckTime=0;
        if(f.aiReaction<=0)
        {
            f.aiReaction=(difficulty==0?.60f:difficulty==1?.34f:.23f)*(.85f+personality*.5f);
            bool obscured=ObstacleOnSegment(f.root.position,target.root.position,.2f,out _);
            Vector3 strafe=Vector3.Cross(toward,Vector3.up)*f.orbitSign;
            Vector3 goal=distance>preferred+4?toward+strafe*.3f:distance<preferred-5?-toward+strafe*.8f:strafe;
            if(obscured) goal=toward*.6f+strafe;
            goal+=strafe*(personality-.5f)*.6f;
            if(f.stuckTime>.65f) { goal=strafe; f.orbitSign=-f.orbitSign; f.stuckTime=0; }
            float currentThreat=ThreatAt(f,Vector3.zero,.75f),bestScore=float.NegativeInfinity;
            Vector3 best=Vector3.zero;
            for(int i=0;i<24;i++)
            {
                Vector3 candidate=Quaternion.Euler(0,i*15,0)*Vector3.forward;
                Vector3 end=f.root.position+candidate*f.spec.speed*.5f;
                if(ObstacleOnSegment(f.root.position,end,f.Radius+.1f,out _)) continue;
                float score=Vector3.Dot(candidate,goal.normalized)*6;
                float newDistance=Vector3.Distance(end,target.root.position);
                score-=Mathf.Abs(newDistance-preferred)*.12f;
                score-=ThreatAt(f,candidate*f.spec.boostSpeed,.75f)*(difficulty==0?.08f:difficulty==1?.32f:.62f);
                // Favor clear exits instead of retreating straight into a nearby rock.
                foreach(Bounds rock in obstacles)
                {
                    float clearance=Vector3.Distance(end,rock.ClosestPoint(end));
                    score-=Mathf.Max(0,3-clearance)*3;
                }
                if(score>bestScore) { bestScore=score; best=candidate; }
            }
            f.aiMove=best;
            f.aiState=currentThreat>3?"EVADE":obscured?"FLANK":distance>preferred+4?"CHASE":"ORBIT";
            bool wantBoost=(difficulty>0 && currentThreat>3) || distance>preferred+14 || obscured;
            if(difficulty==0 && f.aiClock%3.5f>1.1f) wantBoost=false;
            f.boostHeld=wantBoost && f.boostEnergy>f.spec.boostCapacity*.13f;
            if(f.spec.boost==BoostKind.Pulse) f.boostHeld&=(f.aiClock%1.4f)<.5f;
            if(f.boostLocked) f.boostHeld=false;
        }
        // Use aerial thrust to cross obstacles or dodge projectiles. Pilot timing differs by frame.
        bool obstacleAhead=ObstacleOnSegment(f.root.position+Vector3.up*.2f,
            f.root.position+f.aiMove.normalized*4f+Vector3.up*.2f,f.Radius*.65f,out _);
        bool threatened=ThreatAt(f,Vector3.zero,.55f)>14f;
        float jumpCycle=1.7f+personality*1.6f;
        bool jumpWindow=(f.aiClock+personality*2f)%jumpCycle<(.38f+personality*.25f);
        f.jumpHeld=(!testRival || aiJumpEnabled) && (obstacleAhead || (threatened && jumpWindow)) && f.boostEnergy>f.spec.boostCapacity*.18f;
        if(f.jumpHeld && f.jumpHeight<=.02f && f.jumpVelocity<=0) f.jumpVelocity=jumpSpeed;
        Vector3 movement=f.aiMove;
        // Practice rivals pause between maneuvers, while still escaping blocked routes.
        if(difficulty==0 && f.aiClock%3.2f>2.1f && !ObstacleOnSegment(f.root.position,target.root.position,.2f,out _)) movement*=.18f;
        if(testRival && !aiMoveEnabled) { movement=Vector3.zero; f.dashRemaining=0; }
        if(testRival && !aiBoostEnabled) { f.boostHeld=false; f.pulseTime=0; }
        Move(f,movement,dt); FaceOpponent(f,target);
        if(testRival && !aiAttackEnabled) return;
        if(f.aiClock<(difficulty==0?2.8f:1.3f)) return;
        bool lineClear=!ObstacleOnSegment(f.root.position,target.root.position,.2f,out _);
        Vector3 aim=AimLead(f,target);
        if(!smoke) f.weaponSlot=1;
        if(!smoke&&f.magazine<=0&&f.reloadTime<=0) f.reloadTime=1.65f;
        float attackRange=smoke?f.spec.range:82;
        if(lineClear && distance<attackRange && f.weaponTimer<=0 && (smoke||f.reloadTime<=0))
        {
            Attack(f,target,aim);
            if(f.weaponTimer>0) f.weaponTimer*=difficulty==0?1.65f:difficulty==1?1.2f:1;
        }
        if(smoke)
        {
            bool useSpecial=false;
            switch(f.spec.special)
            {
                case SpecialKind.SaberRush: useSpecial=distance>3&&distance<19&&lineClear; break;
                case SpecialKind.Nova: useSpecial=distance<21; break;
                case SpecialKind.Minefield: useSpecial=distance<23; break;
                case SpecialKind.HomingMines: useSpecial=distance<43; break;
                case SpecialKind.MissileFan: useSpecial=distance>12&&distance<55; break;
                default: useSpecial=lineClear&&distance>12&&distance<65&&ThreatAt(f,Vector3.zero,.8f)<8; break;
            }
            if(useSpecial&&f.aiClock>(difficulty==0?7:3)&&f.specialTimer<=0)
            { Special(f,target,aim); f.specialTimer*=difficulty==0?1.6f:difficulty==1?1.2f:1; }
        }
    }
}
