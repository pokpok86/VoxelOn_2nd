using UnityEngine;

public sealed partial class VoxArena
{
    void BuildGround()
    {
        // Repeating grid follows the camera in whole-cell increments. No hard arena walls.
        starRoot=new GameObject("Ground tile field").transform;
        Box("Endless ground",starRoot,new Vector3(0,GroundSurfaceY-.125f,0),new Vector3(900,.25f,900),new Color(.16f,.205f,.22f));
        if(arenaMap==ArenaMapKind.OutdoorCanyon)
        {
            // Outdoor floor is warm rock, not the factory grid/runway.
            Box("Canyon sandy stone floor",starRoot,
                new Vector3(0,GroundSurfaceY+.006f,0),
                new Vector3(190,.018f,190),new Color(.33f,.29f,.24f));
            return;
        }
        Color seam=new Color(.20f,.25f,.26f),paint=new Color(.31f,.39f,.39f);
        for(int i=-16;i<=16;i++)
        {
            Box("Tile seam",starRoot,new Vector3(i*10,GroundSurfaceY+.006f,0),new Vector3(.055f,.012f,320),seam);
            Box("Tile seam",starRoot,new Vector3(0,GroundSurfaceY+.006f,i*10),new Vector3(320,.012f,.055f),seam);
        }
        Transform yard=new GameObject("Orbital foundry training yard").transform;
        Box("Runway center",yard,new Vector3(0,GroundSurfaceY+.025f,0),new Vector3(14,.025f,100),new Color(.125f,.17f,.19f));
        for(int i=-9;i<=9;i++)
        {
            Box("Lane dash",yard,new Vector3(0,GroundSurfaceY+.064f,i*5),new Vector3(.16f,.018f,2.1f),paint);
            for(int side=-1;side<=1;side+=2)
                Box("Lane edge",yard,new Vector3(side*6.6f,GroundSurfaceY+.064f,i*5),new Vector3(.13f,.018f,3.3f),paint);
        }
        var pad=Line("Training perimeter paint",new Color(.24f,.43f,.45f),.10f);
        Circle(pad,new Vector3(0,GroundSurfaceY+.065f,0),38);
        var center=Line("Landing pad paint",new Color(.34f,.47f,.47f),.16f);
        Circle(center,new Vector3(0,GroundSurfaceY+.075f,0),9);
        for(int side=-1;side<=1;side+=2)
        for(int i=0;i<5;i++)
        {
            float x=side*(48+i*14),z=45+i%2*16;
            Box("Distant factory",yard,new Vector3(x,2,z),new Vector3(9,6+i%3*3,12),new Color(.13f,.18f,.21f));
            Box("Factory roof",yard,new Vector3(x,5.2f+i%3*1.5f,z),new Vector3(10,.3f,13),new Color(.21f,.27f,.29f));
            // Bounds are registered after the six primary obstacles in BuildWorld.
        }
        // All obstacle collision is analytical, independent from decorative detail meshes.
    }
    void BuildGroundCover(Transform root,Vector3 size,int kind)
    {
        Color metal=new Color(.27f,.34f,.36f),dark=new Color(.10f,.15f,.18f),yellow=new Color(.70f,.48f,.17f);
        float bottom=GroundSurfaceY-root.position.y,top=size.y*.5f;
        // Painted footprint makes the collision silhouette readable from the quarter camera.
        Color marking=new Color(.38f,.42f,.39f);
        for(int side=-1;side<=1;side+=2)
        {
            Box("Cover footprint",root,new Vector3(side*(size.x*.5f+.35f),bottom+.025f,0),new Vector3(.09f,.02f,size.z+.7f),marking);
            Box("Cover footprint",root,new Vector3(0,bottom+.025f,side*(size.z*.5f+.35f)),new Vector3(size.x+.7f,.02f,.09f),marking);
        }
        Box("Cover base",root,new Vector3(0,bottom+.15f,0),new Vector3(size.x+.5f,.3f,size.z+.5f),dark);
        if(kind%3==0)
        {
            Box("Cargo container",root,new Vector3(0,(top+bottom)*.5f,0),new Vector3(size.x,top-bottom,size.z),new Color(.24f,.36f,.39f));
            for(int i=-2;i<=2;i++)
            {
                Box("Container rib",root,new Vector3(i*size.x/5,(top+bottom)*.5f,size.z*.505f),new Vector3(.09f,top-bottom,.08f),metal);
                Box("Container rib",root,new Vector3(i*size.x/5,(top+bottom)*.5f,-size.z*.505f),new Vector3(.09f,top-bottom,.08f),metal);
            }
            Box("Cargo top plate",root,new Vector3(0,top+.05f,0),new Vector3(size.x,.10f,size.z),metal);
            Box("Cargo identification",root,new Vector3(0,top+.12f,0),new Vector3(size.x*.45f,.04f,size.z*.25f),yellow);
            for(int corner=-1;corner<=1;corner+=2)
                Box("Container latch",root,new Vector3(corner*size.x*.35f,(top+bottom)*.5f,-size.z*.51f),new Vector3(.13f,(top-bottom)*.75f,.08f),dark);
        }
        else if(kind%3==1)
        {
            Box("Generator housing",root,new Vector3(0,(top+bottom)*.5f,0),new Vector3(size.x,top-bottom,size.z),dark);
            Box("Generator armor",root,new Vector3(0,top-.5f,0),new Vector3(size.x+.08f,1,size.z+.08f),metal);
            for(int i=-2;i<=2;i++) Box("Cooling slat",root,new Vector3(i*size.x/6,top+.08f,0),new Vector3(.18f,.12f,size.z*.8f),new Color(.42f,.48f,.48f));
            Shape(PrimitiveType.Cube,"Generator status",root,new Vector3(0,.4f,-size.z*.51f),new Vector3(size.x*.5f,.08f,.05f),Cyan,true);
            for(int side=-1;side<=1;side+=2)
                Box("Generator service panel",root,new Vector3(side*size.x*.3f,-.8f,-size.z*.51f),new Vector3(size.x*.23f,.7f,.06f),metal);
        }
        else
        {
            Box("Concrete barricade",root,new Vector3(0,(top+bottom)*.5f,0),new Vector3(size.x,top-bottom,size.z),new Color(.34f,.36f,.34f));
            Box("Barricade cap",root,new Vector3(0,top+.04f,0),new Vector3(size.x+.08f,.12f,size.z+.08f),dark);
            for(int i=-2;i<=2;i++)
            {
                Transform stripe=Box("Hazard marking",root,new Vector3(i*size.x/5,top+.12f,0),new Vector3(.25f,.05f,size.z*.7f),yellow);
                stripe.localRotation=Quaternion.Euler(0,-25,0);
            }
        }
    }
}
