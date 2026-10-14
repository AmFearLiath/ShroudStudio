# 05 – Prop-Swapper-PoC

## Testfall und Ergebnis

Der geforderte Stuhl-gegen-Zielobjekt-Test wurde in dieser Phase nicht als Spieltest durchgeführt. Es lag kein kontrollierter laufender Testzustand mit beobachtbarem Objekt, Zielobjekt und Speichern/Neuladen vor. Daher sind alle zehn Prüfpunkte (gültige ID, Erzeugung, echter Austausch, Transform, Nebenwirkungen, Darstellung, Nutzung, Persistenz, Invalidierung, Rücknahme) **unbekannt**.

Insbesondere wird kein zusätzlich erzeugtes Objekt als Ersatz gewertet. Ohne Vorher-/Nachher-Snapshot, Objektidentität und Reload-Nachweis wäre ein positives Ergebnis nicht belastbar.

## Cheat-Table als technische Referenz

Die bereitgestellte Datei `data\images\tools\builders_companion\Enshrouded Building Companion Water Update WIP V2.CT` enthält einen Eintrag `Override Placed Prop`. Der Eintrag baut eine Lua-Auswahlliste `PropList` auf, ordnet sichtbaren Namen 32-Byte-Werten zu und schreibt den gewählten Wert in `MeshRepData`. Außerdem enthält die Datei AOB-Scans und Auto-Assembler-Code für den laufenden Prozess sowie separate Steuerungen für Prop-Rotation.

Das verifiziert die beobachtete Referenztechnik: Auswahl und Darstellung werden über Prozess-Hooks bzw. Speicherwerte beeinflusst. Es verifiziert ausdrücklich nicht, dass ein bestehendes Weltobjekt ersetzt wird, dass der Weltzustand geändert wird oder dass die Änderung nach Speichern und Neuladen erhalten bleibt. Die Cheat Engine/Auto-Assembler-Abhängigkeit steht zudem im Widerspruch zur Eigenständigkeitsanforderung und wird nicht übernommen.

## Sicherer PoC-Ablauf

1. Arbeitskopie und Hash des Savegames erstellen.
2. Gewöhnlichen Stuhl in einer isolierten Testzone markieren.
3. Ziel aus einer katalogisierten, zunächst regulären Objektklasse wählen.
4. Vorher-Zustand von Position, Rotation, Identität und Nachbarobjekten erfassen.
5. Operation als transaktionalen Versuch ausführen; Ausgangsobjekt erst nach bestätigter Zielinstanz entfernen.
6. Sichtbarkeit, Kollisions-/Nutzbarkeit und Weltzustand prüfen.
7. Speichern, Prozess beenden, laden und erneut prüfen.
8. Bei jedem Fehler Arbeitskopie zurückspielen und Ergebnis als fehlgeschlagen markieren.

Nicht regulär verfügbare Objekte werden erst nach einem erfolgreichen regulären Testfall untersucht. Eine ID allein ist kein Eignungsnachweis.

**Status:** technisch offen; kein Prop Swapper implementieren, bevor ein echter Ersatz- und Reload-Test möglich ist.
