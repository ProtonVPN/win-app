import os
import re
from datetime import datetime

GLOBAL_ASSEMBLY_INFO_FILE_PATH = 'src/GlobalAssemblyInfo.cs'

def get_remote_url() -> str:
    repository = os.getenv("CI_REPOSITORY_URL", "")
    user = f"git:{os.getenv('RELEASE_PAT')}"
    (_, url) = repository.split("@")
    return f"https://{user}@{url.replace(':', '/')}"

def configure_git(git_email, git_username) -> None:
    os.system(f"git config user.email \"{git_email}\"")
    os.system(f"git config user.name \"{git_username}\"")

def checkout_redesign() -> None:
    BRANCH = "redesign"
    os.system(f"git fetch origin {BRANCH}:{BRANCH}")
    os.system(f"git checkout {BRANCH}")
    os.system(f"git remote set-url origin {get_remote_url()}")

def checkout_branch(name) -> None:
    os.system(f"git checkout -b {name}")

def push_branch(name) -> None:
    os.system(f"git push --set-upstream origin {name}")

def create_commit(message) -> None:
    os.system(f"git commit -m \"{message}\"")

def create_branch(version, branch_name, commit_message) -> None:
    checkout_redesign()
    checkout_branch(branch_name)
    update_app_version(version)
    create_commit(commit_message)
    push_branch(branch_name)

def create_debug_branch(version, commit_message) -> None:
    create_branch(version, f"debug/{version}", commit_message)

def create_release_branch(version, commit_message) -> None:
    create_branch(version, f"release/{version}", commit_message)

def update_app_version(version) -> None:
    content = ''
    with open(GLOBAL_ASSEMBLY_INFO_FILE_PATH, encoding='utf-8') as f:
        content = f.read()
        content = re.sub(r"(AssemblyVersion\(\")([0-9]+\.[0-9]+\.[0-9]+)", rf"\g<1>{version}", content)
        content = re.sub(r"(AssemblyFileVersion\(\")([0-9]+\.[0-9]+\.[0-9]+)", rf"\g<1>{version}", content)
    with open(GLOBAL_ASSEMBLY_INFO_FILE_PATH, 'w') as f:
        f.write(content)

    os.system(f"git add {GLOBAL_ASSEMBLY_INFO_FILE_PATH}")

version = os.getenv('APP_VERSION')
if version == None:
    raise Exception("Missing env variable APP_VERSION")

configure_git(os.getenv('RELEASE_GIT_EMAIL'), os.getenv('RELEASE_GIT_USERNAME'))

create_release_branch(version, f"Increase app version to {version}")
create_debug_branch(version, f"Increase app version to {version}")

dateTime = datetime.now().strftime("%Y%m%d%H%M%S")
create_branch('9.9.9', f"release/9.9.9-from-{version}-{dateTime}", f"Build app version 9.9.9 to test {version} installer")