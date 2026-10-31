using UnityEngine;

public enum PrimaryKind { SplitCenter, Plasma, Gatling, Missiles, Scatter, Rail, Wave, Grenade }
public enum SpecialKind { SweepBeam, SiegeBeam, SaberRush, MissileFan, Nova, RailLance, HomingMines, Minefield }
public enum BoostKind { Glide, Cruise, Pulse }

public sealed class CraftSpec
{
    public string name, role, description, weaponName, specialName;
    public Color color;
    public float health, armor, speed, boostSpeed, boostCapacity, boostDrain, pulseDuration;
    // Future leg-part values. 4:3:2 forward/strafe/reverse movement ratio.
    public float forwardSpeedFactor=1f, strafeSpeedFactor=.75f, reverseSpeedFactor=.5f;
    public float leftSpeedFactor=.6f,rightSpeedFactor=.6f;
    public float fallGravityFactor=1f, knockdownResistance=1f, recoverySpeed=1f;
    // Future head-part lock sensor and aim stability values.
    public float lockRange=80f, aimSpreadDegrees=0.6f;
    // Head sensor memory: lose occluded lock only after this time; distance still hard-limits lock.
    public float occludedLockSeconds=1.5f, occludedLockRange=65f;
    // Leg assembly parameters; step-up height is deliberately below tall jump-only platforms.
    public float stepHeight=.45f, slopeLimitDegrees=32f;
    public float leftArmHandling=1f,leftArmStability=1f,rightArmHandling=1f,rightArmStability=1f;
    public float damage, weaponCooldown, range, projectileSpeed, projectileRadius, size, specialCooldown;
    public PrimaryKind primary;
    public SpecialKind special;
    public BoostKind boost;
    public float ShieldMax => health <= 300 ? health * .15f : 0;
    // Per-fighter runtime copy so tuning the rival never mutates the player's catalog preset.
    public CraftSpec RuntimeCopy() => (CraftSpec)MemberwiseClone();
    public CraftSpec(string name, string role, string description, Color color,
        float hp, float armor, float speed, float boostSpeed, float capacity, float drain, float size,
        BoostKind boost, float pulseDuration, PrimaryKind primary, string weaponName,
        float damage, float cooldown, float range, float projectileSpeed, float radius,
        SpecialKind special, string specialName, float specialCooldown)
    {
        this.name=name; this.role=role; this.description=description; this.color=color;
        health=hp; this.armor=armor; this.speed=speed; this.boostSpeed=boostSpeed;
        boostCapacity=capacity*1.5f; boostDrain=drain; this.size=size; this.boost=boost; this.pulseDuration=pulseDuration;
        this.primary=primary; this.weaponName=weaponName; this.damage=damage; weaponCooldown=cooldown;
        this.range=range; this.projectileSpeed=projectileSpeed; projectileRadius=radius;
        this.special=special; this.specialName=specialName; this.specialCooldown=specialCooldown;
        fallGravityFactor=1f; // Leg-specific value; not derived from frame mass.
    }
}
public static class CraftCatalog
{
    public static readonly CraftSpec[] All = {
        new CraftSpec("KESTREL","CROSS-FIRE","Twin bolts, then a center shot. Sweeping beam.",new Color(.18f,.88f,.96f),
            340,.08f,13,27.5f,160,16,1,BoostKind.Glide,0,
            PrimaryKind.SplitCenter,"SPLIT > CENTER",12,1.35f,65,82,.32f,SpecialKind.SweepBeam,"ARC SWEEP",9),
        new CraftSpec("BASTION","SIEGE FRAME","Slow giant plasma. Anchored heavy laser.",new Color(.96f,.66f,.24f),
            460,.28f,10,20.5f,190,12,1.35f,BoostKind.Cruise,0,
            PrimaryKind.Plasma,"PLASMA ORB",30,1.85f,58,52,.85f,SpecialKind.SiegeBeam,"SIEGE LASER",11),
        new CraftSpec("WISP","ASSAULT FRAME","Short-range rapid burst. Boost saber strike.",new Color(.70f,.43f,1),
            290,.02f,15,42.5f,120,72,.85f,BoostKind.Pulse,.30f,
            PrimaryKind.Gatling,"MICRO GATLING",5,1.2f,30,108,.20f,SpecialKind.SaberRush,"SABER RUSH",8),
        new CraftSpec("NOMAD","MISSILE CARRIER","Tracking missile pair. Wide missile barrage.",new Color(.40f,.85f,.48f),
            350,.10f,12,23,230,9,1.05f,BoostKind.Cruise,0,
            PrimaryKind.Missiles,"TWIN SEEKERS",13,1.75f,70,58,.34f,SpecialKind.MissileFan,"MISSILE FAN",11),
        new CraftSpec("SCATTER","BREACH FRAME","Close pellet fan. Expanding radial burst.",new Color(1,.43f,.32f),
            370,.12f,13,35,155,48,1.12f,BoostKind.Pulse,.42f,
            PrimaryKind.Scatter,"FRAGMENT FAN",7,1.45f,32,76,.38f,SpecialKind.Nova,"NOVA RING",9),
        new CraftSpec("LANCER","RAIL FRAME","Thin fast rail slug. Charged piercing lance.",new Color(.40f,.64f,1),
            300,.04f,13.5f,26.5f,150,17,.95f,BoostKind.Glide,0,
            PrimaryKind.Rail,"RAIL NEEDLE",28,1.8f,90,150,.22f,SpecialKind.RailLance,"CHARGE LANCE",10),
        new CraftSpec("TEMPEST","PURSUIT FRAME","Weaving bolts. Seekers stop, warn, explode.",new Color(1,.40f,.72f),
            330,.07f,13.5f,29,155,19,1,BoostKind.Glide,0,
            PrimaryKind.Wave,"WAVE BURST",10,1.5f,62,76,.32f,SpecialKind.HomingMines,"HOMING BLOOM",10),
        new CraftSpec("ANVIL","AREA CONTROL","Heavy blast shell. Proximity minefield.",new Color(.76f,.82f,.90f),
            430,.22f,10.5f,21.5f,200,11,1.28f,BoostKind.Cruise,0,
            PrimaryKind.Grenade,"BLAST SHELL",26,2.0f,52,56,.65f,SpecialKind.Minefield,"MINEFIELD",12)
    };
    static CraftCatalog()
    {
        // Example head profiles until head parts are independently selectable.
        // Long-range sensor frames keep an occluded lock longer; heavy frames are shorter range.
        for(int i=0;i<All.Length;i++)
        {
            CraftSpec f=All[i];
            f.occludedLockSeconds= i==5 || i==6 ? 3.5f : i==1 || i==7 ? 1.1f : 2f;
            f.occludedLockRange= i==5 || i==6 ? 88f : i==1 || i==7 ? 58f : 70f;
            f.lockRange=Mathf.Max(f.lockRange,f.occludedLockRange);
            f.stepHeight= i==1 || i==7 ? .52f : .58f;
        }
    }
}
