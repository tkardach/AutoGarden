import ble_connect.py as ble
import settings
from bluepy import btle

dev = None

def ParseClimateRequest(args):
 if args.has_key("HW"):
  hwMac = settings.get_device_mac(args["HW"].value)
  
  if not args.has_key("CMD"):

   
