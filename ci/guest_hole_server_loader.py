import json
import random
from urllib.request import urlopen


def load():
    # Constants

    server_list_api_url: str = 'https://api.protonvpn.ch/vpn/logicals'
    num_guest_hole_servers: int = 10
    secure_core_flag: int = 1

    # Main

    print('Started updating guest hole servers json')

    original_json = json.loads(urlopen(server_list_api_url).read())

    logical_servers = original_json["LogicalServers"]
    print(''.join(['Number of logical servers: ', str(len(logical_servers))]))

    non_secure_core_logical_servers = [logical_server for logical_server in logical_servers
                                       if (logical_server['Features'] & secure_core_flag) == 0]
    print(''.join(['Number of non Secure Core logical servers: ', str(len(non_secure_core_logical_servers))]))

    random_logical_servers = random.sample(non_secure_core_logical_servers, num_guest_hole_servers)
    print(''.join(['Number of random logical servers: ', str(len(random_logical_servers))]))

    servers_str: str = ""

    for logical_server in random_logical_servers:
        server = logical_server['Servers'][0]
        server_json_object = "".join(['{"host":"', server['Domain'], '","ip":"', server['EntryIP'], '"}'])
        servers_str = ",".join([servers_str, server_json_object])

    servers_str = servers_str.lstrip(',')
    
    with open('.\Setup\GuestHoleServers.json', 'w') as file:
        file.write("".join(['[', servers_str, ']']))

    print('Finished updating guest hole servers json')
