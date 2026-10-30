using UnityEngine;

public sealed partial class VoxArena
{
    bool finishCleaned;
    void ClearBattlePresentation()
    {
        ClearCombat();
        foreach(Spark spark in sparks) Destroy(spark.visual.gameObject);
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
        inMenu=true; paused=false; showTuning=false; showGuide=false; resumeAfterGuide=false;
        Cursor.lockState=CursorLockMode.None; Cursor.visible=true; view.orthographic=true;
        ClearBattlePresentation(); SavePreferences();
    }
    void SetPaused(bool value)
    {
        paused=value;
        Cursor.lockState=value||spectatorMode?CursorLockMode.None:CursorLockMode.Locked;
        Cursor.visible=value||spectatorMode;
        if(value) StopSounds();
    }
    void FinishPresentation()
    {
        if(finishCleaned) return;
        finishCleaned=true; ClearBattlePresentation();
        PlaySound(SoundKind.Explosion,enemy.hp<=0?enemy.root.position:player.root.position,.7f);
    }
}
