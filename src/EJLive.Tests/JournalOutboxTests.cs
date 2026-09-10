using System.Text;
using EJLive.Core.Engine;
using Xunit;

namespace EJLive.Tests;

public sealed class JournalOutboxTests
{
    [Fact]
    public void Enqueue_PersistsPayloadAndExposesItemSnapshot()
    {
        var outbox = new JournalOutbox();
        var data = Encoding.UTF8.GetBytes("Test content for journal");
        var item = outbox.Enqueue("ATM-TEST", "journal.log", data, 0, "sha256-test");

        try
        {
            var stored = Assert.Single(outbox.Snapshot, candidate => candidate.ItemId == item.ItemId);
            Assert.Equal("ATM-TEST", stored.ATM_ID);
            Assert.Equal(data.Length, stored.SizeBytes);
            Assert.True(File.Exists(stored.PayloadPath));
        }
        finally
        {
            outbox.MarkCompleted(item.ItemId, "test-cleanup");
        }
    }

    [Fact]
    public void MarkCompleted_RemovesItemAndPayload()
    {
        var outbox = new JournalOutbox();
        var item = outbox.Enqueue("ATM-TEST", "complete.log", new byte[] { 1, 2, 3 }, 0, "sha256-test");
        var payloadPath = item.PayloadPath;

        outbox.MarkCompleted(item.ItemId, "ack-ok");

        Assert.DoesNotContain(outbox.Snapshot, candidate => candidate.ItemId == item.ItemId);
        Assert.False(File.Exists(payloadPath));
    }
}
