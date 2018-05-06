from bluepy import btle

def read_characteristic(service, charUUID):
   return service.getCharacteristics(charUUID)[0].read()

def write_characteristic(service, charUUID, cmd):
   return service.getCharacteristics(charUUID)[0].write(cmd)

def get_service(dev, serviceUUID):
   return dev.getServiceByUUID(serviceUUID)

def connect_ble(mac):
   try:
      device = btle.Peripheral(mac)
      return device
   except:
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

