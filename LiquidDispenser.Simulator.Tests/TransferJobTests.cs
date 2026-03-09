using LiquidDispenser.Simulator.Models.Labware;

namespace LiquidDispenser.Simulator.Tests;

[TestClass]
public class TransferJobTests
{
    [TestMethod]
    public async Task TransferJob_UpdatesPlateAndChipVolumes_CorrectlyWithPhysicalAlignment()
    {
        // Arrange
        var core = new Instrument();
        // A standard 384 plate has a 4.5mm pitch. The 8 tip head has a 9.0mm pitch.
        // Therefore, 8 tips will map to every *other* row! (e.g. index 0, 2, 4, 6, 8, 10, 12, 14)
        var plate = new Plate(PlateFormat.Plate384);
        var chip = new Chip(ChipFormat.Chip64x64);

        var initialPlateVolume = plate.Wells[0, 0].Volume;
        var transferVolume = 10.0;

        var job = new TransferJob(plate, 0, 0, chip, 0, 0, transferVolume);

        // Act
        await core.InitializeAsync();
        await core.ExecuteTransferAsync(job);

        // Assert
        // Tip 0 aligns with index 0 (0 * 9.0 = 0 -> 0 / 4.5 = 0)
        Assert.AreEqual(initialPlateVolume - transferVolume, plate.Wells[0, 0].Volume);

        // Tip 1 aligns with index 2 on the plate! (1 * 9.0 = 9.0 -> 9.0 / 4.5 = 2)
        Assert.AreEqual(initialPlateVolume - transferVolume, plate.Wells[2, 0].Volume);

        // Ensure index 1 was skipped physically 
        Assert.AreEqual(initialPlateVolume, plate.Wells[1, 0].Volume);

        // Asserts destination chip alignment. Chip has 1.0mm pitch. 
        // Tip 1 aligns with index 9 on the chip! (1 * 9.0 = 9.0 -> 9.0 / 1.0 = 9)
        Assert.AreEqual(transferVolume, chip.Wells[0, 0].Volume);
        Assert.AreEqual(plate.Wells[0, 0].LiquidType, chip.Wells[0, 0].LiquidType);

        Assert.AreEqual(transferVolume, chip.Wells[9, 0].Volume);
        Assert.AreEqual(plate.Wells[2, 0].LiquidType, chip.Wells[9, 0].LiquidType); // Maps from source index 2
    }
}
