using LiquidDispenser.Simulator.Models.Hardware;

namespace LiquidDispenser.Simulator.Tests;

[TestClass]
public class EightTipHeadTests
{
    [TestMethod]
    public async Task Aspirate_ValidVolume_IncreasesAllTipVolumes()
    {
        // Arrange
        var head = new EightTipHead();
        await head.PickupTipsAsync();

        // Act
        await head.AspirateAsync(15.5);

        // Assert
        Assert.AreEqual(8, head.TipCount);
        for (int i = 0; i < head.TipCount; i++)
        {
            Assert.AreEqual(15.5, head.AspiratedVolumes[i]);
        }
    }

    [TestMethod]
    public async Task Aspirate_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var head = new EightTipHead();
        using var cts = new CancellationTokenSource();
        await head.PickupTipsAsync();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(
            () => head.AspirateAsync(10, cts.Token));
    }

    [TestMethod]
    public async Task Aspirate_WithNoTips_ThrowsInvalidOperationException()
    {
        // Arrange
        var head = new EightTipHead();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => head.AspirateAsync(10));
    }

    [TestMethod]
    public async Task Dispense_MoreThanAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var head = new EightTipHead();
        await head.PickupTipsAsync();
        await head.AspirateAsync(10);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => head.DispenseAsync(15));
    }

    [TestMethod]
    public async Task Dispense_ValidVolume_DecreasesAllTipVolumes()
    {
        // Arrange
        var head = new EightTipHead();
        await head.PickupTipsAsync();
        await head.AspirateAsync(50);

        // Act
        await head.DispenseAsync(10);

        // Assert
        for (int i = 0; i < head.TipCount; i++)
        {
            Assert.AreEqual(40, head.AspiratedVolumes[i]);
        }
    }

    [TestMethod]
    public async Task DropTips_ClearsVolumes()
    {
        // Arrange
        var head = new EightTipHead();
        await head.PickupTipsAsync();
        await head.AspirateAsync(20);

        // Act
        await head.DropTipsAsync();

        // Assert
        Assert.IsFalse(head.HasTips);
        for (int i = 0; i < head.TipCount; i++)
        {
            Assert.AreEqual(0, head.AspiratedVolumes[i]);
        }
    }
}
