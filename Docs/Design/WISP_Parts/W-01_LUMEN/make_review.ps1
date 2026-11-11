$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$dir=$PSScriptRoot
$ref=[Drawing.Bitmap]::new((Join-Path $dir '../References/W-01_LUMEN.png'))
$board=[Drawing.Bitmap]::new(1080,3520)
$g=[Drawing.Graphics]::FromImage($board);$g.Clear([Drawing.Color]::FromArgb(17,22,29));$g.InterpolationMode='HighQualityBicubic'
$title=[Drawing.Font]::new('Segoe UI',24,[Drawing.FontStyle]::Bold)
$small=[Drawing.Font]::new('Segoe UI',14)
$white=[Drawing.SolidBrush]::new([Drawing.Color]::White)
$g.DrawString('W-01 LUMEN | FIRST DESIGN GATE',$title,$white,30,20)
$g.DrawString('Concept (left) / Blender (right) | PD approved 2026-11-12',$small,$white,30,65)
$g.DrawString('Raw four-view renders share 0.96 orthographic scale. Review crops enlarged independently.',$small,$white,30,95)
function Fit($img,$r){
 $s=[Math]::Min($r.Width/$img.Width,$r.Height/$img.Height)
 $w=[int]($img.Width*$s);$h=[int]($img.Height*$s)
 $g.DrawImage($img,$r.X+[int](($r.Width-$w)/2),$r.Y+[int](($r.Height-$h)/2),$w,$h)
}
$rows=@(
 @{name='FRONT'; y=145; ref=[Drawing.Rectangle]::new(70,145,155,290); crop=[Drawing.Rectangle]::new(360,270,380,560); file='01-Front'},
 @{name='RIGHT SIDE'; y=770; ref=[Drawing.Rectangle]::new(270,145,450,290); crop=[Drawing.Rectangle]::new(110,280,880,520); file='02-Right'},
 @{name='REAR'; y=1395; ref=[Drawing.Rectangle]::new(740,145,180,290); crop=[Drawing.Rectangle]::new(360,270,380,560); file='03-Rear'},
 @{name='45 DEGREE'; y=2020; ref=[Drawing.Rectangle]::new(1365,115,390,330); crop=[Drawing.Rectangle]::new(90,220,920,700); file='04-45deg'}
)
foreach($r in $rows){
 $g.DrawString($r.name,$title,$white,30,$r.y)
 $a=$ref.Clone($r.ref,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $im=[Drawing.Bitmap]::new((Join-Path $dir ('W-01_LUMEN-'+$r.file+'.png')))
 $b=$im.Clone($r.crop,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 Fit $a ([Drawing.Rectangle]::new(20,$r.y+50,510,550))
 Fit $b ([Drawing.Rectangle]::new(550,$r.y+50,510,550))
 $a.Dispose();$b.Dispose();$im.Dispose()
}
$g.DrawString('MOUNTED WISP | idle / aimed',$title,$white,30,2650)
foreach($pose in @('idle','aimed')){
 $im=[Drawing.Bitmap]::new((Join-Path $dir "W-01_LUMEN-Mounted-$pose.png"))
 $x=if($pose -eq 'idle'){20}else{550}
 Fit $im ([Drawing.Rectangle]::new($x,2700,510,700));$im.Dispose()
}
$g.DrawString('Automation PASS | Design PD APPROVED | Unity NOT performed',$small,$white,30,3450)
$board.Save((Join-Path $dir 'W-01_LUMEN-Comparison-Phone.png'),[Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$board.Dispose();$ref.Dispose()
'W01_REVIEW_BOARD_PASS'
