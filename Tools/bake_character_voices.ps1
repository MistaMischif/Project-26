$ErrorActionPreference='Stop'
$voiceFolder=Join-Path $PSScriptRoot '../Assets/LivingTable/CandyLand/Resources/Audio/Voices'
New-Item -ItemType Directory -Force -Path $voiceFolder | Out-Null
$profiles=@(
 @{id='plumpy';voice='DavidM';pitch=-4;rate=-1;lines=@('Oops! My berries are making a break for it!','I was saving those for my second breakfast!')},
 @{id='mr-mint';voice='MarkM';pitch=4;rate=1;lines=@('Fresh candy canes! No watering required!','You have got me in mint condition!')},
 @{id='jolly';voice='DavidM';pitch=3;rate=1;lines=@('That tickles my gumdrops!','I am not wobbly. I am extra jolly!')},
 @{id='gramma-nutt';voice='ZiraM';pitch=-4;rate=-2;lines=@('Careful, dear. My pockets are full of peanuts!','A little nutty? Why, thank you!')},
 @{id='princess-lolly';voice='ZiraM';pitch=5;rate=1;lines=@('One royal twirl, extra sprinkles!','My dance moves are pop approved!')},
 @{id='queen-frostine';voice='ZiraM';pitch=-1;rate=-1;lines=@('Stay cool, my sweet little snowflake.','A royal wiggle? How ice of you!')},
 @{id='king-kandy';voice='DavidM';pitch=-6;rate=-1;lines=@('By royal decree, more giggles!','My crown is straight. My jokes are crooked!')},
 @{id='lord-licorice';voice='MarkM';pitch=-5;rate=-1;lines=@('My bats! They always leave before the punch line!','Behold, my very dramatic entrance!')},
 @{id='gloppy';voice='DavidM';pitch=-8;rate=-3;lines=@('Oops. Was that your shoe, or my snack?','I give the stickiest hugs in Candy Land!')}
)
foreach($profile in $profiles){
 for($i=0;$i -lt $profile.lines.Count;$i++){
  $talker=New-Object -ComObject SAPI.SpVoice
  $token=New-Object -ComObject SAPI.SpObjectToken
  $token.SetId('HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens\MSTTS_V110_enUS_'+$profile.voice)
  $talker.Voice=$token;$talker.Rate=$profile.rate
  $stream=New-Object -ComObject SAPI.SpFileStream;$stream.Format.Type=22
  $path=[IO.Path]::GetFullPath((Join-Path $voiceFolder ($profile.id+'-'+$i+'.wav')))
  $stream.Open($path,3,$false);$talker.AudioOutputStream=$stream
  $escaped=[System.Security.SecurityElement]::Escape($profile.lines[$i])
  $null=$talker.Speak('<pitch middle="'+$profile.pitch+'">'+$escaped+'</pitch>',8)
  $stream.Close()
  [Runtime.InteropServices.Marshal]::ReleaseComObject($stream) | Out-Null
  [Runtime.InteropServices.Marshal]::ReleaseComObject($talker) | Out-Null
  [Runtime.InteropServices.Marshal]::ReleaseComObject($token) | Out-Null
 }
}
$profiles | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $PSScriptRoot 'candy-character-voice-profiles.json')
Write-Output '18 offline character lines exported.'
