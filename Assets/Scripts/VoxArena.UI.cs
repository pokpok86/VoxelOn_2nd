using UnityEngine;

public sealed partial class VoxArena
{
    bool showTuning;
    void Panel(Rect r, Color color) { GUI.color = color; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white; }
    void Text(float x, float y, float w, string value, GUIStyle style) => GUI.Label(new Rect(x, y, w, 35), value, style);
    void Bar(float x, float y, float w, float fraction, Color color)
    {
        Panel(new Rect(x, y, w, 8), new Color(.12f, .18f, .24f));
        Panel(new Rect(x, y, w * Mathf.Clamp01(fraction), 8), color);
    }

    void OnGUI()
    {
        if (player == null) return;
        if (label == null)
        {
            label = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            small = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            title = new GUIStyle(label) { fontSize = 28 };
        }
        GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
        if(showGuide) { DrawGuide(); return; }
        if (inMenu) { GUI.enabled=!showTuning; DrawHangar(); GUI.enabled=true; if(showTuning) DrawTuning(); return; }
        Panel(new Rect(0, 0, 1280, 94), new Color(.025f, .045f, .075f, .96f));
        Text(28, 14, 270, "VOX / SKY", title);
        Text(30, 52, 300, "GROUND FRAME  /  DUEL 06", small);
        Text(340, 9, 300, "PILOT / " + player.spec.name, label);
        Text(342, 36, 310, $"HULL {Mathf.CeilToInt(player.hp)} / {player.spec.health}"+(player.spec.ShieldMax>0?$"   SHIELD {Mathf.CeilToInt(player.shieldEnergy)}":""), small);
        Bar(342, 59, 240, player.hp / player.spec.health, Cyan);
        if(player.spec.ShieldMax>0) Bar(342, 73, 240, player.shieldEnergy / player.spec.ShieldMax, new Color(.48f,.67f,1));
        Text(670, 9, 300, "RIVAL / " + enemy.spec.name, label);
        Text(672, 36, 310, $"HULL {Mathf.CeilToInt(enemy.hp)} / {enemy.spec.health}"+(enemy.spec.ShieldMax>0?$"   SHIELD {Mathf.CeilToInt(enemy.shieldEnergy)}":""), small);
        Bar(672, 59, 240, enemy.hp / enemy.spec.health, Orange);
        if(enemy.spec.ShieldMax>0) Bar(672, 73, 240, enemy.shieldEnergy / enemy.spec.ShieldMax, new Color(.48f,.67f,1));
        Text(1010, 18, 250, $"{(int)roundTime / 60:00}:{(int)roundTime % 60:00}     W {wins} / L {losses}", label);
        Panel(new Rect(0, 616, 1280, 104), new Color(.025f, .045f, .075f, .96f));
        Text(28, 626, 360, player.spec.name + " / " + player.spec.role, label);
        Text(28, 658, 360, "LMB  RAPID GUN / "+player.magazine+" / 30", small);
        Text(28, 682, 360, player.reloadTime>0?"RELOADING  "+player.reloadTime.ToString("0.0")+"s":"RMB HOLD  FACE RIVAL", small);
        Text(420, 620, 300, "BOOST / "+player.spec.boost+"  "+Mathf.CeilToInt(player.boostEnergy), small);
        Bar(422, 643, 220, player.boostEnergy / player.spec.boostCapacity, Cyan);
        Text(420, 652, 300, player.weaponTimer > 0 ? "PRIMARY  " + player.weaponTimer.ToString("0.0") + "s" : "PRIMARY READY", small);
        Bar(422, 675, 220, 1-player.weaponTimer/.105f, new Color(1,.70f,.30f));
        Text(740, 620, 500, spectatorMode?"AI SPECTATOR / 360 ORBIT":"PRIMARY WEAPON / RAPID GUN", small);
        Bar(742,643,220,player.magazine/30f,new Color(.8f,.45f,1));
        Text(740, 658, 520, "WASD MOVE / SHIFT BOOST / HOLD SPACE TO CLIMB", small);
        Text(740, 682, 520, "ESC PAUSE   V SOUND "+(muted?"OFF":"ON")+"   AI: "+DifficultyName+"   F1 HELP", small);
        if (!Finished && !paused)
            Text(405, 574, 650, player.lockTime>0?"SPECIAL FIRING / MOVEMENT LOCKED":player.rushTime>0?"SABER RUSH":player.boostLocked ? "BOOST EMPTY / RELEASE SHIFT TO REARM" : AimStatus(), small);
        DrawCombatFeedback();
        DrawRadar();
        if (Finished || paused)
        {
            Panel(new Rect(385, 245, 510, 205), new Color(.025f, .045f, .075f, .96f));
            Text(425, 268, 450, paused ? "PAUSED" : spectatorMode ? (enemy.hp<=0?"BLUE AI WON":"ORANGE AI WON") : enemy.hp <= 0 ? "DUEL WON" : "DUEL LOST", title);
            Text(425, 321, 440, paused ? "ESC to continue" : rivalSelection<0?"R to rematch with a random rival":"R to retry this matchup", label);
            Text(425,349,440,$"DAMAGE DEALT {damageDealt:0}  /  TAKEN {damageTaken:0}",small);
            if (GUI.Button(new Rect(425, 375, 225, 42), "M / RETURN TO HANGAR")) ReturnToHangar();
        }
    }

