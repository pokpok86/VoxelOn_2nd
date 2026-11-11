$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$here = $PSScriptRoot
$fontTitle = [Drawing.Font]::new('Segoe UI', 26, [Drawing.FontStyle]::Bold)
$fontLabel = [Drawing.Font]::new('Segoe UI', 17, [Drawing.FontStyle]::Bold)
$fontSmall = [Drawing.Font]::new('Segoe UI', 13, [Drawing.FontStyle]::Regular)
$bg = [Drawing.Color]::FromArgb(15, 19, 26)
$panel = [Drawing.Color]::FromArgb(26, 33, 43)
$white = [Drawing.Color]::FromArgb(233, 238, 245)
$muted = [Drawing.Color]::FromArgb(169, 184, 202)
$orange = [Drawing.Color]::FromArgb(255, 123, 96)
$cyan = [Drawing.Color]::FromArgb(97, 213, 244)
$green = [Drawing.Color]::FromArgb(124, 232, 151)
$yellow = [Drawing.Color]::FromArgb(255, 220, 90)

function Open-Bitmap([string]$name) { [Drawing.Bitmap]::new((Join-Path $here $name)) }
function Draw-Contain($g, $img, [Drawing.Rectangle]$rect) {
    $scale = [Math]::Min($rect.Width / $img.Width, $rect.Height / $img.Height)
    $w = [int]($img.Width * $scale); $h = [int]($img.Height * $scale)
    $x = $rect.X + [int](($rect.Width - $w) / 2); $y = $rect.Y + [int](($rect.Height - $h) / 2)
    $g.DrawImage($img, $x, $y, $w, $h)
}
function Draw-Label($g, [string]$text, [int]$x, [int]$y, $font=$fontLabel, $color=$white) {
    $brush = [Drawing.SolidBrush]::new($color); $g.DrawString($text, $font, $brush, $x, $y); $brush.Dispose()
}

# Add clear colored pivot markers and a legend without changing the Blender source render.
$raw = Open-Bitmap 'BD01-07-Connection-Axes-raw.png'
$annot = [Drawing.Bitmap]::new($raw.Width, $raw.Height + 142)
$ga = [Drawing.Graphics]::FromImage($annot); $ga.SmoothingMode = 'AntiAlias'; $ga.Clear($bg)
Draw-Label $ga 'BD-01  |  SHOULDER + WAIST PIVOTS' 30 20 $fontTitle
$ga.DrawImage($raw, 0, 62)
$m = Get-Content (Join-Path $here 'manifest.json') -Raw | ConvertFrom-Json
$colors = @{ shoulder_L = $orange; shoulder_R = $orange; waist = $green }
foreach ($p in $m.connection_pixels_closeup) {
    $c = $colors[$p.joint]
    $pen = [Drawing.Pen]::new($c, 5)
    $ga.DrawEllipse($pen, [int]$p.x - 24, [int]$p.y + 62 - 24, 48, 48)
    $ga.FillEllipse([Drawing.SolidBrush]::new($yellow), [int]$p.x - 6, [int]$p.y + 62 - 6, 12, 12)
    $pen.Dispose()
}
$ga.FillRectangle([Drawing.SolidBrush]::new($panel), 0, $raw.Height + 62, $raw.Width, 80)
Draw-Label $ga 'ORANGE  X: shoulder pitch' 30 ($raw.Height + 78) $fontSmall $orange
Draw-Label $ga 'CYAN  Y: slight shoulder spread' 380 ($raw.Height + 78) $fontSmall $cyan
Draw-Label $ga 'GREEN  Z: waist yaw' 800 ($raw.Height + 78) $fontSmall $green
$annot.Save((Join-Path $here 'BD01-07-Connection-Axes.png'), [Drawing.Imaging.ImageFormat]::Png)
$ga.Dispose(); $raw.Dispose(); $annot.Dispose()
Remove-Item -LiteralPath (Join-Path $here 'BD01-07-Connection-Axes-raw.png') -Force

