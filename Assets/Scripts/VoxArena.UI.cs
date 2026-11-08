using System;
using UnityEngine;

public sealed partial class VoxArena
{
    bool showTuning;
    void Panel(Rect r, Color color) { GUI.color = color; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white; }
    readonly System.Collections.Generic.Dictionary<GUIStyle,GUIStyle> localizedBalanceLabels=new System.Collections.Generic.Dictionary<GUIStyle,GUIStyle>();
    void Text(float x,float y,float w,string value,GUIStyle style)
    {
        bool localized=false;if(value!=null)foreach(char c in value)if(c>=0x2E80){localized=true;break;}
        if(localized)
        {
            if(balanceFont==null)balanceFont=Font.CreateDynamicFontFromOSFont(new[]{"Malgun Gothic","맑은 고딕","Arial"},17);
            if(!localizedBalanceLabels.TryGetValue(style,out GUIStyle translated)){translated=new GUIStyle(style){font=balanceFont};localizedBalanceLabels.Add(style,translated);}style=translated;
        }
        GUI.Label(new Rect(x,y,w,35),value,style);
    }
    void Bar(float x, float y, float w, float fraction, Color color)
    {
        Panel(new Rect(x, y, w, 8), new Color(.12f, .18f, .24f));
        Panel(new Rect(x, y, w * Mathf.Clamp01(fraction), 8), color);
    }
    void HudBar(float x,float y,float w,float fraction,Color accent)
    {
        float fill=w*Mathf.Clamp01(fraction);
        Panel(new Rect(x-2,y-2,w+4,10),new Color(.006f,.016f,.027f,.94f));
        Panel(new Rect(x,y,w,6),new Color(.11f,.16f,.21f,.96f));
        if(fill>0)
        {
            Panel(new Rect(x,y,fill,6),accent);
            Color glint=Color.Lerp(accent,Color.white,.3f);glint.a=.72f;
            Panel(new Rect(x,y,fill,1),glint);
        }
        for(int segment=1;segment<4;segment++)
            Panel(new Rect(x+w*segment*.25f-1,y-1,2,8),new Color(.012f,.027f,.039f,.85f));
    }

