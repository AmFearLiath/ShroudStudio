using System.Buffers.Binary;
using System.Text.Json;
using ShroudStudio.Core;
using ShroudStudio.Infrastructure;

namespace ShroudStudio.Tests;

public sealed class InventoryScannerTests
{
    [Fact]
    public void HasKscHeaderRecognizesOnlyKsc1()
    {
        Assert.True(EnshroudedInventoryScanner.HasKscHeader("KSC1"u8));
        Assert.False(EnshroudedInventoryScanner.HasKscHeader("JSON"u8));
        Assert.False(EnshroudedInventoryScanner.HasKscHeader("KSC"u8));
    }

    [Fact]
    public void CollectReportsMissingDirectoriesWithoutWriting()
    {
        var report = new EnshroudedInventoryScanner().Collect(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")),
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        Assert.Null(report.InstallationPath);
        Assert.NotNull(report.Processes);
        Assert.Empty(report.Savegames);
        Assert.NotEmpty(report.Errors);
    }

    [Fact]
    public void SnapshotCopiesFilesAndWritesManifest()
    {
        var source = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        var workspace = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var sourceFile = Path.Combine(source, "sample-index");
        File.WriteAllText(sourceFile, "{\"latest\":1}");

        var snapshot = new SavegameWorkspaceService().CreateSnapshot(source, workspace);

        Assert.Single(snapshot.Files);
        Assert.True(File.Exists(Path.Combine(snapshot.SnapshotDirectory, "sample-index")));
        Assert.True(File.Exists(Path.Combine(snapshot.SnapshotDirectory, "snapshot-manifest.json")));
        Assert.Equal(snapshot.Files[0].Sha256, snapshot.Files[0].Sha256.ToUpperInvariant());
    }

    [Fact]
    public void SnapshotRejectsWorkspaceInsideSource()
    {
        var source = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;

        Assert.Throws<ArgumentException>(() =>
            new SavegameWorkspaceService().CreateSnapshot(source, Path.Combine(source, "workspace")));
    }

    [Fact]
    public void AnalysisGroupsVersionsAndReadsIndexes()
    {
        var source = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        File.WriteAllBytes(Path.Combine(source, "abc12345"), "KSC1-data"u8.ToArray());
        File.WriteAllBytes(Path.Combine(source, "abc12345-1"), "KSC1-data-1"u8.ToArray());
        File.WriteAllText(Path.Combine(source, "abc12345-index"), "{\"latest\":1,\"time\":42,\"deleted\":false}");

        var report = new SavegameAnalysisService().Analyze(source);

        var group = Assert.Single(report.Groups);
        Assert.Equal("abc12345", group.GroupId);
        Assert.Equal(2, group.Versions.Count);
        Assert.Equal("1", Assert.Single(group.Indexes).Latest);
        Assert.Equal(42, Assert.Single(group.Indexes).UnixTime);
        Assert.Empty(report.Errors);
    }

    [Fact]
    public void ByteDiffReportsChangedBytesAndFirstOffset()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        var left = Path.Combine(directory, "left");
        var right = Path.Combine(directory, "right");
        File.WriteAllBytes(left, [1, 2, 3, 4]);
        File.WriteAllBytes(right, [1, 9, 3, 4, 5]);

        var diff = new SavegameDiffService().Compare(left, right);

        Assert.Equal(4, diff.ComparedBytes);
        Assert.Equal(2, diff.DifferentBytes);
        Assert.Equal(1, diff.FirstDifferenceOffset);
        Assert.NotEqual(diff.LeftSha256, diff.RightSha256);
    }

