using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    sealed class Debris { public Transform visual; public Vector3 origin,size; public float phase; }
    readonly List<Bounds> obstacles=new List<Bounds>();
    readonly List<Debris> debris=new List<Debris>();
    Transform starRoot;
    float spaceClock;

    void BuildWorld()
    {
        RenderSettings.ambientLight=new Color(.55f,.59f,.64f); RenderSettings.fog=false;
        QualitySettings.antiAliasing=4; QualitySettings.shadows=ShadowQuality.All; QualitySettings.shadowDistance=140;
        Light light=new GameObject("Distant sun").AddComponent<Light>();
        light.type=LightType.Directional; light.intensity=1.05f; light.shadows=LightShadows.Soft; light.shadowStrength=.65f;
        light.transform.rotation=Quaternion.Euler(40,-30,0);
        view=new GameObject("Quarter view camera").AddComponent<Camera>(); view.tag="MainCamera";
        view.transform.position=new Vector3(46,58,-46); view.transform.LookAt(Vector3.zero);
        view.cullingMask=~(1<<8); view.orthographic=true; view.orthographicSize=24; view.farClipPlane=600;
        view.backgroundColor=new Color(.009f,.014f,.04f); view.clearFlags=CameraClearFlags.SolidColor;
        view.gameObject.AddComponent<AudioListener>();
        BuildGround();
        AddDebris(new Vector3(-16,1.1f,3),new Vector3(5,4.2f,5),0);
        AddDebris(new Vector3(15,1.1f,8),new Vector3(6,3.4f,5),1.4f);
        AddDebris(new Vector3(-5,1.1f,-19),new Vector3(4,1.8f,6),2.8f);
        AddDebris(new Vector3(6,1.1f,23),new Vector3(6,4.2f,4),4.1f);
        AddDebris(new Vector3(-30,1.1f,-25),new Vector3(5,3.4f,5),5.4f);
        AddDebris(new Vector3(28,1.1f,-22),new Vector3(4,1.8f,6),6.7f);
        AddDebris(new Vector3(-34,1.1f,12),new Vector3(7,2.6f,4),1.1f);
        AddDebris(new Vector3(34,1.1f,18),new Vector3(5,3.6f,7),2.2f);
        AddDebris(new Vector3(-22,1.1f,34),new Vector3(8,2.2f,5),3.3f);
        AddDebris(new Vector3(23,1.1f,-37),new Vector3(7,3.0f,5),4.4f);
        AddDebris(new Vector3(-39,1.1f,-8),new Vector3(5,4.0f,7),5.5f);
        AddDebris(new Vector3(40,1.1f,0),new Vector3(8,2.4f,5),6.6f);
        for(int side=-1;side<=1;side+=2) for(int i=0;i<5;i++)
            obstacles.Add(new Bounds(new Vector3(side*(48+i*14),2,45+i%2*16),new Vector3(10,9+i%3*3,13)));
        player=MakeFighter("Pilot frame",Cyan); enemy=MakeFighter("Rival frame",Orange);
        aimMarker=new GameObject("Aim reticle").transform;
        Shape(PrimitiveType.Cube,"Aim cross",aimMarker,Vector3.zero,new Vector3(.8f,.06f,.07f),Cyan,true);
        Shape(PrimitiveType.Cube,"Aim cross",aimMarker,Vector3.zero,new Vector3(.07f,.06f,.8f),Cyan,true);
    }
    Fighter MakeFighter(string name,Color accent)
    {
        Fighter f=new Fighter(); f.root=new GameObject(name).transform;
        f.shadow=Shape(PrimitiveType.Cylinder,"Contact shadow",null,new Vector3(0,-1.08f,0),new Vector3(2.7f,.012f,2.1f),new Color(.075f,.105f,.12f));
        f.thruster=new GameObject("Directional thruster").transform;
        for(int segment=0;segment<7;segment++)
        {
            float taper=1-segment/8f;
            Shape(PrimitiveType.Sphere,"Exhaust plasma",f.thruster,new Vector3(0,0,.25f+segment*.41f),
                new Vector3(.47f*taper,.35f*taper,.65f),Color.Lerp(new Color(.7f,.94f,1),new Color(.08f,.28f,.85f),segment/6f),true);
        }
        return f;
    }
    void AddDebris(Vector3 origin,Vector3 size,float phase)
    {
        Transform root=new GameObject("Ground cover "+debris.Count).transform;
        root.position=origin;
        BuildGroundCover(root,size,debris.Count);
        debris.Add(new Debris{visual=root,origin=origin,size=size,phase=phase});
        obstacles.Add(new Bounds(origin,size));
    }
    void UpdateObstacles(float dt)
    {
        spaceClock+=dt;
        for(int i=0;i<debris.Count;i++)
        {
            Debris d=debris[i];
            Vector3 p=d.origin; // Ground cover stays fixed so players can learn reliable escape routes.
            d.visual.position=p;
            // Collision bounds and the visual translate together.
            obstacles[i]=new Bounds(p,d.size);
        }
        if(player!=null) { ResolveOverlap(player); ResolveOverlap(enemy); }
    }
    bool Occupied(Vector3 p,float radius)
    {
        foreach(Bounds b in obstacles) { Bounds e=b; e.Expand(new Vector3(radius*2,0,radius*2)); if(e.Contains(p)) return true; }
        return false;
    }
    void ResolveOverlap(Fighter f)
    {
        // Only eject a fighter when its collision center is actually inside a solid.
        // Preserve height: resetting Y here previously prevented rooftop traversal.
        for(int iteration=0;iteration<4;iteration++)
        foreach(Bounds b in obstacles)
        {
            Bounds e=b; e.Expand(new Vector3(f.Radius*2+.04f,0,f.Radius*2+.04f));
            Vector3 p=f.root.position;
            if(!e.Contains(p)) continue;
            Vector3[] exits={
                new Vector3(e.min.x-.04f,p.y,p.z),new Vector3(e.max.x+.04f,p.y,p.z),
                new Vector3(p.x,p.y,e.min.z-.04f),new Vector3(p.x,p.y,e.max.z+.04f)};
            float best=float.PositiveInfinity; Vector3 chosen=p;
            foreach(Vector3 exit in exits)
                if(!Occupied(exit,f.Radius) && (exit-p).sqrMagnitude<best)
                { best=(exit-p).sqrMagnitude; chosen=exit; }
            f.root.position=chosen;
        }
    }
    float LandingHeight(Vector3 p,float radius,float previousY)
    {
        float ground=1.1f;
        foreach(Bounds b in obstacles)
        {
            Bounds e=b; e.Expand(new Vector3(radius*2,0,radius*2));
            // A rooftop is a landing surface only if approached from above.
            if(p.x>=e.min.x && p.x<=e.max.x && p.z>=e.min.z && p.z<=e.max.z &&
                previousY>=e.max.y-.03f)
                ground=Mathf.Max(ground,e.max.y);
        }
        return ground;
    }
    bool ObstacleOnSegment(Vector3 from,Vector3 to,float radius,out float nearest)
    {
        Vector3 delta=to-from; float length=delta.magnitude; nearest=float.PositiveInfinity;
        if(length<.0001f) { if(Occupied(from,radius)) nearest=0; return nearest==0; }
        Ray ray=new Ray(from,delta/length);
        foreach(Bounds b in obstacles)
        {
            Bounds e=b; e.Expand(new Vector3(radius*2,0,radius*2));
            if(e.Contains(from)) nearest=0;
            else if(e.IntersectRay(ray,out float hit)&&hit<=length) nearest=Mathf.Min(nearest,hit);
        }
        return nearest<=length;
    }
    void MoveWithObstacles(Fighter f,Vector3 displacement)
    {
        ResolveOverlap(f);
        int count=Mathf.Max(1,Mathf.CeilToInt(displacement.magnitude/.25f));
        Vector3 step=displacement/count,p=f.root.position;
        for(int i=0;i<count;i++)
        {
            Vector3 horizontal=new Vector3(step.x,0,step.z);
            if(!Occupied(p+horizontal,f.Radius)) p+=horizontal;
            else
            {
                Vector3 x=p+Vector3.right*step.x;
                if(!Occupied(x,f.Radius)) p=x;
                Vector3 z=p+Vector3.forward*step.z;
                if(!Occupied(z,f.Radius)) p=z;
            }
            float previousY=p.y;
            Vector3 next=p+Vector3.up*step.y;
            if(step.y>0)
            {
                if(!Occupied(next,f.Radius)) p=next;
                else f.jumpVelocity=Mathf.Min(0,f.jumpVelocity);
            }
            else if(step.y<0)
            {
                float floor=LandingHeight(p,f.Radius,previousY);
                if(next.y<=floor)
                {
                    p.y=floor;
                    f.jumpVelocity=0;
                }
                else if(!Occupied(next,f.Radius)) p=next;
            }
            p.y=Mathf.Max(1.1f,p.y);
        }
        f.root.position=p;
    }
}
