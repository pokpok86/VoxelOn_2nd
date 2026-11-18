using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    void CollisionBroadphaseChecks(Action<bool,string> check)
    {
        var rays=new List<Vector3[]>();var radii=new List<float>();var random=new System.Random(20261119);
        for(int i=0;i<4000;i++)
        {
            Vector3 Point()=>new Vector3((float)random.NextDouble()*280-140,(float)random.NextDouble()*22-3,(float)random.NextDouble()*280-140);
            rays.Add(new[]{Point(),Point()});radii.Add((float)random.NextDouble()*2);
        }
        foreach(Bounds block in obstacles)
        {
            foreach(float radius in new[]{0f,.3f,1.5f})
            {
                Vector3 start=block.center;start.y=block.max.y-.021f;rays.Add(new[]{start,start+Vector3.right*10});radii.Add(radius);
                start.y=block.max.y;rays.Add(new[]{start-Vector3.right*10,start+Vector3.right*10});radii.Add(radius);
                Vector3 corner=block.max+new Vector3(radius,0,radius);corner.y=block.center.y;rays.Add(new[]{corner,corner-Vector3.one*.01f});radii.Add(radius);
            }
        }
        bool identical=true;int mismatch=0;
        for(int i=0;i<rays.Count;i++)
        {
            bool expected=ObstacleOnSegmentCore(rays[i][0],rays[i][1],radii[i],out float oldDistance,false);
            bool actual=ObstacleOnSegment(rays[i][0],rays[i][1],radii[i],out float newDistance);
            bool same=expected==actual&&(float.IsPositiveInfinity(oldDistance)&&float.IsPositiveInfinity(newDistance)||Mathf.Abs(oldDistance-newDistance)<.0001f);
            if(!same){identical=false;mismatch++;}
        }
        check(identical,"broadphase matches reference hit and nearest distance for "+rays.Count+" seeded/roof/corner segments map="+arenaMap+" mismatches="+mismatch);
        Vector3 point=player.root.position;bool zeroReference=ObstacleOnSegmentCore(point,point,.3f,out float zeroOld,false),zeroActual=ObstacleOnSegment(point,point,.3f,out float zeroNew);
        check(zeroReference==zeroActual&&(zeroOld==zeroNew||Mathf.Abs(zeroOld-zeroNew)<.0001f),"zero-length obstacle query preserves original overlap semantics");
        long begin=System.Diagnostics.Stopwatch.GetTimestamp();for(int i=0;i<rays.Count;i++)ObstacleOnSegmentCore(rays[i][0],rays[i][1],radii[i],out _,false);float referenceMs=ProfileElapsed(begin);
        begin=System.Diagnostics.Stopwatch.GetTimestamp();for(int i=0;i<rays.Count;i++)ObstacleOnSegment(rays[i][0],rays[i][1],radii[i],out _);float optimizedMs=ProfileElapsed(begin);
        Debug.Log("COLLISION_BROADPHASE_PROFILE map="+arenaMap+" queries="+rays.Count+" referenceMs="+referenceMs+" optimizedMs="+optimizedMs);
    }
}
