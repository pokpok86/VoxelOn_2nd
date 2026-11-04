using System.Collections.Generic;
using UnityEngine;
public sealed partial class VoxArena
{
    bool TryCoverRoute(Fighter f,Vector3 target,out Vector3 waypoint)
    {
        waypoint=Vector3.zero;float clearance=f.Radius+.35f;
        var nodes=new List<Vector3>{f.root.position,target};
        // Local recovery only: existing AI keeps its normal orbit/accuracy/reaction rules.
        foreach(Bounds wall in obstacles)
        {
            if(wall.max.y<=f.root.position.y||wall.min.y>f.root.position.y||Vector3.Distance(wall.ClosestPoint(f.root.position),f.root.position)>32)continue;
            foreach(float x in new[]{wall.min.x-clearance,wall.max.x+clearance})foreach(float z in new[]{wall.min.z-clearance,wall.max.z+clearance})
            {
                Vector3 p=new Vector3(x,f.root.position.y,z);if(Mathf.Abs(x)>ArenaLimit-clearance||Mathf.Abs(z)>ArenaLimit-clearance||Occupied(p,f.Radius+.1f))continue;
                nodes.Add(p);
            }
            if(nodes.Count>=50)break;
        }
        float[] cost=new float[nodes.Count];int[] previous=new int[nodes.Count];bool[] visited=new bool[nodes.Count];
        for(int i=0;i<nodes.Count;i++){cost[i]=float.PositiveInfinity;previous[i]=-1;}cost[0]=0;
        for(int step=0;step<nodes.Count;step++)
        {
            int current=-1;float best=float.PositiveInfinity;for(int n=0;n<nodes.Count;n++)if(!visited[n]&&cost[n]<best){best=cost[n];current=n;}
            if(current<0)break;if(current==1)break;visited[current]=true;
            for(int n=0;n<nodes.Count;n++)
            {
                if(visited[n]||n==current||ObstacleOnSegment(nodes[current],nodes[n],f.Radius+.1f,out _))continue;
                float distance=cost[current]+Vector3.Distance(nodes[current],nodes[n]);if(distance<cost[n]){cost[n]=distance;previous[n]=current;}
            }
        }
        if(previous[1]<0)return false;int next=1;for(int i=0;i<nodes.Count&&previous[next]>0;i++)next=previous[next];
        waypoint=nodes[next];return next!=1;
    }
    void UpdateCoverRoute(Fighter f,Fighter target)
    {
        if(!ObstacleOnSegment(f.root.position,target.root.position,.2f,out _)){f.aiCoverRouting=false;return;}
        bool arrived=f.aiCoverRouting&&Vector3.Distance(f.root.position,f.aiCoverWaypoint)<.6f;
        bool blocked=f.aiCoverRouting&&ObstacleOnSegment(f.root.position,f.aiCoverWaypoint,f.Radius+.08f,out _);
        if(arrived||blocked)f.aiCoverRouting=false;
        if(!f.aiCoverRouting&&f.aiClock>1.2f&&f.aiClock>=f.aiCoverRetry)
        {
            f.aiCoverRetry=f.aiClock+.8f;f.aiCoverRouting=TryCoverRoute(f,target.root.position,out f.aiCoverWaypoint);
        }
        if(f.aiCoverRouting){f.aiMove=(f.aiCoverWaypoint-f.root.position).normalized;f.aiState="COVER DETOUR";}
    }
}
