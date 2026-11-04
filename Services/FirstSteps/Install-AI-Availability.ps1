# Explicitly authorized persistent startup; no passwords or tokens stored in the task.
$ErrorActionPreference='Stop'
$name='COLTRAN Living Table AI'
$script=Join-Path $PSScriptRoot 'Watch-FirstSteps.ps1'
$existing=Get-ScheduledTask -TaskName $name -ErrorAction SilentlyContinue
if($existing -and !($existing.Actions.Arguments -like ('*'+$script+'*'))){throw 'Task name belongs to another action; left unchanged.'}
if($existing -and $existing.State -eq 'Running'){Stop-ScheduledTask -TaskName $name;for($i=0;$i -lt 20;$i++){if((Get-ScheduledTask -TaskName $name).State -ne 'Running'){break};Start-Sleep -Milliseconds 250};Start-Sleep -Seconds 2}
$account=[Security.Principal.WindowsIdentity]::GetCurrent().Name
$exe=Join-Path $env:SystemRoot 'System32/WindowsPowerShell/v1.0/powershell.exe'
$action=New-ScheduledTaskAction -Execute $exe -Argument ('-NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File "'+$script+'"') -WorkingDirectory $PSScriptRoot
$trigger=New-ScheduledTaskTrigger -AtLogOn -User $account
$settings=New-ScheduledTaskSettingsSet -StartWhenAvailable -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -MultipleInstances IgnoreNew -ExecutionTimeLimit ([TimeSpan]::Zero) -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 1)
$principal=New-ScheduledTaskPrincipal -UserId $account -LogonType Interactive -RunLevel Limited
Register-ScheduledTask -TaskName $name -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Force | Out-Null
Start-ScheduledTask -TaskName $name
Write-Output 'Living Table AI automatic startup and recovery installed.'
