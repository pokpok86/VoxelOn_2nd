using UnityEngine;

public sealed partial class VoxArena
{
    int rivalSelection=-1;
    const string PreferencePrefix="VoxSky.Pilot.";
    static float ValidTuning(float value)=>float.IsNaN(value)||float.IsInfinity(value)?1:Mathf.Clamp(value,.5f,2);
    int ResolveRival()=>rivalSelection<0?UnityEngine.Random.Range(0,CraftCatalog.All.Length):Mathf.Clamp(rivalSelection,0,CraftCatalog.All.Length-1);
    void LoadPreferences(string scope=PreferencePrefix)
    {
        if(smoke&&scope==PreferencePrefix) return; // Regression tests must start from defaults and leave player preferences alone.
        difficulty=Mathf.Clamp(PlayerPrefs.GetInt(scope+"Difficulty",0),0,2);
        aimAssist=PlayerPrefs.GetInt(scope+"AimAssist",1)!=0;
        muted=PlayerPrefs.GetInt(scope+"Muted",0)!=0;
        float storedVolume=PlayerPrefs.GetFloat(scope+"Volume",.7f);
        effectsVolume=float.IsNaN(storedVolume)||float.IsInfinity(storedVolume)?.7f:Mathf.Clamp01(storedVolume);
        selectedCraft=Mathf.Clamp(PlayerPrefs.GetInt(scope+"Frame",0),0,CraftCatalog.All.Length-1);
        rivalSelection=Mathf.Clamp(PlayerPrefs.GetInt(scope+"Rival",-1),-1,CraftCatalog.All.Length-1);
        bulletSpeedScale=ValidTuning(PlayerPrefs.GetFloat(scope+"BulletSpeed",1));
        bulletSizeScale=ValidTuning(PlayerPrefs.GetFloat(scope+"BulletSize",1));
        weaponCooldownScale=ValidTuning(PlayerPrefs.GetFloat(scope+"Cooldown",1));
    }
    void SavePreferences(string scope=PreferencePrefix)
    {
        if(smoke&&scope==PreferencePrefix) return;
        PlayerPrefs.SetInt(scope+"Difficulty",difficulty);
        PlayerPrefs.SetInt(scope+"AimAssist",aimAssist?1:0);
        PlayerPrefs.SetInt(scope+"Muted",muted?1:0);
        PlayerPrefs.SetFloat(scope+"Volume",Mathf.Clamp01(effectsVolume));
        PlayerPrefs.SetInt(scope+"Frame",selectedCraft);
        PlayerPrefs.SetInt(scope+"Rival",rivalSelection);
        PlayerPrefs.SetFloat(scope+"BulletSpeed",ValidTuning(bulletSpeedScale));
        PlayerPrefs.SetFloat(scope+"BulletSize",ValidTuning(bulletSizeScale));
        PlayerPrefs.SetFloat(scope+"Cooldown",ValidTuning(weaponCooldownScale));
        PlayerPrefs.Save();
    }
    void OnApplicationQuit()=>SavePreferences();
}

