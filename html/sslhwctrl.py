import socket, ssl
import json
import sys
import datetime

sys.path.append('./python')

import scan_ble
import save_ble
import connect_ble as bcon
import security
import test_protocols as test
import pickle_list as pkl

# SET VARIABLES

EOF = "<EOF>"
HOST, PORT = '10.0.0.139', 4443
CERTFILE = '/etc/ssl/certs/10.0.0.139.crt'
CERTKEY  = '/etc/ssl/private/10.0.0.139.key'
PWFILE   = '/var/www/html/bin/pw.txt'

READTOKEN = '<r>'
WRITETOKEN1 = '<w>'
WRITETOKEN2 = '</w>'
WRITEERROR  = '<ERR>'

devices = {}
failedDevices = []

# LOCAL METHODS
def get_write(str):
   try:
      return  str.split(WRITETOKEN1)[1].split(WRITETOKEN2)[0]
   except Exception as e:
      print str(e)
      return WRITEERROR

# HANDLER METHODS

def deal_generic(jsonObj):
   jsonObj = jsonObj["BLEDevice"]
   dev = bcon.connect_ble(jsonObj["MAC"])
   if dev is None:
      return "Failed to connect to ble device"
   retString = ""
   for service in jsonObj["Services"]:
      serviceUUID = service["UUID"]
      bleService = bcon.get_service(dev, serviceUUID)
      for characteristic in service["Characteristics"]:
         uuid = characteristic["UUID"]
         cmd = characteristic["CMD"]
         if WRITETOKEN1 in cmd:
            newCmd = get_write(cmd)
            if cmd is WRITEERROR:
               retString += "Failed to identify " + str(cmd) + "\n"
               continue
            bcon.write_characteristic(bleService, uuid, newCmd)
            retString += str(newCmd) + " written to device\n"
         if READTOKEN in cmd:
            readVal = bcon.read_characteristic(bleService, uuid)
            if readVal is not None:
               retString += str(uuid) + " Read\n"
   dev.disconnect()
   return retString

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

def deal_test(jsonObj):
   if jsonObj["Test"] in tests:
      print jsonObj["Test"] + " Test"

      response = tests[jsonObj["Test"]]()
      return response
   else:
      return "Test not recognized"

tests = {
"string_load" : test.test_string_load
}

methods = {
"Generic" : deal_generic,
"Add" : deal_add_device,
"Remove" : deal_remove_device,
"Scan" : deal_scan_devices,
"Test" : deal_test
}


# CLIENT HANDLER

def deal_with_client(connstream):
   try:
      clientReq = connstream.recv(4096)
      jsonObj = json.loads(clientReq)

      if jsonObj["Command"] in methods:
         print jsonObj["Command"] + " Command"

         response = methods[jsonObj["Command"]](jsonObj)
         if jsonObj["Command"] == "Scan" or jsonObj["Command"] == "Test":
            print "Returning " + jsonObj["Command"] + " results"
         else:
            print response

         connstream.sendall(response + EOF)
      else:
         return "Not Found in array"
   except Exception as e:
      print "Error in deal_with_client"
      print str(e)
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

context = ssl.SSLContext(ssl.PROTOCOL_TLS)

bindsocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
bindsocket.bind((HOST, PORT))
bindsocket.listen(1)

# LISTEN

print "\nStarting to listen for client " + str(datetime.datetime.now().time()) + "...\n"

while True:
   try:
      newsocket, fromaddr = bindsocket.accept()

      connstream = ssl.wrap_socket(newsocket,
                                   server_side=True,
                                   certfile=CERTFILE,
                                   keyfile=CERTKEY,
                                   ssl_version=ssl.PROTOCOL_TLSv1)
      responseString = "Tough Shit"

      print "\nConnection Established " + str(datetime.datetime.now().time()) + "..."
      deal_with_client(connstream)
      print "Done with client request...\n"
   except ssl.SSLError as e:
      print str(e)
   except Exception as e:
      print str(e)
   finally:
      if newsocket is not None:
         newsocket.close()
