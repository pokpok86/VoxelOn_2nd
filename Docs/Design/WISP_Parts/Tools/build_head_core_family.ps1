$ErrorActionPreference='Stop'
$blender='C:\Program Files\Blender Foundation\Blender 5.2\blender.exe'
$builder=Join-Path $PSScriptRoot 'build_head_core_family.py'
$verify=Join-Path $PSScriptRoot 'verify_head_core_family.py'
$out=Join-Path (Split-Path $PSScriptRoot) 'HeadCore-20261112'
$parts=@('H-01_RADOME','H-02_LONGSIGHT','H-03_SWIFTFIN','H-04_BULWARK','C-01_STANDARD','C-02_LIGHTFRAME','C-03_BULWARK_CORE','C-04_ARC_CORE')
foreach($part in $parts){
    & $blender --background --python-exit-code 1 --python $builder -- --part $part *> (Join-Path $out ($part+'-Blender.log'))
    if($LASTEXITCODE -ne 0){throw "Blender failed: $part"}
}
& $blender --background --python-exit-code 1 --python $verify *> (Join-Path $out 'Verification.log')
if($LASTEXITCODE -ne 0){throw 'Interface verification failed'}
& python (Join-Path $PSScriptRoot 'make_head_core_review.py')
if($LASTEXITCODE -ne 0){throw 'Review image composition failed; Python Pillow is required'}
