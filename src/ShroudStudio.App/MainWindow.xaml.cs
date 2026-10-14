using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using ShroudStudio.Core;
using ShroudStudio.Infrastructure;

namespace ShroudStudio.App;

public partial class MainWindow : Window
{
    private readonly RuntimeStateService _runtimeStateService = new();
    private readonly List<SavegameSlot> _availableSlots = [];
    private string _currentPage = "overview";
    private bool _hasSelectedSavegame;
    private SavegameSlot? _selectedSavegame;
    private SavegameAutosave? _selectedAutosave;
    private CharacterSavegameOption? _selectedCharacter;
    private ItemExtractionReport? _itemReport;
    private ItemCatalogReport? _itemCatalog;
    private CharacterInventoryReport? _characterInventory;
    private KnowledgePayloadReport? _worldKnowledge;
    private InventorySlotDisplay? _selectedInventorySlot;
    private bool _updatingContainerSelection;

    public MainWindow()
    {
        InitializeComponent();
        RefreshState();
        ShowOverview();
    }

    private async void WindowLoaded(object sender, RoutedEventArgs e)
    {
        ((Storyboard)FindResource("GlowAnimation")).Begin(this, true);
        ((Storyboard)FindResource("AmbientAnimation")).Begin(this, true);
        await LoadSavegamesAsync();
    }

    private async Task LoadSavegamesAsync()
    {
        var savegamePath = ResolveWorkspacePath("data\\savegames");
        SetLoading(true, "Spielstände und Info-Dateien werden gelesen...");
        try
        {
            var report = await Task.Run(() => new SavegameSlotService().Analyze(savegamePath));
            var characters = await Task.Run(() => new CharacterPayloadService().ListOptions(savegamePath));
            _availableSlots.Clear();
            SlotComboBox.Items.Clear();
            AutosaveComboBox.Items.Clear();
            CharacterComboBox.Items.Clear();

            foreach (var slot in report.Slots.Where(slot => slot.Available))
            {
                _availableSlots.Add(slot);
                SlotComboBox.Items.Add(new ComboBoxItem
                {
                    Content = FormatSavegameName(slot),
                    Tag = slot,
                    Style = (Style)FindResource("DarkComboBoxItemStyle")
                });
            }

            if (SlotComboBox.Items.Count > 0)
            {
                SlotComboBox.SelectedIndex = 0;
            }
            else
            {
                SlotComboBox.Items.Add(new ComboBoxItem
                {
                    Content = "Keine Spielstände gefunden",
                    IsEnabled = false,
                    Style = (Style)FindResource("DarkComboBoxItemStyle")
                });
            }

            foreach (var character in characters)
            {
                CharacterComboBox.Items.Add(new ComboBoxItem
                {
                    Content = FormatCharacterName(character),
                    Tag = character,
                    Style = (Style)FindResource("DarkComboBoxItemStyle")
                });
            }

            if (CharacterComboBox.Items.Count > 0)
            {
                CharacterComboBox.SelectedIndex = characters
                    .Select((character, index) => (character, index))
                    .FirstOrDefault(entry => entry.character.IsLatest).index;
            }
            else
            {
                CharacterComboBox.Items.Add(new ComboBoxItem
                {
                    Content = "Keine Charakter-Savegames gefunden",
                    IsEnabled = false,
                    Style = (Style)FindResource("DarkComboBoxItemStyle")
                });
            }
        }
        catch (IOException)
        {
            SetUnavailableState();
        }
        catch (UnauthorizedAccessException)
        {
            SetUnavailableState();
        }
        finally
        {
            SetLoading(false, string.Empty);
        }
    }

    private void SetLoading(bool isLoading, string message)
    {
        LoadingTextBlock.Text = message;
        LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        IsEnabled = !isLoading;
    }

    private static string FormatSavegameName(SavegameSlot slot)
    {
        var autosave = slot.LatestAutosave is { } latest ? $" • Autosave {latest}" : string.Empty;
        var name = slot.WorldName is { Length: > 0 } worldName ? worldName : $"Spielstand {slot.SavegameId}";
        return $"Slot {slot.SlotNumber} • {name}{autosave}";
    }

    private static string FormatCharacterName(CharacterSavegameOption character)
    {
        var name = character.CharacterName is { Length: > 0 }
            ? character.CharacterName
            : "Charaktername unbekannt";
        var level = character.Level is { } value ? $" • Stufe {value}" : string.Empty;
        var latest = character.IsLatest ? " • zuletzt" : string.Empty;
        return $"Charakter {character.Number} • {name}{level}";
    }

    private void RefreshState()
    {
        var savegamePath = ResolveWorkspacePath("data\\savegames");
        try
        {
            var state = _runtimeStateService.Detect(
                "D:\\SteamLibrary\\steamapps\\common\\Enshrouded",
                savegamePath);
            var selected = state.HasSelectedSavegame;
            _hasSelectedSavegame = selected;
            HeaderStatusTextBlock.Text = state.IsGameRunning ? "Spiel läuft" : "Spiel nicht gestartet";
            HeaderStatusTextBlock.Foreground = state.IsGameRunning
                ? Brushes.LightGreen
                : Brushes.Orange;
            FooterStatusTextBlock.Text = selected
                ? "Spielstand erkannt • Original geschützt"
                : "Kein Spielstand ausgewählt";
            SidebarSafetyTextBlock.Text = selected
                ? "Dein Original bleibt geschützt."
                : "Wähle einen Spielstand, um ihn zu verwalten.";
            SafetyStatusTextBlock.Text = selected ? "Original geschützt" : "Spielstand auswählen";
            SafetyStatusTextBlock.Foreground = selected ? Brushes.LightGreen : Brushes.Orange;
            OverviewSavegamePathTextBlock.Text = selected
                ? "Spielstand bereit für sichere, geprüfte Aktionen."
                : "Noch kein gültiger Spielstand ausgewählt.";
            UpdateSelectedSavegameView();
        }
        catch (IOException)
        {
            SetUnavailableState();
        }
        catch (UnauthorizedAccessException)
        {
            SetUnavailableState();
        }
    }