    void DrawHangar()
    {
        Panel(new Rect(0, 0, 1280, 720), new Color(.025f, .045f, .075f, .98f));
        Text(32, 22, 500, "VOX / SKY  —  HANGAR", title);
        Text(34, 65, 1000, "THIRD PERSON DUEL   /   8 MODULAR FRAMES   /   RAPID GUN", small);
        if(GUI.Button(new Rect(1040,28,208,38),"WEAPON TUNING")) showTuning=true;
        if(GUI.Button(new Rect(820,28,200,38),"HOW TO PLAY / F1")) ToggleGuide();
        for (int i = 0; i < CraftCatalog.All.Length; i++)
        {
            CraftSpec spec = CraftCatalog.All[i];
            Rect card = new Rect(32 + i % 4 * 308, 113 + i / 4 * 178, 292, 162);
            bool selected = selectedCraft == i;
            Panel(card, selected ? new Color(.10f,.22f,.29f) : new Color(.065f,.095f,.14f));
            Panel(new Rect(card.x, card.y, 4, card.height), spec.color);
            if(framePortraits!=null) GUI.DrawTexture(new Rect(card.x+183,card.y+35,105,123),framePortraits[i],ScaleMode.ScaleToFit);
            Text(card.x + 16, card.y + 12, 240, spec.name, label);
            Text(card.x + 16, card.y + 43, 255, spec.role, small);
            Text(card.x + 16, card.y + 76, 175, "1  RAPID GUN / 30", small);
            Text(card.x + 16, card.y + 124, 260, $"BOOST {spec.boost} / {spec.boostSpeed:0.#}   HP {spec.health}", small);
            if (GUI.Button(card, GUIContent.none, GUIStyle.none))
            {
                selectedCraft = i;
                ApplyCharacter(player, selectedCraft);
            }
        }
        CraftSpec chosen = CraftCatalog.All[selectedCraft];
        Panel(new Rect(32, 487, 1216, 105), new Color(.065f,.095f,.14f));
        GUI.color = chosen.color;
        Text(50, 500, 1100, chosen.name + "  /  " + chosen.role, label);
        GUI.color = Color.white;
        Text(50, 558, 1150,"MODULES: HEAD / CORE / ARMS / LEGS / WEAPON  -  ASSEMBLY COMING LATER",small);
        Text(50, 533, 1150, $"ARMOR {chosen.armor*100:0}%   SHIELD {chosen.ShieldMax:0}   MOVE {chosen.speed:0.#} / BOOST {chosen.boostSpeed:0.#}   STORED CONCEPT: {chosen.description}", small);
        Text(34, 618, 850, "WASD move / mouse look / RMB lock / LMB fire", label);
        Text(34, 654, 850, "Hold Shift to boost. Space jumps. Empty magazines reload automatically; R reloads early.", small);
        if(GUI.Button(new Rect(210,686,220,25),"AI: "+DifficultyName)) difficulty=(difficulty+1)%3;
        if(GUI.Button(new Rect(450,686,220,25),"AIM ASSIST: "+(aimAssist?"ON":"OFF"))) aimAssist=!aimAssist;
        if(GUI.Button(new Rect(690,686,270,25),"RIVAL: "+(rivalSelection<0?"RANDOM":CraftCatalog.All[rivalSelection].name)))
            rivalSelection=rivalSelection>=CraftCatalog.All.Length-1?-1:rivalSelection+1;
        if(GUI.Button(new Rect(972,686,155,25),"MODE: "+(spectatorMode?"AI VIEW":"PILOT"))) spectatorMode=!spectatorMode;
        if(GUI.Button(new Rect(34,686,150,25),muted?"SOUND OFF / V":"SOUND ON / V")) ToggleMute();
        GUI.backgroundColor = chosen.color;
        if (GUI.Button(new Rect(984, 615, 264, 67), spectatorMode?"START AI SIM / ENTER":"START DUEL / ENTER")) BeginMatch();
        GUI.backgroundColor = Color.white;
    }

