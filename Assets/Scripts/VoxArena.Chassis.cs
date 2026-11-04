using System;
using UnityEngine;
public sealed partial class VoxArena
{
    Transform FindJoint(Transform root,string name)
    {
        foreach(Transform t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;
        throw new InvalidOperationException("Imported chassis joint missing: "+name);
    }
    void ConnectImportedChassis(Fighter f,Transform root)
    {
        // The old generated structures remain hidden for safe model fallback only.
        f.body.localScale=Vector3.one*f.spec.size;
        if(f.spec.mobility==MobilityKind.Tank)
        {
            f.treadLinks=new Transform[2][];f.importedTreadRest=new Vector3[2][];
            for(int side=0;side<2;side++)
            {
                f.treadLinks[side]=new Transform[40];f.importedTreadRest[side]=new Vector3[40];
                for(int i=0;i<40;i++)
                {
                    Transform link=FindJoint(root,"Tread_"+side+"_"+i.ToString("00"));
                    f.treadLinks[side][i]=link;f.importedTreadRest[side][i]=link.localPosition;
                }
            }
        }
        else
        {
            f.quadrupedLegs=new Transform[4];f.quadrupedKnees=new Transform[4];f.quadrupedAnkles=new Transform[4];f.importedHipRest=new Vector3[4];
            for(int i=0;i<4;i++)
            {
                f.quadrupedLegs[i]=FindJoint(root,"SpiderHip_"+i);f.quadrupedKnees[i]=FindJoint(root,"SpiderKnee_"+i);f.quadrupedAnkles[i]=FindJoint(root,"SpiderFoot_"+i);
                f.importedHipRest[i]=f.quadrupedLegs[i].localPosition;
            }
        }
        if(f.importedTreadRest!=null)ConnectCapsuleTreads(f,root);
        float radius=0,sole=float.PositiveInfinity;
        foreach(SkinnedMeshRenderer skin in f.body.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            Vector3[] vertices=skin.sharedMesh.vertices;BoneWeight[] weights=skin.sharedMesh.boneWeights;Matrix4x4[] bind=skin.sharedMesh.bindposes;
            for(int i=0;i<vertices.Length;i++)
            {
                int index=weights[i].boneIndex0;Transform bone=skin.bones[index];if(bone!=f.legsPart&&!bone.IsChildOf(f.legsPart))continue;
                Vector3 p=f.root.InverseTransformPoint((bone.localToWorldMatrix*bind[index]).MultiplyPoint3x4(vertices[i]));
                radius=Mathf.Max(radius,new Vector2(p.x,p.z).magnitude);sole=Mathf.Min(sole,p.y);
            }
        }
        f.importedRadius=radius+.03f;
        if(f.capsuleTreads&&!float.IsInfinity(sole)){f.importedChassisAnchor=f.root.InverseTransformPoint(f.legsPart.position);f.importedChassisAnchor.y-=sole+FighterSupportOffset;}
        Debug.Log("IMPORTED_CHASSIS_READY "+f.spec.name+" footprint="+f.importedRadius);

    }
    void AnimateImportedChassis(Fighter f,float stride,float dt)
    {
        if(f.importedTreadRest!=null)
        {
            if(f.capsuleTreads){AnimateCapsuleTreads(f,dt);return;}
            f.treadPhase+=Vector3.Dot(f.velocity,f.chassisForward)*Mathf.Max(0,dt)*.7f;
            for(int side=0;side<2;side++)for(int i=0;i<40;i++)
            {
                float start=i*Mathf.PI*2/40,a=start+f.treadPhase;
                Vector3 delta=new Vector3(0,.39f*(Mathf.Cos(a)-Mathf.Cos(start)),.80f*(Mathf.Sin(a)-Mathf.Sin(start)));
                f.treadLinks[side][i].localPosition=f.importedTreadRest[side][i]+delta;
                f.treadLinks[side][i].localRotation=Quaternion.Euler(-a*Mathf.Rad2Deg,0,0);
            }
            return;
        }
        if(f.importedHipRest==null)return;
        bool air=f.jumpHeight>.08f;
        for(int i=0;i<4;i++)
        {
            float phase=roundTime*9f+((i==0||i==3)?0:Mathf.PI);
            float cycle=Mathf.Sin(phase)*stride,lift=Mathf.Max(0,cycle)*.10f;
            Transform hip=f.quadrupedLegs[i],knee=f.quadrupedKnees[i],foot=f.quadrupedAnkles[i];
            hip.localPosition=f.importedHipRest[i]+Vector3.up*(air?.04f:lift);
            hip.localRotation=Quaternion.Euler(air?8:cycle*7,0,0);
            knee.localRotation=Quaternion.Euler(air?15:Mathf.Max(0,cycle)*9,0,0);
            foot.localRotation=Quaternion.Inverse(hip.localRotation*knee.localRotation);
            if(!air)
            {
                // Solve from the authored rest offsets, keeping the hip attached to the chassis.
                hip.localRotation=knee.localRotation=Quaternion.identity;
                Vector3 target=foot.position;float scale=foot.lossyScale.y;
                float floor=LandingHeight(new Vector3(target.x,f.root.position.y,target.z),.08f,f.root.position.y)-FighterSupportOffset;
                target.y=floor+.075f*scale+lift*scale;
                Vector3 start=hip.position,first=knee.position-start,second=foot.position-knee.position,d=target-start;
                float l1=first.magnitude,l2=second.magnitude,length=Mathf.Clamp(d.magnitude,Mathf.Abs(l1-l2)+.001f,l1+l2-.001f);
                Vector3 axis=d.normalized,bend=Vector3.ProjectOnPlane(first,axis).normalized;
                if(bend.sqrMagnitude<.01f)bend=Vector3.ProjectOnPlane(Vector3.up,axis).normalized;
                float along=(l1*l1-l2*l2+length*length)/(2*length),height=Mathf.Sqrt(Mathf.Max(0,l1*l1-along*along));
                Vector3 kneeTarget=start+axis*along+bend*height;
                hip.rotation=Quaternion.FromToRotation(first,kneeTarget-start)*hip.rotation;
                knee.rotation=Quaternion.FromToRotation(foot.position-knee.position,target-knee.position)*knee.rotation;
                foot.rotation=Quaternion.LookRotation(f.facing,Vector3.up);
            }
        }
    }
    void ChassisChecks(Action<bool,string> check)
    {
        bool prior=smoke;smoke=false;
        for(int i=0;i<3;i++)
        {
            TestSetupSelectable(i,(i+1)%3);player.root.position=new Vector3(100,FighterGroundY,100);player.facing=Vector3.forward;Animate(player);
            check(i==0?!player.importedChassis:player.importedChassis,"authored prototype imported "+i);
            check(player.headPart!=null&&player.armsPart!=null&&player.legsPart!=null&&player.weaponPart.IsChildOf(player.armsPart),"prototype upper modules and weapon socket "+i);
            Bounds bounds=new Bounds();bool found=false;float footprint=0;
            foreach(SkinnedMeshRenderer skin in player.body.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Mesh mesh=new Mesh();skin.BakeMesh(mesh,true);
                Vector3[] verts=mesh.vertices;BoneWeight[] weights=skin.sharedMesh.boneWeights;
                for(int v=0;v<verts.Length;v++){Vector3 p=skin.transform.TransformPoint(verts[v]);if(!found){bounds=new Bounds(p,Vector3.zero);found=true;}else bounds.Encapsulate(p);
                    Transform bone=skin.bones[weights[v].boneIndex0];if(bone==player.legsPart||bone.IsChildOf(player.legsPart)){Vector3 local=player.root.InverseTransformPoint(p);footprint=Mathf.Max(footprint,new Vector2(local.x,local.z).magnitude);}}

                Destroy(mesh);
            }
            check(found&&Mathf.Abs(bounds.min.y-GroundSurfaceY)<.04f&&bounds.size.y>3&&bounds.size.y<4.6f,"prototype genuine mesh scale and grounded sole "+i+" min="+bounds.min.y.ToString("F3")+" height="+bounds.size.y.ToString("F2"));
            if(i>0)check(player.Radius>=footprint-.01f&&Mathf.Abs(player.importedRadius-footprint)<.05f,"prototype collision encloses lateral undercarriage "+i);
            Vector3 start=player.root.position;for(int tick=0;tick<15;tick++)Move(player,Vector3.forward,.02f);
            check(Vector3.Distance(start,player.root.position)>1f,"prototype actual forward travel "+i);
            player.weaponTimer=0;player.magazine=30;enemy.root.position=player.root.position+Vector3.forward*18;int n=shots.Count;Attack(player,enemy,Vector3.forward);
            check(shots.Count==n+1&&player.magazine==29,"prototype live gun fire "+i);
            float hp=player.hp;Hit(player,enemy.root.position,10);check(player.hp<hp||player.shieldEnergy<player.spec.ShieldMax,"prototype receives damage "+i);
            if(i==1)
            {
                check(player.capsuleTreads,"tracked FBX carries capsule path metadata without changing old rigs");
                if(player.capsuleTreads)
                {
                    player.velocity=Vector3.forward*8;player.chassisForward=player.previousTreadHeading=Vector3.forward;Quaternion wheel=player.importedRoadWheels[0][0].localRotation;AnimateCapsuleTreads(player,.1f);
                    check(Quaternion.Angle(wheel,player.importedRoadWheels[0][0].localRotation)>1&&Mathf.Abs(player.importedTrackTravel[0]-player.importedTrackTravel[1])<.001f,"real drive wheels roll and straight tracks travel together");
                    player.velocity=Vector3.zero;player.chassisForward=Vector3.right;AnimateCapsuleTreads(player,.1f);
                    check(Mathf.Abs(player.importedTrackTravel[0]-player.importedTrackTravel[1])>.1f,"real left and right tracks advance differently during chassis yaw");
                }
            }
        }
        smoke=prior;TestSetup();
    }
}
