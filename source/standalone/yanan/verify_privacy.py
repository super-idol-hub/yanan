"""Check tracked source and embedded asset paths for common accidental leaks."""
import json
from pathlib import Path
import re
import subprocess
import zipfile

root = Path(__file__).resolve().parents[3]
files = subprocess.check_output(['git', '-c', 'safe.directory='+root.as_posix(), 'ls-files', '-z'], cwd=root).decode().split('\0')
patterns = [re.compile(r'gh[pousr]_[A-Za-z0-9]{20,}'), re.compile(r'github_pat_[A-Za-z0-9_]{30,}'),
            re.compile(r'[A-Za-z]:[\\/](?:Users|super_idol)[\\/]', re.I),
            re.compile(r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----')]
errors = []
text_count = 0
for name in filter(None, files):
    path = root/name
    if path.suffix.lower() in {'.cs','.py','.ps1','.md','.json','.yml','.bat','.manifest'}:
        text_count += 1
        text = path.read_text(encoding='utf-8-sig')
        if any(pattern.search(text) for pattern in patterns): errors.append(name + ': potential secret or local path')
    if re.search(r'(^|/)(references|sheets|work)/', name) or path.suffix.lower() in {'.exe','.dll','.mp3','.mp4','.wav'}:
        errors.append(name + ': forbidden source artifact')
embedded_entries = {}
for skin in ('noir', 'stage', 'crimson'):
    with zipfile.ZipFile(root/f'source/standalone/yanan/resources/{skin}-frames.zip') as archive:
        names = archive.namelist()
        embedded_entries[skin] = len(names)
        if any(not re.fullmatch(r'(?:frames/r\d{2}/c\d{2}\.png|motion/r\d{2}/c\d{2}-r\d{2}-c\d{2}\.mtn)', n) for n in names):
            errors.append(skin + ': unexpected embedded asset')
report = {'ok': not errors, 'checkedTextFiles': text_count, 'embeddedEntries':embedded_entries,
          'scope':'tracked filenames, common credential patterns, local absolute paths, embedded archive paths', 'errors':errors}
(root/'qa/evidence/privacy-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(report))
raise SystemExit(1 if errors else 0)
