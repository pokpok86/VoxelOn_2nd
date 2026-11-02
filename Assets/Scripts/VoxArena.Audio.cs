using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    enum SoundKind { Fire,Hit,Shield,Explosion,Boost,Special,Laser,Footstep,HeavyStep,Joint,Landing,HeavyLanding,Charge,ChargeRelease,CoverBreak,Rubble }
    readonly Dictionary<SoundKind,AudioClip> soundClips=new Dictionary<SoundKind,AudioClip>();
    readonly Dictionary<SoundKind,float> soundLast=new Dictionary<SoundKind,float>();
    readonly List<AudioSource> voices=new List<AudioSource>();
    int nextVoice;
    readonly Dictionary<Fighter,float> footstepClock=new Dictionary<Fighter,float>();
    readonly Dictionary<Fighter,float> chargeSoundClock=new Dictionary<Fighter,float>();
    bool muted;
    [Range(0,1)] public float effectsVolume=.7f;
    void BuildAudio()
    {
        var rng=new System.Random(551);
        foreach(SoundKind kind in Enum.GetValues(typeof(SoundKind)))
        {
            float duration=kind==SoundKind.Laser?.45f:kind==SoundKind.Explosion?.35f:kind==SoundKind.Special?.30f:
                kind==SoundKind.CoverBreak?.65f:kind==SoundKind.HeavyLanding?.55f:
                kind==SoundKind.Charge?.38f:kind==SoundKind.ChargeRelease?.24f:
                kind==SoundKind.Landing?.30f:kind==SoundKind.HeavyStep?.26f:
                kind==SoundKind.Footstep?.18f:kind==SoundKind.Rubble?.21f:kind==SoundKind.Joint?.17f:.13f;
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
                float sample=(Mathf.Sin(phase)*(1-noiseAmount)+noise*noiseAmount)*envelope*.28f;
                // Additional original mechanical sound families, generated at startup.
                // Each has its own pitch contour and noise transient.
                if(kind==SoundKind.Footstep || kind==SoundKind.HeavyStep)
                {
                    float heavy=kind==SoundKind.HeavyStep?1f:0f;
                    float thud=Mathf.Sin(t*2*Mathf.PI*(78f-heavy*30f))*.7f+noise*.3f;
                    sample=thud*Mathf.Exp(-t*(5f-heavy*2f))*.37f;
                }
                else if(kind==SoundKind.Joint)
                    sample=(Mathf.Sin(t*t*190f)+noise*.20f)*Mathf.Sin(Mathf.PI*t)*.12f;
                else if(kind==SoundKind.Landing || kind==SoundKind.HeavyLanding)
                    sample=(Mathf.Sin(t*2*Mathf.PI*(kind==SoundKind.HeavyLanding?48f:82f))+noise*.55f)*
                        Mathf.Exp(-t*6f)*(kind==SoundKind.HeavyLanding?.46f:.31f);
                else if(kind==SoundKind.Charge)
                    sample=(Mathf.Sin(t*2*Mathf.PI*(170f+580f*t))+Mathf.Sin(t*2*Mathf.PI*63f)*.2f)*
                        Mathf.Pow(t,.7f)*.24f;
                else if(kind==SoundKind.ChargeRelease)
                    sample=(Mathf.Sin(t*2*Mathf.PI*(850f-620f*t))+noise*.24f)*
                        Mathf.Exp(-t*5f)*.30f;
                else if(kind==SoundKind.CoverBreak)
                    sample=(noise*.8f+Mathf.Sin(t*2*Mathf.PI*72f)*.2f)*
                        Mathf.Exp(-t*5f)*.43f;
                else if(kind==SoundKind.Rubble)
                    sample=(noise*.55f+Mathf.Sin(t*2*Mathf.PI*150f)*.45f)*
                        Mathf.Exp(-t*10f)*.17f;
                data[i]=Mathf.Clamp(sample,-1f,1f);
            }
            AudioClip clip=AudioClip.Create("VoxSky "+kind,count,1,44100,false); clip.SetData(data,0); soundClips.Add(kind,clip);
        }
        for(int i=0;i<10;i++)
        {
            var source=new GameObject("Effect voice "+i).AddComponent<AudioSource>();
            source.transform.SetParent(transform); source.playOnAwake=false; source.spatialBlend=0; voices.Add(source);
        }
    }
    void UpdateMechanicalAudio(Fighter fighter,float dt)
    {
        if(fighter==null || fighter.root==null || inMenu || paused) return;
        float strideSpeed=new Vector2(fighter.velocity.x,fighter.velocity.z).magnitude;
        bool grounded=fighter.jumpHeight<=.08f;
        float stepTimer=footstepClock.TryGetValue(fighter,out float stored)?stored:0f;
        if(grounded && strideSpeed>1.8f)
        {
            stepTimer+=dt;
            float interval=Mathf.Lerp(.47f,.19f,Mathf.Clamp01(strideSpeed/19f));
            if(stepTimer>=interval)
            {
                stepTimer=0f;
                bool heavy=fighter.spec.mobility==MobilityKind.Tank;
                PlaySound(heavy?SoundKind.HeavyStep:SoundKind.Footstep,
                    fighter.root.position,heavy?.85f:.55f,Random.Range(.91f,1.09f));
            }
        }
        else stepTimer=0f;
        footstepClock[fighter]=stepTimer;
        float chargeTimer=chargeSoundClock.TryGetValue(fighter,out float elapsed)?elapsed:0f;
        if(fighter.spec.chargedReleaseJump && fighter.jumpHeld && fighter.jumpHoldElapsed>0f)
        {
            chargeTimer+=dt;
            if(chargeTimer>=.36f)
            {
                chargeTimer=0f;
                PlaySound(SoundKind.Charge,fighter.root.position,.4f,
                    Mathf.Lerp(.75f,1.3f,Mathf.Clamp01(fighter.jumpHoldElapsed/2f)));
            }
        }
        else chargeTimer=0f;
        chargeSoundClock[fighter]=chargeTimer;
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
