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
            "1번 WISP·2번 BASTION·3번 TARANTULA를 선택합니다. 처음에는 PRACTICE를 권장합니다.",
            "WASD: 카메라 기준 이동 / 마우스: 시야 / Shift: 기체별 부스터 / Space: 점프",
            "숫자 1~4: 무기 전환 / 좌클릭: 발사. 조준 보조는 발사 순간에만 약하게 작동합니다.",
            "R: 재장전. 잔탄 장전 2.5초·빈 탄창 4초는 시험값입니다. 다른 무기는 계속 사용할 수 있습니다.",
            "WISP는 Space 유지로 추가 상승합니다. TARANTULA는 Space를 모았다 놓아 점프합니다.",
            "우클릭: WISP 방패 자세 / 2·3번 기존 락온. 방패 자세에는 시험용 이동 페널티가 있습니다.",
            "R: 종료 후 재경기 / M: 기체 선택 / Esc: 일시정지 / V: 음소거 / F: 습득·웅크리기 / G: 습득물 사용",
            "격납고 MAP에서 실내·경계가 있는 Canyon을 선택합니다. 실외 날씨와 시간대는 라운드마다 정해집니다.",
            "F2: 기체 수치·날씨 시험 / AI VIEW: AI 대전 관전 / 레이더 밖의 적은 방향만 표시합니다."
        }:new[]{
            "Choose WISP, BASTION or TARANTULA. PRACTICE is recommended first.",
            "WASD: camera-relative movement. Mouse: look. Shift: frame booster. Space: jump.",
            "1-4: weapon slots. LMB: fire. Weak aim assistance only adjusts the shot direction.",
            "R: reload. Partial 2.5s / empty 4s are trial values. Other weapons keep operating.",
            "Hold Space for WISP air lift. Hold and release Space for TARANTULA charge jump.",
            "RMB: WISP guard pose / existing BASTION-TARANTULA lock. Guard movement penalties are trial values.",
            "R: rematch after defeat. M: hangar. Esc: pause. V: mute. F: crouch/pick up. G: use pickup.",
            "MAP selects Foundry or the bounded Canyon. Outdoor weather/daylight stays fixed for each round.",
            "F2: live frame/environment tests. AI VIEW: spectator. Out-of-range radar gives direction only."
        };
        for(int i=0;i<rows.Length;i++) GUI.Label(new Rect(210,174+i*39,850,38),rows[i],guideBody);
        GUI.color=Cyan;
        GUI.Label(new Rect(210,548,650,60),koreanGuide?"현재 기체·무기·이동 수치는 검증 중인 시험 설정입니다.":"Frame, weapon and movement values are trial settings awaiting play review.",guideBody);
        GUI.color=Color.white;
        if(GUI.Button(new Rect(934,578,140,36),"CLOSE / F1 / ESC")) CloseGuide();
    }
}