    void DrawTuning()
    {
        Panel(new Rect(335,150,610,450),new Color(.04f,.08f,.13f));
        Text(365,168,550,"WEAPON TUNING / ALL FRAMES",title);
        Text(365,223,520,"BULLET SPEED  x"+bulletSpeedScale.ToString("0.00"),label);
        bulletSpeedScale=GUI.HorizontalSlider(new Rect(365,260,540,24),bulletSpeedScale,.5f,2);
        Text(365,290,520,"BULLET SIZE  x"+bulletSizeScale.ToString("0.00"),label);
        bulletSizeScale=GUI.HorizontalSlider(new Rect(365,327,540,24),bulletSizeScale,.5f,2);
        Text(365,357,520,"PRIMARY COOLDOWN  x"+weaponCooldownScale.ToString("0.00"),label);
        weaponCooldownScale=GUI.HorizontalSlider(new Rect(365,394,540,24),weaponCooldownScale,.5f,2);
        Text(365,423,540,"SOUND EFFECTS  "+Mathf.RoundToInt(effectsVolume*100)+"%"+(muted?" / MUTED":""),label);
        effectsVolume=GUI.HorizontalSlider(new Rect(365,460,540,24),effectsVolume,0,1);
        Text(365,490,540,"Weapon tuning affects both sides. Settings saved on duel / exit.",small);
        if(GUI.Button(new Rect(365,545,240,40),"RESET WEAPONS")) bulletSpeedScale=bulletSizeScale=weaponCooldownScale=1;
        if(GUI.Button(new Rect(665,545,240,40),"DONE / ESC")) { showTuning=false; SavePreferences(); }
    }
    Vector2 RadarPoint(Vector3 world)
    {
        Vector3 center=(player.root.position+enemy.root.position)*.5f;
        float scale=64/Mathf.Max(42,Vector3.Distance(player.root.position,enemy.root.position)*.65f);
        return new Vector2(1100+(world.x-center.x)*scale,203-(world.z-center.z)*scale);
    }
    void DrawRadar()
    {
        Panel(new Rect(1025, 110, 150, 174), new Color(.025f, .045f, .075f, .85f));
        Text(1035, 111, 140, "GROUND / RADAR", small);
        foreach (Bounds obstacle in obstacles)
        {
            Vector2 p = RadarPoint(obstacle.center);
            if(p.x>1035&&p.x<1160&&p.y>145&&p.y<266)
                Panel(new Rect(p.x-3,p.y-3,6,6),new Color(.45f,.52f,.58f));
        }
        Vector2 pilot = RadarPoint(player.root.position), rival = RadarPoint(enemy.root.position);
        Panel(new Rect(pilot.x - 3, pilot.y - 3, 6, 6), Cyan);
        Panel(new Rect(rival.x - 3, rival.y - 3, 6, 6), Orange);
    }
}
