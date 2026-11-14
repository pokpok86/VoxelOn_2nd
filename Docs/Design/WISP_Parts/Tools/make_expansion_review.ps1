$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$base=Split-Path $PSScriptRoot
$parts=@('B-02_SKYWING','B-03_CARGO_PACK','L-02_BASTION_TRACK','L-03_TARANTULAN','L-04_DRAFT_HOVER')
$bg=[Drawing.Color]::FromArgb(16,21,28)
$title=[Drawing.Font]::new('Segoe UI',26,[Drawing.FontStyle]::Bold)
$label=[Drawing.Font]::new('Segoe UI',17,[Drawing.FontStyle]::Bold)
$small=[Drawing.Font]::new('Segoe UI',13)
$white=[Drawing.Brushes]::White
$muted=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(162,185,202))
function Fit($g,$path,$rect){
 $im=[Drawing.Image]::FromFile($path)
 $scale=[Math]::Min($rect.Width/$im.Width,$rect.Height/$im.Height)
 $w=[int]($im.Width*$scale);$h=[int]($im.Height*$scale)
 $g.DrawImage($im,($rect.X+[int](($rect.Width-$w)/2)),($rect.Y+[int](($rect.Height-$h)/2)),$w,$h)
 $im.Dispose()
}
$overview=[Drawing.Bitmap]::new(1080,3300)
$og=[Drawing.Graphics]::FromImage($overview);$og.Clear($bg)
$og.DrawString('WISP / NEXT FIVE PARTS',$title,$white,25,25)
$og.DrawString('Same approved base / new design candidates',$small,$muted,25,75)
for($i=0;$i -lt $parts.Count;$i++){
 $p=$parts[$i];$dir=Join-Path $base $p
 $mounted=if($p.StartsWith('B-')){'Mounted-Rear'}else{'Mounted-Front'}
 $board=[Drawing.Bitmap]::new(1080,2820)
 $g=[Drawing.Graphics]::FromImage($board);$g.Clear($bg)
 $g.DrawString($p,$title,$white,25,22)
 $g.DrawString('WISP family / design candidate',$small,$muted,25,75)
 Fit $g (Join-Path $dir "$p-04-45deg.png") ([Drawing.Rectangle]::new(20,110,1040,650))
 $spec=@(@('01-Front',20,805),@('02-Right',550,805),@('03-Rear',20,1330),@('05-Top',550,1330))
 foreach($s in $spec){
  $g.DrawString($s[0],$label,$muted,[int]$s[1]+10,[int]$s[2])
  Fit $g (Join-Path $dir ($p+'-'+$s[0]+'.png')) ([Drawing.Rectangle]::new([int]$s[1],[int]$s[2]+36,510,470))
 }
 $g.DrawString('CONNECTION / MOUNTED WISP',$label,$white,25,1870)
 Fit $g (Join-Path $dir "$p-06-Mount-Closeup.png") ([Drawing.Rectangle]::new(20,1930,510,720))
 Fit $g (Join-Path $dir "$p-$mounted.png") ([Drawing.Rectangle]::new(550,1930,510,720))
 $g.DrawString('PD design review pending',$small,$muted,25,2730)
 $board.Save((Join-Path $dir "$p-Review-Phone.png"),[Drawing.Imaging.ImageFormat]::Png)
 $g.Dispose();$board.Dispose()
 $y=130+$i*620
 $og.DrawString($p,$label,$white,25,$y)
 Fit $og (Join-Path $dir "$p-04-45deg.png") ([Drawing.Rectangle]::new(15,$y+45,515,540))
 Fit $og (Join-Path $dir "$p-$mounted.png") ([Drawing.Rectangle]::new(550,$y+45,515,540))
 Write-Output "EXPANSION_BOARD_PASS $p"
}
$overview.Save((Join-Path $base 'Expansion-Five-Review-Phone.png'),[Drawing.Imaging.ImageFormat]::Png)
$og.Dispose();$overview.Dispose();$title.Dispose();$label.Dispose();$small.Dispose();$muted.Dispose()
