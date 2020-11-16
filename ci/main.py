import sys
import re
import argparse
import win32api
import localization
import sentry
import signing
import tests
import installer
import ssh
import os
from pathlib import Path

parser = argparse.ArgumentParser(description='ProtonVPN CI')
subparsers = parser.add_subparsers(help='sub-command help', dest='command')

subparsers.add_parser('add-languages')

parser_a = subparsers.add_parser('sentry')
parser_a.add_argument('path', type=str, help='Path to GlobalConfig file')
parser_a.add_argument('dsn', type=str, help='Sentry dsn string', nargs='?', const='')

subparsers.add_parser('tap-installer')
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

if len(sys.argv) < 2:
    parser.print_usage()
    sys.exit(1)

args = parser.parse_args()

if args.command == 'sentry':
    sentry.createConfig(args.path, args.dsn)

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
    sys.exit(err)

elif args.command == 'tap-installer':
    print('Building tap installer')
    err = installer.build('1.1.1', '', 'Setup/ProtonVPNTap.aip')
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
    print('Updating guest hole servers json')
    with open('.\Setup\GuestHoleServers.json', 'w') as file:
        file.write(os.environ['GH_SERVERS'])