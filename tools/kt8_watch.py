"""КТ №8: локальный CI. Изменение .cs/.csproj/.props -> restore/build/test.
DOTNET_EXE задает путь к SDK. Без --watch выполняет один pipeline.
"""
import argparse
import hashlib
import os
from pathlib import Path
import subprocess
import time
root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument('--watch', action='store_true')
parser.add_argument('--max-runs', type=int, default=0)
args = parser.parse_args()
dotnet = os.environ.get('DOTNET_EXE', 'dotnet')

def fingerprint():
    digest = hashlib.sha256()
    for base in ['src', 'tests']:
        for p in sorted((root/base).rglob('*')):
            if p.suffix in ['.cs','.csproj','.props'] and not {'bin','obj'}.intersection(p.parts):
                digest.update(str(p.relative_to(root)).encode()); digest.update(p.read_bytes())
    return digest.hexdigest()

def run(number, revision):
    folder = root/'artifacts'/'KT8'/('run-%02d'%number)
    folder.mkdir(parents=True, exist_ok=True)
    environment = dict(os.environ, UseOnlyInMemoryDatabase='true', ASPNETCORE_ENVIRONMENT='Development', DOTNET_USE_POLLING_FILE_WATCHER='1')
    print('КТ №8 RUN %d SOURCE %s' % (number, revision[:12]), flush=True)
    code = 0
    for project in ['UnitTests','IntegrationTests','FunctionalTests']:
        command = [dotnet,'test','tests/'+project,'--blame-hang-timeout','60s','--logger','trx','--results-directory',str(folder/project)]
        with (folder/(project+'.log')).open('w') as log:
            result = subprocess.run(command,cwd=root,env=environment,stdout=log,stderr=subprocess.STDOUT)
        print('%s exit=%d'%(project,result.returncode), flush=True)
        code = code or result.returncode
    (folder/'status.txt').write_text('source='+revision+'\nexit='+str(code)+'\n')
    print('КТ №8 RUN %d %s'%(number,'PASS' if code == 0 else 'FAIL'), flush=True)
    return code

last = None
number = 0
while True:
    current = fingerprint()
    if current != last:
        last=current; number += 1; result=run(number,current)
        if not args.watch or (args.max_runs and number >= args.max_runs):
            raise SystemExit(result)
    time.sleep(1)