    private void SetUnavailableState()
    {
        _hasSelectedSavegame = false;
        HeaderStatusTextBlock.Text = "Status nicht verfügbar";
        HeaderStatusTextBlock.Foreground = Brushes.Orange;
        FooterStatusTextBlock.Text = "Spielstand konnte nicht geprüft werden";
        SidebarSafetyTextBlock.Text = "Prüfe den Speicherort in den Einstellungen.";
        SafetyStatusTextBlock.Text = "Prüfung erforderlich";
        SafetyStatusTextBlock.Foreground = Brushes.Orange;
        OverviewSavegamePathTextBlock.Text = "Der Spielstand konnte noch nicht geprüft werden.";
        OverviewSavegameNameTextBlock.Text = "Spielstand nicht verfügbar";
        OverviewSavegameInfoTextBlock.Text = "Bitte prüfe den Arbeitsbereich in den Einstellungen.";
    }

    private void OverviewNavigationClick(object sender, RoutedEventArgs e) => ShowOverview();

    private void ItemsNavigationClick(object sender, RoutedEventArgs e)
    {
        ShowContent(
            "items",
            "Items",
            "Finde Gegenstände schnell und erhalte verständliche Informationen zu ihrer Verwendung.",
            "Die Item-Datenbank wird aufgebaut, sobald die Spieldefinitionen zuverlässig verfügbar sind.");
    }

    private void InventoryNavigationClick(object sender, RoutedEventArgs e)
    {
        ShowContent(
            "inventory",
            "Inventar & Kisten",
            "Durchsuche dein Inventar und deine Lager, ohne dich durch mehrere Spielstände arbeiten zu müssen.",
            "Die sichere Inventaransicht folgt, sobald Item- und Containerdaten vollständig verifiziert sind.");
    }

    private void WeaponsNavigationClick(object sender, RoutedEventArgs e)
    {
        ShowContent(
            "weapons",
            "Waffen & Ausrüstung",
            "Vergleiche Ausrüstung und bearbeite nur Eigenschaften, deren Wirkung zuverlässig bekannt ist.",
            "Waffenwerte werden erst angezeigt, wenn ihre Bedeutung für deine Spielversion bestätigt ist.");
    }

    private void SettingsNavigationClick(object sender, RoutedEventArgs e)
    {
        ShowContent(
            "settings",
            "Einstellungen",
            "Passe Darstellung, Speicherorte und Sicherheitsverhalten an deine Arbeitsweise an.",
            "Weitere Einstellungen werden ergänzt, sobald die spielerorientierten Arbeitsbereiche verfügbar sind.");
    }

    private void DebugNavigationClick(object sender, RoutedEventArgs e)
    {
        ShowContent(
            "debug",
            "Diagnose",
            "Prüfe erkannte Spiel- und Savegame-Daten für eine sichere Fehleranalyse.",
            "Diese Ansicht enthält technische Nachweise für Support und Entwicklung. Sie verändert keine Daten.");
    }

    private void ShowOverview()
    {
        _currentPage = "overview";
        OverviewPage.Visibility = Visibility.Visible;
        ContentPage.Visibility = Visibility.Collapsed;
        PageTitleTextBlock.Text = "Übersicht";
        PageSubtitleTextBlock.Text = "Verwalte deine Spielstände und bereite deine nächsten Abenteuer vor.";
        UpdateNavigation(OverviewButton);
    }

    private void ShowContent(
        string page,
        string title,
        string subtitle,
        string message)
    {
        _currentPage = page;
        OverviewPage.Visibility = Visibility.Collapsed;
        ContentPage.Visibility = Visibility.Visible;
        PageTitleTextBlock.Text = title;
        PageSubtitleTextBlock.Text = subtitle;
        ContentPageHeadline.Text = title;
        ContentPageMessage.Text = GetCapabilityMessage(page, message);
        ContentPageFacts.Text = GetCapabilityFacts(page);
        UpdateContentPageData(page);
        UpdateItemsList(page);
        UpdateLiveDataPanel(page);
        UpdateNavigation(page switch
        {
            "items" => ItemsButton,
            "inventory" => InventoryButton,
            "weapons" => WeaponsButton,
            "debug" => DebugButton,
            _ => null
        });
    }

