using UnityEngine;

public sealed partial class VoxArena
{
    bool finishCleaned;float finishPresentationTime;
    void ClearBattlePresentation()
    {
        ClearCombat();
        foreach(Spark spark in sparks) ReleasePDPart(spark.visual);
        sparks.Clear(); StopSounds();
        foreach(Fighter fighter in new[]{player,enemy})
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
        if(enemy.hp<=0)BreakRobotModules(enemy);if(player.hp<=0)BreakRobotModules(player);
        PlaySound(SoundKind.Explosion,enemy.hp<=0?enemy.root.position:player.root.position,.7f);
    }
}

