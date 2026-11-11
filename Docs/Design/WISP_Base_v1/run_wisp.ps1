param(
    [string]$BlenderPath = 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe'
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$generator = Join-Path $here 'wisp_automate.py'
$runs = Join-Path $here 'runs'
if (!(Test-Path -LiteralPath $BlenderPath)) { throw "Blender not found: $BlenderPath" }
if (Get-Process -ErrorAction SilentlyContinue | Where-Object ProcessName -eq 'blender') {
    throw 'Blender is already running; close it before generating review models.'
}
New-Item -ItemType Directory -Path $runs -Force | Out-Null

# Use one body configuration across four pose studies so only the articulated pose changes.
$base = [ordered]@{ id='INDUSTRIAL_WISP_POSE'; armor_color=@(0.36,0.37,0.39); frame_color=@(0.025,0.032,0.043); accent_color=@(0.82,0.30,0.07); pose='IDLE'; hip_z=0.615; joint_back_shift_m=0.02; leg_scale=0.890; pelvis_center_z=0.6550; torso_form='WAIST_SCULPT'; torso_height=0.16; torso_center_z=0.815; pelvis_form='HIP_BRIDGE'; pelvis_armor_level=-0.10; torso_armor_level=0.10; chest_armor_height_ratio=0.75; shoulder_gap_m=0.015; shoulder_z_offset=-0.02; foot_offset_y=-0.025; head_form='ANGULAR_SENSOR'; face_form='BEAKED_VISOR'; face_width=0.86; face_height=0.44; antenna_side=0; forearm_form='EGG_GUN'; shoulder_profile='NARROW_WEDGE'; shoulder_size=@(0.13,0.12,0.12); shoulder_bevel=0.006; shoulder_tilt_deg=22; head_size=@(0.132,0.113,0.108); head_center_z=0.9745; head_antenna_height=0.038; neck_radius=0.015; forearm_size=@(0.20,0.19,0.32); muzzle_radius=0.065; muzzle_enabled=$true; upper_arm_size=@(0.066,0.066,0.140); foot_size=@(0.16,0.22,0.12); upper_arm_center_drop=0.10; elbow_drop=0.18; forearm_drop=0.155; core_size=@(0.34,0.23,0.34); chest_size=@(0.25,0.15,0.20); pelvis_size=@(0.14,0.075,0.160); chest_profile='TAPERED'; limb_profile='CANNON_TAPER'; arm_splay_deg=23; elbow_bend_deg=-30; leg_splay_deg=7 }
$variants = @()
foreach ($pose in @('IDLE','AIM','GUARD','CROUCH')) {
    $variant = [ordered]@{}
    foreach ($key in $base.Keys) { $variant[$key] = $base[$key] }
    $variant['id'] = "INDUSTRIAL_WISP_$pose"
    $variant['pose'] = $pose
    $variants += $variant
}
$runDirs = @()
foreach ($variant in $variants) {
    $variantJson = $variant | ConvertTo-Json -Compress
    $env:WISP_VARIANT_JSON = $variantJson
    $consoleOut = Join-Path $here ("blender-console-{0}.log" -f $variant.id)
    $consoleErr = Join-Path $here ("blender-console-{0}.err" -f $variant.id)
    $runStartedAt = Get-Date
    $proc = Start-Process -FilePath $BlenderPath `
        -ArgumentList @('--background','--python-exit-code','1','--python',$generator) `
        -WorkingDirectory $here -WindowStyle Hidden -Wait -PassThru `
        -RedirectStandardOutput $consoleOut -RedirectStandardError $consoleErr
    $run = Get-ChildItem -LiteralPath $runs -Directory |
        Where-Object { $_.Name -Like "*-$($variant.id)" -and $_.CreationTime -ge $runStartedAt.AddSeconds(-1) } |
        Sort-Object Name -Descending | Select-Object -First 1
    if ($null -eq $run) { throw "No output directory was created for $($variant.id)." }
    Move-Item -LiteralPath $consoleOut -Destination (Join-Path $run.FullName 'blender-console.log') -Force
    Move-Item -LiteralPath $consoleErr -Destination (Join-Path $run.FullName 'blender-console.err') -Force
    if ($proc.ExitCode -ne 0 -or !(Test-Path -LiteralPath (Join-Path $run.FullName 'manifest.json'))) {
        throw "Blender failed for $($variant.id); see $($run.FullName)."
    }
    $manifest = Get-Content (Join-Path $run.FullName 'manifest.json') -Raw | ConvertFrom-Json
    if ($manifest.automation -ne 'PASS' -or $manifest.design_review -ne 'PENDING_PD_VISUAL_REVIEW') {
        throw "Unexpected result state for $($variant.id)."
    }
    foreach ($image in $manifest.images) {
        if (!(Test-Path -LiteralPath $image) -or (Get-Item -LiteralPath $image).Length -lt 10000) {
            throw "Missing or invalid render for $($variant.id): $image"
        }
    }
    $runDirs += $run.FullName
    Write-Output "BLENDER_PASS $($variant.id) $($run.FullName)"
}

# One review image mirrors the reference sheet: rows are candidates and
# columns are front, right, back and three-quarter views.
Add-Type -AssemblyName System.Drawing
$overview = Join-Path $runs ("WISP-1-Short-Waist-Shoulder-Links-{0}.png" -f (Get-Date -Format 'yyyyMMdd-HHmmss'))
$cell = 330; $left = 232; $top = 72; $gap = 12; $rowHeight = 360
$bitmap = New-Object System.Drawing.Bitmap(($left + 4 * ($cell + $gap)), ($top + $runDirs.Count * $rowHeight + $gap))
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.Clear([System.Drawing.Color]::FromArgb(18,22,30))
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias
$titleFont = New-Object System.Drawing.Font('Segoe UI', 22, [System.Drawing.FontStyle]::Bold)
$labelFont = New-Object System.Drawing.Font('Segoe UI', 12, [System.Drawing.FontStyle]::Bold)
$rowFont = New-Object System.Drawing.Font('Segoe UI', 15, [System.Drawing.FontStyle]::Bold)
$white = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$cyan = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(63,207,225))
$graphics.DrawString('WISP • INDUSTRIAL ARMOR REFERENCE', $titleFont, $white, 22, 16)
$headers = @('FRONT','RIGHT','BACK','45 DEG')
$fileNames = @('01-front.png','02-right.png','03-back.png','04-three-quarter-45.png')
for ($column = 0; $column -lt 4; $column++) {
    $x = $left + $gap + $column * ($cell + $gap)
    $graphics.DrawString($headers[$column], $labelFont, $cyan, $x + 5, 48)
}
for ($row = 0; $row -lt $runDirs.Count; $row++) {
    $manifest = Get-Content (Join-Path $runDirs[$row] 'manifest.json') -Raw | ConvertFrom-Json
    $graphics.DrawString($manifest.variant.id, $rowFont, $white, 18, ($top + $row * $rowHeight + 142))
    for ($column = 0; $column -lt 4; $column++) {
        $imagePath = Join-Path $runDirs[$row] $fileNames[$column]
        $image = [System.Drawing.Image]::FromFile($imagePath)
        $x = $left + $gap + $column * ($cell + $gap)
        $y = $top + $row * $rowHeight
        $graphics.DrawImage($image, $x, $y, $cell, $cell)
        $image.Dispose()
    }
}
$bitmap.Save($overview, [System.Drawing.Imaging.ImageFormat]::Png)
$white.Dispose(); $cyan.Dispose(); $titleFont.Dispose(); $labelFont.Dispose(); $rowFont.Dispose()
$graphics.Dispose(); $bitmap.Dispose()
if (!(Test-Path -LiteralPath $overview) -or (Get-Item -LiteralPath $overview).Length -lt 100000) {
    throw 'WISP overview contact sheet was not created correctly.'
}
# A portrait pose strip lets PD compare each pose quickly on a phone.
$phone = Join-Path $runDirs[-1] 'WISP-Pose-Review-Phone.png'
$phoneBitmap = New-Object System.Drawing.Bitmap(900, (110 + $runDirs.Count * 900))
$phoneGraphics = [System.Drawing.Graphics]::FromImage($phoneBitmap)
$phoneGraphics.Clear([System.Drawing.Color]::FromArgb(18,22,30))
$phoneWhite = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$phoneCyan = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(63,207,225))
$phoneGraphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias
$phoneTitle = New-Object System.Drawing.Font('Segoe UI', 25, [System.Drawing.FontStyle]::Bold)
$phoneLabel = New-Object System.Drawing.Font('Segoe UI', 18, [System.Drawing.FontStyle]::Bold)
$phoneGraphics.DrawString('WISP • INDUSTRIAL ARMOR REFERENCE', $phoneTitle, $phoneWhite, 25, 20)
for ($row = 0; $row -lt $runDirs.Count; $row++) {
    $manifest = Get-Content (Join-Path $runDirs[$row] 'manifest.json') -Raw | ConvertFrom-Json
    $y = 70 + $row * 900
    $phoneGraphics.DrawString($manifest.pose, $phoneLabel, $phoneCyan, 25, $y)
    $image = [System.Drawing.Image]::FromFile((Join-Path $runDirs[$row] '01-front.png'))
    $phoneGraphics.DrawImage($image, 50, $y + 30, 800, 800)
    $image.Dispose()
}
$phoneBitmap.Save($phone, [System.Drawing.Imaging.ImageFormat]::Png)
$phoneWhite.Dispose(); $phoneCyan.Dispose(); $phoneTitle.Dispose(); $phoneLabel.Dispose(); $phoneGraphics.Dispose(); $phoneBitmap.Dispose()
if (!(Test-Path -LiteralPath $phone) -or (Get-Item -LiteralPath $phone).Length -lt 100000) {
    throw 'WISP pose phone sheet was not created correctly.'
}
Write-Output "AUTOMATION_PASS mirrored shoulder armor caps and orange panels generated; shoulder and hip articulation cans remain cylindrical; design review remains with PD."
Write-Output "OVERVIEW $overview"
Write-Output "PHONE $phone"
