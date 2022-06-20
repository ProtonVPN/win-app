from pathlib import Path
import subprocess
import os
import sys

def run(dir):
    p = Path()
    for f in list(p.glob(dir)):
        filePath = str(f)
        if "ProtonVPN.UI.Test.dll" in filePath or (not filePath.endswith('Test.dll') and not filePath.endswith('Tests.dll')):
            continue

        print(filePath)
        runSingle(filePath)

def runSingle(file):
    print("Running tests on {file}".format(file=file))
    subp = subprocess.Popen(['vstest.console.exe', file], env=os.environ, stdout=subprocess.PIPE, universal_newlines=True)
    print(subp.communicate()[0])
    if subp.returncode != 0:
        sys.exit(subp.returncode)