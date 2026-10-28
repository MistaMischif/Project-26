"""Original Living Table candy-garden waltz. No external samples or melodies."""
import numpy as np, wave
from pathlib import Path
SR=44100; beat=.5; bars=32; duration=bars*3*beat; N=int(duration*SR)
out=np.zeros((N,2),dtype=np.float64)
def note(midi,start,length,gain,voice='bell',pan=0):
 t=np.arange(int((length+1)*SR))/SR; f=440*2**((midi-69)/12)
 if voice=='bell':
  env=(1-np.exp(-t*120))*np.exp(-t/ max(.16,length*.65))
  sig=(np.sin(2*np.pi*f*t)+.23*np.sin(2*np.pi*f*2*t)*np.exp(-t*4)+.08*np.sin(2*np.pi*f*3*t)*np.exp(-t*8))*env
 else:
  env=(1-np.exp(-t*90))*np.exp(-t/max(.10,length*.7))
  sig=(np.sin(2*np.pi*f*t)+.18*np.sin(2*np.pi*f*2*t)+.05*np.sin(2*np.pi*f*3*t))*env
 sig*=gain;idx=(int(start*SR)+np.arange(len(t)))%N
 out[idx,0]+=sig*np.sqrt((1-pan)/2);out[idx,1]+=sig*np.sqrt((1+pan)/2)
chords=[(43,[67,71,74]),(48,[67,72,76]),(50,[66,69,74]),(43,[67,71,74]),(40,[67,71,76]),(45,[69,72,76]),(50,[66,69,74]),(43,[67,71,74])]
motifs=[[79,83,86,83,81,79],[79,84,88,86,84,83],[81,86,84,81,78,81],[83,81,79,74,79,0],[83,86,88,86,83,79],[84,88,86,84,81,0],[81,78,81,86,84,81],[83,81,79,0,74,79]]
for bar in range(bars):
 start=bar*1.5;bass,chord=chords[bar%8]
 note(bass,start,.7,.16,'pluck',-.1)
 for b in [1,2]:
  for j,m in enumerate(chord):note(m,start+b*beat+j*.012,.22,.045,'pluck',-.35+j*.35)
 melody=motifs[bar%8]
 for k,m in enumerate(melody):
  if m:note(m+(12 if bar>=24 and k%3==0 else 0),start+k*.25,.32,.105,'bell',.25*np.sin(bar+k))
 if bar%4==2:note(91,start+1.25,.35,.035,'bell',.65)
# Circular echoes make the tail meet the next loop naturally.
dry=out.copy()
for delay,g in [(0.12,.13),(.24,.08),(.36,.04)]:out+=np.roll(dry,int(delay*SR),axis=0)[:,::-1]*g
peak=np.max(np.abs(out));out*=.66/max(peak,1e-9)
pcm=(out*32767).astype('<i2')
path=Path('Assets/LivingTable/CandyLand/Resources/Audio/candy-garden-waltz.wav')
with wave.open(str(path),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(SR);w.writeframes(pcm.tobytes())
print('seconds',duration,'peak',np.max(abs(out)),'rms',np.sqrt(np.mean(out*out)),'loop_seam',np.max(abs(out[-1]-out[0])))
