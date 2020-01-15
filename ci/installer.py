import os
import subprocess

def build(version, hash, setupFile, params=''):
    installerFile = 'build-installer.txt'
    script = """;aic
SetVersion {version}""".format(version=version, hash=hash)

    if hash:
        script = script + "\r\nSetProperty CommitHash=\"{hash}\"".format(hash=hash)

    if params:
        script = script + "\r\n{params}".format(params=params)
    
    script = script + "\r\nSave\r\nRebuild"
    f = open(installerFile, 'w')
    f.write(script)
    f.close()

    p = subprocess.Popen(['AdvancedInstaller.com', '/execute', setupFile, installerFile],
                         env=os.environ,
                         stdout=subprocess.PIPE,
                         universal_newlines=True)
    print(p.communicate()[0])

    return p.returncode