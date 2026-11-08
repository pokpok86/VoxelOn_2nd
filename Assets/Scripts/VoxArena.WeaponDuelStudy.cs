using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public sealed partial class VoxArena
{
    sealed class WeaponDuelTotals
    {
        public int wins, draws, losses;
        public float remainingHealth, duration, damageDealt;
        public int matches;
    }

    IEnumerator WeaponDuelStudy()
    {
        // Compare shipped data profiles through the regular AI, movement, firing, reload and hit loops.
        string[] ids={"Military_AR","Military_LMG","Military_GL","Military_RL","Military_SR"};
        BalanceDocument defaults=BalanceData.Defaults();
        BalanceWeapon[] weapons=ids.Select(id=>defaults.weapons.First(w=>w.id==id)).ToArray();
        var totals=ids.ToDictionary(id=>id,_=>new WeaponDuelTotals());
        var rows=new StringBuilder("seed,sideSwap,weaponA,weaponB,damageA,intervalA,capacityA,blastA,damageB,intervalB,capacityB,blastB,seconds,winner,weaponAHP,weaponBHP,weaponADamageDealt,weaponBDamageDealt\n");
        bool valid=true;int matches=0;BalanceDocument previousBalance=activeBalance;bool previousPause=paused,previousSmoke=smoke,previousSpectator=spectatorMode;int previousDifficulty=difficulty;
        activeBalance=null;smoke=false;paused=true;spectatorMode=true;difficulty=0;
        for(int a=0;a<weapons.Length;a++)for(int b=a+1;b<weapons.Length;b++)for(int seed=0;seed<5;seed++)for(int sideSwap=0;sideSwap<2;sideSwap++)
        {
            UnityEngine.Random.InitState(842101+seed*97+a*13+b*31);
            TestSetupSelectable(0,0);smoke=false;inMenu=false;paused=true;spectatorMode=true;difficulty=0;targetLock=false;roundTime=0;
            Fighter fighterA=sideSwap==0?player:enemy,fighterB=sideSwap==0?enemy:player;
            fighterA.root.position=new Vector3(sideSwap==0?-12:12,FighterGroundY,sideSwap==0?-12:12);fighterB.root.position=new Vector3(sideSwap==0?12:-12,FighterGroundY,sideSwap==0?12:-12);
            fighterA.hp=fighterA.spec.health;fighterB.hp=fighterB.spec.health;
            fighterA.duelDamageDealt=fighterB.duelDamageDealt=0;
            EquipDuelStudyWeapon(fighterA,weapons[a]);EquipDuelStudyWeapon(fighterB,weapons[b]);
            FaceOpponent(fighterA,fighterB);FaceOpponent(fighterB,fighterA);
            float seconds=0;int ticks=0;
            while(seconds<45&&fighterA.hp>0&&fighterB.hp>0)
            {
                const float dt=.05f;
                EnemyAI(fighterA,fighterB,dt);EnemyAI(fighterB,fighterA,dt);
                FaceOpponent(fighterA,fighterB,dt);FaceOpponent(fighterB,fighterA,dt);
                UpdateObstacles(dt);UpdateCombat(dt);UpdateSparks(dt);UpdatePDVisualEffects(dt);Animate(fighterA);Animate(fighterB);
                seconds+=dt;roundTime+=dt;ticks++;
                valid&=Finite(fighterA.root.position)&&Finite(fighterB.root.position)&&!float.IsNaN(fighterA.hp)&&!float.IsInfinity(fighterA.hp)&&!float.IsNaN(fighterB.hp)&&!float.IsInfinity(fighterB.hp)&&fighterA.hp>=0&&fighterB.hp>=0;
                if(ticks%100==0)yield return null;
            }
            string winner=Mathf.Abs(fighterA.hp-fighterB.hp)<.1f?"draw":fighterA.hp>fighterB.hp?"A":"B";
            float healthA=fighterA.hp,healthB=fighterB.hp;
            rows.AppendLine(string.Join(",",new[]{seed.ToString(CultureInfo.InvariantCulture),sideSwap.ToString(CultureInfo.InvariantCulture),weapons[a].id,weapons[b].id,
                F(weapons[a].Get("damage")),F(weapons[a].Get("interval")),I(weapons[a].Get("capacity")),F(weapons[a].Get("blast")),
                F(weapons[b].Get("damage")),F(weapons[b].Get("interval")),I(weapons[b].Get("capacity")),F(weapons[b].Get("blast")),F(seconds),winner,F(healthA),F(healthB),F(fighterA.duelDamageDealt),F(fighterB.duelDamageDealt)}));
            RecordWeaponDuel(totals[ids[a]],winner=="A"?1:winner=="draw"?0:-1,healthA,seconds,fighterA.duelDamageDealt);
            RecordWeaponDuel(totals[ids[b]],winner=="B"?1:winner=="draw"?0:-1,healthB,seconds,fighterB.duelDamageDealt);matches++;
        }
        activeBalance=previousBalance;paused=previousPause;smoke=previousSmoke;spectatorMode=previousSpectator;difficulty=previousDifficulty;
        string stamp=DateTime.Now.ToString("yyyyMMdd-HHmmss",CultureInfo.InvariantCulture),directory=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Validation");Directory.CreateDirectory(directory);
        string detailPath=Path.Combine(directory,"Weapon-Duel-Study-"+stamp+".csv"),summaryPath=Path.Combine(directory,"Weapon-Duel-Summary-"+stamp+".csv");
        File.WriteAllText(detailPath,rows.ToString(),new UTF8Encoding(false));
        var summary=new StringBuilder("weapon,matches,wins,draws,losses,winPoints,meanRemainingHP,meanFightSeconds,meanDamageDealt\n");
        foreach(string id in ids){WeaponDuelTotals t=totals[id];summary.AppendLine(string.Join(",",id,t.matches,t.wins,t.draws,t.losses,F(t.wins+t.draws*.5f),F(t.remainingHealth/Mathf.Max(1,t.matches)),F(t.duration/Mathf.Max(1,t.matches)),F(t.damageDealt/Mathf.Max(1,t.matches))));}
        File.WriteAllText(summaryPath,summary.ToString(),new UTF8Encoding(false));
        Debug.Log(valid&&matches==100?"WEAPON_DUEL_STUDY_PASS matches="+matches+" detail="+detailPath+" summary="+summaryPath:"WEAPON_DUEL_STUDY_FAIL matches="+matches);
        Application.Quit(valid&&matches==100?0:2);

        static string F(float value)=>value.ToString("F2",CultureInfo.InvariantCulture);
        static string I(float value)=>Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
    }

    void EquipDuelStudyWeapon(Fighter fighter,BalanceWeapon source)
    {
        fighter.weaponProfiles=new WeaponProfile[4];fighter.weaponArtIds=new string[4];
        for(int i=0;i<4;i++)
        {
            WeaponProfile profile=new WeaponProfile(source.name,Loadout[i].mount,Mathf.RoundToInt(source.Get("capacity")),source.Get("damage"),source.Get("interval"),source.Get("speed")*2f,source.Get("radius"),source.Get("range"));
            profile.spread=source.Get("spread");profile.blast=source.Get("blast");profile.weight=source.Get("weight");profile.ammoWeight=source.Get("ammoWeight");profile.partialReload=source.Get("partialReload");profile.emptyReload=source.Get("emptyReload");profile.idleReload=source.Get("idleReload");fighter.weaponProfiles[i]=profile;fighter.weaponArtIds[i]=source.id;
        }
        fighter.weapons=NewWeaponStates(fighter.weaponProfiles);fighter.weaponSlot=1;fighter.magazine=fighter.weaponProfiles[0].capacity;fighter.reloadTime=0;fighter.weaponTimer=0;fighter.aiWeaponReadyAt=0;fighter.aiClock=0;
    }

    static void RecordWeaponDuel(WeaponDuelTotals totals,int result,float remainingHealth,float duration,float damageDealt)
    {
        if(result>0)totals.wins++;else if(result<0)totals.losses++;else totals.draws++;
        totals.remainingHealth+=Mathf.Max(0,remainingHealth);totals.duration+=duration;totals.damageDealt+=Mathf.Max(0,damageDealt);totals.matches++;
    }
}
