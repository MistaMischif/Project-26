param([int]$IntervalSeconds=15,[switch]$Once)
$ErrorActionPreference='Stop'
$mutex=New-Object Threading.Mutex($false,'Local\COLTRAN_LivingTable_AI')
try{$acquired=$mutex.WaitOne(0)}catch [Threading.AbandonedMutexException]{$acquired=$true}
if(!$acquired){exit 0}
Add-Type -TypeDefinition 'using System;using System.Runtime.InteropServices;public static class LivingTablePower { [DllImport("kernel32.dll")] public static extern uint SetThreadExecutionState(uint flags); }'
$log=Join-Path $PSScriptRoot 'availability.log'
function Write-Availability([string]$State){
 if((Test-Path $log) -and (Get-Item $log).Length -gt 1048576){Move-Item -LiteralPath $log -Destination ($log+'.previous') -Force}
 Add-Content -LiteralPath $log -Value ((Get-Date -Format o)+' '+$State)
}
try {
 Write-Availability 'SUPERVISOR_STARTED'
 $previous='';$powerState=''
 do {
  try {
   $healthy=$false
   try {
    $config=Get-Content (Join-Path $PSScriptRoot 'first-steps-connection.json') -Raw | ConvertFrom-Json
    $health=Invoke-RestMethod 'http://127.0.0.1:8790/health' -TimeoutSec 3
    $ready=Invoke-RestMethod 'http://127.0.0.1:8790/ready' -Headers @{Authorization=('Bearer '+$config.deviceToken);'X-Living-Table-Monitor'='host'} -TimeoutSec 5
    $powerFlags=if($health.clientActive){[uint32]2147483649}else{[uint32]2147483648}
    $powerResult=[LivingTablePower]::SetThreadExecutionState($powerFlags)
    if($powerResult -eq 0){Write-Availability 'SLEEP_HOLD_FAILED'}elseif($powerState -ne [string]$health.clientActive){$powerState=[string]$health.clientActive;Write-Availability ('SLEEP_HOLD '+$powerState)}
    $healthy=$health.service -eq 'Living Table First Steps' -and $health.version -eq '0.4.3' -and $ready.ready
    $models=(Invoke-RestMethod 'http://127.0.0.1:11436/api/tags' -TimeoutSec 3).models.name
    $healthy=$healthy -and ($models -contains 'llama3.2:3b') -and ($models -contains 'qwen3.5:latest')
   } catch { $healthy=$false }
   if(!$healthy){
    Write-Availability 'RECOVERY_STARTED'
    & (Join-Path $PSScriptRoot 'Launch-FirstSteps-Test.ps1') -SkipWarmup | Out-Null
    $previous='';Write-Availability 'RECOVERY_COMPLETED'
   } elseif($previous -ne 'READY'){Write-Availability 'READY';$previous='READY'}
  } catch { Write-Availability ('RECOVERY_FAILED '+$_.Exception.GetType().Name);$previous='' }
  if(!$Once){Start-Sleep -Seconds $IntervalSeconds}
 } while(!$Once)
} finally { [LivingTablePower]::SetThreadExecutionState([uint32]2147483648) | Out-Null;$mutex.ReleaseMutex();$mutex.Dispose() }
