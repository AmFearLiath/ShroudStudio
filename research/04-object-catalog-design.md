# 04 – Object-Catalog-Design

## Ziel

Der Katalog ist eine evidenzbasierte Registry, kein bloßes ID-Verzeichnis. UI-Filter und Vorschauen dürfen nur bestätigte Eigenschaften versprechen.

## Kernmodell

Ein `CatalogEntry` enthält:

- stabile interne ID und originale Kennung
- Name, Aliase, Kategorie und Varianten
- Quelle, Spielversion, Hash/Extraktionsdatum
- sichtbare Form, Platzierungsart und Transform-Unterstützung
- Fähigkeiten `canSpawn`, `canReplace`, `canPersist`, jeweils mit Evidenzstatus
- Einzelspieler-/Multiplayer-Scope
- bekannte Risiken, Lizenznotiz und Testfallreferenz

Operationen arbeiten mit einem `CapabilityContract`: Voraussetzungen, unterstützte Versionen/Modi, Aktivierung, Deaktivierung, Verifikation, Fehlerzustand und Recovery-Information. `submitted` wird niemals als `succeeded` behandelt.

## Aktualisierung

Katalogimporte werden versioniert und zunächst in einen Quarantänebestand geschrieben. Ein Diff erkennt neue, entfernte und geänderte Kennungen. Erst ein gezielter Test kann `unknown` nach `observed` oder `verified` heben. Fehlende Definitionen werden nicht stillschweigend aus alten Versionen übernommen.

## UI-Folgerung

Suche, Kategorie, Variante und Evidenzstatus sind sichere erste Filter. Vorschauen müssen als Referenzbild gekennzeichnet werden, wenn sie nicht aus verifiziertem Spieldatenbezug stammen.

**Status:** Datenmodell und Aktualisierungsprozess empfohlen; konkrete Katalogeinträge fehlen noch.
