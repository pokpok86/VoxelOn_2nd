using UnityEngine;
public sealed partial class VoxArena
{
    [Header("Quadruped visual gait; no movement-stat changes")]
    public float quadWalkStride=1.3f,quadRunStride=4.8f,quadStepHeight=.24f;
    sealed class QuadPose
    {
        public Vector3 previous,heading,bodyScale;public float phase;public Vector3[] rest=new Vector3[4],feet=new Vector3[4],start=new Vector3[4],goal=new Vector3[4];public bool[] stance=new bool[4];
    }
    void AnimateQuadrupedGait(Fighter f,float stride,float dt)
    {
        bool air=f.jumpHeight>.08f;
        if(air)
        {
            f.quadPose=null;for(int i=0;i<4;i++){f.quadrupedLegs[i].localPosition=f.importedHipRest[i]+Vector3.up*.04f;f.quadrupedLegs[i].localRotation=Quaternion.Euler(8,0,0);f.quadrupedKnees[i].localRotation=Quaternion.Euler(15,0,0);f.quadrupedAnkles[i].rotation=Quaternion.LookRotation(f.facing,Vector3.up);}return;
        }
        float travelled=f.quadPose==null?0:Vector3.Distance(f.root.position,f.quadPose.previous);
        if(f.quadPose==null||travelled>3||(f.quadPose.bodyScale-f.body.lossyScale).sqrMagnitude>.0001f)
        {
            f.quadPose=new QuadPose{previous=f.root.position,heading=f.root.forward,bodyScale=f.body.lossyScale};
            for(int i=0;i<4;i++){f.quadrupedLegs[i].localPosition=f.importedHipRest[i];f.quadrupedLegs[i].localRotation=f.quadrupedKnees[i].localRotation=Quaternion.identity;Vector3 at=f.quadrupedAnkles[i].position;f.quadPose.rest[i]=f.root.InverseTransformPoint(at);f.quadPose.feet[i]=at;f.quadPose.stance[i]=true;}
            travelled=0;
        }
        QuadPose q=f.quadPose;float speed=Vector3.ProjectOnPlane(f.velocity,Vector3.up).magnitude,ratio=Mathf.Clamp01(speed/Mathf.Max(1,f.spec.speed));float length=Mathf.Lerp(quadWalkStride,quadRunStride,ratio);
        float turning=Mathf.Abs(Vector3.SignedAngle(q.heading,f.root.forward,Vector3.up))*Mathf.Deg2Rad*.65f;
        q.phase=Mathf.Repeat(q.phase+(travelled+turning)/Mathf.Max(.5f,length),1);q.previous=f.root.position;q.heading=f.root.forward;
        Vector3 direction=speed>.1f?Vector3.ProjectOnPlane(f.velocity,Vector3.up).normalized:f.root.forward;const float duty=.62f;
        for(int i=0;i<4;i++)
        {
            Transform hip=f.quadrupedLegs[i],knee=f.quadrupedKnees[i],foot=f.quadrupedAnkles[i];hip.localPosition=f.importedHipRest[i];hip.localRotation=knee.localRotation=Quaternion.identity;
            float cycle=Mathf.Repeat(q.phase+((i==0||i==3)?0:.5f),1);bool stance=cycle<duty;Vector3 nominal=f.root.TransformPoint(q.rest[i]);
            if(!stance&&q.stance[i]){q.start[i]=q.feet[i];q.goal[i]=nominal+direction*Mathf.Min(1.2f,length*.33f);}
            if(!stance)
            {
                float t=(cycle-duty)/(1-duty);q.feet[i]=Vector3.Lerp(q.start[i],q.goal[i],t*t*(3-2*t));
                q.feet[i].y=LandingHeight(new Vector3(q.feet[i].x,f.root.position.y,q.feet[i].z),.08f,f.root.position.y)-FighterSupportOffset+.075f*foot.lossyScale.y+Mathf.Sin(t*Mathf.PI)*quadStepHeight;
            }
            else
            {
                Vector3 offset=Vector3.ProjectOnPlane(q.feet[i]-nominal,Vector3.up);if(offset.magnitude>.85f){Vector3 at=nominal+offset.normalized*.85f;q.feet[i].x=at.x;q.feet[i].z=at.z;}
                q.feet[i].y=LandingHeight(new Vector3(q.feet[i].x,f.root.position.y,q.feet[i].z),.08f,f.root.position.y)-FighterSupportOffset+.075f*foot.lossyScale.y;
            }
            q.stance[i]=stance;SolveTwoBone(hip,knee,foot,q.feet[i],Vector3.up);foot.rotation=Quaternion.LookRotation(f.facing,Vector3.up);
        }
    }
}
