import pickle
import os
from bluepy import btle

OUTPUT_FILE = '/home/pi/Documents/AutoGarden/html/python/saved_ble.pkl'

def get_device_macs():
   try:
      with open(OUTPUT_FILE, 'rb') as f:
         return pickle.load(f)
   except:
      return None

def check_uuid(uuid):
   try:
      btle.UUID(uuid)
      return True
   except:
      return False

def device_services_dict(dev):
   try:
      responseDictr
      services = {}
      serviceCount = 0
      for service in dev.getServices():
         if check_uuid(service.uuid.getCommonName()):
            uuidKey = "UUID" + serviceCount
            services[uuidKey] = service.uuid.getCommonName()
            serviceCount += 1
   except:
      return None

def remove_ble_mac(mac):
   try:
      devices = []
      if not os.stat(OUTPUT_FILE).st_size == 0:
         with open(OUTPUT_FILE, 'rb') as f:
            devices = pickle.load(f)
      with open(OUTPUT_FILE, 'wb') as f:
         if mac not in devices:
            print "MAC not in list"
            return False
         else:
            devices.remove(mac)
            pickle.dump(devices, f)
            print "MAC removed from list"
      return True
   except:
      print "remove_ble_mac failed"

def save_ble_mac(mac):
   try:
      devices = []
      if not os.stat(OUTPUT_FILE).st_size == 0:
         with open(OUTPUT_FILE, 'rb') as f:
            devices = pickle.load(f)
      with open(OUTPUT_FILE, 'wb') as f:
         if mac not in devices:
            devices.append(mac)
            pickle.dump(devices, f)
         else:
            print "MAC already exists"
      return True
   except IOError:
      print "Failed reading/writing to file"
      return False
   except pickle.PickleError:
      print "Failure during pickling"
      return False
   except:
      print "save_ble_mac failed to add " + mac
      return False


