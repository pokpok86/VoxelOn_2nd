$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$here = $PSScriptRoot
$fontTitle = [Drawing.Font]::new('Segoe UI', 26, [Drawing.FontStyle]::Bold)
$fontLabel = [Drawing.Font]::new('Segoe UI', 17, [Drawing.FontStyle]::Bold)
$fontSmall = [Drawing.Font]::new('Segoe UI', 13, [Drawing.FontStyle]::Regular)
$bg = [Drawing.Color]::FromArgb(15,19,26); $panel = [Drawing.Color]::FromArgb(26,33,43)
$white = [Drawing.Color]::FromArgb(233,238,245); $muted = [Drawing.Color]::FromArgb(169,184,202)
function Open-Bitmap([string]$name) { [Drawing.Bitmap]::new((Join-Path $here $name)) }
function Draw-Contain($g,$img,[Drawing.Rectangle]$rect) {
    $scale=[Math]::Min($rect.Width/$img.Width,$rect.Height/$img.Height)
    $w=[int]($img.Width*$scale); $h=[int]($img.Height*$scale)
    $g.DrawImage($img,$rect.X+[int](($rect.Width-$w)/2),$rect.Y+[int](($rect.Height-$h)/2),$w,$h)
}
function Label($g,[string]$text,[int]$x,[int]$y,$font=$fontLabel,$color=$white) {
    $brush=[Drawing.SolidBrush]::new($color); $g.DrawString($text,$font,$brush,$x,$y); $brush.Dispose()
}
function Panel($g,[int]$y,[string]$title) {
    $g.FillRectangle([Drawing.SolidBrush]::new($panel),20,$y,1040,470); Label $g $title 38 ($y+12)
}
function View($g,[string]$file,[int]$x,[int]$y,[int]$w,[string]$title) {
    $img=Open-Bitmap $file; Label $g $title $x ($y+3) $fontSmall $muted
    Draw-Contain $g $img ([Drawing.Rectangle]::new($x,$y+30,$w,380)); $img.Dispose()
}
$sheet=[Drawing.Bitmap]::new(1080,2160); $g=[Drawing.Graphics]::FromImage($sheet)
$g.SmoothingMode='AntiAlias'; $g.InterpolationMode='HighQualityBicubic'; $g.Clear($bg)
Label $g 'WISP  HD-01  |  SENSOR HEAD MODULE' 34 24 $fontTitle
Label $g 'One continuous optical slit  |  compact faceted shell  |  fixed neck interface' 36 68 $fontSmall $muted
Panel $g 112 'DESIGN GUIDE + GENERATED FRONT'
$ref=Open-Bitmap 'HD01-Design-Guide.jpg'; $front=Open-Bitmap 'HD01-01-Front.png'
Label $g 'Supplied guide' 36 152 $fontSmall $muted; Draw-Contain $g $ref ([Drawing.Rectangle]::new(36,182,500,380))
Label $g 'Generated front' 548 152 $fontSmall $muted; Draw-Contain $g $front ([Drawing.Rectangle]::new(548,182,500,380))
$ref.Dispose(); $front.Dispose()
Panel $g 600 'ORTHOGRAPHIC VIEWS'
View $g 'HD01-01-Front.png' 36 648 500 'Front'; View $g 'HD01-02-Right.png' 548 648 500 'Right'
Panel $g 1082 'BACK + LEFT'
View $g 'HD01-03-Back.png' 36 1130 500 'Back'; View $g 'HD01-04-Left.png' 548 1130 500 'Left'
Panel $g 1564 'THREE-QUARTER + TOP'
View $g 'HD01-05-45deg.png' 36 1612 500 '45 degree'; View $g 'HD01-06-Top.png' 548 1612 500 'Top'
Label $g 'PENDING PD DESIGN REVIEW  |  Blender 5.2.3  |  No Unity import' 34 2068 $fontLabel $muted
$sheet.Save((Join-Path $here 'HD01-Review-Phone.png'),[Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $sheet.Dispose()

$cmp=[Drawing.Bitmap]::new(1500,850); $gc=[Drawing.Graphics]::FromImage($cmp)
$gc.SmoothingMode='AntiAlias'; $gc.InterpolationMode='HighQualityBicubic'; $gc.Clear($bg)
Label $gc 'HD-01  |  REFERENCE / GENERATED COMPARISON' 34 20 $fontTitle
$gc.FillRectangle([Drawing.SolidBrush]::new($panel),20,74,720,740); $gc.FillRectangle([Drawing.SolidBrush]::new($panel),760,74,720,740)
Label $gc 'SUPPLIED HEAD GUIDE' 42 88; Label $gc 'GENERATED THREE-QUARTER' 782 88
$ref=Open-Bitmap 'HD01-Design-Guide.jpg'; $gen=Open-Bitmap 'HD01-05-45deg.png'
Draw-Contain $gc $ref ([Drawing.Rectangle]::new(36,132,688,662)); Draw-Contain $gc $gen ([Drawing.Rectangle]::new(776,132,688,662))
$cmp.Save((Join-Path $here 'HD01-Concept-Comparison.png'),[Drawing.Imaging.ImageFormat]::Png)
$ref.Dispose(); $gen.Dispose(); $gc.Dispose(); $cmp.Dispose()
Write-Output 'HD01_REVIEW_IMAGES_PASS'
