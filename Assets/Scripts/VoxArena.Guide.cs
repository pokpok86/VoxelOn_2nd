using UnityEngine;

public sealed partial class VoxArena
{
    bool showGuide,resumeAfterGuide,koreanGuide;
    Font guideFont;
    GUIStyle guideBody,guideHeading;
    void ToggleGuide()
    {
        if(showGuide) { CloseGuide(); return; }
        resumeAfterGuide=!inMenu&&!paused;
        showGuide=true; if(!inMenu) SetPaused(true);
    }
    void CloseGuide()
    {
        showGuide=false;
        if(resumeAfterGuide) SetPaused(false);
        resumeAfterGuide=false;
    }
    void DrawGuide()
    {
        if(guideBody==null)
        {
            string[] names=Font.GetOSInstalledFontNames();
            koreanGuide=System.Array.Exists(names,name=>name=="Malgun Gothic"||name=="맑은 고딕");
            if(koreanGuide) guideFont=Font.CreateDynamicFontFromOSFont(new[]{"Malgun Gothic","맑은 고딕"},18);
            guideBody=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true,font=guideFont};
            guideHeading=new GUIStyle(guideBody){fontSize=26,fontStyle=FontStyle.Bold};
        }
        Panel(new Rect(0,0,1280,720),new Color(.015f,.025f,.045f,.94f));
        Panel(new Rect(170,84,940,552),new Color(.055f,.09f,.14f));
        Panel(new Rect(170,84,5,552),Cyan);
        GUI.Label(new Rect(207,110,850,45),koreanGuide?"첫 출격 가이드":"FIRST SORTIE / HOW TO PLAY",guideHeading);
        string[] rows=koreanGuide?new[]{
            "처음에는 AI: PRACTICE와 AIM ASSIST: ON으로 시작하세요.",
            "WASD는 카메라 기준 이동입니다. 우클릭 락온을 풀면 이동 방향으로 몸이 회전합니다.",
            "상대가 화면 밖이나 장애물 뒤로 가면 락온이 풀립니다. 왼쪽 버튼으로 공격합니다.",
            "1번 슬롯: 30발 고속탄 총   /   자동 재장전   /   R: 조기 재장전   /   Space: 점프, 길게 눌러 추가 상승",
            "Shift를 누르고 이동하면 부스터가 작동합니다. Shift를 놓으면 기본 속도로 돌아옵니다.",
            "실드는 WISP·LANCER만 있습니다. 부스터 연료와는 별개입니다.",
            "R: 재경기   /   M: 기체 선택   /   Esc: 일시정지   /   V: 음소거",
            "격납고 MODE를 AI VIEW로 바꾸면 AI 대 AI 전투를 쿼터뷰로 관전합니다. 자동 회전하며 휠 버튼 드래그로 조작할 수 있습니다.",
            "RIVAL에서 연습 상대를 고르고, WEAPON TUNING에서 탄속·소리를 조정하세요."
        }:new[]{
            "Start with AI: PRACTICE and AIM ASSIST: ON.",
            "WASD moves relative to the camera. After releasing right-mouse lock, the body turns toward movement.",
            "Lock breaks when the rival is hidden or leaves the view. Left mouse attacks.",
            "Slot 1: 30-round rapid gun   /   Auto-reload on empty; R reloads early   /   Space: jump; hold for extra lift.",
            "Hold Shift while moving to boost; release Shift to return to normal speed.",
            "Only WISP and LANCER have shields. Boost fuel is separate.",
            "R: rematch. M: hangar. Esc: pause. V: mute.",
            "Select AI VIEW in the hangar to watch both AI pilots. The quarter view orbits automatically; drag with the middle mouse button to steer it.",
            "RIVAL selects a practice opponent. WEAPON TUNING adjusts weapons and sound."
        };
        for(int i=0;i<rows.Length;i++) GUI.Label(new Rect(210,174+i*39,850,38),rows[i],guideBody);
        GUI.color=Cyan;
        GUI.Label(new Rect(210,548,650,60),koreanGuide?"추천: NOMAD로 조작을 익히고, WISP로 접근전을 연습해 보세요.":"Try NOMAD for movement practice, then WISP for close combat.",guideBody);
        GUI.color=Color.white;
        if(GUI.Button(new Rect(934,578,140,36),"CLOSE / F1 / ESC")) CloseGuide();
    }
}