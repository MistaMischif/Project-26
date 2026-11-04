param([switch]$PrepareModels,[string]$ModelDirectory='',[string]$Adb='',[switch]$UsbOnly,[string]$WirelessAddress='',[switch]$SkipWarmup)
$ErrorActionPreference='Stop'
Write-Host 'Living Table local AI setup - no paid API or account required.'
$node=(Get-Command node -ErrorAction SilentlyContinue).Source
$ollama=(Get-Command ollama -ErrorAction SilentlyContinue).Source
if(!$ollama){$ollama=Join-Path $env:LOCALAPPDATA 'Programs/Ollama/ollama.exe'}
if(!$node -or !(Test-Path $ollama)){throw 'Install Node 22+ and Ollama first, then run this launcher again.'}
$runtimeSettings=Join-Path $PSScriptRoot 'runtime-config.json'
if(!$ModelDirectory -and (Test-Path $runtimeSettings)){$ModelDirectory=(Get-Content $runtimeSettings -Raw | ConvertFrom-Json).modelDirectory}
if($ModelDirectory){
 if(!(Test-Path -LiteralPath $ModelDirectory -PathType Container)){throw 'Saved model directory is unavailable. Connect the model drive or supply -ModelDirectory.'}
 $ModelDirectory=(Resolve-Path -LiteralPath $ModelDirectory).Path
 @{modelDirectory=$ModelDirectory} | ConvertTo-Json | Set-Content -LiteralPath $runtimeSettings
 $env:OLLAMA_MODELS=$ModelDirectory
}
$env:OLLAMA_HOST='127.0.0.1:11436'
$ready=$false;try{$null=Invoke-RestMethod http://127.0.0.1:11436/api/tags -TimeoutSec 2;$ready=$true}catch{}
if(!$ready){Write-Host 'Starting local model runtime...';Start-Process $ollama -ArgumentList 'serve' -WindowStyle Hidden -RedirectStandardOutput (Join-Path $PSScriptRoot 'runtime.log') -RedirectStandardError (Join-Path $PSScriptRoot 'runtime-error.log') | Out-Null;for($i=0;$i -lt 30;$i++){Start-Sleep -Seconds 1;try{$null=Invoke-RestMethod http://127.0.0.1:11436/api/tags -TimeoutSec 1;$ready=$true;break}catch{}}}
if(!$ready){throw 'Local model runtime did not become ready. See runtime-error.log.'}
$models=(Invoke-RestMethod http://127.0.0.1:11436/api/tags).models.name
foreach($model in @('llama3.2:3b','qwen3.5:latest')){if($models -notcontains $model){if(!$PrepareModels){throw "Missing $model. Run the Prepare Local AI launcher to download model weights."};Write-Host "Downloading $model (first setup only; several GB total)...";& $ollama pull $model;if($LASTEXITCODE -ne 0){throw 'Model download failed; rerun to resume.'}}}
if(!$SkipWarmup){Write-Host 'Warming the Meta companion...'
$body=@{model='llama3.2:3b';stream=$false;keep_alive='30m';messages=@(@{role='user';content='Reply ready.'});options=@{num_predict=8}} | ConvertTo-Json -Depth 5
$null=Invoke-RestMethod http://127.0.0.1:11436/api/chat -Method Post -ContentType application/json -Body $body -TimeoutSec 120}
if(!$UsbOnly){& (Join-Path $PSScriptRoot 'Configure-Wireless.ps1') -Address $WirelessAddress}
$health=$null;try{$health=Invoke-RestMethod http://127.0.0.1:8790/health -TimeoutSec 2}catch{}
if($health -and $health.service -eq 'Living Table First Steps' -and $health.version -ne '0.4.3'){
 $listener=Get-NetTCPConnection -LocalPort 8790 -State Listen | Select-Object -First 1;$process=Get-CimInstance Win32_Process -Filter "ProcessId=$($listener.OwningProcess)";$expected=Join-Path $PSScriptRoot 'server.mjs'
 if($process.Name -ne 'node.exe' -or !$process.CommandLine.Contains($expected)){throw 'Another host revision is running from a different folder. Close that host before using this package.'}
 Stop-Process -Id $process.ProcessId
}
if(!$Adb){$Adb=(Get-Command adb -ErrorAction SilentlyContinue).Source}
if(!$Adb){$candidate='C:/Program Files/Unity/Hub/Editor/6000.3.9f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb.exe';if(Test-Path $candidate){$Adb=$candidate}}
& (Join-Path $PSScriptRoot 'Start-FirstSteps.ps1') -Adb $Adb
Start-Sleep -Seconds 1
$health=Invoke-RestMethod http://127.0.0.1:8790/health -TimeoutSec 3
if($health.version -ne '0.4.3'){throw 'Wrong local host revision'}
Write-Host 'Ready: Meta Llama companion + separate Qwen camera recognition.'
if(!$Adb){Write-Host 'Headset not connected: install Android platform-tools and rerun to set up USB forwarding.'}else{Write-Host 'Pairing copied. With wireless configured, USB can now be unplugged. Keep host powered and on the same network.'}
