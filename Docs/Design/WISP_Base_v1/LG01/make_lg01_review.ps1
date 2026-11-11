$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = $PSScriptRoot
$manifest = Get-Content (Join-Path $root 'manifest.json') -Raw | ConvertFrom-Json
$rawPath = Join-Path $root 'LG01-06-Joint-Centers-Axes-raw.png'
$raw = [System.Drawing.Image]::FromFile($rawPath)
$axisCanvas = New-Object System.Drawing.Bitmap $raw.Width, ($raw.Height + 150)
$g = [System.Drawing.Graphics]::FromImage($axisCanvas)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$bg = [System.Drawing.Color]::FromArgb(14,18,25)
$g.Clear($bg)
$white = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$muted = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(205,214,225))
$title = New-Object System.Drawing.Font('Segoe UI',24,[System.Drawing.FontStyle]::Bold)
$legend = New-Object System.Drawing.Font('Segoe UI',15,[System.Drawing.FontStyle]::Regular)
$g.DrawString('LG-01  |  JOINT CENTERS + ROTATION AXES',$title,$white,28,15)
$orangePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255,135,112),6)
$cyanPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(115,220,255),6)
$yellowPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255,224,85),6)
$g.DrawLine($orangePen,32,75,72,75)
$g.DrawString('X: 고관절·무릎·발목 앞뒤 회전',$legend,$muted,84,62)
$g.DrawLine($cyanPen,435,75,475,75)
$g.DrawString('Y: 고관절 좌우 벌림',$legend,$muted,487,62)
$g.DrawEllipse($yellowPen,31,108,13,13)
$g.DrawString('노란 표식: 실제 피벗 중심',$legend,$muted,58,101)
$g.DrawImage($raw,0,150,$raw.Width,$raw.Height)
$dotFill = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255,224,85))
$dotOutline = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(28,31,35),3)
foreach($point in $manifest.joint_center_pixels_in_axis_render) {
    $cx=[int]$point.x; $cy=[int]$point.y+150
    $g.FillEllipse($dotFill,$cx-7,$cy-7,14,14)
    $g.DrawEllipse($dotOutline,$cx-8,$cy-8,16,16)
}
$axisOut=Join-Path $root 'LG01-06-Joint-Centers-Axes.png'
$axisCanvas.Save($axisOut,[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $axisCanvas.Dispose(); $raw.Dispose()

# A phone-friendly review sheet with the requested five views and the axis guide.
$files=@('LG01-01-Front.png','LG01-02-Right.png','LG01-03-Back.png',
         'LG01-04-Left.png','LG01-05-45deg.png','LG01-06-Joint-Centers-Axes.png')
$labels=@('FRONT','RIGHT SIDE','BACK','LEFT SIDE','45 DEG','JOINT AXES')
$sheet=New-Object System.Drawing.Bitmap 900,1680
$sg=[System.Drawing.Graphics]::FromImage($sheet)
$sg.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$sg.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$sg.Clear($bg)
$sheetTitle=New-Object System.Drawing.Font('Segoe UI',28,[System.Drawing.FontStyle]::Bold)
$sheetLabel=New-Object System.Drawing.Font('Segoe UI',16,[System.Drawing.FontStyle]::Bold)
$sg.DrawString('WISP LG-01  |  REVIEW',$sheetTitle,$white,30,22)
$cellW=420; $cellH=470; $startX=20; $startY=100; $gap=20
for($i=0;$i -lt $files.Count;$i++) {
    $im=[System.Drawing.Image]::FromFile((Join-Path $root $files[$i]))
    $col=$i%2; $row=[Math]::Floor($i/2)
    $x=$startX+$col*($cellW+$gap); $y=$startY+$row*($cellH+$gap)
    $sg.DrawString($labels[$i],$sheetLabel,$muted,$x+8,$y+4)
    $frame=New-Object System.Drawing.Rectangle ($x+4),($y+34),($cellW-8),($cellH-40)
    $ratio=[Math]::Min($frame.Width/$im.Width,$frame.Height/$im.Height)
    $dw=[int]($im.Width*$ratio); $dh=[int]($im.Height*$ratio)
    $dx=$frame.X+[int](($frame.Width-$dw)/2); $dy=$frame.Y+[int](($frame.Height-$dh)/2)
    $sg.DrawImage($im,$dx,$dy,$dw,$dh)
    $im.Dispose()
}
$sheetPath=Join-Path $root 'LG01-Review-Phone.png'
$sheet.Save($sheetPath,[System.Drawing.Imaging.ImageFormat]::Png)
$sg.Dispose(); $sheet.Dispose(); $white.Dispose(); $muted.Dispose()
$title.Dispose(); $legend.Dispose(); $orangePen.Dispose(); $cyanPen.Dispose(); $yellowPen.Dispose()
$dotFill.Dispose(); $dotOutline.Dispose(); $sheetTitle.Dispose(); $sheetLabel.Dispose()
Write-Output "LG01_REVIEW_IMAGES_PASS $axisOut $sheetPath"
