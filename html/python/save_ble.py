import pickle
import os
from bluepy import btle
import pickle_list as pkl

OUTPUT_FILE = '/home/pi/Documents/AutoGarden/html/bin/saved_ble.pkl'

def get_device_macs():
   return pkl.pickle_get(OUTPUT_FILE)

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
   return pkl.pickle_remove(OUTPUT_FILE, mac)

def save_ble_mac(mac):
   return pkl.pickle_add(OUTPUT_FILE, mac)
