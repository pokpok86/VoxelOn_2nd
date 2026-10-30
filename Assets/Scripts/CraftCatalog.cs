using UnityEngine;

public enum PrimaryKind { SplitCenter, Plasma, Gatling, Missiles, Scatter, Rail, Wave, Grenade }
public enum SpecialKind { SweepBeam, SiegeBeam, SaberRush, MissileFan, Nova, RailLance, HomingMines, Minefield }
public enum BoostKind { Glide, Cruise, Pulse }

public sealed class CraftSpec
{
    public string name, role, description, weaponName, specialName;
    public Color color;
    public float health, armor, speed, boostSpeed, boostCapacity, boostDrain, pulseDuration;
    public float damage, weaponCooldown, range, projectileSpeed, projectileRadius, size, specialCooldown;
    public PrimaryKind primary;
    public SpecialKind special;
    public BoostKind boost;
    public float ShieldMax => health <= 300 ? health * .15f : 0;
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
}

