"""Portrait review pages from the actual four biped Blender renders."""
from pathlib import Path
import sys,json,shutil
sys.path.insert(0,str(Path.home()/'AppData/Local/Temp/VoxelOn-ImageTools'))
from PIL import Image,ImageDraw,ImageFont
BASE=Path(__file__).resolve().parent.parent;OUT=BASE/'Biped-Four-20261112'
catalog=json.loads((OUT/'PART_CATALOG.json').read_text());font=lambda n:ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',n)
page=Image.new('RGB',(1080,2600),'#101722');d=ImageDraw.Draw(page)
d.text((28,20),'WISP | 이족형 하체 4종',font=font(38),fill='white')
d.text((28,80),'같은 허리 높이 · 고속 경량 / 표준 / 중장갑 / 초중량',font=font(24),fill='#aabbcc')
for i,r in enumerate(catalog['parts']):
    part=r['id'];folder=OUT/part;y=145+i*605;d.text((28,y),part+' · '+r['label_ko'],font=font(29),fill='#65e6ed')
    for x,name in [(0,part+'-05-45deg.png'),(540,part+'-07-Mounted-45deg.png')]:
        im=Image.open(folder/name).convert('RGB');im=im.crop((45,75,855,825)) if x==0 else im.crop((60,245,660,825));im.thumbnail((530,545));page.paste(im,(x+(540-im.width)//2,y+45))
    review=Image.new('RGB',(1080,1890),'#101722');rd=ImageDraw.Draw(review);rd.text((25,20),part+' · '+r['label_ko'],font=font(32),fill='white')
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
for name,target in [('VoxelOn-Biped-Four.log','Blender-Run.log'),('VoxelOn-Biped-Verify.log','Reopen-Validation.log')]:shutil.copy2(Path.home()/'AppData/Local/Temp'/name,OUT/target)
lines=['# WISP 이족형 하체 4종 디자인 후보','',
'사용자의 네 가지 방향(아주 가늘고 약해 보이는 고속형 / 현행 표준형 업데이트 / 두꺼운 중장갑형 / 가장 육중한 초중량형)에 따른 별도 Blender 후보입니다. 기존 파츠 번호와 혼동하지 않도록 임시 식별자 LB-01~LB-04를 사용했습니다.','',
'## 공통 기준','- 기존 허리 소켓 메시와 접점 높이 Z=0.505m를 그대로 유지했습니다. 골반을 포함하는 하체 전체 모듈입니다.','- 고관절·무릎·발목 높이는 모두 0.455 / 0.254 / 0.066m입니다. 다리 길이를 늘리지 않고 굵기·장갑 분할·골반 폭·발 크기로 차이를 냈습니다.','- 최초 LG01의 고관절 앞뒤/좌우, 무릎 앞뒤, 발목 앞뒤 회전 계층을 재사용했고 원통형 관절은 유지했습니다.','- 장착 모습은 최신 어깨 간격 수정 C-01을 사용했습니다. 몸통·머리·하박의 루트 변환은 바꾸지 않았습니다.','- 전용 상세 컨셉아트는 새로 주어지지 않았으므로 세부 외형은 이번 제작용 제안입니다. 고속/중량 명칭은 외형 방향이며 게임 성능값을 뜻하지 않습니다.','',
'## 실제 실행한 검사','- Blender 5.2.3으로 4종 각각 단독/장착 .blend를 생성했습니다.','- 각 파츠 정면·우측면·후면·좌측면·45도, 실제 관절 굽힘 자세, 소체 장착45도 PNG를 렌더했습니다.','- 양쪽 고관절 앞뒤 회전, 좌우 벌림, 무릎 굽힘, 발목 회전과 원위치 복귀를 실제로 적용해 하위 다리/발 이동을 검사했습니다.','- 기본 자세 메시의 좌우 대칭, 메시 데이터 유효성과 기존 허리 소켓 정점 동일 여부를 검사했습니다.','- 저장된 단독/장착 파일 8개를 다시 열어 소켓과 무릎 동작을 재검사했습니다.','- 기존 LG01과 최신 C-01 Blender 파일의 SHA256 보존을 확인했습니다.','- 디자인 확인 대기입니다. Unity 적용·게임 이동 속도/무게 조정·게임 보행/점프 검증·커밋·푸시는 수행하지 않았습니다. Blender 관절 검사는 게임 플레이 검증과 구분합니다.','',
'[전체 4종 비교](Biped-Four-Review-Phone.png) / [표준형 이전과 비교](Standard-Before-After.png) / [저장 파일 재검사](Reopen-Validation.json)','']
descriptions=['얇은 허벅지와 종아리, 좁은 골반, 노출된 가는 프레임과 작은 발.','기존 관절 높이를 유지하고 각진 허벅지·종아리 외피, 무릎 보호판과 골반 장갑을 정리.','두꺼운 앞뒤 장갑과 겹친 정강이 판, 넓어진 골반과 발.','가장 굵은 다리 외피와 넓은 골반, 큰 원통 관절, 넓고 묵직한 발.']
for r,description in zip(catalog['parts'],descriptions):
    p=r['id'];lines.extend([f'## {p} — {r["label_ko"]}',description,f'[검토 이미지]({p}/{p}-Review-Phone.png) / [Blender 원본]({p}/{p}.blend) / [장착 원본]({p}/{p}-Mounted.blend)',f'[정면]({p}/{p}-01-Front.png) / [우측면]({p}/{p}-02-Right.png) / [후면]({p}/{p}-03-Back.png) / [좌측면]({p}/{p}-04-Left.png) / [45도]({p}/{p}-05-45deg.png)',f'[관절 굽힘]({p}/{p}-06-Bend-Pose.png) / [장착]({p}/{p}-07-Mounted-45deg.png)',''])
(OUT/'README.md').write_text('\n'.join(lines),encoding='utf-8')
print('BIPED_REVIEW_READY',len(catalog['parts']))
