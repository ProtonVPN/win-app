import os
import re
import subprocess
from pathlib import Path

class Localization:
    src = ''
    dst = ''
    validator = ''

    def __init__(self, src, dst, validator):
        self.src = src
        self.dst = dst
        self.validator = validator

    def AddLanguages(self):
        returnCode = 0
        for f in list(Path().glob(self.src)):
            lang = self.getLanguageCode(f)
            if self.validate(f.absolute()):
                self.AddLanguage(f)
            else:
                returnCode = 1
                break
        return returnCode

    def validate(self, file):
        print(file)
        code = subprocess.call("{validator} \"{file}\"".format(validator=self.validator, file=file, shell=True))
        return code == 0

    def AddLanguage(self, src):
        lang = self.getLanguageCode(src)
        dest = self.dst.format(lang=lang)
        os.replace(str(src), dest)

    def getLanguageCode(self, path):
        match = re.match('.+ProtonVPN Windows app\.(.+)\.resx', str(path))
        return match.group(1)