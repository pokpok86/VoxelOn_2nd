using UnityEngine;

// All geometry below is local to a detachable module. Gameplay stats stay in CraftSpec
// until the assembly system is introduced; visual changes never silently change balance.
public sealed partial class VoxArena
{
    Bounds wispHeadBounds;
    bool wispHeadBoundsReady;
    void CacheWispHeadBounds(Fighter fighter)
    {
        if(wispHeadBoundsReady)return;
        foreach(SkinnedMeshRenderer skin in fighter.body.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            Vector3[] points=skin.sharedMesh.vertices;BoneWeight[] weights=skin.sharedMesh.boneWeights;Matrix4x4[] bind=skin.sharedMesh.bindposes;
            for(int i=0;i<points.Length;i++)
            {
                int index=weights[i].boneIndex0;Transform bone=skin.bones[index];
                if(bone!=fighter.headPart&&!bone.IsChildOf(fighter.headPart))continue;
                Vector3 local=fighter.headPart.InverseTransformPoint((bone.localToWorldMatrix*bind[index]).MultiplyPoint3x4(points[i]));
                if(!wispHeadBoundsReady){wispHeadBounds=new Bounds(local,Vector3.zero);wispHeadBoundsReady=true;}
                else wispHeadBounds.Encapsulate(local);
            }
        }
    }
    bool ProjectedWispHeadCoversReticle()
    {
        if(!wispHeadBoundsReady||player.spec.name!="WISP")return false;
        Vector2 min=Vector2.one*float.PositiveInfinity,max=Vector2.one*float.NegativeInfinity;
        for(int i=0;i<8;i++)
        {
            Vector3 corner=wispHeadBounds.center+Vector3.Scale(wispHeadBounds.extents,
                new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
            Vector3 point=view.WorldToViewportPoint(player.headPart.TransformPoint(corner));
            if(point.z<=0)continue;
            min=Vector2.Min(min,new Vector2(point.x,point.y));max=Vector2.Max(max,new Vector2(point.x,point.y));
        }
        const float marginX=22f/1280,marginY=22f/720;
        return min.x<=.5f+marginX&&max.x>=.5f-marginX&&min.y<=.5f+marginY&&max.y>=.5f-marginY;
    }
    Transform Module(string name,Transform parent,Vector3 position)
    {
        Transform t=new GameObject(name).transform; t.SetParent(parent,false); t.localPosition=position; return t;
    }
    Transform Plate(string name,Transform parent,Vector3 p,Vector3 size,Color color,float tilt=0)
    {
        Transform t=Box(name,parent,p,size,color); t.localRotation=Quaternion.Euler(tilt,0,0); return t;
    }
    // Eight-sided silhouette with a recessed top and bottom edge.
    // Mesh is built in normalized local space; Box handles existing materials.
    // Visual mesh only: collision and gameplay use the original arena logic.
    Transform BeveledPlate(string name,Transform parent,Vector3 p,Vector3 size,
        Color color,float tilt=0,float bevel=.16f,float taper=.88f)
    {
        Transform t=Plate(name,parent,p,size,color,tilt);
        MeshFilter filter=t.GetComponent<MeshFilter>();
        if(filter==null) return t;
        Mesh mesh=new Mesh();
        float[] heights={-.5f,-.5f+bevel,.5f-bevel,.5f};
        float[] widths={taper,1f,1f,taper};
        Vector3[] vertices=new Vector3[16];
        for(int ring=0;ring<4;ring++)
        {
            float half=widths[ring]*.5f;
            vertices[ring*4+0]=new Vector3(-half,heights[ring],-half);
            vertices[ring*4+1]=new Vector3( half,heights[ring],-half);
            vertices[ring*4+2]=new Vector3( half,heights[ring], half);
            vertices[ring*4+3]=new Vector3(-half,heights[ring], half);
        }
        int[] triangles=new int[84];
        int n=0;
        // Winding is outward-facing when viewed from outside the plate.
        for(int ring=0;ring<3;ring++)
            for(int side=0;side<4;side++)
            {
                int a=ring*4+side,b=ring*4+(side+1)%4;
                int c=(ring+1)*4+side,d=(ring+1)*4+(side+1)%4;
                triangles[n++]=a;triangles[n++]=c;triangles[n++]=b;
                triangles[n++]=b;triangles[n++]=c;triangles[n++]=d;
            }
        triangles[n++]=0;triangles[n++]=1;triangles[n++]=2;
        triangles[n++]=0;triangles[n++]=2;triangles[n++]=3;
        triangles[n++]=12;triangles[n++]=14;triangles[n++]=13;
        triangles[n++]=12;triangles[n++]=15;triangles[n++]=14;
        mesh.vertices=vertices;
        mesh.triangles=triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        filter.sharedMesh=mesh;
        return t;
    }
    // First-pass procedural articulation: obvious pose differences are intentional.
    // Neutral-pose geometry stays unchanged; only the child pivots rotate.
    void AnimateLegJoints(Fighter f,float stride,float gait)
    {
        bool airborne=f.jumpHeight>.08f || f.jumpVelocity>1f;
        bool landing=f.knockdownTime>0f;
        bool heavy=f.spec.mobility==MobilityKind.Tank;
        bool runner=f.spec.mobility==MobilityKind.Runner;
        float phase=roundTime*(heavy?7f:runner?16f:11f);
        if(f.spec.mobility==MobilityKind.Quadruped && f.quadrupedLegs!=null)
        {
            bool spider=f.spec.name=="TARANTULA";
            for(int i=0;i<f.quadrupedLegs.Length;i++)
            {
                float cycle=Mathf.Sin(phase+((i==0||i==3)?0f:Mathf.PI));
                float lift=Mathf.Max(0f,cycle)*stride;
                float swing=(spider?24f:32f)*cycle*stride;
                float kneeBend=airborne?(spider?22f:35f):Mathf.Max(0f,cycle)*(spider?28f:20f);
                if(f.quadrupedLegs[i]!=null) f.quadrupedLegs[i].localRotation=Quaternion.Euler(swing,0,0);
                if(f.quadrupedKnees[i]!=null) f.quadrupedKnees[i].localRotation=Quaternion.Euler(kneeBend+lift*18f,0,0);
                if(f.quadrupedAnkles[i]!=null) f.quadrupedAnkles[i].localRotation=Quaternion.Euler(airborne?-12f:-lift*16f,0,0);
            }
            return;
        }
        for(int side=-1;side<=1;side+=2)
        {
            Transform hip=side<0?f.leftLeg:f.rightLeg;
            if(hip==null) continue;
            Transform knee=hip.Find(side<0?"Left knee pivot":"Right knee pivot");
            if(knee==null && f.spec.name=="WISP") knee=hip.Find(side<0?"Left_knee":"Right_knee");
            if(knee==null) continue;
            Transform ankle=knee.Find(side<0?"Left ankle pivot":"Right ankle pivot");
            if(ankle==null && f.spec.name=="WISP") ankle=knee.Find(side<0?"Left_ankle":"Right_ankle");
            float cycle=Mathf.Sin(phase+(side<0?0f:Mathf.PI));
            float lift=Mathf.Max(0f,cycle)*stride;
            float bend=airborne?(runner?37f:heavy?18f:29f):
                landing?52f:(heavy?16f:runner?43f:30f)*lift;
            float charge=f.spec.chargedReleaseJump && f.jumpHeld && f.jumpHeight<=.02f ? Mathf.Clamp01(f.jumpHoldElapsed/Mathf.Max(.01f,f.spec.jumpChargeSeconds)):0f;
            if(charge>0f) bend=Mathf.Lerp(bend,65f,charge);
            float anklePitch=charge>0f?-bend*.5f:airborne?-18f:landing?-24f:-bend*.58f+cycle*stride*6f;
            knee.localRotation=Quaternion.Euler(bend,0,0);
            if(ankle!=null) ankle.localRotation=Quaternion.Euler(anklePitch,0,0);
        }
    }
    void BuildFourLegs(Fighter f,Color armor,Color dark,Color edge)
    {
        f.pelvisPart.gameObject.SetActive(false);
        if(f.leftLeg!=null) f.leftLeg.gameObject.SetActive(false);
        if(f.rightLeg!=null) f.rightLeg.gameObject.SetActive(false);
        bool spider=f.spec.name=="TARANTULA";
        f.quadrupedLegs=new Transform[4]; f.quadrupedKnees=new Transform[4]; f.quadrupedAnkles=new Transform[4];
        // A wide, low chassis makes the four-legged silhouette read before its small details.
        Plate(spider?"Tarantula abdomen armor":"Hound ribcage armor",f.legsPart,
            new Vector3(0,spider?-.40f:-.34f,spider?-.12f:-.06f),
            spider?new Vector3(1.42f,.43f,1.50f):new Vector3(1.08f,.47f,1.72f),armor);
        if(spider)
        {
            BeveledPlate("Tarantula front thorax",f.legsPart,new Vector3(0,-.20f,.48f),
                new Vector3(.93f,.34f,.72f),edge,0,.14f,.72f);
            BeveledPlate("Tarantula rear abdomen shell",f.legsPart,new Vector3(0,-.31f,-.48f),
                new Vector3(1.12f,.37f,.72f),armor,0,.16f,.82f);
            Plate("Tarantula dorsal seam",f.legsPart,new Vector3(0,-.095f,-.38f),
                new Vector3(.055f,.025f,.60f),edge);
            for(int side=-1;side<=1;side+=2)
                for(int spot=0;spot<3;spot++)
                    Shape(PrimitiveType.Sphere,"Tarantula dorsal sensor",f.legsPart,
                        new Vector3(side*(.30f+spot*.18f),-.08f,-.53f+spot*.16f),
                        Vector3.one*.10f,edge,true);
        }
        for(int i=0;i<4;i++)
        {
            int side=(i%2==0)?-1:1; bool front=i<2;
            float x=side*(front?(spider?.62f:.46f):(spider?.82f:.48f)), z=front?.57f:-.68f;
            Transform hip=Module((spider?"Spider":"Hound")+" leg root "+i,
                f.legsPart,new Vector3(x,spider?-.51f:-.48f,z));
            f.quadrupedLegs[i]=hip;
            float upperLength=spider?.62f:.52f;
            Shape(PrimitiveType.Sphere,"Hip rotary joint",hip,Vector3.zero,
                Vector3.one*(spider?.23f:.25f),dark);
            Transform upper=BeveledPlate("Upper leg armor",hip,
                new Vector3(side*(spider?.30f:.12f),-.20f,front?.10f:-.10f),
                new Vector3(spider?.28f:.30f,upperLength,spider?.34f:.35f),edge,0,.13f,.76f);
            upper.localRotation=Quaternion.Euler(front?10f:-10f,0,side*(spider?34f:12f));
            Transform knee=Module("Knee articulation",hip,
                new Vector3(side*(spider?.51f:.16f),-.40f,front?.16f:-.16f));
            f.quadrupedKnees[i]=knee;
            Shape(PrimitiveType.Sphere,"Knee joint",knee,Vector3.zero,
                Vector3.one*(spider?.20f:.19f),dark);
            float lower=spider?.72f:.61f;
            Transform lowerPlate=BeveledPlate("Lower leg armor",knee,
                new Vector3(side*(spider?.20f:.04f),-.28f,front?.08f:-.08f),
                new Vector3(spider?.23f:.25f,lower,spider?.27f:.28f),armor,0,.12f,.74f);
            lowerPlate.localRotation=Quaternion.Euler(front?-8f:8f,0,side*(spider?20f:4f));
            Transform ankle=Module("Ankle articulation",knee,
                new Vector3(side*(spider?.36f:.08f),-.55f,front?.09f:-.09f));
            f.quadrupedAnkles[i]=ankle;
            Plate(spider?"Tarsal foot":"Padded paw",ankle,new Vector3(0,-.035f,.08f),
                spider?new Vector3(.28f,.14f,.38f):new Vector3(.38f,.20f,.45f),dark);
            if(spider)
                for(int toe=0;toe<2;toe++) Plate("Tarantula claw",ankle,new Vector3((toe==0?-.07f:.07f),-.10f,.29f),new Vector3(.07f,.08f,.20f),armor,-12);
            else Plate("Hound toe cap",ankle,new Vector3(0,.055f,.24f),new Vector3(.27f,.09f,.20f),armor);
        }
        if(!spider)
        {
            foreach(string name in new[]{"Helmet","Helmet crown","Face mask","Sensor housing","Visor","Sensor fin"})
                for(int i=0;i<f.headPart.childCount;i++)
                    if(f.headPart.GetChild(i).name==name) f.headPart.GetChild(i).gameObject.SetActive(false);
            BeveledPlate("Hound compact skull",f.headPart,Vector3.zero,
                new Vector3(.63f,.43f,.64f),armor,0,.15f,.80f);
            BeveledPlate("Hound cheek armor",f.headPart,new Vector3(0,-.13f,.12f),
                new Vector3(.66f,.25f,.49f),edge,0,.10f,.83f);
            BeveledPlate("Hound muzzle",f.headPart,new Vector3(0,-.10f,.51f),
                new Vector3(.43f,.25f,.83f),armor,0,.11f,.65f);
            Plate("Hound nose bridge",f.headPart,new Vector3(0,-.025f,.84f),
                new Vector3(.24f,.12f,.24f),edge,-8);
            Plate("Hound nose",f.headPart,new Vector3(0,-.065f,.98f),
                new Vector3(.22f,.15f,.11f),dark);
            Plate("Hound jaw",f.headPart,new Vector3(0,-.25f,.54f),
                new Vector3(.37f,.09f,.62f),dark,-4);
            Plate("Hound jaw guard",f.headPart,new Vector3(0,-.20f,.76f),
                new Vector3(.34f,.075f,.18f),armor);
            for(int side=-1;side<=1;side+=2)
                Shape(PrimitiveType.Cube,"Hound muzzle vent",f.headPart,
                    new Vector3(side*.09f,-.08f,.84f),new Vector3(.035f,.05f,.035f),edge,true);
            for(int side=-1;side<=1;side+=2)
            {
                Shape(PrimitiveType.Sphere,"Hound optical eye",f.headPart,new Vector3(side*.21f,.045f,.35f),
                    new Vector3(.105f,.10f,.07f),new Color(1,.72f,.25f),true);
                BeveledPlate("Hound brow",f.headPart,new Vector3(side*.20f,.14f,.35f),
                    new Vector3(.24f,.075f,.15f),edge,side*8,.035f,.72f);
                Transform ear=BeveledPlate("Hound upright ear",f.headPart,
                    new Vector3(side*.24f,.32f,-.07f),new Vector3(.18f,.42f,.20f),armor,
                    side*-10,.12f,.66f);
                ear.localRotation=Quaternion.Euler(-12,0,side*-12);
                Shape(PrimitiveType.Sphere,"Hound shoulder bearing",f.legsPart,
                    new Vector3(side*.43f,-.37f,.46f),Vector3.one*.24f,edge);
                Shape(PrimitiveType.Sphere,"Hound hip bearing",f.legsPart,
                    new Vector3(side*.43f,-.38f,-.53f),Vector3.one*.23f,edge);
            }
            BeveledPlate("Hound neck bridge",f.legsPart,new Vector3(0,-.27f,.59f),
                new Vector3(.52f,.32f,.60f),edge,-12,.11f,.78f);
            BeveledPlate("Hound tail root",f.legsPart,new Vector3(0,-.17f,-.86f),
                new Vector3(.22f,.22f,.43f),edge,26,.08f,.78f);
            BeveledPlate("Hound tail mid",f.legsPart,new Vector3(0,.01f,-1.12f),
                new Vector3(.18f,.19f,.34f),armor,35,.07f,.72f);
            BeveledPlate("Hound tail tip",f.legsPart,new Vector3(0,.18f,-1.32f),
                new Vector3(.15f,.18f,.25f),edge,42,.06f,.68f);
        }
        else
        {
            foreach(string name in new[]{"Helmet","Helmet crown","Face mask","Sensor housing","Visor","Sensor fin"})
                for(int i=0;i<f.headPart.childCount;i++)
                    if(f.headPart.GetChild(i).name==name) f.headPart.GetChild(i).gameObject.SetActive(false);
            BeveledPlate("Tarantula sensor carapace",f.headPart,Vector3.zero,new Vector3(.58f,.28f,.48f),armor,0,.10f,.78f);
            for(int side=-1;side<=1;side+=2)
                for(int row=0;row<2;row++)
                    Shape(PrimitiveType.Sphere,"Tarantula optic array",f.headPart,
                        new Vector3(side*(.16f+row*.075f),-.02f-row*.075f,.245f),
                        Vector3.one*(row==0?.085f:.065f),new Color(1,.18f,.38f),true);
        }
        // These archetypes are chassis-first quadrupeds; the inherited biped
        // chest and shoulder rig obscured the four-leg silhouette. Keep their
        // weapon on a dedicated center mount, not visible humanoid arms.
        f.corePart.gameObject.SetActive(false);
        f.armsPart.gameObject.SetActive(false);
    }
    void ApplyImportedWisp(Fighter f)
    {
        if(wispModelSource==null || f.spec.name!="WISP") return;

        Transform legacyCore=f.corePart,legacyHead=f.headPart,legacyArms=f.armsPart,legacyLegs=f.legsPart;
        GameObject imported=Instantiate(wispModelSource,f.body);
        bool articulated=IsArticulatedWisp;
        imported.name=articulated?"WISP artist model v003":"WISP artist model v001";
        Transform root=imported.transform.Find(articulated?"WISP_Rig_v003/Root":"Root");
        Transform arms=root!=null?root.Find("Arms"):null;
        Transform core=root!=null?root.Find("Core"):null;
        Transform head=root!=null?root.Find("Head"):null;
        Transform legs=root!=null?root.Find("Legs"):null;
        Transform rightShoulder=arms!=null?arms.Find("Right_shoulder"):null;
        Transform rightHand=rightShoulder!=null?rightShoulder.Find("Right_elbow/Right_hand"):null;
        Transform leftShoulder=arms!=null?arms.Find("Left_shoulder"):null;
        Transform leftHand=leftShoulder!=null?leftShoulder.Find("Left_elbow/Left_hand"):null;
        Transform leftThigh=legs!=null?legs.Find("Left_thigh"):null;
        Transform rightThigh=legs!=null?legs.Find("Right_thigh"):null;
        if(root==null || arms==null || core==null || head==null || legs==null || rightShoulder==null || rightHand==null || leftShoulder==null || leftHand==null || leftThigh==null || rightThigh==null)
        {
            Debug.LogError(wispModelSource.name+" is missing a required module or joint. Keeping the procedural WISP model.");
            Destroy(imported);
            return;
        }

        // The FBX origin is at the soles, while the gameplay root is chest-high.
        // Offset all four modules so the imported sole plane matches the arena floor.
        // Match the arena's established robot scale without changing collision or stats.
        const float visualScale=1.75f;
        float soleAdjustedSupport=FighterSupportOffset-.014f*visualScale;
        if(articulated)
        {
            imported.transform.localScale=Vector3.one*visualScale;
            imported.transform.localPosition=Vector3.down*(FighterSupportOffset/Mathf.Max(.01f,f.spec.size));
            foreach(SkinnedMeshRenderer skin in imported.GetComponentsInChildren<SkinnedMeshRenderer>(true)) skin.updateWhenOffscreen=true;
        }
        else foreach(Transform part in new[]{arms,core,head,legs})
        {
            part.SetParent(f.body,false);
            part.localPosition*=visualScale;
            part.localScale*=visualScale;
            part.localPosition+=Vector3.down*(soleAdjustedSupport/Mathf.Max(.01f,f.spec.size));
        }
        f.armsPart=arms; f.corePart=core; f.headPart=head; f.legsPart=legs;
        f.pelvisPart=core.Find("Pelvis"); f.backPart=core.Find("BACKPACK_SOCKET");
        f.leftArm=leftShoulder; f.rightArm=rightShoulder;
        f.leftLeg=leftThigh; f.rightLeg=rightThigh;

        // Preserve weapon and boost effects while removing only duplicate primitive armor.
        Transform[] obsolete={legacyCore,legacyHead,legacyArms,legacyLegs};
        foreach(Transform group in obsolete)
            if(group!=null)
                foreach(Renderer renderer in group.GetComponentsInChildren<Renderer>(true))
                {
                    Transform t=renderer.transform;
                    if((f.gun!=null && t.IsChildOf(f.gun)) || (f.fist!=null && t.IsChildOf(f.fist)) ||
                       (f.saber!=null && t.IsChildOf(f.saber)) || (f.thruster!=null && t.IsChildOf(f.thruster)) ||
                       (f.jumpFlame!=null && t.IsChildOf(f.jumpFlame)) || (f.shield!=null && t.IsChildOf(f.shield))) continue;
                    renderer.enabled=false;
                }

        // Existing movement animation drives the imported shoulder and thigh pivots.
        // WISP uses a left-hand gun and a right-hand shield.
        f.weaponPart.SetParent(articulated?leftHand:leftShoulder,false);
        f.weaponPart.localPosition=articulated?new Vector3(0,-.015f,.1f):new Vector3(-.07f,-.81f,.115f);
        f.weaponPart.localRotation=Quaternion.identity;
        f.weaponPart.localScale=Vector3.one;
        // Keep the existing saber rush functional, now rooted at the imported left hand.
        Transform proceduralSaber=f.saber;
        proceduralSaber.gameObject.SetActive(false);
        f.shield.SetParent(f.body,false);

        Transform[] importedModules=articulated?new[]{imported.transform}:new[]{arms,core,head,legs};
        if(wispRuntimeMaterial==null)
        {
            Material source=null;
            foreach(Transform module in importedModules)
                foreach(Renderer renderer in module.GetComponentsInChildren<Renderer>(true))
                {
                    if(renderer.transform.IsChildOf(f.weaponPart)) continue;
                    foreach(Material material in renderer.sharedMaterials)
                        if(material!=null) { source=material; break; }
                    if(source!=null) break;
                }
            if(source!=null)
            {
                // Clone the artist-authored FBX material so the player build keeps
                // its imported shader properties and atlas binding intact.
                wispRuntimeMaterial=new Material(source);
                wispRuntimeMaterial.name="WISP_Atlas_Runtime";
                if(source.HasProperty("_Color") && wispRuntimeMaterial.HasProperty("_Color"))
                    wispRuntimeMaterial.SetColor("_Color",source.GetColor("_Color"));
                if(wispBaseColorTexture!=null)
                {
                    if(wispRuntimeMaterial.HasProperty("_BaseMap")) wispRuntimeMaterial.SetTexture("_BaseMap",wispBaseColorTexture);
                    if(wispRuntimeMaterial.HasProperty("_MainTex")) wispRuntimeMaterial.SetTexture("_MainTex",wispBaseColorTexture);
                }
                if(wispEmissionTexture!=null && wispRuntimeMaterial.HasProperty("_EmissionMap"))
                {
                    wispRuntimeMaterial.SetTexture("_EmissionMap",wispEmissionTexture);
                    wispRuntimeMaterial.EnableKeyword("_EMISSION");
                    if(wispRuntimeMaterial.HasProperty("_EmissionColor")) wispRuntimeMaterial.SetColor("_EmissionColor",Color.white*((wispModelSource.name.Contains("v004")||wispModelSource.name.Contains("v005"))?.9f:2.5f));
                }
                if(wispRuntimeMaterial.HasProperty("_Metallic")) wispRuntimeMaterial.SetFloat("_Metallic",.05f);
                if(wispRuntimeMaterial.HasProperty("_Glossiness")) wispRuntimeMaterial.SetFloat("_Glossiness",.14f);
            }
        }
        if(wispRuntimeMaterial!=null)
        {
            foreach(Transform module in importedModules)
            foreach(Renderer renderer in module.GetComponentsInChildren<Renderer>(true))
            {
                if(renderer.transform.IsChildOf(f.weaponPart)) continue;
                Material[] materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++) materials[i]=wispRuntimeMaterial;
                renderer.sharedMaterials=materials;
            }
        }
        Transform swordSocket=rightHand.Find("SWORD_SOCKET");
        if(wispSwordSource!=null && swordSocket!=null)
        {
            GameObject sword=Instantiate(wispSwordSource,swordSocket,false);
            sword.name="WISP sword v001";
            sword.transform.localPosition=Vector3.zero;
            sword.transform.localRotation=Quaternion.identity;
            sword.transform.localScale=Vector3.one;
            foreach(Renderer renderer in sword.GetComponentsInChildren<Renderer>(true))
                if(wispRuntimeMaterial!=null) renderer.sharedMaterial=wispRuntimeMaterial;
            f.saber=sword.transform;
            f.saber.gameObject.SetActive(false);
        }
        f.guardElbow=rightShoulder.Find("Right_elbow");
        Transform originalShieldSocket=leftShoulder.Find("Left_elbow/SHIELD_SOCKET");
        Vector3 shieldOffset=originalShieldSocket!=null?originalShieldSocket.localPosition:new Vector3(-.18f,-.03f,.08f);
        shieldOffset.x=-shieldOffset.x;
        Transform shieldSocket=Module("RIGHT_SHIELD_SOCKET",f.guardElbow,shieldOffset);
        if(wispShieldSource!=null && shieldSocket!=null)
        {
            GameObject shieldMesh=Instantiate(wispShieldSource,shieldSocket,false);
            shieldMesh.name="WISP right shield (guard pose)";
            shieldMesh.transform.localPosition=Vector3.zero;
            shieldMesh.transform.localRotation=Quaternion.identity;
            shieldMesh.transform.localScale=Vector3.one;
            f.guardShield=shieldMesh.transform;
            foreach(Renderer renderer in shieldMesh.GetComponentsInChildren<Renderer>(true))
                if(wispRuntimeMaterial!=null) renderer.sharedMaterial=wispRuntimeMaterial;
        }
        if(wispSwordSource!=null && swordSocket==null)
            Debug.LogWarning("WISP sword FBX was supplied, but the documented SWORD_SOCKET was not found.");
        if(wispShieldSource!=null && shieldSocket==null)
            Debug.LogWarning("WISP shield FBX was supplied, but the documented SHIELD_SOCKET was not found.");
        CacheWispHeadBounds(f);
        if(f==player) RegisterCameraFadeRenderers(f);
        Debug.Log("WISP_ART_MODEL_APPLIED "+wispModelSource.name+"; base="+(wispBaseColorTexture!=null)+" emission="+(wispEmissionTexture!=null)+" shader="+(wispRuntimeMaterial!=null?wispRuntimeMaterial.shader.name:"none")+" emissionProperty="+(wispRuntimeMaterial!=null&&wispRuntimeMaterial.HasProperty("_EmissionMap"))+" emissionMapped="+(wispRuntimeMaterial!=null&&wispRuntimeMaterial.GetTexture("_EmissionMap")!=null));
    }
    void BuildTankTracks(Fighter f,Color armor,Color dark,Color edge)
    {
        f.pelvisPart.gameObject.SetActive(false);
        if(f.leftLeg!=null) f.leftLeg.gameObject.SetActive(false);
        if(f.rightLeg!=null) f.rightLeg.gameObject.SetActive(false);
        Plate("BASTION low tank hull",f.legsPart,new Vector3(0,-.69f,-.02f),
            new Vector3(1.62f,.60f,1.50f),armor);
        BeveledPlate("Tank front glacis",f.legsPart,new Vector3(0,-.57f,.68f),
            new Vector3(1.34f,.42f,.28f),edge,-12,.15f,.82f);
        Plate("Tank upper deck",f.legsPart,new Vector3(0,-.32f,-.04f),
            new Vector3(1.15f,.20f,1.03f),dark);
        Plate("Tank armor prow",f.legsPart,new Vector3(0,-.47f,.68f),
            new Vector3(.76f,.10f,.15f),armor,-12);
        f.treadLinks=new Transform[2][]; f.treadPerimeter=2.8f;
        f.treadSidePhases=new float[2];
        f.previousTreadHeading=Vector3.ProjectOnPlane(f.chassisForward,Vector3.up).normalized;
        float radius=.39f, halfStraight=.43f;
        f.treadPerimeter=4f*halfStraight+2f*Mathf.PI*radius;
        for(int side=0;side<2;side++)
        {
            float x=side==0?-.83f:.83f;
            Transform track=Module("Continuous tank track "+side,f.legsPart,new Vector3(x,-.91f,0));
            float width=.60f;
            // Individual shoes form the visible continuous run; omit the solid
            // outer sidewall that hid the links in the first capture.
            f.treadLinks[side]=new Transform[28];
            for(int i=0;i<f.treadLinks[side].Length;i++)
            {
                Transform link=Plate("Individual moving tread shoe",track,Vector3.zero,
                    new Vector3(width,.105f,.15f),i%2==0?edge:dark);
                f.treadLinks[side][i]=link;
            }
            for(int wheel=0;wheel<4;wheel++)
            {
                float z=-.48f+wheel*.32f;
                Transform roller=Shape(PrimitiveType.Cylinder,"Road roller",track,new Vector3(0,-.06f,z),
                    new Vector3(.27f,.14f,.27f),edge);
                roller.localRotation=Quaternion.Euler(0,0,90);
            }
        }
        // Twin elevated command pods replace the conventional helmet silhouette.
        foreach(Transform child in f.headPart) child.gameObject.SetActive(false);
        for(int side=-1;side<=1;side+=2)
        {
            BeveledPlate("Bastion square sensor turret",f.corePart,
                new Vector3(side*.43f,1.22f,-.07f),new Vector3(.53f,.48f,.49f),armor,
                side*-5,.12f,.86f);
            Plate("Bastion sensor grille",f.corePart,new Vector3(side*.43f,1.20f,.205f),
                new Vector3(.34f,.31f,.055f),dark);
            for(int lens=0;lens<2;lens++)
                Shape(PrimitiveType.Sphere,"Bastion amber optic",f.corePart,
                    new Vector3(side*.43f+(lens==0?-.105f:.105f),1.22f+(lens==0?.07f:-.07f),.245f),
                    Vector3.one*.095f,new Color(1,.48f,.08f),true);
            Plate("Bastion pod crown",f.corePart,new Vector3(side*.43f,1.49f,-.07f),
                new Vector3(.60f,.08f,.54f),edge);
        }
        Plate("Bastion turret bridge",f.corePart,new Vector3(0,1.17f,-.10f),
            new Vector3(.34f,.23f,.34f),dark);
        // Paired shoulder missile trays echo the heavy twin-weapon concept art.
        for(int side=-1;side<=1;side+=2)
        {
            Transform arm=side<0?f.leftArm:f.rightArm;
            BeveledPlate("Bastion shoulder missile tray",arm,
                new Vector3(side*.18f,.50f,-.03f),new Vector3(.76f,.29f,.82f),dark,0,.09f,.85f);
            for(int cell=0;cell<4;cell++)
            {
                float x=(cell%2==0?-.16f:.16f), z=cell<2?.18f:-.15f;
                Plate("Bastion missile cell",arm,new Vector3(side*.18f+x,.67f,z),
                    new Vector3(.18f,.07f,.18f),edge);
                Shape(PrimitiveType.Sphere,"Bastion missile status lamp",arm,
                    new Vector3(side*.18f+x,.715f,z),Vector3.one*.055f,
                    new Color(1,.45f,.10f),true);
            }
        }
        AnimateTreads(f,0f);
    }
    void PlaceTreadLink(Transform link,float distance,float perimeter,float x,float radius,float halfStraight)
    {
        float d=Mathf.Repeat(distance,perimeter), arc=Mathf.PI*radius;
        Vector3 p; Vector3 tangent;
        if(d<2f*halfStraight) { p=new Vector3(x,radius,halfStraight-d); tangent=Vector3.back; }
        else if((d-=2f*halfStraight)<arc)
        { float a=Mathf.PI*.5f+d/radius; p=new Vector3(x,radius*Mathf.Sin(a),-halfStraight-radius*Mathf.Cos(a)); tangent=new Vector3(0,Mathf.Cos(a),-Mathf.Sin(a)); }
        else if((d-=arc)<2f*halfStraight) { p=new Vector3(x,-radius,-halfStraight+d); tangent=Vector3.forward; }
        else { d-=2f*halfStraight; float a=-Mathf.PI*.5f+d/radius; p=new Vector3(x,radius*Mathf.Sin(a),halfStraight+radius*Mathf.Cos(a)); tangent=new Vector3(0,Mathf.Cos(a),-Mathf.Sin(a)); }
        link.localPosition=p; link.localRotation=Quaternion.LookRotation(tangent,Vector3.Cross(tangent,Vector3.right));
    }
    void AnimateTreads(Fighter f,float dt)
    {
        if(f.treadLinks==null || f.treadPerimeter<=0f) return;
        // Track travel follows chassis forward, not the independently aimed root/upper body.
        Vector3 chassis=Vector3.ProjectOnPlane(f.chassisForward,Vector3.up);
        if(chassis.sqrMagnitude<.0001f) chassis=Vector3.forward;
        chassis.Normalize();
        float speed=Vector3.Dot(f.velocity,chassis);
        // Each track travels a different distance during a chassis yaw. The track
        // centers are +/-0.83m, so pure pivoting drives opposite shoe directions.
        if(f.treadSidePhases==null || f.treadSidePhases.Length!=f.treadLinks.Length)
            f.treadSidePhases=new float[f.treadLinks.Length];
        Vector3 previous=Vector3.ProjectOnPlane(f.previousTreadHeading,Vector3.up);
        if(previous.sqrMagnitude<.0001f) previous=chassis;
        float yawRadians=Vector3.SignedAngle(previous.normalized,chassis,Vector3.up)*Mathf.Deg2Rad;
        f.previousTreadHeading=chassis;
        for(int side=0;side<f.treadLinks.Length;side++)
        {
            float sideOffset=side==0?-.83f:.83f;
            f.treadSidePhases[side]=Mathf.Repeat(
                f.treadSidePhases[side]+speed*dt+yawRadians*sideOffset,f.treadPerimeter);
            for(int i=0;i<f.treadLinks[side].Length;i++)
                PlaceTreadLink(f.treadLinks[side][i],
                    f.treadSidePhases[side]+i*f.treadPerimeter/f.treadLinks[side].Length,
                    f.treadPerimeter,0,.39f,.43f);
        }
    }
    void BuildRobot(Fighter f)
    {
        f.treadLinks=null; f.treadPerimeter=0f; f.treadPhase=0f;
        f.treadSidePhases=null; f.previousTreadHeading=f.chassisForward;
        f.quadrupedLegs=null; f.quadrupedKnees=null; f.quadrupedAnkles=null;
        if(f.body!=null) { f.body.gameObject.SetActive(false); Destroy(f.body.gameObject); }
        f.body=Module("Modular robot",f.root,Vector3.zero);
        f.corePart=Module("Core module",f.body,Vector3.zero);
        f.headPart=Module("Head module",f.body,new Vector3(0,1.34f,.02f));
        f.armsPart=Module("Arm mounting frame (separate left and right sockets)",f.body,Vector3.zero);
        f.legsPart=Module("Legs and pelvis module",f.body,Vector3.zero);
        f.pelvisPart=Module("Pelvis socket",f.legsPart,Vector3.zero);
        f.backPart=Module("Backpack module",f.corePart,new Vector3(0,.42f,-.52f));
        Color armor=f.spec.color, hull=new Color(.55f,.62f,.68f), dark=new Color(.065f,.09f,.12f);
        Color edge=new Color(.28f,.35f,.40f), light=new Color(.85f,.9f,.91f);
        bool heavy=f.spec.mobility==MobilityKind.Tank;
        bool lightFrame=f.spec.mobility==MobilityKind.Quadruped;
        bool kestrelSd=f.frameIndex==0 || f.spec.mobility==MobilityKind.Runner;
        float shoulder=kestrelSd?1.00f:heavy?1.04f:lightFrame?.74f:.87f;
        Plate("Core inner frame",f.corePart,new Vector3(0,.45f,0),new Vector3(kestrelSd?.94f:.82f,.95f,kestrelSd?.70f:.62f),dark);
        BeveledPlate("Armor chest",f.corePart,new Vector3(0,.70f,.23f),
            new Vector3(heavy?1.45f:kestrelSd?1.36f:1.10f,kestrelSd?.64f:.54f,kestrelSd?.68f:.60f),armor,-12);
        Plate("Chest collar",f.corePart,new Vector3(0,1.03f,.03f),new Vector3(.85f,.18f,.55f),hull);
        Plate("Reactor glass",f.corePart,new Vector3(0,.62f,.58f),new Vector3(.27f,.22f,.06f),dark);
        Shape(PrimitiveType.Cube,"Reactor light",f.corePart,new Vector3(0,.62f,.62f),new Vector3(.14f,.07f,.025f),armor,true);
        for(int side=-1;side<=1;side+=2)
        {
            Plate("Chest trim",f.corePart,new Vector3(side*.45f,.78f,.55f),new Vector3(.18f,.08f,.06f),light);
            for(int vent=0;vent<3;vent++) Plate("Cooling vent",f.corePart,new Vector3(side*.36f,.34f+vent*.08f,.36f),new Vector3(.24f,.025f,.04f),dark);
            BeveledPlate("Armor hip skirt",f.pelvisPart,new Vector3(side*(kestrelSd?.50f:.45f),-.42f,.20f),
                new Vector3(kestrelSd?.34f:.28f,kestrelSd?.50f:.46f,kestrelSd?.47f:.43f),armor,-16);
        }
        Plate("Waist coupling",f.corePart,new Vector3(0,-.09f,0),new Vector3(.5f,.35f,.43f),dark);
        Plate("Waist rotary bearing",f.pelvisPart,new Vector3(0,-.21f,0),new Vector3(.54f,.10f,.49f),hull);
        Plate("Pelvis",f.pelvisPart,new Vector3(0,-.39f,0),new Vector3(kestrelSd?.82f:.70f,kestrelSd?.32f:.29f,kestrelSd?.61f:.55f),edge);
        // Heads intentionally use different profiles, while sharing the same socket origin.
        BeveledPlate("Helmet",f.headPart,Vector3.zero,
            new Vector3(kestrelSd?.76f:heavy?.7f:.54f,kestrelSd?.64f:.47f,kestrelSd?.61f:.50f),hull);
        Plate("Helmet crown",f.headPart,new Vector3(0,kestrelSd?.32f:.25f,-.04f),
            new Vector3(kestrelSd?.48f:.34f,kestrelSd?.16f:.13f,kestrelSd?.49f:.41f),armor,-10);
        Plate("Face mask",f.headPart,new Vector3(0,-.12f,.29f),new Vector3(kestrelSd?.39f:.28f,.22f,.12f),edge);
        Plate("Sensor housing",f.headPart,new Vector3(0,.02f,.30f),new Vector3(kestrelSd?.62f:.49f,kestrelSd?.17f:.15f,.08f),dark);
        Shape(PrimitiveType.Cube,"Visor",f.headPart,new Vector3(0,.03f,.35f),new Vector3(f.frameIndex==5?.16f:kestrelSd?.52f:.40f,kestrelSd?.075f:.065f,.025f),armor,true);
        if(f.spec.mobility==MobilityKind.Runner)
            for(int side=-1;side<=1;side+=2)
            {
                Shape(PrimitiveType.Sphere,"Wisp cyan optical lens",f.headPart,
                    new Vector3(side*.145f,.025f,.385f),new Vector3(.10f,.085f,.06f),
                    new Color(.12f,.85f,1f),true);
                Plate("Wisp lower face guard",f.headPart,
                    new Vector3(side*.12f,-.16f,.35f),new Vector3(.18f,.08f,.06f),edge,-8);
            }
        if(f.frameIndex==5) Plate("Range finder",f.headPart,new Vector3(.31f,.1f,.20f),new Vector3(.17f,.19f,.50f),dark);
        if(!heavy)
            for(int side=-1;side<=1;side+=2)
            {
                Transform fin=Plate("Sensor fin",f.headPart,new Vector3(side*.28f,.27f,0),new Vector3(.06f,.38f,.09f),armor);
                fin.localRotation=Quaternion.Euler(0,0,side*-25);
            }
        for(int side=-1;side<=1;side+=2)
        {
            Transform arm=Module(side<0?"Left arm socket":"Right arm socket",f.armsPart,new Vector3(side*shoulder,.74f,0));
            Plate("Shoulder hinge",arm,Vector3.zero,new Vector3(.28f,.36f,.42f),dark);
            BeveledPlate("Armor shoulder",arm,new Vector3(side*(kestrelSd?.16f:.12f),.08f,0),
                new Vector3(heavy?.88f:kestrelSd?.78f:.58f,heavy?.62f:kestrelSd?.58f:.45f,kestrelSd?.78f:.72f),armor,-8);
            Plate("Shoulder cap",arm,new Vector3(side*(kestrelSd?.16f:.13f),.36f,0),
                new Vector3(heavy?.83f:kestrelSd?.73f:.52f,.07f,kestrelSd?.68f:.60f),hull);
            if(f.spec.mobility==MobilityKind.Tank)
            {
                // One coherent, wide shoulder armor mass instead of decorative rods.
                // Follows the arm socket and never changes hitboxes or handling.
                BeveledPlate("Tank shoulder bulwark",arm,
                    new Vector3(side*.21f,.18f,-.07f),
                    new Vector3(.98f,.69f,.94f),armor,-6,.15f,.90f);
                Plate("Tank shoulder inset",arm,
                    new Vector3(side*.21f,.20f,.425f),
                    new Vector3(.65f,.28f,.07f),dark);
            }
            Plate("Upper arm",arm,new Vector3(0,-.37f,0),new Vector3(.30f,.42f,.33f),edge);
            Shape(PrimitiveType.Sphere,"Elbow joint",arm,new Vector3(0,-.58f,.08f),Vector3.one*.29f,dark);
            BeveledPlate("Armor forearm",arm,new Vector3(0,-.75f,.24f),new Vector3(.42f,.45f,.64f),hull,-12);
            for(int vent=0;vent<2;vent++)
                Plate("Forearm heat slit",arm,new Vector3(side*.224f,-.64f-vent*.13f,.25f),new Vector3(.025f,.045f,.30f),dark);
            Plate("Elbow servo cover",arm,new Vector3(0,-.56f,-.12f),new Vector3(.24f,.16f,.12f),armor);
            Plate("Wrist trim",arm,new Vector3(0,-.79f,.52f),new Vector3(.45f,.13f,.14f),armor);
            Plate("Manipulator",arm,new Vector3(0,-.83f,.64f),new Vector3(.28f,.28f,.28f),dark);
            if(side>0) f.fist=Plate("Punch gauntlet",arm,new Vector3(0,-.83f,.82f),new Vector3(.42f,.34f,.42f),armor);
            Transform leg=Module(side<0?"Left leg socket":"Right leg socket",f.legsPart,new Vector3(side*.30f,-.56f,0));
            BeveledPlate("Thigh",leg,new Vector3(0,-.26f,0),new Vector3(heavy?.53f:.37f,.52f,.43f),hull);
            // Explicit articulated child pivots. Positions preserve the existing assembled silhouette.
            Transform knee=Module(side<0?"Left knee pivot":"Right knee pivot",leg,new Vector3(0,-.59f,0));
            Transform ankle=Module(side<0?"Left ankle pivot":"Right ankle pivot",knee,new Vector3(0,-.75f,0));
            Plate("Knee coupling",knee,Vector3.zero,new Vector3(.31f,.22f,.34f),dark);
            Plate("Armor knee",knee,new Vector3(0,-0.010f,.25f),new Vector3(.39f,.29f,.18f),armor,-18);
            BeveledPlate("Shin",knee,new Vector3(0,-0.350f,-.02f),new Vector3(heavy?.57f:.38f,.64f,.46f),hull,-7);
            Plate("Knee piston",knee,new Vector3(0,-0.160f,-.25f),new Vector3(.10f,.42f,.10f),edge);
            Plate("Shin side trim",knee,new Vector3(side*.24f,-0.390f,.08f),new Vector3(.07f,.42f,.29f),armor);
            // Frame 01: a recessed shin intake and two floating armor lips.
            // Attached to the existing leg socket; animation and hitboxes are untouched.
            if(f.frameIndex==0)
            {
                Plate("Shin intake recess",knee,new Vector3(0,-0.410f,.228f),
                    new Vector3(.22f,.28f,.025f),dark);
                for(int slit=0;slit<3;slit++)
                    Plate("Shin intake grille "+slit,knee,
                        new Vector3(0,-.31f-slit*.075f,.248f),
                        new Vector3(.17f,.022f,.018f),edge);
                BeveledPlate("Shin upper guard",knee,new Vector3(0,-0.110f,.235f),
                    new Vector3(.34f,.12f,.13f),armor,-8);
                Plate("Shin lower guard",knee,new Vector3(0,-0.610f,.22f),
                    new Vector3(.32f,.075f,.12f),hull);
            }
            Plate("Calf booster",knee,new Vector3(0,-0.260f,-.35f),new Vector3(.27f,.46f,.28f),dark,15);
            Shape(PrimitiveType.Cube,"Calf nozzle",knee,new Vector3(0,-.51f,-.40f),new Vector3(.16f,.08f,.12f),armor,true);
            Plate("Foot",ankle,new Vector3(0,0.000f,kestrelSd?.18f:.15f),
                new Vector3(heavy?.61f:kestrelSd?.64f:.42f,kestrelSd?.25f:.22f,kestrelSd?.92f:.76f),dark);
            BeveledPlate("Toe armor",ankle,new Vector3(0,kestrelSd?.13f:.10f,kestrelSd?.43f:.38f),
                new Vector3(heavy?.57f:kestrelSd?.59f:.39f,kestrelSd?.14f:.12f,kestrelSd?.40f:.30f),armor,-10);
            // Mobility prototype silhouettes: attached to existing animated sockets,
            // with no gameplay collider or change to weapon/boost statistics.
            if(f.spec.mobility==MobilityKind.Runner)
            {
                // Readable swept-back sprinting profile, not loose rods.
                // The upper-leg fairing makes the runner's thigh a single readable
                // aerodynamic mass, without changing the hip/knee pivots or colliders.
                BeveledPlate("Runner thigh fairing",leg,new Vector3(side*.11f,-.25f,.10f),
                    new Vector3(.45f,.50f,.53f),armor,-9,.17f,.79f);
                BeveledPlate("Runner calf aero shell",knee,new Vector3(side*.17f,-0.360f,-.31f),
                    new Vector3(.25f,.76f,.48f),armor,-18,.20f,.65f);
                BeveledPlate("Runner heel spur",ankle,new Vector3(0,0.000f,-.25f),
                    new Vector3(.32f,.15f,.50f),hull,-12,.18f,.70f);
            }
            else if(f.spec.mobility==MobilityKind.Tank)
            {
                // Heavy tread-inspired greaves are a visual handling cue; this is
                // still a biped prototype, not a functional tracked vehicle.
                // Widen the continuous outer tread housing outward for a heavier tank silhouette.
                // Keep the knee pivot, tread relief and gameplay collision unchanged.
                BeveledPlate("Tank outer greave",knee,new Vector3(side*.35f,-0.320f,-.06f),
                    new Vector3(.33f,.98f,.82f),dark,0,.12f,.92f);
                BeveledPlate("Tank greave armor",knee,new Vector3(side*.48f,-0.320f,.06f),
                    new Vector3(.12f,.77f,.77f),armor,0,.17f,.78f);
                // Closed rectangular tread silhouette on the outside of each greave.
                // Cosmetic only: tread motion, ground contact and tracked steering
                // require a separate chassis rig; do not treat this as a tracked drive.
                for(int tread=0;tread<5;tread++)
                {
                    float y=-.70f+tread*.19f;
                    Plate("Tank tread front "+tread,knee,
                        new Vector3(side*.555f,y,.40f),
                        new Vector3(.055f,.15f,.085f),edge);
                    Plate("Tank tread rear "+tread,knee,
                        new Vector3(side*.555f,y,-.45f),
                        new Vector3(.055f,.15f,.085f),edge);
                }
                for(int tread=0;tread<4;tread++)
                {
                    float z=-.37f+tread*.23f;
                    Plate("Tank tread upper "+tread,knee,
                        new Vector3(side*.555f,.13f,z),
                        new Vector3(.055f,.085f,.19f),edge);
                    Plate("Tank tread lower "+tread,knee,
                        new Vector3(side*.555f,-.77f,z),
                        new Vector3(.055f,.085f,.19f),edge);
                }
            }
            Plate("Backpack booster",f.backPart,new Vector3(side*.38f,0,-.10f),new Vector3(.31f,.90f,.43f),edge);
            Shape(PrimitiveType.Cube,"Backpack nozzle",f.backPart,new Vector3(side*.38f,-.48f,-.14f),new Vector3(.23f,.09f,.27f),armor,true);
            // Frame 01 only: recessed armored thruster cassette and service detailing.
            // These decorative pieces remain parented to the existing backpack socket.
            if(f.frameIndex==0)
            {
                Plate("Thruster outer rail",f.backPart,new Vector3(side*.58f,.04f,-.13f),
                    new Vector3(.075f,.83f,.42f),dark);
                Plate("Thruster upper service cap",f.backPart,new Vector3(side*.38f,.49f,-.13f),
                    new Vector3(.40f,.10f,.49f),hull);
                Plate("Thruster lower guard",f.backPart,new Vector3(side*.38f,-.38f,-.38f),
                    new Vector3(.39f,.12f,.11f),dark);
                for(int rib=0;rib<3;rib++)
                    Plate("Thruster heat rib "+rib,f.backPart,
                        new Vector3(side*.38f,.22f-rib*.19f,-.365f),
                        new Vector3(.30f,.055f,.09f),hull);
                Plate("Thruster status lamp",f.backPart,new Vector3(side*.38f,.37f,-.375f),
                    new Vector3(.14f,.07f,.04f),armor);
            }
            if(f.frameIndex==0||f.frameIndex==6)
            {
                Transform wing=Plate("Stabilizer",f.backPart,new Vector3(side*.69f,.21f,-.2f),new Vector3(.17f,.9f,.30f),armor,-20);
                wing.localRotation=Quaternion.Euler(-20,0,side*-30);
            }
            if(f.frameIndex==3||f.frameIndex==7)
            {
                Transform pod=Plate("Launcher pod",f.backPart,new Vector3(side*.71f,.70f,.07f),new Vector3(.49f,.55f,.85f),dark,-12);
                for(int n=0;n<4;n++) Shape(PrimitiveType.Cube,"Missile cap",pod,new Vector3(n%2*.44f-.22f,n/2*.40f-.20f,.51f),new Vector3(.27f,.25f,.05f),armor);
            }
            if(side<0) { f.leftArm=arm;f.leftLeg=leg; } else {f.rightArm=arm;f.rightLeg=leg;}
        }
        if(f.spec.mobility==MobilityKind.Runner)
        {
            // Push WISP toward a true super-deformed read: head dominates, torso
            // and limbs stay compact, and broad feet keep the tiny frame grounded.
            f.headPart.localScale=Vector3.one*2.02f;
            f.headPart.localPosition=new Vector3(0,1.16f,.02f);
            f.corePart.localScale=new Vector3(.91f,.90f,.92f);
            f.armsPart.localScale=new Vector3(.88f,.74f,.88f);
            f.legsPart.localScale=new Vector3(1f,.50f,1f);
            BeveledPlate("Wisp cheek guard L",f.headPart,new Vector3(-.32f,-.12f,.24f),
                new Vector3(.20f,.20f,.20f),armor,0,.06f,.78f);
            BeveledPlate("Wisp cheek guard R",f.headPart,new Vector3(.32f,-.12f,.24f),
                new Vector3(.20f,.20f,.20f),armor,0,.06f,.78f);
            for(int side=-1;side<=1;side+=2)
            {
                Transform antenna=Plate("Wisp swept antenna",f.headPart,
                    new Vector3(side*.30f,.37f,-.04f),new Vector3(.075f,.38f,.10f),edge);
                antenna.localRotation=Quaternion.Euler(0,0,side*-24f);
                Shape(PrimitiveType.Sphere,"Wisp antenna lamp",f.headPart,
                    new Vector3(side*.38f,.53f,-.04f),Vector3.one*.09f,armor,true);
            }
        }
        else if(f.spec.mobility==MobilityKind.Tank) BuildTankTracks(f,armor,dark,edge);
        else if(f.spec.mobility==MobilityKind.Quadruped)
        {
            bool spider=f.spec.name=="TARANTULA";
            f.corePart.localPosition=new Vector3(0,-.18f,0);
            f.corePart.localScale=new Vector3(spider?.90f:.88f,spider?.72f:.78f,.88f);
            f.armsPart.localPosition=new Vector3(0,-.22f,0);
            f.armsPart.localScale=new Vector3(.82f,.68f,.82f);
            f.headPart.localPosition=new Vector3(0,spider?.04f:-.04f,spider?.65f:1.00f);
            f.headPart.localScale=spider?Vector3.one*.78f:new Vector3(1.12f,1.10f,1.34f);
            f.legsPart.localScale=new Vector3(spider?1.12f:1.08f,1f,spider?1.05f:1.08f);
            BuildFourLegs(f,armor,dark,edge);
        }
        else if(f.spec.mobility==MobilityKind.Tank)
        {
            // A heavier, wider shoulder line balances the new true track base.
            f.corePart.localScale=new Vector3(1.08f,1.02f,1.04f);
            f.armsPart.localScale=new Vector3(1.05f,.96f,1.04f);
        }
        f.weaponPart=Module("Right weapon socket",f.rightArm,new Vector3(0,-.75f,.70f));
        if(f.spec.mobility==MobilityKind.Quadruped)
        {
            f.weaponPart.SetParent(f.legsPart,false);
            f.weaponPart.localPosition=new Vector3(0,-.34f,.53f);
            f.weaponPart.localRotation=Quaternion.identity;
            f.weaponPart.localScale=Vector3.one*.82f;
        }
        f.gun=Module("Primary weapon module",f.weaponPart,Vector3.zero);
        bool longGun=f.frameIndex==5; float length=longGun?2.6f:heavy?1.55f:1.05f;
        Plate("Weapon receiver",f.gun,new Vector3(0,0,.26f),new Vector3(heavy?.58f:.34f,.34f,.80f),dark);
        Plate("Weapon casing",f.gun,new Vector3(0,.20f,.30f),new Vector3(.30f,.09f,.63f),armor);
        if(f.frameIndex==2||f.frameIndex==4)
        {
            for(int n=0;n<(f.frameIndex==2?3:2);n++)
                Plate("Multi barrel",f.gun,new Vector3((n-1)*.13f,0,.9f),new Vector3(.09f,.12f,.76f),edge);
        }
        else
        {
            Plate("Barrel",f.gun,new Vector3(0,0,length*.64f),new Vector3(longGun?.15f:.27f,.20f,length),edge);
            Plate("Muzzle",f.gun,new Vector3(0,0,length*1.12f),new Vector3(.35f,.30f,.18f),dark);
            Shape(PrimitiveType.Cube,"Bore glow",f.gun,new Vector3(0,0,length*1.12f+.10f),new Vector3(.19f,.13f,.03f),armor,true);
        }
        Transform leftWeapon=Module("Left weapon socket",f.leftArm,new Vector3(0,-.7f,.7f));
        Plate("Secondary emitter",leftWeapon,Vector3.zero,new Vector3(.26f,.30f,.54f),dark);
        f.saber=Shape(PrimitiveType.Cube,"Energy saber",leftWeapon,new Vector3(0,0,1.4f),new Vector3(.13f,.13f,2.8f),armor,true);
        f.saber.gameObject.SetActive(false);
        f.shield=Module("Shield flash",f.body,new Vector3(0,.2f,0));
        for(int axis=0;axis<3;axis++)
        {
            LineRenderer field=Line("Shield contour",armor,.055f); field.transform.SetParent(f.shield,false);
            field.useWorldSpace=false; Circle(field,Vector3.zero,1.55f);
            field.transform.localRotation=axis==0?Quaternion.identity:Quaternion.Euler(axis==1?90:0,0,axis==2?90:0);
        }
        f.shield.gameObject.SetActive(false);
        // Visual-only model-kit pass: raised seams, colored vents, and readable
        // silhouette accents. No colliders or combat parameters are changed.
        Color seam=new Color(.12f,.17f,.21f);
        Color lamp=Color.Lerp(armor,Color.white,.55f);
        Plate("Chest armor split",f.corePart,new Vector3(0,.79f,.548f),new Vector3(.045f,.32f,.045f),seam,-12);
        Plate("Lower chest seam",f.corePart,new Vector3(0,.47f,.49f),new Vector3(.71f,.035f,.05f),seam);
        Plate("Core status indicator",f.corePart,new Vector3(0,.64f,.665f),new Vector3(.10f,.05f,.025f),lamp);
        for(int side=-1;side<=1;side+=2)
        {
            Plate("Chest shoulder gasket",f.corePart,new Vector3(side*.65f,.81f,.03f),
                new Vector3(.12f,.37f,.46f),dark,side*8);
            Plate("Hip armor accent",f.pelvisPart,new Vector3(side*.46f,-.35f,.43f),
                new Vector3(.19f,.045f,.035f),light);
            Transform arm=side<0?f.leftArm:f.rightArm;
            Transform leg=side<0?f.leftLeg:f.rightLeg;
            Plate("Shoulder inset seam",arm,new Vector3(side*.13f,.12f,.367f),
                new Vector3(heavy?.60f:.39f,.035f,.025f),seam);
            Plate("Shoulder unit marking",arm,new Vector3(side*.12f,.23f,.375f),
                new Vector3(.16f,.055f,.025f),lamp);
            Plate("Forearm armor seam",arm,new Vector3(0,-.72f,.565f),
                new Vector3(.29f,.035f,.025f),seam);
            Plate("Knee running light",leg,new Vector3(0,-.59f,.356f),
                new Vector3(.19f,.045f,.025f),lamp);
            Plate("Shin center ridge",leg,new Vector3(0,-.96f,.223f),
                new Vector3(.055f,.40f,.035f),armor,-7);
            Plate("Toe split line",leg,new Vector3(0,-1.175f,.54f),
                new Vector3(.03f,.035f,.20f),seam);
            Plate("Backpack caution stripe",f.backPart,new Vector3(side*.38f,.27f,-.325f),
                new Vector3(.19f,.06f,.035f),lamp);
        }
        Plate("Helmet eyebrow",f.headPart,new Vector3(0,.13f,.32f),
            new Vector3(heavy?.62f:.48f,.055f,.09f),armor,-8);
        Plate("Chin guard",f.headPart,new Vector3(0,-.245f,.305f),
            new Vector3(.21f,.065f,.095f),dark);
        // Prototype the 01 standard unit's higher-resolution voxel skin.
        // Keep the existing modular skeleton, joints and gameplay hit volumes.
        if(f.frameIndex==0)
        {
            Color ceramic=new Color(.81f,.82f,.78f);
            Color steel=new Color(.19f,.25f,.34f);
            Color highlight=new Color(.94f,.89f,.76f);
            // Staggered small plates form a layered visor brow and chest shell.
            for(int x=-3;x<=3;x++)
            {
                float px=x*.155f;
                float height=.09f+(3-Mathf.Abs(x))*.022f;
                Plate("01 stepped helmet brow",f.headPart,
                    new Vector3(px,.20f+height*.25f,.29f),
                    new Vector3(.146f,height,.17f),x%2==0?ceramic:highlight);
            }
            for(int x=-3;x<=3;x++)
            {
                float px=x*.155f;
                float depth=.60f-Mathf.Abs(x)*.027f;
                Plate("01 segmented breastplate",f.corePart,
                    new Vector3(px,.88f,depth),
                    new Vector3(.145f,.13f,.105f),x%3==0?steel:ceramic,-12);
            }
            // 01 torso shoulder-root fairings: segmented floating armor over
            // the fixed core, leaving the moving arm sockets unobstructed.
            // These are purely visual; collision and CraftSpec are unchanged.
            for(int side=-1;side<=1;side+=2)
            {
                BeveledPlate("01 upper chest shoulder fairing",f.corePart,
                    new Vector3(side*.43f,.89f,.31f),
                    new Vector3(.28f,.19f,.35f),ceramic,-14,.12f,.86f);
                Plate("01 fairing dark undercut",f.corePart,
                    new Vector3(side*.45f,.76f,.48f),
                    new Vector3(.23f,.055f,.045f),steel,-14);
                for(int rib=0;rib<2;rib++)
                    Plate("01 collar vent rib",f.corePart,
                        new Vector3(side*(.27f+rib*.11f),1.045f,.23f),
                        new Vector3(.055f,.035f,.19f),rib==0?steel:highlight);
            }
            for(int side=-1;side<=1;side+=2)
            {
                Transform arm=side<0?f.leftArm:f.rightArm;
                Transform leg=side<0?f.leftLeg:f.rightLeg;
                for(int row=0;row<3;row++)
                {
                    Plate("01 shoulder armor tile",arm,
                        new Vector3(side*.13f,.27f-row*.14f,.38f),
                        new Vector3(.42f-row*.035f,.12f,.095f),
                        row==1?steel:ceramic);
                    Plate("01 shin layered armor",leg,
                        new Vector3(0,-.76f-row*.19f,.235f),
                        new Vector3(.34f-row*.025f,.17f,.10f),
                        row==1?steel:ceramic,-7);
                }
                Plate("01 backpack exhaust guard",f.backPart,
                    new Vector3(side*.38f,-.31f,-.35f),
                    new Vector3(.29f,.20f,.13f),steel);
                // Frame 01 rear heat exchanger: shallow recessed fins, kept
                // behind the core to avoid changing the forward silhouette.
                for(int fin=0;fin<4;fin++)
                {
                    Plate("01 rear radiator fin",f.backPart,
                        new Vector3(side*.38f,.12f-fin*.105f,-.355f),
                        new Vector3(.23f,.038f,.045f),fin==0?highlight:steel);
                }
                // Mechanical shoulder identification stripes follow arm animation.
                for(int stripe=0;stripe<2;stripe++)
                {
                    Plate("01 shoulder hazard stripe",arm,
                        new Vector3(side*.13f,.33f-stripe*.065f,.445f),
                        new Vector3(.19f,.028f,.025f),stripe==0?highlight:steel);
                }
                // Rear-facing stacked nozzle trim: follows the backpack module,
                // and is decorative only (existing boost logic is untouched).
                for(int ring=0;ring<3;ring++)
                {
                    float width=.34f-ring*.055f;
                    Plate("01 booster nozzle collar",f.backPart,
                        new Vector3(side*.38f,-.50f-ring*.065f,-.16f),
                        new Vector3(width,.045f,width),ring==1?highlight:steel);
                }
                Plate("01 booster exhaust core",f.backPart,
                    new Vector3(side*.38f,-.68f,-.16f),
                    new Vector3(.12f,.025f,.12f),new Color(.28f,.77f,.95f));
                // Side-mounted knee actuator and mechanical inset stay attached
                // to the leg module so movement does not leave parts behind.
                Plate("01 knee side actuator",leg,
                    new Vector3(side*.235f,-.59f,-.045f),
                    new Vector3(.085f,.19f,.20f),steel);
                Plate("01 knee actuator pin",leg,
                    new Vector3(side*.29f,-.59f,-.045f),
                    new Vector3(.035f,.09f,.09f),highlight);
                // 01 ankle articulation: shallow guards, exposed hinge blocks
                // and a toe-side maintenance seam. Parent to the existing leg
                // module, not the root, so animation and collision stay intact.
                Plate("01 ankle flexible gaiter",leg,
                    new Vector3(0,-1.185f,-.015f),
                    new Vector3(.29f,.085f,.36f),steel);
                for(int ankleSide=-1;ankleSide<=1;ankleSide+=2)
                {
                    Plate("01 ankle pivot bracket",leg,
                        new Vector3(ankleSide*.235f,-1.225f,.045f),
                        new Vector3(.075f,.19f,.24f),ceramic);
                    Plate("01 ankle hinge pin",leg,
                        new Vector3(ankleSide*.28f,-1.225f,.065f),
                        new Vector3(.035f,.075f,.085f),highlight);
                    Plate("01 toe outer guard",leg,
                        new Vector3(ankleSide*.215f,-1.275f,.425f),
                        new Vector3(.09f,.10f,.25f),ceramic,-10);
                }
                Plate("01 toe maintenance seam",leg,
                    new Vector3(0,-1.169f,.525f),
                    new Vector3(.25f,.018f,.035f),steel,-10);
                Plate("01 shoulder hinge cover",arm,
                    new Vector3(side*.37f,-.02f,-.02f),
                    new Vector3(.085f,.24f,.29f),steel);
            }
        }
        AddFrameIdentity(f,armor,hull,dark,edge);
        ApplyImportedWisp(f);
    }
    RenderTexture[] framePortraits;
    void BuildPortraits()
    {
        framePortraits=new RenderTexture[CraftCatalog.Selectable.Length];
        Fighter model=MakeFighter("Portrait studio robot",Cyan); model.root.position=new Vector3(0,60,0);
        Camera camera=new GameObject("Portrait studio camera").AddComponent<Camera>();
        camera.enabled=false; camera.orthographic=true; camera.orthographicSize=2.9f; camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(.065f,.095f,.14f,1); camera.cullingMask=1<<8;
        camera.transform.position=model.root.position+new Vector3(5,3.5f,7); camera.transform.LookAt(model.root.position+Vector3.up*.1f);
        for(int i=0;i<CraftCatalog.Selectable.Length;i++)
        {
            ApplySelectableCharacter(model,i); model.body.localPosition=Vector3.up*.4f;
            model.gun.gameObject.SetActive(true); model.fist.gameObject.SetActive(false);
            foreach(Transform t in model.body.GetComponentsInChildren<Transform>()) t.gameObject.layer=8;
            framePortraits[i]=new RenderTexture(192,224,16); camera.targetTexture=framePortraits[i]; camera.Render();
        }
        camera.targetTexture=null; Destroy(camera.gameObject); Destroy(model.root.gameObject); Destroy(model.thruster.gameObject); Destroy(model.shadow.gameObject);
    }
}
