$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$base=Split-Path $PSScriptRoot
$parts=@('W-02_BREAKER','W-03_RATTLE','W-04_EDGE','W-05_AEGIS','B-01_TWIN_DRIVE')
$refCrops=@{
 'W-02_BREAKER'=[Drawing.Rectangle]::new(1340,90,420,435)
 'W-03_RATTLE'=[Drawing.Rectangle]::new(1280,90,475,410)
 'W-04_EDGE'=[Drawing.Rectangle]::new(1070,80,450,560)
 'W-05_AEGIS'=[Drawing.Rectangle]::new(1140,80,315,475)
 'B-01_TWIN_DRIVE'=[Drawing.Rectangle]::new(1410,100,345,280)
}
$title=[Drawing.Font]::new('Segoe UI',24,[Drawing.FontStyle]::Bold)
$label=[Drawing.Font]::new('Segoe UI',16,[Drawing.FontStyle]::Bold)
$small=[Drawing.Font]::new('Segoe UI',12)
$white=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(236,240,245))
$muted=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(166,183,202))
$bg=[Drawing.Color]::FromArgb(16,21,28)
function Fit($g,$img,$rect){
 $s=[Math]::Min($rect.Width/$img.Width,$rect.Height/$img.Height)
 $w=[int]($img.Width*$s);$h=[int]($img.Height*$s)
 $g.DrawImage($img,$rect.X+[int](($rect.Width-$w)/2),$rect.Y+[int](($rect.Height-$h)/2),$w,$h)
}
function Crop-Model($img){
 $back=$img.GetPixel(0,0);$minX=$img.Width;$minY=$img.Height;$maxX=0;$maxY=0
 for($y=0;$y -lt $img.Height;$y+=4){for($x=0;$x -lt $img.Width;$x+=4){
  $c=$img.GetPixel($x,$y)
  if([Math]::Abs([int]$c.R-$back.R)+[Math]::Abs([int]$c.G-$back.G)+[Math]::Abs([int]$c.B-$back.B) -gt 38){
   $minX=[Math]::Min($minX,$x);$minY=[Math]::Min($minY,$y);$maxX=[Math]::Max($maxX,$x);$maxY=[Math]::Max($maxY,$y)
  }
 }}
 if($maxX -le $minX){throw 'No model detected in render'}
 $pad=35;$minX=[Math]::Max(0,$minX-$pad);$minY=[Math]::Max(0,$minY-$pad)
 $maxX=[Math]::Min($img.Width,$maxX+$pad);$maxY=[Math]::Min($img.Height,$maxY+$pad)
 return $img.Clone([Drawing.Rectangle]::new($minX,$minY,$maxX-$minX,$maxY-$minY),[Drawing.Imaging.PixelFormat]::Format32bppArgb)
}
$overview=[Drawing.Bitmap]::new(1080,3150);$og=[Drawing.Graphics]::FromImage($overview);$og.Clear($bg);$og.InterpolationMode='HighQualityBicubic'
$og.DrawString('WISP | NEXT FIVE PARTS',$title,$white,30,20)
$og.DrawString('Actual Blender renders | Each new design awaits PD review',$small,$muted,30,65)
$row=0
foreach($part in $parts){
 $dir=Join-Path $base $part
 $board=[Drawing.Bitmap]::new(1080,2890);$g=[Drawing.Graphics]::FromImage($board);$g.Clear($bg);$g.InterpolationMode='HighQualityBicubic'
 $g.DrawString($part+' | DESIGN REVIEW',$title,$white,30,20)
 $g.DrawString('Concept crop (left) / Actual Blender (right)',$label,$muted,30,70)
 $ref=[Drawing.Bitmap]::new((Join-Path $base "References/$part.png"));$crop=$ref.Clone($refCrops[$part],[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $hero=[Drawing.Bitmap]::new((Join-Path $dir "$part-04-45deg.png"));$heroCrop=Crop-Model $hero
 Fit $g $crop ([Drawing.Rectangle]::new(20,115,510,660));Fit $g $heroCrop ([Drawing.Rectangle]::new(550,115,510,660))
 $ref.Dispose();$crop.Dispose()
 $g.DrawString('FOUR VIEWS | shared orthographic scale per part',$label,$white,30,805)
 $spec=@(@('01-Front',20,850),@('02-Right',550,850),@('03-Rear',20,1370),@('04-45deg',550,1370))
 foreach($s in $spec){
  $im=[Drawing.Bitmap]::new((Join-Path $dir ($part+'-'+$s[0]+'.png')))
  $g.DrawString($s[0],$small,$muted,[int]$s[1]+12,[int]$s[2])
  Fit $g $im ([Drawing.Rectangle]::new([int]$s[1],[int]$s[2]+25,510,480));$im.Dispose()
 }
 $g.DrawString('MOUNT CLOSE-UP / MOUNTED WISP',$label,$white,30,1930)
 foreach($s in @(@('05-Mount-Closeup',20),@('Mounted-idle',550))){
  $im=[Drawing.Bitmap]::new((Join-Path $dir ($part+'-'+$s[0]+'.png')))
  Fit $g $im ([Drawing.Rectangle]::new([int]$s[1],1990,510,720));$im.Dispose()
 }
 # Draw caption bands last so differently proportioned crops cannot hide labels.
 $band=[Drawing.SolidBrush]::new($bg)
 $g.FillRectangle($band,0,790,1080,55);$g.FillRectangle($band,0,1910,1080,70);$g.FillRectangle($band,0,2750,1080,100)
 $g.DrawString('FOUR VIEWS | shared orthographic scale per part',$label,$white,30,805)
 $g.DrawString('MOUNT CLOSE-UP / MOUNTED WISP',$label,$white,30,1930)
 foreach($s in $spec){$g.DrawString($s[0],$small,$muted,[int]$s[1]+12,[int]$s[2])}
 $g.DrawString('Automation PASS | PD design PENDING | Unity not performed',$small,$muted,30,2775)
 $band.Dispose()
 $board.Save((Join-Path $dir "$part-Review-Phone.png"),[Drawing.Imaging.ImageFormat]::Png)
 $g.Dispose();$board.Dispose()
 $y=110+$row*600
 $og.DrawString($part,$title,$white,30,$y)
 Fit $og $heroCrop ([Drawing.Rectangle]::new(20,$y+55,610,510))
 $mounted=[Drawing.Bitmap]::new((Join-Path $dir "$part-Mounted-idle.png"))
 Fit $og $mounted ([Drawing.Rectangle]::new(650,$y+55,410,510))
 $mounted.Dispose();$hero.Dispose();$heroCrop.Dispose();$row++
 Write-Output "REVIEW_BOARD_PASS $part"
}
$overview.Save((Join-Path $base 'FirstBatch-Five-Review-Phone.png'),[Drawing.Imaging.ImageFormat]::Png)
$og.Dispose();$overview.Dispose()