    private void UpdateItemsList(string page)
    {
        var visible = page is "items" or "inventory" or "weapons";
        ItemsListTitle.Text = page switch
        {
            "inventory" => "Inventar",
            "weapons" => "Ausrüstung",
            _ => "Extrahierte Item-Kandidaten"
        };
        ItemsListTitle.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        ItemsListBox.Visibility = page == "items" ? Visibility.Visible : Visibility.Collapsed;
        ItemSearchPanel.Visibility = page == "items" ? Visibility.Visible : Visibility.Collapsed;
        InventoryWorkspacePanel.Visibility = page is "inventory" or "weapons"
            ? Visibility.Visible : Visibility.Collapsed;
        ContainerSelectionPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        ItemsListBox.Items.Clear();
        InventoryGrid.Items.Clear();
        ContainerComboBox.Items.Clear();
        ItemsListMessage.Text = string.Empty;
        ItemsListMessage.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        if (!visible)
        {
            return;
        }

        if (_selectedCharacter is not { } character)
        {
            ItemsListMessage.Text = "Kein Charakter ausgewählt.";
            return;
        }

        _itemReport ??= new ItemExtractionService().Extract(
            ResolveWorkspacePath("data\\savegames"), character);
        _itemCatalog ??= new ItemCatalogService().Discover(
            @"D:\SteamLibrary\steamapps\common\Enshrouded");
        if (page is "inventory" or "weapons" && _characterInventory is null)
        {
            _characterInventory = new CharacterInventoryService().Analyze(
                ResolveWorkspacePath("data\\savegames"),
                character,
                _itemCatalog);
        }
        PopulateContainerSelection(page);
        if (page == "items")
            PopulateCatalogItems();

        var resolvedCount = _itemReport.Candidates.Count(candidate =>
            candidate.ResolutionStatus is EvidenceStatus.Verified or EvidenceStatus.Observed);
        if (page == "items" && _itemReport.Candidates.Count > 0)
        {
            ItemsListMessage.Text = $"{_itemReport.Candidates.Count} konkrete Iteminstanzen erkannt; " +
                $"{resolvedCount} Namen aus lokalen Definitionen zugeordnet.";
        }
        else if (page == "items" && _itemReport.Containers is { Count: > 0 })
        {
            var containerNames = string.Join(", ", _itemReport.Containers
                .Select(container => container.DisplayName)
                .Distinct(StringComparer.OrdinalIgnoreCase));
            ItemsListMessage.Text += $" Im gewählten Charakter erkannt: {containerNames}; " +
                "konkrete Item-IDs daraus sind noch nicht aufgelöst.";
        }
    }

