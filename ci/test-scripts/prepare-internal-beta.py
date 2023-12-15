from glob import glob
from datetime import datetime, timezone
import requests
from win32com.client import *
import json
import hashlib
import os
import shutil

project_path = os.getenv('CI_PROJECT_DIR') + "\protonvpn-windows-alpha"
git = f'git -C "{project_path}"'
win_update_prod_endpoint = "https://protonvpn.com/download/windows-releases.json"

def delete_all_files_by_ending(dir_path, file_extenstion):
    for root, _, files in os.walk(dir_path):
        for file in files: 
            if file.endswith(file_extenstion):
                os.remove(os.path.join(root, file)) 
  
def get_version_number(file_path):
    information_parser = Dispatch("Scripting.FileSystemObject")
    version = information_parser.GetFileVersion(file_path)
    return version

def get_file_hash(filename, hash, bytes):
   with open(filename,'rb') as file:
       chunk = 0
       while chunk != b'':
           chunk = file.read(bytes)
           hash.update(chunk)

   return hash.hexdigest()

def generate_file_json(version, installer_path):
    file_json = {}
    file_json['Url'] = f'{os.getenv("INTERNAL_INSTALLER_URL")}/ProtonVPN_v' + version + '.exe'
    file_json['SHA1CheckSum'] = get_file_hash(installer_path, hashlib.sha1(), 1024)
    file_json['SHA256CheckSum'] = get_file_hash(installer_path, hashlib.sha256(), 4096)
    file_json['SHA512CheckSum'] = get_file_hash(installer_path, hashlib.sha512(), 4096)
    file_json['Arguments'] = '/silent'
    return file_json

def configure_git(git_email, git_username):
    os.system(f"{git} config user.email \"{git_email}\"")
    os.system(f"{git} config user.name \"{git_username}\"")

def get_update_json(version, installer_path):
    update_json = {}
    update_json['Version'] = version
    update_json['File'] = generate_file_json(version, installer_path)
    update_json['ChangeLog'] = get_changelog()
    update_json['ReleaseDate'] = datetime.now(timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ')
    update_json['MinimumOsVersion'] = "10.0.17763"
    update_json["RolloutPercentage"] = 100
    return update_json

def get_changelog():
    commit_hash = os.popen('git log --grep="Increase app version to" --pretty=format:%h -1 --skip=1').read()
    return os.popen(f'git log {commit_hash}..HEAD --pretty=format:%s').read().split('\n')

def push_new_internal_beta(version):
    os.system(f'{git} add .')
    os.system(f'{git} checkout -b "internal-beta/{version}"')
    os.system(f'{git} commit -m "Internal beta release {version}"')
    os.system(f'{git} push --set-upstream origin internal-beta/{version} -o merge_request.create')   

def clone_main_branch():
    os.system(f'git clone https://{os.getenv("INTERNAL_BETA_TOKEN_NAME")}:{os.getenv("INTERNAL_BETA_TOKEN")}@{os.getenv("INTERNAL_BETA_REPO")}')

def move_files_to_beta_folder(win_update_json, installer_path):
    with open(f'{project_path}\\public\\win-update.json', 'w', encoding='utf-8') as f:
        json.dump(win_update_json, f, ensure_ascii=False, indent=4)
    shutil.copy(installer_path,f'{project_path}\\public')

def get_full_update_json(version, installer_path):
    win_update_response = requests.get(win_update_prod_endpoint).text.strip()
    win_update_json = json.loads(win_update_response)
    releases_array = win_update_json["Categories"][0]["Releases"]
    releases_array.insert(0,get_update_json(version, installer_path))
    win_update_json["Categories"][0]["Releases"] = releases_array
    return win_update_json

if __name__ == '__main__':
    installer_path = glob(os.getenv('CI_PROJECT_DIR') + '\\setup\\Installers\\Proton' + '*.exe')[0]
    version = get_version_number(installer_path)[:-2]

    clone_main_branch()
    configure_git(os.getenv('RELEASE_GIT_EMAIL'), os.getenv('RELEASE_GIT_USERNAME'))
    delete_all_files_by_ending(project_path, '.exe')
    win_update_json = get_full_update_json(version, installer_path)
    move_files_to_beta_folder(win_update_json, installer_path)
    push_new_internal_beta(version)