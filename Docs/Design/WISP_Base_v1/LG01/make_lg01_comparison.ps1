$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$here = $PSScriptRoot
$bg = [Drawing.Color]::FromArgb(15,19,26)
$panel = [Drawing.Color]::FromArgb(26,33,43)
$white = [Drawing.Color]::FromArgb(233,238,245)
$muted = [Drawing.Color]::FromArgb(169,184,202)
$titleFont = [Drawing.Font]::new('Segoe UI',25,[Drawing.FontStyle]::Bold)
$labelFont = [Drawing.Font]::new('Segoe UI',17,[Drawing.FontStyle]::Bold)
$smallFont = [Drawing.Font]::new('Segoe UI',13,[Drawing.FontStyle]::Regular)
$img = [Drawing.Bitmap]::new(1500,850); $g = [Drawing.Graphics]::FromImage($img)
$g.SmoothingMode='AntiAlias'; $g.InterpolationMode='HighQualityBicubic'; $g.Clear($bg)
function Label($text,$x,$y,$font,$color) {
    $b=[Drawing.SolidBrush]::new($color); $g.DrawString($text,$font,$b,$x,$y); $b.Dispose()
}
function Contain($bitmap,$rect) {
    $scale=[Math]::Min($rect.Width/$bitmap.Width,$rect.Height/$bitmap.Height)
    $w=[int]($bitmap.Width*$scale); $h=[int]($bitmap.Height*$scale)
    $g.DrawImage($bitmap,$rect.X+[int](($rect.Width-$w)/2),$rect.Y+[int](($rect.Height-$h)/2),$w,$h)
}
Label 'WISP LG-01  |  REFERENCE / GENERATED COMPARISON' 34 20 $titleFont $white
$g.FillRectangle([Drawing.SolidBrush]::new($panel),20,74,720,740)
$g.FillRectangle([Drawing.SolidBrush]::new($panel),760,74,720,740)
Label 'SUPPLIED LEG GUIDE' 42 88 $labelFont $white
Label 'GENERATED 45° LEG PAIR' 782 88 $labelFont $white
$ref=[Drawing.Bitmap]::new((Join-Path $here 'LG01-Design-Guide.jpg'))
$gen=[Drawing.Bitmap]::new((Join-Path $here 'LG01-05-45deg.png'))
Contain $ref ([Drawing.Rectangle]::new(36,132,688,662))
Contain $gen ([Drawing.Rectangle]::new(776,132,688,662))
$ref.Dispose(); $gen.Dispose()
$img.Save((Join-Path $here 'LG01-Concept-Comparison.png'),[Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $img.Dispose()
Write-Output 'LG01_COMPARISON_PASS'
