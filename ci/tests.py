from pathlib import Path
import subprocess
import os
import sys

def run(dir):
    p = Path()
    for f in list(p.glob(dir)):
        if "ProtonVPN.UI.Test.dll" in str(f):
            continue

        runSingle(str(f))

def runSingle(file):
    print("Running tests on {file}".format(file=file))
    subp = subprocess.Popen(['vstest.console.exe', file], env=os.environ, stdout=subprocess.PIPE, universal_newlines=True)
    print(subp.communicate()[0])
    if subp.returncode != 0:
        sys.exit(subp.returncode)