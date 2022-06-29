import sys
import re
import os
import argparse
import win32api
import localization
import config
import signing
import tests
import installer
import ssh
import guest_hole_server_loader
import slack
import hashlib
from pathlib import Path

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

subparsers.add_parser('add-languages')

parser_a = subparsers.add_parser('globalConfig')
parser_a.add_argument('globalConfigPath', type=str, help='Path to GlobalConfig file')

subparsers.add_parser('defaultConfig')
subparsers.add_parser('tap-installer')
subparsers.add_parser('tun-installer')
subparsers.add_parser('sign')

custom_parser = subparsers.add_parser('app-installer')
custom_parser.add_argument('hash', type=str, help='Commit hash string')

custom_parser = subparsers.add_parser('add-commit-hash')
custom_parser.add_argument('hash', type=str, help='Commit hash string')

parser_c = subparsers.add_parser('tests')
parser_c.add_argument('path', type=str, help='Path for tests')

custom_parser = subparsers.add_parser('prepare-ssh')
custom_parser.add_argument('key', type=str, help='Private ssh key as a string')

subparsers.add_parser('update-gh-list')
subparsers.add_parser('send-slack-notification')

if len(sys.argv) < 2:
    parser.print_usage()
    sys.exit(1)

args = parser.parse_args()

if args.command == 'globalConfig':
    config.createGlobalConfig(args.globalConfigPath)

if args.command == 'defaultConfig':
    configPath = "src\ProtonVPN.Common\Configuration\Source\DefaultConfig.cs"
    f = open(configPath, "rt")
    data = f.read()
    data = data.replace('[InternalReleaseHost]', os.environ.get("INTERNAL_RELEASE_HOST"))
    f.close()

    f = open(configPath, "wt")
    f.write(data)
    f.close()

elif args.command == 'add-languages':
    loc = localization.Localization(
        "locales\\*.resx",
        '.\src\\ProtonVPN.Translations\\Properties\\Resources.{lang}.resx',
        '.\\src\\bin\\ProtonVPN.MarkupValidator.exe')
    returnCode = loc.AddLanguages()
    sys.exit(returnCode)

elif args.command == 'tests':
    tests.run('{path}*.Test.dll'.format(path=args.path))

elif args.command == 'sign':
    signing.sign()

elif args.command == 'app-installer':
    v = win32api.GetFileVersionInfo('.\src\\bin\ProtonVPN.exe', '\\')
    semVersion = "%d.%d.%d" % (v['FileVersionMS'] / 65536, v['FileVersionMS'] % 65536, v['FileVersionLS'] / 65536)
    params = ''
    p = Path()
    for f in list(p.glob('.\src\\bin\**\ProtonVPN.Translations.resources.dll')):
        params = params + "\r\nAddFolder APPDIR {folder}".format(folder=f.parent.absolute())

    print('Building app installer')
    err = installer.build(semVersion, args.hash, 'Setup/ProtonVPN.aip', params)
    print_sha256('.\Setup\ProtonVPN-SetupFiles\ProtonVPN_win_v{semVersion}.exe'.format(semVersion=semVersion))
    sys.exit(err)

elif args.command == 'tap-installer':
    print('Building tap installer')
    version = '1.1.4'
    err = installer.build(version, '', 'Setup/ProtonVPNTap.aip')
    print_sha256('.\Setup\ProtonVPNTap-SetupFiles\ProtonVPNTap_{version}.exe'.format(version=version))
    sys.exit(err)

elif args.command == 'tun-installer':
    print('Building tun installer')
    version = '0.13.1'
    err = installer.build(version, '', 'Setup/ProtonVPNTun.aip')
    print_sha256('.\Setup\ProtonVPNTun-SetupFiles\ProtonVPNTun_{version}.exe'.format(version=version))
    sys.exit(err)

elif args.command == 'add-commit-hash':
    path = '.\src\GlobalAssemblyInfo.cs'
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