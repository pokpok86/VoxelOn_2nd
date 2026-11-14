"""Compose a portrait review sheet from actual Blender renders."""
import sys,json,shutil
from pathlib import Path
sys.path.insert(0,str(Path.home()/'AppData/Local/Temp/VoxelOn-ImageTools'))
from PIL import Image,ImageDraw,ImageFont
out=Path(__file__).resolve().parent.parent/'C01-Depth-Review-20261112'
font=lambda size:ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',size)
sheet=Image.new('RGB',(1080,1850),'#101722');d=ImageDraw.Draw(sheet)
d.text((36,24),'C-01 STANDARD | 흉부 깊이 수정',font=font(36),fill='white')
d.text((36,82),'동일한 카메라 · 동일한 폭과 높이 · 연결 규격 유지',font=font(23),fill='#a7bec9')
d.text((200,128),'이전',font=font(30),fill='#a7bec9');d.text((742,128),'수정',font=font(30),fill='#65e6ed')
rows=[('45도','Before-05-45deg.png','C01-Depth-03-45deg.png'),('측면','Before-02-Right.png','C01-Depth-02-Side.png'),('기본 소체 장착','Before-07-Mounted.png','C01-Depth-04-Mounted-45deg.png')]
for i,(label,a,b) in enumerate(rows):
    y=185+i*550;d.text((30,y),label,font=font(25),fill='#65e6ed')
    for x,name in [(0,a),(540,b)]:
        im=Image.open(out/name).convert('RGB')
        # Equal crop for each comparison pair, never independently rescale the model.
        im=im.crop((95,190,805,750)) if i<2 else im.crop((65,230,655,835))
        im.thumbnail((530,485));sheet.paste(im,(x+(540-im.width)//2,y+44))
sheet.save(out/'C01-Depth-Before-After-Phone.png')
front=Image.new('RGB',(1080,650),'#101722');dr=ImageDraw.Draw(front)
for x,label,name in [(0,'이전','Before-01-Front.png'),(540,'수정','C01-Depth-01-Front.png')]:
    dr.text((x+210,16),label,font=font(30),fill='white')
    im=Image.open(out/name).convert('RGB').crop((95,190,805,750));im.thumbnail((530,540));front.paste(im,(x,80))
front.save(out/'C01-Depth-Front-Compare.png')
shutil.copy2(Path.home()/'AppData/Local/Temp/VoxelOn-C01-Depth-Review.log',out/'Blender-Run.log')
r=json.loads((out/'Validation.json').read_text())
before=r['before_size_xyz_m'];after=r['after_size_xyz_m']
text=f'''# C-01 STANDARD 흉부 깊이 수정 후보

사용자 요청에 따라 C-01만 별도 제작했습니다. C-02~C-04 확대 작업은 디자인 확인 후 진행합니다.

## 구조 변경
- 중앙 흉부를 앞으로 돌출시키고 넓은 경사면으로 측면 장갑에 이어지게 구성했습니다.
- 좌우 장갑은 중앙보다 뒤로 배치하여 겹침과 깊이가 보이게 했습니다.
- 가슴 아래 프레임과 장갑이 허리로 내려가며 좁아지도록 재구성했습니다.
- 어깨 장갑 외측만 뒤로 살짝 물렸습니다. 목·어깨 회전축·팔꿈치·허리 인터페이스 및 짧은 상박 치수는 그대로입니다.
- 장착 이미지에서 머리·하박·다리 루트 변환은 원본과 동일합니다.

## 실제 확인
- Blender {r['blender']}: 원본 파일 생성과 4개 시점 렌더 완료.
- 전체 폭 / 깊이 / 높이: 이전 {before[0]:.6f} / {before[1]:.6f} / {before[2]:.6f} m → 수정 {after[0]:.6f} / {after[1]:.6f} / {after[2]:.6f} m.
- 폭과 높이는 동일하며 외곽 깊이도 확대하지 않았습니다. 기존 덕트가 차지하던 전면 공간에 흉부를 배치했습니다.
- 중앙 장갑 전면 Y: {r['before_central_armor_front_y_m']:.4f} → {r['after_central_armor_front_y_m']:.4f} m. 수정 측면 장갑 전면은 Y -0.060~ -0.034 m입니다. 전면 방향은 -Y입니다.
- 연결부 중심과 경계 좌표가 수정 전후 동일함을 검사했습니다.
- 보존 대상으로 기록한 기존 Blender 원본 {len(r['source_hashes'])}개 파일의 SHA256이 모두 동일합니다.
- 실제 렌더 이미지를 열어 정면·측면·45도·소체 장착 형태를 확인했습니다.
- 디자인: PD 확인 대기. Unity 적용, 게임 동작·충돌 검증, 커밋·푸시는 수행하지 않았습니다.

## 검토 파일
- [수정 전후 세로 비교](C01-Depth-Before-After-Phone.png)
- [정면](C01-Depth-01-Front.png) / [측면](C01-Depth-02-Side.png) / [45도](C01-Depth-03-45deg.png)
- [기본 소체 장착 45도](C01-Depth-04-Mounted-45deg.png)
- [단독 Blender 원본](C-01_STANDARD-Depth-v1.blend)
- [소체 장착 Blender 원본](C-01_STANDARD-Depth-v1-Mounted.blend)
- [검사 기록](Validation.json) / [실행 로그](Blender-Run.log)

원본 HeadCore-20261112 폴더는 변경하지 않았습니다. 관절 중심을 고정한 상태에서 흉부와 측면 장갑의 깊이 차이로 어깨 연결이 가슴 뒤쪽에 보이도록 했습니다.
'''
(out/'README.md').write_text(text,encoding='utf-8')
print(out/'C01-Depth-Before-After-Phone.png')
