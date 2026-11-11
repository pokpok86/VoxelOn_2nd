$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$dir=$PSScriptRoot
$board=[Drawing.Bitmap]::new(1080,2340)
$g=[Drawing.Graphics]::FromImage($board)
$g.Clear([Drawing.Color]::FromArgb(15,19,26))
$g.InterpolationMode='HighQualityBicubic'
$font=[Drawing.Font]::new('Segoe UI',24,[Drawing.FontStyle]::Bold)
$small=[Drawing.Font]::new('Segoe UI',16)
$brush=[Drawing.SolidBrush]::new([Drawing.Color]::White)
$g.DrawString('WISP | CONCEPT REFINEMENT', $font,$brush,30,20)
$g.DrawString('Concept crops (left) / Actual Blender renders (right)', $small,$brush,30,65)
$ref=[Drawing.Bitmap]::new((Join-Path $dir 'WISP-Latest-Concept.png'))
function Fit($img,$r){
 $s=[Math]::Min($r.Width/$img.Width,$r.Height/$img.Height)
 $w=[int]($img.Width*$s);$h=[int]($img.Height*$s)
 $g.DrawImage($img,$r.X+[int](($r.Width-$w)/2),$r.Y+[int](($r.Height-$h)/2),$w,$h)
}
$rows=@(
 @{title='HD-01 | Sensor head'; y=115; crop=[Drawing.Rectangle]::new(510,80,215,205); rcrop=[Drawing.Rectangle]::new(220,180,700,760); file='HD01/HD01-05-45deg.png'},
 @{title='BD-01 | Chest and shoulders'; y=835; crop=[Drawing.Rectangle]::new(260,195,630,355); rcrop=[Drawing.Rectangle]::new(80,180,940,820); file='BD01/BD01-05-45deg.png'},
 @{title='LG-01 | Leg armor and joints'; y=1555; crop=[Drawing.Rectangle]::new(260,600,670,760); rcrop=[Drawing.Rectangle]::new(220,100,660,910); file='LG01/LG01-05-45deg.png'}
)
foreach($r in $rows){
 $g.DrawString($r.title,$font,$brush,30,$r.y)
 $crop=$ref.Clone($r.crop,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $render=[Drawing.Bitmap]::new((Join-Path $dir $r.file))
 Fit $crop ([Drawing.Rectangle]::new(20,$r.y+55,510,630))
 $renderCrop=$render.Clone($r.rcrop,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 Fit $renderCrop ([Drawing.Rectangle]::new(550,$r.y+55,510,630))
 $crop.Dispose();$render.Dispose();$renderCrop.Dispose()
}
$g.DrawString('Design review pending | No Unity import',$small,$brush,30,2290)
$board.Save((Join-Path $dir 'WISP-Concept-Refinement-Phone.png'),[Drawing.Imaging.ImageFormat]::Png)
$ref.Dispose();$g.Dispose();$board.Dispose()
'CONCEPT_REFINEMENT_COMPARISON_PASS'
