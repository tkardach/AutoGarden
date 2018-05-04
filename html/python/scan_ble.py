from bluepy.btle import Scanner, DefaultDelegate
import json


class ScanDelegate(DefaultDelegate):
    def __init__(self):
        DefaultDelegate.__init__(self)

    def handleDiscovery(self, dev, isNewDev, isNewData):
        if isNewDev:
            print "Discovered device", dev.addr
        elif isNewData:
            print "Received new data from", dev.addr


def get_scan_devices_json(time):
    scanner = Scanner().withDelegate(ScanDelegate())
    devices = scanner.scan(time)
    jsonComplete = {}
    for dev in devices:
        jsonDict = {}
        jsonDict["MAC"] = dev.addr
        jsonDict["Header"] = "Device %s (%s), RSSI=%d dB" % (dev.addr, dev.addrType, dev.rssi)
        jsonContent = {}
        for (adtype, desc, value) in dev.getScanData():
            jsonContent[desc] = value
        jsonDict["Content"] = jsonContent
        jsonComplete[dev.addr] = jsonDict
    return json.dumps(jsonComplete)

