from pathlib import Path
import subprocess
import os
import sys

def sign():
    exts = [".dll", ".exe"]
    mainpath = "src/bin"
    files = [p for p in Path(mainpath).rglob('*') if p.suffix in exts]
    filepaths = []
    
    for f in files:
        filepath = str(f)
        unsign(filepath)
        filepaths.append(filepath)

        if len(filepaths) >= 30:
            filepaths = sign_filepaths(filepaths)
        
    if len(filepaths) > 0:
        filepaths = sign_filepaths(filepaths)

def sign_filepaths(filepaths):
    cmd = ['signtool.exe', 'sign', '/a', '/tr', 'http://timestamp.globalsign.com/tsa/r6advanced1', '/td', 'SHA256', '/fd', 'SHA256']
    cmd.extend(filepaths)
    filepaths.clear()
    
    p = subprocess.Popen(cmd)
    p.communicate()
    
    if p.returncode != 0:
        print("Failed to sign the files.")
        sys.exit(p.returncode)
        
    return filepaths


def unsign(filepath):
    p = subprocess.Popen(['dumpbin.exe', '/HEADERS', filepath], stdout=subprocess.PIPE)
    out, err = p.communicate()

    if "machine (x86)" in str(out):
        delcert = 'ci\delcert_x86.exe'
    else:
        delcert = 'ci\delcert_x64.exe'

    print(delcert + " " + filepath)

    p = subprocess.Popen([delcert, filepath])
    p.communicate()