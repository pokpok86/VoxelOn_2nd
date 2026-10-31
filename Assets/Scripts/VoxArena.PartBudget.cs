using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public sealed partial class VoxArena
{
    // Every editable part receives exactly 100 allocation points. Values are
    // derived from its original frame preset, never from the previously edited
    // value; reallocating cannot increase the overall part power budget.
    readonly Dictionary<string,float[]> allocations=new Dictionary<string,float[]>();
    readonly string[][] allocationFields={
        new[] {"lockRange","occludedLockRange","occludedLockSeconds","aimSpreadDegrees"},
        new[] {"health","armor","boostCapacity","boostDrain","boostSpeed","pulseDuration"},
        new[] {"leftArmHandling","leftArmStability"},
        new[] {"rightArmHandling","rightArmStability"},
        new[] {"forwardSpeedFactor","reverseSpeedFactor","leftSpeedFactor","rightSpeedFactor"},
        new[] {"damage","weaponCooldown","range","projectileSpeed","projectileRadius","specialCooldown"}
    };
    // Higher point allocation always means stronger performance.
    bool InvertedAllocation(string key)=>key=="aimSpreadDegrees" || key=="boostDrain" ||
        key=="weaponCooldown" || key=="specialCooldown";
    string AllocationKey(Fighter f,int category)=>f.teamId+":"+category;
    float[] AllocationFor(Fighter f,int category)
    {
        string id=AllocationKey(f,category);
        if(!allocations.TryGetValue(id,out float[] points))
        {
            string[] fields=allocationFields[category];
            points=new float[fields.Length];
            // Directional example requested: 40 front, 30 back, 15 per side.
            if(category==4) {points[0]=40;points[1]=30;points[2]=15;points[3]=15;}
            else
            {
                float each=100f/fields.Length;
                for(int i=0;i<fields.Length;i++) points[i]=each;
            }
            allocations[id]=points;
        }
        return points;
    }
    void ResetAllocations(Fighter f)
    {
        for(int i=0;i<allocationFields.Length;i++) allocations.Remove(AllocationKey(f,i));
        for(int i=0;i<allocationFields.Length;i++) ApplyAllocation(f,i);
    }
    void AllocatePoint(Fighter f,int category,int index,float desired)
    {
        float[] p=AllocationFor(f,category);
        float old=p[index], next=Mathf.Clamp(desired,0,100), change=next-old;
        if(Mathf.Abs(change)<.0001f) return;
        if(change>0)
        {
            float others=100-old;
            if(others<.0001f) return;
            for(int i=0;i<p.Length;i++) if(i!=index)
                p[i]=Mathf.Max(0,p[i]-change*p[i]/others);
        }
        else
        {
            float others=100-old;
            if(others<.0001f)
            {
                float each=-change/(p.Length-1);
                for(int i=0;i<p.Length;i++) if(i!=index) p[i]+=each;
            }
            else
                for(int i=0;i<p.Length;i++) if(i!=index)
                    p[i]+=(-change)*p[i]/others;
        }
        p[index]=next;
        ApplyAllocation(f,category);
    }
    void ApplyAllocation(Fighter f,int category)
    {
        if(f==null || category<0 || category>=allocationFields.Length) return;
        float[] points=AllocationFor(f,category);
        string[] keys=allocationFields[category];
        CraftSpec preset=CraftCatalog.All[f.frameIndex];
        float neutral=100f/keys.Length;
        for(int i=0;i<keys.Length;i++)
        {
            FieldInfo field=typeof(CraftSpec).GetField(keys[i],BindingFlags.Public|BindingFlags.Instance);
            if(field==null || field.FieldType!=typeof(float)) continue;
            float baseline=(float)field.GetValue(preset);
            // A 0-point stat retains a minimal 20% effectiveness; no infinite
            // cooldowns, zero ranges or negative stats.
            float strength=.2f+.8f*(points[i]/neutral);
            float value=InvertedAllocation(keys[i])?baseline/Mathf.Max(.2f,strength):baseline*strength;
            field.SetValue(f.spec,value);
        }
        if(category==1)
        {
            f.hp=Mathf.Min(f.hp,f.spec.health);
            f.boostEnergy=Mathf.Min(f.boostEnergy,f.spec.boostCapacity);
        }
    }
    void DrawBudgetCategory(Fighter f,int category)
    {
        string[] keys=allocationFields[category];
        float[] points=AllocationFor(f,category);
        GUI.Label(new Rect(205,307,770,26),"FIXED PART BUDGET: 100 / 100   -   increasing one stat reduces others");
        GUI.BeginGroup(new Rect(199,340,865,305));
        testScroll=GUI.BeginScrollView(new Rect(0,0,865,305),testScroll,new Rect(0,0,840,keys.Length*49+20));
        for(int i=0;i<keys.Length;i++)
        {
            string key=keys[i];
            FieldInfo field=typeof(CraftSpec).GetField(key);
            float effective=field==null?0:(float)field.GetValue(f.spec);
            string display=category==4?i==0?"FORWARD":i==1?"BACKWARD":i==2?"LEFT":"RIGHT":key;
            GUI.Label(new Rect(8,i*49,435,28),display+"  "+points[i].ToString("0.0")+"%   ("+effective.ToString("0.00")+")");
            float desired=GUI.HorizontalSlider(new Rect(448,i*49+8,365,23),points[i],0,100);
            if(Mathf.Abs(desired-points[i])>.05f) AllocatePoint(f,category,i,desired);
        }
        GUI.EndScrollView();
        GUI.EndGroup();
    }
}