"""Package approved C01 and compose real-render portrait torso reviews."""
import sys,json,shutil,hashlib
from pathlib import Path
sys.path.insert(0,str(Path.home()/'AppData/Local/Temp/VoxelOn-ImageTools'))
from PIL import Image,ImageDraw,ImageFont
BASE=Path(__file__).resolve().parent.parent;OUT=BASE/'CoreDepth-Family-20261112'
parts=['C-01_STANDARD','C-02_LIGHTFRAME','C-03_BULWARK_CORE','C-04_ARC_CORE']
labels=['C-01 STANDARD · 균형형','C-02 LIGHTFRAME · 경량형','C-03 BULWARK CORE · 중장갑형','C-04 ARC CORE · 에너지형']
font=lambda n:ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',n)
dest=OUT/parts[0];dest.mkdir(exist_ok=True);src=BASE/'C01-CompactUpper-20261112'
for a,b in [('C01-Depth-01-Front.png',parts[0]+'-01-Front.png'),('C01-Depth-02-Side.png',parts[0]+'-02-Side.png'),('C01-Depth-03-45deg.png',parts[0]+'-03-45deg.png'),('C01-Depth-04-Mounted-45deg.png',parts[0]+'-04-Mounted-45deg.png'),('C-01_STANDARD-CompactUpper-v8.blend',parts[0]+'-Depth-v1.blend'),('C-01_STANDARD-CompactUpper-v8-Mounted.blend',parts[0]+'-Depth-v1-Mounted.blend'),('Validation.json','Validation.json')]:shutil.copy2(src/a,dest/b)
sheet=Image.new('RGB',(1080,2380),'#101722');d=ImageDraw.Draw(sheet)
d.text((28,20),'WISP | 몸통 4종 입체 구조 업데이트',font=font(36),fill='white')
d.text((28,80),'C-01 확인 방향 적용 · 공통 기계식 허리 · 기존 연결 규격 유지',font=font(23),fill='#aabbcc')
catalog=[]
for i,(part,label) in enumerate(zip(parts,labels)):
    folder=OUT/part;y=135+i*555;d.text((28,y),label,font=font(29),fill='#65e6ed')
    for x,name in [(0,part+'-03-45deg.png'),(540,part+'-04-Mounted-45deg.png')]:
        pic=Image.open(folder/name).convert('RGB');pic=pic.crop((40,190,860,770)) if x==0 else pic.crop((60,245,660,820));pic.thumbnail((530,490));sheet.paste(pic,(x+(540-pic.width)//2,y+48))
    # One review per new variant, all requested angles, paired at equal scale.
    if i:
        page=Image.new('RGB',(1080,2380),'#101722');pd=ImageDraw.Draw(page)
        pd.text((28,20),label,font=font(32),fill='white');pd.text((210,80),'이전',font=font(28),fill='#aabbcc');pd.text((740,80),'수정',font=font(28),fill='#65e6ed')
        for j,(text,before,after) in enumerate([('정면','Before-01-Front.png',part+'-01-Front.png'),('측면','Before-02-Right.png',part+'-02-Side.png'),('45도','Before-05-45deg.png',part+'-03-45deg.png'),('소체 장착','Before-07-Mounted.png',part+'-04-Mounted-45deg.png')]):
            yy=135+j*555;pd.text((28,yy),text,font=font(24),fill='#65e6ed')
            for xx,name in [(0,before),(540,after)]:
                pic=Image.open(folder/name).convert('RGB');pic=pic.crop((40,190,860,770)) if j<3 else pic.crop((60,245,660,820));pic.thumbnail((530,490));page.paste(pic,(xx+(540-pic.width)//2,yy+45))
        page.save(folder/(part+'-Before-After-Phone.png'))
        shutil.copy2(Path.home()/f'AppData/Local/Temp/VoxelOn-C0{i+1}-Depth.log',folder/'Blender-Run.log')
    with Image.open(folder/(part+'-01-Front.png')) as p:assert p.size==(900,900)
    catalog.append({'part':part,'geometry':part+'/'+part+'-Depth-v1.blend','mounted':part+'/'+part+'-Depth-v1-Mounted.blend','basis':'C01 v8 approved proportion and mechanical waist','design_status':'C01 direction accepted' if i==0 else 'PD review pending'})
sheet.save(OUT/'CoreDepth-Family-Phone.png')
shutil.copy2(Path.home()/'AppData/Local/Temp/VoxelOn-CoreDepth-Verify.log',OUT/'Reopen-Validation.log')
(OUT/'PART_CATALOG.json').write_text(json.dumps({'parts':catalog},indent=2),encoding='utf-8')
lines=['# WISP 몸통 4종 입체 구조 업데이트','',
'C-01 v8에서 확인받은 작은 흉부, 분리된 장갑, 얇은 기계식 허리 방향을 C-02~C-04에 확대했습니다. C-01은 확인받은 파일을 수정 없이 복사했고 나머지는 기존 원본에서 별도 후보로 제작했습니다.','',
'## 형태 차이','- C-01 STANDARD: 앞서 확인한 균형형. 상체를 10% 줄인 v8 그대로입니다.','- C-02 LIGHTFRAME: 좁은 가슴의 전면 중앙이 앞으로 모이는 쐐기 구조. 뒤쪽 폭과 하단 폭도 함께 수축합니다.','- C-03 BULWARK CORE: 가슴부터 등으로 이어지는 두꺼운 하우징에 상·하 흉부 장갑을 앞뒤로 겹쳤습니다.','- C-04 ARC CORE: 중앙이 실제로 열린 코어 베이와 돌출 테두리, 안쪽 발광 로드, 감싸는 좌우 장갑으로 구성했습니다.','',
'## 보존과 실제 검사','- 목·어깨·팔꿈치·허리 연결 좌표를 원본과 비교해 동일함을 확인했습니다. 기존 짧은 상박과 각 형식의 어깨 장갑은 유지합니다.','- 새 3종의 기계식 허리 메시 7개가 C-01 v8과 정점 좌표까지 동일함을 저장 파일을 다시 열어 검사했습니다.','- 새 몸통 메시의 데이터 유효성과 좌우 대칭을 검사했습니다. 기존 원본 해시도 보존 검사에 통과했습니다.','- 각 형식의 기존 폭·높이를 확대하지 않았으며 원본 외곽 깊이 안에서 앞·뒤 볼륨을 재배분했습니다.','- Blender 5.2.3으로 단독/장착 .blend와 정면·측면·45도·소체 장착45도 이미지를 생성했습니다. 실제 렌더 이미지를 열어 검토했습니다.','- C-02~C-04 디자인은 PD 확인 대기입니다. Unity 적용·게임 충돌/애니메이션 검사·커밋·푸시는 이번에 수행하지 않았습니다.','',
'[4종 전체 검토 이미지](CoreDepth-Family-Phone.png) / [저장 파일 재검사 결과](Reopen-Validation.json)','']
for p in parts:
    lines.extend([f'## {p}',f'[Blender 원본]({p}/{p}-Depth-v1.blend) / [장착 원본]({p}/{p}-Depth-v1-Mounted.blend)',f'[정면]({p}/{p}-01-Front.png) / [측면]({p}/{p}-02-Side.png) / [45도]({p}/{p}-03-45deg.png) / [소체 장착]({p}/{p}-04-Mounted-45deg.png)']+([f'[수정 전후 비교]({p}/{p}-Before-After-Phone.png)'] if p!=parts[0] else [])+[''])
(OUT/'README.md').write_text('\n'.join(lines),encoding='utf-8')
print('CORE_DEPTH_REVIEW_IMAGES_READY',len(parts))
