using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
public sealed partial class VoxArena
{
    bool balanceEditorOpen,balanceDirty,balancePreviewUnavailable;BalanceDocument balanceDraft,activeBalance;BalanceDeveloperPresetBank developerPresetBank;BalanceDesign balanceDesignClipboard;int balanceTab,balanceWeaponIndex,balancePartIndex,balanceFrameIndex,balanceMountIndex,developerPresetSelection;float balancePreviewYaw,balancePreviewLastRenderTime;bool balancePreviewDragging;Vector2 balanceListScroll,balanceValueScroll,balancePartVariantScroll;string balanceFileName="PD-Balance",balanceMessage="",balanceDirectoryOverride;
    Texture2D balancePixel,balanceGlowTexture;Font balanceFont;GUIStyle weaponTraitCaption;readonly Dictionary<string,Texture2D> balanceWeaponIcons=new Dictionary<string,Texture2D>();bool balanceIconsRequested,balanceIconsReady;readonly Dictionary<string,string> balanceNumberText=new Dictionary<string,string>();readonly HashSet<string> balanceInvalidNumbers=new HashSet<string>();string balanceRangeEditKey="",balanceRangeMinText="",balanceRangeMaxText="";BalanceNumber balanceRangeEditTarget;
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
    string DeveloperPresetsPath()=>Path.Combine(BalanceDirectory,"DeveloperPresets.json");
    BalanceDeveloperPresetBank DeveloperPresetBank()
    {
        if(developerPresetBank==null)developerPresetBank=BalanceDeveloperPresetFiles.Load(DeveloperPresetsPath());
        return developerPresetBank;
    }
    void SaveDeveloperPreset(int index)
    {
        if(balanceInvalidNumbers.Count>0)throw new InvalidDataException("빨간색 수치 입력을 먼저 수정해 주세요.");
        developerPresetBank=BalanceDeveloperPresetFiles.SaveSlot(DeveloperPresetsPath(),DeveloperPresetBank(),index,balanceDraft.name,balanceDraft);
        developerPresetSelection=index;balanceMessage="개발용 실험 프리셋 "+(index+1).ToString("00")+" 저장 완료. 기본 데이터와 플레이어 설계 슬롯은 변경하지 않았습니다.";
    }
    void LoadDeveloperPreset(int index)
    {
        BalanceDeveloperPreset preset=DeveloperPresetBank().slots[index];
        if(preset.snapshot==null)throw new InvalidDataException("비어 있는 실험 프리셋입니다.");
        balanceDraft=BalanceData.Copy(preset.snapshot);balanceNumberText.Clear();balanceInvalidNumbers.Clear();balanceDirty=true;
        SelectCustomizationDesign(Array.FindIndex(balanceDraft.designs,x=>x.id==balanceDraft.activeDesignId),false);BalanceSelectPreview();
        developerPresetSelection=index;balanceMessage="실험 프리셋을 편집 초안으로 불러왔습니다. 기본 파일 저장과 전투 적용은 별도로 실행해야 합니다.";
    }
    void OpenBalanceEditor()
    {
        InitializeAutomaticBalance();
        if(balanceDraft==null)return;
        string chassis=CraftCatalog.Selectable[selectedCraft].name;
        int index=Array.FindIndex(balanceDraft.designs,d=>d.id==balanceDraft.activeDesignId&&!d.isEmpty&&d.chassisId==chassis);
        if(index<0)index=Array.FindIndex(balanceDraft.designs,d=>!d.isEmpty&&d.chassisId==chassis);
        balanceEditorOpen=true;showTuning=weaponGalleryOpen=false;
        if(balanceTab==3||balanceTab==4)balanceTab=0;
        if(index>=0)SelectCustomizationDesign(index);
        BalanceSelectPreview();if(!balanceIconsRequested){balanceIconsRequested=true;StartCoroutine(CacheBalanceWeaponIcons());}
    }
    void InitializeAutomaticBalance()
    {
        if(balanceDraft!=null)return;
        try
        {
            string path=BalancePath();
            BalanceDocument loaded=File.Exists(path)?BalanceFiles.Load(path):BalanceData.Defaults();
            if(!loaded.handMountDefaultsApplied)
            {
                // Preserve the user's complete pre-migration document once.
                if(File.Exists(path)&&!File.Exists(path+".before-hand-only.bak"))File.Copy(path,path+".before-hand-only.bak");
                foreach(BalanceWeapon weapon in loaded.weapons)
                {
                    if(weapon.kind=="unarmed")continue;
                    weapon.hands=true;weapon.shoulders=false;weapon.left=weapon.right=true;
                }
                loaded.handMountDefaultsApplied=true;RepairBalanceMounts(loaded);
                BalanceFiles.Save(path,loaded);
            }
            balanceDraft=loaded;activeBalance=BalanceData.Copy(loaded);
            balanceMessage="변경사항은 즉시 적용되고 자동 저장됩니다.";
        }
        catch(Exception e){balanceMessage="사용자 데이터 읽기 오류: "+e.Message;Debug.LogError(balanceMessage);}
    }
    static void RepairBalanceMounts(BalanceDocument document)
    {
        foreach(BalanceFrame frame in document.frames)Repair(frame.slots,frame.id);
        foreach(BalanceDesign design in document.designs)if(!design.isEmpty)Repair(design.slots,design.chassisId);
        void Repair(string[] slots,string chassis)
        {
            for(int i=0;i<slots.Length;i++)
            {
                BalanceWeapon weapon=Array.Find(document.weapons,w=>w.id==slots[i]);
                if(weapon==null||!BalanceData.CanEquip(weapon,i,chassis))slots[i]="UNARMED";
            }
            if(document.formatVersion>=5&&slots.Length>=4&&slots[2]==slots[3])
            {
                BalanceWeapon duplicate=Array.Find(document.weapons,w=>w.id==slots[2]);
                if(duplicate!=null&&duplicate.shoulders&&!BalanceData.CanDuplicateShoulderWeapon(duplicate))slots[3]="UNARMED";
            }
        }
    }
    void FlushAutomaticBalance()
    {
        if(!balanceDirty||balanceDraft==null||balanceInvalidNumbers.Count>0)return;
        try
        {
            RepairBalanceMounts(balanceDraft);
            BalanceDocument valid=BalanceData.ValidateAndNormalize(balanceDraft);
            BalanceFiles.Save(BalancePath(),valid);
            activeBalance=BalanceData.Copy(valid);balanceDirty=false;
            if(inMenu&&player!=null&&enemy!=null){ApplySelectableCharacter(player,selectedCraft);ApplySelectableCharacter(enemy,ResolveRival());BuildPortraits();BalanceSelectPreview();}
            balanceMessage="자동 저장 완료 · 변경사항 적용됨";
        }
        catch(Exception e){balanceMessage="자동 저장 실패: "+e.Message;}
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
        if(EditingDesign()?.isEmpty==true)throw new InvalidDataException("빈 설계 슬롯은 먼저 기체를 지정해야 합니다.");
        if(balanceInvalidNumbers.Count>0)throw new InvalidDataException("빨간색 수치 입력을 먼저 수정해 주세요.");
        BalanceDocument checkedDraft=BalanceData.ValidateAndNormalize(balanceDraft);activeBalance=BalanceData.Copy(checkedDraft);
        ApplySelectableCharacter(player,selectedCraft);ApplySelectableCharacter(enemy,ResolveRival());ResetRound();BuildPortraits();
        balanceMessage="적용 완료: 선택한 설계를 플레이어 기체에 반영했습니다. 저장하지 않은 변경은 종료 시 사라집니다.";
    }
    void ApplyBalanceParts(Fighter f)
    {
        f.headVisualId="HEAD_STANDARD";f.coreVisualId="CORE_STANDARD";f.leftArmVisualId=f.rightArmVisualId="ARM_STANDARD";f.legsVisualId="LEGS_STANDARD";f.boosterVisualId="BOOSTER_TWIN";
        if(activeBalance==null||f.selectableIndex<0)return;
        BalancePart[] parts=activeBalance.parts;
        bool designPreview=f.root!=null&&f.root.name=="Portrait studio robot";
        if(f==player||designPreview){BalanceDesign selected=Array.Find(activeBalance.designs,x=>x.id==activeBalance.activeDesignId&&x.chassisId==f.spec.name);if(selected!=null)parts=selected.parts;}
        BalancePart head=parts.FirstOrDefault(part=>part!=null&&part.frame==f.spec.name&&part.category=="HEAD");
        if(head!=null)f.headVisualId=BalanceData.NormalizePartVisualId(head,head.visualId);
        BalancePart core=parts.FirstOrDefault(part=>part!=null&&part.frame==f.spec.name&&part.category=="CORE");
        if(core!=null)f.coreVisualId=BalanceData.NormalizePartVisualId(core,core.visualId);
        BalancePart leftArm=parts.FirstOrDefault(part=>part!=null&&part.frame==f.spec.name&&part.category=="LEFT_ARM");
        BalancePart rightArm=parts.FirstOrDefault(part=>part!=null&&part.frame==f.spec.name&&part.category=="RIGHT_ARM");
        BalancePart legs=parts.FirstOrDefault(part=>part!=null&&part.frame==f.spec.name&&part.category=="LEGS");
        if(leftArm!=null)f.leftArmVisualId=BalanceData.NormalizePartVisualId(leftArm,leftArm.visualId);
        if(rightArm!=null)f.rightArmVisualId=BalanceData.NormalizePartVisualId(rightArm,rightArm.visualId);
        if(legs!=null)f.legsVisualId=BalanceData.NormalizePartVisualId(legs,legs.visualId);
        BalancePart booster=parts.FirstOrDefault(part=>part!=null&&part.frame==f.spec.name&&part.category=="BOOSTER");
        if(booster!=null)f.boosterVisualId=BalanceData.NormalizePartVisualId(booster,booster.visualId);
        foreach(BalancePart part in parts)if(part.frame==f.spec.name)foreach(BalanceNumber n in part.numbers)if(n.implemented)
            typeof(CraftSpec).GetField(n.key).SetValue(f.spec,n.value);
    }
    void ApplyBalanceDesignPaint(Fighter f)
    {
        bool designPreview=f!=null&&f.root!=null&&f.root.name=="Portrait studio robot";if(f==null||f.body==null||f!=player&&!designPreview)return;BalanceDesign design=activeBalance==null?null:Array.Find(activeBalance.designs,x=>x.id==activeBalance.activeDesignId&&x.chassisId==f.spec.name);
        Color tint=Color.white;bool applyTint=design!=null&&!design.isEmpty&&!string.IsNullOrEmpty(design.paintColorHex)&&ColorUtility.TryParseHtmlString("#"+design.paintColorHex,out tint);
        foreach(Renderer renderer in f.body.GetComponentsInChildren<Renderer>(true))
        {
            if(renderer==null||f.weaponPart!=null&&renderer.transform.IsChildOf(f.weaponPart)||f.saber!=null&&renderer.transform.IsChildOf(f.saber)||f.shield!=null&&renderer.transform.IsChildOf(f.shield)||f.thruster!=null&&renderer.transform.IsChildOf(f.thruster)||f.jumpFlame!=null&&renderer.transform.IsChildOf(f.jumpFlame))continue;
            Material[] materials=renderer.sharedMaterials;
            for(int i=0;i<materials.Length;i++)
            {
                Material material=materials[i];if(material==null)continue;string property=material.HasProperty("_BaseColor")?"_BaseColor":material.HasProperty("_Color")?"_Color":null;if(property==null)continue;
                MaterialPropertyBlock block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block,i);block.SetColor(property,applyTint?tint:material.GetColor(property));renderer.SetPropertyBlock(block,i);
            }
        }
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
            BalanceWeapon w=Array.Find(activeBalance.weapons,x=>x.id==slots[i]);WeaponProfile p=new WeaponProfile(w.name,Loadout[i].mount,(int)w.Get("capacity"),w.Get("damage"),w.Get("interval"),w.Get("speed")*2f,w.Get("radius"),w.Get("range"));
            p.guardReduction=w.Get("guardReduction");p.weight=w.Get("weight");p.ammoWeight=w.Get("ammoWeight");p.trait=w.traitIconId;p.additionalTraits=w.additionalTraitIconIds;p.lockStrength=w.Get("lockStrength");p.lockPenalty=w.Get("lockPenalty");p.blocksShoulderWeapons=w.blocksShoulderWeapons;p.allowsShoulderDuplicate=w.allowsShoulderDuplicate;p.isShield=w.kind=="shield"||BalanceData.HasWeaponTrait(w,"SHIELD");p.isBeamShield=p.isShield&&(w.id.Contains("Holo")||w.id.Contains("BeamShield"));p.isUnarmed=w.kind=="unarmed";p.partialReload=w.Get("partialReload");p.emptyReload=w.Get("emptyReload");p.idleReload=w.Get("idleReload");p.spread=w.Get("spread");p.blast=w.Get("blast");profiles[i]=p;
        }
        bool shoulderBlocked=profiles.Take(2).Any(p=>p.blocksShoulderWeapons&&!p.isShield&&!p.isUnarmed);
        for(int i=2;i<4;i++)profiles[i].shoulderBlocked=shoulderBlocked;
        if(shoulderBlocked&&f.weaponSlot>2)f.weaponSlot=1;
        return profiles;
    }
    static readonly string[] balanceSkillIds={"SweepBeam","SiegeBeam","SaberRush","MissileFan","Nova","RailLance","HomingMines","Minefield"};
    static string BalanceSkillTitle(string id)=>id=="SweepBeam"?"아크 스윕":id=="SiegeBeam"?"공성 레이저":id=="SaberRush"?"세이버 러시":id=="MissileFan"?"미사일 부채꼴":id=="Nova"?"노바 링":id=="RailLance"?"차지 랜스":id=="HomingMines"?"추적 지뢰":id=="Minefield"?"근접 지뢰밭":id;
    static string BalanceSkillDescription(string id)=>id=="SweepBeam"?"전방을 넓게 쓸어 빔을 발사합니다.":id=="SiegeBeam"?"잠시 정지해 강한 관통 빔을 집중 발사합니다.":id=="SaberRush"?"조준 방향으로 돌진해 근접 공격을 합니다.":id=="MissileFan"?"락온 후 부채꼴로 유도 미사일 9발을 발사합니다.":id=="Nova"?"주변으로 폭발 탄환 16발을 펼칩니다.":id=="RailLance"?"빠르고 긴 관통 랜스를 직선으로 발사합니다.":id=="HomingMines"?"락온한 방향으로 추적 지뢰 3발을 발사합니다.":id=="Minefield"?"기체 주변에 접근 감지 지뢰 5개를 설치합니다.":"기체 특수 스킬";
    static string BalanceSkillGlyphId(string id)=>id=="SaberRush"?"JUMP":id=="MissileFan"||id=="HomingMines"?"LOCK":id=="Nova"||id=="Minefield"?"AREA":id=="RailLance"?"RADAR":"DAMAGE";
    float BalanceSkillCooldown(string chassis)=>CraftCatalog.Selectable.FirstOrDefault(x=>x.name==chassis)?.specialCooldown??8f;
    void DrawBalanceSkills()
    {
        BalanceDesign design=EditingDesign();if(design==null)return;
        GUI.Label(new Rect(24,120,840,28),"스킬을 설계 슬롯에 지정합니다. 현재 전투 규칙에 구현된 8개 특수 동작을 사용합니다.",small);
        if(weaponTraitCaption==null)weaponTraitCaption=new GUIStyle(small){fontSize=11,alignment=TextAnchor.MiddleCenter};
        for(int i=0;i<balanceSkillIds.Length;i++)
        {
            string id=balanceSkillIds[i];int col=i%4,row=i/4;Rect card=new Rect(24+col*218,157+row*154,204,140);bool selected=design.skillId==id;
            Panel(card,selected?new Color(.07f,.28f,.31f):new Color(.045f,.075f,.10f));Panel(new Rect(card.x,card.y,card.width,2),selected?Cyan:new Color(.18f,.29f,.35f));
            GUI.backgroundColor=selected?new Color(.10f,.26f,.29f):new Color(.07f,.11f,.15f);bool clicked=GUI.Button(new Rect(card.x+4,card.y+4,card.width-8,card.height-8),GUIContent.none);GUI.backgroundColor=Color.white;
            DrawWeaponTraitGlyph(new Rect(card.x+13,card.y+14,34,34),BalanceSkillGlyphId(id),selected?Color.white:Orange);
            GUI.Label(new Rect(card.x+57,card.y+13,card.width-66,26),BalanceSkillTitle(id),new GUIStyle(GUI.skin.label){fontSize=15,fontStyle=FontStyle.Bold});
            GUI.Label(new Rect(card.x+12,card.y+58,card.width-24,42),BalanceSkillDescription(id),new GUIStyle(small){wordWrap=true});
            GUI.Label(new Rect(card.x+12,card.y+105,card.width-24,20),"재사용 대기  "+BalanceSkillCooldown(design.chassisId).ToString("0.#")+"초",small);
            if(clicked&&design.skillId!=id){design.skillId=id;balanceDirty=true;balanceMessage=BalanceSkillTitle(id)+" 지정됨. 자동 저장됩니다.";}
        }
        string current=BalanceData.IsSkillId(design.skillId)?design.skillId:BalanceData.DefaultSkillIdForChassis(design.chassisId);
        Panel(new Rect(24,474,870,76),new Color(.045f,.085f,.11f));
        DrawWeaponTraitGlyph(new Rect(38,493,38,38),BalanceSkillGlyphId(current),Cyan);
        GUI.Label(new Rect(88,483,300,24),"선택 스킬  /  "+BalanceSkillTitle(current),new GUIStyle(GUI.skin.label){fontSize=17,fontStyle=FontStyle.Bold});
        GUI.Label(new Rect(88,511,500,28),BalanceSkillDescription(current),small);
        GUI.Label(new Rect(620,500,250,24),"COOLDOWN  "+BalanceSkillCooldown(design.chassisId).ToString("0.#")+" s",small);
    }
    void ApplyBalanceSkill(Fighter f)
    {
        if(f!=player||activeBalance==null)return;
        BalanceDesign design=Array.Find(activeBalance.designs,x=>x.id==activeBalance.activeDesignId&&x.chassisId==f.spec.name);
        if(design==null||!Enum.TryParse(design.skillId,out SpecialKind skill))return;
        f.spec.special=skill;f.spec.specialName=BalanceSkillTitle(skill.ToString());
    }
    bool HasEquippedGuard(Fighter f)=>f!=null&&f.weaponProfiles!=null&&Array.Exists(f.weaponProfiles,p=>p.isShield);
    bool UsesNativeGuardShield(Fighter f)=>f!=null&&f.spec.name=="WISP"&&f.weaponProfiles!=null&&f.weaponProfiles[1].isShield&&f.weaponArtIds!=null&&f.weaponArtIds[1]=="WISP_GuardShield";
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
            string[] tabs={"장착","무기 옵션","파츠 수치","스킬 선택","신형 외형"};
            int[] tabIds={0,1,2,5,6};
            for(int i=0;i<tabs.Length;i++)
            {
                GUI.backgroundColor=balanceTab==tabIds[i]?new Color(1f,.42f,.10f):new Color(.10f,.15f,.21f);
                if(GUI.Button(new Rect(365+i*142,58,134,34),tabs[i])){balanceTab=tabIds[i];balanceListScroll=balanceValueScroll=Vector2.zero;BalanceSelectPreview();}
            }
            GUI.backgroundColor=Color.white;
            Panel(new Rect(20,104,328,550),new Color(.035f,.055f,.080f));
            Panel(new Rect(20,104,4,550),new Color(1f,.40f,.09f));
            string frameName=balanceFrameIndex>=0&&balanceFrameIndex<balanceDraft.frames.Length?balanceDraft.frames[balanceFrameIndex].id:"WISP";
            int designIndex=Array.FindIndex(balanceDraft.designs,x=>x.id==balanceDraft.activeDesignId);if(designIndex<0)designIndex=0;
            BalanceDesign activeDesign=EditingDesign();Text(40,112,286,(designIndex+1).ToString("00")+" / "+(activeDesign!=null&&activeDesign.isEmpty?"빈 슬롯":frameName),label);
            Rect previewRect=new Rect(36,145,296,420);
            if(activeDesign!=null&&activeDesign.isEmpty)
            {
                Panel(previewRect,new Color(.045f,.075f,.10f));GUI.Label(new Rect(63,319,242,70),"비어 있는 설계\n기체를 선택해 구성하세요.",new GUIStyle(GUI.skin.label){fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=true});
            }
            else
            {
                if(customizationPortraits!=null&&balanceFrameIndex>=0&&balanceFrameIndex<customizationPortraits.Length&&customizationPortraits[balanceFrameIndex]!=null)GUI.DrawTexture(previewRect,customizationPortraits[balanceFrameIndex],ScaleMode.ScaleToFit,true);
                DrawBalanceSelectionHighlight(previewRect);Text(42,570,270,"기체를 클릭한 채 좌우로 움직여 회전",small);
                Event ev=Event.current;
                if(ev.type==EventType.MouseDown&&previewRect.Contains(ev.mousePosition)){balancePreviewDragging=true;ev.Use();}
                else if(ev.type==EventType.MouseDrag&&balancePreviewDragging){balancePreviewYaw+=ev.delta.x*.65f;if(Time.unscaledTime-balancePreviewLastRenderTime>=.02f){portraitYaw=balancePreviewYaw;RenderPortraitFrame(balanceFrameIndex);RenderPortraitFrame(balanceFrameIndex,true);balancePreviewLastRenderTime=Time.unscaledTime;}ev.Use();}
                else if(ev.type==EventType.MouseUp&&balancePreviewDragging){balancePreviewDragging=false;portraitYaw=balancePreviewYaw;RenderPortraitFrame(balanceFrameIndex);RenderPortraitFrame(balanceFrameIndex,true);ev.Use();}
            }
            GUI.backgroundColor=Color.white;
            Panel(new Rect(360,106,896,488),new Color(.035f,.055f,.080f));
            GUI.Label(new Rect(383,101,160,28),"설정 이름");BalanceText(ref balanceDraft.name,new Rect(476,99,245,28),80);
            GUI.Label(new Rect(760,101,475,28),balanceTab==4?"개발 전용 · 10개 실험 슬롯":"양손 · 양어깨 장착 / 개별 수치 조정",new GUIStyle(GUI.skin.label){fontSize=14});
            bool activeDesignEmpty=EditingDesign()?.isEmpty==true;
            Matrix4x4 oldMatrix=GUI.matrix;
            if(balanceTab!=6)GUI.matrix=Matrix4x4.TRS(new Vector3(684,120,0),Quaternion.identity,new Vector3(.70f,.70f,1f))*oldMatrix;
            if(activeDesignEmpty&&balanceTab!=0)GUI.Label(new Rect(382,146,820,70),"현재 슬롯은 비어 있습니다. [장착]에서 WISP, BASTION, TARANTULA 중 기체를 골라 설계를 만드세요.",new GUIStyle(GUI.skin.label){fontSize=17,wordWrap=true});
            else if(balanceTab==0)DrawUnifiedCustomization();else if(balanceTab==1)DrawBalanceWeapons();else if(balanceTab==2)DrawBalanceParts();else if(balanceTab==4)DrawDeveloperPresets();else if(balanceTab==5)DrawBalanceSkills();else if(balanceTab==6)DrawFamilyArtOptions();else DrawBalanceFiles();
            GUI.matrix=oldMatrix;
            Text(372,602,875,balanceMessage,new GUIStyle(small){wordWrap=true});
            Text(372,630,875,balanceTab==4?"실험 프리셋 파일: "+DeveloperPresetsPath():"파일: "+BalanceDirectory+Path.DirectorySeparatorChar+balanceFileName+".json",small);
            Text(372,670,875,"변경사항 즉시 적용 · 자동 저장",small);
        }
        finally{GUI.skin.font=previous;GUI.backgroundColor=Color.white;}
    }
    void DrawBalanceSelectionHighlight(Rect view)
    {
        BalanceDesign design=EditingDesign();if(design==null||design.isEmpty)return;Rect focus;string category;
        if(balanceTab==0)
        {
            bool shoulder=balanceMountIndex>=2;bool left=balanceMountIndex==0||balanceMountIndex==2;
            focus=new Rect(view.x+view.width*(left?.08f:.66f),view.y+view.height*(shoulder?.23f:.27f),view.width*.26f,view.height*.27f);
            category=shoulder?(left?"LEFT SHOULDER":"RIGHT SHOULDER"):(left?"LEFT HAND":"RIGHT HAND");
        }
        else if(balanceTab==2&&design.parts!=null&&design.parts.Length>0)
        {
            BalancePart p=design.parts[Mathf.Clamp(balancePartIndex,0,design.parts.Length-1)];category=p.category;
            float y=category=="HEAD"?.08f:category=="CORE"?.31f:category=="ARMS"?.36f:category=="LEGS"?.65f:category=="BOOSTER"?.27f:.4f;
            float x=category=="BOOSTER"?.68f:.29f;float w=category=="CORE"?.42f:category=="LEGS"?.42f:.40f;
            focus=new Rect(view.x+view.width*x,view.y+view.height*y,view.width*w,view.height*(category=="HEAD"?.18f:category=="CORE"?.27f:category=="LEGS"?.27f:.22f));
        }
        else return;
        if(balanceGlowTexture==null)
        {
            balanceGlowTexture=new Texture2D(64,64,TextureFormat.RGBA32,false,true);Color[] pixels=new Color[64*64];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {
                float radius=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;
                float alpha=1f-Mathf.SmoothStep(.40f,1f,radius);pixels[y*64+x]=new Color(1,1,1,alpha*.7f);
            }
            balanceGlowTexture.SetPixels(pixels);balanceGlowTexture.Apply();
        }
        float pulse=.18f+.14f*(.5f+.5f*Mathf.Sin(Time.unscaledTime*2.4f));Color prior=GUI.color;
        GUI.color=new Color(.18f,.94f,1f,pulse);GUI.DrawTexture(new Rect(focus.x-focus.width*.12f,focus.y-focus.height*.12f,focus.width*1.24f,focus.height*1.24f),balanceGlowTexture,ScaleMode.StretchToFill,true);GUI.color=prior;
    }
    void DrawDeveloperPresets()
    {
        BalanceDeveloperPresetBank bank;
        try{bank=DeveloperPresetBank();}catch(Exception e){Text(42,145,1050,"프리셋 파일을 읽지 못했습니다: "+e.Message,label);return;}
        Text(32,126,1110,"현재 밸런스 초안을 별도 저장하고 비교합니다. 저장/불러오기만으로 기본 파일이나 전투값은 바뀌지 않습니다.",small);
        for(int i=0;i<BalanceDeveloperPresetFiles.SlotCount;i++)
        {
            BalanceDeveloperPreset preset=bank.slots[i];int row=i/2,col=i%2;Rect card=new Rect(30+col*570,160+row*102,540,98);bool selected=i==developerPresetSelection;
            Panel(card,selected?new Color(.07f,.21f,.25f):new Color(.045f,.075f,.10f));
            Panel(new Rect(card.x,card.y,card.width,2),selected?Cyan:new Color(.18f,.29f,.35f));
            GUI.backgroundColor=selected?new Color(.10f,.25f,.29f):new Color(.08f,.12f,.16f);
            if(GUI.Button(new Rect(card.x+8,card.y+3,card.width-16,23),preset.id+"  /  "+preset.name))developerPresetSelection=i;
            GUI.backgroundColor=Color.white;
            if(!preset.populated||preset.snapshot==null)GUI.Label(new Rect(card.x+10,card.y+29,card.width-20,18),"비어 있음 · 저장된 실험 없음",small);
            else
            {
                BalanceDesign design=Array.Find(preset.snapshot.designs,d=>d.id==preset.snapshot.activeDesignId);
                BalanceWeapon weapon=null;
                if(design!=null&&design.slots!=null&&design.slots.Length>0)weapon=Array.Find(preset.snapshot.weapons,w=>w.id==design.slots[0]);
                GUI.Label(new Rect(card.x+10,card.y+27,card.width-20,18),design==null?"설계 정보 없음":design.chassisId+" · "+(weapon==null?"무장 정보 없음":weapon.name),small);
                GUI.Label(new Rect(card.x+10,card.y+45,card.width-20,18),weapon==null?"":"DAMAGE "+weapon.Get("damage").ToString("0.#")+"  ·  RATE "+(1f/weapon.Get("interval")).ToString("0.##")+"/s",small);
            }
            if(GUI.Button(new Rect(card.x+8,card.y+72,160,22),"현재값 저장"))BalanceAction(()=>SaveDeveloperPreset(i));
            bool loadEnabled=preset.populated&&preset.snapshot!=null,wasEnabled=GUI.enabled;GUI.enabled=wasEnabled&&loadEnabled;
            if(GUI.Button(new Rect(card.x+176,card.y+72,160,22),"초안 불러오기"))BalanceAction(()=>LoadDeveloperPreset(i));
            GUI.enabled=wasEnabled;
        }
    }
    BalanceDesign EditingDesign()=>balanceDraft!=null?Array.Find(balanceDraft.designs,x=>x.id==balanceDraft.activeDesignId):null;
    void SelectCustomizationDesign(int index,bool markDirty=true)
    {
        if(balanceDraft==null||index<0||index>=balanceDraft.designs.Length)return;
        BalanceDesign design=balanceDraft.designs[index];balanceDraft.activeDesignId=design.id;
        if(design.isEmpty){balanceFrameIndex=-1;if(markDirty)balanceDirty=true;BalanceSelectPreview();return;}
        balanceFrameIndex=Array.FindIndex(balanceDraft.frames,x=>x.id==design.chassisId);if(balanceFrameIndex<0)return;
        int craft=Array.FindIndex(CraftCatalog.Selectable,x=>x.name==design.chassisId);
        if(craft<0)return;
        selectedCraft=craft;balancePreviewYaw=portraitYaw=0;if(markDirty)balanceDirty=true;
        ApplySelectableCharacter(player,selectedCraft);BuildPortraits();BalanceSelectPreview();
    }
    void SelectCustomizationFrame(int index)
    {
        if(index<0||index>=balanceDraft.frames.Length)return;BalanceDesign design=EditingDesign();if(design==null)return;
        if(design.isEmpty){BalanceData.CreateDesign(balanceDraft,design.id,balanceDraft.frames[index].id);balanceDirty=true;SelectCustomizationDesign(Array.FindIndex(balanceDraft.designs,x=>x.id==design.id),false);balanceMessage=design.name+" 설계 슬롯을 만들었습니다. 파일 저장 전까지 초안에만 반영됩니다.";return;}
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
    void RefreshBalanceDraftPreview()
    {
        if(balanceDraft==null||!inMenu)return;
        BalanceDocument applied=activeBalance;
        try
        {
            activeBalance=BalanceData.ValidateAndNormalize(balanceDraft);int craft=Array.FindIndex(CraftCatalog.Selectable,x=>x.name==EditingDesign()?.chassisId);
            if(craft>=0){selectedCraft=craft;ApplySelectableCharacter(player,craft);BuildPortraits();BalanceSelectPreview();}
        }
        catch(Exception e){Debug.LogWarning("BALANCE_PREVIEW "+e.Message);}
        finally{activeBalance=applied;}
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
            DrawWeaponTraitGlyph(new Rect(row.x+56,row.y+9,14,14),item.traitIconId,item.family=="future"?Cyan:Orange);
            GUI.Label(new Rect(row.x+75,row.y+3,row.width-79,25),(i==balanceWeaponIndex?"> ":"")+item.name,new GUIStyle(GUI.skin.label){fontSize=16,alignment=TextAnchor.MiddleLeft});
        }
        GUI.EndScrollView();BalanceWeapon w=balanceDraft.weapons[balanceWeaponIndex];
        GUI.Label(new Rect(330,118,650,25),w.id+" / "+w.family+(BalanceData.HasWeaponTrait(w,"SHIELD")||w.kind=="shield"?" / 방패·우클릭 가드":w.kind=="firearm"?" / 사격 무기":w.kind=="unarmed"?" / 빈 손은 주먹 공격":" / 외형·수치 보관 (장착 준비 전)"));
        GUI.Label(new Rect(330,151,90,25),"이름");BalanceText(ref w.name,new Rect(420,148,530,29),80);
        GUI.Label(new Rect(330,186,90,25),"특성 이름");BalanceText(ref w.icon,new Rect(420,183,530,29),40);
        GUI.Label(new Rect(330,222,90,26),"장착 위치");
        if(w.kind=="firearm"&&!BalanceData.HasWeaponTrait(w,"SHIELD"))
        {
            int mount=w.shoulders?1:0;
            int next=GUI.SelectionGrid(new Rect(420,222,320,28),mount,new[]{"손 전용","어깨 전용"},2);
            if(next!=mount){w.hands=next==0;w.shoulders=next==1;w.left=w.right=true;balanceDirty=true;}
        }
        else GUI.Label(new Rect(420,222,400,26),w.kind=="unarmed"?"미장착":"손 전용");
        bool mayDuplicate=GUI.Toggle(new Rect(754,222,240,24),w.allowsShoulderDuplicate,"양쪽 어깨 중복 착용");
        if(mayDuplicate!=w.allowsShoulderDuplicate){w.allowsShoulderDuplicate=mayDuplicate;balanceDirty=true;}
        bool blocksShoulder=w.kind=="firearm"&&GUI.Toggle(new Rect(754,246,300,24),w.blocksShoulderWeapons,"장착 중 어깨 무장 사용 불가");
        if(w.kind=="firearm"&&blocksShoulder!=w.blocksShoulderWeapons){w.blocksShoulderWeapons=blocksShoulder;balanceDirty=true;}
        DrawWeaponTraitIconPicker(w);
        GUI.Label(new Rect(330,310,60,24),"메모");BalanceText(ref w.notes,new Rect(395,307,555,28),2000);
        if(balancePreviewUnavailable)GUI.Label(new Rect(980,168,270,80),"장착물 없음\n손 슬롯: 주먹 공격");else if(weaponPreviewTexture!=null)GUI.DrawTexture(new Rect(960,118,300,162),weaponPreviewTexture,ScaleMode.ScaleToFit);
        DrawBalanceNumbers(w.id,(BalanceData.HasWeaponTrait(w,"SHIELD")||w.kind=="shield")?w.numbers.Where(n=>n.key=="guardReduction").ToArray():w.numbers.Where(n=>n.key!="guardReduction").ToArray(),new Rect(330,343,924,251));
        if(GUI.Button(new Rect(24,600,285,38),"+ 새 사용자 화기"))BalanceAction(CreateCustomFirearmFromEditor);
    }
    static readonly string[] balanceWeaponTraitIds={"DAMAGE","LOCK","RADAR","AREA","RAPID","RELOAD","SHIELD"};
    static string WeaponTraitLabel(string id)=>id=="DAMAGE"?"DMG":id=="LOCK"?"LOCK":id=="RADAR"?"RADAR":id=="AREA"?"AREA":id=="RAPID"?"RATE":id=="RELOAD"?"LOAD":id=="JUMP"?"JUMP":"GUARD";
    static string BalanceTraitMeaning(string id)=>id=="DAMAGE"?"공격 피해 관련":id=="LOCK"?"우클릭 락온 · 유도 지속 비율 0이면 직진":id=="RADAR"?"탐지·레이더 관련":id=="AREA"?"범위 공격 관련":id=="RAPID"?"연사 속도 관련":id=="RELOAD"?"재장전 관련":id=="JUMP"?"점프·부스터 기동 관련":id=="SHIELD"?"방패 · 우클릭 정면 피해 감소 · 무기 선택 제외":"등록되지 않은 특성";
    static string BalancePartPerformanceSummary(BalancePart part)
    {
        BalanceNumber[] values=(part?.numbers??new BalanceNumber[0]).Where(n=>n!=null&&n.implemented).Take(3).ToArray();
        if(values.Length==0)return "전투에 적용되는 수치 없음";
        return string.Join("  ·  ",values.Select(n=>n.label+" "+n.value.ToString("0.##",CultureInfo.InvariantCulture)+(string.IsNullOrEmpty(n.unit)?"":" "+n.unit)));
    }
    static readonly string[] balanceAssemblySlotIds={"HEAD","CORE","ARMS","LEGS","BOOSTER","SKILL"};
    static readonly string[] balanceAssemblySlotLabels={"머리","몸통","팔","다리","백팩","스킬"};
    static string BalanceAssemblyCategory(BalancePart part)=>part.category=="LEFT_ARM"||part.category=="RIGHT_ARM"?"ARMS":part.category;
    static readonly string[] balanceCustomizationPartSlots={"HEAD","CORE","LEFT_ARM","RIGHT_ARM","LEGS","BOOSTER","SKILL"};
    static readonly string[] balanceCustomizationPartLabels={"머리","몸통","왼팔","오른팔","다리","백팩","스킬"};
    static string[] BalancePartVisualIds(BalancePart part)
    {
        if(part==null)return Array.Empty<string>();
        bool head=part.category=="HEAD",core=part.category=="CORE",arm=part.category=="LEFT_ARM"||part.category=="RIGHT_ARM",legs=part.category=="LEGS";
        CraftSpec frame=legs?CraftCatalog.Selectable.FirstOrDefault(c=>c.name==part.frame):null;bool bipedLegs=frame!=null&&(frame.mobility==MobilityKind.StandardBiped||frame.mobility==MobilityKind.Runner);
        return head?new[]{"HEAD_STANDARD","HEAD_RANGEFINDER","HEAD_OMNI"}:core?new[]{"CORE_STANDARD","CORE_REACTOR","CORE_BULWARK"}:arm?new[]{"ARM_STANDARD","ARM_REINFORCED","ARM_PRECISION","ARM_ASSAULT","ARM_BULWARK","ARM_SCOUT","ARM_CLAW","ARM_BRUISER","ARM_VANGUARD","ARM_TACTICAL","ARM_EXOSKELETON"}:legs?(bipedLegs?new[]{"LEGS_STANDARD","LEGS_ASSAULT","LEGS_SCOUT","LEGS_RANGER","LEGS_HEAVY","LEGS_JUMPER","LEGS_REVERSE"}:new[]{"LEGS_STANDARD","LEGS_ASSAULT","LEGS_SCOUT"}):new[]{"BOOSTER_TWIN","BOOSTER_VECTOR","BOOSTER_CYCLONE"};
    }
    static int BalanceAssemblyPartIndex(BalanceDesign design,string slot,int armSide=0)
    {
        if(design?.parts==null)return -1;
        string category=slot=="ARMS"?(armSide==0?"LEFT_ARM":"RIGHT_ARM"):slot;
        return Array.FindIndex(design.parts,p=>p!=null&&p.frame==design.chassisId&&p.category==category);
    }
    void DrawUnifiedCustomization()
    {
        BalanceDesign design=EditingDesign();if(design==null||design.isEmpty)return;
        GUI.Label(new Rect(16,136,550,24),"무기",new GUIStyle(GUI.skin.label){fontSize=15,fontStyle=FontStyle.Bold});
        GUI.Label(new Rect(604,136,600,24),"파츠",new GUIStyle(GUI.skin.label){fontSize=15,fontStyle=FontStyle.Bold});
        string[] mountLabels={"왼손","오른손","왼어깨","오른어깨"};
        for(int mount=0;mount<mountLabels.Length;mount++)
        {
            Rect card=new Rect(12,164+mount*99,560,88);Panel(card,new Color(.045f,.075f,.10f));
            GUI.Label(new Rect(card.x+8,card.y+31,72,26),mountLabels[mount],new GUIStyle(GUI.skin.label){fontSize=13,alignment=TextAnchor.MiddleCenter});
            if(mount>=2&&BackpackWeaponMountsPending){GUI.Label(new Rect(card.x+94,card.y+27,card.width-110,32),"미장착 · 백팩 장착 준비 중",small);continue;}
            BalanceWeapon[] allowed=Array.FindAll(balanceDraft.weapons,w=>BalanceData.CanEquip(w,mount,design.chassisId)&&!(mount>=2&&design.slots[mount==2?3:2]==w.id&&!BalanceData.CanDuplicateShoulderWeapon(w)));
            if(allowed.Length==0){GUI.Label(new Rect(card.x+84,card.y+27,card.width-100,32),"선택 가능한 장비 없음",small);continue;}
            int selected=Array.FindIndex(allowed,w=>w.id==design.slots[mount]);
            if(selected<0){selected=0;design.slots[mount]=allowed[0].id;balanceMountIndex=mount;balanceDirty=true;BalanceWeaponPreview(allowed[0]);RefreshBalanceDraftPreview();}
            Rect prev=new Rect(card.x+82,card.y+14,38,60),box=new Rect(card.x+124,card.y+8,348,72),next=new Rect(card.x+478,card.y+14,38,60);
            bool priorEnabled=GUI.enabled;GUI.enabled=priorEnabled&&allowed.Length>1;GUI.backgroundColor=new Color(.07f,.12f,.17f);
            if(GUI.Button(prev,"‹",new GUIStyle(GUI.skin.button){fontSize=27,fontStyle=FontStyle.Bold})){balanceMountIndex=mount;CycleLoadoutWeapon(design,allowed,(selected+allowed.Length-1)%allowed.Length);}
            if(GUI.Button(next,"›",new GUIStyle(GUI.skin.button){fontSize=27,fontStyle=FontStyle.Bold})){balanceMountIndex=mount;CycleLoadoutWeapon(design,allowed,(selected+1)%allowed.Length);}
            GUI.enabled=priorEnabled;GUI.backgroundColor=Color.white;Panel(box,new Color(.025f,.045f,.065f));
            BalanceWeapon weapon=allowed[selected];
            if(balanceWeaponIcons.TryGetValue(weapon.id,out Texture2D icon))GUI.DrawTexture(new Rect(box.x+5,box.y+4,box.width-10,box.height-8),icon,ScaleMode.ScaleToFit,true);
            else DrawBalanceWeaponMini(new Rect(box.x+5,box.y+4,box.width-10,box.height-8),weapon,true);
            DrawWeaponTraitGlyph(new Rect(box.x+box.width-24,box.y+box.height-23,17,17),weapon.traitIconId,Cyan);
        }
        for(int row=0;row<balanceCustomizationPartSlots.Length;row++)
        {
            string slot=balanceCustomizationPartSlots[row];Rect card=new Rect(604,164+row*58,604,52);Panel(card,new Color(.045f,.075f,.10f));
            Rect partLabel=new Rect(card.x+4,card.y+12,76,27);
            if(slot=="SKILL")
            {
                GUI.Label(partLabel,balanceCustomizationPartLabels[row],new GUIStyle(GUI.skin.label){fontSize=12,alignment=TextAnchor.MiddleCenter});
                int selected=Array.IndexOf(balanceSkillIds,design.skillId);if(selected<0)selected=Array.IndexOf(balanceSkillIds,BalanceData.DefaultSkillIdForChassis(design.chassisId));if(selected<0)selected=0;
                Rect prev=new Rect(card.x+82,card.y+5,38,42),box=new Rect(card.x+124,card.y+4,424,44),next=new Rect(card.x+552,card.y+5,38,42);
                bool priorEnabled=GUI.enabled;GUI.enabled=priorEnabled&&balanceSkillIds.Length>1;GUI.backgroundColor=new Color(.07f,.12f,.17f);
                if(GUI.Button(prev,"‹",new GUIStyle(GUI.skin.button){fontSize=24,fontStyle=FontStyle.Bold})){selected=(selected+balanceSkillIds.Length-1)%balanceSkillIds.Length;design.skillId=balanceSkillIds[selected];balanceDirty=true;ApplyBalanceSkill(player);}
                if(GUI.Button(next,"›",new GUIStyle(GUI.skin.button){fontSize=24,fontStyle=FontStyle.Bold})){selected=(selected+1)%balanceSkillIds.Length;design.skillId=balanceSkillIds[selected];balanceDirty=true;ApplyBalanceSkill(player);}
                GUI.enabled=priorEnabled;GUI.backgroundColor=Color.white;Panel(box,new Color(.025f,.045f,.065f));DrawWeaponTraitGlyph(new Rect(box.x+10,box.y+8,27,27),BalanceSkillGlyphId(balanceSkillIds[selected]),Cyan);
                GUI.Label(new Rect(box.x+48,box.y+2,box.width-56,box.height-4),BalanceSkillTitle(balanceSkillIds[selected]),new GUIStyle(GUI.skin.label){fontSize=14,alignment=TextAnchor.MiddleLeft});continue;
            }
            int side=slot=="RIGHT_ARM"?1:0;string category=slot=="LEFT_ARM"||slot=="RIGHT_ARM"?"ARMS":slot;int partIndex=BalanceAssemblyPartIndex(design,category,side);
            if(GUI.Button(partLabel,balanceCustomizationPartLabels[row],new GUIStyle(GUI.skin.button){fontSize=12}))FocusCustomizationPart(partIndex);
            if(partIndex<0){GUI.Label(new Rect(card.x+84,card.y+10,card.width-94,30),"선택 가능한 파츠 없음",small);continue;}
            BalancePart part=design.parts[partIndex];string[] visualIds=BalancePartVisualIds(part);int selectedVisual=Array.IndexOf(visualIds,part.visualId);
            if(selectedVisual<0){selectedVisual=0;BalanceData.SetPartVisualId(part,visualIds[0]);balanceDirty=true;}
            Rect prevArrow=new Rect(card.x+82,card.y+5,38,42),partBox=new Rect(card.x+124,card.y+4,424,44),nextArrow=new Rect(card.x+552,card.y+5,38,42);
            bool wasEnabled=GUI.enabled;GUI.enabled=wasEnabled&&visualIds.Length>1;GUI.backgroundColor=new Color(.07f,.12f,.17f);
            if(GUI.Button(prevArrow,"‹",new GUIStyle(GUI.skin.button){fontSize=24,fontStyle=FontStyle.Bold}))CycleUnifiedPart(part,visualIds,(selectedVisual+visualIds.Length-1)%visualIds.Length,partIndex);
            if(GUI.Button(nextArrow,"›",new GUIStyle(GUI.skin.button){fontSize=24,fontStyle=FontStyle.Bold}))CycleUnifiedPart(part,visualIds,(selectedVisual+1)%visualIds.Length,partIndex);
            GUI.enabled=wasEnabled;GUI.backgroundColor=Color.white;Panel(partBox,new Color(.025f,.045f,.065f));
            Color frameColor=CraftCatalog.Selectable.FirstOrDefault(c=>c.name==design.chassisId)?.color??Color.white;if(!string.IsNullOrEmpty(design.paintColorHex))ColorUtility.TryParseHtmlString("#"+design.paintColorHex,out frameColor);
            BalancePart preview=new BalancePart{frame=part.frame,category=part.category,visualId=visualIds[selectedVisual]};DrawBalancePartSilhouette(new Rect(partBox.x+155,partBox.y+2,112,40),preview,true,frameColor);
        }
    }
    void CycleUnifiedPart(BalancePart part,string[] visualIds,int index,int partIndex)
    {
        balancePartIndex=partIndex;CyclePartVisual(part,visualIds,index);
    }
    void FocusCustomizationPart(int partIndex)
    {
        BalanceDesign design=EditingDesign();if(design==null||partIndex<0||partIndex>=design.parts.Length)return;
        balancePartIndex=partIndex;portraitYaw=balancePreviewYaw=design.parts[partIndex].category=="BOOSTER"?180f:0f;RefreshBalanceDraftPreview();
    }
    void SetBalanceWeaponTrait(BalanceWeapon weapon,string id)
    {
        weapon.traitIconId=id;weapon.additionalTraitIconIds=Array.Empty<string>();
        if(id=="SHIELD"){weapon.hands=true;weapon.shoulders=false;weapon.left=weapon.right=true;}
        balanceDirty=true;
    }
    void ToggleBalanceWeaponTrait(BalanceWeapon weapon,string id)
    {
        if(id=="SHIELD"){SetBalanceWeaponTrait(weapon,id);return;}
        if(weapon.traitIconId=="SHIELD")weapon.traitIconId=id;
        else if(weapon.traitIconId==id)
        {
            string[] extras=weapon.additionalTraitIconIds??Array.Empty<string>();
            if(extras.Length>0){weapon.traitIconId=extras[0];weapon.additionalTraitIconIds=extras.Length==1?Array.Empty<string>():extras.Skip(1).ToArray();}
            else weapon.traitIconId="DAMAGE";
        }
        else
        {
            var extras=(weapon.additionalTraitIconIds??Array.Empty<string>()).ToList();
            if(extras.Contains(id))extras.Remove(id);else extras.Add(id);
            weapon.additionalTraitIconIds=extras.ToArray();
        }
        balanceDirty=true;
    }
    void DrawWeaponTraitIconPicker(BalanceWeapon weapon)
    {
        GUI.Label(new Rect(330,255,90,24),"특성 아이콘");
        if(weaponTraitCaption==null)weaponTraitCaption=new GUIStyle(small){fontSize=11,alignment=TextAnchor.MiddleCenter};
        for(int i=0;i<balanceWeaponTraitIds.Length;i++)
        {
            string id=balanceWeaponTraitIds[i];Rect tile=new Rect(420+i*66,250,60,50);bool selected=BalanceData.HasWeaponTrait(weapon,id);
            GUI.backgroundColor=selected?new Color(.10f,.42f,.50f):new Color(.07f,.12f,.17f);
            if(GUI.Button(tile,GUIContent.none))ToggleBalanceWeaponTrait(weapon,id);
            GUI.backgroundColor=Color.white;DrawWeaponTraitGlyph(new Rect(tile.x+21,tile.y+3,18,18),id,selected?Color.white:(weapon.family=="future"?Cyan:Orange));
            GUI.Label(new Rect(tile.x+2,tile.y+27,tile.width-4,18),WeaponTraitLabel(id),weaponTraitCaption);
        }
    }
    void DrawWeaponTraitGlyph(Rect r,string id,Color ink)
    {
        float x=r.x,y=r.y,w=r.width,h=r.height,cx=x+w*.5f,cy=y+h*.5f,t=Mathf.Max(1,w*.09f);
        if(id=="DAMAGE")
        {
            Panel(new Rect(cx-t*.5f,y+2*t,t,h-4*t),ink);Panel(new Rect(x+2*t,cy-t*.5f,w-4*t,t),ink);
            Panel(new Rect(x+2*t,y+2*t,t,t),ink);Panel(new Rect(x+w-3*t,y+2*t,t,t),ink);Panel(new Rect(x+2*t,y+h-3*t,t,t),ink);Panel(new Rect(x+w-3*t,y+h-3*t,t,t),ink);
        }
        else if(id=="LOCK")
        {
            float a=w*.32f,b=w*.68f;
            Panel(new Rect(x+2,y+2,b-a,t),ink);Panel(new Rect(x+2,y+2,t,b-a),ink);Panel(new Rect(x+w-b,y+2,b-a,t),ink);Panel(new Rect(x+w-t-b,y+2,t,b-a),ink);
            Panel(new Rect(x+2,y+h-t-2,b-a,t),ink);Panel(new Rect(x+2,y+h-b,t,b-a),ink);Panel(new Rect(x+w-b,y+h-t-2,b-a,t),ink);Panel(new Rect(x+w-t-b,y+h-b,t,b-a),ink);Panel(new Rect(cx-t,cy-t,t*2,t*2),ink);
        }
        else if(id=="RADAR"||id=="AREA")
        {
            float inset=id=="RADAR"?2:1;Panel(new Rect(x+inset,y+inset,w-2*inset,t),ink);Panel(new Rect(x+inset,y+h-inset-t,w-2*inset,t),ink);Panel(new Rect(x+inset,y+inset,t,h-2*inset),ink);Panel(new Rect(x+w-inset-t,y+inset,t,h-2*inset),ink);
            if(id=="RADAR"){Panel(new Rect(cx-t,cy-t,t*2,t*2),ink);Panel(new Rect(cx-2*t,cy-t*.5f,t,t),ink);Panel(new Rect(cx+t,cy-t*.5f,t,t),ink);}
            else{Panel(new Rect(cx-t*.5f,y+4*t,t,t),ink);Panel(new Rect(cx-t*.5f,y+h-5*t,t,t),ink);Panel(new Rect(x+4*t,cy-t*.5f,t,t),ink);Panel(new Rect(x+w-5*t,cy-t*.5f,t,t),ink);}
        }
        else if(id=="RAPID")
        {
            for(int i=0;i<3;i++){float yy=y+2*t+i*3.5f*t;Panel(new Rect(x+3*t,yy,w-6*t,t*1.2f),ink);}
        }
        else if(id=="RELOAD")
        {
            Panel(new Rect(x+3*t,y+2*t,w-6*t,t),ink);Panel(new Rect(x+3*t,y+h-3*t,w-6*t,t),ink);Panel(new Rect(x+4*t,y+3*t,t,h-6*t),ink);Panel(new Rect(x+w-5*t,y+3*t,t,h-6*t),ink);Panel(new Rect(cx-t*.5f,cy-t*.5f,t,t),ink);
        }
        else if(id=="JUMP")
        {
            Panel(new Rect(cx-t*.6f,y+3*t,t*1.2f,h-6*t),ink);Panel(new Rect(cx-w*.28f,y+3*t,w*.56f,t),ink);Panel(new Rect(x+3*t,y+h-3*t,w-6*t,t),ink);
        }
        else
        {
            Panel(new Rect(x+3*t,y+2*t,w-6*t,t),ink);Panel(new Rect(x+3*t,y+2*t,t,h-5*t),ink);Panel(new Rect(x+w-4*t,y+2*t,t,h-5*t),ink);Panel(new Rect(x+4*t,y+h-4*t,w-8*t,t),ink);Panel(new Rect(x+5*t,y+5*t,w-10*t,t),ink);
        }
    }
    void DrawBalancePartSilhouette(Rect r,BalancePart part,bool selected,Color chassisColor)
    {
        Matrix4x4 previousMatrix=GUI.matrix;Matrix4x4 pivot=Matrix4x4.TRS(new Vector3(r.x,r.y,0),Quaternion.identity,Vector3.one);GUI.matrix=previousMatrix*pivot*Matrix4x4.Scale(new Vector3(r.width/68f,r.height/52f,1))*pivot.inverse;r=new Rect(r.x,r.y,68,52);
        Color metal=new Color(.57f,.66f,.72f),dark=new Color(.10f,.16f,.21f),accent=selected?Cyan:Orange;
        Panel(r,new Color(.035f,.055f,.075f));
        if(part.category=="HEAD")
        {
            Panel(new Rect(r.x+16,r.y+7,31,24),metal);Panel(new Rect(r.x+21,r.y+11,21,7),chassisColor);Panel(new Rect(r.x+22,r.y+20,19,5),dark);
            if(part.visualId=="HEAD_RANGEFINDER"){Panel(new Rect(r.x+29,r.y+1,3,15),accent);Panel(new Rect(r.x+24,r.y+1,13,3),accent);}
            else if(part.visualId=="HEAD_OMNI"){Panel(new Rect(r.x+19,r.y+18,4,4),accent);Panel(new Rect(r.x+29,r.y+18,4,4),accent);Panel(new Rect(r.x+39,r.y+18,4,4),accent);Panel(new Rect(r.x+30,r.y+2,3,5),dark);}
            else {Panel(new Rect(r.x+29,r.y+21,8,2),accent);Panel(new Rect(r.x+30,r.y+3,3,5),accent);}
        }
        else if(part.category=="CORE")
        {
            Panel(new Rect(r.x+8,r.y+12,14,9),metal);Panel(new Rect(r.x+41,r.y+12,14,9),metal);Panel(new Rect(r.x+17,r.y+9,29,31),metal);Panel(new Rect(r.x+21,r.y+13,21,22),chassisColor);Panel(new Rect(r.x+27,r.y+23,10,5),accent);Panel(new Rect(r.x+21,r.y+37,21,4),dark);
            if(part.visualId=="CORE_REACTOR"){Panel(new Rect(r.x+23,r.y+17,17,15),dark);Panel(new Rect(r.x+27,r.y+20,9,9),accent);Panel(new Rect(r.x+29,r.y+22,5,5),Color.white);}
            else if(part.visualId=="CORE_BULWARK"){Panel(new Rect(r.x+12,r.y+10,7,28),dark);Panel(new Rect(r.x+44,r.y+10,7,28),dark);Panel(new Rect(r.x+20,r.y+8,23,4),accent);}
        }
        else if(part.category=="LEFT_ARM"||part.category=="RIGHT_ARM")
        {
            bool mirrored=part.category=="RIGHT_ARM";float sign=mirrored?-1:1;Panel(new Rect(r.x+28+sign*5,r.y+8,14,12),metal);Panel(new Rect(r.x+30+sign*7,r.y+12,10,6),chassisColor);Panel(new Rect(r.x+31+sign*9,r.y+21,10,19),metal);Panel(new Rect(r.x+33+sign*9,r.y+25,7,11),chassisColor);Panel(new Rect(r.x+29+sign*11,r.y+39,15,7),dark);Panel(new Rect(r.x+33+sign*11,r.y+40,7,3),accent);
            if(part.visualId=="ARM_REINFORCED")Panel(new Rect(r.x+27+sign*11,r.y+27,13,11),accent);
            else if(part.visualId=="ARM_PRECISION"){Panel(new Rect(r.x+29+sign*14,r.y+25,4,12),dark);Panel(new Rect(r.x+34+sign*14,r.y+25,4,12),accent);}
            else if(part.visualId=="ARM_ASSAULT"){Panel(new Rect(r.x+26+sign*12,r.y+25,18,13),accent);Panel(new Rect(r.x+28+sign*13,r.y+37,5,8),dark);Panel(new Rect(r.x+35+sign*13,r.y+37,5,8),dark);}
            else if(part.visualId=="ARM_BULWARK"){Panel(new Rect(r.x+24+sign*11,r.y+22,22,19),dark);Panel(new Rect(r.x+28+sign*12,r.y+25,14,12),accent);}
            else if(part.visualId=="ARM_SCOUT"){Panel(new Rect(r.x+31+sign*13,r.y+17,4,17),accent);Panel(new Rect(r.x+35+sign*13,r.y+14,4,7),dark);Panel(new Rect(r.x+29+sign*13,r.y+25,3,13),dark);}
            else if(part.visualId=="ARM_CLAW"){Panel(new Rect(r.x+30+sign*12,r.y+37,4,10),accent);Panel(new Rect(r.x+36+sign*12,r.y+37,4,10),accent);Panel(new Rect(r.x+42+sign*12,r.y+37,4,10),accent);}
            else if(part.visualId=="ARM_BRUISER"){Panel(new Rect(r.x+24+sign*13,r.y+23,24,18),dark);Panel(new Rect(r.x+27+sign*13,r.y+22,5,6),accent);Panel(new Rect(r.x+34+sign*13,r.y+22,5,6),accent);Panel(new Rect(r.x+41+sign*13,r.y+22,5,6),accent);Panel(new Rect(r.x+39+sign*17,r.y+34,8,10),metal);}
            else if(part.visualId=="ARM_VANGUARD"){Panel(new Rect(r.x+26+sign*12,r.y+17,22,23),metal);Panel(new Rect(r.x+29+sign*13,r.y+20,16,16),accent);Panel(new Rect(r.x+32+sign*14,r.y+24,10,4),dark);Panel(new Rect(r.x+31+sign*14,r.y+29,12,3),accent);}
            else if(part.visualId=="ARM_TACTICAL"){Panel(new Rect(r.x+30+sign*12,r.y+19,15,19),dark);Panel(new Rect(r.x+28+sign*12,r.y+18,3,18),accent);Panel(new Rect(r.x+45+sign*12,r.y+18,3,18),accent);Panel(new Rect(r.x+33+sign*12,r.y+39,11,4),metal);}
            else if(part.visualId=="ARM_EXOSKELETON"){Panel(new Rect(r.x+28+sign*12,r.y+21,17,8),dark);Panel(new Rect(r.x+28+sign*12,r.y+33,17,8),metal);Panel(new Rect(r.x+27+sign*12,r.y+29,3,5),accent);Panel(new Rect(r.x+43+sign*12,r.y+29,3,5),accent);}
        }
        else if(part.category=="LEGS")
        {
            Panel(new Rect(r.x+18,r.y+5,28,10),metal);Panel(new Rect(r.x+20,r.y+9,24,4),chassisColor);Panel(new Rect(r.x+21,r.y+15,9,22),metal);Panel(new Rect(r.x+34,r.y+15,9,22),metal);Panel(new Rect(r.x+17,r.y+37,15,6),dark);Panel(new Rect(r.x+31,r.y+37,15,6),dark);Panel(new Rect(r.x+23,r.y+21,5,5),accent);Panel(new Rect(r.x+36,r.y+21,5,5),accent);
            if(part.visualId=="LEGS_ASSAULT"){Panel(new Rect(r.x+20,r.y+25,11,13),accent);Panel(new Rect(r.x+33,r.y+25,11,13),accent);}
            else if(part.visualId=="LEGS_SCOUT"){Panel(new Rect(r.x+24,r.y+18,3,14),dark);Panel(new Rect(r.x+38,r.y+18,3,14),dark);Panel(new Rect(r.x+23,r.y+38,5,4),accent);Panel(new Rect(r.x+37,r.y+38,5,4),accent);}
            else if(part.visualId=="LEGS_RANGER"){Panel(new Rect(r.x+19,r.y+14,10,13),dark);Panel(new Rect(r.x+35,r.y+14,10,13),dark);Panel(new Rect(r.x+21,r.y+28,9,11),accent);Panel(new Rect(r.x+34,r.y+28,9,11),accent);}
            else if(part.visualId=="LEGS_HEAVY"){Panel(new Rect(r.x+16,r.y+13,15,23),accent);Panel(new Rect(r.x+33,r.y+13,15,23),accent);Panel(new Rect(r.x+21,r.y+38,9,5),dark);Panel(new Rect(r.x+34,r.y+38,9,5),dark);}
            else if(part.visualId=="LEGS_JUMPER"){Panel(new Rect(r.x+22,r.y+16,5,19),dark);Panel(new Rect(r.x+39,r.y+16,5,19),dark);Panel(new Rect(r.x+21,r.y+21,7,3),accent);Panel(new Rect(r.x+38,r.y+21,7,3),accent);Panel(new Rect(r.x+21,r.y+28,7,3),accent);Panel(new Rect(r.x+38,r.y+28,7,3),accent);}
            else if(part.visualId=="LEGS_REVERSE"){Panel(new Rect(r.x+19,r.y+17,11,18),dark);Panel(new Rect(r.x+34,r.y+17,11,18),dark);Panel(new Rect(r.x+18,r.y+37,13,7),accent);Panel(new Rect(r.x+33,r.y+37,13,7),accent);}
        }
        else if(part.category=="BOOSTER")
        {
            if(part.visualId=="BOOSTER_CYCLONE")
            {Panel(new Rect(r.x+16,r.y+7,33,27),metal);Panel(new Rect(r.x+20,r.y+10,25,21),dark);Panel(new Rect(r.x+24,r.y+14,17,13),chassisColor);Panel(new Rect(r.x+29,r.y+18,7,6),accent);Panel(new Rect(r.x+19,r.y+35,6,8),dark);Panel(new Rect(r.x+40,r.y+35,6,8),dark);}
            else if(part.visualId=="BOOSTER_VECTOR")
            {Panel(new Rect(r.x+20,r.y+11,24,25),metal);Panel(new Rect(r.x+23,r.y+14,18,18),chassisColor);Panel(new Rect(r.x+14,r.y+5,7,32),accent);Panel(new Rect(r.x+43,r.y+5,7,32),accent);Panel(new Rect(r.x+21,r.y+37,8,7),dark);Panel(new Rect(r.x+35,r.y+37,8,7),dark);}
            else
            {Panel(new Rect(r.x+18,r.y+7,29,26),metal);Panel(new Rect(r.x+22,r.y+11,21,17),chassisColor);Panel(new Rect(r.x+16,r.y+33,11,8),dark);Panel(new Rect(r.x+38,r.y+33,11,8),dark);Panel(new Rect(r.x+19,r.y+39,5,5),accent);Panel(new Rect(r.x+41,r.y+39,5,5),accent);}
        }
        else
        {
            Panel(new Rect(r.x+17,r.y+9,30,31),metal);Panel(new Rect(r.x+21,r.y+13,22,23),chassisColor);Panel(new Rect(r.x+28,r.y+22,9,6),accent);
        }
        GUI.matrix=previousMatrix;
    }
    void DrawBalanceParts()
    {
        BalanceDesign design=EditingDesign();if(design==null||design.parts==null||design.parts.Length==0)return;
        if(balancePartIndex<0||balancePartIndex>=design.parts.Length||design.parts[balancePartIndex].frame!=design.chassisId)balancePartIndex=BalanceAssemblyPartIndex(design,"HEAD");
        balanceListScroll=GUI.BeginScrollView(new Rect(24,118,285,475),balanceListScroll,new Rect(0,0,258,408));
        for(int slotIndex=0;slotIndex<balanceAssemblySlotIds.Length;slotIndex++)
        {
            string slot=balanceAssemblySlotIds[slotIndex];int col=slotIndex%2,rowIndex=slotIndex/2;Rect tile=new Rect(col*126,rowIndex*136,120,128);
            if(slot=="SKILL")
            {
                bool skillSelected=balanceTab==5;GUI.backgroundColor=skillSelected?new Color(.08f,.43f,.48f):new Color(.07f,.12f,.17f);bool clicked=GUI.Button(tile,GUIContent.none);GUI.backgroundColor=Color.white;if(skillSelected)Panel(new Rect(tile.x,tile.y,tile.width,3),Cyan);Panel(new Rect(tile.x+26,tile.y+17,68,58),new Color(.035f,.055f,.075f));DrawWeaponTraitGlyph(new Rect(tile.x+47,tile.y+32,26,26),BalanceSkillGlyphId(design.skillId),skillSelected?Cyan:Orange);GUI.Label(new Rect(tile.x+4,tile.y+84,tile.width-8,24),balanceAssemblySlotLabels[slotIndex],weaponTraitCaption??small);if(clicked){balanceTab=5;balanceValueScroll=Vector2.zero;}continue;
            }
            int i=slot=="ARMS"&&design.parts[balancePartIndex].category=="RIGHT_ARM"?BalanceAssemblyPartIndex(design,slot,1):BalanceAssemblyPartIndex(design,slot);if(i<0)continue;BalancePart part=design.parts[i];bool selected=BalanceAssemblyCategory(design.parts[balancePartIndex])==slot;
            GUI.backgroundColor=selected?new Color(.08f,.43f,.48f):new Color(.07f,.12f,.17f);
            if(GUI.Button(tile,GUIContent.none)){balanceValueScroll=Vector2.zero;FocusCustomizationPart(i);}
            GUI.backgroundColor=Color.white;if(selected)Panel(new Rect(tile.x,tile.y,tile.width,3),Cyan);
            Color frameColor=CraftCatalog.Selectable.FirstOrDefault(x=>x.name==design.chassisId)?.color??Color.white;if(!string.IsNullOrEmpty(design.paintColorHex))ColorUtility.TryParseHtmlString("#"+design.paintColorHex,out frameColor);DrawBalancePartSilhouette(new Rect(tile.x+26,tile.y+13,68,58),part,selected,frameColor);
            DrawWeaponTraitGlyph(new Rect(tile.x+49,tile.y+76,20,20),part.partTraitIconId,selected?Cyan:Orange);GUI.Label(new Rect(tile.x+4,tile.y+99,tile.width-8,23),balanceAssemblySlotLabels[slotIndex],weaponTraitCaption??small);
        }
        GUI.EndScrollView();if(balanceTab==5)return;if(balancePartIndex<0||balancePartIndex>=design.parts.Length)return;BalancePart p=design.parts[balancePartIndex];GUI.Label(new Rect(330,120,605,35),design.id+" / "+p.frame+" / "+p.name+" / "+p.id);
        if(p.category=="LEFT_ARM"||p.category=="RIGHT_ARM")
        {
            bool left=p.category=="LEFT_ARM";GUI.backgroundColor=left?new Color(.10f,.42f,.50f):new Color(.07f,.12f,.17f);if(GUI.Button(new Rect(952,120,130,28),"왼팔")){balancePartIndex=BalanceAssemblyPartIndex(design,"ARMS",0);balanceValueScroll=Vector2.zero;}GUI.backgroundColor=!left?new Color(.10f,.42f,.50f):new Color(.07f,.12f,.17f);if(GUI.Button(new Rect(1088,120,140,28),"오른팔")){balancePartIndex=BalanceAssemblyPartIndex(design,"ARMS",1);balanceValueScroll=Vector2.zero;}GUI.backgroundColor=Color.white;
        }
        int[] variants=Enumerable.Range(0,design.parts.Length).Where(i=>design.parts[i]!=null&&design.parts[i].frame==design.chassisId&&design.parts[i].category==p.category).ToArray();
        if(variants.Length>0)
        {
            int currentVariant=Array.IndexOf(variants,balancePartIndex);if(currentVariant<0){balancePartIndex=variants[0];currentVariant=0;}
            Rect leftArrow=new Rect(430,190,58,58),variantBox=new Rect(492,190,670,58),rightArrow=new Rect(1166,190,58,58);
            GUI.enabled=variants.Length>1;GUI.backgroundColor=new Color(.07f,.12f,.17f);
            if(GUI.Button(leftArrow,"‹",new GUIStyle(GUI.skin.button){fontSize=30,fontStyle=FontStyle.Bold}))
            {balancePartIndex=variants[(currentVariant+variants.Length-1)%variants.Length];balanceValueScroll=Vector2.zero;}
            if(GUI.Button(rightArrow,"›",new GUIStyle(GUI.skin.button){fontSize=30,fontStyle=FontStyle.Bold}))
            {balancePartIndex=variants[(currentVariant+1)%variants.Length];balanceValueScroll=Vector2.zero;}
            GUI.enabled=true;GUI.backgroundColor=Color.white;Panel(variantBox,new Color(.045f,.075f,.10f));
            BalancePart option=design.parts[balancePartIndex];Color variantColor=CraftCatalog.Selectable.FirstOrDefault(x=>x.name==design.chassisId)?.color??Color.white;
            DrawBalancePartSilhouette(new Rect(variantBox.x+8,variantBox.y+3,84,52),option,true,variantColor);
            GUI.Label(new Rect(variantBox.x+104,variantBox.y+4,variantBox.width-112,variantBox.height-8),option.name,new GUIStyle(GUI.skin.label){fontSize=16,alignment=TextAnchor.MiddleLeft,clipping=TextClipping.Clip});
        }
        GUI.Label(new Rect(330,164,70,25),"메모");BalanceText(ref p.notes,new Rect(405,160,830,30),2000);
        if(p.category=="HEAD"||p.category=="CORE"||p.category=="LEFT_ARM"||p.category=="RIGHT_ARM"||p.category=="LEGS"||p.category=="BOOSTER")DrawPartVisualPicker(p);
        CraftSpec selectedLegFrame=p.category=="LEGS"?CraftCatalog.Selectable.FirstOrDefault(c=>c.name==p.frame):null;bool expandedBipedLegPicker=selectedLegFrame!=null&&(selectedLegFrame.mobility==MobilityKind.StandardBiped||selectedLegFrame.mobility==MobilityKind.Runner);bool expandedArmPicker=p.category=="LEFT_ARM"||p.category=="RIGHT_ARM";
        float traitPickerY=expandedArmPicker||expandedBipedLegPicker?320:266;DrawPartTraitIconPicker(p,traitPickerY);float statsY=traitPickerY+90;
        GUI.Label(new Rect(330,traitPickerY+48,900,37),"아이콘 "+WeaponTraitLabel(p.partTraitIconId)+" · "+BalanceTraitMeaning(p.partTraitIconId)+"\n적용 수치  "+BalancePartPerformanceSummary(p),new GUIStyle(small){fontSize=12,wordWrap=true});
        DrawBalanceNumbers(design.id+"/"+p.id,p.numbers,new Rect(330,statsY,924,594-statsY));
        if(p.category=="CORE"&&GUI.Button(new Rect(24,600,285,38),"+ 몸통 파츠 변형 추가"))BalanceAction(CreateCustomPartFromEditor);
    }
    static readonly string[] balancePartPaintHex={"","E8E5DA","F2782B","40B7C8","82964C","687887","8B68B7"};
    static readonly string[] balancePartPaintNames={"기본","상아","주황","청록","올리브","강철","보라"};
    void DrawDesignPaintPalette(BalanceDesign design)
    {
        GUI.Label(new Rect(24,272,84,25),"외장 색상");if(weaponTraitCaption==null)weaponTraitCaption=new GUIStyle(small){fontSize=10,alignment=TextAnchor.MiddleCenter};
        Color chassisColor=CraftCatalog.Selectable.FirstOrDefault(x=>x.name==design.chassisId)?.color??Color.white;
        for(int i=0;i<balancePartPaintHex.Length;i++)
        {
            string hex=balancePartPaintHex[i];Color swatch=chassisColor;if(!string.IsNullOrEmpty(hex))ColorUtility.TryParseHtmlString("#"+hex,out swatch);
            float x=105+i*64;Rect tile=new Rect(x,268,60,34);bool selected=(design.paintColorHex??"")==hex;GUI.backgroundColor=selected?new Color(.10f,.30f,.34f):new Color(.06f,.10f,.14f);
            if(GUI.Button(tile,GUIContent.none)){design.paintColorHex=hex;balanceDirty=true;RefreshBalanceDraftPreview();balanceMessage="기체 외장 색상을 미리보기에 반영했습니다.";}
            GUI.backgroundColor=Color.white;if(selected)Panel(new Rect(x,268,60,2),Cyan);Panel(new Rect(x+4,272,15,25),swatch);GUI.Label(new Rect(x+20,272,39,25),balancePartPaintNames[i],weaponTraitCaption);
        }
    }
    void DrawPartTraitIconPicker(BalancePart part,float y)
    {
        GUI.Label(new Rect(330,y+5,90,24),"특성 아이콘");if(weaponTraitCaption==null)weaponTraitCaption=new GUIStyle(small){fontSize=11,alignment=TextAnchor.MiddleCenter};
        string[] partTraitIds={"DAMAGE","LOCK","RADAR","AREA","RAPID","RELOAD","JUMP","SHIELD"};
        for(int i=0;i<partTraitIds.Length;i++)
        {
            string id=partTraitIds[i];Rect tile=new Rect(420+i*66,y,60,42);bool selected=part.partTraitIconId==id;GUI.backgroundColor=selected?new Color(.10f,.42f,.50f):new Color(.07f,.12f,.17f);
            if(GUI.Button(tile,GUIContent.none)){part.partTraitIconId=id;balanceDirty=true;}GUI.backgroundColor=Color.white;DrawWeaponTraitGlyph(new Rect(tile.x+21,tile.y+2,18,18),id,selected?Color.white:Orange);GUI.Label(new Rect(tile.x+2,tile.y+22,tile.width-4,17),WeaponTraitLabel(id),weaponTraitCaption);
        }
    }
    void DrawPartVisualPicker(BalancePart part)
    {
        bool head=part.category=="HEAD",core=part.category=="CORE",arm=part.category=="LEFT_ARM"||part.category=="RIGHT_ARM",legs=part.category=="LEGS";GUI.Label(new Rect(330,260,86,25),head?"머리":core?"몸통":arm?"팔":legs?"다리":"백팩");
        CraftSpec frame=legs?CraftCatalog.Selectable.FirstOrDefault(c=>c.name==part.frame):null;bool bipedLegs=frame!=null&&(frame.mobility==MobilityKind.StandardBiped||frame.mobility==MobilityKind.Runner);
        string[] ids=head?new[]{"HEAD_STANDARD","HEAD_RANGEFINDER","HEAD_OMNI"}:core?new[]{"CORE_STANDARD","CORE_REACTOR","CORE_BULWARK"}:arm?new[]{"ARM_STANDARD","ARM_REINFORCED","ARM_PRECISION","ARM_ASSAULT","ARM_BULWARK","ARM_SCOUT","ARM_CLAW","ARM_BRUISER","ARM_VANGUARD","ARM_TACTICAL","ARM_EXOSKELETON"}:legs?(bipedLegs?new[]{"LEGS_STANDARD","LEGS_ASSAULT","LEGS_SCOUT","LEGS_RANGER","LEGS_HEAVY","LEGS_JUMPER","LEGS_REVERSE"}:new[]{"LEGS_STANDARD","LEGS_ASSAULT","LEGS_SCOUT"}):new[]{"BOOSTER_TWIN","BOOSTER_VECTOR","BOOSTER_CYCLONE"};
        string[] labels=head?new[]{"기본 센서","장거리 안테나","옴니 어레이"}:core?new[]{"기본 코어","반응로 장갑" ,"중장갑 프레임"}:arm?new[]{"기본 팔","보강 건틀릿","정밀 서보","돌격 건틀릿","바스티온 장갑","정찰 센서","클로 암","브루저","뱅가드","택티컬","외골격 피스톤"}:legs?(bipedLegs?new[]{"기본 하체","돌격 그리브","정찰 관절","레인저 장갑","헤비 아머","점프 피스톤","역관절"}:new[]{"기본 하체","돌격 그리브","정찰 관절"}):new[]{"쌍발 표준","벡터 윙","사이클론 터빈"};
        int selectedIndex=Array.IndexOf(ids,part.visualId);if(selectedIndex<0){selectedIndex=0;BalanceData.SetPartVisualId(part,ids[0]);balanceDirty=true;RefreshBalanceDraftPreview();}
        Rect leftArrow=new Rect(420,258,58,58),previewBox=new Rect(482,258,650,58),rightArrow=new Rect(1138,258,58,58);
        GUI.enabled=ids.Length>1;GUI.backgroundColor=new Color(.07f,.12f,.17f);
        if(GUI.Button(leftArrow,"‹",new GUIStyle(GUI.skin.button){fontSize=30,fontStyle=FontStyle.Bold}))CyclePartVisual(part,ids,(selectedIndex+ids.Length-1)%ids.Length);
        if(GUI.Button(rightArrow,"›",new GUIStyle(GUI.skin.button){fontSize=30,fontStyle=FontStyle.Bold}))CyclePartVisual(part,ids,(selectedIndex+1)%ids.Length);
        GUI.enabled=true;GUI.backgroundColor=Color.white;Panel(previewBox,new Color(.045f,.075f,.10f));
        BalancePart selectedPreview=new BalancePart{frame=part.frame,category=part.category,visualId=ids[selectedIndex]};Color frameColor=CraftCatalog.Selectable.FirstOrDefault(c=>c.name==part.frame)?.color??Color.white;
        if(!string.IsNullOrEmpty(EditingDesign()?.paintColorHex))ColorUtility.TryParseHtmlString("#"+EditingDesign().paintColorHex,out frameColor);
        DrawBalancePartSilhouette(new Rect(previewBox.x+6,previewBox.y+3,86,52),selectedPreview,true,frameColor);
        GUI.Label(new Rect(previewBox.x+120,previewBox.y+4,previewBox.width-130,previewBox.height-8),labels[selectedIndex],new GUIStyle(GUI.skin.label){fontSize=16,alignment=TextAnchor.MiddleLeft});
    }
    void CyclePartVisual(BalancePart part,string[] ids,int index)
    {
        if(part==null||ids==null||ids.Length<2)return;BalanceData.SetPartVisualId(part,ids[Mathf.Clamp(index,0,ids.Length-1)]);portraitYaw=balancePreviewYaw=part.category=="BOOSTER"?180f:0f;balanceDirty=true;RefreshBalanceDraftPreview();balanceMessage="외형 적용";
    }
    void CreateCustomFirearmFromEditor()
    {
        int serial=1;string id;do{id="CUSTOM_FIREARM_"+serial.ToString("00");serial++;}while(balanceDraft.weapons.Any(w=>w.id==id));
        BalanceWeapon added=BalanceData.AddCustomFirearm(balanceDraft,id,"사용자 화기 "+(serial-1).ToString("00"));balanceWeaponIndex=Array.IndexOf(balanceDraft.weapons,added);balanceDirty=true;BalanceSelectPreview();balanceMessage=id+" 등록됨. 성능을 편집한 뒤 파일 저장을 누르세요.";
    }
    void CreateCustomPartFromEditor()
    {
        BalanceDesign design=EditingDesign();if(design==null)throw new InvalidDataException("먼저 설계 슬롯을 선택하세요.");int serial=1;string id;do{id="CUSTOM_PART_"+design.chassisId+"_"+serial.ToString("00");serial++;}while(balanceDraft.parts.Any(p=>p.id==id));
        BalancePart added=BalanceData.AddCustomPart(balanceDraft,design.id,id,"사용자 몸통 파츠 "+(serial-1).ToString("00"));balancePartIndex=Array.IndexOf(design.parts,added);if(balancePartIndex<0)balancePartIndex=Array.FindIndex(design.parts,p=>p.id==id);balanceListScroll.y=Mathf.Max(0,balancePartIndex*35-400);balanceDirty=true;balanceMessage=id+" 등록됨. 설계별 수치를 편집한 뒤 파일 저장을 누르세요.";
    }
    void DrawBalanceNumbers(string owner,BalanceNumber[] values,Rect area)
    {
        balanceValueScroll=GUI.BeginScrollView(area,balanceValueScroll,new Rect(0,0,895,values.Length*36+4));
        for(int i=0;i<values.Length;i++)
        {
            BalanceNumber n=values[i];string key=owner+"/"+n.key;float y=i*36;
            Color old=GUI.color;GUI.color=n.implemented?Color.white:new Color(1,.70f,.32f);GUI.Label(new Rect(5,y+3,190,27),n.label+(n.implemented?"":" [보관]"));GUI.color=old;
            int stage=n.max>n.min?Mathf.Clamp(Mathf.RoundToInt(Mathf.InverseLerp(n.min,n.max,n.value)*5f),0,5):0;
            int nextStage=Mathf.Clamp(Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(198,y+8,280,20),stage,0,5)),0,5);
            if(nextStage!=stage)
            {
                float value=Mathf.Lerp(n.min,n.max,nextStage/5f);if(n.integer)value=Mathf.Round(value);
                n.value=Mathf.Clamp(value,n.min,n.max);balanceNumberText[key]=n.value.ToString("0.#####",CultureInfo.InvariantCulture);balanceInvalidNumbers.Remove(key);balanceDirty=true;
            }
            GUI.Label(new Rect(484,y+3,58,27),stage+" / 5",small);
            string displayedValue=(n.key=="speed"?n.value*2f:n.value).ToString(n.integer?"0":"0.###",CultureInfo.InvariantCulture);
            GUI.Label(new Rect(545,y+3,174,27),displayedValue+" "+n.unit+(n.key=="speed"?" (×2)":""),small);
            if(GUI.Button(new Rect(728,y+2,118,28),"Min / Max"))
            {
                balanceRangeEditKey=key;balanceRangeEditTarget=n;balanceRangeMinText=n.min.ToString("0.#####",CultureInfo.InvariantCulture);balanceRangeMaxText=n.max.ToString("0.#####",CultureInfo.InvariantCulture);
            }
        }
        GUI.EndScrollView();
        if(balanceRangeEditTarget!=null&&balanceRangeEditKey.StartsWith(owner+"/",StringComparison.Ordinal))
        {
            Rect window=new Rect(Mathf.Clamp(area.x+area.width-342,8,940),Mathf.Clamp(area.y+Mathf.Min(38,area.height-150),88,540),334,146);
            Panel(window,new Color(.025f,.045f,.075f,.99f));
            GUI.Window(78231,window,DrawBalanceNumberRangeWindow,"수치 범위 설정");
        }
    }
    void DrawBalanceNumberRangeWindow(int windowId)
    {
        BalanceNumber number=balanceRangeEditTarget;if(number==null){GUI.DragWindow(new Rect(0,0,334,22));return;}
        GUI.Label(new Rect(12,28,94,24),"최솟값");balanceRangeMinText=GUI.TextField(new Rect(102,27,92,26),balanceRangeMinText,24);
        GUI.Label(new Rect(196,28,42,24),"최댓값");balanceRangeMaxText=GUI.TextField(new Rect(238,27,84,26),balanceRangeMaxText,24);
        GUI.Label(new Rect(12,57,310,22),"안전 범위  "+number.safeMin.ToString("0.###")+" ~ "+number.safeMax.ToString("0.###"),small);
        if(GUI.Button(new Rect(102,91,105,30),"적용"))
        {
            float min,max;bool parsedMin=float.TryParse(balanceRangeMinText,NumberStyles.Float,CultureInfo.InvariantCulture,out min);bool parsedMax=float.TryParse(balanceRangeMaxText,NumberStyles.Float,CultureInfo.InvariantCulture,out max);
            bool valid=parsedMin&&parsedMax&&!float.IsNaN(min)&&!float.IsInfinity(min)&&!float.IsNaN(max)&&!float.IsInfinity(max)&&min>=number.safeMin&&max<=number.safeMax&&min<=max;
            if(valid)
            {
                float stage=number.max>number.min?Mathf.InverseLerp(number.min,number.max,number.value):0f;number.min=min;number.max=max;
                float value=Mathf.Lerp(min,max,stage);if(number.integer)value=Mathf.Round(value);number.value=Mathf.Clamp(value,min,max);
                string ownerKey=balanceRangeEditKey;balanceNumberText[ownerKey]=number.value.ToString("0.#####",CultureInfo.InvariantCulture);balanceInvalidNumbers.Remove(ownerKey);balanceDirty=true;balanceRangeEditKey="";balanceRangeEditTarget=null;
            }
            else balanceInvalidNumbers.Add(balanceRangeEditKey);
        }
        if(GUI.Button(new Rect(215,91,105,30),"닫기")){balanceInvalidNumbers.Remove(balanceRangeEditKey);balanceRangeEditKey="";balanceRangeEditTarget=null;}
        GUI.DragWindow(new Rect(0,0,334,22));
    }
    void BalanceText(ref string text,Rect rect,int max){string value=GUI.TextField(rect,text??"",max);if(value!=text){balanceDirty=true;text=value;}}
    void DrawBalanceLoadout()
    {
        BalanceDesign design=EditingDesign();if(design==null)return;
        GUI.Label(new Rect(24,151,88,28),"설계 이름");if(design.isEmpty)GUI.Label(new Rect(112,148,220,27),"빈 슬롯");else BalanceText(ref design.name,new Rect(112,148,220,27),80);
        if(design.isEmpty){GUI.Label(new Rect(24,222,530,74),"빈 설계 슬롯입니다. 위에서 기체를 선택하면 해당 기체의 기본 장비와 파츠로 새 설계를 시작합니다.",new GUIStyle(GUI.skin.label){wordWrap=true,fontSize=15});return;}
        if(balanceMountIndex<0||balanceMountIndex>=design.slots.Length)return;
        GUI.Label(new Rect(24,212,52,52),"설명");Color priorBackground=GUI.backgroundColor;GUI.backgroundColor=new Color(.06f,.085f,.11f);
        string description=GUI.TextArea(new Rect(78,210,476,54),design.description??"",GUI.skin.textArea);GUI.backgroundColor=priorBackground;
        if(description.Length>1000)description=description.Substring(0,1000);if(description!=design.description){design.description=description;balanceDirty=true;}
        DrawDesignPaintPalette(design);
        BalanceWeapon[] allowed=Array.FindAll(balanceDraft.weapons,w=>BalanceData.CanEquip(w,balanceMountIndex,design.chassisId)&&!(balanceMountIndex>=2&&design.slots[balanceMountIndex==2?3:2]==w.id&&!BalanceData.CanDuplicateShoulderWeapon(w)));
        if(allowed.Length>0)
        {
            int selectedIndex=Array.FindIndex(allowed,w=>w.id==design.slots[balanceMountIndex]);
            if(selectedIndex<0){selectedIndex=0;design.slots[balanceMountIndex]=allowed[0].id;balanceDirty=true;BalanceWeaponPreview(allowed[0]);RefreshBalanceDraftPreview();}
            Rect leftArrow=new Rect(24,346,58,146),weaponBox=new Rect(86,346,400,146),rightArrow=new Rect(490,346,58,146);
            GUI.enabled=allowed.Length>1;GUI.backgroundColor=new Color(.07f,.12f,.17f);
            if(GUI.Button(leftArrow,"‹",new GUIStyle(GUI.skin.button){fontSize=36,fontStyle=FontStyle.Bold}))CycleLoadoutWeapon(design,allowed,(selectedIndex+allowed.Length-1)%allowed.Length);
            if(GUI.Button(rightArrow,"›",new GUIStyle(GUI.skin.button){fontSize=36,fontStyle=FontStyle.Bold}))CycleLoadoutWeapon(design,allowed,(selectedIndex+1)%allowed.Length);
            GUI.enabled=true;GUI.backgroundColor=Color.white;Panel(weaponBox,new Color(.045f,.075f,.10f));
            BalanceWeapon weapon=allowed[selectedIndex];
            if(balanceWeaponIcons.TryGetValue(weapon.id,out Texture2D iconImage))GUI.DrawTexture(new Rect(weaponBox.x+12,weaponBox.y+8,weaponBox.width-24,weaponBox.height-16),iconImage,ScaleMode.ScaleToFit);
            else DrawBalanceWeaponMini(new Rect(weaponBox.x+12,weaponBox.y+8,weaponBox.width-24,weaponBox.height-16),weapon,true);
            DrawWeaponTraitGlyph(new Rect(weaponBox.x+weaponBox.width-30,weaponBox.y+weaponBox.height-30,22,22),weapon.traitIconId,Cyan);
        }
        else GUI.Label(new Rect(42,362,490,90),"장착 가능한 항목 없음",new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter});
        if(balancePreviewUnavailable)GUI.Label(new Rect(575,210,300,180),"맨손 공격\n장착물이 없는 손은 기존 주먹 동작을 사용합니다.");
        else if(weaponPreviewTexture!=null)GUI.DrawTexture(new Rect(575,205,300,210),weaponPreviewTexture,ScaleMode.ScaleToFit);
        BalanceWeapon current=Array.Find(balanceDraft.weapons,w=>w.id==design.slots[balanceMountIndex]);
        if(current!=null)
        {
            Panel(new Rect(575,420,630,152),new Color(.045f,.085f,.11f));
            Panel(new Rect(575,420,5,152),new Color(.10f,.78f,.82f));
            GUI.Label(new Rect(596,426,365,28),current.name,new GUIStyle(GUI.skin.label){fontSize=19,fontStyle=FontStyle.Bold});
            string traits=string.Join("+",new[]{current.traitIconId}.Concat(current.additionalTraitIconIds??Array.Empty<string>()));
            GUI.Label(new Rect(596,454,365,22),"특성 "+traits+" · "+BalanceTraitMeaning(current.traitIconId),small);
            GUI.Label(new Rect(596,480,365,48),current.notes??"",new GUIStyle(GUI.skin.label){wordWrap=true});
            if(balanceMountIndex>=2&&design.slots.Take(2).Select(id=>Array.Find(balanceDraft.weapons,w=>w.id==id)).Any(w=>w!=null&&w.blocksShoulderWeapons))GUI.Label(new Rect(575,395,620,22),"손에 든 중화기 때문에 어깨 무기는 전투 중 비활성화됩니다.",new GUIStyle(small){normal={textColor=Orange}});
            if(current.kind=="firearm")
            {
                DrawLoadoutStatBar(975,432,"DAMAGE",current.Get("damage"),Mathf.Max(50,current.Get("damage")),Color.cyan);
                DrawLoadoutStatBar(975,466,"AMMO",current.Get("capacity"),Mathf.Max(60,current.Get("capacity")),new Color(.98f,.54f,.20f));
                DrawLoadoutStatBar(975,500,"RANGE",current.Get("range"),Mathf.Max(120,current.Get("range")),new Color(.42f,.76f,1f));
                GUI.Label(new Rect(975,534,205,23),"RELOAD  "+current.Get("emptyReload").ToString("0.0")+" s",small);
            }
            else GUI.Label(new Rect(975,474,220,30),current.kind=="shield"||BalanceData.HasWeaponTrait(current,"SHIELD")?"GUARD / 우클릭 방어":"UNARMED / 주먹 공격",small);
            if(GUI.Button(new Rect(575,579,260,34),"선택 장비 옵션 편집")){balanceWeaponIndex=Mathf.Max(0,Array.FindIndex(balanceDraft.weapons,w=>w.id==current.id));balanceTab=1;balanceValueScroll=Vector2.zero;BalanceSelectPreview();}
        }
    }
    void CycleLoadoutWeapon(BalanceDesign design,BalanceWeapon[] allowed,int index)
    {
        if(design==null||allowed==null||allowed.Length<2||balanceMountIndex<0||balanceMountIndex>=design.slots.Length)return;
        BalanceWeapon weapon=allowed[Mathf.Clamp(index,0,allowed.Length-1)];design.slots[balanceMountIndex]=weapon.id;balanceDirty=true;BalanceWeaponPreview(weapon);RefreshBalanceDraftPreview();
    }
    void DrawLoadoutStatBar(float x,float y,string title,float value,float max,Color accent)
    {
        GUI.Label(new Rect(x,y,66,22),title,new GUIStyle(GUI.skin.label){fontSize=11});
        Panel(new Rect(x+68,y+5,112,10),new Color(.12f,.17f,.21f));
        Panel(new Rect(x+68,y+5,112*Mathf.Clamp01(value/Mathf.Max(1,max)),10),accent);
        GUI.Label(new Rect(x+184,y,44,22),value.ToString("0.#"),new GUIStyle(GUI.skin.label){fontSize=11,alignment=TextAnchor.MiddleRight});
    }    System.Collections.IEnumerator CacheBalanceWeaponIcons()
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
