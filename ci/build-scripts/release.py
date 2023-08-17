import os
import re

def get_remote_url():
    repository = os.getenv("CI_REPOSITORY_URL", "")
    user = f"git:{os.getenv('RELEASE_PAT')}"
    (_, url) = repository.split("@")
    return f"https://{user}@{url.replace(':', '/')}"

def configure_git(git_email, git_username):
    os.system(f"git config user.email \"{git_email}\"")
    os.system(f"git config user.name \"{git_username}\"")

def checkout_develop():
    os.system("git fetch origin develop-v2:develop-v2")
    os.system("git checkout develop-v2")
    os.system(f"git remote set-url origin {get_remote_url()}")

def checkout_branch(name):
    os.system(f"git checkout -b {name}")

def delete_branch(name):
    os.system(f"git push origin --delete {name}")

def push_branch(name):
    os.system(f"git push --set-upstream origin {name}")

def create_commit(message):
    os.system(f"git commit -m \"{message}\"")

def create_debug_branch(version):
    branch = f"debug/{version}"
    checkout_branch(branch)
    push_branch(branch)

def create_release_branch(version, commit_message):
    checkout_develop()
    branch = f"release/{version}"
    checkout_branch(branch)
    update_app_version(version)
    create_commit(commit_message)
    push_branch(branch)

def create_release_and_debug_branches(version):
    create_release_branch(version, f"Increase app version to {version}")
    create_debug_branch(version)

def update_app_version(version):
    file_path = 'src/GlobalAssemblyInfo.cs'
    content = ''
    with open(file_path, encoding='utf-8') as f:
        content = f.read()
        content = re.sub(r"(AssemblyVersion\(\")([0-9]+\.[0-9]+\.[0-9]+)", rf"\g<1>{version}", content)
        content = re.sub(r"(AssemblyFileVersion\(\")([0-9]+\.[0-9]+\.[0-9]+)", rf"\g<1>{version}", content)
    with open(file_path, 'w') as f:
        f.write(content)

    os.system(f"git add {file_path}")

version = os.getenv('APP_VERSION')
if version == None:
    raise Exception("Missing env variable APP_VERSION")

configure_git(os.getenv('RELEASE_GIT_EMAIL'), os.getenv('RELEASE_GIT_USERNAME'))

create_release_and_debug_branches(version)
delete_branch("release/9.9.9")
create_release_branch('9.9.9', f"Build app version 9.9.9 to test {version} installer")