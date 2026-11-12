import sys,os
from pathlib import Path
try:from PIL import Image,ImageFont,ImageDraw
except ModuleNotFoundError:
    sys.path.insert(0,str(Path(os.environ['TEMP'])/'VoxelOn-ImageTools'))
    from PIL import Image,ImageFont,ImageDraw
root=Path(__file__).resolve().parent.parent/'SlimHeads-20261112'
font=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',23);big=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',30)
canvas=Image.new('RGB',(900,1100),(13,18,25));d=ImageDraw.Draw(canvas);d.text((24,15),'슬림형 머리 / 두 방향의 제작 초안',fill='white',font=big)
mounted=Image.new('RGB',(900,750),(13,18,25));ImageDraw.Draw(mounted).text((24,15),'기존 몸통 장착 / 목 연결 유지',fill='white',font=big)
for i,(part,title,second) in enumerate([('H-05_NEEDLE','앞뒤로 길고 납작한 형','02-Right'),('H-06_LANCET','세로로 길고 갸름한 형','01-Front')]):
    y=90+i*500;d.text((24,y),part+' · '+title,fill='white',font=font)
    for col,suffix in enumerate(['05-45deg',second]):canvas.paste(Image.open(root/part/(part+'-'+suffix+'.png')).convert('RGB').resize((450,450)),(col*450,y+35))
    ImageDraw.Draw(mounted).text((i*450+16,80),part,fill='white',font=font)
    im=Image.open(root/part/(part+'-07-Mounted.png')).convert('RGB');mounted.paste(im.resize((450,600)),(i*450,120))
canvas.save(root/'Slim-Heads-Review-Phone.png');mounted.save(root/'Slim-Heads-Mounted-Phone.png')
print('SLIM_HEADS_REVIEW_COMPLETE')
