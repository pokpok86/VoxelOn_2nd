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
            "현재 플레이어와 AI 모두 WISP로 출격합니다. 처음에는 AI: PRACTICE를 추천합니다.",
            "WASD는 카메라 기준 이동입니다. 마우스로 시야를 돌리고, 옆 방향키로 옆걸음합니다.",
            "WISP는 완전 수동 조준입니다. 좌클릭은 왼손 총, 우클릭 유지로 오른손 방패를 듭니다.",
            "왼손: 30발 고속탄 총   /   자동 재장전   /   R: 조기 재장전   /   Space: 점프, 길게 눌러 추가 상승",
            "Shift를 누르고 이동하면 부스터가 작동합니다. Shift를 놓으면 기본 속도로 돌아옵니다.",
            "우클릭 방패는 현재 자세 연출입니다. 자동 실드 게이지와 부스터 연료는 별개입니다.",
            "R: 재경기   /   M: 기체 선택   /   Esc: 일시정지   /   V: 음소거",
            "격납고 MODE를 AI VIEW로 바꾸면 AI 대 AI 전투를 쿼터뷰로 관전합니다. 자동 회전하며 휠 버튼 드래그로 조작할 수 있습니다.",
            "WEAPON TUNING에서 탄속·탄 크기·소리를 조정할 수 있습니다."
        }:new[]{
            "Both pilots use WISP. Start with AI: PRACTICE.",
            "WASD moves relative to the camera. Mouse look sets heading; side keys strafe.",
            "WISP uses manual aim. LMB fires the left gun; hold RMB to raise the right shield.",
            "Left hand: 30-round rapid gun   /   Auto-reload on empty; R reloads early   /   Space: jump; hold for extra lift.",
            "Hold Shift while moving to boost; release Shift to return to normal speed.",
            "RMB shield is a guard pose for now. Automatic shield energy and boost fuel are separate.",
            "R: rematch. M: hangar. Esc: pause. V: mute.",
            "Select AI VIEW in the hangar to watch both AI pilots. The quarter view orbits automatically; drag with the middle mouse button to steer it.",
            "WEAPON TUNING adjusts projectile speed, size and sound."
        };
        for(int i=0;i<rows.Length;i++) GUI.Label(new Rect(210,174+i*39,850,38),rows[i],guideBody);
        GUI.color=Cyan;
        GUI.Label(new Rect(210,548,650,60),koreanGuide?"추천: NOMAD로 조작을 익히고, WISP로 접근전을 연습해 보세요.":"Try NOMAD for movement practice, then WISP for close combat.",guideBody);
        GUI.color=Color.white;
        if(GUI.Button(new Rect(934,578,140,36),"CLOSE / F1 / ESC")) CloseGuide();
    }
}