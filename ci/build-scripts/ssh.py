import os

def prepare(key):
    path = os.path.join(os.getenv('userprofile'), '.ssh')
    os.makedirs(path, exist_ok=True)
    f = open(os.path.join(path, 'id_rsa'), 'w', newline='\n')
    f.write(key)
    f.write('\n')
    f.close()