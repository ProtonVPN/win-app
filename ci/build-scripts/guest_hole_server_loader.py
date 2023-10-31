import random
import requests

def load():
    # Constants

    server_list_api_url: str = 'https://api.protonvpn.ch/vpn/logicals?SignServer=Server.EntryIP,Server.Label'
    num_guest_hole_servers: int = 10
    secure_core_flag: int = 1

    # Main

    print('Started updating guest hole servers json')

    proxies = {
        'http': 'http://proxy.plabs.ch:3128',
        'https': 'http://proxy.plabs.ch:3128',
    }

    response = requests.get(server_list_api_url, proxies=proxies)
    original_json = response.json()

    logical_servers = original_json["LogicalServers"]
    print(''.join(['Number of logical servers: ', str(len(logical_servers))]))

    non_secure_core_logical_servers = [logical_server for logical_server in logical_servers
                                       if (logical_server['Features'] & secure_core_flag) == 0]
    print(''.join(['Number of non Secure Core logical servers: ', str(len(non_secure_core_logical_servers))]))

    random.shuffle(non_secure_core_logical_servers)

    servers_str: str = ""
    unique_entries: array = []

    for logical_server in non_secure_core_logical_servers:
        server = logical_server['Servers'][0]
        if server['EntryIP'] in unique_entries:
            continue

        server_json_object = "".join(['{"host":"', server['Domain'], '","ip":"', server['EntryIP'], '","signature":"', server['Signature'], '","label":"', server['Label'], '"}'])
        servers_str = ",".join([servers_str, server_json_object])
        unique_entries.append(server['EntryIP'])

        if len(unique_entries) >= num_guest_hole_servers:
            break

    servers_str = servers_str.lstrip(',')

    with open('.\Setup\GuestHoleServers.json', 'w') as file:
        file.write("".join(['[', servers_str, ']']))

    print('Finished updating guest hole servers json')