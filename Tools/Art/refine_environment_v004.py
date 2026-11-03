"""Author an opaque canyon perimeter in Blender, preserving earlier sources.
Open environment v003. Base walls align to the existing +/-140m gameplay edge.
"""
import bpy, math, os
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
source=open(os.path.join(BASE,'Tools/Art/refine_environment_v002.py'),encoding='utf-8-sig').read()
exec(source.split("for kind,name in enumerate")[0])
k=Kit('CanyonBoundary')
for axis in [0,1]:
 for side in [-1,1]:
  # Opaque base stops gaps behind the surface tiles; its inside face is 140.04.
  p=(side*142.52,0,12) if axis==0 else (0,side*142.52,12)
  size=(4.96,290,24) if axis==0 else (290,4.96,24)
  k.box(p,size,6)
  for seg in range(18):
   along=-136+seg*16;h=24+(seg*7+axis*3+(1 if side>0 else 0))%6*2
   # Flat safe inside face; variable eroded crowns stay outside the playable area.
   p=(side*142.52,along,h/2) if axis==0 else (along,side*142.52,h/2)
   k.box(p,(4.94,15.98,h) if axis==0 else (15.98,4.94,h),7 if seg%4==0 else 6)
   for row in range(int(h)):
    for col in range(8):
     a=along-8+(col+.5)*2
     shade=7 if row%8==0 else 8 if row%8 in [1,2] else 6
     if (col*7+row*11+seg)%41==0:shade=7
     depth=.06 if row%5 else .08
     p=(side*140.03,a,row+.5) if axis==0 else (a,side*140.03,row+.5)
     k.box(p,(depth,1.98,.98) if axis==0 else (1.98,depth,.98),shade)
   if seg%3==0:
    p=(side*142.5,along,h+.06) if axis==0 else (along,side*142.5,h+.06)
    k.box(p,(4.8,12,.12) if axis==0 else (12,4.8,.12),9)
k.finish()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'ArtSource/Environment/VoxelOn_Environment_v004.blend'))
print('VOXELON_ENVIRONMENT_V004_PASS CanyonBoundary, playable edge +/-140m preserved')
