using System;
using Xunit;
using Xunit.Abstractions;
using AutoGarden.HardwareCommunication;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace AutoGardenTest
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestClimateDeviceHTTPCommandCreation()
        {
            var climateDevice = new ClimateDevice("TestDevice");
            var expectedString = 
                string.Format("HW=TestDevice&SUUID={0}&CMD0={1}&CMD1={2}&CMD2={3}",
                              climateDevice.ServiceUUID, ClimateDevice.TEMPERATURE_F_UUID,
                              ClimateDevice.TEMPERATURE_C_UUID, ClimateDevice.HUMIDITY_UUID);

            Assert.Equal(expectedString, climateDevice.CommandString);
        }

        [Fact]
        public void TestRPiHTTPRequest()
        {
            var cmdStr = "TEST=Testing";
            var expectedString = "\nOK\n";

            var retValue = RPiCommLink.GetInstance().SendCommand(cmdStr);

            Assert.Equal(expectedString, retValue);
        }

        [Fact]
        public async void TestRPiHTTPRequestAsync()
        {
            var cmdStr = "TEST=Testing";
            var expectedString = "\nOK\n";

            var retValue = await RPiCommLink.GetInstance().SendCommandAsync(cmdStr);

            Assert.Equal(expectedString, retValue);
        }

        [Fact]
        public void TestClimateDeviceConnection()
        {
            var climateDevice = new ClimateDevice("ClimateServiceBLE1");
            var cmdStr = "HW=ClimateServiceBLE1";
            var expectedResult = "\nSuccessfully connected to BLE device\n";

            var retValue = RPiCommLink.GetInstance().SendCommand(cmdStr);


            output.WriteLine(retValue);

            Assert.Equal(expectedResult, retValue);
        }
    }
}
