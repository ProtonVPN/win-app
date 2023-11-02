import os
import re
import subprocess
import pathlib

def build(version, hash, setupFile):
    fileData = ''
    with open(setupFile, 'r') as file:
      fileData = file.read()

    fileData = re.sub('(#define MyAppVersion \")([0-9+]\.[0-9+]\.[0-9+])(\".+)', rf'\g<1>{version}\3', fileData, 1, flags = re.M | re.DOTALL)

    if hash:
        fileData = fileData.replace('#define Hash ""', "#define Hash \"{hash}\"".format(hash=hash))

    setupFile = pathlib.Path(setupFile).parent.resolve().__str__() + '\\' + 'setup-edited.iss'
    print(setupFile)
    with open(setupFile, 'w') as file:
      file.write(fileData)

    p = subprocess.Popen(['iscc', setupFile, "/Ssigntool=signtool.exe $p"],
                         env=os.environ,
                         stdout=subprocess.PIPE,
                         universal_newlines=True)
    print(p.communicate()[0])

    return p.returncode