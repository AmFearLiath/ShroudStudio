# 03 – Runtime-Machbarkeit

## Bewertete Zugriffswege

| Ansatz | Ergebnis | Risiko |
|---|---|---|
| Offizielle lokale API | Nicht gefunden | niedrig |
| Server-JSON/Serverkonsole | Konfiguration und Monitoring, keine Spielerzustands-API | niedrig |
| Offline-Savegame | grundsätzlich untersuchbar, aber proprietär binär | mittel |
| Prozessspeicher/Injection | technisch denkbar, aber nicht nachgewiesen und updatefragil | hoch |
| Eigene Spielmodifikation | keine offizielle Schnittstelle im Projekt nachgewiesen | unbekannt |

## Proof of Concept

Am 13.10.2026 wurde der erste read-only Nachweis durchgeführt:

- `D:\SteamLibrary\steamapps\common\Enshrouded\enshrouded.exe` wurde als Prozess gefunden.
- Prozess-ID war `43184`; der Prozess reagierte.
- Startzeit war `13.10.2026 05:31:23`.
- `eml.json` war vorhanden und enthielt eine Export-Konfiguration.
- Im `mods`-Ordner lagen zwei Modpakete mit `mod.json`, README und Lua-Dateien.
- Es wurde kein Prozessspeicher geschrieben, kein Code injiziert und kein Savegame verändert.

Damit ist Prozessidentifikation plus lokale Mod-Konfigurationsprüfung reproduzierbar verifiziert. Ein separat benanntes EML-Modul ließ sich in der gekürzten Modulprüfung nicht sicher nachweisen; daraus darf nicht auf einen fehlenden Loader geschlossen werden.

Dieser PoC beweist weiterhin keinen God-Mode-, Flug- oder Ressourcen-Erfolg.

Ein zulässiger nächster PoC ist ein read-only Prozessdetektor mit expliziter Versionsprüfung und einer Capability-Anzeige „Prozess gefunden / Spielmodus unbekannt“. Erst danach wäre ein kontrollierter, reversibler Test mit separater Testwelt vertretbar.

## Bewertung

Eine eigenständige Runtime Engine ist als Adapter-/Capability-Architektur grundsätzlich realisierbar. Eine verlässlich unterstützte Manipulation von Gesundheit, Ausdauer, Mana, Flug oder Bewegung ist derzeit nicht verifiziert. Multiplayer darf nicht als lokal kontrollierbar angenommen werden; Serverautorität und Synchronisation müssen separat getestet werden.

**Status:** Architekturhypothese plausibel; wirksame Runtime-Modifikation unbekannt, nicht implementieren.
