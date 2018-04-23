#!/usr/bin/python

import cgi
import cgitb
import settings
import json
from bluepy import btle

cgitb.enable()

print 'Content-type: text/html\n\n'
arguments = cgi.FieldStorage()

# init local ble device
dev = None
response = {}

def parse_variables(args):
 # Check if it is an HTTP request test
 if args.has_key("TEST"):
  return "OK"
 
 # Check to make sure the HTTP Request is from a trusted user
 if not args.has_key("ID"):
  return "No ID Specified"
 else:
  id = args["ID"].value
  if id != "Tombo":
   return "Incorrect ID" 
 
 # Get the BLE device information needed to connect
 if args.has_key("HW"):
  hwName = args["HW"].value
  hwMac = settings.get_device_mac(hwName)
  
  # Connect to the BLE device 
  if hwMac is None:
   return "Unrecognized HW Name"
  else:
   try:
    dev = btle.Peripheral(hwMac)
   except:
    return "Failed to connect to BLE device"
   if not args.has_key("CMD") or not args.has_key("CMD0"):
    return "Successfully connected to BLE device"
     
   # Get Service UUID to parse the appropriate commands  
   if args.has_key("SUUID"):
    serviceName = settings.get_service_name(args["SUUID"].value)
    if service is None:
     return "Unrecognized service UUID"
    else:
     serviceObj = dev.getServiceByUUID(args["SUUID"].value) 
     services[serviceName](serviceObj)
   else:
    return "No service matching UUID"
 else:
  return "No Device Specified"


def climate_commands(service):
 if not arguments.has_key("CMD"):
  for x in range(0,len(arguments)):
   cmdStr = "CMD" + str(x)
   if arguments.has_key(cmdStr):
    charUUID = arguments[cmdStr].value
    try:
     character = service.getCharacteristics(uuid=charUUID)[0] 
     response[charUUID] = character.read() 
    except:
     response[charUUID] = "Error getting/reading character"
   else:
    dev.disconnect()
    return json.dumps(response, ensure_ascii=False) 
 else:
  charUUID = arguments["CMD"].value
  try:
   character = service.getCharacteristics(uuid=charUUID)[0]
   response[charUUID] = character.read()
  except:
   response[charUUID] = "Error getting/reading character"


services = { 
 "CLIMATE_SERVICE" : climate_commands,
}


print parse_variables(arguments)

