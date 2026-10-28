"""Original warm background music and soft cartoon wing flaps; no external samples."""
import numpy as np,wave
from pathlib import Path
SR=22050;beat=.625;duration=60;N=int(SR*duration);out=np.zeros((N,2));rng=np.random.default_rng(53)
def note(m,start,length,gain,pan=0,bass=False):
 t=np.arange(int((length+.5)*SR))/SR;f=440*2**((m-69)/12)
 env=(1-np.exp(-t*24))*np.exp(-t/(length*.65))
 sound=(np.sin(2*np.pi*f*t)+(.13 if bass else .06)*np.sin(2*np.pi*f*2*t))*env*gain
 idx=(int(start*SR)+np.arange(len(t)))%N
 out[idx,0]+=sound*np.sqrt((1-pan)/2);out[idx,1]+=sound*np.sqrt((1+pan)/2)
chords=[(43,[55,59,62]),(48,[55,60,64]),(45,[57,60,64]),(50,[57,62,66])]
motifs=[[67,71,69,67,64,67],[67,72,71,69,67,64],[69,72,71,69,64,67],[66,69,72,69,67,0]]
for bar in range(24):
 st=bar*4*beat;bass,chord=chords[bar%4]
 note(bass,st,.9,.17,-.15,True);note(bass+7,st+2.5*beat,.55,.11,-.15,True)
 for b in [.8,2,3.3]:
  for j,m in enumerate(chord):note(m,st+b*beat+j*.018,.5,.048,-.35+j*.35)
 for k,m in enumerate(motifs[bar%4]):
  if m:note(m,st+[0,.65,1.5,2,2.8,3.4][k]*beat,.55,.075,.2)
# Smooth rolled-off spectrum: no bells, chimes, hats, or bright transients.
freq=np.fft.rfftfreq(N,1/SR);filt=1/(1+(freq/1350)**8)
for ch in range(2):out[:,ch]=np.fft.irfft(np.fft.rfft(out[:,ch])*filt,n=N)
out+=np.roll(out,int(.19*SR),axis=0)*.08;out*=.60/np.max(np.abs(out))
folder=Path('Assets/LivingTable/CandyLand/Resources/Audio');folder.mkdir(exist_ok=True)
def save(name,data):
 data=np.clip(data,-1,1);channels=1 if data.ndim==1 else data.shape[1]
 with wave.open(str(folder/name),'wb') as w:w.setnchannels(channels);w.setsampwidth(2);w.setframerate(SR);w.writeframes((data*32767).astype('<i2').tobytes())
save('candy-soft-shuffle.wav',out)
t=np.arange(int(.44*SR))/SR;noise=rng.normal(size=len(t));noise=np.convolve(noise,np.ones(18)/18,'same');flap=np.zeros(len(t))
for at in [.035,.25]:
 env=np.exp(-((t-at)/.035)**2);flap+=env*(noise*.40+np.sin(2*np.pi*(125*t-35*t*t))*.08)
flap*=.45/max(abs(flap));save('bat-flutter.wav',flap)
print('Warm gameplay track: 60 seconds. Bat flutter: 0.44 seconds.')
