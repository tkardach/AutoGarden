#!/usr/bin/python

import cgi
import cgitb
from bluepy import btle

cgitb.enable()

print 'Content-type: text/html\n\n'
arguments = cgi.FieldStorage()

climate_hw = None

try:
 climate_hw = btle.Peripheral("30:ae:a4:7b:01:6a")
except:
 print "Failed to connect to bluetooth device"

if climate_hw is not None:
 hw = arguments.keys()[0]
 val = arguments[hw].value

 ret = "no ret"
 if (hw == "HW"):
  if (val == "Climate"):
   ret = "Temp F : "
   ret += climate_hw.getServices()[2].getCharacteristics()[0].read()
   ret += "    Temp C : "
   ret += climate_hw.getServices()[2].getCharacteristics()[1].read()

 print ret

