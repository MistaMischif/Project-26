from pathlib import Path
from PIL import Image,ImageEnhance
import json
root=Path('Services/FirstSteps/fixtures');target=root/'holdout';target.mkdir(exist_ok=True);items=[]
for base in [0,1,2,3,6,7]:
 for variant,(brightness,angle) in enumerate([(0.85,-3),(1.10,3),(1.0,0)]):
  im=Image.open(root/f'{base}.jpg').convert('RGB');background=im.getpixel((0,0));im=im.rotate(angle,resample=Image.Resampling.BICUBIC,fillcolor=background);im=ImageEnhance.Brightness(im).enhance(brightness)
  name=f'{base}-{variant}.jpg';im.save(target/name,quality=77 if variant==2 else 85)
  items.append({'file':name,'expected':(root/f'{base}.txt').read_text().strip(),'source':base,'brightness':brightness,'rotation':angle})
(target/'manifest.json').write_text(json.dumps({'kind':'Synthetic perturbation holdout; related to development fixtures, not independent real-board data','items':items},indent=2)+'\n')
