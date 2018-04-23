from bluepy import btle


class BLEDevice:
 """A class representing a Bluetooth Low Energy Device"""
 def __init__(self, mac_adr):
  self.dev = btle.Peripheral()
  try:
   self.dev.connect(mac_addr)
  except:
   pass

 def connect(self,mac_adr):
  try:
   self.dev.connect(mac_adr)
  except:
   pass

 def disconnect(self):
  try:
   self.dev.disconnect()
  except:
   pass

 def __enter__(self):
  return self

 def __exit__(self, type, value, traceback):
  self.dev.disconnect()

 def getService(self,index):
  try:
   return self.dev.getServices()[index]
  except:
   return None

 def getCharacteristic(self, s_index, c_index):
  try:
   return self.dev.getServices()[s_index].getCharacteristics()[c_index]
  except:
   return None

 def getPeripheral(self):
  return self.dev

 def __delete__(self):
  self.dev.disconnect() 
