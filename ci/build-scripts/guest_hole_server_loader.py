import os
import random
import requests

def load():
    # Constants

    num_guest_hole_servers: int = 10
    secure_core_flag: int = 1

    # Main

    print('Started updating guest hole servers json')

    servers_str: str = os.getenv('GUEST_HOLE_LOGICALS')
    servers_str = servers_str.strip()
    servers_str = servers_str.strip(',')

    with open(r'.\Setup\GuestHoleServers.json', 'w') as file:
        file.write(servers_str)

    print('Finished updating guest hole servers json')