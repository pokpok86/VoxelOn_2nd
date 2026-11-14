"""Portrait review pages from the actual four biped Blender renders."""
from pathlib import Path
import sys,json,shutil
sys.path.insert(0,str(Path.home()/'AppData/Local/Temp/VoxelOn-ImageTools'))
from PIL import Image,ImageDraw,ImageFont
BASE=Path(__file__).resolve().parent.parent;OUT=BASE/'Biped-Four-Refined-20261112'
catalog=json.loads((OUT/'PART_CATALOG.json').read_text());font=lambda n:ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',n)
page=Image.new('RGB',(1080,2600),'#101722');d=ImageDraw.Draw(page)
d.text((28,20),'WISP | 이족형 1 · 2 · 3 · 4번',font=font(38),fill='white')
d.text((28,80),'백식 · 제타 · GP02 하체 형태 참고 / WISP 비율 유지',font=font(24),fill='#aabbcc')
for i,r in enumerate(catalog['parts']):
    part=r['id'];folder=OUT/part;y=145+i*605;d.text((28,y),r['label_ko'],font=font(29),fill='#65e6ed')
    for x,name in [(0,part+'-05-45deg.png'),(540,part+'-07-Mounted-45deg.png')]:
        im=Image.open(folder/name).convert('RGB');im=im.crop((45,75,855,825)) if x==0 else im.crop((60,245,660,825));im.thumbnail((530,545));page.paste(im,(x+(540-im.width)//2,y+45))
    review=Image.new('RGB',(1080,1890),'#101722');rd=ImageDraw.Draw(review);rd.text((25,20),r['label_ko'],font=font(32),fill='white')
    views=[('정면','01-Front'),('우측면','02-Right'),('후면','03-Back'),('45도','05-45deg'),('실제 관절 굽힘','06-Bend-Pose'),('소체 장착','07-Mounted-45deg')]
    for j,(label,suffix) in enumerate(views):
        x=(j%2)*540;yy=90+(j//2)*595;rd.text((x+20,yy),label,font=font(26),fill='#65e6ed')
        im=Image.open(folder/(part+'-'+suffix+'.png')).convert('RGB');im=im.crop((45,75,855,825)) if j<5 else im.crop((60,245,660,825));im.thumbnail((530,540));review.paste(im,(x+(540-im.width)//2,yy+40))
    review.save(folder/(part+'-Review-Phone.png'))
page.save(OUT/'Biped-Four-Review-Phone.png')
# Standard update against the preserved previous module at the same camera scale.
comparison=Image.new('RGB',(1080,790),'#101722');cd=ImageDraw.Draw(comparison)
for x,label,p in [(0,'이전 기본 다리',BASE.parent/'WISP_Base_v1/LG01/LG01-05-45deg.png'),(540,'표준형 업데이트',OUT/'LB-02_STRIDER/LB-02_STRIDER-05-45deg.png')]:
    cd.text((x+35,25),label,font=font(30),fill='white');im=Image.open(p).convert('RGB').resize((900,900)).crop((45,75,855,825));im.thumbnail((530,680));comparison.paste(im,(x+(540-im.width)//2,90))
comparison.save(OUT/'Standard-Before-After.png')
for name,target in [('VoxelOn-Biped-Refined.log','Blender-Run.log'),('VoxelOn-Biped-Refined-Verify.log','Reopen-Validation.log'),('VoxelOn-Biped-Refined-01.log','Blender-Refine-01.log')]:shutil.copy2(Path.home()/'AppData/Local/Temp'/name,OUT/target)
lines=['# WISP 이족형 하체 4종 디자인 후보','',
'사용자가 번호별로 지정한 수정과 참고 로봇의 하체 사진을 바탕으로 만든 두 번째 Blender 후보입니다. 기존 파츠 번호와 혼동하지 않도록 임시 식별자 LB-01~LB-04를 사용했습니다.','',
'## 공통 기준','- 기존 허리 소켓 메시와 접점 높이 Z=0.505m를 그대로 유지했습니다. 골반을 포함하는 하체 전체 모듈입니다.','- 고관절·무릎·발목 높이는 모두 0.455 / 0.254 / 0.066m입니다. 다리 길이를 늘리지 않고 굵기·장갑 분할·골반 폭·발 크기로 차이를 냈습니다.','- 최초 LG01의 고관절 앞뒤/좌우, 무릎 앞뒤, 발목 앞뒤 회전 계층을 재사용했고 원통형 관절은 유지했습니다.','- 장착 모습은 최신 어깨 간격 수정 C-01을 사용했습니다. 몸통·머리·하박의 루트 변환은 바꾸지 않았습니다.','- 백식의 종아리 노출 프레임, 제타의 정강이/발목 장갑, GP02의 아래로 벌어지는 하퇴 외피를 참고했습니다. 세부 형태는 WISP에 맞춘 제작용 제안입니다. 고속/중량 명칭은 외형 방향이며 게임 성능값을 뜻하지 않습니다.','',
'## 실제 실행한 검사','- Blender 5.2.3으로 4종 각각 단독/장착 .blend를 생성했습니다.','- 각 파츠 정면·우측면·후면·좌측면·45도, 실제 관절 굽힘 자세, 소체 장착45도 PNG를 렌더했습니다.','- 양쪽 고관절 앞뒤 회전, 좌우 벌림, 무릎 굽힘, 발목 회전과 원위치 복귀를 실제로 적용해 하위 다리/발 이동을 검사했습니다.','- 기본 자세 메시의 좌우 대칭, 메시 데이터 유효성과 기존 허리 소켓 정점 동일 여부를 검사했습니다.','- 저장된 단독/장착 파일 8개를 다시 열어 소켓과 무릎 동작을 재검사했습니다.','- 기존 LG01과 최신 C-01 Blender 파일의 SHA256 보존을 확인했습니다.','- 디자인 확인 대기입니다. Unity 적용·게임 이동 속도/무게 조정·게임 보행/점프 검증·커밋·푸시는 수행하지 않았습니다. Blender 관절 검사는 게임 플레이 검증과 구분합니다.','',
'[전체 4종 비교](Biped-Four-Review-Phone.png) / [표준형 이전과 비교](Standard-Before-After.png) / [저장 파일 재검사](Reopen-Validation.json)','']
descriptions=['1번: 허벅지를 2번과 동일한 크기/형태로 확대. 종아리는 윗부분의 짧은 장갑만 남기고 레일·피스톤·연결 프레임 노출. 발은 낮고 앞코가 가늘어지는 형태.','2번: 발 폭·길이를 약 15% 축소. 무릎 아래 종아리를 도톰하게 하고 뒤쪽 외피가 발목 근처까지 내려옴.','3번: 2번의 허벅지·종아리·발·내부 프레임을 그대로 포함하고 외측 허벅지와 긴 정강이·발목 장갑을 추가.','4번: 종아리 위쪽은 좁고 아래쪽이 넓은 12면 플레어 덮개. 밑면을 막지 않은 속 빈 구조이며, 발은 넓고 큰 다각형 앞코.']
for r,description in zip(catalog['parts'],descriptions):
    p=r['id'];lines.extend([f'## {p} — {r["label_ko"]}',description,f'[검토 이미지]({p}/{p}-Review-Phone.png) / [Blender 원본]({p}/{p}.blend) / [장착 원본]({p}/{p}-Mounted.blend)',f'[정면]({p}/{p}-01-Front.png) / [우측면]({p}/{p}-02-Right.png) / [후면]({p}/{p}-03-Back.png) / [좌측면]({p}/{p}-04-Left.png) / [45도]({p}/{p}-05-45deg.png)',f'[관절 굽힘]({p}/{p}-06-Bend-Pose.png) / [장착]({p}/{p}-07-Mounted-45deg.png)',''])
lines.extend(['## 참고 자료와 해석','', '[백식 하체 실물 사진](https://tsumiplahguc.hatenablog.com/entry/hguc200hyakushiki) / [제타 하체 실물 사진](https://blog.livedoor.jp/hobisima-rittai/MSZ-006_2017-04-23) / [GP02 공식 제품 사진](https://www.bandaispirits.co.jp/products/search/detail.php?grp_id=5325&prd_id=4543112450692000) / [GP02 후면 사진](https://schizophonic9.com/re6/hguc_gp02.html)','', '사진을 직접 열어 형태를 확인했습니다. 참고 사진 자체는 재배포하지 않았고 WISP 결과물만 포함합니다. 색·문장·무장이나 원본 로봇 전체를 복제하는 작업은 아닙니다. 4번은 발목 쪽으로 넓어지는 종아리를 요청한 것으로 해석했습니다.','', '저장 파일에서 1번/2번 허벅지 형상 일치, 3번/2번 기본 다리 메시 일치, 2번 뒤쪽 외피가 1번보다 아래까지 내려오는 것, 4번 하단 폭이 상단보다 큰 것을 추가 검사했습니다.','', '[직전 후보와 비교](Biped-Refined-Before-After-Phone.png)'])
(OUT/'README.md').write_text('\n'.join(lines),encoding='utf-8')
print('BIPED_REVIEW_READY',len(catalog['parts']))
