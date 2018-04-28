from bluepy import btle


def connect_ble(mac):
   try:
      device = btle.Peripheral(mac)
      return device
   except:
      print "Failed to connect to " + mac
      return None

