import os
import re
import subprocess

def build(version, hash, setupFile):

    createSetupFile(version, hash)

    p = subprocess.Popen(['iscc', setupFile, "/Ssigntool=signtool.exe $p"],
                         env=os.environ,
                         stdout=subprocess.PIPE,
                         universal_newlines=True)
    print(p.communicate()[0])

    return p.returncode
    
def createSetupFile(version, hash):
    baseSetupFile = 'Setup/SetupBase.iss'
    fileData = ''
    with open(baseSetupFile, 'r') as file:
        fileData = file.read()

    fileData = re.sub(r'(#define MyAppVersion \")([0-9+]\.[0-9+]\.[0-9+])(\".+)', rf'\g<1>{version}\3', fileData, 1, flags = re.M | re.DOTALL)

    if hash:
        fileData = fileData.replace('#define Hash ""', "#define Hash \"{hash}\"".format(hash=hash))

    with open(baseSetupFile, 'w') as file:
        file.write(fileData)