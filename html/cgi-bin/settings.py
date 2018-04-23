from bluepy import btle
import ble_connect as blec

mac_dict = {"ClimateServiceBLE1" : "30:ae:a4:7b:01:6a"}

service_uuids = {
 "4fafc201-1fb5-459e-8fcc-c5c9c331914b" : "CLIMATE_SERVICE"
}


def get_service_name(uuid):
 if service_uuids.has_key(uuid):
  return service_uuids[uuid]
 else:
  return None

def get_device_mac(deviceName):
 if mac_dict.has_key(deviceName):
  return mac_dict[deviceName]
 else:
  return None 

def climatectrl_mac():
 return "30:ae:a4:7b:01:6a"

