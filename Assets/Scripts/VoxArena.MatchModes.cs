using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    const float TimedFragDuration = 180f;
    const int TimedFragTarget = 10;
    float fragTimeRemaining;
    readonly Dictionary<int,int> teamFragScores = new Dictionary<int,int>();
    BalanceDesign[] respawnDesignOptions = Array.Empty<BalanceDesign>();
    int respawnDesignSelection;
    bool playerRespawnPrompt;
    float playerRespawnPromptRemaining;

    bool IsMatchFinished()
    {
        if(!timedFragMode)
        {
            if(fighterRoster.Count==0) return player.hp<=0||enemy.hp<=0;
            int activeTeam=-1;
            foreach(Fighter fighter in fighterRoster)
            {
                if(fighter==null||fighter.deadVisual||fighter.hp<=0)continue;
                if(activeTeam<0)activeTeam=fighter.teamId;
                else if(activeTeam!=fighter.teamId)return false;
            }
            return true;
        }
        if(fragTimeRemaining<=0) return true;
        foreach(var entry in teamFragScores) if(entry.Value>=TimedFragTarget) return true;
        return false;
    }
    int MatchWinnerTeam()
    {
        if(timedFragMode) return FragWinnerTeam();
        int activeTeam=-1;
        foreach(Fighter fighter in fighterRoster)
        {
            if(fighter==null||fighter.deadVisual||fighter.hp<=0)continue;
            if(activeTeam<0)activeTeam=fighter.teamId;
            else if(activeTeam!=fighter.teamId)return -1;
        }
        return activeTeam;
    }
    int FragScore(int team) => teamFragScores.TryGetValue(team,out int score)?score:0;
    int FragWinnerTeam()
    {
        int a=FragScore(0),b=FragScore(1);
        return a==b?-1:(a>b?0:1);
    }
    void StartMatchRules()
    {
        teamFragScores.Clear();teamFragScores[0]=0;teamFragScores[1]=0;
        foreach(Fighter fighter in fighterRoster){fighter.matchKills=fighter.matchDeaths=0;fighter.respawnTimer=0;fighter.deadVisual=false;fighter.root.gameObject.SetActive(true);}
        fragTimeRemaining=TimedFragDuration;
        playerRespawnPrompt=false;playerRespawnPromptRemaining=0;respawnDesignSelection=0;
    }
    void ProcessTimedFrag(float dt)
    {
        if(!timedFragMode||Finished)return;
        fragTimeRemaining=Mathf.Max(0,fragTimeRemaining-dt);
        if(Finished)return;
        if(playerRespawnPrompt)
        {
            playerRespawnPromptRemaining=Mathf.Max(0,playerRespawnPromptRemaining-dt);
            for(int i=0;i<8;i++) if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i))) respawnDesignSelection=i;
            if(Input.GetKeyDown(KeyCode.Return)||playerRespawnPromptRemaining<=0)
            {
                playerRespawnPrompt=false;player.respawnTimer=0;
                Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;
            }
        }
        foreach(Fighter fighter in fighterRoster)
        {
            if(!fighter.deadVisual)continue;
            if(fighter==player&&playerRespawnPrompt)continue;
            fighter.respawnTimer=Mathf.Max(0,fighter.respawnTimer-dt);
            if(fighter.respawnTimer<=0)RespawnTimedFragFighter(fighter);
        }
    }
    void RegisterTimedFragDeath(Fighter attacker,Fighter target)
    {
        if(!timedFragMode||target==null||target.deadVisual)return;
        target.matchDeaths++;target.deadVisual=true;target.respawnTimer=target==player?5f:2.5f;
        target.root.gameObject.SetActive(false);
        if(attacker!=null&&IsHostile(attacker,target))
        {
            attacker.matchKills++;
            teamFragScores[attacker.teamId]=FragScore(attacker.teamId)+1;
        }
        if(target==player)
        {
            BalanceDocument choices=activeBalance??BalanceData.Defaults();
            respawnDesignOptions=choices.designs??Array.Empty<BalanceDesign>();
            respawnDesignSelection=Mathf.Max(0,Array.FindIndex(respawnDesignOptions,x=>x!=null&&x.id==choices.activeDesignId));
            playerRespawnPrompt=true;playerRespawnPromptRemaining=5f;
            Cursor.lockState=CursorLockMode.None;Cursor.visible=true;targetLock=false;
        }
    }
    void RespawnTimedFragFighter(Fighter fighter)
    {
        if(fighter==null||!fighter.deadVisual)return;
        if(fighter==player&&respawnDesignOptions.Length>0&&respawnDesignSelection>=0&&respawnDesignSelection<respawnDesignOptions.Length)
        {
            BalanceDesign design=respawnDesignOptions[respawnDesignSelection];
            if(design!=null)
            {
                if(activeBalance==null)activeBalance=BalanceData.Defaults();
                activeBalance.activeDesignId=design.id;
                int chassis=Array.FindIndex(CraftCatalog.Selectable,x=>x.name==design.chassisId);
                if(chassis>=0){selectedCraft=chassis;ApplySelectableCharacter(fighter,chassis);}
            }
        }
        fighter.root.gameObject.SetActive(true);fighter.deadVisual=false;
        fighter.hp=fighter.spec.health;fighter.boostEnergy=fighter.spec.boostCapacity;fighter.shieldEnergy=fighter.spec.ShieldMax;
        fighter.weapons=NewWeaponStates(fighter.weaponProfiles);fighter.weaponSlot=1;fighter.magazine=fighter.weaponProfiles[0].capacity;fighter.reloadTime=0;
        fighter.velocity=Vector3.zero;fighter.jumpHeight=fighter.jumpVelocity=0;fighter.knockdownTime=0;fighter.invulnerableTime=1.25f;
        Vector3 spawn=ArenaSpawnPosition(fighter.teamId);
        Vector2 offset=UnityEngine.Random.insideUnitCircle*2.2f;fighter.root.position=spawn+new Vector3(offset.x,0,offset.y);
        fighter.lastPosition=fighter.root.position;fighter.respawnTimer=0;fighter.aiCoverRouting=false;fighter.aiCoverRetry=0;
        Fighter target=Opponent(fighter);if(target!=null&&!target.deadVisual)FaceOpponent(fighter,target);
        if(fighter==player){targetLock=false;aimMarker.gameObject.SetActive(true);ResetMouseLookCapture();}
        Animate(fighter);UpdateObstacles(0);
    }
    void DrawRespawnSelection()
    {
        if(!playerRespawnPrompt)return;
        Panel(new Rect(300,180,680,360),new Color(.02f,.04f,.07f,.98f));
        Text(335,198,610,"RESPAWN / SELECT A STORED DESIGN",title);
        Text(338,242,600,$"Choose a saved frame and loadout · auto-respawn in {playerRespawnPromptRemaining:0}s",small);
        for(int i=0;i<8;i++)
        {
            float x=338+(i%2)*300,y=278+(i/2)*48;
            BalanceDesign design=i<respawnDesignOptions.Length?respawnDesignOptions[i]:null;
            string name=design==null?$"EMPTY DESIGN {i+1}":$"{i+1}. {design.name} / {design.chassisId}";
            Color previous=GUI.backgroundColor;GUI.backgroundColor=i==respawnDesignSelection?Cyan:new Color(.14f,.20f,.27f);
            if(GUI.Button(new Rect(x,y,280,38),name))respawnDesignSelection=i;
            GUI.backgroundColor=previous;
        }
        Text(338,478,610,"Number keys 1-8 select · Enter confirms · timeout keeps the highlighted design",small);
    }
}

