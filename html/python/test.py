import sys
import unicodedata

sys.path.append('./python')

import scan_ble
import save_ble
import connect_ble

ISO = "ISO-8859-1"
ASCII = "ascii"
UTF8 = "utf-8"
UTF16 = "utf-16"

langList = [ISO, ASCII, UTF8, UTF16]


list = save_ble.get_device_macs()

print list

dev = connect_ble.connect_ble(list[0])
if dev is not None:
   print dev.getServices()
else:
   print "Something failed"


#for mac in list:
#   for code in langList:
#      print "\nAttempting " + code
#      dev = connect_ble.connect_ble(mac.encode(code))
#      if dev is not None:
#         print dev.getServices()
#      else:
#         print "Something failed"
