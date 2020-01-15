import requests
import zipfile
import io
import os
import re
import shutil
import json
from pathlib import Path
import subprocess

class Localization:
    key = ''
    outputPath = ''
    languageDst = ''
    languages = ''
    validator = ''
    baseUrl = "https://api.crowdin.com/api/project/protonvpn/"
    zipUrl = baseUrl + "download/all.zip?key={key}"

    def __init__(self, key, languages, outputPath, languageDst, validator):
        self.key = key
        self.languages = languages
        self.outputPath = outputPath
        self.languageDst = languageDst
        self.validator = validator

    def downloadTranslations(self):   
        self.downloadZip()
        p = Path()
        error = False
        for f in list(p.glob('.\langs\**\*.resx')):
            if self.validate(f.absolute()):
                self.AddLanguage(f)
            else:
                error = True
                break
        
        shutil.rmtree(self.outputPath)
        
        return 1 if error else 0
        
    def validate(self, file):
        print(file)
        code = subprocess.call("{validator} \"{file}\"".format(validator=self.validator, file=file, shell=True))
        return code == 0

    def AddLanguage(self, src):
        lang = self.getLanguageCode(src)
        if lang in self.languages.split(','):
            dest = self.languageDst.format(lang=lang)
            os.replace(str(src), dest)

    def getLanguageCode(self, path):
        match = re.match('.+\.([A-Za-zl-]+)\.', str(path))
        return match.group(1)

    def downloadZip(self):
        response = requests.get(self.zipUrl.format(key=self.key))
        with zipfile.ZipFile(io.BytesIO(response.content)) as zip:
            zip.extractall(self.outputPath)