    // Visual-only lock feedback: combat targeting remains authoritative.
    float lockVisual;
    Vector2 lockScreenPosition;
    bool lockVisualInitialized;
    bool ShowAimReticle=>!inMenu&&!paused&&!Finished&&!spectatorMode&&!player.deadVisual;
    bool TryGetTargetBounds(Fighter target,out Bounds world)
    {
        world=default;
        if(target==null||target.body==null)return false;
        bool found=false;
        foreach(Renderer renderer in target.body.GetComponentsInChildren<Renderer>())
        {
            if(!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
            bool equipment=false;
            if(target.weaponVisuals!=null)foreach(Transform visual in target.weaponVisuals)
                if(visual!=null&&(renderer.transform==visual||renderer.transform.IsChildOf(visual))){equipment=true;break;}
            if(!equipment&&target.guardShield!=null&&(renderer.transform==target.guardShield||renderer.transform.IsChildOf(target.guardShield)))equipment=true;
            if(equipment)continue;
            if(!found){world=renderer.bounds;found=true;}else world.Encapsulate(renderer.bounds);
        }
        return found;
    }
    Vector3 LockAimPoint(Fighter target)
    {
        // Fighter roots are ground anchors; using renderer bounds can fall below that
        // on compact/tread frames. Keep shots and the visible lock marker at chest height.
        return target!=null&&target.root!=null?target.root.position+Vector3.up*1.2f:Vector3.zero;
    }
    bool TryGetTargetScreenRect(out Rect rect)
    {
        rect=default;
        if(view==null||!TryGetTargetBounds(enemy,out Bounds world))return false;
        Vector3 min=new Vector3(float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity),max=new Vector3(float.NegativeInfinity,float.NegativeInfinity,float.NegativeInfinity);
        for(int i=0;i<8;i++)
        {
            Vector3 corner=world.center+Vector3.Scale(world.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
            Vector3 point=view.WorldToScreenPoint(corner);if(point.z<=0)return false;
            Vector2 ui=new Vector2(point.x*1280f/Screen.width,(Screen.height-point.y)*720f/Screen.height);
            min=Vector3.Min(min,ui);max=Vector3.Max(max,ui);
        }
        float pad=Mathf.Clamp(Mathf.Max(max.x-min.x,max.y-min.y)*.08f,8,24);
        rect=Rect.MinMaxRect(min.x-pad,min.y-pad,max.x+pad,max.y+pad);
        return rect.xMax>=0&&rect.xMin<=1280&&rect.yMax>=0&&rect.yMin<=720;
    }
    bool TryGetTargetScreenPoint(out Vector2 point)
    {
        point=default;
        if(view==null||enemy==null)return false;
        Vector3 projected=view.WorldToScreenPoint(LockAimPoint(enemy));
        if(projected.z<=0)return false;
        point=new Vector2(projected.x*1280f/Screen.width,(Screen.height-projected.y)*720f/Screen.height);
        return true;
    }
    void DrawAimReticle()
    {
        if(!ShowAimReticle) return;
        Color ink=new Color(.015f,.025f,.035f,.95f);
        Color ready=canShoot?Color.white:new Color(1,.62f,.25f);
        Rect[] ticks={new Rect(623,359,10,2),new Rect(647,359,10,2),new Rect(639,343,2,10),new Rect(639,367,2,10)};
        foreach(Rect tick in ticks)
        {
            Panel(new Rect(tick.x-1,tick.y-1,tick.width+2,tick.height+2),ink);
            Panel(tick,ready);
        }
        Panel(new Rect(637,357,6,6),ink);
        Panel(new Rect(639,359,2,2),Color.white);
    }
    void DrawLockReticle()
    {
        if(spectatorMode || Finished || paused || view==null || enemy==null || enemy.deadVisual) return;
        bool onScreen=TryGetTargetScreenRect(out Rect targetRect);
        if(ShouldDrawSoftAssistReticle(onScreen)&&lockVisual<.05f)DrawSoftAssistReticle(targetRect);
        float target=targetLock&&onScreen?1f:0f;
        lockVisual=Mathf.MoveTowards(lockVisual,target,Time.unscaledDeltaTime*(target>lockVisual?5.5f:2.8f));
        if(lockVisual<=.001f){lockVisualInitialized=false;return;}
        if(onScreen&&TryGetTargetScreenPoint(out Vector2 targetPoint)){lockScreenPosition=targetPoint;lockVisualInitialized=true;}
        float targetSize=onScreen?Mathf.Max(targetRect.width,targetRect.height):108f;
        float eased=lockVisual*lockVisual*(3-2*lockVisual);
        float size=Mathf.Lerp(Mathf.Max(48f,targetSize),Mathf.Max(36f,targetSize*.48f),eased);
        float alpha=Mathf.Lerp(.04f,.94f,eased);
        Color color=Color.Lerp(new Color(.45f,.72f,.84f,alpha),new Color(.2f,1f,.94f,alpha),eased);
        color.a=alpha;
        float x=lockScreenPosition.x-size*.5f,y=lockScreenPosition.y-size*.5f;
        float corner=Mathf.Lerp(10f,15f,eased),thickness=2f;
        // Four open corners remain readable without covering the opponent.
        for(int i=0;i<4;i++)
        {
            float cx=x+(i%2==0?0:size),cy=y+(i<2?0:size);
            Panel(new Rect(cx-(i%2==0?0:corner),cy,corner,thickness),color);
            Panel(new Rect(cx,cy-(i<2?0:corner),thickness,corner),color);
        }
        if(eased>.8f)
        {
            Color center=color; center.a*=.55f;
            Panel(new Rect(lockScreenPosition.x-2,lockScreenPosition.y-2,4,4),center);
        }
    }
    bool ShouldDrawSoftAssistReticle(bool targetVisible)=>assistEngaged&&!targetLock&&targetVisible;
    void DrawSoftAssistReticle(Rect targetRect)
    {
        Vector2 center=targetRect.center;float size=Mathf.Clamp(Mathf.Max(targetRect.width,targetRect.height)*.56f,28,58);
        float corner=Mathf.Clamp(size*.22f,7,12),thickness=1.5f;float x=center.x-size*.5f,y=center.y-size*.5f;
        Color color=Cyan;color.a=.34f+.08f*(.5f+.5f*Mathf.Sin(Time.unscaledTime*4.5f));
        for(int i=0;i<4;i++)
        {
            bool right=i%2==1,bottom=i>=2;float cx=right?x+size-corner:x,cy=bottom?y+size-thickness:y;
            Panel(new Rect(cx,cy,corner,thickness),color);
            Panel(new Rect(right?x+size-thickness:x,bottom?y+size-corner:y,thickness,corner),color);
        }
        Rect tag=new Rect(x,y-17,56,14);Panel(tag,new Color(.025f,.045f,.075f,.64f));
        GUI.color=new Color(.30f,1f,.94f,color.a+.12f);GUI.Label(new Rect(tag.x+3,tag.y-1,tag.width-4,tag.height),"ASSIST",new GUIStyle(small){fontSize=9,fontStyle=FontStyle.Bold});GUI.color=Color.white;
    }

    void OnGUI()
    {
        if (player == null) return;
        if (label == null)
        {
            label = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            small = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            title = new GUIStyle(label) { fontSize = 28 };
        }
        GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
        if(inMenu&&balanceEditorOpen){DrawBalanceEditor();return;}
        if(inMenu&&weaponGalleryOpen) { DrawWeaponGallery(); return; }
        if(testLabOpen) { DrawTestLab(); return; }
        if(showGuide) { DrawGuide(); return; }
        if (inMenu) { GUI.enabled=!showTuning; DrawHangar(); GUI.enabled=true; if(showTuning) DrawTuning(); return; }
        Panel(new Rect(0, 0, 1280, 94), new Color(.025f, .045f, .075f, .96f));
        Text(28, 14, 270, "Voxel-On", title);
        Text(30, 52, 300, "GROUND FRAME  /  DUEL 06", small);
        Text(340, 9, 300, "PILOT / " + player.spec.name, label);
        Text(342, 36, 310, $"HULL {Mathf.CeilToInt(player.hp)} / {player.spec.health}"+(player.spec.ShieldMax>0?$"   SHIELD {Mathf.CeilToInt(player.shieldEnergy)}":""), small);
        Panel(new Rect(328,12,3,68),Cyan);
        HudBar(342, 59, 240, player.hp / player.spec.health, Cyan);
        if(player.spec.ShieldMax>0) HudBar(342, 73, 240, player.shieldEnergy / player.spec.ShieldMax, new Color(.48f,.67f,1));
        Text(670, 9, 300, "RIVAL / " + enemy.spec.name, label);
        Text(672, 36, 310, $"HULL {Mathf.CeilToInt(enemy.hp)} / {enemy.spec.health}"+(enemy.spec.ShieldMax>0?$"   SHIELD {Mathf.CeilToInt(enemy.shieldEnergy)}":""), small);
        Panel(new Rect(658,12,3,68),Orange);
        HudBar(672, 59, 240, enemy.hp / enemy.spec.health, Orange);
        if(enemy.spec.ShieldMax>0) HudBar(672, 73, 240, enemy.shieldEnergy / enemy.spec.ShieldMax, new Color(.48f,.67f,1));
        Text(1010, 18, 250, timedFragMode?$"FRAG {FragScore(0)}-{FragScore(1)}  {Mathf.CeilToInt(fragTimeRemaining/60):00}:{Mathf.CeilToInt(fragTimeRemaining%60):00}":$"{(int)roundTime / 60:00}:{(int)roundTime % 60:00}     W {wins} / L {losses}", label);
        Panel(new Rect(0, 616, 1280, 104), new Color(.025f, .045f, .075f, .96f));
        Text(28, 626, 360, player.spec.name + " / " + player.spec.role, label);
        Text(28,658,360,EquippedProfile(player).mount+" / "+EquippedProfile(player).name,small);
        Text(28,682,360,player.reloadTime>0?"RELOADING "+player.reloadTime.ToString("F1")+"s":"AMMO "+player.magazine+" / "+EquippedProfile(player).capacity,small);
        Text(420, 620, 300, "BOOST / "+(player.spec.name=="WISP"?"RUN / SLIDE":player.spec.boost.ToString())+"  "+Mathf.CeilToInt(player.boostEnergy), small);
        Bar(422, 643, 220, player.boostEnergy / player.spec.boostCapacity, Cyan);
        Text(420,652,300,"SHIFT BOOST / SPACE JUMP",small);
        Bar(422, 675, 220, 1-player.weaponTimer/(EquippedProfile(player).interval*weaponCooldownScale/Mathf.Max(.2f,player.spec.rightArmHandling)), new Color(1,.70f,.30f));
        Text(420,687,300,spectatorMode?"AI SPECTATOR / 360 ORBIT":CanStartLockOn(player)?"RMB LOCK-ON / F1 HELP":HasEquippedGuard(player)?"RMB GUARD / F1 HELP":"MANUAL AIM / F1 HELP",small);
        if (!Finished && !paused)
            Text(405, 574, 650, player.lockTime>0?"SPECIAL FIRING / MOVEMENT LOCKED":player.rushTime>0?"SABER RUSH":player.boostLocked ? "BOOST EMPTY / RELEASE SHIFT TO REARM" : AimStatus(), small);
        DrawCombatFeedback();
        DrawRadar();
        float gravityShift=FoundryGravityEventStrength;
        if(gravityShift>.01f)Text(468,96,470,$"FOUNDRY GRAVITY {Mathf.RoundToInt(Mathf.Lerp(100,foundryMinimumGravity*100,gravityShift))}% / MOVE +{Mathf.RoundToInt(foundryMobilityBonus*gravityShift*100)}%",small);
        DrawLockReticle();
        DrawAimReticle();DrawAssistEcho();DrawWeaponLoadout();DrawRespawnSelection();
        if (paused || Finished&&Time.time-finishPresentationTime>1.2f)
        {
            Panel(new Rect(385, 245, 510, 205), new Color(.025f, .045f, .075f, .96f));
            int winner=MatchWinnerTeam();
            string result=timedFragMode?(winner<0?"FRAG DRAW":winner==0?"BLUE FRAG WIN":"ORANGE FRAG WIN"):
                fighterRoster.Count>2?(winner<0?"MATCH DRAW":spectatorMode?(winner==0?"BLUE TEAM WON":"ORANGE TEAM WON"):winner==player.teamId?"TEAM WON":"TEAM LOST"):
                spectatorMode?(winner==0?"BLUE AI WON":"ORANGE AI WON"):enemy.hp<=0?"DUEL WON":"DUEL LOST";
            Text(425,268,450,paused?"PAUSED":result,title);
            Text(425, 321, 440, paused ? "ESC to continue" : rivalSelection<0?"R to rematch with a random rival":"R to retry this matchup", label);
            Text(425,349,440,$"DAMAGE DEALT {damageDealt:0}  /  TAKEN {damageTaken:0}",small);
            if (GUI.Button(new Rect(425, 375, 225, 42), "M / RETURN TO HANGAR")) ReturnToHangar();
        }
    }

    void DrawHangar()
    {
        Panel(new Rect(0, 0, 1280, 720), new Color(.025f, .045f, .075f, .98f));
        Text(32, 22, 500, "Voxel-On  —  HANGAR", title);
        Text(255, 65, 990, "THIRD PERSON DUEL   /   THREE CHASSIS PROTOTYPES / FOUR WEAPON SLOTS", small);
        if(GUI.Button(new Rect(34,76,208,29),"MODE: "+(timedFragMode?"TIMED FRAG":"DUEL")))timedFragMode=!timedFragMode;
        if(GUI.Button(new Rect(1040,28,208,38),"WEAPON / PART OPTIONS")) OpenBalanceEditor();
        if(GUI.Button(new Rect(600,28,200,38),"MAP: "+(arenaMap==ArenaMapKind.IndoorFactory?"FOUNDRY":arenaMap==ArenaMapKind.OutdoorCanyon?"CANYON":arenaMap==ArenaMapKind.CoastalBay?"COASTAL":"PLAINS"))) SwitchArenaMap();
        if(GUI.Button(new Rect(820,28,200,38),"HOW TO PLAY / F1")) ToggleGuide();
        int[] lineup={0,1,2};
        float[] cardX={42,450,858};
        for(int order=0;order<lineup.Length;order++)
        {
            int i=lineup[order];if(i>=CraftCatalog.UnlockedCount)continue;
            CraftSpec spec=CraftCatalog.Selectable[i];
            if(activeBalance!=null){spec=spec.RuntimeCopy();BalanceDesign configured=Array.Find(activeBalance.designs,x=>x.id==activeBalance.activeDesignId&&x.chassisId==spec.name);BalancePart[] parts=configured!=null?configured.parts:activeBalance.parts;foreach(BalancePart part in parts)if(part.frame==spec.name)foreach(BalanceNumber n in part.numbers)if(n.implemented)typeof(CraftSpec).GetField(n.key).SetValue(spec,n.value);}
            bool selected=selectedCraft==i;Rect card=new Rect(cardX[order],112,380,356);
            Panel(card,selected?new Color(.10f,.22f,.29f,.96f):new Color(.045f,.075f,.11f,.92f));
            Color cardAccent=selected?spec.color:new Color(.42f,.54f,.62f);
            Panel(new Rect(card.x,card.y,card.width,3),cardAccent);
            Panel(new Rect(card.x,card.y+card.height-3,card.width,3),selected?cardAccent:new Color(.08f,.12f,.16f));
            Panel(new Rect(card.x,card.y,4,card.height),spec.color);
            Text(card.x+18,card.y+8,244,(i+1)+"  /  "+spec.name,label);
            Rect stateChip=new Rect(card.x+270,card.y+9,96,24);
            Panel(stateChip,selected?new Color(spec.color.r,spec.color.g,spec.color.b,.15f):new Color(.08f,.12f,.16f));
            Panel(new Rect(stateChip.x,stateChip.y,2,stateChip.height),cardAccent);
            GUI.color=selected?Color.Lerp(cardAccent,Color.white,.45f):new Color(.75f,.83f,.88f);
            Text(stateChip.x+7,stateChip.y-5,stateChip.width-10,selected?"SELECTED":"READY",small);GUI.color=Color.white;
            if(framePortraits!=null&&i<framePortraits.Length&&framePortraits[i]!=null)GUI.DrawTexture(new Rect(card.x+62,card.y+47,256,250),framePortraits[i],ScaleMode.ScaleToFit,true);
            string[] loadout=i==0?new[]{"AR / HANDGUN","LAUNCHER / RIFLE"}:i==1?new[]{"LMG / ROCKET","GATLING / LAUNCHER"}:new[]{"BEAM / SHOTGUN","GRENADE / SNIPER"};
            Text(card.x+14,card.y+298,350,loadout[0]+"   ·   "+loadout[1],small);
            if(GUI.Button(new Rect(card.x+18,card.y+324,164,26),selected?"선택됨 · 커스터마이징":"커스터마이징"))
            {
                selectedCraft=i;balanceFrameIndex=i;balanceTab=0;balanceMountIndex=0;ApplySelectableCharacter(player,selectedCraft);OpenBalanceEditor();
            }
            if(GUI.Button(new Rect(card.x+198,card.y+324,164,26),"기체 선택")){selectedCraft=i;ApplySelectableCharacter(player,selectedCraft);}
        }
        CraftSpec chosen = CraftCatalog.Selectable[selectedCraft];
        Panel(new Rect(32, 482, 1216, 112), new Color(.065f,.095f,.14f));
        GUI.color = chosen.color;
        Text(50, 492, 1100, chosen.name + "  /  " + chosen.role, label);
        GUI.color = Color.white;
        Text(50, 567, 1150,"현재 기체 외형과 무장 구성을 비교한 뒤 커스터마이징을 여세요.",small);
        Text(50, 531, 1150, $"ARMOR {chosen.armor*100:0}%   SHIELD {chosen.ShieldMax:0}   MOVE {chosen.speed:0.#} / BOOST {chosen.boostSpeed:0.#}   STORED CONCEPT: {chosen.description}", small);
        Text(34, 618, 850, "WASD / mouse look / 1-4 weapons / RMB "+
            (CanStartLockOn(player)?"lock-on":HasEquippedGuard(player)?"guard":"manual aim")+" / LMB fire", label);
        string mobilityHelp=chosen.name=="WISP"?"Shift: forward run / side-back slide. Space: jump / hold for lift.":chosen.name=="TARANTULA"?"Shift: frame booster. Hold and release Space: charged jump.":"Shift: frame booster. Space: low jump.";
        Text(34,654,850,mobilityHelp+"  R: reload / stop firing: auto reload.",small);
        if(GUI.Button(new Rect(210,686,220,25),"AI: "+DifficultyName)) difficulty=(difficulty+1)%3;
        bool manualWisp=false;
        bool previousEnabled=GUI.enabled; GUI.enabled=previousEnabled&&!manualWisp;
        if(GUI.Button(new Rect(450,686,220,25),"AIM ASSIST: "+(aimAssist?"ON":"OFF"))) aimAssist=!aimAssist;
        GUI.enabled=previousEnabled;
        if(CraftCatalog.UnlockedCount==1) Text(690,684,270,"RIVAL / WISP",small);
        else if(GUI.Button(new Rect(690,686,270,25),"RIVAL: "+(rivalSelection<0?"RANDOM":CraftCatalog.Selectable[rivalSelection].name)))
            rivalSelection=rivalSelection>=CraftCatalog.UnlockedCount-1?-1:rivalSelection+1;
        if(GUI.Button(new Rect(972,686,155,25),"MODE: "+(spectatorMode?"AI VIEW":"PILOT"))) spectatorMode=!spectatorMode;
        if(GUI.Button(new Rect(34,686,150,25),muted?"SOUND OFF":"SOUND ON")) ToggleMute();
        GUI.backgroundColor = chosen.color;
        if (GUI.Button(new Rect(984, 615, 264, 67), spectatorMode?"START AI SIM / ENTER":timedFragMode?"START FRAG / ENTER":"START DUEL / ENTER")) BeginMatch();
        GUI.backgroundColor = Color.white;
    }

    void DrawTuning()
    {
        Panel(new Rect(335,150,610,450),new Color(.04f,.08f,.13f));
        Text(365,168,550,"WEAPON TUNING / ALL FRAMES",title);
        Text(365,223,520,"BULLET SPEED  x"+bulletSpeedScale.ToString("0.00"),label);
        bulletSpeedScale=GUI.HorizontalSlider(new Rect(365,260,540,24),bulletSpeedScale,.5f,2);
        Text(365,290,520,"BULLET SIZE  x"+bulletSizeScale.ToString("0.00"),label);
        bulletSizeScale=GUI.HorizontalSlider(new Rect(365,327,540,24),bulletSizeScale,.5f,2);
        Text(365,357,520,"PRIMARY COOLDOWN  x"+weaponCooldownScale.ToString("0.00"),label);
        weaponCooldownScale=GUI.HorizontalSlider(new Rect(365,394,540,24),weaponCooldownScale,.5f,2);
        Text(365,423,540,"SOUND EFFECTS  "+Mathf.RoundToInt(effectsVolume*100)+"%"+(muted?" / MUTED":""),label);
        effectsVolume=GUI.HorizontalSlider(new Rect(365,460,540,24),effectsVolume,0,1);
        Text(365,490,540,"Weapon tuning affects both sides. Settings saved on duel / exit.",small);
        if(GUI.Button(new Rect(365,545,240,40),"RESET WEAPONS")) bulletSpeedScale=bulletSizeScale=weaponCooldownScale=1;
        if(GUI.Button(new Rect(665,545,240,40),"DONE / ESC")) { showTuning=false; SavePreferences(); }
    }
    Vector2 RadarPoint(Vector3 world)
    {
        Vector3 center=(player.root.position+enemy.root.position)*.5f;
        float scale=64/Mathf.Max(42,Vector3.Distance(player.root.position,enemy.root.position)*.65f);
        return new Vector2(1100+(world.x-center.x)*scale,203-(world.z-center.z)*scale);
    }
    void DrawRadar(){DrawSensorRadar();}
}







