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

# Command tokens
READTOKEN = '<r>'
WRITETOKEN1 = '<w>'
WRITETOKEN2 = '</w>'
WRITEERROR  = '<ERR>'

devices = {}
failedDevices = []

############# LOCAL METHODS #############

# Parses a string with the WRITETOKEN1 and WRITETOKEN2
# strings within them
def get_write(str):
   try:
      return  str.split(WRITETOKEN1)[1].split(WRITETOKEN2)[0]
   except Exception as e:
      print str(e)
      return WRITEERROR

############# HANDLER METHODS #############

# Deals with a generic command targetted at a
# specific BLE device. Returns a JSON Object
# representing the device, containing a dict
# of all services and their characteristics
# with the server return values.
def deal_generic(jsonObj):
   jsonObj = jsonObj["BLEDevice"]
   # Connect to the BLE device
   dev = bcon.connect_ble(jsonObj["MAC"])
   if dev is None:
      return "Failed to connect to ble device"
   jDevice = {}
   jService = {}
   # Itterate services
   for service in jsonObj["Services"]:
      jChar = {}
      serviceUUID = service["UUID"]
      bleService = bcon.get_service(dev, serviceUUID)
      # Itterate characteristics; send commands
      for characteristic in service["Characteristics"]:
         uuid = characteristic["UUID"]
         cmd = characteristic["CMD"]
         # Write command
         if WRITETOKEN1 in cmd:
            newCmd = get_write(cmd)
            if cmd is WRITEERROR:
               jChar[uuid] = WRITEERROR
               continue
            bcon.write_characteristic(bleService, uuid, newCmd)
            jChar[uuid] = bcon.read_characteristic(bleSerivce, uuid)
         # Read command
         if READTOKEN in cmd:
            readVal = bcon.read_characteristic(bleService, uuid)
            if readVal is not None:
               jChar[uuid] = readVal
      jService[serviceUUID] = jChar
   jDevice[jsonObj["MAC"]] = jService
   # Disconnect from device
   dev.disconnect()
   return json.dumps(jDevice)

# Adds a device's MAC address to storage
def deal_add_device(jsonObj):
   print "Adding Device..."
   if save_ble.save_ble_mac(jsonObj["MAC"]):
      return "Device added successfully!"
   else:
      return "Device not added"

# Removes a device's MAC address from storage
def deal_remove_device(jsonObj):
   print "Removing Device..."
   if save_ble.remove_ble_mac(jsonObj["MAC"]):
      return jsonObj["MAC"] + " removed"
   return "Failed to remove"

# Scan for nearby BLE devices and return the
# results as a JSON object
def deal_scan_devices(jsonObj):
   retJson = scan_ble.get_scan_devices_json(5)
   return retJson

# Test handler
def deal_test(jsonObj):
   if jsonObj["Test"] in tests:
      print jsonObj["Test"] + " Test"

      response = tests[jsonObj["Test"]]()
      return response
   else:
      return "Test not recognized"

# Collection of test methods
tests = {
"string_load" : test.test_string_load
}

# Collection of handler methods
methods = {
"Generic" : deal_generic,
"Add" : deal_add_device,
"Remove" : deal_remove_device,
"Scan" : deal_scan_devices,
"Test" : deal_test
}


############ CLIENT HANDLER ##############

# General method for dealing with client requests
def deal_with_client(connstream):
   try:
      clientReq = connstream.recv(4096)
      jsonObj = json.loads(clientReq)

      # Check command type and call function
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

############# SETUP SERVER #############

macList = save_ble.get_device_macs()

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
   except Exception as e:
      print str(e)
   finally:
      if newsocket is not None:
         newsocket.close()
