"""Compose actual rendered views and supplied reference crops; no generated artwork."""
import sys,json,os
from pathlib import Path
try:from PIL import Image,ImageDraw,ImageFont,ImageOps
except ModuleNotFoundError:
    sys.path.insert(0,str(Path(os.environ['TEMP'])/'VoxelOn-ImageTools'))
    from PIL import Image,ImageDraw,ImageFont,ImageOps
root=Path(__file__).resolve().parent.parent/'HeadCore-20261112'
items=json.loads((root/'Reference-Manifest.json').read_text())
font=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',23);large=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',29)
bg=(13,18,25)
def label(canvas,xy,text,big=False):ImageDraw.Draw(canvas).text(xy,text,fill=(230,238,245),font=large if big else font)
for group,prefix in [('Heads','H-'),('Cores','C-')]:
    ids=[x['id'] for x in items if x['id'].startswith(prefix)]
    canvas=Image.new('RGB',(900,1060),bg);label(canvas,(24,14),'머리 파츠 4종' if prefix=='H-' else '몸통 파츠 4종 · 짧은 상박 포함',True)
    mounted=Image.new('RGB',(900,1360),bg);label(mounted,(24,14),'기존 비율 장착 검토 / '+group,True)
    for i,part in enumerate(ids):
        x,y=i%2*450,70+i//2*490
        label(canvas,(x+16,y),part.replace('_',' '));im=Image.open(root/part/(part+'-05-45deg.png')).convert('RGB');canvas.paste(im.resize((450,450)),(x,y+32))
        yy=70+i//2*640;label(mounted,(x+16,yy),part.replace('_',' '));im=Image.open(root/part/(part+'-07-Mounted.png')).convert('RGB');mounted.paste(im.resize((450,600)),(x,yy+32))
    canvas.save(root/(group+'-Review-Phone.png'));mounted.save(root/(group+'-Mounted-Phone.png'))
crops={'H-01_RADOME':(180,650,770,1160),'H-02_LONGSIGHT':(140,70,950,700),'H-03_SWIFTFIN':(170,60,940,660),'H-04_BULWARK':(180,650,840,1170),'C-01_STANDARD':(80,140,920,670),'C-02_LIGHTFRAME':(0,690,520,1160),'C-03_BULWARK_CORE':(80,150,970,650),'C-04_ARC_CORE':(110,130,930,660)}
for item in items:
    part=item['id'];canvas=Image.new('RGB',(900,1320),bg);label(canvas,(25,14),part.replace('_',' ')+' / 형태 비교',True)
    label(canvas,(25,62),'제공 컨셉 (일부 영역)');reference=Image.open(root/'References'/(part+'.jpg')).convert('RGB').crop(crops[part]);reference=ImageOps.contain(reference,(850,540));canvas.paste(reference,((900-reference.width)//2,102+(540-reference.height)//2))
    label(canvas,(25,665),'Blender 제작 후보 · 디자인 검토 대기');render=Image.open(root/part/(part+('-05-45deg.png' if part.startswith('H-') else '-01-Front.png'))).convert('RGB');render=ImageOps.contain(render,(850,580));canvas.paste(render,((900-render.width)//2,710))
    canvas.save(root/part/(part+'-Concept-Comparison-Phone.png'))
print('HEAD_CORE_COMPARISONS_COMPLETE 8')

cores=[x['id'] for x in items if x['id'].startswith('C-')]
canvas=Image.new('RGB',(900,1100),bg);label(canvas,(24,14),'몸통 · 어깨 실루엣 비교',True);label(canvas,(24,56),'동일한 촬영 크기 / 색과 장식을 제외한 실제 외곽')
for i,part in enumerate(cores):
    x,y=i%2*450,110+i//2*490;label(canvas,(x+16,y),part.replace('_',' '));im=Image.open(root/part/(part+'-08-Silhouette-Front.png')).convert('RGB');canvas.paste(im.resize((450,450)),(x,y+32))
canvas.save(root/'Cores-Silhouette-Phone.png')
