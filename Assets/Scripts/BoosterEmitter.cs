using UnityEngine;
using UnityEngine.Rendering;

// Reusable local +Z exhaust. Attach to any socket, orient it, then call SetThrust.
[DisallowMultipleComponent]
public sealed class BoosterEmitter : MonoBehaviour
{
    public float riseSeconds=.18f,fallSeconds=.24f,pulseAmount=.025f,pulseHz=2.8f,size=1f;
    public Color outerColor=new Color(.35f,.72f,1f,.32f),innerColor=new Color(.86f,.95f,1f,.42f);
    public float Envelope { get; private set; }
    public float Target { get; private set; }
    public bool Visible=>Envelope>.001f;
    Renderer[] shells;Material[] materials;Mesh[] meshes;float clock;
    void Awake()
    {
        shells=new Renderer[2];materials=new Material[2];meshes=new Mesh[2];
        Shader shader=Resources.Load<Shader>("BoosterPlume");
        for(int i=0;i<2;i++)
        {
            GameObject shell=new GameObject(i==0?"Soft outer exhaust":"Pale exhaust core");shell.layer=gameObject.layer;shell.transform.SetParent(transform,false);
            meshes[i]=BuildMesh(i==0?.30f:.15f,i==0?1.6f:1.1f);
            shell.AddComponent<MeshFilter>().sharedMesh=meshes[i];shells[i]=shell.AddComponent<MeshRenderer>();
            materials[i]=new Material(shader);shells[i].sharedMaterial=materials[i];shells[i].shadowCastingMode=ShadowCastingMode.Off;shells[i].receiveShadows=false;
        }
        Advance(0);
    }
    public void SetThrust(float amount){Target=Mathf.Clamp01(amount);}
    public void SetWorldPose(Vector3 origin,Vector3 exhaustDirection)
    {
        if(exhaustDirection.sqrMagnitude<.0001f)return;
        Vector3 axis=exhaustDirection.normalized;transform.SetPositionAndRotation(origin,Quaternion.LookRotation(axis,Mathf.Abs(Vector3.Dot(axis,Vector3.up))>.95f?Vector3.forward:Vector3.up));
    }
    public void ResetEmission(){Target=Envelope=0;Advance(0);}
    void Update(){Advance(Time.deltaTime);}
    public void Advance(float dt)
    {
        dt=Mathf.Max(0,dt);clock+=dt;
        Envelope=Mathf.MoveTowards(Envelope,Target,dt/Mathf.Max(.01f,Target>Envelope?riseSeconds:fallSeconds));
        float pulse=Envelope>=.99f?1+Mathf.Sin(clock*pulseHz*Mathf.PI*2)*pulseAmount:1;
        float scale=size*Envelope*pulse;transform.localScale=new Vector3(scale,scale,scale);
        if(shells==null)return;
        for(int i=0;i<shells.Length;i++)
        {
            shells[i].enabled=Visible;Color color=i==0?outerColor:innerColor;color.a*=Envelope;materials[i].SetColor("_Tint",color);
        }
    }
    static Mesh BuildMesh(float radius,float length)
    {
        const int segments=24;float[] z={0,.23f,1};float[] widths={.65f,1,0};
        Vector3[] vertices=new Vector3[segments*3];Color[] colors=new Color[vertices.Length];int[] triangles=new int[segments*12];
        for(int ring=0;ring<3;ring++)for(int j=0;j<segments;j++)
        {
            float angle=j*Mathf.PI*2/segments;int index=ring*segments+j;
            vertices[index]=new Vector3(Mathf.Cos(angle)*radius*widths[ring],Mathf.Sin(angle)*radius*widths[ring],z[ring]*length);
            colors[index]=new Color(1,1,1,ring==2?0:ring==0?.7f:1);
        }
        int t=0;for(int ring=0;ring<2;ring++)for(int j=0;j<segments;j++)
        {
            int a=ring*segments+j,b=ring*segments+(j+1)%segments,c=a+segments,d=b+segments;
            triangles[t++]=a;triangles[t++]=c;triangles[t++]=b;triangles[t++]=b;triangles[t++]=c;triangles[t++]=d;
        }
        Mesh mesh=new Mesh{name="Reusable soft plume"};mesh.vertices=vertices;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateBounds();return mesh;
    }
    void OnDestroy(){if(materials!=null)foreach(Material m in materials)if(m!=null)Destroy(m);if(meshes!=null)foreach(Mesh mesh in meshes)if(mesh!=null)Destroy(mesh);}
}
