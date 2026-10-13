"""КТ №4–7: подписанные, последовательно выполняемые этапы интеграции."""
import os
from pathlib import Path
import subprocess
import sys
import argparse
parser=argparse.ArgumentParser()
parser.add_argument("--kt", choices=["4","5","6","7"])
args=parser.parse_args()
root=Path(__file__).resolve().parents[1]
dotnet=os.environ.get('DOTNET_EXE','dotnet')
env=dict(os.environ,UseOnlyInMemoryDatabase='true',DOTNET_USE_POLLING_FILE_WATCHER='1',ASPNETCORE_ENVIRONMENT='Development')
steps=[('KT4','big-bang','FunctionalTests','KT=4'),('KT4','ownership-regression','IntegrationTests','KT=4'),
       ('KT5','01-controller-stubs','FunctionalTests','KT=5&Stage=1'),('KT5','02-service-stubs','IntegrationTests','KT=5&Stage=2'),
       ('KT5','03-real-repositories','IntegrationTests','KT=5&Stage=3'),('KT5','04-web-all-layers','FunctionalTests','KT=5&Stage=4'),
       ('KT6','01-domain','IntegrationTests','KT=6&Stage=1'),('KT6','02-repository','IntegrationTests','KT=6&Stage=2'),
       ('KT6','03-service','IntegrationTests','KT=6&Stage=3'),('KT6','04-web','FunctionalTests','KT=6&Stage=4'),
       ('KT7','hybrid-layers','IntegrationTests','KT=7'),('KT7','hybrid-web','FunctionalTests','KT=7')]
failed=0
for kt,stage,project,query in steps:
    if args.kt and kt != "KT"+args.kt: continue
    out=root/'artifacts'/kt/stage;out.mkdir(parents=True,exist_ok=True)
    command=[dotnet,'test','tests/'+project,'--no-restore','--filter',query,'--blame-hang-timeout','60s','--logger','trx','--results-directory',str(out)]
    print('%s %s START'%(kt,stage),flush=True)
    with (out/'console.log').open('w') as log:
        result=subprocess.run(command,cwd=root,env=env,stdout=log,stderr=subprocess.STDOUT)
    import xml.etree.ElementTree as ET
    count=0
    for trx in out.glob('*.trx'):
        counters=ET.parse(trx).find('.//{*}Counters')
        if counters is not None: count+=int(counters.attrib.get('total','0'))
    passed=result.returncode==0 and count>0
    print('%s %s %s tests=%d'%(kt,stage,'PASS' if passed else 'FAIL',count),flush=True)
    failed+=not passed
sys.exit(bool(failed))
