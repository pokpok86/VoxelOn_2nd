using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable] public sealed class BalanceNumber
{
    public string key,label,unit;public float value,min,max;public bool implemented=true,integer;
    public BalanceNumber(string key,string label,float value,float min,float max,string unit="",bool implemented=true,bool integer=false){this.key=key;this.label=label;this.value=value;this.min=min;this.max=max;this.unit=unit;this.implemented=implemented;this.integer=integer;}
}
[Serializable] public sealed class BalanceWeapon
{
    public string id,name,family,kind,icon="",notes="";public bool hands=true,shoulders=true,left=true,right=true;public BalanceNumber[] numbers;
    public float Get(string key)=>numbers.First(n=>n.key==key).value;
    public void Set(string key,float value)=>numbers.First(n=>n.key==key).value=value;
}
[Serializable] public sealed class BalancePart {public string id,frame,category,name,notes="";public BalanceNumber[] numbers;}
[Serializable] public sealed class BalanceFrame {public string id;public string[] slots;}
[Serializable] public sealed class BalanceDocument
{
    public int formatVersion=1;public string name="PD Balance 01",notes="";public BalanceWeapon[] weapons;public BalancePart[] parts;public BalanceFrame[] frames;
}
public static class BalanceData
{
    [Serializable] sealed class ArtEntry {public string id,title,family,kind;}
    [Serializable] sealed class ArtCatalog {public ArtEntry[] models;}
    public static BalanceDocument Copy(BalanceDocument d)=>JsonUtility.FromJson<BalanceDocument>(JsonUtility.ToJson(d));
    public static BalanceDocument Defaults()
    {
        TextAsset text=Resources.Load<TextAsset>("PDParts/Weapons20261105/catalog_v003");if(text==null)throw new InvalidOperationException("Missing weapon catalog");
        ArtEntry[] art=JsonUtility.FromJson<ArtCatalog>(text.text).models;var weapons=new List<BalanceWeapon>();
        foreach(ArtEntry e in art)
        {
            bool gun=e.kind=="firearm";float capacity=30,damage=8.05f,interval=.18f/.65f,speed=132,radius=.24f,range=82,blast=0;
            if(e.id=="Military_GL"){capacity=8;damage=27.6f;interval=1;speed=110;radius=.29f;range=92;}
            else if(e.id=="Military_RL"||e.id=="Future_Launcher"){capacity=12;damage=14.95f;interval=.42f/.65f;speed=100;radius=.28f;range=100;blast=2;}
            else if(e.id=="Future_Sniper"||e.id=="Military_SR"){capacity=4;damage=46;interval=1.1f/.65f;speed=180;radius=.20f;range=130;}
            else if(e.id.Contains("LMG")||e.id.Contains("Gatling")){capacity=60;damage=5;interval=.15f;}
            else if(e.id.Contains("SG")||e.id.Contains("Shotgun")){capacity=8;damage=18;interval=.7f;}
            else if(e.id=="Military_HG"){capacity=12;damage=9;interval=.35f;}
            var n=new List<BalanceNumber>{
                new BalanceNumber("capacity","탄창",capacity,1,999,"발",gun,true),new BalanceNumber("damage","공격력",damage,0,1000,"",gun),
                new BalanceNumber("interval","발사 간격",interval,.05f,10,"초",gun),new BalanceNumber("speed","탄속",speed,1,600,"m/s",gun),
                new BalanceNumber("radius","탄환 판정 반경",radius,.02f,.8f,"m",gun),new BalanceNumber("range","사거리",range,1,300,"m",gun),
                new BalanceNumber("partialReload","잔탄 장전",2.5f,.1f,30,"초",gun),new BalanceNumber("emptyReload","빈 탄창 장전",4,.1f,30,"초",gun),
                new BalanceNumber("idleReload","자동 장전 대기",2.2f,.1f,60,"초",gun),new BalanceNumber("spread","추가 탄퍼짐",0,0,20,"도",gun),new BalanceNumber("blast","폭발 반경",blast,0,15,"m",gun),
                new BalanceNumber("weight","무기 무게",5,0,1000,"kg",false),new BalanceNumber("ammoWeight","탄약 1발 무게",.05f,0,100,"kg",false),
                new BalanceNumber("recoil","반동",0,0,100,"",false),new BalanceNumber("lockStrength","락온 성능",0,0,1,"",false),new BalanceNumber("lockPenalty","락온 이동 페널티",0,0,1,"",false),
                new BalanceNumber("energyPerShot","발사 에너지",0,0,1000,"",false),new BalanceNumber("heatPerShot","발사 발열",0,0,1000,"",false),new BalanceNumber("cooling","초당 냉각",10,0,1000,"",false),new BalanceNumber("maxHeat","과열 한계",100,1,10000,"",false)
            };
            weapons.Add(new BalanceWeapon{id=e.id,name=e.id.Replace('_',' '),family=e.family,kind=e.kind,icon=e.title,hands=gun,shoulders=gun&&e.id!="Military_HG"&&e.id!="Military_SMG",numbers=n.ToArray()});
        }
        BalanceWeapon shield=JsonUtility.FromJson<BalanceWeapon>(JsonUtility.ToJson(weapons.First(w=>w.id=="Future_Shield")));
        shield.id="WISP_GuardShield";shield.name="기본 방패 (WISP 오른손)";shield.kind="shield";shield.family="existing";shield.icon="GUARD";shield.hands=true;shield.shoulders=false;shield.left=false;shield.right=true;shield.notes="기존 WISP 방패와 우클릭 가드 모션. 같은 손의 총과 동시에 장착하지 않습니다.";weapons.Add(shield);
        BalanceWeapon empty=JsonUtility.FromJson<BalanceWeapon>(JsonUtility.ToJson(weapons.First(w=>w.id=="Military_HG")));
        empty.id="UNARMED";empty.name="미장착 (손은 주먹)";empty.kind="unarmed";empty.family="builtin";empty.icon="FIST";empty.hands=empty.shoulders=empty.left=empty.right=true;
        empty.notes="손 슬롯은 왼쪽 클릭으로 주먹 공격, 어깨 슬롯은 장착물 없음.";empty.Set("capacity",1);empty.Set("damage",15);empty.Set("interval",.6f);empty.Set("range",2.2f);empty.Set("radius",.3f);
        foreach(BalanceNumber n in empty.numbers)n.implemented=n.key=="damage"||n.key=="interval"||n.key=="range"||n.key=="radius";weapons.Add(empty);
        var parts=new List<BalancePart>();var frames=new List<BalanceFrame>();
        foreach(CraftSpec preset in CraftCatalog.Selectable.Take(CraftCatalog.UnlockedCount))
        {
            CraftSpec f=preset.RuntimeCopy();
            // Match the existing neutral part allocation used when spawning the three prototypes.
            if(f.name!="WISP"){f.forwardSpeedFactor*=1.48f;f.reverseSpeedFactor*=1.16f;f.leftSpeedFactor*=.68f;f.rightSpeedFactor*=.68f;}
            frames.Add(new BalanceFrame{id=f.name,slots=new[]{"Military_AR","Military_GL","Military_RL","Future_Sniper"}});
            Add("HEAD","머리 / 센서",new[]{N("lockRange","락온 범위",1,200,"m"),N("radarRange","레이더 범위",1,200,"m"),N("aimSpreadDegrees","기체 탄퍼짐",0,15,"도"),N("occludedLockSeconds","가림 기억 시간",0,5,"초")});
            Add("CORE","코어 / 몸체",new[]{N("health","내구도",1,5000),N("armor","피해 감소율",0,.9f),N("boostCapacity","에너지 용량",1,2000)});
            Add("LEFT_ARM","왼팔",new[]{N("leftArmHandling","조작 성능",.2f,3),N("leftArmStability","안정성",.2f,3)});
            Add("RIGHT_ARM","오른팔",new[]{N("rightArmHandling","조작 성능",.2f,3),N("rightArmStability","안정성",.2f,3)});
            Add("LEGS","하체 / 이동",new[]{N("speed","기본 속도",1,60,"m/s"),N("prototypeBaseSpeed","전용 이동 기준속도",1,60,"m/s"),N("forwardSpeedFactor","전진 비율",.1f,2),N("reverseSpeedFactor","후진 비율",.1f,2),N("leftSpeedFactor","좌측 비율",.1f,2),N("rightSpeedFactor","우측 비율",.1f,2),N("jumpImpulseScale","점프 충격 배율",.1f,3),N("jumpHeightScale","점프 높이 배율",.1f,3),N("fallGravityFactor","낙하 중력 배율",.2f,4),N("stepHeight","자동 턱 넘기 높이",0,.8f,"m")});
            Add("BOOSTER","부스터",new[]{N("boostSpeed","부스트 속도",1,90,"m/s"),N("boostDrain","초당 에너지 소모",.1f,200),N("verticalBoostScale","수직 추력 배율",0,3)});
            BalanceNumber N(string key,string label,float min,float max,string unit="")=>new BalanceNumber(key,label,(float)typeof(CraftSpec).GetField(key).GetValue(f),min,max,unit);
            void Add(string category,string name,BalanceNumber[] values)
            {
                var all=new List<BalanceNumber>(values){new BalanceNumber("plannedWeight","파츠 무게",5,0,1000,"kg",false)};
                parts.Add(new BalancePart{id=f.name+"_"+category,frame=f.name,category=category,name=name,numbers=all.ToArray()});
            }
        }
        return new BalanceDocument{weapons=weapons.ToArray(),parts=parts.ToArray(),frames=frames.ToArray()};
    }
    public static BalanceDocument ValidateAndNormalize(BalanceDocument raw)
    {
        if(raw==null||raw.formatVersion!=1)throw new InvalidDataException("지원하지 않는 설계 파일 버전입니다.");
        BalanceDocument result=Defaults();
        if(raw.weapons==null||raw.parts==null||raw.frames==null||raw.weapons.Length!=result.weapons.Length||raw.parts.Length!=result.parts.Length||raw.frames.Length!=result.frames.Length)throw new InvalidDataException("무기·파츠·기체 목록이 누락되었습니다.");
        result.name=Text(raw.name,80);result.notes=Text(raw.notes,2000);
        var seen=new HashSet<string>();
        foreach(BalanceWeapon w in raw.weapons)
        {
            if(w==null||w.id==null||!seen.Add(w.id))throw new InvalidDataException("무기 ID가 중복되거나 비어 있습니다.");
            BalanceWeapon dest=result.weapons.FirstOrDefault(x=>x.id==w.id);if(dest==null)throw new InvalidDataException("알 수 없는 무기 ID: "+w.id);
            dest.name=Text(w.name,80);dest.notes=Text(w.notes,2000);dest.icon=Text(w.icon,40);dest.hands=w.hands;dest.shoulders=w.shoulders;dest.left=w.left;dest.right=w.right;Numbers(dest.numbers,w.numbers,w.id);
        }
        seen.Clear();foreach(BalancePart p in raw.parts)
        {
            if(p==null||p.id==null||!seen.Add(p.id))throw new InvalidDataException("파츠 ID가 중복되거나 비어 있습니다.");BalancePart dest=result.parts.FirstOrDefault(x=>x.id==p.id);if(dest==null)throw new InvalidDataException("알 수 없는 파츠 ID");dest.notes=Text(p.notes,2000);Numbers(dest.numbers,p.numbers,p.id);
        }
        seen.Clear();foreach(BalanceFrame f in raw.frames)
        {
            if(f==null||f.id==null||!seen.Add(f.id)||f.slots==null||f.slots.Length!=4)throw new InvalidDataException("기체별 무장 4개가 필요합니다.");
            BalanceFrame dest=result.frames.FirstOrDefault(x=>x.id==f.id);if(dest==null)throw new InvalidDataException("알 수 없는 기체 ID");
            for(int i=0;i<4;i++)
            {
                BalanceWeapon w=result.weapons.FirstOrDefault(x=>x.id==f.slots[i]);
                if(w==null||!CanEquip(w,i,f.id))throw new InvalidDataException(f.id+"의 "+(i+1)+"번 슬롯에 장착할 수 없는 무기입니다.");dest.slots[i]=w.id;
            }
        }
        return result;
    }
    static string Text(string value,int max){if(value==null)return "";if(value.Length>max)throw new InvalidDataException("이름 또는 메모가 너무 깁니다.");return value;}
    static void Numbers(BalanceNumber[] expected,BalanceNumber[] input,string owner)
    {
        if(input==null||input.Length!=expected.Length)throw new InvalidDataException(owner+": 수치 항목이 누락되었습니다.");var seen=new HashSet<string>();
        foreach(BalanceNumber n in input)
        {
            if(n==null||n.key==null||!seen.Add(n.key))throw new InvalidDataException(owner+": 수치 키 중복");BalanceNumber dest=expected.FirstOrDefault(x=>x.key==n.key);
            if(dest==null||float.IsNaN(n.value)||float.IsInfinity(n.value)||n.value<dest.min||n.value>dest.max||(dest.integer&&n.value!=Mathf.Round(n.value)))throw new InvalidDataException(owner+" / "+n.key+": 허용 범위를 벗어난 수치입니다.");dest.value=n.value;
        }
    }
    public static bool CanEquip(BalanceWeapon w,int slot,string frame=null)=>(w.kind=="firearm"||w.kind=="unarmed"||(w.id=="WISP_GuardShield"&&slot==1&&(frame==null||frame=="WISP")))&&(slot<2?w.hands:w.shoulders)&&(slot%2==0?w.left:w.right);
}
public static class BalanceFiles
{
    public static void Save(string path,BalanceDocument draft)
    {
        BalanceDocument valid=BalanceData.ValidateAndNormalize(draft);string json=JsonUtility.ToJson(valid,true);Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";
        try{File.WriteAllText(temp,json,new System.Text.UTF8Encoding(false));if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);}finally{if(File.Exists(temp))File.Delete(temp);}
    }
    public static BalanceDocument Load(string path)
    {
        if(new FileInfo(path).Length>5*1024*1024)throw new InvalidDataException("파일 크기 제한은 5MB입니다.");
        return BalanceData.ValidateAndNormalize(JsonUtility.FromJson<BalanceDocument>(File.ReadAllText(path)));
    }
}
