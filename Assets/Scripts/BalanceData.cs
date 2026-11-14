using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable] public sealed class BalanceNumber
{
    public string key,label,unit;public float value,min,max,safeMin,safeMax;public bool implemented=true,integer;
    public BalanceNumber(string key,string label,float value,float min,float max,string unit="",bool implemented=true,bool integer=false){this.key=key;this.label=label;this.value=value;this.min=min;this.max=max;safeMin=min;safeMax=max;this.unit=unit;this.implemented=implemented;this.integer=integer;}
}
[Serializable] public sealed class BalanceWeapon
{
    public string id,name,family,kind,icon="",traitIconId="",notes="";public string[] additionalTraitIconIds;public bool hands=true,shoulders=true,left=true,right=true,allowsShoulderDuplicate=true,blocksShoulderWeapons;public BalanceNumber[] numbers;
    public float Get(string key)=>numbers.First(n=>n.key==key).value;
    public void Set(string key,float value)=>numbers.First(n=>n.key==key).value=value;
}
[Serializable] public sealed class BalancePart {public string id,frame,category,name,partTraitIconId="",visualId="",notes="";public BalanceNumber[] numbers;}
[Serializable] public sealed class BalanceFrame {public string id;public string[] slots;}
[Serializable] public sealed class BalanceDesign {public string id,name,chassisId,skillId="",description="",paintColorHex="";public bool isEmpty;public string[] slots;public BalancePart[] parts;}
[Serializable] public sealed class BalanceDocument
{
    public int formatVersion=6;public bool handMountDefaultsApplied;public string name="PD Balance 01",notes="",activeDesignId="DESIGN_01";public BalanceWeapon[] weapons;public BalancePart[] parts;public BalanceFrame[] frames;public BalanceDesign[] designs;
}
[Serializable] public sealed class BalanceDeveloperPreset {public string id,name;public bool populated;public BalanceDocument snapshot;}
[Serializable] public sealed class BalanceDeveloperPresetBank {public int formatVersion=1;public BalanceDeveloperPreset[] slots;}
public static class BalanceData
{
    [Serializable] sealed class ArtEntry {public string id,title,family,kind;}
    [Serializable] sealed class ArtCatalog {public ArtEntry[] models;}
    public static BalanceDocument Copy(BalanceDocument d)=>JsonUtility.FromJson<BalanceDocument>(JsonUtility.ToJson(d));
    public static bool IsWeaponTraitIconId(string id)=>id=="DAMAGE"||id=="LOCK"||id=="RADAR"||id=="AREA"||id=="RAPID"||id=="RELOAD"||id=="SHIELD";
    public static bool IsPartTraitIconId(string id)=>id=="JUMP"||IsWeaponTraitIconId(id);
    public static string DefaultPartVisualId(BalancePart part)=>part==null?"STANDARD":part.category=="BOOSTER"?"BOOSTER_TWIN":part.category=="HEAD"?"HEAD_STANDARD":part.category=="CORE"?"CORE_STANDARD":part.category=="LEFT_ARM"||part.category=="RIGHT_ARM"?"ARM_STANDARD":part.category=="LEGS"?"LEGS_STANDARD":"STANDARD";
    public static bool IsPartVisualId(string category,string id)=>category=="BOOSTER"?
        id=="BOOSTER_TWIN"||id=="BOOSTER_VECTOR"||id=="BOOSTER_CYCLONE":category=="HEAD"?
        id=="HEAD_STANDARD"||id=="HEAD_RANGEFINDER"||id=="HEAD_OMNI":category=="CORE"?
        id=="CORE_STANDARD"||id=="CORE_REACTOR"||id=="CORE_BULWARK":category=="LEFT_ARM"||category=="RIGHT_ARM"?
        id=="ARM_STANDARD"||id=="ARM_REINFORCED"||id=="ARM_PRECISION"||id=="ARM_ASSAULT"||id=="ARM_BULWARK"||id=="ARM_SCOUT"||id=="ARM_CLAW"||id=="ARM_BRUISER"||id=="ARM_VANGUARD"||id=="ARM_TACTICAL"||id=="ARM_EXOSKELETON":category=="LEGS"?
        id=="LEGS_STANDARD"||id=="LEGS_ASSAULT"||id=="LEGS_SCOUT"||id=="LEGS_RANGER"||id=="LEGS_HEAVY"||id=="LEGS_JUMPER"||id=="LEGS_REVERSE":id=="STANDARD";
    public static string NormalizePartVisualId(BalancePart part,string id)
    {
        if(string.IsNullOrWhiteSpace(id)||part?.category=="LEGS"&&id=="STANDARD")return DefaultPartVisualId(part);
        if(!IsPartVisualId(part?.category,id))throw new InvalidDataException((part?.id??"파츠")+": 지원하지 않는 파츠 모델 ID "+id);
        return id;
    }
    public static void SetPartVisualId(BalancePart part,string id)
    {
        if(part==null)throw new ArgumentNullException(nameof(part));part.visualId=NormalizePartVisualId(part,id);
    }
    public static bool IsSkillId(string id)=>Enum.GetNames(typeof(SpecialKind)).Contains(id);
    public static string DefaultSkillIdForChassis(string chassisId)
    {
        CraftSpec chassis=(CraftCatalog.Selectable??new CraftSpec[0]).FirstOrDefault(x=>x.name==chassisId);
        return chassis!=null?chassis.special.ToString():"SaberRush";
    }
    public static string DefaultPartTraitIcon(BalancePart part)
    {
        if(part==null)return "DAMAGE";
        if(part.category=="HEAD")return "RADAR";
        if(part.category=="CORE")return "SHIELD";
        if(part.category=="LEGS"||part.category=="BOOSTER")return "JUMP";
        return "DAMAGE";
    }
    public static string DefaultWeaponTraitIcon(BalanceWeapon weapon)
    {
        if(weapon==null)return "DAMAGE";
        if(weapon.kind=="shield")return "SHIELD";
        if(weapon.Get("blast")>0)return "AREA";
        if(weapon.Get("lockStrength")>0)return "LOCK";
        if(weapon.Get("interval")<=.18f)return "RAPID";
        return "DAMAGE";
    }
    public static bool HasWeaponTrait(BalanceWeapon weapon,string id)=>weapon!=null&&(weapon.traitIconId==id||(weapon.additionalTraitIconIds??Array.Empty<string>()).Contains(id));
    public static BalanceDesign CopyDesign(BalanceDesign source)=>source==null?null:JsonUtility.FromJson<BalanceDesign>(JsonUtility.ToJson(source));
    public static void PasteDesign(BalanceDesign source,BalanceDesign destination)
    {
        if(source==null||destination==null)throw new ArgumentNullException("설계 슬롯");
        BalanceDesign copy=CopyDesign(source);destination.name=copy.name;destination.chassisId=copy.chassisId;destination.skillId=copy.skillId;destination.description=copy.description??"";destination.paintColorHex=copy.paintColorHex??"";destination.isEmpty=copy.isEmpty;destination.slots=copy.slots==null?new string[0]:(string[])copy.slots.Clone();destination.parts=CopyParts(copy.parts);
    }
    public static void ClearDesign(BalanceDesign design)
    {
        if(design==null)throw new ArgumentNullException(nameof(design));design.name="";design.chassisId="";design.skillId="";design.description="";design.paintColorHex="";design.isEmpty=true;design.slots=new string[0];design.parts=new BalancePart[0];
    }
    public static void CreateDesign(BalanceDocument document,string designId,string chassisId)
    {
        if(document==null)throw new ArgumentNullException(nameof(document));BalanceDesign design=(document.designs??new BalanceDesign[0]).FirstOrDefault(x=>x!=null&&x.id==designId);
        if(design==null||!design.isEmpty)throw new InvalidDataException("새 설계를 만들 빈 슬롯을 찾을 수 없습니다.");BalanceFrame frame=(document.frames??new BalanceFrame[0]).FirstOrDefault(x=>x!=null&&x.id==chassisId);
        if(frame==null)throw new InvalidDataException("선택한 기체를 찾을 수 없습니다.");design.isEmpty=false;design.name=frame.id;design.chassisId=frame.id;design.skillId=DefaultSkillIdForChassis(frame.id);design.description="";design.paintColorHex="";design.slots=(string[])frame.slots.Clone();design.parts=CopyParts(document.parts);
    }
    public static void ResetDesign(BalanceDocument document,string designId)
    {
        if(document==null)throw new ArgumentNullException(nameof(document));
        int index=Array.FindIndex(document.designs??new BalanceDesign[0],x=>x!=null&&x.id==designId);
        if(index<0)throw new InvalidDataException("초기화할 설계 슬롯을 찾을 수 없습니다.");
        BalanceDesign defaults=Defaults().designs[index];
        BalancePart[] customDefaults=(document.parts??new BalancePart[0]).Where(p=>p!=null&&p.id!=null&&p.id.StartsWith("CUSTOM_",StringComparison.Ordinal)).Select(p=>CopyParts(new[]{p})[0]).ToArray();
        PasteDesign(defaults,document.designs[index]);document.designs[index].parts=document.designs[index].parts.Concat(customDefaults).ToArray();
    }
    public static BalanceDocument Defaults()
    {
        TextAsset text=Resources.Load<TextAsset>("PDParts/Weapons20261105/catalog_v003");if(text==null)throw new InvalidOperationException("Missing weapon catalog");
        ArtEntry[] art=JsonUtility.FromJson<ArtCatalog>(text.text).models;var weapons=new List<BalanceWeapon>();
        foreach(ArtEntry e in art)
        {
            bool gun=e.kind=="firearm";float capacity=30,damage=16.1f,interval=.18f/.65f,speed=132,radius=.24f,range=82,blast=0,partialReload=2.5f,emptyReload=4f;
            if(e.id=="Military_GL"){capacity=8;damage=55.2f;interval=1;speed=110;radius=.29f;range=92;}
            else if(e.id=="Military_RL"){capacity=4;damage=29.9f;interval=.42f/.65f;speed=100;radius=.28f;range=100;blast=5.5f;partialReload=4.5f;emptyReload=7f;}
            else if(e.id=="Future_Launcher"){capacity=12;damage=29.9f;interval=.42f/.65f;speed=100;radius=.28f;range=100;blast=2;}
            else if(e.id=="Future_Sniper"||e.id=="Military_SR"){capacity=4;damage=92;interval=1.1f/.65f;speed=180;radius=.20f;range=130;}
            else if(e.id=="Military_LMG"){capacity=60;damage=8;interval=.15f;}
            else if(e.id.Contains("LMG")||e.id.Contains("Gatling")){capacity=60;damage=10;interval=.15f;}
            else if(e.id.Contains("SG")||e.id.Contains("Shotgun")){capacity=8;damage=36;interval=.7f;}
            else if(e.id=="Military_HG"){capacity=12;damage=18;interval=.35f;}
            var n=new List<BalanceNumber>{
                new BalanceNumber("capacity","탄창",capacity,1,999,"발",gun,true),new BalanceNumber("damage","공격력",damage,0,1000,"",gun),
                new BalanceNumber("interval","발사 간격",interval,.05f,10,"초",gun),new BalanceNumber("speed","탄속도",speed,1,600,"m/s",gun),
                new BalanceNumber("radius","탄환 크기 / 판정 반경",radius,.02f,.8f,"m",gun),new BalanceNumber("range","사거리",range,1,300,"m",gun),
                new BalanceNumber("partialReload","잔탄 장전 (호환)",partialReload,.1f,30,"초",false),new BalanceNumber("emptyReload","빈 탄창 장전 (호환)",emptyReload,.1f,30,"초",false),new BalanceNumber("reloadTime","재장전 시간",Mathf.Max(partialReload,emptyReload),.1f,30,"초",gun),
                new BalanceNumber("idleReload","자동 장전 대기",2.2f,.1f,60,"초",gun),new BalanceNumber("spread","추가 탄퍼짐",0,0,20,"도",gun),new BalanceNumber("blast","피격 폭발 반경 (0=없음)",blast,0,15,"m",gun),
                new BalanceNumber("guardReduction","정면 가드 방어율",.5f,0,1,"비율",true),new BalanceNumber("weight","무기 무게",5,0,1000,"kg",gun||e.kind=="shield"),new BalanceNumber("ammoWeight","탄약 1발 무게",.05f,0,100,"kg",false),
                new BalanceNumber("recoil","발사 반동",0,0,100,"%",gun),new BalanceNumber("lockStrength","유도 지속 비율 (0=직진)",0,0,1,"",gun),new BalanceNumber("lockPenalty","락온 이동 페널티",0,0,1,"",gun),
                new BalanceNumber("energyPerShot","발사 에너지",0,0,1000,"",false),new BalanceNumber("heatPerShot","발사 발열",0,0,1000,"",false),new BalanceNumber("cooling","초당 냉각",10,0,1000,"",false),new BalanceNumber("maxHeat","과열 한계",100,1,10000,"",false)
            };
            var weapon=new BalanceWeapon{id=e.id,name=e.id.Replace('_',' '),family=e.family,kind=e.kind,icon=e.title,hands=gun,shoulders=gun&&e.id!="Military_HG"&&e.id!="Military_SMG",allowsShoulderDuplicate=true,numbers=n.ToArray()};weapon.traitIconId=DefaultWeaponTraitIcon(weapon);weapon.additionalTraitIconIds=Array.Empty<string>();
            if(e.id=="Military_RL"){weapon.traitIconId="LOCK";weapon.additionalTraitIconIds=new[]{"AREA"};weapon.blocksShoulderWeapons=true;weapon.allowsShoulderDuplicate=false;}
            weapons.Add(weapon);
        }
        BalanceWeapon shield=JsonUtility.FromJson<BalanceWeapon>(JsonUtility.ToJson(weapons.First(w=>w.id=="Future_Shield")));
        shield.id="WISP_GuardShield";shield.name="기본 방패 (WISP 오른손)";shield.kind="shield";shield.family="existing";shield.icon="GUARD";shield.hands=true;shield.shoulders=false;shield.left=false;shield.right=true;shield.notes="기존 WISP 방패와 우클릭 가드 모션. 같은 손의 총과 동시에 장착하지 않습니다.";shield.traitIconId=DefaultWeaponTraitIcon(shield);shield.additionalTraitIconIds=Array.Empty<string>();weapons.Add(shield);
        BalanceWeapon empty=JsonUtility.FromJson<BalanceWeapon>(JsonUtility.ToJson(weapons.First(w=>w.id=="Military_HG")));
        empty.id="UNARMED";empty.name="미장착 (손은 주먹)";empty.kind="unarmed";empty.family="builtin";empty.icon="FIST";empty.hands=empty.shoulders=empty.left=empty.right=true;
        empty.notes="손 슬롯은 왼쪽 클릭으로 주먹 공격, 어깨 슬롯은 장착물 없음.";empty.Set("capacity",1);empty.Set("damage",30);empty.Set("interval",.6f);empty.Set("range",2.2f);empty.Set("radius",.3f);empty.additionalTraitIconIds=Array.Empty<string>();
        foreach(BalanceNumber n in empty.numbers)n.implemented=n.key=="damage"||n.key=="interval"||n.key=="range"||n.key=="radius";weapons.Add(empty);
        var parts=new List<BalancePart>();var frames=new List<BalanceFrame>();
        foreach(CraftSpec preset in CraftCatalog.Selectable.Take(CraftCatalog.UnlockedCount))
        {
            CraftSpec f=preset.RuntimeCopy();
            // Match the existing neutral part allocation used when spawning the three prototypes.
            if(f.name!="WISP"){f.forwardSpeedFactor*=1.48f;f.reverseSpeedFactor*=1.16f;f.leftSpeedFactor*=.68f;f.rightSpeedFactor*=.68f;}
            frames.Add(new BalanceFrame{id=f.name,slots=new[]{"Military_AR","Military_GL","Military_RL","Future_Sniper"}});
            Add("HEAD","머리 / 센서",new[]{N("lockRange","락온 범위",1,200,"m"),N("radarRange","레이더 범위",1,200,"m"),N("aimSpreadDegrees","기체 탄퍼짐",0,15,"도"),N("occludedLockSeconds","가림 기억 시간",0,5,"초")});
            Add("CORE","코어 / 몸체",new[]{N("health","내구도",1,5000),N("armor","피해 감소율",0,.9f),N("boostCapacity","에너지 용량",1,2000),N("shieldRecoverySeconds","실드 완전 회복 시간",1,180,"초")});
            Add("LEFT_ARM","왼팔",new[]{N("leftArmHandling","조작 성능",.2f,3),N("leftArmStability","안정성",.2f,3)});
            Add("RIGHT_ARM","오른팔",new[]{N("rightArmHandling","조작 성능",.2f,3),N("rightArmStability","안정성",.2f,3)});
            Add("LEGS","하체 / 이동",new[]{N("speed","기본 속도",1,60,"m/s"),N("prototypeBaseSpeed","전용 이동 기준속도",1,60,"m/s"),N("forwardSpeedFactor","전진 비율",.1f,2),N("reverseSpeedFactor","후진 비율",.1f,2),N("leftSpeedFactor","좌측 비율",.1f,2),N("rightSpeedFactor","우측 비율",.1f,2),N("jumpImpulseScale","점프 충격 배율",.1f,10),N("jumpHeightScale","점프 높이 배율",.1f,10),N("fallGravityFactor","낙하 중력 배율",.2f,4),N("stepHeight","자동 턱 넘기 높이",0,.8f,"m")});
            Add("BOOSTER","부스터",new[]{N("boostSpeed","부스트 속도",1,90,"m/s"),N("boostDrain","초당 에너지 소모",.1f,200),N("verticalBoostScale","수직 추력 배율",0,3)});
            BalanceNumber N(string key,string label,float min,float max,string unit="")=>new BalanceNumber(key,label,(float)typeof(CraftSpec).GetField(key).GetValue(f),min,max,unit);
            void Add(string category,string name,BalanceNumber[] values)
            {
                var all=new List<BalanceNumber>(values){new BalanceNumber("plannedWeight","파츠 무게",5,0,1000,"kg",false)};
                var part=new BalancePart{id=f.name+"_"+category,frame=f.name,category=category,name=name,numbers=all.ToArray()};part.partTraitIconId=DefaultPartTraitIcon(part);part.visualId=DefaultPartVisualId(part);parts.Add(part);
            }
        }
        BalanceFrame[] frameData=frames.ToArray();
        BalancePart[] partData=parts.ToArray();BalanceDesign[] designs=DefaultDesigns(frameData);
        foreach(BalanceDesign design in designs)design.parts=CopyParts(partData);
        return new BalanceDocument{formatVersion=6,activeDesignId="DESIGN_01",weapons=weapons.ToArray(),parts=partData,frames=frameData,designs=designs};
    }
    public static BalancePart[] CopyParts(BalancePart[] parts)=>parts==null?null:JsonUtility.FromJson<PartArray>(JsonUtility.ToJson(new PartArray{items=parts})).items;
    [Serializable] sealed class PartArray {public BalancePart[] items;}
    public static BalanceWeapon AddCustomFirearm(BalanceDocument document,string id,string name)
    {
        if(document==null||document.weapons==null||!ValidCustomId(id)||document.weapons.Any(w=>w!=null&&w.id==id)||(document.parts??new BalancePart[0]).Any(p=>p!=null&&p.id==id))throw new InvalidDataException("사용자 화기 ID가 비어 있거나 이미 등록되어 있습니다.");
        if(document.weapons.Count(w=>w!=null&&w.id.StartsWith("CUSTOM_",StringComparison.Ordinal))>=32)throw new InvalidDataException("사용자 화기 정의는 32개까지 등록할 수 있습니다.");
        BalanceWeapon source=document.weapons.FirstOrDefault(w=>w!=null&&w.id=="Military_AR");if(source==null)throw new InvalidDataException("기본 Military_AR 정의가 없습니다.");
        string safeName=Text(name,80);if(string.IsNullOrWhiteSpace(safeName))throw new InvalidDataException("사용자 화기 이름이 비어 있습니다.");
        BalanceWeapon added=JsonUtility.FromJson<BalanceWeapon>(JsonUtility.ToJson(source));added.id=id;added.name=safeName;added.family="custom";added.kind="firearm";added.hands=true;added.shoulders=false;added.left=added.right=true;added.icon="CUSTOM";added.notes="기본 화기 수치 구조에서 시작한 사용자 정의 화기입니다.";
        document.weapons=document.weapons.Concat(new[]{added}).ToArray();return added;
    }
    public static BalancePart AddCustomPart(BalanceDocument document,string designId,string id,string name)
    {
        if(document==null||document.parts==null||!ValidCustomId(id)||document.parts.Any(p=>p!=null&&p.id==id)||(document.weapons??new BalanceWeapon[0]).Any(w=>w!=null&&w.id==id))throw new InvalidDataException("사용자 파츠 ID가 비어 있거나 이미 등록되어 있습니다.");
        if(document.parts.Count(p=>p!=null&&p.id.StartsWith("CUSTOM_",StringComparison.Ordinal))>=32)throw new InvalidDataException("사용자 파츠 정의는 32개까지 등록할 수 있습니다.");
        BalanceDesign selected=(document.designs??new BalanceDesign[0]).FirstOrDefault(d=>d!=null&&d.id==designId);if(selected==null)throw new InvalidDataException("대상 설계를 찾을 수 없습니다.");
        BalancePart source=document.parts.FirstOrDefault(p=>p!=null&&p.frame==selected.chassisId&&p.category=="CORE"&&!p.id.StartsWith("CUSTOM_",StringComparison.Ordinal));if(source==null)throw new InvalidDataException("선택 기체에 기본 CORE 파츠가 없습니다.");
        string safeName=Text(name,80);if(string.IsNullOrWhiteSpace(safeName))throw new InvalidDataException("사용자 파츠 이름이 비어 있습니다.");
        if(document.designs==null||document.designs.Length!=8||document.designs.Any(d=>d==null||d.parts!=null&&d.parts.Any(p=>p!=null&&p.id==id)))throw new InvalidDataException("설계 슬롯 목록이 잘못되었거나 파츠 ID가 이미 사용 중입니다.");
        BalancePart added=CopyParts(new[]{source})[0];added.id=id;added.name=safeName;added.notes="기본 코어 수치 구조를 복제한 사용자 파츠입니다.";document.parts=document.parts.Concat(new[]{CopyParts(new[]{added})[0]}).ToArray();
        foreach(BalanceDesign design in document.designs)
        {
            if(design.parts==null)design.parts=new BalancePart[0];if(design.parts.Any(p=>p!=null&&p.id==id))throw new InvalidDataException("설계 슬롯에 사용자 파츠가 이미 있습니다: "+design.id);
            design.parts=design.parts.Concat(new[]{CopyParts(new[]{added})[0]}).ToArray();
        }
        return added;
    }
    static BalanceDesign[] DefaultDesigns(BalanceFrame[] frames)
    {
        var designs=new BalanceDesign[8];
        for(int i=0;i<designs.Length;i++)
        {
            BalanceFrame source=frames[i<frames.Length?i:0];
            string description=i==0?"고속 기동과 근접 교전을 위한 경량 샘플 설계.":i==1?"중장갑과 묵직한 화력을 갖춘 전차형 샘플 설계.":i==2?"네 다리와 센서 운용을 위한 사족 샘플 설계.":"";
            designs[i]=new BalanceDesign{id="DESIGN_"+(i+1).ToString("00"),name=source.id+(i<frames.Length?"":" / "+(i+1)),chassisId=source.id,skillId=DefaultSkillIdForChassis(source.id),description=description,slots=(string[])source.slots.Clone()};
        }
        return designs;
    }
    public static BalanceDocument ValidateAndNormalize(BalanceDocument raw)
    {
        if(raw==null||raw.formatVersion<1||raw.formatVersion>6)throw new InvalidDataException("지원하지 않는 설계 파일 버전입니다.");
        int sourceVersion=raw.formatVersion;
        BalanceDocument result=Defaults();
        if(raw.weapons==null||raw.parts==null||raw.frames==null||raw.weapons.Length<result.weapons.Length||raw.weapons.Length>result.weapons.Length+32||raw.parts.Length<result.parts.Length||raw.parts.Length>result.parts.Length+32||raw.frames.Length!=result.frames.Length)throw new InvalidDataException("무기·파츠·기체 목록이 누락되었거나 확장 한도를 벗어났습니다.");
        result.name=Text(raw.name,80);result.notes=Text(raw.notes,2000);result.handMountDefaultsApplied=raw.handMountDefaultsApplied;
        var seen=new HashSet<string>();
        var weaponRegistry=new List<BalanceWeapon>(result.weapons);
        foreach(BalanceWeapon w in raw.weapons)
        {
            if(w==null||w.id==null||!seen.Add(w.id))throw new InvalidDataException("무기 ID가 중복되거나 비어 있습니다.");
            BalanceWeapon dest=weaponRegistry.FirstOrDefault(x=>x.id==w.id);
            if(dest==null)
            {
                if(!ValidCustomId(w.id)||w.kind!="firearm"||raw.parts.Any(p=>p!=null&&p.id==w.id))throw new InvalidDataException("새 무기는 고유한 CUSTOM_ ID와 firearm 종류를 사용해야 합니다: "+w.id);
                dest=JsonUtility.FromJson<BalanceWeapon>(JsonUtility.ToJson(weaponRegistry.First(x=>x.id=="Military_AR")));dest.id=w.id;dest.family="custom";dest.kind="firearm";weaponRegistry.Add(dest);
            }
            dest.name=Text(w.name,80);dest.notes=Text(w.notes,2000);dest.icon=Text(w.icon,40);dest.hands=w.hands;dest.shoulders=w.shoulders;dest.left=w.left;dest.right=w.right;
            dest.allowsShoulderDuplicate=sourceVersion<5?w.id!="Military_RL":w.allowsShoulderDuplicate;
            dest.blocksShoulderWeapons=sourceVersion<5?w.id=="Military_RL":w.blocksShoulderWeapons;Numbers(dest.numbers,w.numbers,w.id);
            string traitIcon=w.traitIconId=="JUMP"?"DAMAGE":string.IsNullOrWhiteSpace(w.traitIconId)?DefaultWeaponTraitIcon(dest):w.traitIconId;
            if(!IsWeaponTraitIconId(traitIcon))throw new InvalidDataException(w.id+": 알 수 없는 특성 아이콘 ID "+traitIcon);
            var additionalTraits=new List<string>();foreach(string id in w.additionalTraitIconIds??Array.Empty<string>())
            {
                if(!IsWeaponTraitIconId(id))throw new InvalidDataException(w.id+": 알 수 없는 추가 특성 아이콘 ID "+id);
                if(id=="SHIELD"||traitIcon=="SHIELD")throw new InvalidDataException(w.id+": 방패 특성은 다른 전투 특성과 중복할 수 없습니다.");
                if(id!=traitIcon&&!additionalTraits.Contains(id))additionalTraits.Add(id);
            }
            dest.traitIconId=traitIcon;dest.additionalTraitIconIds=additionalTraits.ToArray();
        }
        foreach(BalanceWeapon required in result.weapons)if(!seen.Contains(required.id))throw new InvalidDataException("기본 무기 정의를 제거할 수 없습니다: "+required.id);
        result.weapons=weaponRegistry.ToArray();
        var partRegistry=new List<BalancePart>(result.parts);seen.Clear();foreach(BalancePart p in raw.parts)
        {
            if(p==null||p.id==null||!seen.Add(p.id))throw new InvalidDataException("파츠 ID가 중복되거나 비어 있습니다.");BalancePart dest=partRegistry.FirstOrDefault(x=>x.id==p.id);
            if(dest==null)
            {
                if(!ValidCustomId(p.id)||string.IsNullOrWhiteSpace(p.frame)||weaponRegistry.Any(w=>w.id==p.id))throw new InvalidDataException("새 파츠는 무기와 중복되지 않는 CUSTOM_ ID와 기존 기체를 지정해야 합니다: "+p.id);
                BalancePart template=partRegistry.FirstOrDefault(x=>x.frame==p.frame&&x.category==p.category);if(template==null)throw new InvalidDataException("새 파츠는 기존 기체·파츠 분류를 사용해야 합니다: "+p.id);
                dest=CopyParts(new[]{template})[0];dest.id=p.id;partRegistry.Add(dest);
            }
            if(p.frame!=dest.frame||p.category!=dest.category)throw new InvalidDataException("파츠의 기체/분류를 바꿀 수 없습니다: "+p.id);
            dest.name=Text(p.name,80);dest.notes=Text(p.notes,2000);dest.visualId=NormalizePartVisualId(dest,p.visualId);string partTrait=string.IsNullOrWhiteSpace(p.partTraitIconId)?DefaultPartTraitIcon(dest):p.partTraitIconId;if(!IsPartTraitIconId(partTrait))throw new InvalidDataException(p.id+": 알 수 없는 파츠 특성 아이콘 ID "+partTrait);dest.partTraitIconId=partTrait;Numbers(dest.numbers,p.numbers,p.id);
        }
        foreach(BalancePart required in result.parts)if(!seen.Contains(required.id))throw new InvalidDataException("기본 파츠 정의를 제거할 수 없습니다: "+required.id);
        result.parts=partRegistry.ToArray();foreach(BalanceDesign design in result.designs)design.parts=CopyParts(result.parts);
        if(sourceVersion<4)foreach(BalanceDesign design in result.designs)design.parts=CopyParts(result.parts);
        seen.Clear();foreach(BalanceFrame f in raw.frames)
        {
            if(f==null||f.id==null||!seen.Add(f.id)||f.slots==null||f.slots.Length!=4)throw new InvalidDataException("기체별 무장 4개가 필요합니다.");
            BalanceFrame dest=result.frames.FirstOrDefault(x=>x.id==f.id);if(dest==null)throw new InvalidDataException("알 수 없는 기체 ID");
            for(int i=0;i<4;i++)
            {
                BalanceWeapon w=result.weapons.FirstOrDefault(x=>x.id==f.slots[i]);
                if(w==null||!CanEquip(w,i,f.id))throw new InvalidDataException(f.id+"의 "+(i+1)+"번 슬롯에 장착할 수 없는 무기입니다.");dest.slots[i]=w.id;
            }
            ValidateShoulderPair(dest.slots,result.weapons,f.id);
        }
        if(sourceVersion==1)
        {
            // V1 stored one loadout per chassis. Preserve those exactly in the first three stable slots.
            for(int i=0;i<result.frames.Length;i++)
            {
                BalanceFrame frame=result.frames[i];BalanceDesign design=result.designs[i];
                design.name=frame.id;design.chassisId=frame.id;design.slots=(string[])frame.slots.Clone();
            }
        }
        else
        {
            if(raw.designs==null||raw.designs.Length!=8)throw new InvalidDataException("설계 파일에는 8개의 저장 슬롯이 필요합니다.");
            seen.Clear();foreach(BalanceDesign design in raw.designs)
            {
                if(design==null||string.IsNullOrWhiteSpace(design.id)||!seen.Add(design.id))throw new InvalidDataException("설계 슬롯 ID가 중복되거나 비어 있습니다.");
                BalanceDesign dest=result.designs.FirstOrDefault(x=>x.id==design.id);if(dest==null)throw new InvalidDataException("알 수 없는 설계 슬롯 ID: "+design.id);
                if(design.isEmpty){dest.name="";dest.chassisId="";dest.skillId="";dest.description="";dest.paintColorHex="";dest.isEmpty=true;dest.slots=new string[0];dest.parts=new BalancePart[0];continue;}
                BalanceFrame frame=result.frames.FirstOrDefault(x=>x.id==design.chassisId);if(frame==null)throw new InvalidDataException(design.id+": 알 수 없는 기체 ID");
                if(design.slots==null||design.slots.Length!=4)throw new InvalidDataException(design.id+": 무장 4개가 필요합니다.");
                dest.isEmpty=false;dest.name=Text(design.name,80);dest.chassisId=design.chassisId;dest.description=Text(design.description,1000);string paintHex=design.paintColorHex??"";if(!string.IsNullOrEmpty(paintHex)&&!ValidColorHex(paintHex))throw new InvalidDataException(design.id+": 외장 색상은 6자리 RGB HEX여야 합니다.");dest.paintColorHex=paintHex.ToUpperInvariant();string skill=string.IsNullOrWhiteSpace(design.skillId)?DefaultSkillIdForChassis(frame.id):design.skillId;if(!IsSkillId(skill))throw new InvalidDataException(design.id+": 알 수 없는 스킬 ID "+skill);dest.skillId=skill;
                for(int i=0;i<4;i++)
                {
                    BalanceWeapon w=result.weapons.FirstOrDefault(x=>x.id==design.slots[i]);
                    if(w==null||!CanEquip(w,i,frame.id))throw new InvalidDataException(design.id+"의 "+(i+1)+"번 슬롯에 장착할 수 없는 무기입니다.");dest.slots[i]=w.id;
                }
                ValidateShoulderPair(dest.slots,result.weapons,design.id);
                if(sourceVersion>=4)CopyValidatedParts(dest,design.parts,result.parts,new HashSet<string>());
            }
        }
        if(sourceVersion>=3)
        {
            if(string.IsNullOrWhiteSpace(raw.activeDesignId)||!result.designs.Any(x=>x.id==raw.activeDesignId))throw new InvalidDataException("현재 선택 설계 ID를 찾을 수 없습니다.");
            result.activeDesignId=raw.activeDesignId;
        }
        else result.activeDesignId="DESIGN_01";
        result.formatVersion=6;
        return result;
    }
    static void CopyValidatedParts(BalanceDesign design,BalancePart[] input,BalancePart[] expected,HashSet<string> seen)
    {
        int required=expected.Count(p=>!p.id.StartsWith("CUSTOM_",StringComparison.Ordinal));if(input==null||input.Length<required||input.Length>expected.Length)throw new InvalidDataException(design.id+": 파츠 수치 목록이 누락되었거나 확장 한도를 벗어났습니다.");
        seen.Clear();foreach(BalancePart part in input)
        {
            if(part==null||string.IsNullOrWhiteSpace(part.id)||!seen.Add(part.id))throw new InvalidDataException(design.id+": 파츠 ID 중복");
            BalancePart dest=design.parts.FirstOrDefault(x=>x.id==part.id);if(dest==null)throw new InvalidDataException(design.id+": 알 수 없는 파츠 ID "+part.id);
            if(part.frame!=dest.frame||part.category!=dest.category)throw new InvalidDataException(design.id+": 파츠의 기체/종류 ID를 바꿀 수 없습니다.");
            dest.notes=Text(part.notes,2000);dest.visualId=NormalizePartVisualId(dest,part.visualId);string partTrait=string.IsNullOrWhiteSpace(part.partTraitIconId)?DefaultPartTraitIcon(dest):part.partTraitIconId;if(!IsPartTraitIconId(partTrait))throw new InvalidDataException(design.id+"/"+part.id+": 알 수 없는 특성 아이콘 ID "+partTrait);dest.partTraitIconId=partTrait;Numbers(dest.numbers,part.numbers,design.id+"/"+part.id);
        }
        foreach(BalancePart part in expected)if(!part.id.StartsWith("CUSTOM_",StringComparison.Ordinal)&&!seen.Contains(part.id))throw new InvalidDataException(design.id+": 기본 파츠가 누락되었습니다: "+part.id);
    }
    static bool ValidCustomId(string id)
    {
        if(string.IsNullOrEmpty(id)||id.Length>64||!id.StartsWith("CUSTOM_",StringComparison.Ordinal)||id.EndsWith("_",StringComparison.Ordinal))return false;
        for(int i=0;i<id.Length;i++){char c=id[i];if(!(c>='A'&&c<='Z'||c>='0'&&c<='9'||c=='_'))return false;}
        return id.Length>7;
    }
    static bool ValidColorHex(string value)
    {
        if(value.Length!=6)return false;for(int i=0;i<value.Length;i++){char c=value[i];if(!(c>='0'&&c<='9'||c>='A'&&c<='F'||c>='a'&&c<='f'))return false;}return true;
    }
    static string Text(string value,int max){if(value==null)return "";if(value.Length>max)throw new InvalidDataException("이름 또는 메모가 너무 깁니다.");return value;}
    static void Numbers(BalanceNumber[] expected,BalanceNumber[] input,string owner)
    {
        if(input!=null)
        {
            var migrated=input.ToList();
            AddLegacyDefault("guardReduction");
            AddLegacyDefault("shieldRecoverySeconds");
            if(expected.Any(n=>n.key=="reloadTime")&&!migrated.Any(n=>n!=null&&n.key=="reloadTime"))
            {
                BalanceNumber reload=expected.First(n=>n.key=="reloadTime");
                BalanceNumber partial=migrated.FirstOrDefault(n=>n!=null&&n.key=="partialReload");
                BalanceNumber empty=migrated.FirstOrDefault(n=>n!=null&&n.key=="emptyReload");
                reload=JsonUtility.FromJson<BalanceNumber>(JsonUtility.ToJson(reload));
                reload.value=Mathf.Max(partial?.value??reload.value,empty?.value??reload.value);
                migrated.Add(reload);
            }
            input=migrated.ToArray();

            void AddLegacyDefault(string key)
            {
                BalanceNumber fallback=expected.FirstOrDefault(n=>n.key==key);
                if(fallback!=null&&!migrated.Any(n=>n!=null&&n.key==key))migrated.Add(JsonUtility.FromJson<BalanceNumber>(JsonUtility.ToJson(fallback)));
            }
        }
        if(input==null||input.Length!=expected.Length)throw new InvalidDataException(owner+": 수치 항목이 누락되었습니다.");var seen=new HashSet<string>();
        foreach(BalanceNumber n in input)
        {
            if(n==null||n.key==null||!seen.Add(n.key))throw new InvalidDataException(owner+": 수치 키 중복");BalanceNumber dest=expected.FirstOrDefault(x=>x.key==n.key);
            if(dest==null)throw new InvalidDataException(owner+": 알 수 없는 수치 키 "+n.key);
            float safeMin=dest.min,safeMax=dest.max;
            if(float.IsNaN(n.value)||float.IsInfinity(n.value)||float.IsNaN(n.min)||float.IsInfinity(n.min)||float.IsNaN(n.max)||float.IsInfinity(n.max)||n.min<safeMin||n.max>safeMax||n.min>n.max||n.value<n.min||n.value>n.max||(dest.integer&&n.value!=Mathf.Round(n.value)))throw new InvalidDataException(owner+" / "+n.key+": 허용 범위를 벗어난 수치입니다.");
            dest.safeMin=safeMin;dest.safeMax=safeMax;dest.min=n.min;dest.max=n.max;dest.value=n.value;
        }
    }
    public static bool CanEquip(BalanceWeapon w,int slot,string frame=null)=>(w.kind=="firearm"||w.kind=="unarmed"||w.kind=="shield"||HasWeaponTrait(w,"SHIELD"))&&((HasWeaponTrait(w,"SHIELD")||w.kind=="shield")?slot<2&&w.hands:(slot<2?w.hands:w.shoulders))&&(slot%2==0?w.left:w.right);
    public static bool CanDuplicateShoulderWeapon(BalanceWeapon w)=>w!=null&&w.allowsShoulderDuplicate;
    static void ValidateShoulderPair(string[] slots,BalanceWeapon[] weapons,string owner)
    {
        if(slots==null||slots.Length<4||slots[2]!=slots[3])return;
        BalanceWeapon duplicate=weapons.FirstOrDefault(w=>w.id==slots[2]);
        if(duplicate!=null&&duplicate.shoulders&&!CanDuplicateShoulderWeapon(duplicate))throw new InvalidDataException(owner+": "+duplicate.name+"은 양쪽 어깨에 중복 장착할 수 없습니다.");
    }
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
public static class BalanceDeveloperPresetFiles
{
    public const int SlotCount=10;
    public static BalanceDeveloperPresetBank Empty()
    {
        var slots=new BalanceDeveloperPreset[SlotCount];
        for(int i=0;i<slots.Length;i++)slots[i]=new BalanceDeveloperPreset{id="DEV_PRESET_"+(i+1).ToString("00"),name="실험 "+(i+1).ToString("00")};
        return new BalanceDeveloperPresetBank{formatVersion=1,slots=slots};
    }
    public static BalanceDeveloperPresetBank ValidateAndNormalize(BalanceDeveloperPresetBank raw)
    {
        if(raw==null||raw.formatVersion!=1)throw new InvalidDataException("개발 프리셋 파일 버전을 확인할 수 없습니다.");
        if(raw.slots!=null&&raw.slots.Length>SlotCount)throw new InvalidDataException("개발 프리셋은 10개까지만 저장할 수 있습니다.");
        BalanceDeveloperPresetBank result=Empty();
        if(raw.slots==null)return result;
        for(int i=0;i<raw.slots.Length;i++)
        {
            BalanceDeveloperPreset source=raw.slots[i];if(source==null)continue;
            string expected="DEV_PRESET_"+(i+1).ToString("00");
            if(source.id!=expected)throw new InvalidDataException("개발 프리셋 슬롯 ID가 순서와 일치하지 않습니다: "+expected);
            if(string.IsNullOrWhiteSpace(source.name)||source.name.Length>64)throw new InvalidDataException(expected+" 이름은 1~64자여야 합니다.");
            BalanceDeveloperPreset destination=result.slots[i];destination.name=source.name.Trim();
            bool hasSnapshot=source.populated||source.snapshot!=null&&source.snapshot.weapons!=null&&source.snapshot.weapons.Length>0;
            destination.populated=hasSnapshot;
            if(hasSnapshot)destination.snapshot=BalanceData.ValidateAndNormalize(source.snapshot);
        }
        return result;
    }
    public static BalanceDeveloperPresetBank Load(string path)
    {
        if(!File.Exists(path))return Empty();
        if(new FileInfo(path).Length>50*1024*1024)throw new InvalidDataException("개발 프리셋 파일 크기 제한은 50MB입니다.");
        return ValidateAndNormalize(JsonUtility.FromJson<BalanceDeveloperPresetBank>(File.ReadAllText(path)));
    }
    public static BalanceDeveloperPresetBank SaveSlot(string path,BalanceDeveloperPresetBank bank,int index,string name,BalanceDocument draft)
    {
        if(index<0||index>=SlotCount)throw new ArgumentOutOfRangeException(nameof(index));
        if(string.IsNullOrWhiteSpace(name)||name.Trim().Length>64)throw new InvalidDataException("프리셋 이름은 1~64자여야 합니다.");
        BalanceDeveloperPresetBank result=ValidateAndNormalize(bank??Empty());
        result.slots[index].name=name.Trim();result.slots[index].populated=true;result.slots[index].snapshot=BalanceData.ValidateAndNormalize(draft);
        string temp=path+".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(temp,JsonUtility.ToJson(result,true),new System.Text.UTF8Encoding(false));
            if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
        }
        finally{if(File.Exists(temp))File.Delete(temp);}
        return result;
    }
}
