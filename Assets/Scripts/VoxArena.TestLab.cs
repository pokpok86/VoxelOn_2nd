using System;
using System.Reflection;
using UnityEngine;

// Runtime-only tuning overlay. Values never modify CraftCatalog presets or disk saves.
public sealed partial class VoxArena
{
    bool testLabOpen;
    bool testLabEnabled=true;
    bool aiMoveEnabled=true,aiAttackEnabled=true,aiJumpEnabled=true,aiBoostEnabled=true;
    int testSide, testCategory;
    Vector2 testScroll;
    readonly string[][] testFields={
        // HEAD: sensors, tracking, precision
        new[]{"lockRange","occludedLockRange","occludedLockSeconds","aimSpreadDegrees"},
        // CORE: durability, boost system, defense
        new[]{"health","armor","boostCapacity","boostDrain","boostSpeed","pulseDuration"},
        // LEFT ARM: individual aim/handling and weapon mounts
        new[]{"leftArmHandling","leftArmStability"},
        // RIGHT ARM
        new[]{"rightArmHandling","rightArmStability"},
        // LEGS + PELVIS: mobility, impact resistance, terrain
        new[]{"speed","forwardSpeedFactor","strafeSpeedFactor","reverseSpeedFactor",
            "stepHeight","slopeLimitDegrees","fallGravityFactor","knockdownResistance","recoverySpeed"},
        // WEAPONS: all current shared primary/special values
        new[]{"damage","weaponCooldown","range","projectileSpeed","projectileRadius","specialCooldown"},
        // VISUAL: model scale (not mass)
        new[]{"size"}
    };
    readonly string[] testCategories={"HEAD","CORE","LEFT ARM","RIGHT ARM","LEGS + PELVIS","WEAPONS","VISUAL"};
    Fighter TestFighter=>testSide==0?player:enemy;
    void SetTestCursor()
    {
        Cursor.lockState=testLabOpen?CursorLockMode.None:CursorLockMode.Locked;
        Cursor.visible=testLabOpen;
    }
    void UpdateTestLabFrame()
    {
        // Keep a frozen scene while editing values, so sliders are easy to compare.
        // Combat resumes on F2 without resetting the round.
    }
    float SliderMax(string field,float value)
    {
        switch(field)
        {
            case "armor": return .85f;
            case "aimSpreadDegrees": return 15f;
            case "occludedLockSeconds": return 12f;
            case "forwardSpeedFactor": case "strafeSpeedFactor": case "reverseSpeedFactor": return 2f;
            case "fallGravityFactor": case "knockdownResistance": case "recoverySpeed": return 3f;
            case "leftArmHandling": case "rightArmHandling":
            case "leftArmStability": case "rightArmStability": return 2f;
            case "stepHeight": return 3f;
            case "slopeLimitDegrees": return 75f;
            case "size": return 1.8f;
            case "pulseDuration": return 3f;
            case "weaponCooldown": case "specialCooldown": return 30f;
            case "projectileRadius": return 3f;
            case "boostDrain": return 120f;
            default: return Mathf.Max(20f,value*2.5f);
        }
    }
    float SliderMin(string field)
    {
        if(field=="armor" || field=="aimSpreadDegrees" || field=="occludedLockSeconds" ||
            field=="stepHeight" || field=="slopeLimitDegrees" || field=="pulseDuration") return 0;
        if(field=="size") return .5f;
        if(field=="fallGravityFactor" || field=="recoverySpeed" || field=="knockdownResistance") return .2f;
        return .05f;
    }
    void DrawTestLab()
    {
        if(!testLabOpen || inMenu || testSide<0 || player==null || enemy==null) return;
        Panel(new Rect(175,52,930,615),new Color(.025f,.045f,.075f,.97f));
        Text(199,66,780,"VOXELON / LIVE PART TEST LAB  (F2 CLOSE)",title);
        if(GUI.Button(new Rect(970,67,108,34),"CLOSE F2")) { testLabOpen=false; SetTestCursor(); }
        if(GUI.Button(new Rect(200,112,155,31),testSide==0?"PILOT  [EDIT]":"PILOT")) {testSide=0;testScroll=Vector2.zero;}
        if(GUI.Button(new Rect(366,112,155,31),testSide==1?"RIVAL  [EDIT]":"RIVAL")) {testSide=1;testScroll=Vector2.zero;}
        if(GUI.Button(new Rect(537,112,170,31),"RESET THIS FRAME"))
        {
            Fighter f=TestFighter;
            f.spec=CraftCatalog.All[f.frameIndex].RuntimeCopy();
            f.body.localScale=Vector3.one*f.spec.size;
            f.hp=Mathf.Min(f.hp,f.spec.health);
            f.boostEnergy=Mathf.Min(f.boostEnergy,f.spec.boostCapacity);
        }
        if(GUI.Button(new Rect(718,112,170,31),"REFILL HULL / FUEL"))
        {
            Fighter f=TestFighter; f.hp=f.spec.health; f.boostEnergy=f.spec.boostCapacity;
            f.shieldEnergy=f.spec.ShieldMax; f.magazine=30; f.reloadTime=0;
        }
        Text(200,156,740,"AI CONTROLS / applied to rival during live play",small);
        aiMoveEnabled=GUI.Toggle(new Rect(200,181,150,28),aiMoveEnabled,"MOVE");
        aiAttackEnabled=GUI.Toggle(new Rect(350,181,150,28),aiAttackEnabled,"ATTACK");
        aiJumpEnabled=GUI.Toggle(new Rect(500,181,150,28),aiJumpEnabled,"JUMP");
        aiBoostEnabled=GUI.Toggle(new Rect(650,181,150,28),aiBoostEnabled,"BOOST");
        Text(200,213,750,"Changes are runtime-only. No mass or weight parameters are used by this lab.",small);
        for(int i=0;i<testCategories.Length;i++)
        {
            int row=i/4,col=i%4;
            Rect button=new Rect(200+col*216,246+row*37,206,31);
            if(GUI.Button(button,(testCategory==i?"[":"")+testCategories[i]+(testCategory==i?"]":"")))
            { testCategory=i; testScroll=Vector2.zero; }
        }
        Fighter selected=TestFighter;
        if(testCategory==5)
        {
            if(GUI.Button(new Rect(199,313,210,25),"PRIMARY: "+selected.spec.primary))
                selected.spec.primary=(PrimaryKind)(((int)selected.spec.primary+1)%Enum.GetValues(typeof(PrimaryKind)).Length);
            if(GUI.Button(new Rect(417,313,210,25),"SPECIAL: "+selected.spec.special))
                selected.spec.special=(SpecialKind)(((int)selected.spec.special+1)%Enum.GetValues(typeof(SpecialKind)).Length);
        }
        if(testCategory==1 && GUI.Button(new Rect(199,313,210,25),"BOOST: "+selected.spec.boost))
            selected.spec.boost=(BoostKind)(((int)selected.spec.boost+1)%Enum.GetValues(typeof(BoostKind)).Length);
        GUI.BeginGroup(new Rect(199,333,865,310));
        testScroll=GUI.BeginScrollView(new Rect(0,0,865,310),testScroll,new Rect(0,0,836,testFields[testCategory].Length*53+15));
        string[] fields=testFields[testCategory];
        for(int i=0;i<fields.Length;i++)
        {
            string key=fields[i];
            FieldInfo field=typeof(CraftSpec).GetField(key,BindingFlags.Instance|BindingFlags.Public);
            if(field==null || field.FieldType!=typeof(float)) continue;
            float value=(float)field.GetValue(selected.spec);
            float min=SliderMin(key),max=SliderMax(key,value);
            GUI.Label(new Rect(8,i*53,420,25),key+"  "+value.ToString("0.00"));
            float next=GUI.HorizontalSlider(new Rect(430,i*53+8,380,23),value,min,max);
            if(Mathf.Abs(next-value)>.0001f)
            {
                field.SetValue(selected.spec,next);
                if(key=="size") selected.body.localScale=Vector3.one*next;
                if(key=="health") selected.hp=Mathf.Min(selected.hp,next);
                if(key=="boostCapacity") selected.boostEnergy=Mathf.Min(selected.boostEnergy,next);
            }
        }
        GUI.EndScrollView();
        GUI.EndGroup();
    }
}
