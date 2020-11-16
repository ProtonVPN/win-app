from pathlib import Path
import subprocess
import os
import sys

def sign():
    exts = [".dll", ".exe"]
    mainpath = "src/bin"
    files = [p for p in Path(mainpath).rglob('*') if p.suffix in exts]
    for f in files:
        filepath = str(Path(__file__).parent.absolute()) + "\\..\\" + str(f)
        unsign(filepath)
        p = subprocess.Popen(['signtool.exe', 'sign', '/a', '/tr', 'http://timestamp.digicert.com', '/td', 'SHA256', '/fd', 'SHA256', filepath])
        p.communicate()
        if p.returncode != 0:
            print("Failed to sign: " + filepath)
            sys.exit(p.returncode)

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