﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using AutoGarden.HardwareCommunication;
using AutoGarden.Bluetooth;

namespace AutoGardenNUnit
{
    [TestFixture()]
    public class Test
    {

        [Test()]
        public void TestRemoveEOF()
        {
            var testString = "{\"Test\" : \"JSONObject\"}<EOF>";

            var resultString = testString.Replace("<EOF>", "");

            var expectedString = "{\"Test\" : \"JSONObject\"}";

            Assert.AreEqual(resultString, expectedString);
        }

        [Test()]
        public void TestSendJSONThroughSSL()
        {
            var expectedString = "\nOK\n";

            var climateDevice = new BLEDevice("ClimateServiceBLE1");
            climateDevice.AddService(new ClimateService());

            var cmdStr = climateDevice.CreateJSONRequest();

            var retValue = RPiCommLink.GenericCommand(cmdStr);

            Assert.Equals(expectedString, retValue);
        }

        [Test()]
        public void TestScanCommandSSL()
        {
            List<BLEDevice> retValue = RPiCommLink.ScanCommand();

            if (retValue != null) {
                foreach (var dev in retValue)
                {
                    var str = string.Format("Name : {0}\nMAC : {1}\n",
                                            dev.DeviceName, dev.MAC);
                    Console.WriteLine(str);
                }
            }

            Assert.IsNotNull(retValue);
        }

        [Test()]
        public void TestAddCommandSSL()
        {
            var retValue = RPiCommLink.AddDeviceCommand("30:ae:a4:7b:01:6a");

            Assert.IsNotEmpty(retValue);
        }

        [Test()]
        public void TestRemoveCommandSSL()
        {
            var retValue = RPiCommLink.RemoveDeviceCommand("30:ae:a4:7b:01:6a");

            Assert.IsNotEmpty(retValue);
        }

        [Test()]
        public void TestAddClimateService()
        {
            var climateDevice = new BLEDevice("ClimateServiceBLE1");
            Assert.True(climateDevice.AddService(new ClimateService()));
        }

    }
}
