using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
public sealed partial class VoxArena
{
    bool balanceEditorOpen,balanceDirty,balancePreviewUnavailable;BalanceDocument balanceDraft,activeBalance;BalanceDesign balanceDesignClipboard;int balanceTab,balanceWeaponIndex,balancePartIndex,balanceFrameIndex,balanceMountIndex;float balancePreviewYaw;bool balancePreviewDragging;Vector2 balanceListScroll,balanceValueScroll;string balanceFileName="PD-Balance",balanceMessage="",balanceDirectoryOverride;
    Texture2D balancePixel;Font balanceFont;readonly Dictionary<string,Texture2D> balanceWeaponIcons=new Dictionary<string,Texture2D>();bool balanceIconsRequested,balanceIconsReady;readonly Dictionary<string,string> balanceNumberText=new Dictionary<string,string>();readonly HashSet<string> balanceInvalidNumbers=new HashSet<string>();
    string BalanceDirectory
    {
        get
        {
            if(balanceDirectoryOverride!=null)return balanceDirectoryOverride;
            string project=Path.GetFullPath(Path.Combine(Application.dataPath,Application.isEditor?"..":"../../.."));
            return Directory.Exists(Path.Combine(project,"Assets"))?Path.Combine(project,"Data","Balance"):Path.Combine(Application.persistentDataPath,"BalanceProfiles");
        }
    }
    string BalancePath()
    {
        string name=balanceFileName.Trim();if(name.Length==0||name.Length>64||name.IndexOfAny(Path.GetInvalidFileNameChars())>=0||name=="."||name==".."||name.EndsWith("."))throw new InvalidDataException("올바른 파일 이름을 입력하세요.");
        return Path.Combine(BalanceDirectory,name+".json");
    }
    void OpenBalanceEditor()
    {
        if(balanceDraft==null)
        {
            balanceDraft=BalanceData.Defaults();
            try{string path=BalancePath();if(File.Exists(path))balanceDraft=BalanceFiles.Load(path);else BalanceFiles.Save(path,balanceDraft);balanceMessage="파일을 불러왔습니다. 전투에 반영하려면 [적용]을 누르세요.";}catch(Exception e){balanceMessage=e.Message;}
        }
        balanceEditorOpen=true;showTuning=weaponGalleryOpen=false;SelectCustomizationDesign(Array.FindIndex(balanceDraft.designs,x=>x.id==balanceDraft.activeDesignId),false);BalanceSelectPreview();if(!balanceIconsRequested){balanceIconsRequested=true;StartCoroutine(CacheBalanceWeaponIcons());}
    }
    void BalanceSelectPreview()
    {
        if(balanceDraft==null)return;
        if(balanceTab==0){BalanceMountPreview();return;}
        if(balanceTab!=1||balanceWeaponIndex<0||balanceWeaponIndex>=balanceDraft.weapons.Length)return;
        BalanceWeaponPreview(balanceDraft.weapons[balanceWeaponIndex]);
    }
    void BalanceSave()
    {
        if(balanceInvalidNumbers.Count>0)throw new InvalidDataException("빨간색 수치 입력을 먼저 수정해 주세요.");BalanceFiles.Save(BalancePath(),balanceDraft);balanceDirty=false;balanceMessage="저장 완료: "+Path.GetFileName(BalancePath())+" (이전 파일은 .bak으로 보관)";
    }
    void BalanceLoad()
    {
        balanceDraft=BalanceFiles.Load(BalancePath());balanceNumberText.Clear();balanceInvalidNumbers.Clear();balanceDirty=false;balanceMessage="불러오기 완료. 아직 전투에는 반영하지 않았습니다.";SelectCustomizationDesign(Array.FindIndex(balanceDraft.designs,x=>x.id==balanceDraft.activeDesignId),false);BalanceSelectPreview();
    }
    void BalanceApply()
    {
        if(!inMenu)throw new InvalidOperationException("기체 선택 화면에서만 적용할 수 있습니다.");
        if(balanceInvalidNumbers.Count>0)throw new InvalidDataException("빨간색 수치 입력을 먼저 수정해 주세요.");
        BalanceDocument checkedDraft=BalanceData.ValidateAndNormalize(balanceDraft);activeBalance=BalanceData.Copy(checkedDraft);
        ApplySelectableCharacter(player,selectedCraft);ApplySelectableCharacter(enemy,ResolveRival());ResetRound();BuildPortraits();
        balanceMessage="적용 완료: 선택한 설계를 플레이어 기체에 반영했습니다. 저장하지 않은 변경은 종료 시 사라집니다.";
    }
    void ApplyBalanceParts(Fighter f)
    {
        if(activeBalance==null||f.selectableIndex<0)return;
        BalancePart[] parts=activeBalance.parts;
        if(f==player){BalanceDesign selected=Array.Find(activeBalance.designs,x=>x.id==activeBalance.activeDesignId&&x.chassisId==f.spec.name);if(selected!=null)parts=selected.parts;}
        foreach(BalancePart part in parts)if(part.frame==f.spec.name)foreach(BalanceNumber n in part.numbers)if(n.implemented)
            typeof(CraftSpec).GetField(n.key).SetValue(f.spec,n.value);
    }
    WeaponProfile[] BalanceWeaponProfiles(Fighter f)
    {
        WeaponProfile[] profiles=NewWeaponProfiles();f.weaponArtIds=(string[])EquippedArtIds.Clone();if(activeBalance==null||f.selectableIndex<0)return profiles;
        BalanceFrame frame=Array.Find(activeBalance.frames,x=>x.id==f.spec.name);if(frame==null)return profiles;
        BalanceDesign design=Array.Find(activeBalance.designs,x=>x.id==activeBalance.activeDesignId&&x.chassisId==f.spec.name);
        string[] slots=design!=null?design.slots:frame.slots;
        f.weaponArtIds=(string[])slots.Clone();
        for(int i=0;i<4;i++)
        {
            BalanceWeapon w=Array.Find(activeBalance.weapons,x=>x.id==slots[i]);WeaponProfile p=new WeaponProfile(w.name,Loadout[i].mount,(int)w.Get("capacity"),w.Get("damage"),w.Get("interval"),w.Get("speed"),w.Get("radius"),w.Get("range"));
            p.isShield=w.kind=="shield";p.isUnarmed=w.kind=="unarmed";p.partialReload=w.Get("partialReload");p.emptyReload=w.Get("emptyReload");p.idleReload=w.Get("idleReload");p.spread=w.Get("spread");p.blast=w.Get("blast");profiles[i]=p;
        }
        return profiles;
    }
    bool HasEquippedGuard(Fighter f)=>f!=null&&f.spec.name=="WISP"&&f.weaponProfiles!=null&&f.weaponProfiles[1].isShield;
    void EquipGuardForReview(Fighter f){f.weaponProfiles[1].isShield=true;if(f.guardShield!=null)f.guardShield.gameObject.SetActive(true);if(f.weaponVisuals!=null)f.weaponVisuals[1].gameObject.SetActive(false);}
    void BalanceAction(Action action){try{action();}catch(Exception e){balanceMessage="오류: "+e.Message;Debug.LogWarning("BALANCE_EDITOR "+e.Message);}}
    void DrawBalanceEditor()
    {
        if(balanceFont==null)balanceFont=Font.CreateDynamicFontFromOSFont(new[]{"Malgun Gothic","맑은 고딕","Arial"},17);
        Font previous=GUI.skin.font;GUI.skin.font=balanceFont;
        try
        {
            Panel(new Rect(0,0,1280,720),new Color(.018f,.032f,.052f,.99f));
            Panel(new Rect(0,0,1280,7),new Color(1f,.38f,.08f));
            Text(28,17,700,"VOXEL-ON  /  HANGAR CUSTOMIZATION",new GUIStyle(GUI.skin.label){fontSize=27,fontStyle=FontStyle.Bold});
            Text(30,55,720,"FRAME  /  WEAPON & PARTS WORKSHOP",small);
            if(GUI.Button(new Rect(1144,20,112,36),"닫기 / ESC")){balanceEditorOpen=false;return;}
            string[] tabs={"장착","무기 옵션","파츠 수치","파일 저장"};
            for(int i=0;i<tabs.Length;i++)
            {
                GUI.backgroundColor=balanceTab==i?new Color(1f,.42f,.10f):new Color(.10f,.15f,.21f);
                if(GUI.Button(new Rect(365+i*142,58,134,34),tabs[i])){balanceTab=i;balanceListScroll=balanceValueScroll=Vector2.zero;BalanceSelectPreview();}
            }
            GUI.backgroundColor=Color.white;
            Panel(new Rect(20,104,328,550),new Color(.035f,.055f,.080f));
            Panel(new Rect(20,104,4,550),new Color(1f,.40f,.09f));
            string frameName=balanceFrameIndex>=0&&balanceFrameIndex<balanceDraft.frames.Length?balanceDraft.frames[balanceFrameIndex].id:"WISP";
            int designIndex=Array.FindIndex(balanceDraft.designs,x=>x.id==balanceDraft.activeDesignId);if(designIndex<0)designIndex=0;
            Text(40,112,286,(balanceFrameIndex+1).ToString("00")+" / "+frameName,label);
            Rect previewRect=new Rect(36,145,296,390);
            if(framePortraits!=null&&balanceFrameIndex>=0&&balanceFrameIndex<framePortraits.Length&&framePortraits[balanceFrameIndex]!=null)
                GUI.DrawTexture(previewRect,framePortraits[balanceFrameIndex],ScaleMode.ScaleToFit,true);
            Text(42,541,270,"마우스로 드래그해 회전",small);
            if(GUI.Button(new Rect(42,575,128,28),"< 45°")){balancePreviewYaw-=45;portraitYaw=balancePreviewYaw;BuildPortraits();}
            if(GUI.Button(new Rect(185,575,128,28),"45° >")){balancePreviewYaw+=45;portraitYaw=balancePreviewYaw;BuildPortraits();}
            Event ev=Event.current;
            if(ev.type==EventType.MouseDown&&previewRect.Contains(ev.mousePosition)){balancePreviewDragging=true;ev.Use();}
            else if(ev.type==EventType.MouseDrag&&balancePreviewDragging){balancePreviewYaw+=ev.delta.x*.65f;ev.Use();}
            else if(ev.type==EventType.MouseUp&&balancePreviewDragging){balancePreviewDragging=false;portraitYaw=balancePreviewYaw;BuildPortraits();ev.Use();}
            GUIStyle designStyle=new GUIStyle(GUI.skin.button){fontSize=12,padding=new RectOffset(1,1,1,1)};
            for(int i=0;i<balanceDraft.designs.Length;i++)
            {
                BalanceDesign item=balanceDraft.designs[i];bool active=i==designIndex;GUI.backgroundColor=active?new Color(.10f,.57f,.66f):new Color(.09f,.13f,.18f);
                int row=i/4,col=i%4;string shortChassis=item.chassisId=="TARANTULA"?"TAR":item.chassisId=="BASTION"?"BAS":"WIS";
                if(GUI.Button(new Rect(38+col*73,602+row*24,69,21),(i+1).ToString("00")+" "+shortChassis,designStyle))SelectCustomizationDesign(i);
            }
            GUI.backgroundColor=Color.white;
            Panel(new Rect(360,106,896,488),new Color(.035f,.055f,.080f));
            GUI.Label(new Rect(383,101,160,28),"설정 이름");BalanceText(ref balanceDraft.name,new Rect(476,99,245,28),80);
            GUI.Label(new Rect(760,101,475,28),"양손 · 양어깨 장착 / 개별 수치 조정",new GUIStyle(GUI.skin.label){fontSize=14});
            if(balanceTab==0)
            {
                string[] mountNames={"LEFT HAND","RIGHT HAND","LEFT SHOULDER","RIGHT SHOULDER"};
                for(int i=0;i<4;i++)
                {
                    GUI.backgroundColor=balanceMountIndex==i?new Color(.10f,.57f,.66f):new Color(.10f,.15f,.21f);
                    if(GUI.Button(new Rect(378+i*214,134,204,38),mountNames[i])){balanceMountIndex=i;balanceListScroll=Vector2.zero;BalanceMountPreview();}
                }
                GUI.backgroundColor=Color.white;
            }
            Matrix4x4 oldMatrix=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(new Vector3(454,120,0),Quaternion.identity,new Vector3(.70f,.70f,1f))*oldMatrix;
            if(balanceTab==0)DrawBalanceLoadout();else if(balanceTab==1)DrawBalanceWeapons();else if(balanceTab==2)DrawBalanceParts();else DrawBalanceFiles();
            GUI.matrix=oldMatrix;
            Text(372,602,875,balanceMessage,new GUIStyle(small){wordWrap=true});
            Text(372,630,875,"파일: "+BalanceDirectory+Path.DirectorySeparatorChar+balanceFileName+".json",small);
            if(GUI.Button(new Rect(372,666,150,34),"파일 저장"))BalanceAction(BalanceSave);
            if(GUI.Button(new Rect(535,666,150,34),"파일 불러오기"))BalanceAction(()=>{if(balanceDirty){balanceMessage="수정 중입니다. 파일 탭에서 [현재 수정 버리고 불러오기]를 눌러주세요.";balanceTab=3;}else BalanceLoad();});
            if(GUI.Button(new Rect(698,666,150,34),"전투에 적용"))BalanceAction(BalanceApply);
            Text(870,670,370,"주황 수치 중 일부는 저장 전용입니다.",small);
        }
        finally{GUI.skin.font=previous;GUI.backgroundColor=Color.white;}
    }
    BalanceDesign EditingDesign()=>balanceDraft!=null?Array.Find(balanceDraft.designs,x=>x.id==balanceDraft.activeDesignId):null;
    void SelectCustomizationDesign(int index,bool markDirty=true)
    {
        if(balanceDraft==null||index<0||index>=balanceDraft.designs.Length)return;
        BalanceDesign design=balanceDraft.designs[index];balanceDraft.activeDesignId=design.id;
        balanceFrameIndex=Array.FindIndex(balanceDraft.frames,x=>x.id==design.chassisId);if(balanceFrameIndex<0)return;
        int craft=Array.FindIndex(CraftCatalog.Selectable,x=>x.name==design.chassisId);
        if(craft<0)return;
        selectedCraft=craft;balancePreviewYaw=portraitYaw=0;if(markDirty)balanceDirty=true;
        ApplySelectableCharacter(player,selectedCraft);BuildPortraits();BalanceSelectPreview();
    }
    void SelectCustomizationFrame(int index)
    {
        if(index<0||index>=balanceDraft.frames.Length)return;BalanceDesign design=EditingDesign();if(design==null)return;
        BalanceFrame chassis=balanceDraft.frames[index];design.chassisId=chassis.id;
        for(int i=0;i<design.slots.Length;i++)
        {
            BalanceWeapon weapon=Array.Find(balanceDraft.weapons,x=>x.id==design.slots[i]);
            if(weapon==null||!BalanceData.CanEquip(weapon,i,chassis.id))design.slots[i]=chassis.slots[i];
        }
        balanceDirty=true;SelectCustomizationDesign(Array.FindIndex(balanceDraft.designs,x=>x.id==design.id),false);
    }
    void BalanceMountPreview()
    {
        BalanceDesign design=EditingDesign();if(design==null||balanceMountIndex<0||balanceMountIndex>=design.slots.Length)return;string id=design.slots[balanceMountIndex];
        BalanceWeapon w=Array.Find(balanceDraft.weapons,x=>x.id==id);if(w!=null)BalanceWeaponPreview(w);
    }
    void BalanceWeaponPreview(BalanceWeapon weapon)
    {
        WeaponArtEntry[] entries=WeaponCatalog().models;int i=Array.FindIndex(entries,x=>x.id==weapon.id);
        balancePreviewUnavailable=weapon.kind=="unarmed";
        if(i<0&&weapon.id.StartsWith("CUSTOM_",StringComparison.Ordinal))i=Array.FindIndex(entries,x=>x.id=="Military_AR");
        if(i>=0)ShowWeaponPreview(i);
        else if(weapon.id=="WISP_GuardShield")ShowWeaponPreview(0,null,wispShieldSource);
    }    void DrawBalanceWeapons()
    {
        balanceListScroll=GUI.BeginScrollView(new Rect(24,118,285,475),balanceListScroll,new Rect(0,0,258,balanceDraft.weapons.Length*35));
        for(int i=0;i<balanceDraft.weapons.Length;i++)
        {
            BalanceWeapon item=balanceDraft.weapons[i];Rect row=new Rect(0,i*35,255,31);
            if(GUI.Button(row,GUIContent.none)){balanceWeaponIndex=i;balanceValueScroll=Vector2.zero;BalanceSelectPreview();}
            Texture2D icon=balanceWeaponIcons.TryGetValue(item.id,out Texture2D cached)?cached:item.id.StartsWith("CUSTOM_",StringComparison.Ordinal)&&balanceWeaponIcons.TryGetValue("Military_AR",out Texture2D rifleIcon)?rifleIcon:null;
            if(icon!=null)GUI.DrawTexture(new Rect(row.x+4,row.y+1,45,29),icon,ScaleMode.ScaleToFit,true);
            else DrawBalanceWeaponMini(new Rect(row.x+4,row.y+1,45,29),item,i==balanceWeaponIndex);
            GUI.Label(new Rect(row.x+55,row.y+3,row.width-59,25),(i==balanceWeaponIndex?"> ":"")+item.name,new GUIStyle(GUI.skin.label){fontSize=16,alignment=TextAnchor.MiddleLeft});
        }
        GUI.EndScrollView();BalanceWeapon w=balanceDraft.weapons[balanceWeaponIndex];
        GUI.Label(new Rect(330,118,650,25),w.id+" / "+w.family+(w.kind=="firearm"?" / 사격 무기":w.kind=="shield"?" / 오른손 방패·우클릭 가드":w.kind=="unarmed"?" / 빈 손은 주먹 공격":" / 외형·수치 보관 (장착 준비 전)"));
        GUI.Label(new Rect(330,151,90,25),"이름");BalanceText(ref w.name,new Rect(420,148,530,29),80);
        GUI.Label(new Rect(330,186,90,25),"특성 표기");BalanceText(ref w.icon,new Rect(420,183,530,29),40);
        bool old=w.hands;w.hands=GUI.Toggle(new Rect(330,222,145,26),w.hands,"손 장착 허용");balanceDirty|=old!=w.hands;
        old=w.shoulders;w.shoulders=GUI.Toggle(new Rect(490,222,160,26),w.shoulders,"어깨 장착 허용");balanceDirty|=old!=w.shoulders;
        old=w.left;w.left=GUI.Toggle(new Rect(670,222,120,26),w.left,"왼쪽 허용");balanceDirty|=old!=w.left;
        old=w.right;w.right=GUI.Toggle(new Rect(810,222,130,26),w.right,"오른쪽 허용");balanceDirty|=old!=w.right;
        GUI.Label(new Rect(330,254,60,24),"메모");BalanceText(ref w.notes,new Rect(395,251,555,28),2000);
        if(balancePreviewUnavailable)GUI.Label(new Rect(980,168,270,80),"장착물 없음\n손 슬롯: 주먹 공격");else if(weaponPreviewTexture!=null)GUI.DrawTexture(new Rect(960,118,300,162),weaponPreviewTexture,ScaleMode.ScaleToFit);
        DrawBalanceNumbers(w.id,w.numbers,new Rect(330,295,924,299));
        if(GUI.Button(new Rect(24,600,285,38),"+ 새 사용자 화기"))BalanceAction(CreateCustomFirearmFromEditor);
    }
    void DrawBalanceParts()
    {
        BalanceDesign design=EditingDesign();if(design==null||design.parts==null||design.parts.Length==0)return;
        balancePartIndex=Mathf.Clamp(balancePartIndex,0,design.parts.Length-1);
        balanceListScroll=GUI.BeginScrollView(new Rect(24,118,285,475),balanceListScroll,new Rect(0,0,258,design.parts.Length*35));
        for(int i=0;i<design.parts.Length;i++){BalancePart part=design.parts[i];if(GUI.Button(new Rect(0,i*35,255,31),(i==balancePartIndex?"> ":"")+part.frame+" / "+part.name)){balancePartIndex=i;balanceValueScroll=Vector2.zero;}}
        GUI.EndScrollView();BalancePart p=design.parts[balancePartIndex];GUI.Label(new Rect(330,120,850,35),design.id+" / "+p.frame+" / "+p.name+" / "+p.id);GUI.Label(new Rect(330,164,70,25),"메모");BalanceText(ref p.notes,new Rect(405,160,830,30),2000);
        GUI.Label(new Rect(330,204,900,28),"선택한 설계 슬롯에만 파츠 수치를 저장합니다. 기체 크기·관절·외장은 변경하지 않습니다.");DrawBalanceNumbers(design.id+"/"+p.id,p.numbers,new Rect(330,248,924,346));
        if(GUI.Button(new Rect(24,600,285,38),"+ 새 사용자 코어 파츠"))BalanceAction(CreateCustomPartFromEditor);
    }
    void CreateCustomFirearmFromEditor()
    {
        int serial=1;string id;do{id="CUSTOM_FIREARM_"+serial.ToString("00");serial++;}while(balanceDraft.weapons.Any(w=>w.id==id));
        BalanceWeapon added=BalanceData.AddCustomFirearm(balanceDraft,id,"사용자 화기 "+(serial-1).ToString("00"));balanceWeaponIndex=Array.IndexOf(balanceDraft.weapons,added);balanceDirty=true;BalanceSelectPreview();balanceMessage=id+" 등록됨. 성능을 편집한 뒤 파일 저장을 누르세요.";
    }
    void CreateCustomPartFromEditor()
    {
        BalanceDesign design=EditingDesign();if(design==null)throw new InvalidDataException("먼저 설계 슬롯을 선택하세요.");int serial=1;string id;do{id="CUSTOM_PART_"+design.chassisId+"_"+serial.ToString("00");serial++;}while(balanceDraft.parts.Any(p=>p.id==id));
        BalancePart added=BalanceData.AddCustomPart(balanceDraft,design.id,id,"사용자 코어 파츠 "+(serial-1).ToString("00"));balancePartIndex=Array.IndexOf(design.parts,added);if(balancePartIndex<0)balancePartIndex=Array.FindIndex(design.parts,p=>p.id==id);balanceListScroll.y=Mathf.Max(0,balancePartIndex*35-400);balanceDirty=true;balanceMessage=id+" 등록됨. 설계별 수치를 편집한 뒤 파일 저장을 누르세요.";
    }
    void DrawBalanceNumbers(string owner,BalanceNumber[] values,Rect area)
    {
        balanceValueScroll=GUI.BeginScrollView(area,balanceValueScroll,new Rect(0,0,895,values.Length*34+4));
        for(int i=0;i<values.Length;i++)
        {
            BalanceNumber n=values[i];string key=owner+"/"+n.key;float y=i*34;if(!balanceNumberText.TryGetValue(key,out string raw))raw=n.value.ToString("0.#####",CultureInfo.InvariantCulture);
            Color old=GUI.color;GUI.color=n.implemented?Color.white:new Color(1,.70f,.32f);GUI.Label(new Rect(5,y+3,310,27),n.label+(n.implemented?"":" [보관]"));GUI.color=old;
            if(balanceInvalidNumbers.Contains(key))GUI.color=new Color(1,.35f,.35f);string next=GUI.TextField(new Rect(320,y,130,28),raw,24);GUI.color=old;
            if(next!=raw)
            {
                balanceNumberText[key]=next;balanceDirty=true;
                bool valid=float.TryParse(next,NumberStyles.Float,CultureInfo.InvariantCulture,out float v)&&!float.IsNaN(v)&&!float.IsInfinity(v)&&v>=n.min&&v<=n.max&&(!n.integer||v==Mathf.Round(v));
                if(valid){n.value=v;balanceInvalidNumbers.Remove(key);}else balanceInvalidNumbers.Add(key);
            }
            GUI.Label(new Rect(470,y+3,400,27),n.unit+"   ("+n.min.ToString("0.###")+" ~ "+n.max.ToString("0.###")+")");
        }
        GUI.EndScrollView();
    }
    void BalanceText(ref string text,Rect rect,int max){string value=GUI.TextField(rect,text??"",max);if(value!=text){balanceDirty=true;text=value;}}
    void DrawBalanceLoadout()
    {
        BalanceDesign design=EditingDesign();if(design==null||balanceMountIndex<0||balanceMountIndex>=design.slots.Length)return;
        GUI.Label(new Rect(24,151,88,28),"설계 이름");BalanceText(ref design.name,new Rect(112,148,220,27),80);
        GUI.Label(new Rect(350,151,45,28),"기체");string[] chassisNames={"WISP","BASTION","TARANTULA"};
        for(int c=0;c<chassisNames.Length;c++)
        {
            int ix=c;GUI.backgroundColor=design.chassisId==chassisNames[c]?new Color(.10f,.57f,.66f):new Color(.10f,.15f,.21f);
            if(GUI.Button(new Rect(398+c*124,148,116,27),chassisNames[c]))SelectCustomizationFrame(ix);
        }
        GUI.backgroundColor=Color.white;
        GUI.Label(new Rect(24,180,132,28),"슬롯 관리");
        if(GUI.Button(new Rect(148,178,126,30),"현재 슬롯 복사")){balanceDesignClipboard=BalanceData.CopyDesign(design);balanceMessage=design.name+" 설계를 복사했습니다. 대상 슬롯을 선택한 뒤 붙여넣으세요.";}
        bool oldEnabled=GUI.enabled;GUI.enabled=oldEnabled&&balanceDesignClipboard!=null;
        if(GUI.Button(new Rect(281,178,126,30),"붙여넣기")){BalanceData.PasteDesign(balanceDesignClipboard,design);balanceDirty=true;SelectCustomizationDesign(Array.FindIndex(balanceDraft.designs,x=>x.id==design.id),false);balanceMessage=design.name+" 슬롯에 복사한 설계를 넣었습니다.";}
        GUI.enabled=oldEnabled;
        if(GUI.Button(new Rect(414,178,126,30),"이 슬롯 초기화")){BalanceAction(()=>{BalanceData.ResetDesign(balanceDraft,design.id);balanceNumberText.Clear();balanceInvalidNumbers.Clear();balanceDirty=true;SelectCustomizationDesign(Array.FindIndex(balanceDraft.designs,x=>x.id==design.id),false);balanceMessage=design.name+" 슬롯을 기본 장비로 초기화했습니다. 저장 전까지 파일은 바뀌지 않습니다.";});}
        BalanceWeapon[] allowed=Array.FindAll(balanceDraft.weapons,w=>BalanceData.CanEquip(w,balanceMountIndex,design.chassisId));
        balanceListScroll=GUI.BeginScrollView(new Rect(24,212,530,361),balanceListScroll,new Rect(0,0,510,Mathf.Max(365,allowed.Length*54)));
        for(int i=0;i<allowed.Length;i++)
        {
            BalanceWeapon weapon=allowed[i];float y=i*54;bool selected=design.slots[balanceMountIndex]==weapon.id;
            GUI.backgroundColor=selected?new Color(.08f,.43f,.48f):new Color(.08f,.12f,.17f);
            bool clicked=GUI.Button(new Rect(0,y,500,48),"");if(balanceWeaponIcons.TryGetValue(weapon.id,out Texture2D iconImage))GUI.DrawTexture(new Rect(6,y+5,66,38),iconImage,ScaleMode.ScaleToFit);else DrawBalanceWeaponMini(new Rect(6,y+5,66,38),weapon,selected);
            GUI.Label(new Rect(80,y+4,405,22),(selected?"✓  ":"◇  ")+weapon.name,new GUIStyle(GUI.skin.label){fontStyle=FontStyle.Bold});
            GUI.Label(new Rect(80,y+25,405,18),weapon.family+"  ·  "+weapon.icon,small);
            if(clicked){design.slots[balanceMountIndex]=weapon.id;balanceDirty=true;BalanceWeaponPreview(weapon);}
        }
        GUI.backgroundColor=Color.white;GUI.EndScrollView();
        if(balancePreviewUnavailable)GUI.Label(new Rect(575,210,300,180),"맨손 공격\n장착물이 없는 손은 기존 주먹 동작을 사용합니다.");
        else if(weaponPreviewTexture!=null)GUI.DrawTexture(new Rect(575,205,300,210),weaponPreviewTexture,ScaleMode.ScaleToFit);
        BalanceWeapon current=Array.Find(balanceDraft.weapons,w=>w.id==design.slots[balanceMountIndex]);
        if(current!=null)
        {
            GUI.Label(new Rect(575,425,330,28),current.name,new GUIStyle(GUI.skin.label){fontSize=18,fontStyle=FontStyle.Bold});
            GUI.Label(new Rect(575,458,330,65),current.notes??"",new GUIStyle(GUI.skin.label){wordWrap=true});
            GUI.Label(new Rect(575,528,330,32),"상세 성능 수치는 [무기 옵션]에서 편집",small);
            if(GUI.Button(new Rect(575,565,260,34),"선택 무기 옵션 편집")){balanceWeaponIndex=Mathf.Max(0,Array.FindIndex(balanceDraft.weapons,w=>w.id==current.id));balanceTab=1;balanceValueScroll=Vector2.zero;BalanceSelectPreview();}
        }
    }
    System.Collections.IEnumerator CacheBalanceWeaponIcons()
    {
        EnsureWeaponStudio();WeaponArtEntry[] entries=WeaponCatalog().models;RenderTexture iconTarget=new RenderTexture(128,80,16);iconTarget.Create();RenderTexture previous=weaponStudioCamera.targetTexture;
        foreach(BalanceWeapon weapon in balanceDraft.weapons)
        {
            int index=Array.FindIndex(entries,x=>x.id==weapon.id);if(index<0&&weapon.id.StartsWith("CUSTOM_",StringComparison.Ordinal))index=Array.FindIndex(entries,x=>x.id=="Military_AR");if(index<0)continue;
            ShowWeaponPreview(index);weaponStudioCamera.targetTexture=iconTarget;RenderWeaponPreview();RenderTexture active=RenderTexture.active;RenderTexture.active=iconTarget;
            Texture2D icon=new Texture2D(128,80,TextureFormat.RGB24,false);icon.ReadPixels(new Rect(0,0,128,80),0,0);icon.Apply();RenderTexture.active=active;balanceWeaponIcons[weapon.id]=icon;
            weaponStudioCamera.targetTexture=previous;yield return null;
        }
        weaponStudioCamera.targetTexture=previous;iconTarget.Release();Destroy(iconTarget);balanceIconsReady=true;BalanceSelectPreview();
    }
    void DrawBalanceWeaponMini(Rect r,BalanceWeapon weapon,bool selected)
    {
        if(balancePixel==null){balancePixel=new Texture2D(1,1,TextureFormat.RGBA32,false);balancePixel.SetPixel(0,0,Color.white);balancePixel.Apply();}
        Color accent=weapon.kind=="shield"?new Color(.22f,.76f,.92f):weapon.id.Contains("Future")?new Color(.78f,.47f,1f):new Color(1f,.60f,.18f);
        GUI.DrawTexture(r,balancePixel);Color bg=selected?new Color(.04f,.24f,.28f):new Color(.04f,.07f,.10f);GUI.color=bg;GUI.DrawTexture(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),balancePixel);GUI.color=Color.white;
        if(weapon.kind=="shield")
        {
            GUI.color=accent;GUI.DrawTexture(new Rect(r.x+20,r.y+3,28,31),balancePixel);GUI.color=new Color(.06f,.10f,.14f);GUI.DrawTexture(new Rect(r.x+25,r.y+8,18,21),balancePixel);GUI.color=Color.white;return;
        }
        if(weapon.kind=="unarmed")
        {
            GUI.color=accent;GUI.DrawTexture(new Rect(r.x+23,r.y+8,26,22),balancePixel);GUI.DrawTexture(new Rect(r.x+15,r.y+15,10,8),balancePixel);GUI.color=Color.white;return;
        }
        bool longGun=weapon.id.Contains("SR")||weapon.id.Contains("Sniper")||weapon.id.Contains("Launcher")||weapon.id.Contains("RL");
        bool heavy=weapon.id.Contains("LMG")||weapon.id.Contains("Gatling")||weapon.id.Contains("GL")||weapon.id.Contains("Shotgun");
        GUI.color=new Color(.62f,.70f,.76f);GUI.DrawTexture(new Rect(r.x+8,r.y+14,longGun?45:34,12),balancePixel);GUI.color=accent;GUI.DrawTexture(new Rect(r.x+29,r.y+10,heavy?24:17,20),balancePixel);GUI.color=new Color(.14f,.19f,.24f);GUI.DrawTexture(new Rect(r.x+46,r.y+15,longGun?17:12,5),balancePixel);GUI.DrawTexture(new Rect(r.x+34,r.y+28,7,7),balancePixel);GUI.color=Color.white;
    }
    void DrawBalanceFiles()
    {
        GUI.Label(new Rect(30,125,170,28),"파일 이름 (.json 제외)");BalanceText(ref balanceFileName,new Rect(225,122,430,30),64);
        GUI.Label(new Rect(30,174,1200,68),"저장: 현재 편집값을 즉시 파일에 기록합니다.  불러오기: 파일 내용을 편집 창에 가져옵니다.\n전투에 적용: 양쪽 기체에 동일하게 반영합니다. 저장과 적용은 각각 별도로 실행합니다.");
        if(GUI.Button(new Rect(30,260,350,34),"현재 수정 버리고 파일 불러오기"))BalanceAction(BalanceLoad);
        if(GUI.Button(new Rect(400,260,350,34),"현재 수정 버리고 기본값 불러오기"))BalanceAction(()=>{balanceDraft=BalanceData.Defaults();balanceNumberText.Clear();balanceInvalidNumbers.Clear();balanceDirty=true;balanceMessage="기본값을 편집 창에 가져왔습니다. 기존 파일은 변경하지 않았습니다.";});
        if(GUI.Button(new Rect(770,260,440,34),"기존 전체 배율 · 소리 설정")){balanceEditorOpen=false;showTuning=true;}
        GUI.Label(new Rect(30,320,1200,30),"저장된 파일 (이름을 누른 뒤 [파일 불러오기]):");
        string[] files=Directory.Exists(BalanceDirectory)?Directory.GetFiles(BalanceDirectory,"*.json"):new string[0];balanceListScroll=GUI.BeginScrollView(new Rect(30,356,1190,232),balanceListScroll,new Rect(0,0,1150,files.Length*35));
        for(int i=0;i<files.Length;i++)if(GUI.Button(new Rect(0,i*35,1100,30),Path.GetFileName(files[i])))balanceFileName=Path.GetFileNameWithoutExtension(files[i]);GUI.EndScrollView();
    }
}
