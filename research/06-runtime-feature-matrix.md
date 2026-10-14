# 06 – Runtime-Feature-Matrix

| Kennung | Funktion | Status | Priorität | Hauptnachweis |
|---|---|---|---|---|
| runtime.process-detect | Spielprozess erkennen | PoC vorbereitbar | Jetzt | Prozessname, Datei-/Versionsprüfung |
| runtime.read-only-status | Capability-/Versionsstatus lesen | PoC vorbereitbar | Jetzt | reproduzierbares Log |
| runtime.god-mode | Schaden/Überleben | Unbekannt | Weitere Forschung | Wirkung und Rücksetzung fehlen |
| runtime.infinite-stamina | Ausdauer | Unbekannt | Nach PoC | keine API/Adressstabilität |
| runtime.infinite-mana | Mana | Unbekannt | Nach PoC | keine API/Adressstabilität |
| runtime.flight | Fliegen | Unbekannt | Weitere Forschung | Bewegung und Multiplayer ungeklärt |
| runtime.movement | Geschwindigkeit | Unbekannt | Weitere Forschung | Synchronisation ungeklärt |
| runtime.resources | Ressourcen | Unbekannt | Weitere Forschung | Save/runtime-Trennung |
| runtime.object-operation | Objektoperation | Unbekannt | Nach Prop-PoC | kein Ersatznachweis |

## Aktualisierung 13.10.2026

`runtime.process-detect` ist durch einen laufenden, reagierenden `enshrouded.exe`-Prozess verifiziert. EML-Konfigurationsdatei und Modpakete wurden read-only gefunden. Shroudtopia bestätigt als externe Referenz einen DLL-Mod-Lifecycle und nennt Flug-/Ressourcenbeispiele, dokumentiert aber zugleich fehlende spielbezogene Funktionen im gemeinsamen Kontext. Die bereitgestellte Cheat-Table bestätigt AOB-/Hook-basierte Referenztechnik für Prop-Auswahl und Rotation, nicht jedoch Persistenz oder echten Austausch.

Die Matrix bleibt für alle spielverändernden Funktionen bei `Unbekannt`, bis Wirkung, Rücksetzung und gegebenenfalls Speichern/Neuladen separat gemessen werden.

Für jeden Eintrag gelten Capability-Vertrag, Spielversion, Betriebsart, Aktivierungs-/Deaktivierungsweg, Verifikation, Fehlerzustand und Recovery als Pflichtfelder. „Unbekannt“ bedeutet ausdrücklich nicht „funktioniert nicht“, aber auch nicht „unterstützt“.

**Empfehlung:** zuerst read-only Erkennung und Statusanzeige; keine invasive Funktion als erste Produktfunktion.
