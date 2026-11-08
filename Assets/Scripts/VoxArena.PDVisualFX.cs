using System;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class VoxArena
{
    sealed class VisualParticle {public Transform visual;public Vector3 velocity,spin;public Color color;public float age,life,size;public bool smoke;}
    sealed class FlyingModule {public Transform part,parent;public Vector3 position,scale,velocity;public Quaternion rotation;}
    readonly Dictionary<string,Stack<Transform>> fxPools=new Dictionary<string,Stack<Transform>>();
    readonly Dictionary<Transform,string> fxKinds=new Dictionary<Transform,string>();
    readonly List<VisualParticle> visualParticles=new List<VisualParticle>();
    readonly List<FlyingModule> flyingModules=new List<FlyingModule>();
    Transform fxPoolRoot;Material fxSolid,fxSoft;MaterialPropertyBlock fxProperties;
    int fxPoolCapacity;
    void BuildPDVisualEffects()
    {
        fxProperties=new MaterialPropertyBlock();
        fxPoolRoot=new GameObject("Bounded Blender visual effect pool").transform;
        fxSolid=new Material(hullShader);fxSolid.EnableKeyword("_EMISSION");fxSolid.color=Color.white;
        fxSoft=TransparentMaterial(Color.white);fxSoft.EnableKeyword("_EMISSION");
        string[] kinds={"RockFragment","ArmorFragment","SparkChunk","SmokePuff","WaterDrop","Ripple","Crack"};int[] counts={128,64,96,48,40,24,32};
        for(int k=0;k<kinds.Length;k++)
        {
            Stack<Transform> pool=new Stack<Transform>();fxPools[kinds[k]]=pool;
            for(int i=0;i<counts[k];i++)
            {
                Transform part=SpawnPDPart(kinds[k],fxPoolRoot,Vector3.zero,Vector3.one);
                SetPartMaterial(part,kinds[k]=="SmokePuff"||kinds[k]=="Ripple"?fxSoft:fxSolid);
                fxKinds[part]=kinds[k];part.gameObject.SetActive(false);pool.Push(part);fxPoolCapacity++;
            }
        }
    }
    void ColorPDPart(Transform part,Color color,bool glow=false)
    {
        fxProperties.Clear();fxProperties.SetColor("_Color",color);fxProperties.SetColor("_EmissionColor",glow?color*2:Color.black);
        foreach(Renderer r in part.GetComponentsInChildren<Renderer>())r.SetPropertyBlock(fxProperties);
    }
    Transform AcquirePDPart(string kind,Vector3 position,Vector3 scale,Color color)
    {
        if(!fxPools.TryGetValue(kind,out Stack<Transform> pool)||pool.Count==0)return null;
        Transform part=pool.Pop();part.SetParent(fxPoolRoot,false);part.position=position;part.rotation=Quaternion.identity;part.localScale=scale;part.gameObject.SetActive(true);ColorPDPart(part,color,kind=="SparkChunk"||kind=="WaterDrop");return part;
    }
    void ReleasePDPart(Transform part)
    {
        if(part==null)return;
        if(!fxKinds.TryGetValue(part,out string kind)){Destroy(part.gameObject);return;}
        part.gameObject.SetActive(false);part.SetParent(fxPoolRoot,false);fxPools[kind].Push(part);
    }
    void EmitPDParticle(string kind,Vector3 position,Vector3 velocity,float size,Color color,float life,bool smoke=false)
    {
        if(visualParticles.Count>=160)return;
        Transform part=AcquirePDPart(kind,position,Vector3.one*size,color);if(part==null)return;
        visualParticles.Add(new VisualParticle{visual=part,velocity=velocity,spin=new Vector3(45,90,60)*(size+.3f),color=color,life=life,size=size,smoke=smoke});
    }
    void NotifyPartDamage(Fighter f,Vector3 point,float damage)
    {
        if(f.partDamage==null)f.partDamage=new float[6];
        Transform[] parts={f.headPart,f.corePart,f.leftArm,f.rightArm,f.leftLeg,f.rightLeg};int closest=1;float distance=float.PositiveInfinity;
        for(int i=0;i<parts.Length;i++)if(parts[i]!=null){float d=(parts[i].position-point).sqrMagnitude;if(d<distance){closest=i;distance=d;}}
        f.partDamage[closest]+=damage;f.lastDamagedPart=closest;
        for(int i=0;i<4;i++)EmitPDParticle("SparkChunk",point,UnityEngine.Random.onUnitSphere*3,.08f,Orange,.3f);
    }
    void SetupBoostVisual(Fighter f)
    {
        f.boostVisual=new GameObject("Reusable booster exhaust").transform;f.boostVisual.SetParent(f.body,false);
        f.boostEmitter=f.boostVisual.gameObject.AddComponent<BoosterEmitter>();
        foreach(Renderer r in f.thruster.GetComponentsInChildren<Renderer>())r.enabled=false;
        foreach(Renderer r in f.jumpFlame.GetComponentsInChildren<Renderer>())r.enabled=false;
    }
    void AnimateBoostVisual(Fighter f,float dt)
    {
        if(f.boostEmitter==null)return;
        f.boostVisual.gameObject.SetActive(true);
        bool vertical=f.spec.allowVerticalBoost&&f.jumpHeld&&f.boostEnergy>0&&!f.boostLocked;
        bool ground=(f.boosting||f.dashRemaining>0)&&f.jumpHeight<=.08f;
        bool visible=!inMenu&&!Finished&&!f.deadVisual&&(vertical||ground);
        Vector3 origin=f.backPart!=null?f.backPart.TransformPoint(new Vector3(0,-.35f,-.18f)):f.root.position+Vector3.down*.55f;
        Vector3 direction=vertical?Vector3.down:f.velocity.sqrMagnitude>.5f?-f.velocity.normalized:-f.facing;
        if(!visible&&f.boostEmitter.Visible)direction=f.boostVisual.forward;
        f.boostEmitter.SetWorldPose(origin,direction);f.boostEmitter.SetThrust(visible?1:0);f.exhaustBlend=f.boostEmitter.Envelope;
    }
    void BreakRobotModules(Fighter f)
    {
        if(f.deadVisual)return;f.deadVisual=true;
        foreach(Transform t in new[]{f.headPart,f.leftArm,f.rightArm,f.legsPart})
        {
            if(t==null)continue;FlyingModule m=new FlyingModule{part=t,parent=t.parent,position=t.localPosition,rotation=t.localRotation,scale=t.localScale,velocity=UnityEngine.Random.onUnitSphere*4+Vector3.up*4};
            t.SetParent(fxPoolRoot,true);flyingModules.Add(m);
        }
        for(int i=0;i<28;i++)EmitPDParticle("ArmorFragment",f.root.position,UnityEngine.Random.onUnitSphere*6+Vector3.up*3,i%6==0?.55f:.16f,f.spec.color,3.5f);
        for(int i=0;i<8;i++)EmitPDParticle("SmokePuff",f.root.position,Vector3.up*(1+i*.15f),.4f,new Color(.20f,.23f,.25f,.5f),2.8f,true);
        EmitPDParticle("Ripple",f.root.position,Vector3.zero,1,new Color(1,.68f,.25f,.45f),.6f,true);
    }
    void RestorePDVisuals()
    {
        foreach(FlyingModule m in flyingModules)if(m.part!=null&&m.parent!=null){m.part.SetParent(m.parent,false);m.part.localPosition=m.position;m.part.localRotation=m.rotation;m.part.localScale=m.scale;}
        flyingModules.Clear();foreach(VisualParticle p in visualParticles)ReleasePDPart(p.visual);visualParticles.Clear();
        foreach(Fighter f in new[]{player,enemy})if(f!=null){f.deadVisual=false;f.partDamage=new float[6];f.exhaustBlend=0;if(f.boostEmitter!=null)f.boostEmitter.ResetEmission();f.damageSmokeClock=0;if(f.boostVisual!=null)f.boostVisual.gameObject.SetActive(false);}
    }
    void UpdatePDVisualEffects(float dt)
    {
        if(paused||fxPoolRoot==null)return;
        for(int i=visualParticles.Count-1;i>=0;i--)
        {
            VisualParticle p=visualParticles[i];p.age+=dt;if(p.age>=p.life){ReleasePDPart(p.visual);visualParticles.RemoveAt(i);continue;}
            if(!p.smoke)p.velocity+=Vector3.down*14*dt;p.visual.position+=p.velocity*dt;p.visual.Rotate(p.spin*dt,Space.World);
            float floor=SurfaceHeight(p.visual.position)+p.size*.3f;if(!p.smoke&&p.visual.position.y<floor){Vector3 at=p.visual.position;at.y=floor;p.visual.position=at;p.velocity*=.25f;p.velocity.y=Mathf.Abs(p.velocity.y)*.2f;}
            float fade=1-p.age/p.life;p.visual.localScale=Vector3.one*p.size*(p.smoke?1+p.age*2:Mathf.Min(1,fade*4));Color color=p.color;color.a*=fade;ColorPDPart(p.visual,color,fxKinds[p.visual]=="SparkChunk");
        }
        foreach(FlyingModule m in flyingModules)if(m.part!=null)
        {
            m.velocity+=Vector3.down*15*dt;Vector3 next=m.part.position+m.velocity*dt;float floor=SurfaceHeight(next)+.4f;
            if(next.y<floor){next.y=floor;m.velocity=Vector3.zero;}m.part.position=next;if(m.velocity.sqrMagnitude>.01f)m.part.Rotate(new Vector3(35,80,25)*dt,Space.World);
        }
        if(inMenu)return;
        foreach(Fighter f in new[]{player,enemy})
        {
            if(f==null||f.deadVisual||f.partDamage==null)continue;
            f.damageSmokeClock+=dt;
            if(f.damageSmokeClock>.55f)
            {
                Transform[] parts={f.headPart,f.corePart,f.leftArm,f.rightArm,f.leftLeg,f.rightLeg};
                for(int i=0;i<6;i++)if(parts[i]!=null&&f.partDamage[i]>f.spec.health*.10f)
                {
                    Vector3 outward=parts[i].position-f.corePart.position;if(outward.sqrMagnitude<.01f)outward=-f.facing;
                    Vector3 smokeOrigin=parts[i].position+outward.normalized*.42f*f.spec.size+Vector3.up*.50f;
                    EmitPDParticle("SmokePuff",smokeOrigin,Vector3.up*1.35f,.32f,new Color(.13f,.15f,.17f,.65f),1.2f,true);
                    if(f.partDamage[i]>f.spec.health*.22f)EmitPDParticle("SparkChunk",parts[i].position,UnityEngine.Random.onUnitSphere,.055f,Orange,.2f);
                }
                f.damageSmokeClock=0;
            }
        }
    }
    void VisualEffectChecks(Action<bool,string> check)
    {
        TestSetupSelectable(0,1);int capacity=fxPoolCapacity;Vector3 position=player.root.position;
        for(int i=0;i<400;i++)EmitPDParticle("SparkChunk",position,Vector3.up,.1f,Orange,.2f);
        check(visualParticles.Count<=160&&fxPoolCapacity==capacity,"visual effects use a fixed pool under burst load");
        UpdatePDVisualEffects(.3f);check(visualParticles.Count==0,"expired particles return to pool");
        NotifyPartDamage(player,player.leftArm.position,40);check(player.partDamage[2]>0,"hit location accumulates left-arm damage");
        UpdatePDVisualEffects(.6f);check(visualParticles.Exists(p=>p.smoke&&p.visual.gameObject.activeSelf),"damaged joint emits a real pooled smoke mesh after the delay");
        Transform headParent=player.headPart.parent;BreakRobotModules(player);check(player.deadVisual&&flyingModules.Count==4&&player.headPart.parent==fxPoolRoot,"defeat separates real head arm and undercarriage modules");
        RestorePDVisuals();check(!player.deadVisual&&player.headPart.parent==headParent&&flyingModules.Count==0&&visualParticles.Count==0,"respawn restores rig hierarchy and clears damage effects");
        TestSetup();
    }
}
