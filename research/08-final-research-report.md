# 08 – Abschlussbericht und Entscheidungsvorlage

## Kurzfazit

ShroudStudio ist als eigenständige Windows/WPF-Suite grundsätzlich realisierbar, aber der aktuelle Workspace enthält noch keine Implementierung. Die Savegames sind proprietäre Binärdateien mit `KSC1`-Headern und JSON-Indizes; ein sicherer Editor ist daraus noch nicht ableitbar. Eine offizielle Runtime-Modifikations-API wurde nicht verifiziert. Runtime-Manipulation und Prop-Swap bleiben daher Forschung, nicht Produktfunktion.

## Entscheidungen

1. **Eigenständige Runtime Engine:** grundsätzlich als Capability-/Adapter-System ja; wirksame Manipulation aktuell nicht bestätigt.
2. **Erster produktiver Kandidat:** read-only Prozess-, Versions- und Capability-Erkennung.
3. **Eigenständiger Prop Swapper:** technisch offen; erst nach isoliertem Ersatz-/Reload-Test.
4. **Nachweislich geeignete Objektklassen:** derzeit keine.
5. **Nicht reguläre Objekte:** ID, Visualisierung oder Erzeugung müssen separat nachgewiesen werden; derzeit keine Freigabe.
6. **Persistenz nach Neustart/Laden:** für keine neue Operation getestet.
7. **Lokal/Multiplayer:** Serverkonfiguration und Dateiverwaltung sind dokumentiert; Spielerzustands- und Objektänderungen, besonders Multiplayer, sind ungeklärt.
8. **Zuverlässige Katalogquellen:** eigene, versionierte und getestete Extrakte/Beobachtungen; vorhandene Projektdateien liefern noch keine Objektliste.
9. **Wiederverwendbare Architektur:** aus dem Bestand nichts; Konzept und Forschungsauftrag sind Anforderungen, keine Codebasis.
10. **Nächste Implementierungen:** Backup/Hash, read-only Inspector, Capability-Modell, Tests.
11. **Wesentliche Risiken:** Spielupdates, proprietäres Format, Serverautorität, unklare Lizenz von Referenzdaten, inkonsistente Savegames.
12. **Vorläufig nicht implementieren:** God Mode, Flug, Ressourcenmanipulation, Prozessinjektion und Prop-Swap ohne PoC.

Der read-only Runtime-PoC wurde inzwischen erweitert: Der laufende, reagierende Enshrouded-Prozess und die lokale EML-Konfiguration wurden erkannt. Damit ist die erste sichere Status-/Kompatibilitätsstufe praktisch nachgewiesen. Zusätzlich sind KSC1-Blob-Tabelle, Blob-Grenzen und CRC-64-Prüfung an einem realen Weltstand reproduzierbar verifiziert. Das ist noch keine semantische Savegame-Dekodierung und ändert nicht die negative Aussage zu unbestätigten Cheats oder Prop-Persistenz.

## Priorisierung

| Kategorie | Funktionen |
|---|---|
| Jetzt implementieren | Inventar-/Backup-Service, read-only KSC1-Analyse, Prozess-/Versionsstatus |
| Nach weiterem PoC | bestätigte Savegame-Felder, regulärer Objekt-Ersatz |
| Weitere Forschung erforderlich | God Mode, Ausdauer, Mana, Flug, Bewegung, nicht reguläre Objekte, Multiplayer |
| Derzeit nicht empfehlenswert | invasive Injection als Produktbasis, ungeprüftes Savegame-Schreiben |

Die Berichte trennen verifizierte lokale Befunde, öffentliche Dokumentation, Hypothesen und unbekannte Ergebnisse. Der Auftrag ist als Research-Baseline aktualisiert; die Definition-of-Done-Punkte für echte Runtime- und Prop-Swap-Tests bleiben ausdrücklich offen und müssen in einer kontrollierten Testumgebung nachgeholt werden.
