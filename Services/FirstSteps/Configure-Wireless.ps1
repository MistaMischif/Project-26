param([string]$Address='', [string]$OpenSsl='')
$ErrorActionPreference='Stop'
if(!$Address){$Address=(Get-NetIPConfiguration | Where-Object {$_.IPv4DefaultGateway -and $_.NetAdapter.Status -eq 'Up'} | Select-Object -First 1).IPv4Address.IPAddress}
if(!$Address -or $Address -match '^127\.|^169\.254\.' -or $Address -notmatch '^\d+\.\d+\.\d+\.\d+$'){throw 'Choose the computer local IPv4 address with -Address.'}
if(!$OpenSsl){$OpenSsl=(Get-Command openssl -ErrorAction SilentlyContinue).Source}
if(!$OpenSsl){$OpenSsl='C:/Program Files/Git/usr/bin/openssl.exe'}
if(!(Test-Path -LiteralPath $OpenSsl)){throw 'Install free OpenSSL (included with Git for Windows) for wireless pairing.'}
$folder=Join-Path $PSScriptRoot 'private-tls';New-Item -ItemType Directory -Path $folder -Force | Out-Null
$cert=Join-Path $folder 'host-cert.pem';$key=Join-Path $folder 'host-key.pem'
if(!(Test-Path $cert) -or !(Test-Path $key)){$savedPreference=$ErrorActionPreference;$ErrorActionPreference='Continue';try{& $OpenSsl req -x509 -newkey rsa:2048 -nodes -keyout $key -out $cert -days 365 -subj '/CN=Living Table Local' -addext ('subjectAltName=IP:'+$Address) 2>$null}finally{$ErrorActionPreference=$savedPreference};if($LASTEXITCODE -ne 0){throw 'Certificate creation failed'}}
$pem=Get-Content -Raw $cert;$der=[Convert]::FromBase64String(($pem -replace '-----[^-]+-----','' -replace '\s',''));$sha=[Security.Cryptography.SHA256]::Create();try{$pin=([BitConverter]::ToString($sha.ComputeHash($der))).Replace('-','').ToLowerInvariant()}finally{$sha.Dispose()}
$configPath=Join-Path $PSScriptRoot 'first-steps-connection.json';if(!(Test-Path $configPath)){$bytes=New-Object byte[] 32;$rng=[Security.Cryptography.RandomNumberGenerator]::Create();try{$rng.GetBytes($bytes)}finally{$rng.Dispose()};@{url='http://127.0.0.1:8790';deviceToken=[Convert]::ToBase64String($bytes)}|ConvertTo-Json|Set-Content $configPath -Encoding UTF8};$config=Get-Content -Raw $configPath | ConvertFrom-Json
$config.url='https://'+$Address+':8791';$config | Add-Member -NotePropertyName certificateSha256 -NotePropertyValue $pin -Force
$config | ConvertTo-Json | Set-Content -LiteralPath $configPath -Encoding UTF8
Write-Output ('Wireless host configured at https://'+$Address+':8791; paired certificate pinned. Keep the computer and Quest on the same reachable network.')
