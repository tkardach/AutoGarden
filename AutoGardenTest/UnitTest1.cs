using System;
using Xunit;
using Xunit.Abstractions;
using AutoGarden.HardwareCommunication;
using System.Threading.Tasks;

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
        public void TestRPiHTTPRequest()
        {
            var cmdStr = "TEST=Testing";
            var expectedString = "\nOK\n";

            var retValue = RPiCommLink.SendCommand(cmdStr);

            Assert.Equal(expectedString, retValue);
        }

        [Fact]
        public async Task TestRPiHTTPRequestAsync()
        {
            var cmdStr = "TEST=Testing";
            var expectedString = "\nOK\n";

            var retValue = await RPiCommLink.SendCommandAsync(cmdStr);

            Assert.Equal(expectedString, retValue);
        }

        [Fact]
        public void TestSendJSONThroughSSL()
        {
            var expectedString = "\nOK\n";

            var climateDevice = new BLEDevice("ClimateServiceBLE1");
            climateDevice.AddService(new ClimateService());

            var cmdStr = climateDevice.CreateJSONRequest();

            var retValue = RPiCommLink.GenericCommand(cmdStr);

            Assert.Equal(expectedString, retValue);
        }

        [Fact]
        public void TestScanCommandSSL()
        {
            var retValue = RPiCommLink.ScanCommand();

            output.WriteLine(retValue[0].DeviceName);

            Assert.NotEmpty(retValue);
        }

        [Fact]
        public void TestAddCommandSSL()
        {
            var retValue = RPiCommLink.AddDeviceCommand("30:ae:a4:7b:01:6a");

            output.WriteLine(retValue);

            Assert.NotEmpty(retValue);
        }

        [Fact]
        public void TestRemoveCommandSSL()
        {
            var retValue = RPiCommLink.RemoveDeviceCommand("30:ae:a4:7b:01:6a");

            output.WriteLine(retValue);

            Assert.NotEmpty(retValue);
        }

        [Fact]
        public void TestAddClimateService()
        {
            var climateDevice = new BLEDevice("ClimateServiceBLE1");
            Assert.True(climateDevice.AddService(new ClimateService()));
        }

        [Fact]
        public void TestSSLHTTPRequest()
        {
            var retValue = RPiCommLink.RunClient("Oh hi mark");

            output.WriteLine(retValue);
            Assert.Equal("OK", retValue);
        }
    }
}
