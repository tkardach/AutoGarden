import socket, ssl
import json

# SET VARIABLES

packet, reply = "<packet>SOME_DATA</packet>", ""
HOST, PORT = '10.0.0.139', 4443
CERTFILE = '/etc/ssl/certs/10.0.0.139.crt'
CERTKEY  = '/etc/ssl/private/10.0.0.139.key'


# HANDLER METHODS

def deal_generic(jsonObj):
   print "I did not hit her, I did nawt!"
   return "Oh Hi Mark<EOF>"

def deal_add_device():
   return 0

def deal_remove_device():
   return 0

methods = {
"Generic" : deal_generic,
"Add" : deal_add_device,
"Remove" : deal_remove_device
}


# CLIENT HANDLER

def deal_with_client(connstream):
   try:
      jsonObj = json.loads(connstream.read())

      if jsonObj["Command"] in methods:
         print jsonObj["Command"] + " Command"

         response = methods[jsonObj["Command"]](jsonObj)
         connstream.write(response)
      else:
         print "NOT FOUND"
         return "Not Found in array"
   except:
      return 'An Error Occurred in deal_with_client'
   finally:
      connstream.close()
# CREATE SOCKET

bindsocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
bindsocket.settimeout(10)
bindsocket.bind((HOST, PORT))
bindsocket.listen(5)

# LISTEN
while True:
   try:
      newsocket, fromaddr = bindsocket.accept()
      connstream = ssl.wrap_socket(newsocket,
                                   server_side=True,
                                   certfile=CERTFILE,
                                   keyfile=CERTKEY,
                                   ssl_version=ssl.PROTOCOL_TLSv1)
      responseString = "Tough Shit"

      print "Connection Established..."
      deal_with_client(connstream)
      print "Done with client request..."
   except:
      pass
