import socket, ssl
import json
import sys

sys.path.append('./python')

import scan_ble
import save_ble
import connect_ble

# SET VARIABLES

EOF = "<EOF>"
HOST, PORT = '10.0.0.139', 4443
CERTFILE = '/etc/ssl/certs/10.0.0.139.crt'
CERTKEY  = '/etc/ssl/private/10.0.0.139.key'

devices = {}
failedDevices = []

# LOCAL METHODS

def connect_ble_devices(devList):
   try:
      completeSuccess = True

      # if devlist is empty return true
      if not devList:
         return True

      # for each mac in list, try to connect
      for mac in devList:
         if mac in devices:
            continue
         dev = connect_ble.connect_ble(mac)

         # if device connected, add to devices
         if dev is not None:
            devices[mac] = dev

         # else add to failed devices
         elif mac not in failedDevices:
            failedDevices.append(mac)
            completeSuccess = False

      return completeSuccess
   except Exception as e:
      print "connect_ble_devices failed: " + str(e)
      return False

def retry_ble_connect():
   return connect_ble_devices(failedDevices)

# HANDLER METHODS

def deal_generic(jsonObj):
   print "I did not hit her, I did nawt!"
   return "Oh Hi Mark"

def deal_add_device(jsonObj):
   print "Adding Device..."
   if save_ble.save_ble_mac(jsonObj["MAC"]):
      return "Device added successfully!"
   else:
      return "Device not added"

def deal_remove_device(jsonObj):
   print "Removing Device..."
   if save_ble.remove_ble_mac(jsonObj["MAC"]):
      return jsonObj["MAC"] + " removed"
   return "Failed to remove"

def deal_scan_devices(jsonObj):
   retJson = scan_ble.get_scan_devices_json(5)
   return retJson

methods = {
"Generic" : deal_generic,
"Add" : deal_add_device,
"Remove" : deal_remove_device,
"Scan" : deal_scan_devices
}


# CLIENT HANDLER

def deal_with_client(connstream):
   try:
      jsonObj = json.loads(connstream.read())

      if jsonObj["Command"] in methods:
         print jsonObj["Command"] + " Command"

         response = methods[jsonObj["Command"]](jsonObj)
         if jsonObj["Command"] == "Scan":
            print "Returning scan results"
         else:
            print response

         connstream.write(response + EOF)
      else:
         return "Not Found in array"
   except:
      return 'An Error Occurred in deal_with_client'
   finally:
      connstream.shutdown(socket.SHUT_RDWR)
      connstream.close()

# SETUP SERVER

macList = save_ble.get_device_macs()


#if macList:
#   print "Connecting to BLE devices:"
#   if connect_ble_devices(macList):
#      print "\nSuccessfully connected to BLE Devices"
#   else:
#      print "\nRetrying failed BLE connections"
#      retry_ble_connect()

# CREATE SOCKET

bindsocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#bindsocket.settimeout(10)
bindsocket.bind((HOST, PORT))
bindsocket.listen(1)

# LISTEN

print "\nStarting to listen for client...\n"

while True:
   try:
      newsocket, fromaddr = bindsocket.accept()

      connstream = ssl.wrap_socket(newsocket,
                                   server_side=True,
                                   certfile=CERTFILE,
                                   keyfile=CERTKEY,
                                   ssl_version=ssl.PROTOCOL_TLSv1)
      responseString = "Tough Shit"

      print "\nConnection Established..."
      deal_with_client(connstream)
      print "Done with client request...\n"
   except ssl.SSLError as e:
      print str(e)
   except Exception as e:
      print str(e)
   finally:
      if newsocket is not None:
         newsocket.close()
