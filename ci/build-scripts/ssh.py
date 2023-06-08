import os

def prepare(key):
    path = '{userprofile}\.ssh'.format(userprofile=os.getenv('userprofile'))
    os.makedirs(path, exist_ok=True)
    f = open(path + '\id_rsa', 'w', newline='\n')
    f.write(key)
    f.write('\n')
    f.close()