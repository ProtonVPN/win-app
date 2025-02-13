import sys
import re
import os
import argparse
import win32api
import installer
import ssh
import guest_hole_server_loader
import slack
import hashlib

def get_sha256(file_path):
    sha256_hash = hashlib.sha256()
    with open(file_path, "rb") as f:
        for byte_block in iter(lambda: f.read(4096), b""):
            sha256_hash.update(byte_block)
    return sha256_hash.hexdigest()

def print_sha256(file_path):
    sha256 = get_sha256(file_path)
    print(os.path.basename(file_path) + ' SHA256: ' + sha256)

parser = argparse.ArgumentParser(description='ProtonVPN CI')
subparsers = parser.add_subparsers(help='sub-command help', dest='command')

subparsers.add_parser('lint-languages')
subparsers.add_parser('defaultConfig')
subparsers.add_parser('sign')

custom_parser = subparsers.add_parser('app-installer')
custom_parser.add_argument('hash', type=str, help='Commit hash string')
custom_parser.add_argument('platform', type=str, help='Platform: x64 or arm64')

custom_parser = subparsers.add_parser('app-bti-installer')
custom_parser.add_argument('hash', type=str, help='Commit hash string')

custom_parser = subparsers.add_parser('add-commit-hash')
custom_parser.add_argument('hash', type=str, help='Commit hash string')

custom_parser = subparsers.add_parser('prepare-ssh')
custom_parser.add_argument('key', type=str, help='Private ssh key as a string')

subparsers.add_parser('update-gh-list')
subparsers.add_parser('send-slack-notification')

if len(sys.argv) < 2:
    parser.print_usage()
    sys.exit(1)

args = parser.parse_args()

if args.command == 'defaultConfig':
    configPath = "src\ProtonVPN.Common\Configuration\Source\DefaultConfig.cs"
    f = open(configPath, "rt")
    data = f.read()
    data = data.replace('[InternalReleaseHost]', os.environ.get("INTERNAL_RELEASE_HOST"))
    f.close()

    f = open(configPath, "wt")
    f.write(data)
    f.close()

elif args.command == 'app-installer':
    build_path = os.environ.get('BUILD_PATH', '.\\src\\bin\\win-x64\\publish\\')
    exe_path = os.path.join(build_path, 'ProtonVPN.exe')
    print('Executable File Path:', exe_path)
    v = win32api.GetFileVersionInfo(exe_path, '\\')
    semVersion = "%d.%d.%d" % (v['FileVersionMS'] / 65536, v['FileVersionMS'] % 65536, v['FileVersionLS'] / 65536)
    print('Building app installer')
    err = installer.build(semVersion, args.hash, 'Setup/Setup.{platform}.iss'.format(platform=args.platform))
    installer_filename = 'ProtonVPN_v{semVersion}_{platform}.exe'.format(semVersion=semVersion, platform=args.platform)

    if 'BTI' in build_path:
        installer_filename = 'ProtonVPN_v{semVersion}_{platform}_BTI.exe'.format(semVersion=semVersion, platform=args.platform)
    installer_path = os.path.join(r'.\Setup\Installers', installer_filename)

    print_sha256(installer_path)
    sys.exit(err)

elif args.command == 'add-commit-hash':
    path = r'.\src\GlobalAssemblyInfo.cs'
    with open(path, 'r') as file:
        content = file.read()
        matches = re.search(r"AssemblyVersion\(\"([0-9]+.[0-9]+.[0-9]+)", content)
        data = content.replace('$AssemblyVersion', '{version}-{hash}'.format(version=matches[1], hash=args.hash))
    with open(path, 'w') as file:
        file.write(data)

elif args.command == 'prepare-ssh':
    print('Writing ssh key to the file')
    ssh.prepare(args.key)

elif args.command == 'update-gh-list':
    print('Executing guest hole server loader')
    guest_hole_server_loader.load()

elif args.command == 'send-slack-notification':
    print('Sending installer file to slack')
    slack.send()