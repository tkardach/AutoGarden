from Crypto.Cipher import AES
from Crypto.Cipher import PKCS1_OAEP
from Crypto.PublicKey import RSA

PUBLICKEY = '/etc/ssl/certs/10.0.0.139.pem'
PRIVATEKEY  = '/etc/ssl/private/10.0.0.139.key'

def encrypt_string(string):
    rsaKey = RSA.importKey(open(PUBLICKEY, 'r'))
    pkcs1CipherTmp = PKCS1_OAEP.new(rsaKey)
    encryptedString = pkcs1CipherTmp.encrypt(string)
    return encryptedString

def decrypt_string(encryptedString):
    rsaKey = RSA.importKey(open(PRIVATEKEY, 'r'))
    pkcs1CipherTmp = PKCS1_OAEP.new(rsaKey)
    decryptedString = pkcs1CipherTmp.decrypt(encryptedString)
    return decryptedString

