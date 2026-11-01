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
$settingsPath=Join-Path $runnerHome 'CodexTaskRunner.settings.json'
$localSettings=Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8|ConvertFrom-Json -AsHashtable
$localSettings.GitExecutable=(Get-Command git.exe -ErrorAction Stop).Source
$localSettings.CodexExecutable=(Get-Command codex.exe -ErrorAction Stop).Source
[IO.File]::WriteAllText($settingsPath,(ConvertTo-Json -InputObject $localSettings -Depth 8),[Text.UTF8Encoding]::new($false))
$runner=Join-Path $runnerHome 'CodexTaskRunner.ps1'
$powerShell=(Get-Command pwsh.exe -ErrorAction Stop).Source
$userId='{0}\{1}' -f $env:USERDOMAIN,$env:USERNAME
$action=New-ScheduledTaskAction -Execute $powerShell `
    -Argument "-NoLogo -NoProfile -ExecutionPolicy Bypass -File `"$runner`" -ProjectRootOverride `"$projectRoot`" -Once" `
    -WorkingDirectory $projectRoot
$startBoundary=(Get-Date).AddMinutes(2).ToString('yyyy-MM-ddTHH:mm:ss')
$commandXml=[Security.SecurityElement]::Escape($powerShell)
$argumentsXml=[Security.SecurityElement]::Escape("-NoLogo -NoProfile -ExecutionPolicy Bypass -File `"$runner`" -ProjectRootOverride `"$projectRoot`" -Once")
$directoryXml=[Security.SecurityElement]::Escape($projectRoot)
$userXml=[Security.SecurityElement]::Escape($userId)
$xml=@"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.4" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo><Description>Poll VoxelOn GitHub tasks every 10 minutes; run only in the signed-in user session.</Description></RegistrationInfo>
  <Triggers><CalendarTrigger><Repetition><Interval>PT10M</Interval><Duration>P1D</Duration><StopAtDurationEnd>false</StopAtDurationEnd></Repetition><StartBoundary>$startBoundary</StartBoundary><Enabled>true</Enabled><ScheduleByDay><DaysInterval>1</DaysInterval></ScheduleByDay></CalendarTrigger></Triggers>
  <Principals><Principal id="Author"><UserId>$userXml</UserId><LogonType>InteractiveToken</LogonType><RunLevel>LeastPrivilege</RunLevel></Principal></Principals>
  <Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy><DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries><StopIfGoingOnBatteries>false</StopIfGoingOnBatteries><StartWhenAvailable>true</StartWhenAvailable><Enabled>true</Enabled><ExecutionTimeLimit>PT2H</ExecutionTimeLimit><Priority>7</Priority></Settings>
  <Actions Context="Author"><Exec><Command>$commandXml</Command><Arguments>$argumentsXml</Arguments><WorkingDirectory>$directoryXml</WorkingDirectory></Exec></Actions>
</Task>
"@
Register-ScheduledTask -TaskName $taskName -Xml $xml -Force|Out-Null
$task=Get-ScheduledTask -TaskName $taskName
$info=Get-ScheduledTaskInfo -TaskName $taskName
Write-Output "Installed: $taskName"
Write-Output "User: $userId (interactive sign-in required)"
Write-Output "Interval: 10 minutes; overlap policy: $($task.Settings.MultipleInstances)"
Write-Output "Next run: $($info.NextRunTime)"
