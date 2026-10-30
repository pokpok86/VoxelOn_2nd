using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    enum SoundKind { Fire,Hit,Shield,Explosion,Boost,Special,Laser }
    readonly Dictionary<SoundKind,AudioClip> soundClips=new Dictionary<SoundKind,AudioClip>();
    readonly Dictionary<SoundKind,float> soundLast=new Dictionary<SoundKind,float>();
    readonly List<AudioSource> voices=new List<AudioSource>();
    int nextVoice;
    bool muted;
    [Range(0,1)] public float effectsVolume=.7f;
    void BuildAudio()
    {
        var rng=new System.Random(551);
        foreach(SoundKind kind in Enum.GetValues(typeof(SoundKind)))
        {
            float duration=kind==SoundKind.Laser?.45f:kind==SoundKind.Explosion?.35f:kind==SoundKind.Special?.30f:.13f;
            int count=(int)(44100*duration); float[] data=new float[count];
            float phase=0;
            for(int i=0;i<count;i++)
            {
                float t=i/(float)count,noise=(float)rng.NextDouble()*2-1;
                float frequency=kind==SoundKind.Shield?Mathf.Lerp(1300,550,t):kind==SoundKind.Special?Mathf.Lerp(180,1000,t):
                    kind==SoundKind.Laser?210:kind==SoundKind.Explosion?65:kind==SoundKind.Boost?120:Mathf.Lerp(430,80,t);
                phase+=frequency*2*Mathf.PI/44100;
                float noiseAmount=kind==SoundKind.Explosion?.85f:kind==SoundKind.Boost?.8f:kind==SoundKind.Hit?.65f:.25f;
                float envelope=Mathf.Min(1,t*40)*Mathf.Pow(1-t,2);
                data[i]=(Mathf.Sin(phase)*(1-noiseAmount)+noise*noiseAmount)*envelope*.28f;
            }
            AudioClip clip=AudioClip.Create("VoxSky "+kind,count,1,44100,false); clip.SetData(data,0); soundClips.Add(kind,clip);
        }
        for(int i=0;i<10;i++)
        {
            var source=new GameObject("Effect voice "+i).AddComponent<AudioSource>();
            source.transform.SetParent(transform); source.playOnAwake=false; source.spatialBlend=0; voices.Add(source);
        }
    }
    void ToggleMute()
    {
        muted=!muted;
        if(muted) StopSounds();
    }
    void StopSounds()
    {
        foreach(AudioSource voice in voices) voice.Stop();
        soundLast.Clear();
    }
    void PlaySound(SoundKind kind,Vector3 position,float volume,float pitch=1)
    {
        if(muted||smoke||inMenu||paused||effectsVolume<=0||voices.Count==0) return;
        if(soundLast.TryGetValue(kind,out float last)&&Time.unscaledTime-last<.07f) return;
        soundLast[kind]=Time.unscaledTime;
        AudioSource source=voices[nextVoice++%voices.Count]; source.clip=soundClips[kind];
        source.volume=.35f*volume*Mathf.Clamp01(effectsVolume); source.pitch=Mathf.Clamp(pitch,.55f,1.6f);
        source.panStereo=Mathf.Clamp((view.WorldToViewportPoint(position).x-.5f)*1.2f,-.7f,.7f);
        source.Play();
    }
}
