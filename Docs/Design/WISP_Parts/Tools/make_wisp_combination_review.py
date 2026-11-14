"""Compose portrait assembly and detail sheets from actual Blender renders."""
import sys,json,shutil
from pathlib import Path
sys.path.insert(0,str(Path.home()/'AppData/Local/Temp/VoxelOn-ImageTools'))
from PIL import Image,ImageDraw,ImageFont
OUT=Path(__file__).resolve().parent.parent/'Combination-Review-20261112'
catalog=json.loads((OUT/'PART_COMBINATIONS.json').read_text(encoding='utf-8'))['combinations'];font=lambda n:ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',n)
roles=['경량 돌격 조합','표준 사격 조합','중장갑 방어 조합','에너지 중화기 조합']
sheet=Image.new('RGB',(1080,3440),'#101722');d=ImageDraw.Draw(sheet)
d.text((25,20),'WISP | 최신 파츠 조합과 포인트',font=font(36),fill='white')
d.text((25,78),'허벅지와 발 방향 유지 · 식별 문자 · 작은 발광부 · 스트라이프',font=font(22),fill='#aabbcc')
details=Image.new('RGB',(1080,1810),'#101722');dd=ImageDraw.Draw(details);dd.text((25,20),'작은 문자 · 발광 · 스트라이프 확대',font=font(33),fill='white')
for i,(r,role) in enumerate(zip(catalog,roles)):
    n=r['number'];y=135+i*810;d.text((25,y),str(i+1)+'번 하체 · '+role,font=font(30),fill='#65e6ed')
    for x,suffix in [(0,'01-Front45'),(540,'03-Back45')]:
        im=Image.open(OUT/n/(n+'-'+suffix+'.png')).convert('RGB');im.thumbnail((530,742));sheet.paste(im,(x+(540-im.width)//2,y+45))
    x=(i%2)*540;dy=85+(i//2)*850;dd.text((x+20,dy),str(i+1)+' · '+role,font=font(25),fill='#65e6ed');im=Image.open(OUT/n/(n+'-04-Points-Closeup.png')).convert('RGB');im.thumbnail((530,790));details.paste(im,(x+(540-im.width)//2,dy+42))
sheet.save(OUT/'Combinations-Four-Phone.png');details.save(OUT/'Point-Details-Phone.png')
for source,target in [('VoxelOn-WISP-Combinations.log','Blender-Build.log'),('VoxelOn-WISP-Combination-Framing.log','Blender-Framing.log'),('VoxelOn-WISP-Combination-Verify.log','Blender-Reopen-Verify.log')]:shutil.copy2(Path.home()/'AppData/Local/Temp'/source,OUT/target)
lines=['# WISP 최신 파츠 조합·포인트 미리보기','',
'사용자가 확인 중인 최신 다리 자세(좁힌 골반, 다리 벌림, 허벅지와 같은 발 방향)를 유지한 4가지 조합입니다. 머리·몸통·하체·좌우 하박 무장·백팩을 기존 Blender 파츠로 조립했습니다. 각 원본 파일은 보존하고 별도 조합 파일에만 표시를 추가했습니다.','',
'## 추가한 시각 포인트','- 가슴과 어깨의 작은 기체 번호 및 WISP 표기.','- 어깨의 짧은 주황 스트라이프.','- 허벅지의 작은 정비 식별 문자, 무장 표면의 짧은 코드.','- 가슴/종아리의 작은 청록색 발광 스트립과 기존 렌즈·포구·코어·추진기 발광.','- 표시와 빛은 표면에 가깝게 배치했고 해당 파츠에 부모 연결해 함께 움직이도록 했습니다.','- 발광은 정적 재질 표현이며 점멸 애니메이션이나 새 기능/성능 수치는 추가하지 않았습니다.','',
'## 조합 목록','| 조합 | 머리 | 몸통 | 하체 | 오른팔 | 왼팔 | 백팩 |','| --- | --- | --- | --- | --- | --- | --- |']
for r,role in zip(catalog,roles):lines.append(f'| {r["number"]} {role} | {r["head"]} | {r["core"]} | {r["lower"]} | {r["right_weapon"]} | {r["left_weapon"]} | {r["backpack"]} |')
lines.extend(['','## 실제 확인','- Blender 5.2.3으로 조합별 앞45도·정면·뒤45도·포인트 확대 PNG와 수정 가능한 .blend를 생성했습니다.','- 실제 렌더를 열어 구도를 검토하고 긴 무장 끝이 잘리지 않도록 전체 구도를 다시 맞췄습니다. 카메라 구도만 조정했으며 파츠 크기는 바꾸지 않았습니다.','- 저장 파일 4개를 다시 열어 목과 허리 접점 오차, 팔꿈치 축과 무장 루트 일치, 최신 다리/발 자세와 바닥 접촉을 검사했습니다.','- 문자/표면 포인트가 파츠에 부모 연결되어 있고, 각 조합에 실제 발광 재질 스트립이 있는지 검사했습니다.','- 모든 입력 원본의 SHA256 보존 검사 통과. 검사 기록은 Reopen-Validation.json입니다.','- 디자인 확인 대기입니다. 전체 움직임/충돌 검증·Unity 적용·성능 변경·커밋·푸시는 이번에 수행하지 않았습니다.','', '[전체 조합](Combinations-Four-Phone.png) / [포인트 확대](Point-Details-Phone.png) / [실제 검사 기록](Reopen-Validation.json)',''])
for r,role in zip(catalog,roles):
    n=r['number'];lines.extend([f'## {n} {role}',f'[Blender 원본]({r["blend"]})',f'[앞45도]({n}/{n}-01-Front45.png) / [정면]({n}/{n}-02-Front.png) / [뒤45도]({n}/{n}-03-Back45.png) / [포인트 확대]({n}/{n}-04-Points-Closeup.png)',''])
(OUT/'README.md').write_text('\n'.join(lines),encoding='utf-8')
print('COMBINATION_REVIEW_READY',len(catalog))
