using UnityEngine;

public sealed partial class VoxArena
{
    // Cross-process verification uses an isolated namespace, never the player's settings.
    bool RunPreferenceProbe()
    {
        string[] args=System.Environment.GetCommandLineArgs();
        bool write=System.Array.IndexOf(args,"--prefs-probe-write")>=0;
        bool read=System.Array.IndexOf(args,"--prefs-probe-read")>=0;
        if(!write&&!read) return false;
        smoke=true; // Also blocks the normal OnApplicationQuit save.
        const string scope="VoxSky.Validation.20261027.";
        string[] keys={"Difficulty","AimAssist","Muted","Volume","Frame","Rival","BulletSpeed","BulletSize","Cooldown"};
        bool ok=true;
        try
        {
            if(write)
            {
                difficulty=1; aimAssist=false; muted=true; effectsVolume=.23f;
                selectedCraft=5; rivalSelection=3; bulletSpeedScale=1.4f; bulletSizeScale=.8f; weaponCooldownScale=1.3f;
                SavePreferences(scope);
                Debug.Log("VOXSKY_PREFS_WRITE_PASS");
            }
            else
            {
                LoadPreferences(scope);
                ok=difficulty==1&&!aimAssist&&muted&&selectedCraft==5&&rivalSelection==3&&
                    Mathf.Abs(effectsVolume-.23f)<.0001f&&Mathf.Abs(bulletSpeedScale-1.4f)<.0001f&&
                    Mathf.Abs(bulletSizeScale-.8f)<.0001f&&Mathf.Abs(weaponCooldownScale-1.3f)<.0001f;
                Debug.Log(ok?"VOXSKY_PREFS_RESTART_PASS":"VOXSKY_PREFS_RESTART_FAIL");
            }
        }
        catch(System.Exception exception) { ok=false; Debug.LogException(exception); }
        finally
        {
            if(read||!ok)
            {
                foreach(string key in keys) PlayerPrefs.DeleteKey(scope+key);
                PlayerPrefs.Save();
            }
        }
        Application.Quit(ok?0:2); return true;
    }
}
