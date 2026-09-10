using Xunit;
using EJLive.Core;

namespace EJLive.Tests
{
    public class ProtocolTests
    {
        [Fact]
        public void BuildMessage_WithParts_JoinsWithPipe()
        {
            var result = Protocol.BuildMessage(Protocol.HEARTBEAT, "ATM001", "RUNNING", "5");
            Assert.Equal("HEARTBEAT|ATM001|RUNNING|5", result);
        }

        [Fact]
        public void ParseMessage_EmptyInput_ReturnsEmpty()
        {
            var parts = Protocol.ParseMessage("");
            Assert.Empty(parts);
        }

        [Fact]
        public void ParseMessage_ValidInput_ReturnsParts()
        {
            var parts = Protocol.ParseMessage("EJDATA|journal.log|1024");
            Assert.Equal(3, parts.Length);
        }
    }
}
