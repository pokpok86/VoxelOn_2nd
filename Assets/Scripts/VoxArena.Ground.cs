using UnityEngine;

public sealed partial class VoxArena
{
    void BuildGround()
    {
        if(arenaMap==ArenaMapKind.OpenPlains){starRoot=SpawnPDPart("PlainsTerrain",null,new Vector3(0,GroundSurfaceY,0),Vector3.one);CachePlainsSurface();return;}
        // Repeating grid follows the camera in whole-cell increments. No hard arena walls.
        starRoot=new GameObject("Ground tile field").transform;
        Box("Endless ground",starRoot,new Vector3(0,GroundSurfaceY-(arenaMap==ArenaMapKind.OutdoorCanyon?.15f:.125f),0),new Vector3(900,.25f,900),new Color(.16f,.205f,.22f));
        if(arenaMap==ArenaMapKind.OutdoorCanyon)
        {
            // Outdoor floor is warm rock, not the factory grid/runway.
            Box("Canyon sandy stone floor",starRoot,
                new Vector3(0,GroundSurfaceY-.02f,0),
                new Vector3(280,.01f,280),new Color(.33f,.29f,.24f));
            BuildOutdoorFloorDetails(starRoot);
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
        BuildVoxelCover(root,size,kind);
    }
}