# Tall review board sized for a phone: reference, orthographic views, then joints.
$W = 1080; $H = 3630
$sheet = [Drawing.Bitmap]::new($W, $H)
$g = [Drawing.Graphics]::FromImage($sheet); $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.Clear($bg)
Draw-Label $g 'WISP  BD-01  |  COMPACT BODY MODULE' 34 24 $fontTitle
Draw-Label $g 'Body, shoulder pivots, short arm sockets, fixed neck + yaw waist only' 36 68 $fontSmall $muted

function Panel($g, [int]$y, [int]$height, [string]$title) {
    $g.FillRectangle([Drawing.SolidBrush]::new($panel), 20, $y, 1040, $height)
    Draw-Label $g $title 38 ($y + 12) $fontLabel
}
function Draw-PanelImage($g, [string]$file, [int]$x, [int]$y, [int]$w, [int]$h, [string]$label) {
    $img = Open-Bitmap $file
    Draw-Label $g $label $x ($y + 4) $fontSmall $muted
    Draw-Contain $g $img ([Drawing.Rectangle]::new($x, $y + 30, $w, $h - 34))
    $img.Dispose()
}

Panel $g 112 770 'REFERENCE + FRONT'
Draw-PanelImage $g 'BD01-Design-Guide.jpg' 34 154 500 700 'BD-01 supplied guide'
Draw-PanelImage $g 'BD01-01-Front.png' 546 154 500 700 'Generated front'

Panel $g 900 1050 'ORTHOGRAPHIC VIEWS'
Draw-PanelImage $g 'BD01-01-Front.png' 34 946 500 480 'Front'
Draw-PanelImage $g 'BD01-02-Right.png' 546 946 500 480 'Right'
Draw-PanelImage $g 'BD01-03-Back.png' 34 1436 500 480 'Back'
Draw-PanelImage $g 'BD01-04-Left.png' 546 1436 500 480 'Left'

Panel $g 1970 650 '45-DEGREE SILHOUETTE'
Draw-PanelImage $g 'BD01-05-45deg.png' 235 2015 610 590 '45 degree'

Panel $g 2635 820 'CONNECTION DETAIL'
Draw-PanelImage $g 'BD01-06-Connections-Closeup.png' 34 2680 500 720 'Clean close-up'
Draw-PanelImage $g 'BD01-07-Connection-Axes.png' 546 2680 500 720 'Pivot axes + centers'

Draw-Label $g 'PENDING PD DESIGN REVIEW  |  Blender 5.2.3  |  No Unity import' 34 3480 $fontLabel $muted
$sheet.Save((Join-Path $here 'BD01-Review-Phone.png'), [Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $sheet.Dispose()

# A quick side-by-side comparison of the supplied guide's whole torso panel and the generated 45° view.
$cmp = [Drawing.Bitmap]::new(1500, 850)
$gc = [Drawing.Graphics]::FromImage($cmp); $gc.SmoothingMode = 'AntiAlias'; $gc.InterpolationMode = 'HighQualityBicubic'; $gc.Clear($bg)
Draw-Label $gc 'BD-01  |  REFERENCE / GENERATED COMPARISON' 34 20 $fontTitle
$gc.FillRectangle([Drawing.SolidBrush]::new($panel), 20, 74, 720, 740)
$gc.FillRectangle([Drawing.SolidBrush]::new($panel), 760, 74, 720, 740)
Draw-Label $gc 'SUPPLIED BODY GUIDE' 42 88 $fontLabel
Draw-Label $gc 'GENERATED 45° MODULE' 782 88 $fontLabel
$guide = Open-Bitmap 'BD01-Design-Guide.jpg'; $gen = Open-Bitmap 'BD01-05-45deg.png'
Draw-Contain $gc $guide ([Drawing.Rectangle]::new(36, 132, 688, 662))
Draw-Contain $gc $gen ([Drawing.Rectangle]::new(776, 132, 688, 662))
$cmp.Save((Join-Path $here 'BD01-Concept-Comparison.png'), [Drawing.Imaging.ImageFormat]::Png)
$guide.Dispose(); $gen.Dispose(); $gc.Dispose(); $cmp.Dispose()

Write-Output 'BD01_REVIEW_IMAGES_PASS'
