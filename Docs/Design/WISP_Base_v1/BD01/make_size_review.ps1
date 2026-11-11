$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$canvas=[Drawing.Bitmap]::new(1100,2360)
$g=[Drawing.Graphics]::FromImage($canvas)
$g.Clear([Drawing.Color]::FromArgb(16,21,28))
$font=[Drawing.Font]::new('Segoe UI',28,[Drawing.FontStyle]::Bold)
$paths=@((Join-Path $PSScriptRoot 'BD01-Size-Before.png'),(Join-Path $PSScriptRoot '../../WISP_Parts/W-01_LUMEN/W-01_LUMEN-Mounted-idle.png'))
$labels=@('BEFORE','AFTER - BODY 82%')
for($i=0;$i -lt 2;$i++){
 $y=$i*1180
 $g.DrawString($labels[$i],$font,[Drawing.Brushes]::White,35,($y+20))
 $img=[Drawing.Image]::FromFile($paths[$i])
 $g.DrawImage($img,0,($y+80),1100,1100)
 $img.Dispose()
}
$canvas.Save((Join-Path $PSScriptRoot 'BD01-Size-Comparison-Phone.png'),[Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$canvas.Dispose();$font.Dispose()
