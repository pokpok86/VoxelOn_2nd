using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    // The fighter root is centered near the chest while grounded. Keep the
    // visible floor and obstacle collision bottoms aligned to the same plane.
    const float GroundSurfaceY=-.91f;
    const float FighterSupportOffset=2.01f;
    const float FighterGroundY=GroundSurfaceY+FighterSupportOffset;
    sealed class Debris { public Transform visual; public Vector3 origin,size; public float phase; }
    readonly List<Bounds> obstacles=new List<Bounds>();
    readonly List<Debris> debris=new List<Debris>();
    Transform starRoot;
    // Terrain blocks share the obstacle model so navigation, shots and landing agree.
    readonly List<Bounds> terrainSteps=new List<Bounds>();
    float spaceClock;

    void BuildWorld()
    {
        RenderSettings.ambientLight=new Color(.55f,.59f,.64f); RenderSettings.fog=false;
        QualitySettings.antiAliasing=4; QualitySettings.shadows=ShadowQuality.All; QualitySettings.shadowDistance=140;
        Light light=new GameObject("Distant sun").AddComponent<Light>();
        light.type=LightType.Directional; light.intensity=1.05f; light.shadows=LightShadows.Soft; light.shadowStrength=.65f;
        light.transform.rotation=Quaternion.Euler(40,-30,0);
        // Warm key and cool rim give the voxel armor readable depth without
        // adding dynamic shadows or touching physics / gameplay systems.
        light.color=new Color(1f,.94f,.85f);
        Light rim=new GameObject("Cool mech rim light").AddComponent<Light>();
        rim.type=LightType.Directional;
        rim.color=new Color(.42f,.69f,1f);
        rim.intensity=.34f;
        rim.shadows=LightShadows.None;
        rim.transform.rotation=Quaternion.Euler(28,145,0);
        RenderSettings.ambientLight=new Color(.39f,.45f,.54f);
        view=new GameObject("Quarter view camera").AddComponent<Camera>(); view.tag="MainCamera";
        view.transform.position=new Vector3(46,58,-46); view.transform.LookAt(Vector3.zero);
        view.cullingMask=~(1<<8); view.orthographic=true; view.orthographicSize=24; view.farClipPlane=600;
        view.backgroundColor=new Color(.009f,.014f,.04f); view.clearFlags=CameraClearFlags.SolidColor;
        view.gameObject.AddComponent<AudioListener>();
        BuildGround();
        BuildVisualEnvironment();
        BuildSalvage();
        AddDebris(new Vector3(-16,1.1f,3),new Vector3(5,4.2f,5),0);
        AddDebris(new Vector3(20,1.1f,38),new Vector3(6,3.4f,5),1.4f);
        AddDebris(new Vector3(-5,1.1f,-19),new Vector3(4,1.8f,6),2.8f);
        AddDebris(new Vector3(6,1.1f,23),new Vector3(6,4.2f,4),4.1f);
        AddDebris(new Vector3(-48,1.1f,-35),new Vector3(5,3.4f,5),5.4f);
        AddDebris(new Vector3(55,1.1f,-42),new Vector3(4,1.8f,6),6.7f);
        AddDebris(new Vector3(-58,1.1f,30),new Vector3(7,2.6f,4),1.1f);
        AddDebris(new Vector3(64,1.1f,40),new Vector3(5,3.6f,7),2.2f);
        AddDebris(new Vector3(-38,1.1f,60),new Vector3(8,2.2f,5),3.3f);
        AddDebris(new Vector3(42,1.1f,-62),new Vector3(7,3.0f,5),4.4f);
        AddDebris(new Vector3(-70,1.1f,-10),new Vector3(5,4.0f,7),5.5f);
        AddDebris(new Vector3(78,1.1f,8),new Vector3(8,2.4f,5),6.6f);
        // Spread cover across the open ground so roaming beyond the starting pad
        // leads to new routes instead of an empty, unbounded-looking plain.
        AddDebris(new Vector3(-136,1.1f,110),new Vector3(8,3.4f,6),.5f);
        AddDebris(new Vector3(142,1.1f,92),new Vector3(6,2.2f,9),1.6f);
        AddDebris(new Vector3(158,1.1f,-54),new Vector3(7,4.0f,5),2.7f);
        AddDebris(new Vector3(-162,1.1f,-104),new Vector3(9,2.6f,7),3.8f);
        AddDebris(new Vector3(12,1.1f,154),new Vector3(7,3.6f,6),4.9f);
        AddDebris(new Vector3(-34,1.1f,-158),new Vector3(6,2.4f,8),6.0f);
        AddDebris(new Vector3(104,1.1f,166),new Vector3(8,3.0f,5),.9f);
        AddDebris(new Vector3(-108,1.1f,-166),new Vector3(7,4.2f,6),2.0f);
        AddDebris(new Vector3(184,1.1f,18),new Vector3(6,2.8f,8),3.1f);
        AddDebris(new Vector3(-184,1.1f,42),new Vector3(8,2.2f,6),4.2f);
        AddDebris(new Vector3(160,1.1f,160),new Vector3(9,3.4f,7),5.3f);
        AddDebris(new Vector3(-166,1.1f,-164),new Vector3(7,2.6f,9),6.4f);
        BuildElevationCourse();
        for(int side=-1;side<=1;side+=2) for(int i=0;i<5;i++)
            obstacles.Add(new Bounds(new Vector3(side*(48+i*14),2,45+i%2*16),new Vector3(10,9+i%3*3,13)));
        player=MakeFighter("Pilot frame",Cyan); enemy=MakeFighter("Rival frame",Orange);
        aimMarker=new GameObject("Aim reticle").transform;
        Shape(PrimitiveType.Cube,"Aim cross",aimMarker,Vector3.zero,new Vector3(.8f,.06f,.07f),Cyan,true);
        Shape(PrimitiveType.Cube,"Aim cross",aimMarker,Vector3.zero,new Vector3(.07f,.06f,.8f),Cyan,true);
    }
    void AddTerrainBlock(string name,Vector3 center,Vector3 size,Color color)
    {
        Box(name,null,center,size,color);
        Bounds bounds=new Bounds(center,size);
        obstacles.Add(bounds);
        terrainSteps.Add(bounds);
    }
    void BuildElevationCourse()
    {
        // Low, closely spaced terraces read as a gentle stepped ramp.
        // Taller isolated ledges demand a jump rather than automatic stepping.
        Color stone=new Color(.21f,.31f,.37f),edge=new Color(.35f,.45f,.50f);
        for(int i=0;i<7;i++)
        {
            float height=.36f*(i+1);
            AddTerrainBlock("West access ramp "+i,new Vector3(-24+i*2.5f,GroundSurfaceY+height*.5f,20),
                new Vector3(2.6f,height,8),stone);
        }
        // Match the final stair top so the deck does not form a taller hidden
        // collision wall across the last step.
        const float deckTop=GroundSurfaceY+.36f*7;
        float deckHeight=deckTop-GroundSurfaceY;
        AddTerrainBlock("West raised deck",new Vector3(-3,GroundSurfaceY+deckHeight*.5f,20),new Vector3(13,deckHeight,11),edge);
        for(int i=0;i<4;i++)
        {
            float height=1.7f*(i+1);
            AddTerrainBlock("East jump ledge "+i,new Vector3(20+i*5f,GroundSurfaceY+height*.5f,-14),
                new Vector3(4.5f,height,7),stone);
        }
    }
    Fighter MakeFighter(string name,Color accent)
    {
        Fighter f=new Fighter(); f.root=new GameObject(name).transform;
        f.shadow=Shape(PrimitiveType.Cylinder,"Contact shadow",null,new Vector3(0,GroundSurfaceY+.06f,0),new Vector3(2.7f,.012f,2.1f),new Color(.075f,.105f,.12f));
        f.jumpFlame=Shape(PrimitiveType.Capsule,"Jump exhaust flare",f.root,new Vector3(0,-.1f,-1.4f),
            new Vector3(.48f,.65f,.48f),new Color(.66f,.94f,1),true);
        f.jumpFlame.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        f.jumpFlame.GetComponent<Renderer>().receiveShadows=false;
        f.jumpFlame.gameObject.SetActive(false);
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
        obstacles.Add(DebrisBounds(origin,size));
    }
    Bounds DebrisBounds(Vector3 center,Vector3 size)
    {
        float top=center.y+size.y*.5f;
        float height=Mathf.Max(.01f,top-GroundSurfaceY);
        return new Bounds(new Vector3(center.x,(top+GroundSurfaceY)*.5f,center.z),
            new Vector3(size.x,height,size.z));
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
            obstacles[i]=DebrisBounds(p,d.size);
        }
        if(player!=null) { ResolveOverlap(player); ResolveOverlap(enemy); }
    }
    bool Occupied(Vector3 p,float radius)
    {
        foreach(Bounds b in obstacles) if(InsideObstacle(b,p,radius)) return true;
        return false;
    }
    bool OverlapsFootprint(Bounds obstacle,Vector3 point,float radius)
    {
        float dx=Mathf.Max(Mathf.Abs(point.x-obstacle.center.x)-obstacle.extents.x,0);
        float dz=Mathf.Max(Mathf.Abs(point.z-obstacle.center.z)-obstacle.extents.z,0);
        return dx*dx+dz*dz<=radius*radius;
    }
    bool InsideObstacle(Bounds obstacle,Vector3 point,float radius)
    {
        // Round the footprint corners to the fighter's circular radius; square
        // expansion made narrow gaps and corners unnecessarily sticky.
        return OverlapsFootprint(obstacle,point,radius) &&
            point.y>=obstacle.min.y && point.y<obstacle.max.y-.02f;
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
            if(!InsideObstacle(b,p,f.Radius+.02f)) continue;
            // Course blocks are handled by movement/landing, never by spawn
            // overlap ejection. Ejecting near a step top pushed a climber away
            // before TryStepUp could advance onto the next terrace.
            if(terrainSteps.Contains(b)) continue;
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
        float ground=FighterGroundY;
        foreach(Bounds b in obstacles)
        {
            // A rooftop is a landing surface only if approached from above.
            if(OverlapsFootprint(b,p,radius) &&
                previousY>=b.max.y-.03f)
                ground=Mathf.Max(ground,b.max.y+FighterSupportOffset);
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
            if(InsideObstacle(b,from,radius)) nearest=0;
            else
            {
                // Rays running over a roof should not treat the support surface as a wall.
                e.max=new Vector3(e.max.x,b.max.y-.02f,e.max.z);
                if(e.IntersectRay(ray,out float hit)&&hit<=length) nearest=Mathf.Min(nearest,hit);
            }
        }
        return nearest<=length;
    }
    bool TryStepUp(Fighter f,Vector3 p,Vector3 horizontal,out Vector3 stepped)
    {
        stepped=p;
        if(horizontal.sqrMagnitude<.00001f || f.jumpVelocity>1f) return false;
        float maxStep=Mathf.Max(0,f.spec.stepHeight);
        foreach(Bounds b in terrainSteps)
        {
            Vector3 ahead=p+horizontal;
            if(!OverlapsFootprint(b,ahead,f.Radius)) continue;
            float landingY=b.max.y+FighterSupportOffset;
            float rise=landingY-p.y;
            if(rise<=.01f || rise>maxStep) continue;
            Vector3 candidate=new Vector3(ahead.x,landingY+.01f,ahead.z);
            bool blocked=Occupied(candidate,f.Radius);
            if(!blocked) { stepped=candidate; return true; }
        }
        return false;
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
            else if(TryStepUp(f,p,horizontal,out Vector3 stepped)) p=stepped;
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
                float jumpTop=f.jumpOriginY+jumpMaxHeight;
                if(next.y>jumpTop) { next.y=jumpTop; p=next; f.jumpVelocity=0; }
                else if(!Occupied(next,f.Radius)) p=next;
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
            p.y=Mathf.Max(FighterGroundY,p.y);
        }
        f.root.position=p;
    }
}
