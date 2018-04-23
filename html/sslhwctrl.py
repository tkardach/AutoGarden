import socket, ssl

# SET VARIABLES

packet, reply = "<packet>SOME_DATA</packet>", ""
HOST, PORT = '10.0.0.139', 4443
CERTFILE = '/etc/ssl/certs/10.0.0.139.crt'
CERTKEY  = '/etc/ssl/private/10.0.0.139.key'

# CLIENT HANDLER

def deal_with_client(connstream):
   try:
      data = connstream.read()
      # null data means the client is finished with us
      while data:
         #if not do_something(connstream, data):
            # we'll assume do_something returns False
            # when we're finished with client
           #break
         data = connstream.read()
         print data
      # finished with client
      connstream.write("Hello from the server!<EOF>")
      connstream.close()
   except:
      print 'An Error Occurred in deal_with_client'

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
      print connstream.read()
      connstream.write("Haaaa go fuck yourself<EOF>")
      connstream.close()
      #deal_with_client(connstream)
   except:
      pass

