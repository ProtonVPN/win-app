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
        public void ItShouldReturnCorrectIp(string address, string expectedAddres)
        {
            new NetworkAddress(address).Ip.Should().Be(expectedAddres);
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
