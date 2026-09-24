"""Validate and deterministically pack final authored PNGs and motion meshes."""
import argparse
import hashlib
import json
from pathlib import Path
import struct
import zipfile
from PIL import Image

COUNTS = (7,8,8,4,5,8,6,6,6,8,8,6,8,8,8,8,8,8,8,8,8,8,8,8)

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--root',type=Path,required=True)
    parser.add_argument('--archive',type=Path,required=True)
    parser.add_argument('--report',type=Path,required=True)
    args=parser.parse_args()
    errors=[]; records=[]; gaze=[]
    expected={f'frames/r{r:02}/c{c:02}.png' for r,n in enumerate(COUNTS) for c in range(n)}
    actual={p.relative_to(args.root).as_posix() for p in (args.root/'frames').rglob('*') if p.is_file()}
    if actual!=expected: errors.append('PNG paths/count differ from 24-row, 176-frame protocol')
    for name in sorted(expected & actual):
        path=args.root/name
        try:
            with Image.open(path) as im:
                im.load()
                if im.mode!='RGBA' or im.size!=(528,808): errors.append(name+': not 528x808 RGBA')
                alpha=im.getchannel('A'); bounds=alpha.getbbox()
                if not bounds or bounds[0]<8 or bounds[1]<8 or bounds[2]>520 or bounds[3]>800: errors.append(name+': empty or clipped silhouette')
                if alpha.getextrema()!=(0,255): errors.append(name+': missing transparent background/opaque core')
                if name.startswith(('frames/r09/','frames/r10/')): gaze.append({'frame':name,'bounds':bounds})
                records.append({'path':name,'bytes':path.stat().st_size,'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
        except Exception as e: errors.append(name+': '+str(e))
    if gaze:
        heights=[item['bounds'][3]-item['bounds'][1] for item in gaze]
        feet=[item['bounds'][3] for item in gaze]
        if max(heights)-min(heights)>2 or len(set(feet))!=1:
            errors.append('gaze rows must share the same figure height and foot anchor')
    motions=[]
    required_pairs=set()
    for r,n in enumerate(COUNTS):
        if r in (9,10): continue
        for c in range(n):
            required_pairs.add((r,c,r,(c+1)%n))
            if r: required_pairs.add((r,c,0,0))
    required_pairs.add((0,5,0,0))
    required_pairs.add((22,3,22,6))
    for r,c,tr,tc in sorted(required_pairs):
        name=f'motion/r{r:02}/c{c:02}-r{tr:02}-c{tc:02}.mtn'
        path=args.root/name
        if not path.is_file(): errors.append(name+': missing'); continue
        data=path.read_bytes()
        if len(data)!=3412 or struct.unpack('<4sHHHH',data[:12])!=(b'XWM1',17,25,132,202): errors.append(name+': invalid motion format')
        motions.append({'path':name,'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()})
    args.report.parent.mkdir(parents=True,exist_ok=True)
    report={'ok':not errors,'frames':len(records),'rows':24,'motionFields':len(motions),'gazeBounds':gaze,'visualAcceptance':'requires human review','errors':errors,'files':records+motions}
    args.report.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
    if errors: raise SystemExit('\n'.join(errors))
    args.archive.parent.mkdir(parents=True,exist_ok=True)
    with zipfile.ZipFile(args.archive,'w') as z:
        for record in records+motions:
            info=zipfile.ZipInfo(record['path'],date_time=(2026,1,1,0,0,0)); info.compress_type=zipfile.ZIP_DEFLATED
            z.writestr(info,(args.root/record['path']).read_bytes())
    print(json.dumps({'ok':True,'frames':len(records),'motionFields':len(motions),'archiveBytes':args.archive.stat().st_size,'sha256':hashlib.sha256(args.archive.read_bytes()).hexdigest()}))

if __name__=='__main__': main()