    [Fact]
    public void SemanticDiffAttributesComponentChangesToSchemaFields()
    {
        var schema = new ComponentSchemaFieldObservation(
            "value", 0, 4, 0, EvidenceStatus.Observed);
        var beforeComponent = new SavedEntityComponentObservation(
            "Example", 7, 4, false, EvidenceStatus.Observed, [schema], [1, 0, 0, 0]);
        var afterComponent = new SavedEntityComponentObservation(
            "Example", 7, 4, false, EvidenceStatus.Observed, [schema], [2, 0, 0, 0]);
        var before = new SavedEntityArchiveReport(
            4,
            [new SavedEntityObservation(42, 1, [beforeComponent], EvidenceStatus.Observed)],
            ["Example"], [], EvidenceStatus.Observed);
        var after = new SavedEntityArchiveReport(
            4,
            [new SavedEntityObservation(42, 1, [afterComponent], EvidenceStatus.Observed)],
            ["Example"], [], EvidenceStatus.Observed);

        var diff = new SavegameSemanticDiffService().Compare(before, after);

        var component = Assert.Single(diff.Changes);
        var field = Assert.Single(component.Fields);
        Assert.Equal(42UL, component.EntityId);
        Assert.Equal("value", field.FieldPath);
        Assert.Equal(SavegameChangeKind.Changed, field.Kind);
        Assert.Contains("0x01000000", field.BeforeValue);
        Assert.Contains("0x02000000", field.AfterValue);
    }

    [Fact]
    public void Ksc1StructureAnalyzerReportsObservedHeaderAndMarkers()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        var path = Path.Combine(directory, "sample-save");
        File.WriteAllBytes(path,
        [
            (byte)'K', (byte)'S', (byte)'C', (byte)'1',
            8, 0, 0, 0,
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
            0x39, 0x05, 0, 0,
            (byte)'E', (byte)'X', (byte)'T', (byte)'S', 0x19, 0, 0, 0
        ]);

        var report = new Ksc1StructureAnalyzer().Analyze(path);

