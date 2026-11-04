using UnityEngine;
public sealed partial class VoxArena
{
    const float TrackArcRadius=.38f,TrackHalfStraight=.41f;
    static float CapsuleTrackLength=>4*TrackHalfStraight+2*Mathf.PI*TrackArcRadius;
    static Vector3 CapsuleTrackPoint(float distance,out float angle)
    {
        float straight=2*TrackHalfStraight,arc=Mathf.PI*TrackArcRadius;distance=Mathf.Repeat(distance,CapsuleTrackLength);
        if(distance<straight){angle=0;return new Vector3(0,TrackArcRadius,-TrackHalfStraight+distance);}distance-=straight;
        if(distance<arc){angle=distance/TrackArcRadius;return new Vector3(0,TrackArcRadius*Mathf.Cos(angle),TrackHalfStraight+TrackArcRadius*Mathf.Sin(angle));}distance-=arc;
        if(distance<straight){angle=Mathf.PI;return new Vector3(0,-TrackArcRadius,TrackHalfStraight-distance);}angle=(distance-straight)/TrackArcRadius;Vector3 p=new Vector3(0,-TrackArcRadius*Mathf.Cos(angle),-TrackHalfStraight-TrackArcRadius*Mathf.Sin(angle));angle+=Mathf.PI;return p;
    }
    void ConnectCapsuleTreads(Fighter f,Transform root)
    {
        f.capsuleTreads=false;foreach(Transform child in root.GetComponentsInChildren<Transform>(true))if(child.name=="TrackPath_Capsule_v001")f.capsuleTreads=true;
        if(!f.capsuleTreads)return;f.importedTrackTravel=new float[2];f.importedTreadRotations=new Quaternion[2][];f.importedRoadWheels=new Transform[2][];f.importedWheelRest=new Quaternion[2][];f.importedWheelTurns=new float[2][];f.previousTreadHeading=f.chassisForward;
        for(int side=0;side<2;side++)
        {
            f.importedTreadRotations[side]=new Quaternion[40];for(int i=0;i<40;i++)f.importedTreadRotations[side][i]=f.treadLinks[side][i].localRotation;
            f.importedRoadWheels[side]=new Transform[5];f.importedWheelRest[side]=new Quaternion[5];f.importedWheelTurns[side]=new float[5];
            for(int i=0;i<5;i++){Transform wheel=FindJoint(root,"Wheel_"+side+"_"+i);f.importedRoadWheels[side][i]=wheel;f.importedWheelRest[side][i]=wheel.localRotation;}
        }
    }
    void ResetCapsuleTreads(Fighter f)
    {
        if(!f.capsuleTreads||f.importedTrackTravel==null)return;
        System.Array.Clear(f.importedTrackTravel,0,f.importedTrackTravel.Length);
        foreach(float[] turns in f.importedWheelTurns)System.Array.Clear(turns,0,turns.Length);
        f.previousTreadHeading=f.chassisForward;
    }
    void AnimateCapsuleTreads(Fighter f,float dt)
    {
        Vector3 heading=Vector3.ProjectOnPlane(f.chassisForward,Vector3.up).normalized;if(heading.sqrMagnitude<.1f)heading=Vector3.forward;
        float yaw=Vector3.SignedAngle(f.previousTreadHeading,heading,Vector3.up)*Mathf.Deg2Rad;f.previousTreadHeading=heading;
        float travel=Vector3.Dot(f.velocity,heading)*Mathf.Max(0,dt)/Mathf.Max(.01f,f.legsPart.lossyScale.y);
        for(int side=0;side<2;side++)
        {
            float step=travel+(side==0?1:-1)*yaw*.81f;f.importedTrackTravel[side]=Mathf.Repeat(f.importedTrackTravel[side]+step,CapsuleTrackLength);
            for(int i=0;i<40;i++)
            {
                float start=i*CapsuleTrackLength/40;Vector3 rest=CapsuleTrackPoint(start,out float restAngle),now=CapsuleTrackPoint(start+f.importedTrackTravel[side],out float angle);
                f.treadLinks[side][i].localPosition=f.importedTreadRest[side][i]+now-rest;f.treadLinks[side][i].localRotation=Quaternion.AngleAxis((angle-restAngle)*Mathf.Rad2Deg,Vector3.right)*f.importedTreadRotations[side][i];
            }
            for(int i=0;i<5;i++){float radius=i==0||i==4?.32f:.16f;f.importedWheelTurns[side][i]=Mathf.Repeat(f.importedWheelTurns[side][i]+step/radius,Mathf.PI*2);f.importedRoadWheels[side][i].localRotation=Quaternion.AngleAxis(f.importedWheelTurns[side][i]*Mathf.Rad2Deg,Vector3.right)*f.importedWheelRest[side][i];}
        }
    }
}
