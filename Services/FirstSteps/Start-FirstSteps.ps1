param([string]$OllamaUrl='http://127.0.0.1:11436',[string]$Model='qwen3.5:latest',[string]$Adb='', [string]$Apk='')
$ErrorActionPreference='Stop'
$configPath=Join-Path $PSScriptRoot 'first-steps-connection.json'
if(!(Test-Path -LiteralPath $configPath)) {
 $bytes=New-Object byte[] 32
 $rng=[Security.Cryptography.RandomNumberGenerator]::Create();try{$rng.GetBytes($bytes)}finally{$rng.Dispose()}
 @{url='http://127.0.0.1:8790';deviceToken=[Convert]::ToBase64String($bytes)} | ConvertTo-Json | Set-Content -LiteralPath $configPath
}
$config=Get-Content -Raw -LiteralPath $configPath | ConvertFrom-Json
$env:LIVING_TABLE_DEVICE_TOKEN=$config.deviceToken
$env:OLLAMA_URL=$OllamaUrl
$env:FIRST_STEPS_MODEL=$Model
$running=$false
try{$health=Invoke-RestMethod 'http://127.0.0.1:8790/health';$running=$health.service -eq 'Living Table First Steps'}catch{}
if($running -and ($health.version -ne '0.4.3' -or $health.model -ne $Model)){throw 'A different vision host is running. Stop that host before starting this revision.'}
if(!$running){Start-Process (Get-Command node).Source -ArgumentList ('"'+(Join-Path $PSScriptRoot 'server.mjs')+'"') -WindowStyle Hidden -RedirectStandardOutput (Join-Path $PSScriptRoot 'host.log') -RedirectStandardError (Join-Path $PSScriptRoot 'host-error.log') | Out-Null}
$deviceReady=$false
if($Adb){try{$state=& $Adb get-state 2>$null;$deviceReady=$LASTEXITCODE -eq 0 -and $state -eq 'device'}catch{$deviceReady=$false}}
if($Apk -and !$deviceReady){throw 'APK installation requires a connected, authorized USB headset.'}
if($deviceReady){
 if($Apk){& $Adb install -r $Apk;if($LASTEXITCODE -ne 0){throw 'APK install failed'}}
 if($config.url.StartsWith('http://127.0.0.1')){& $Adb reverse tcp:8790 tcp:8790;if($LASTEXITCODE -ne 0){throw 'USB forwarding failed'}}
 & $Adb shell mkdir -p /sdcard/Android/data/com.livingtable.firststeps/files
 & $Adb push $configPath /sdcard/Android/data/com.livingtable.firststeps/files/first-steps-connection.json
 if($LASTEXITCODE -ne 0){throw 'Connection installation failed'}
}
if(!$deviceReady){Write-Host 'USB is absent. Existing Wi-Fi-paired headsets can still connect; pair new devices by USB once.'}
Remove-Item Env:LIVING_TABLE_DEVICE_TOKEN
Write-Output 'Local First Steps host configured; connection token was not printed. Wireless pairing, when configured, remains usable without USB.'