        Assert.Equal("KSC1", report.Magic);
        Assert.Equal((uint)8, report.FormatValue);
        var marker = Assert.Single(report.Markers);
        Assert.Equal("EXTS", marker.Name);
        Assert.Equal((uint)25, marker.CandidateLength);
    }

    [Fact]
    public void Ksc1ContainerAnalyzerValidatesTablePayloadAndBlobMetadata()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        var path = Path.Combine(directory, "container-save");
        var payload = new byte[] { 0x28, 0xB5, 0x2F, 0xFD, 1, 2, 3 };
        var table = new byte[12];
        BinaryPrimitives.WriteUInt32LittleEndian(table.AsSpan(0, 4), 0x539);
        "EXTS"u8.CopyTo(table.AsSpan(4, 4));
        BinaryPrimitives.WriteUInt32LittleEndian(table.AsSpan(8, 4), (uint)payload.Length);
        var bytes = new byte[0x18 + table.Length + payload.Length];
        "KSC1"u8.CopyTo(bytes.AsSpan(0, 4));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4, 4), 1);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(8, 8), Ksc1Crc64.Compute(table));
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(16, 8), Ksc1Crc64.Compute(payload));
        table.CopyTo(bytes.AsSpan(0x18));
        payload.CopyTo(bytes.AsSpan(0x18 + table.Length));
        File.WriteAllBytes(path, bytes);

        var report = new Ksc1ContainerAnalyzer().Analyze(path);

        Assert.True(report.TableCrcValid);
        Assert.True(report.DataCrcValid);
        var blob = Assert.Single(report.Blobs);
        Assert.Equal("EXTS", blob.Tag);
        Assert.True(blob.LooksLikeZstd);
        Assert.Equal(EvidenceStatus.Verified, report.Status);
    }

    [Fact]
    public void ProvidedWorldSaveCanBeDecompressedReadOnly()
    {
        var workspace = FindWorkspaceRoot();
        var path = Path.Combine(workspace, "data", "savegames", "34d85178");
        if (!File.Exists(path))
        {
            return;
        }

        var container = new Ksc1ContainerAnalyzer().Analyze(path);
        Assert.True(container.TableCrcValid);
        Assert.True(container.DataCrcValid);

        var reader = new Ksc1PayloadReader();
        var payloads = container.Blobs.Select(blob => reader.Read(path, blob)).ToArray();

        Assert.Equal(container.Blobs.Count, payloads.Length);
        Assert.All(payloads, payload => Assert.True(payload.UncompressedSize > 0));
    }

    [Fact]
    public void ProvidedWorldSaveExposesKnowledgeEntries()
    {
        var workspace = FindWorkspaceRoot();
        var path = Path.Combine(workspace, "data", "savegames", "34d85178");
        if (!File.Exists(path))
            return;

        var knowledge = new KnowledgePayloadService().Analyze(path);

        Assert.True(knowledge.Status == EvidenceStatus.Observed,
            string.Join(Environment.NewLine, knowledge.Errors));
        Assert.NotEmpty(knowledge.Entries);
        Assert.All(knowledge.Entries, entry => Assert.Equal(EvidenceStatus.Observed, entry.Status));
    }

    [Fact]
    public void ProvidedCharacterSaveExposesReadOnlyBdbFields()
    {
        var workspace = FindWorkspaceRoot();
        var savegames = Path.Combine(workspace, "data", "savegames");
        if (!File.Exists(Path.Combine(savegames, "characters-index")))
        {
            return;
        }

        var report = new CharacterPayloadService().Analyze(savegames);

        Assert.NotEmpty(report.Characters);
        Assert.All(report.Characters, character =>
        {
            Assert.Equal("CHAR", character.Tag);
            Assert.Equal(EvidenceStatus.Observed, character.Status);
            Assert.True(character.NodeCount > 0);
            Assert.NotEmpty(character.Fields);
        });
        Assert.Contains(report.Characters, character => character.Name is not null);
        Assert.Contains(report.Characters, character => character.Level is not null);
    }

    [Fact]
    public void ProvidedCharacterBackupsExposeSelectableAutosaves()
    {
        var workspace = FindWorkspaceRoot();
        var savegames = Path.Combine(workspace, "data", "savegames");
        if (!File.Exists(Path.Combine(savegames, "characters-index")))
        {
            return;
        }

        var options = new CharacterPayloadService().ListOptions(savegames);

        Assert.NotEmpty(options);
        Assert.Equal(4, options.Count);
        Assert.All(options, option => Assert.True(option.IsLatest));
        Assert.All(options, option => Assert.StartsWith("characters", option.FileName));
        Assert.Contains(options, option => option.CharacterName == "Kaerwen" && option.Level == 22);
        Assert.Contains(options, option => option.Level == 45);
        Assert.Contains(options, option => option.IsLatest && option.Level == 45);
        Assert.Equal((uint)45, options[0].Level);
    }

    [Fact]
    public void ProvidedCharacterBackupsExposeReadOnlyItemCandidates()
    {
        var workspace = FindWorkspaceRoot();
        var savegames = Path.Combine(workspace, "data", "savegames");
        if (!File.Exists(Path.Combine(savegames, "characters-index")))
        {
            return;
        }

        var character = new CharacterPayloadService().ListOptions(savegames).First();
        var report = new ItemExtractionService().Extract(savegames, character);

        Assert.NotNull(report);
        Assert.NotNull(report.Candidates);
        Assert.All(report.Candidates, candidate =>
        {
            Assert.False(string.IsNullOrWhiteSpace(candidate.IdOrReference));
            Assert.False(string.IsNullOrWhiteSpace(candidate.SourcePath));
            Assert.NotEqual(EvidenceStatus.Unknown, candidate.EvidenceStatus);
        });
        Assert.DoesNotContain(report.Candidates, candidate =>
            candidate.IdOrReference.StartsWith("node:", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(report.Entities);
        Assert.NotEqual(EvidenceStatus.Unknown, report.Status);
        Assert.Contains(report.Containers ?? [],
            container => container.Name == "blueprintMaterialsRoof");
        Assert.Contains(report.Containers ?? [],
            container => container.Name == "terrainMaterials");
    }

    [Fact]
    public void ProvidedCharacterBackupsExposeSavedEntityComponents()
    {
        var workspace = FindWorkspaceRoot();
        var savegames = Path.Combine(workspace, "data", "savegames");
        if (!File.Exists(Path.Combine(savegames, "characters-index")))
            return;

        var service = new CharacterPayloadService();
        var character = service.ListOptions(savegames).First();
        var entities = service.AnalyzeSavedEntities(savegames, character);

        Assert.Equal(EvidenceStatus.Observed, entities.Status);
        Assert.NotEmpty(entities.Entities);
        Assert.NotEmpty(entities.ComponentDefinitions);
        Assert.Contains(entities.ComponentDefinitions, name => name == "Inventory");
        Assert.Contains(entities.Entities.SelectMany(entity => entity.Components),
            component => component.Name == "Inventory");
        var inventory = entities.Entities.SelectMany(entity => entity.Components)
            .First(component => component.Name == "Inventory");
        Assert.NotEmpty(inventory.Fields ?? []);
        var slots = Assert.Single(inventory.Fields!, field => field.Name == "slots");
        Assert.NotNull(slots.ElementCount);
        Assert.NotEmpty(slots.ElementFields ?? []);
        Assert.Contains(slots.ElementFields!, field => field.Name == "id" && field.Size == 4);
        var data = Assert.Single(slots.ElementFields!, field => field.Name == "data");
        Assert.NotEmpty(data.ElementFields ?? []);
        Assert.Contains(data.ElementFields!, field => field.Name == "count" && field.Size == 4);
        Assert.Contains(data.ElementFields!, field => field.Name == "pide" && field.Size == 4);
        Assert.Contains(entities.Entities.SelectMany(entity => entity.Components),
            component => component.Name == "InventorySetup");
    }

    [Fact]
    public void ProvidedCharacterBackupsExposeInventorySlotReferences()
    {
        var workspace = FindWorkspaceRoot();
        var savegames = Path.Combine(workspace, "data", "savegames");
        if (!File.Exists(Path.Combine(savegames, "characters-index")))
            return;

        var character = new CharacterPayloadService().ListOptions(savegames).First();
        var catalog = new ItemCatalogService().Discover(@"D:\SteamLibrary\steamapps\common\Enshrouded");
        var inventory = new CharacterInventoryService().Analyze(savegames, character, catalog);

        Assert.Equal(EvidenceStatus.Observed, inventory.Status);
        Assert.NotEmpty(inventory.Slots);
        Assert.NotEmpty(inventory.Containers);
        Assert.All(inventory.Slots, slot => Assert.StartsWith("Inventargruppe", slot.ContainerName));
        Assert.All(inventory.Containers, container =>
        {
            Assert.True(container.StorageAreaCount > 0);
            Assert.True(container.SlotCount > 0);
        });
        Assert.Contains(inventory.Slots, slot => slot.ItemReference != 0);
        Assert.Contains(inventory.Slots, slot => slot.ItemReference != 0 && slot.Quantity is > 0);
        Assert.Contains(inventory.Slots, slot => slot.Instance is not null);
        Assert.All(inventory.Slots.Where(slot => slot.Instance is not null), slot =>
        {
            Assert.Equal(slot.ItemReference, slot.Instance!.ItemReference);
            Assert.Equal(slot.ItemStateReference, (uint)slot.Instance.EntityId);
        });
        Assert.Contains(inventory.Slots, slot => slot.Instance?.HasWeaponGem == true);
        Assert.Contains(inventory.Slots, slot => slot.Instance?.HasDurability == true);
        Assert.Contains(inventory.Slots, slot => slot.Instance?.Properties is { Count: > 0 });
        Assert.Contains(inventory.Slots, slot => slot.Instance?.Components?.Contains("ItemState") == true);
        if (catalog.Status != EvidenceStatus.Unknown)
            Assert.Contains(inventory.Slots, slot => slot.ItemName is not null);
    }

    [Fact]
    public void ItemCatalogResolvesOnlyValidatedDescriptorNames()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        File.WriteAllText(Path.Combine(directory, "items.json"),
            """{"items":[{"id":12345,"displayName":"Testküche"}]}""");
        File.WriteAllBytes(Path.Combine(directory, "enshrouded.exe"), [1, 2, 3, 4]);

        var service = new ItemCatalogService();
        var catalog = service.Discover(directory);
        var resolved = service.Resolve("12345", catalog);
        var unknown = service.Resolve("99999", catalog);

        Assert.Equal("Testküche", resolved.Definition?.DisplayName);
        Assert.Equal(EvidenceStatus.Verified, resolved.Status);
        Assert.Null(unknown.Definition);
        Assert.Contains("Name nicht aufgelöst", unknown.Evidence);
    }

    [Fact]
    public void ItemCatalogReadsValidatedXmlDescriptorNames()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        File.WriteAllText(Path.Combine(directory, "items.xml"),
            """<items><item id="54321" name="Testschwert" /></items>""");

        var catalog = new ItemCatalogService().Discover(directory);
        var resolved = new ItemCatalogService().Resolve("54321", catalog);

        Assert.Equal("Testschwert", resolved.Definition?.DisplayName);
        Assert.Equal(EvidenceStatus.Verified, resolved.Status);
    }

    [Fact]
    public void ItemCatalogReadsExportedItemDetails()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        File.WriteAllText(Path.Combine(directory, "item_index.json"),
            """[{"item_id":"54321","debug_name":"Weapon_T4_Sword","category":"Weapons","rarity":"Rare","equipment_slot":"MainHand","level_min":12,"level_max":15}]""");

        var catalog = new ItemCatalogService().Discover(directory);
        var resolved = new ItemCatalogService().Resolve("54321", catalog);

        Assert.Equal("Weapon_T4_Sword", resolved.Definition?.DisplayName);
        Assert.Equal("Weapons", resolved.Definition?.Category);
        Assert.Equal(12, resolved.Definition?.MinLevel);
        Assert.Equal("MainHand", resolved.Definition?.EquipmentSlot);
    }

    [Fact]
    public void ItemCatalogPrefersInternalNameOverLocalizationKey()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        File.WriteAllText(Path.Combine(directory, "item_index.json"),
            """[{"item_id":"54321","debug_name":"Weapon_T4_Sword","name":"13f8a50b-4a1a-44d8-ac81-61982a36907a","name_text":""}]""");

        var catalog = new ItemCatalogService().Discover(directory);
        var resolved = new ItemCatalogService().Resolve("54321", catalog);

        Assert.Equal("Weapon_T4_Sword", resolved.Definition?.DisplayName);
    }

    [Fact]
    public void ItemCatalogUsesExportedLocalizationForItemKeys()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        var localizationDirectory = Directory.CreateDirectory(
            Path.Combine(directory, "export", "localization_dump")).FullName;
        File.WriteAllText(Path.Combine(localizationDirectory, "localization_map.json"),
            """{"13f8a50b-4a1a-44d8-ac81-61982a36907a":"Eisenschwert"}""");
        File.WriteAllText(Path.Combine(directory, "item_index.json"),
            """[{"item_id":"54321","debug_name":"Weapon_T4_Sword","name":"13f8a50b-4a1a-44d8-ac81-61982a36907a"}]""");

        var catalog = new ItemCatalogService().Discover(directory);
        var resolved = new ItemCatalogService().Resolve("54321", catalog);

        Assert.Equal("Eisenschwert", resolved.Definition?.DisplayName);
    }

    [Fact]
    public void GameDataAnalysisFingerprintsLocalDefinitionSources()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        File.WriteAllText(Path.Combine(directory, "enshrouded.kfc"), "descriptor");
        var exportDirectory = Directory.CreateDirectory(
            Path.Combine(directory, "export", "mapping_discovery")).FullName;
        File.WriteAllText(Path.Combine(exportDirectory, "item_index.json"),
            """[{"item_id":"54321","debug_name":"Weapon_T4_Sword"}]""");

        var report = new GameDataAnalysisService().Analyze(directory);

        Assert.Equal(2, report.Sources.Count);
        Assert.Contains("enshrouded.kfc", report.ChangedSources);
        Assert.Equal(1, report.ItemDefinitionCount);
    }

    [Fact]
    public void CharacterAnalysisCanBeExportedWithoutTouchingSavegames()
    {
        var workspace = FindWorkspaceRoot();
        var savegames = Path.Combine(workspace, "data", "savegames");
        if (!File.Exists(Path.Combine(savegames, "characters-index")))
            return;

        var character = new CharacterPayloadService().ListOptions(savegames).First();
        var outputDirectory = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        var output = Path.Combine(outputDirectory, "character-analysis.json");
        var sourceFile = Path.Combine(savegames, "characters-index");
        var sourceHashBefore = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            File.ReadAllBytes(sourceFile)));

        new SavegameAnalysisExportService().WriteCharacterExport(
            savegames,
            @"D:\SteamLibrary\steamapps\common\Enshrouded",
            character,
            output);

        using var document = JsonDocument.Parse(File.ReadAllBytes(output));
        Assert.Equal(2, document.RootElement.GetProperty("SchemaVersion").GetInt32());
        Assert.True(document.RootElement.GetProperty("Items").GetProperty("Containers").GetArrayLength() > 0);
        Assert.True(document.RootElement.GetProperty("Inventory").GetProperty("Slots").GetArrayLength() > 0);
        Assert.Equal(sourceHashBefore, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            File.ReadAllBytes(sourceFile))));
    }

    [Fact]
    public void RuntimeStateRequiresAnIndexAndKscContainerForSavegameSelection()
    {
        var directory = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        File.WriteAllText(Path.Combine(directory, "world-index"), "{\"latest\":0}");
        File.WriteAllBytes(Path.Combine(directory, "world"), "KSC1"u8.ToArray());

        var state = new RuntimeStateService().Detect("missing-installation", directory);

        Assert.True(state.HasSelectedSavegame);
        Assert.Equal(directory, state.SelectedSavegamePath);
    }

    [Fact]
    public void ProvidedBackupsMapAutosavesToKnownSlots()
    {
        var workspace = FindWorkspaceRoot();
        var savegames = Path.Combine(workspace, "data", "savegames");
        if (!Directory.Exists(savegames))
        {
            return;
        }

        var report = new SavegameSlotService().Analyze(savegames);
        var slotSeven = Assert.Single(report.Slots, slot => slot.SlotNumber == 7);

        Assert.Equal("34d85178", slotSeven.SavegameId);
        Assert.True(slotSeven.Available);
        Assert.NotNull(slotSeven.LatestAutosave);
        Assert.NotNull(slotSeven.LatestFileName);
        Assert.True(slotSeven.VersionCount > 0);
        Assert.Equal("Solo Run", Assert.Single(report.Slots, slot => slot.SavegameId == "36d8549e").WorldName);
    }

    [Fact]
    public void ProvidedBackupsContainOnlyCrcValidKscContainers()
    {
        var workspace = FindWorkspaceRoot();
        var savegames = Path.Combine(workspace, "data", "savegames");
        if (!Directory.Exists(savegames))
        {
            return;
        }

        var paths = Directory.EnumerateFiles(savegames)
            .Where(path => EnshroudedInventoryScanner.HasKscHeader(File.ReadAllBytes(path)))
            .ToArray();

        Assert.Equal(143, paths.Length);
        foreach (var path in paths)
        {
            var report = new Ksc1ContainerAnalyzer().Analyze(path);
            Assert.True(report.TableCrcValid, path);
            Assert.True(report.DataCrcValid, path);
            Assert.Empty(report.Errors);
        }
    }

    private static string FindWorkspaceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ROADMAP.md")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
