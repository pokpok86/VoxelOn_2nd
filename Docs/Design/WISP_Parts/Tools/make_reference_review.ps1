$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$base=Join-Path (Split-Path $PSScriptRoot) 'ReferenceBatch-20261112'
$groups=@(@('B-01_SKYWING','B-02_CROSSLANCE','B-03_OVERTHRUST','B-04_LONGTANK','B-05_RAMJET'),@('L-01_STRIDER','L-02_BASTION_TRACK','L-03_TARANTULAN','L-05_SKYLIFT'))
$crops=@{
 'B-01_SKYWING'=[Drawing.Rectangle]::new(20,145,930,515)
 'B-02_CROSSLANCE'=[Drawing.Rectangle]::new(285,85,665,600)
 'B-03_OVERTHRUST'=[Drawing.Rectangle]::new(0,155,580,490)
 'B-04_LONGTANK'=[Drawing.Rectangle]::new(0,105,670,445)
 'B-05_RAMJET'=[Drawing.Rectangle]::new(175,60,780,465)
}
$bg=[Drawing.Color]::FromArgb(16,21,28)
$title=[Drawing.Font]::new('Segoe UI',25,[Drawing.FontStyle]::Bold)
$label=[Drawing.Font]::new('Segoe UI',16,[Drawing.FontStyle]::Bold)
$small=[Drawing.Font]::new('Segoe UI',12)
$white=[Drawing.Brushes]::White
$muted=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(162,185,202))
function Fit($g,$image,$rect){
 $s=[Math]::Min($rect.Width/$image.Width,$rect.Height/$image.Height)
 $w=[int]($image.Width*$s);$h=[int]($image.Height*$s)
 $g.DrawImage($image,($rect.X+[int](($rect.Width-$w)/2)),($rect.Y+[int](($rect.Height-$h)/2)),$w,$h)
}
function FileFit($g,$path,$rect){$img=[Drawing.Image]::FromFile($path);Fit $g $img $rect;$img.Dispose()}
for($groupIndex=0;$groupIndex -lt 2;$groupIndex++){
 $parts=$groups[$groupIndex]
 $overview=[Drawing.Bitmap]::new(1080,(130+$parts.Count*620))
 $og=[Drawing.Graphics]::FromImage($overview);$og.Clear($bg)
 $heading=if($groupIndex -eq 0){'BACKPACKS / REFERENCE CANDIDATES'}else{'LOWER BODY / REFERENCE CANDIDATES'}
 $og.DrawString($heading,$title,$white,20,25)
 $og.DrawString('Current WISP proportions preserved',$small,$muted,20,75)
 for($i=0;$i -lt $parts.Count;$i++){
  $p=$parts[$i];$dir=Join-Path $base $p
  $mounted=if($groupIndex -eq 0){'Mounted-Rear'}else{'Mounted-Front'}
  $ref=[Drawing.Bitmap]::new((Join-Path $base ('References/'+$p+'.jpg')))
  $crop=if($crops.ContainsKey($p)){$crops[$p]}else{[Drawing.Rectangle]::new(500,650,510,540)}
  $hero=$ref.Clone($crop,$ref.PixelFormat)
  $board=[Drawing.Bitmap]::new(1080,3040);$g=[Drawing.Graphics]::FromImage($board);$g.Clear($bg)
  $g.DrawString($p,$title,$white,25,20)
  $g.DrawString('REFERENCE / BLENDER CANDIDATE',$label,$muted,25,90)
  Fit $g $hero ([Drawing.Rectangle]::new(20,135,510,555))
  FileFit $g (Join-Path $dir "$p-04-45deg.png") ([Drawing.Rectangle]::new(550,135,510,555))
  foreach($s in @(@('01-Front',20,760),@('02-Right',550,760),@('03-Rear',20,1290),@('05-Top',550,1290))){
   $g.DrawString($s[0],$label,$muted,[int]$s[1]+10,[int]$s[2])
   FileFit $g (Join-Path $dir ($p+'-'+$s[0]+'.png')) ([Drawing.Rectangle]::new([int]$s[1],[int]$s[2]+35,510,480))
  }
  $g.DrawString('COMMON CONNECTION / CURRENT WISP',$label,$white,25,1870)
  FileFit $g (Join-Path $dir "$p-06-Mount-Closeup.png") ([Drawing.Rectangle]::new(20,1935,510,870))
  FileFit $g (Join-Path $dir "$p-$mounted.png") ([Drawing.Rectangle]::new(550,1935,510,870))
  $g.DrawString('Design review pending / Blender only',$small,$muted,25,2930)
  $board.Save((Join-Path $dir "$p-Comparison-Phone.png"),[Drawing.Imaging.ImageFormat]::Png)
  $g.Dispose();$board.Dispose()
  $y=125+$i*620;$og.DrawString($p,$label,$white,25,$y)
  FileFit $og (Join-Path $dir "$p-04-45deg.png") ([Drawing.Rectangle]::new(15,$y+45,515,540))
  FileFit $og (Join-Path $dir "$p-$mounted.png") ([Drawing.Rectangle]::new(550,$y+45,515,540))
  $hero.Dispose();$ref.Dispose();Write-Output "REFERENCE_BOARD_PASS $p"
 }
 $fileName=if($groupIndex -eq 0){'Backpacks-Review-Phone.png'}else{'LowerBody-Review-Phone.png'}
 $overview.Save((Join-Path $base $fileName),[Drawing.Imaging.ImageFormat]::Png);$og.Dispose();$overview.Dispose()
}
$title.Dispose();$label.Dispose();$small.Dispose();$muted.Dispose()
