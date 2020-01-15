from pathlib import Path
import subprocess
import os
import sys

def run(dir):
    p = Path()
    for f in list(p.glob(dir)):
        print("Running tests on {file}".format(file=str(f)))
        subp = subprocess.Popen(['vstest.console.exe', str(f)], env=os.environ, stdout=subprocess.PIPE, universal_newlines=True)
        print(subp.communicate()[0])
        if subp.returncode != 0:
            sys.exit(subp.returncode)