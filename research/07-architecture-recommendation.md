# 07 – Architektur- und Umsetzungsempfehlung

Da noch kein Bestandscode existiert, wird die Zielstruktur als neue Lösung vorgeschlagen, ohne vorhandene Module zu behaupten:

- `EMS.Shared`: Versionen, Evidenzstatus, Capability-Verträge und Fehlercodes
- `EMS.SaveData`: Kopieren, Hashen, read-only Analyse und später versionierte Parser
- `EMS.ObjectCatalog`: Quellen, Katalogeinträge, Varianten und Evidenz
- `EMS.RuntimeAdapters`: getrennte, explizit versionierte Adapter; zunächst nur Prozess-/Versionsprüfung
- `EMS.RuntimeEngine`: Operationen, Verifikation, Timeout und Statusübergänge
- `EMS.WorldObjectToolkit`: Auswahl und ausschließlich nachgewiesene Objektoperationen
- `EMS.OperationRecovery`: Backup, Journal, Recovery-Plan und Ergebnisnachweis
- `EMS.CompatibilityCenter`: Spielversion, Modus, Voraussetzungen und bekannte Einschränkungen
- `EMS.IngameControlCenter`: WPF-UI, die nur geprüfte Capabilities anbietet

Jede riskante Operation läuft über `prepare -> validate -> execute -> verify -> commit` und bleibt bis zur Verifikation reversibel. Savegame-Parser, Runtime-Adapter und Weltobjektoperationen teilen Modelle, aber nicht ungeprüfte Schreibmechanismen.

## Reihenfolge

1. .NET-10/WPF-Grundgerüst und Tests.
2. Backup-/Hash-Service und KSC1 read-only Inspector.
3. Capability- und Kompatibilitätsmodell.
4. Read-only Prozessdetektor.
5. kontrollierter Savegame-Parser-PoC mit bekannten Testdifferenzen.
6. Stuhl-Test und erst danach Prop-Swap.

**Status:** empfohlenes Architekturmodell; konkrete Spieladapter bleiben unbestätigt.
