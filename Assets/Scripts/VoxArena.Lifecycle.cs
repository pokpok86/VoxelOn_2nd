using UnityEngine;

public sealed partial class VoxArena
{
    bool finishCleaned;float finishPresentationTime;
    void ClearBattlePresentation()
    {
        ClearCombat();
        foreach(Spark spark in sparks) ReleasePDPart(spark.visual);
        sparks.Clear(); StopSounds();
        foreach(Fighter fighter in fighterRoster)
        {
            fighter.thruster.gameObject.SetActive(false);
            fighter.saber.gameObject.SetActive(false);
            fighter.shield.gameObject.SetActive(false);
        }
        aimMarker.gameObject.SetActive(false);
    }
    void ReturnToHangar()
    {
        RestorePDVisuals();
        ResetMouseLookCapture();
        inMenu=true; paused=false; showTuning=false; showGuide=false; resumeAfterGuide=false;
        Cursor.lockState=CursorLockMode.None; Cursor.visible=true; view.orthographic=true;
        ClearBattlePresentation(); SavePreferences();
    }
    void SetPaused(bool value)
    {
        ResetMouseLookCapture();
        paused=value;
        Cursor.lockState=value||spectatorMode?CursorLockMode.None:CursorLockMode.Locked;
        Cursor.visible=value||spectatorMode;
        if(value) StopSounds();
    }
    void FinishPresentation()
    {
        if(finishCleaned) return;
        finishCleaned=true;finishPresentationTime=Time.time; playerRespawnPrompt=false; ClearBattlePresentation();
        foreach(Fighter fighter in fighterRoster) if(fighter.hp<=0)BreakRobotModules(fighter);
        Fighter firstEliminated=fighterRoster.Find(f=>f!=null&&f.hp<=0);
        PlaySound(SoundKind.Explosion,firstEliminated!=null?firstEliminated.root.position:player.root.position,.7f);
    }
}

