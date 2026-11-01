param(
    [switch]$Remove,
    [string]$ProjectRootOverride
)

$ErrorActionPreference='Stop'
$taskName='VoxelOn Codex Task Poller'
if($Remove){Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue;Write-Output "Removed: $taskName";exit 0}
$projectRoot=if($ProjectRootOverride){(Resolve-Path $ProjectRootOverride).Path}else{(Resolve-Path (Join-Path $PSScriptRoot '..')).Path}
$runnerHome=Join-Path $projectRoot 'UserSettings\CodexTaskRunner'
New-Item -ItemType Directory -Path $runnerHome -Force|Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CodexTaskRunner.ps1') -Destination (Join-Path $runnerHome 'CodexTaskRunner.ps1') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CodexTaskRunner.settings.json') -Destination (Join-Path $runnerHome 'CodexTaskRunner.settings.json') -Force
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $runnerHome 'Install-CodexTaskScheduler.ps1') -Force
$runner=Join-Path $runnerHome 'CodexTaskRunner.ps1'
$powerShell=(Get-Command pwsh.exe -ErrorAction Stop).Source
$userId='{0}\{1}' -f $env:USERDOMAIN,$env:USERNAME
$action=New-ScheduledTaskAction -Execute $powerShell `
    -Argument "-NoLogo -NoProfile -ExecutionPolicy Bypass -File `"$runner`" -ProjectRootOverride `"$projectRoot`" -Once" `
    -WorkingDirectory $projectRoot
$trigger=New-ScheduledTaskTrigger -Daily -At ((Get-Date).AddMinutes(2))
$trigger.Repetition.Interval='PT10M'
$trigger.Repetition.Duration='P1D'
$settings=New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew `
    -ExecutionTimeLimit (New-TimeSpan -Hours 2) -StartWhenAvailable
$principal=New-ScheduledTaskPrincipal -UserId $userId -LogonType Interactive -RunLevel Limited
Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger `
    -Settings $settings -Principal $principal -Description 'Poll VoxelOn GitHub tasks every 10 minutes; run only in the signed-in user session.' -Force|Out-Null
$task=Get-ScheduledTask -TaskName $taskName
$info=Get-ScheduledTaskInfo -TaskName $taskName
Write-Output "Installed: $taskName"
Write-Output "User: $userId (interactive sign-in required)"
Write-Output "Interval: 10 minutes; overlap policy: $($task.Settings.MultipleInstances)"
Write-Output "Next run: $($info.NextRunTime)"
