import socket, ssl
import json
import sys

sys.path.append('./python')

import scan_ble
import save_ble

# SET VARIABLES

EOF = "<EOF>"
HOST, PORT = '10.0.0.139', 4443
CERTFILE = '/etc/ssl/certs/10.0.0.139.crt'
CERTKEY  = '/etc/ssl/private/10.0.0.139.key'

devices = {}
failedDevices = []

# LOCAL METHODS

def connect_ble(mac):
   try:
      if mac in devices:
         return devices[mac]
      device = btle.Peripheral(mac)
      return device
   except:
      if mac not in failedDevices:
         failedDevices.append(mac)
      print "Failed to connect to " + mac
      return None

def connect_ble_devices(devList):
   try:
      completeSuccess = True
      # if devlist is empty return true
      if not devList:
         return True

      # for each mac in list, try to connect
      for mac in devList:
         dev = connect_ble(mac)
         # if device connected, add to devices
         if dev is not None:
            devices[mac] = dev
         # else add to failed devices
         elif mac not in failedDevices:
            completeSuccess = False
      return completeSuccess
   except:
      print "connect_ble_devices failed"
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
      print "Saved Device"
      return "Device added successfully!"
   else:
      print "Device not saved"
      return "Device not added"

def deal_remove_device(jsonObj):
   print "Removing Device..."
   if save_ble.remove_ble_mac(jsonObj["MAC"]):
      print jsonObj["MAC"] + " removed"
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
         connstream.write(response + EOF)
      else:
         print "NOT FOUND"
         return "Not Found in array"
   except:
      return 'An Error Occurred in deal_with_client'
   finally:
      connstream.close()

# SETUP SERVER

macList = save_ble.get_device_macs()


if macList:
   if connect_ble_devices(macList):
      print "\nSuccessfully connected to BLE Devices"
   else:
      print "\nRetrying failed BLE connections"
      retry_ble_connect()

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

      print "\nConnection Established..."
      deal_with_client(connstream)
      print "Done with client request...\n"
   except:
      pass