    private void PopulateCatalogItems()
    {
        if (_itemCatalog is null)
            return;

        ItemsListBox.Items.Clear();
        var search = ItemSearchTextBox.Text.Trim();
        var definitions = _itemCatalog.Definitions
            .Where(definition =>
                string.IsNullOrWhiteSpace(search) ||
                definition.DisplayName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                definition.Category?.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                definition.Reference.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(definition => definition.Category)
            .ThenBy(definition => definition.DisplayName)
            .Take(500)
            .ToArray();

        foreach (var definition in definitions)
        {
            ItemsListBox.Items.Add(new ItemCandidate(
                definition.Reference,
                null,
                "Lokale Spieldaten",
                definition.Evidence,
                definition.EvidenceStatus,
                definition.EquipmentSlot,
                definition.Category,
                ResolvedName: ToPlayerItemName(definition.DisplayName),
                ResolvedCategory: ToPlayerCategory(definition.Category),
                ResolvedDetails: FormatItemDetails(definition),
                ResolutionStatus: EvidenceStatus.Verified,
                ResolutionEvidence: definition.Evidence));
        }

        ItemsListTitle.Text = "Spieldaten-Katalog";
        var limitNote = definitions.Length == 500 ? " (erste 500 Treffer)" : string.Empty;
        ItemsListMessage.Text = $"{_itemCatalog.Definitions.Count} lokale Definitionen verfügbar; " +
            $"{definitions.Length} Treffer angezeigt{limitNote}.";
    }

    private void ItemSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_currentPage == "items" && _itemCatalog is not null)
            PopulateCatalogItems();
    }

    private void PopulateContainerSelection(string page)
    {
        _updatingContainerSelection = true;
        try
        {
            if (page is "inventory" or "weapons" && _characterInventory is not null)
            {
                foreach (var container in _characterInventory.Containers)
                {
                    ContainerComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = $"{container.Name} · {container.SlotCount} Plätze",
                        Tag = container.Name,
                        Style = (Style)FindResource("DarkComboBoxItemStyle")
                    });
                }
            }
            else
            {
                foreach (var container in _itemReport?.Containers ?? [])
                {
                    ContainerComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = container.DisplayName,
                        Tag = container,
                        Style = (Style)FindResource("DarkComboBoxItemStyle")
                    });
                }
            }

            if (ContainerComboBox.Items.Count > 0)
                ContainerComboBox.SelectedIndex = 0;
        }
        finally
        {
            _updatingContainerSelection = false;
        }

        if (page is "inventory" or "weapons")
            PopulateInventoryGrid(page);
    }

    private void PopulateInventoryGrid(string page)
    {
        _characterInventory ??= _selectedCharacter is { } character
            ? new CharacterInventoryService().Analyze(
                ResolveWorkspacePath("data\\savegames"),
                character,
                _itemCatalog)
            : new CharacterInventoryReport([], [], [], EvidenceStatus.Unknown);
        var slots = new List<InventorySlotDisplay>();
        if (page is "inventory" or "weapons" && _characterInventory.Slots.Count > 0)
        {
            var selectedContainer = (ContainerComboBox.SelectedItem as ComboBoxItem)?.Tag as string;
            var sourceSlots = _characterInventory.Slots
                .Where(slot => selectedContainer is null ||
                               string.Equals(slot.ContainerName, selectedContainer, StringComparison.Ordinal));
            if (page == "weapons")
            {
                sourceSlots = sourceSlots.Where(slot =>
                    slot.Instance?.BaseDamage is not null || slot.Instance?.HasWeaponGem == true);
            }

            slots.AddRange(sourceSlots
                .Select(slot =>
            {
                var occupied = slot.ItemReference != 0;
                var details = FormatPlayerSlotDetails(slot, occupied);
                return new InventorySlotDisplay(
                    slot.SlotIndex,
                    $"{slot.ContainerName} · {slot.SlotIndex + 1}",
                    ToPlayerItemName(slot.ItemName ?? (occupied ? "Belegt" : "Leer")),
                    details,
                    occupied,
                    slot.Status.ToString(),
                    slot.Instance,
                    slot.Quantity,
                    slot.Category,
                    slot.Definition);
            }));
            ItemsListMessage.Text =
                page == "weapons"
                    ? $"{slots.Count} gespeicherte Waffeninstanzen mit Basisschaden oder Sockel erkannt. " +
                      "Die Zuordnung zu einem konkreten Ausrüstungsslot wird erst nach weiterer Validierung angezeigt."
                    : $"{_characterInventory.Slots.Count} Plätze in {_characterInventory.Containers.Count} gespeicherten " +
                      $"Inventargruppen wurden direkt gelesen; {_characterInventory.Slots.Count(slot => slot.ItemName is not null)} " +
                      "Items sind lokal zugeordnet.";
        }
        else
        {
            ItemsListMessage.Text = page == "weapons"
                ? "Keine gespeicherte Waffeninstanz mit bestätigtem Basisschaden oder Sockel gefunden."
                : "Keine gespeicherten Inventarplätze für den gewählten Charakter gefunden.";
        }

        foreach (var slot in slots)
            InventoryGrid.Items.Add(slot);

        _selectedInventorySlot = slots.FirstOrDefault(slot => slot.IsOccupied);
        UpdateInventoryInspector(page, slots);
    }

    private void ContainerSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingContainerSelection || _currentPage is not ("inventory" or "weapons"))
            return;

        PopulateInventoryGrid(_currentPage);
    }

    private static string? FormatItemDetails(ItemDefinition? definition)
    {
        if (definition is null)
            return null;

        var details = new List<string>();
        if (!string.IsNullOrWhiteSpace(definition.Rarity))
            details.Add(definition.Rarity);
        if (definition.MinLevel is not null || definition.MaxLevel is not null)
            details.Add($"Stufe {definition.MinLevel?.ToString() ?? "?"}-{definition.MaxLevel?.ToString() ?? "?"}");
        if (!string.IsNullOrWhiteSpace(definition.EquipmentSlot))
            details.Add(ToPlayerCategory(definition.EquipmentSlot));
        if (!string.IsNullOrWhiteSpace(definition.DamageSummary))
            details.Add(definition.DamageSummary);
        if (definition.Buffs is { Count: > 0 })
            details.Add($"{definition.Buffs.Count} Boni/Buffs");
        if (!string.IsNullOrWhiteSpace(definition.Description))
            details.Add(definition.Description.Length > 90
                ? definition.Description[..90] + "…"
                : definition.Description);
        return details.Count == 0 ? null : string.Join(" · ", details);
    }

    private static string ToPlayerCategory(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Unbekannte Kategorie";

        return value
            .Replace("keen::ItemInfo", "Itemdefinition", StringComparison.OrdinalIgnoreCase)
            .Replace("Type: 0000025a07cf60d8", "Itemdefinition", StringComparison.OrdinalIgnoreCase)
            .Replace("Weapons", "Waffen", StringComparison.OrdinalIgnoreCase)
            .Replace("Weapon", "Waffe", StringComparison.OrdinalIgnoreCase)
            .Replace("Armor", "Rüstung", StringComparison.OrdinalIgnoreCase)
            .Replace("Consumables", "Verbrauchsgegenstände", StringComparison.OrdinalIgnoreCase)
            .Replace("Consumable", "Verbrauchsgegenstand", StringComparison.OrdinalIgnoreCase)
            .Replace("Materials", "Materialien", StringComparison.OrdinalIgnoreCase)
            .Replace("MainHand", "Haupthand", StringComparison.OrdinalIgnoreCase)
            .Replace("OffHand", "Nebenhand", StringComparison.OrdinalIgnoreCase)
            .Replace("_", " ")
            .Trim();
    }

    private static string ToPlayerItemName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Unbekanntes Item";

        var name = value.Replace("_", " ").Replace("-", " ");
        name = Regex.Replace(name, @"(?<=[a-z])(?=[A-Z])", " ");
        name = Regex.Replace(name, @"\bT(\d+)\b", "Stufe $1", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, @"\b(\d+)H\b", "$1-Hand", RegexOptions.IgnoreCase);
        name = name
            .Replace("Weapon", "Waffe", StringComparison.OrdinalIgnoreCase)
            .Replace("Armor", "Rüstung", StringComparison.OrdinalIgnoreCase)
            .Replace("Consumable", "Verbrauchsgegenstand", StringComparison.OrdinalIgnoreCase)
            .Replace("Food", "Nahrung", StringComparison.OrdinalIgnoreCase)
            .Replace("Potion", "Trank", StringComparison.OrdinalIgnoreCase)
            .Replace("Sword", "Schwert", StringComparison.OrdinalIgnoreCase)
            .Replace("Axe", "Axt", StringComparison.OrdinalIgnoreCase)
            .Replace("Staff", "Stab", StringComparison.OrdinalIgnoreCase)
            .Replace("Wand", "Zauberstab", StringComparison.OrdinalIgnoreCase)
            .Replace("Bow", "Bogen", StringComparison.OrdinalIgnoreCase)
            .Replace("Shield", "Schild", StringComparison.OrdinalIgnoreCase)
            .Replace("Epic", "Episch", StringComparison.OrdinalIgnoreCase)
            .Replace("Rare", "Selten", StringComparison.OrdinalIgnoreCase);
        return Regex.Replace(name, @"\s+", " ").Trim();
    }

    private string GetCapabilityMessage(string page, string fallback)
    {
        if (_selectedCharacter is not { } character)
        {
            return fallback;
        }

        return page switch
        {
            "items" when character.HasInventoryData =>
                $"Im gewählten Charakter-Savegame wurden Itemdaten erkannt. Die sichere Item-Zuordnung wird noch aufgebaut; unbekannte Werte bleiben unverändert.",
            "inventory" when character.HasInventoryData =>
                $"Das Savegame enthält verknüpfte Inventargruppen für {FormatCharacterName(character)}. " +
                "Jede Gruppe enthält ausschließlich über InventorySetup bestätigte Speicherbereiche.",
            "weapons" when character.HasEquipmentData =>
                $"Ausrüstungsdaten wurden im Charakter-Savegame erkannt. Werte werden erst angezeigt, wenn ihre Bedeutung versionssicher bestätigt ist.",
            "debug" =>
                "Die Diagnose zeigt nur geprüfte Strukturen und Hinweise. Unbekannte Daten werden nicht als Spielwerte interpretiert.",
            _ => fallback
        };
    }

    private string GetCapabilityFacts(string page)
    {
        var character = _selectedCharacter;
        return page switch
        {
            "items" when character is { HasInventoryData: true } =>
                $"Ausgewählter Charakter: {FormatCharacterName(character)}\nErkannt: Itemdaten und Nutzungsreihenfolge",
            "inventory" when character is { } =>
                $"Ausgewählter Charakter: {FormatCharacterName(character)}\n" +
                $"Inventardaten: {(character.HasInventoryData ? "erkannt" : "nicht bestätigt")}\n" +
                $"Questdaten: {(character.HasActiveQuest ? "erkannt" : "nicht bestätigt")}",
            "weapons" when character is { } =>
                $"Ausgewählter Charakter: {FormatCharacterName(character)}\n" +
                $"Ausrüstungsstruktur: {(character.HasEquipmentData ? "erkannt" : "noch nicht bestätigt")}",
            "debug" => BuildDebugFacts(),
            _ => string.Empty
        };
    }

    private string BuildDebugFacts()
    {
        var lines = new List<string>
        {
            $"Savegame-Arbeitsbereich: {ResolveWorkspacePath("data\\savegames")}",
            $"Welt: {_selectedSavegame?.WorldName ?? "nicht ausgewählt"}",
            $"Autosave: {_selectedAutosave?.FileName ?? "nicht ausgewählt"}",
            $"Charakter: {(_selectedCharacter is { } character ? FormatCharacterName(character) : "nicht ausgewählt")}"
        };

        if (_selectedCharacter is { } selectedCharacter)
        {
            var entities = new CharacterPayloadService().AnalyzeSavedEntities(
                ResolveWorkspacePath("data\\savegames"), selectedCharacter);
            lines.Add($"EHD0-Entitäten: {entities.Entities.Count} ({entities.Status})");
            lines.Add($"Komponentendefinitionen: {entities.ComponentDefinitions.Count}");
            var inventory = _characterInventory ?? new CharacterInventoryService().Analyze(
                ResolveWorkspacePath("data\\savegames"), selectedCharacter, _itemCatalog);
            lines.Add($"Inventarplätze: {inventory.Slots.Count}");
            lines.Add($"Lokal zugeordnete Items: {inventory.Slots.Count(slot => slot.ItemName is not null)}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private void UpdateContentPageData(string page)
    {
        ContentPageDataPanel.Visibility = _selectedCharacter is not null &&
            (page is "items" or "inventory" or "weapons" or "debug")
            ? Visibility.Visible
            : Visibility.Collapsed;
        if (_selectedCharacter is not { } character)
        {
            ContentPageDataTitle1.Text = "Charakter";
            ContentPageDataValue1.Text = "Kein Charakter ausgewählt";
            ContentPageDataTitle2.Text = string.Empty;
            ContentPageDataValue2.Text = string.Empty;
            ContentPageDataTitle3.Text = string.Empty;
            ContentPageDataValue3.Text = string.Empty;
            return;
        }

        ContentPageDataTitle1.Text = "Ausgewählter Charakter";
        ContentPageDataValue1.Text = FormatCharacterName(character);
        if (page == "items")
        {
            ContentPageDataTitle2.Text = "Itemstrukturen";
            ContentPageDataValue2.Text = character.HasInventoryData
                ? "Nutzungsreihenfolge erkannt"
                : "Nicht bestätigt";
            ContentPageDataTitle3.Text = "Favoriten";
            ContentPageDataValue3.Text = character.HasFavoritesData
                ? "Favoritenreihenfolge erkannt"
                : "Nicht bestätigt";
        }
        else if (page == "inventory")
        {
            ContentPageDataTitle2.Text = "Inventardaten";
            ContentPageDataValue2.Text = character.HasInventoryData ? "Erkannt" : "Nicht bestätigt";
            ContentPageDataTitle3.Text = "Questdaten";
            ContentPageDataValue3.Text = character.HasActiveQuest ? "Aktive Queststruktur erkannt" : "Nicht bestätigt";
        }
        else if (page == "weapons")
        {
            ContentPageDataTitle2.Text = "Ausrüstungsdaten";
            ContentPageDataValue2.Text = character.HasEquipmentData ? "Struktur erkannt" : "Nicht bestätigt";
            ContentPageDataTitle3.Text = "Bearbeitung";
            ContentPageDataValue3.Text = "Noch nicht freigegeben";
        }
        else
        {
            var entities = new CharacterPayloadService().AnalyzeSavedEntities(
                ResolveWorkspacePath("data\\savegames"), character);
            ContentPageDataTitle2.Text = "Entitäten";
            ContentPageDataValue2.Text = $"{entities.Entities.Count} erkannt";
            ContentPageDataTitle3.Text = "Komponenten";
            ContentPageDataValue3.Text = $"{entities.ComponentDefinitions.Count} erkannt";
        }
    }

    private void ItemsListPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (sender is not ListBox listBox ||
            FindVisualChild<ScrollViewer>(listBox) is not { } scrollViewer ||
            scrollViewer.ScrollableHeight <= 0)
        {
            return;
        }

        var nextOffset = scrollViewer.VerticalOffset - e.Delta;
        var clampedOffset = Math.Clamp(nextOffset, 0, scrollViewer.ScrollableHeight);
        if (Math.Abs(clampedOffset - scrollViewer.VerticalOffset) > double.Epsilon)
        {
            scrollViewer.ScrollToVerticalOffset(clampedOffset);
            e.Handled = true;
        }
    }

    private void InventoryGridPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer || scrollViewer.ScrollableHeight <= 0)
            return;

        var nextOffset = scrollViewer.VerticalOffset - e.Delta;
        scrollViewer.ScrollToVerticalOffset(Math.Clamp(nextOffset, 0, scrollViewer.ScrollableHeight));
        e.Handled = true;
    }

    private void InventorySlotClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: InventorySlotDisplay slot })
            return;

        _selectedInventorySlot = slot;
        UpdateInventoryInspector(_currentPage, InventoryGrid.Items.OfType<InventorySlotDisplay>().ToArray());
    }

    private void UpdateInventoryInspector(string page, IReadOnlyList<InventorySlotDisplay> slots)
    {
        var selected = _selectedInventorySlot is { } selectedSlot &&
                       slots.Contains(selectedSlot)
            ? selectedSlot
            : slots.FirstOrDefault(slot => slot.IsOccupied);
        if (selected is null)
        {
            InventoryInspectorName.Text = page == "weapons"
                ? "Keine bestätigte Waffeninstanz"
                : "Keine belegte Iteminstanz";
            InventoryInspectorState.Text = "Wähle eine Inventargruppe oder lade einen Charakter mit gespeicherten Items.";
            InventoryInspectorDetails.Text = "Noch keine Instanzdaten verfügbar.";
            return;
        }

        InventoryInspectorName.Text = selected.ItemName;
        InventoryInspectorState.Text = selected.SlotLabel;
        InventoryInspectorDetails.Text = FormatInspectorDetails(selected);
    }

    private static string? FormatPlayerSlotDetails(
        CharacterInventorySlotObservation slot,
        bool occupied)
    {
        if (!occupied)
            return null;

        var details = new List<string>();
        if (slot.Quantity is { } quantity)
            details.Add($"Menge {quantity}");
        if (!string.IsNullOrWhiteSpace(slot.Category))
            details.Add(ToPlayerCategory(slot.Category));
        if (slot.Instance?.Level is { } level)
            details.Add($"Itemstufe {level}");
        if (slot.Instance?.RarityCode is { } rarity)
            details.Add($"Seltenheitscode {rarity}");
        if (slot.Instance?.BaseDamage is { } damage)
            details.Add($"Schaden {damage}");
        if (slot.Instance?.HasDurability == true)
            details.Add("Haltbarkeit");
        if (slot.Instance?.HasPerks == true)
            details.Add("Perks");
        if (slot.Instance?.HasWeaponGem == true)
            details.Add("Sockel");
        if (slot.Instance?.HasColorPalette == true)
            details.Add("Färbung");

        return details.Count > 0
            ? string.Join(" · ", details)
            : "Gespeichert; Detailwerte sind für dieses Item noch nicht bestätigt.";
    }

    private static string FormatInspectorDetails(InventorySlotDisplay slot)
    {
        var lines = new List<string>();
        if (slot.Quantity is { } quantity)
            lines.Add($"Menge: {quantity}");
        if (!string.IsNullOrWhiteSpace(slot.Category))
            lines.Add($"Kategorie: {ToPlayerCategory(slot.Category)}");
        if (!string.IsNullOrWhiteSpace(slot.Definition?.Description))
            lines.Add($"Beschreibung: {slot.Definition.Description}");
        if (!string.IsNullOrWhiteSpace(slot.Definition?.EquipmentSlot))
            lines.Add($"Ausrüstungsplatz: {ToPlayerCategory(slot.Definition.EquipmentSlot)}");
        if (!string.IsNullOrWhiteSpace(slot.Definition?.Rarity))
            lines.Add($"Seltenheit: {slot.Definition.Rarity}");
        if (!string.IsNullOrWhiteSpace(slot.Definition?.DamageSummary))
            lines.Add($"Schadensverteilung: {slot.Definition.DamageSummary}");
        if (slot.Definition?.Buffs is { Count: > 0 } buffs)
        {
            lines.Add("Spieldaten-Boni/Buffs:");
            lines.AddRange(buffs.Select(buff => $"  - {buff}"));
        }
        if (slot.Definition?.MinLevel is not null || slot.Definition?.MaxLevel is not null)
            lines.Add($"Spielstufe: {slot.Definition.MinLevel?.ToString() ?? "?"}-{slot.Definition.MaxLevel?.ToString() ?? "?"}");
        if (!string.IsNullOrWhiteSpace(slot.Details))
            lines.Add($"Übersicht: {slot.Details}");

        if (slot.Instance is { } instance)
        {
            lines.Add(string.Empty);
            lines.Add("Bestätigte Instanzwerte:");
            if (instance.Level is { } level)
                lines.Add($"  Itemstufe: {level}");
            if (instance.BaseDamage is { } damage)
                lines.Add($"  Basisschaden: {damage}");
            if (instance.RarityCode is { } rarity)
                lines.Add($"  Seltenheitscode: {rarity}");
            lines.Add($"  Iteminstanz: {instance.EntityId}");

            if (instance.Properties is { Count: > 0 } properties)
            {
                lines.Add(string.Empty);
                lines.Add("Ausgelesene Werte aus den Itemkomponenten:");
                foreach (var property in properties)
                {
                    var name = ToPlayerPropertyName(property.Name);
                    lines.Add($"  {name}: {property.Value} [{property.Component}]");
                }
            }

            if (instance.Components is { Count: > 0 } components)
            {
                lines.Add(string.Empty);
                lines.Add($"Komponenten: {string.Join(", ", components.Select(ToPlayerComponentName))}");
            }
        }
        else
        {
            lines.Add(string.Empty);
            lines.Add("Keine verknüpfte Iteminstanz im Savegame bestätigt.");
        }

        return lines.Count > 0
            ? string.Join(Environment.NewLine, lines)
            : "Itemreferenz ist gespeichert; weitere Eigenschaften sind noch nicht bestätigt.";
    }

    private static string ToPlayerPropertyName(string value)
    {
        var knownName = value switch
        {
            "baseDamageUi" => "Basisschaden",
            "itemRarityUi" => "Seltenheit",
            "itemLevelUi" => "Itemstufe",
            "itemId" => "Item-ID",
            _ => null
        };
        if (knownName is not null)
            return knownName;

        var result = Regex.Replace(value, "([a-z])([A-Z])", "$1 $2")
            .Replace('_', ' ')
            .Replace('-', ' ');
        return result.Length == 0 ? value : char.ToUpperInvariant(result[0]) + result[1..];
    }

    private static string ToPlayerComponentName(string value) => value switch
    {
        "ItemState" => "Itemstatus",
        "Durability" => "Haltbarkeit",
        "PerkContainerNew" => "Perks/Boni",
        "WeaponGemSlot" => "Waffensockel",
        "ColorPaletteItem" => "Färbung",
        _ => ToPlayerPropertyName(value)
    };

    private void UpdateLiveDataPanel(string page)
    {
        var visible = page is "items" or "inventory" or "weapons";
        LiveDataPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        if (!visible)
            return;

        if (page == "items")
        {
            LiveDataGlyph.Text = "◇";
            LiveDataTitle.Text = "Lokaler Enshrouded-Itemkatalog";
            LiveDataDescription.Text = "Durchsuche reale Definitionen aus deiner lokalen Spielinstallation.";
            LiveDataValue.Text = _itemCatalog is null ? "–" : _itemCatalog.Definitions.Count.ToString();
            return;
        }

        var inventory = _characterInventory;
        var resolved = inventory?.Slots.Count(slot => slot.ItemName is not null) ?? 0;
        if (page == "inventory")
        {
            LiveDataGlyph.Text = "▤";
            LiveDataTitle.Text = "Gespeichertes Charakterinventar";
            LiveDataDescription.Text = "Jede Kachel entspricht einem direkt gelesenen Slot; Mengen und bestätigte Instanzmerkmale stehen in der Kachel.";
            LiveDataValue.Text = inventory is null ? "–" : $"{resolved}/{inventory.Slots.Count}";
            return;
        }

        var weaponCount = inventory?.Slots.Count(slot =>
            slot.Instance?.BaseDamage is not null || slot.Instance?.HasWeaponGem == true) ?? 0;
        LiveDataGlyph.Text = "⚔";
        LiveDataTitle.Text = "Gespeicherte Waffeninstanzen";
        LiveDataDescription.Text = "Nur Instanzen mit bestätigtem Basisschaden oder Waffensockel werden angezeigt.";
        LiveDataValue.Text = weaponCount.ToString();
    }

    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                return match;
            }

            if (FindVisualChild<T>(child) is { } descendant)
            {
                return descendant;
            }
        }

        return null;
    }

    private void UpdateNavigation(Button? active)
    {
        foreach (var button in new[] { OverviewButton, ItemsButton, InventoryButton, WeaponsButton, DebugButton })
        {
            button.Background = button == active ? new SolidColorBrush(Color.FromRgb(41, 79, 77)) : Brushes.Transparent;
            button.BorderBrush = button == active ? new SolidColorBrush(Color.FromRgb(92, 170, 158)) : Brushes.Transparent;
            button.Foreground = button == active ? Brushes.White : (Brush)FindResource("MutedBrush");
        }
    }

    private void CreateBackupClick(object sender, RoutedEventArgs e)
    {
        var savegamePath = ResolveWorkspacePath("data\\savegames");
        try
        {
            var workspaceRoot = Path.Combine(savegamePath, "workspaces");
            var snapshot = new SavegameWorkspaceService().CreateSnapshot(savegamePath, workspaceRoot);
            FooterStatusTextBlock.Text = $"Sicherung erstellt • {snapshot.Files.Count} Dateien geschützt";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            FooterStatusTextBlock.Text = $"Sicherung nicht erstellt: {ex.Message}";
        }
    }

    private async void ReanalyzeGameDataClick(object sender, RoutedEventArgs e)
    {
        const string installationPath = @"D:\SteamLibrary\steamapps\common\Enshrouded";
        var baselinePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ShroudStudio",
            "game-data-baseline.json");
        SetLoading(true, "Lokale Spieldaten werden auf Änderungen geprüft...");
        try
        {
            var report = await Task.Run(() =>
            {
                var previous = GameDataAnalysisService.ReadBaseline(baselinePath);
                var result = new GameDataAnalysisService().Analyze(installationPath, previous);
                if (result.Status != EvidenceStatus.Unknown)
                    GameDataAnalysisService.WriteBaseline(baselinePath, result);
                return result;
            });

            _itemCatalog = null;
            _characterInventory = null;
            var changeLabel = report.ChangedSources.Count == 0
                ? "keine Änderungen"
                : $"{report.ChangedSources.Count} Änderungen";
            FooterStatusTextBlock.Text =
                $"Spieldaten geprüft • {report.ItemDefinitionCount} Definitionen • {changeLabel}";
            if (_currentPage is "items" or "inventory")
                UpdateItemsList(_currentPage);
        }
        catch (IOException ex)
        {
            FooterStatusTextBlock.Text = $"Spieldaten konnten nicht geprüft werden: {ex.Message}";
        }
        catch (UnauthorizedAccessException ex)
        {
            FooterStatusTextBlock.Text = $"Spieldaten konnten nicht geprüft werden: {ex.Message}";
        }
        finally
        {
            SetLoading(false, string.Empty);
        }
    }

    private void SlotSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsInitialized)
        {
            _selectedSavegame = (SlotComboBox.SelectedItem as ComboBoxItem)?.Tag as SavegameSlot;
            _worldKnowledge = null;
            PopulateAutosaves();
            UpdateSelectedSavegameView();
            RefreshState();
        }
    }

    private void PopulateAutosaves()
    {
        AutosaveComboBox.Items.Clear();
        _selectedAutosave = null;
        if (_selectedSavegame?.Autosaves is not { Count: > 0 } autosaves)
        {
            AutosaveComboBox.Items.Add(new ComboBoxItem
            {
                Content = "Keine Autosaves gefunden",
                IsEnabled = false,
                Style = (Style)FindResource("DarkComboBoxItemStyle")
            });
            return;
        }

        foreach (var autosave in autosaves)
        {
            AutosaveComboBox.Items.Add(new ComboBoxItem
            {
                Content = autosave.IsLatest
                    ? $"Autosave {autosave.Number} • zuletzt"
                    : $"Autosave {autosave.Number}",
                Tag = autosave,
                Style = (Style)FindResource("DarkComboBoxItemStyle")
            });
        }

        var latestIndex = autosaves
            .Select((autosave, index) => (autosave, index))
            .FirstOrDefault(entry => entry.autosave.IsLatest).index;
        AutosaveComboBox.SelectedIndex = latestIndex;
        _selectedAutosave = autosaves[latestIndex];
    }

    private void AutosaveSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsInitialized)
        {
            _selectedAutosave = (AutosaveComboBox.SelectedItem as ComboBoxItem)?.Tag as SavegameAutosave;
            _worldKnowledge = null;
            UpdateSelectedSavegameView();
        }
    }

    private void UpdateSelectedSavegameView()
    {
        if (_selectedSavegame is null)
        {
            return;
        }

        var worldName = _selectedSavegame.WorldName is { Length: > 0 }
            ? _selectedSavegame.WorldName
            : $"Spielstand {_selectedSavegame.SavegameId}";
        var autosave = _selectedAutosave is { } selectedAutosave
            ? $"Autosave {selectedAutosave.Number}"
            : "Autosave unbekannt";
        var savedAt = _selectedSavegame.LatestUnixTime is { } unixTime
            ? DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime().ToString("g")
            : "unbekannt";
        _worldKnowledge ??= _selectedAutosave?.FileName is { } selectedFile
            ? new KnowledgePayloadService().Analyze(
                Path.Combine(ResolveWorkspacePath("data\\savegames"), selectedFile))
            : null;
        var knowledgeSummary = _worldKnowledge is { Status: EvidenceStatus.Observed } knowledge
            ? $" • {knowledge.Entries.Count} Weltwissen-Einträge"
            : string.Empty;
        OverviewSavegameNameTextBlock.Text = $"{worldName} • Slot {_selectedSavegame.SlotNumber}";
        OverviewSavegameInfoTextBlock.Text =
            $"{autosave} • zuletzt gespeichert {savedAt} • {_selectedSavegame.VersionCount} Versionen" +
            (_selectedAutosave?.FileName is { } file ? $" • Datei {file}" : string.Empty) +
            knowledgeSummary;
        OverviewCharacterInfoTextBlock.Text = _selectedCharacter is { } character
        ? $"Charakter-Savegame: {FormatCharacterName(character)}" +
          (character.HasActiveQuest ? " • Questdaten erkannt" : string.Empty) +
          (character.HasInventoryData ? " • Inventardaten erkannt" : string.Empty)
        : "Kein Charakter-Savegame ausgewählt";
        HeaderStatusTextBlock.Text = $"Geladen: {worldName} • {autosave}";
    }

    private void CharacterSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized)
        {
        return;
        }

        _selectedCharacter = (CharacterComboBox.SelectedItem as ComboBoxItem)?.Tag as CharacterSavegameOption;
        _itemReport = null;
        _itemCatalog = null;
        _characterInventory = null;
        UpdateSelectedSavegameView();
        if (_currentPage != "overview")
        {
            UpdateItemsList(_currentPage);
            ContentPageMessage.Text = GetCapabilityMessage(_currentPage, ContentPageMessage.Text);
            ContentPageFacts.Text = GetCapabilityFacts(_currentPage);
            UpdateContentPageData(_currentPage);
            UpdateLiveDataPanel(_currentPage);
        }
    }

    private static string ResolveWorkspacePath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        var currentDirectoryCandidate = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
        if (Directory.Exists(currentDirectoryCandidate))
        {
            return currentDirectoryCandidate;
        }

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var workspaceMarker = Path.Combine(directory.FullName, "ROADMAP.md");
            var workspaceCandidate = Path.Combine(directory.FullName, path);
            if (File.Exists(workspaceMarker) && Directory.Exists(workspaceCandidate))
            {
                return workspaceCandidate;
            }

            directory = directory.Parent;
        }

        return currentDirectoryCandidate;
    }
}
