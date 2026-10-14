# 00 – Projektinventur

## Stand

Untersuchung am 13.10.2026 in `F:\ShroudStudio`. Das Verzeichnis ist kein Git-Repository und enthält aktuell keine `.cs`, `.csproj`, `.sln`, `.json`, `.csv`, Test- oder Parserdateien.

Vorhanden sind:

- `docs\forschungsauftrag.md`
- `docs\konzept.md`, `docs\konzept - erweiterung.md`, `docs\ausrichtung.md`
- sieben PNG-UI-Referenzen unter `docs\images`
- 160 Savegame-Dateien unter `data\savegames`
- zwei RAR-Backups

Es gibt damit noch keine implementierte Architektur, keine Bibliotheken, keine Ziel-Framework-Datei und keine wiederverwendbaren Parser. Die im Auftrag genannten Module (`EMS.*`) sind derzeit Konzeptnamen, keine vorhandenen Komponenten.

## Savegame-Befund

Die Dateien gruppieren sich unter anderem nach Welt-IDs `34d85178`, `36d8549e`, `37d85631`, `38d857c4`, `39d85957`, `3ad85aea` und `3bd85c7d`. Jede Gruppe besitzt Versionsdateien, eine `_index`-Datei und häufig `_info`-Dateien. Die Indexdateien sind lesbares JSON mit `latest`, `time` und `deleted`. Welt- und Info-Dateien beginnen mit `KSC1`; die Nutzdaten sind nicht als UTF-8-Text lesbar. Beispielgrößen reichen von wenigen KB bis etwa 13 MB.

Die RAR-Dateien wurden nicht entpackt oder verändert. Es wurde keine Originaldatei überschrieben.

## Lokale Spielinstallation

Unter `D:\SteamLibrary\steamapps\common\Enshrouded` ist eine Installation vorhanden. Festgestellt wurden `enshrouded.exe`, `enshrouded.kfc`, große `enshrouded_*.dat`-Pakete, `enshrouded.log` und `enshrouded_local.json`. Die Pakete sind eine mögliche Quelle für spätere Katalogforschung, aber weder Dateinamen noch IDs beweisen eine platzierbare oder speicherbare Objektdefinition.

## Entscheidungen und Blockaden

1. Es gibt keinen Bestandscode, der erweitert werden kann.
2. Ein erster sinnvoller Meilenstein ist ein read-only Inspector mit Kopie-/Hash-Schutz, nicht ein Editor.
3. Savegame-Schreiben, Prozessspeicherzugriff und Asset-Extraktion benötigen jeweils eigene Tests und dürfen nicht über einen gemeinsamen unvalidierten Schreibpfad laufen.
4. Runtime- und Prop-Swap-Tests konnten in dieser Arbeitsphase nicht als Spieltest durchgeführt werden, weil kein laufender Spielprozess und keine kontrollierte Testwelt vorlagen.

**Status:** verifiziert durch lokale Dateisysteminventur; Runtime-Wirkung und dauerhafte Änderungen unbekannt.
