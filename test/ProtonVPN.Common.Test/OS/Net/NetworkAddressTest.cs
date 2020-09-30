using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.OS.Net;

namespace ProtonVPN.Common.Test.OS.Net
{
    [TestClass]
    public class NetworkAddressTest
    {
        [TestMethod]
        [DataRow("127.0.0.1")]
        [DataRow("8.8.8.8")]
        [DataRow("8.8.8.0")]
        [DataRow("8.8.8.255")]
        [DataRow("8.8.8.0/24")]
        public void NetworkAddressShouldBeValid(string address)
        {
            new NetworkAddress(address).Valid().Should().BeTrue();
        }

        [TestMethod]
        [DataRow("a.a.a.a")]
        [DataRow("1.1.1.256")]
        [DataRow("127.0.0.0/33")]
        [DataRow("127.0.0.0/a")]
        public void NetworkAddressShouldNotBeValid(string address)
        {
            new NetworkAddress(address).Valid().Should().BeFalse();
        }

        [TestMethod]
        [DataRow("127.0.0.1", "127.0.0.1")]
        [DataRow("127.0.0.0/24", "127.0.0.0")]
        [DataRow("127.0.0.0/32", "127.0.0.0")]
        [DataRow("127.0.0.1/32", "127.0.0.1")]
        [DataRow("127.5.5.5/16", "127.5.0.0")]
        [DataRow("127.0.0.5/8", "127.0.0.0")]
        [DataRow("127.65.55.1/10", "127.64.0.0")]
        public void ItShouldReturnCorrectIp(string address, string expectedAddress)
        {
            new NetworkAddress(address).Ip.Should().Be(expectedAddress);
        }

        [TestMethod]
        [DataRow("127.0.0.1", "255.255.255.255")]
        [DataRow("127.0.0.0/8", "255.0.0.0")]
        [DataRow("127.0.0.0/16", "255.255.0.0")]
        [DataRow("127.0.0.0/24", "255.255.255.0")]
        [DataRow("127.0.0.0/32", "255.255.255.255")]
        public void ItShouldReturnCorrectMask(string address, string expectedMask)
        {
            new NetworkAddress(address).Mask.Should().Be(expectedMask);
        }
    }
}
