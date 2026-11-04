using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator BalanceStudy()
    {
        StringBuilder csv=new StringBuilder("scenario,seed,pilotStartHP,rivalStartHP,pilotStartAmmo,rivalStartAmmo,seconds,winner,pilotHP,rivalHP,pilotTravel,rivalTravel,pilotJumps,rivalJumps,pilotReloadFrames,rivalReloadFrames\n");
        bool valid=true;int matches=0;difficulty=0;bool priorSpectator=spectatorMode;spectatorMode=true;
        for(int scenario=0;scenario<3;scenario++)for(int seed=0;seed<4;seed++)
        {
            UnityEngine.Random.InitState(411040+seed);TestSetupSelectable(0,0);smoke=false;inMenu=false;muted=true;spectatorMode=true;
            player.root.position=new Vector3(-12,FighterGroundY,-12);enemy.root.position=new Vector3(12,FighterGroundY,12);
            if(scenario==1){player.hp*=.35f;enemy.hp*=.65f;enemy.magazine=0;StartWeaponReload(enemy);}
            if(scenario==2){player.magazine=3;enemy.magazine=30;player.root.position=new Vector3(-18,FighterGroundY,6);enemy.root.position=new Vector3(18,FighterGroundY,30);}
            float pStart=player.hp,eStart=enemy.hp,seconds=0,pTravel=0,eTravel=0;int pAmmo=player.magazine,eAmmo=enemy.magazine,pJumps=0,eJumps=0,pReload=0,eReload=0;bool pAir=false,eAir=false;
            while(seconds<45&&!Finished)
            {
                const float dt=.05f;Vector3 p=player.root.position,e=enemy.root.position;
                EnemyAI(player,enemy,dt);EnemyAI(enemy,player,dt);UpdateCombat(dt);UpdateObstacles(dt);UpdateSparks(dt);UpdatePDVisualEffects(dt);Animate(player);Animate(enemy);
                seconds+=dt;roundTime+=dt;pTravel+=Vector3.Distance(p,player.root.position);eTravel+=Vector3.Distance(e,enemy.root.position);
                if(!pAir&&player.jumpHeight>.1f)pJumps++;if(!eAir&&enemy.jumpHeight>.1f)eJumps++;pAir=player.jumpHeight>.1f;eAir=enemy.jumpHeight>.1f;
                if(player.reloadTime>0)pReload++;if(enemy.reloadTime>0)eReload++;
                valid&=Finite(player.root.position)&&Finite(enemy.root.position)&&player.hp>=0&&enemy.hp>=0;
                if(Mathf.RoundToInt(seconds/dt)%80==0)yield return null;
            }
            string winner=enemy.hp<=0?"pilot":player.hp<=0?"rival":"timeout";
            csv.AppendLine(scenario+","+seed+","+pStart.ToString("F1")+","+eStart.ToString("F1")+","+pAmmo+","+eAmmo+","+seconds.ToString("F2")+","+winner+","+player.hp.ToString("F1")+","+enemy.hp.ToString("F1")+","+pTravel.ToString("F1")+","+eTravel.ToString("F1")+","+pJumps+","+eJumps+","+pReload+","+eReload);matches++;
        }
        string path=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Validation","PD14-Balance-Study.csv");File.WriteAllText(path,csv.ToString());spectatorMode=priorSpectator;
        Debug.Log(valid&&matches==12?"BALANCE_STUDY_PASS 12 scenarios recorded; AI proxy, not manual fairness approval":"BALANCE_STUDY_FAIL");Application.Quit(valid&&matches==12?0:2);
    }
    void AIRevisionChecks(Action<bool,string> check)
    {
        bool prior=smoke;
        for(int frame=0;frame<3;frame++)
        {
            TestSetupSelectable(0,frame);smoke=false;enemy.root.position=new Vector3(100,FighterGroundY,100);player.root.position=enemy.root.position+Vector3.forward*20;enemy.aiClock=5;enemy.aiReaction=2;enemy.aiMove=Vector3.forward;
            Bounds hurdle=new Bounds(enemy.root.position+Vector3.forward*3, new Vector3(10,2,1));obstacles.Add(hurdle);float peak=0;
            for(int i=0;i<32;i++){EnemyAI(enemy,player,.05f);peak=Mathf.Max(peak,enemy.jumpHeight);}
            check(peak>.1f,"AI uses frame-specific jump against an actual obstacle "+frame+" peak="+peak.ToString("F2"));obstacles.RemoveAt(obstacles.Count-1);
            enemy.weaponSlot=1;enemy.weaponTimer=0;enemy.magazine=0;enemy.reloadTime=4;enemy.aiWeaponReadyAt=0;
            ChooseAIWeapon(enemy,20);check(enemy.weaponSlot==1,"AI has a readable reaction gap before weapon switch "+frame);
        }
        smoke=prior;TestSetup();
    }
}
