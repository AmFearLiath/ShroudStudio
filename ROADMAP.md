# ShroudStudio Roadmap

Diese Roadmap wird während der Entwicklung fortgeschrieben. Ein Punkt wird erst abgehakt, wenn die zugehörige Änderung validiert wurde.

## Phase 0 – Forschung und Sicherheitsrahmen

- [x] Projekt- und Datenquellen inventarisieren
- [x] Referenzprojekte und Cheat-Table als technische Referenzen dokumentieren
- [x] Runtime-Prozess und EML read-only nachweisen
- [x] Risiken, Lizenzgrenzen und nicht verifizierte Funktionen dokumentieren

## Phase 1 – Sichere technische Basis

- [x] Roadmap und abhängige Arbeitspakete anlegen
- [x] .NET-10-/WPF-Solution anlegen
- [x] Gemeinsames Capability- und Ergebnisstatusmodell anlegen
- [x] Read-only Enshrouded-/EML-/Savegame-Inventur implementieren
- [x] Backup- und SHA-256-Hash-Schutz implementieren
- [x] Unit-Tests für Dateierkennung, Header und Sicherheitsgrenzen ergänzen
- [x] UI-Zustandsprüfung für laufendes Spiel und ausgewählten Savegame-Arbeitsbereich ergänzen
- [x] Build und Tests erfolgreich ausführen

## Phase 2 – Savegame Data Engine

- [x] KSC1-Header und Indexdateien versioniert analysieren
- [x] Byte-Diff- und Snapshot-Bericht implementieren
- [x] Read-only KSC1-Strukturinspektor mit beobachteten Markern implementieren
- [x] KSC1-Container-Tabelle, Blob-Grenzen und CRC-64 read-only validieren
- [x] Ersten bekannten Savegame-Datensatz reproduzierbar dekodieren (CHAR/BDB1 read-only)
- [x] Zstd-Payloads read-only dekomprimieren und Größen/Hashes protokollieren
- [x] Charakter-`CHAR`-Blobs als BDB1-Objektarchive strukturell inspizieren
- [x] Aktiven Charakter-Slot über `characters-index` read-only analysieren
- [x] Alle bereitgestellten Welt-/Charaktercontainer read-only validieren und Blob-Tags inventarisieren
- [x] Mehrere CHAR-Objekte pro Charakterdatei erkennen und die bisherige First-Match-Annahme verwerfen
- [ ] Schreibpfad erst nach validiertem Parser und Recovery-Test freigeben

## Phase 3 – Runtime Engine

- [ ] Read-only Prozess-/Versionsadapter implementieren
- [ ] EML-/Mod-Konfiguration und Capability-Verträge anzeigen
- [ ] Reversible Runtime-PoCs mit separater Testwelt durchführen
- [ ] Einzelspieler-/Multiplayer-Grenzen getrennt verifizieren

## Phase 4 – Object Catalog und World Toolkit

- [ ] Evidenzbasiertes Katalogschema implementieren
- [ ] Cheat-Table-IDs nur als nicht verifizierte Referenzdaten importieren
- [ ] Reguläres Objekt als kontrollierten Prop-Swap-Testfall untersuchen
- [ ] Persistenz, Nutzbarkeit, Nebenwirkungen und Recovery nachweisen
- [ ] Nicht reguläre Objekte erst nach erfolgreichem Basistest untersuchen

## Phase 5 – Produktreife

- [x] Spielerorientiertes WPF-Designsystem mit Logo, Navigation, Statusleiste und Animationen aufbauen
- [x] Maximiertes Premium-Layout mit Glasflächen, Tiefenwirkung und animierter Ambient-Beleuchtung ergänzen
- [x] Dunkles Neon-Theme mit eigener Dropdown-Darstellung und sichtbaren Leuchteffekten nachschärfen
- [x] Workspace-Backups als feste Entwicklungsquelle verwenden und erkannte Spielstände im UI auswählen lassen
- [x] Bekannte Savegame-Slots und rotierende Autosaves im UI spielerfreundlich zuordnen
- [x] Weltname und Info-Datei-Daten aus dem vorhandenen Shroudkeeper-Verfahren übernehmen
- [x] Abhängige Slot-/Autosave-Auswahl und sichtbaren Ladezustand integrieren
- [x] Welt-/Autosave-Auswahl in die Übersicht verlagern und Charakter-Savegames separat auswählbar machen
- [x] Separate Spielstände-Navigation entfernen und Savegame-Auswahl auf die Übersicht konzentrieren
- [x] Technische Diagnoseaktionen aus der regulären Spieleroberfläche entfernen
- [ ] WPF-Arbeitsbereiche nach bestätigten Spieler-Capabilities mit echten Daten füllen
- [x] Bestätigte Charakter-Capabilities und erkannte Inventar-/Queststrukturen in den Spielerbereichen anzeigen
- [x] Erste spielerorientierte Item-Ansicht mit erkannter Item-/Nutzungsstruktur aufbauen
- [x] Begrenzte Listenbereiche mit eigenem Mausrad-Scrolling und sichtbaren Itemkarten ergänzen
- [x] Verschachtelte BDB1-Itemkandidaten mit EHD0/ESC2/CTCB-Komponenten read-only erfassen und unbekannte Felder markieren
- [x] Lokale Itemexporte mit Item-ID, Namen, Kategorie, Seltenheit, Slot und Levelbereich read-only korrelieren
- [x] Strukturelle BDB1-`node:*`-Platzhalter aus der Spieleransicht entfernen und nicht als Items ausgeben
- [x] Erkannte Item-Container und ihre BDB1-Arrayknoten read-only als Containerstatus erfassen
- [x] Verlustbewussten JSON-Analyseexport für Charakter, Container, Items und lokale Definitionen ergänzen
- [x] Container-Auswahl sowie Inventar-/Equipment-Grid für bestätigte Iteminstanzen vorbereiten
- [x] BDB1-`data`-Array sowie eingebettete EHD0-/ESC2-/CTCB-Entitäts- und Komponentenschemas read-only dekodieren
- [x] Gespeicherte Inventory-Slots gegen lokale Item-IDs korrelieren und bestätigte Namen im Grid anzeigen
- [x] Bestätigte Inventarmengen aus `Inventory.slots[].data.count` auslesen und anzeigen
- [x] Versionierte KNOW-Tabellen aus Welt-Savegames read-only auswerten
- [x] Optionale GUID-zu-Text-Lokalisierung aus lokalem Laufzeitexport in den Itemkatalog integrieren
- [x] Lokale Spieldatendefinitionen per Fingerprint auf Änderungen prüfen und Katalog erneut analysieren
- [x] Diagnosebereich für geprüfte Savegame-, Entitäts-, Inventar- und Katalogbefunde ergänzen
- [x] Inventarslots über `pide` mit verifizierten ItemState-Entitäten korrelieren
- [x] Instanzmerkmale für Itemstufe, Basisschaden, Haltbarkeit, Perks, Sockel und Färbung read-only anzeigen
- [x] Inventargruppen im UI auswählbar machen und den Grid-Inhalt nach Auswahl neu aufbauen
- [x] Interne Itembezeichner in der Spieleransicht lesbar aufbereiten und technische Katalogschlüssel ausblenden
- [x] Skalarwerte und verschachtelte Felder verknüpfter Itemkomponenten im Instanzinspektor anzeigen
- [x] Semantische Before-/After-Diffs für EHD0-Entities, Komponenten und Schemafelder implementieren
- [x] Verlustbewusste JSON-Exporte für semantische Savegame-Diffs absichern
- [x] Kontrollvergleich `characters-8`/`characters-9` mit logischer Inventarslot-Auflösung durchführen
- [x] Spielähnliches Ausrüstungs-Slotlayout mit effektivem/Prestige-Toggle vorbereiten
- [x] Lokale Armor-Descriptors um nachweisbare Kategorie- und Equipmentplatz-Metadaten ergänzen
- [ ] BDB1-Arrayknoten von `itemLastUsageOrders` und `itemFavoritesOrders` bis zu konkreten Iteminstanzen auflösen
- [ ] Inventory-Kategorien semantisch als Rucksack, Aktionsleisten, Ausrüstung und Kisten klassifizieren
- [ ] ItemState, Menge, Perks, Sockel und Ausrüstungskomponenten per Versionsvergleich validieren
- [x] Welt-Savegames und globale Charakter-Savegames getrennt auswählen und auswerten
- [ ] Verifizierte Weltfortschritt-Daten wie Flammenaltäre, Flammenstufe und aktive Quest auslesen
- [ ] Aktiven Charakter über ID-/Owner-Referenzen statt über das erste gefundene CHAR-Objekt bestimmen
- [ ] Compatibility Center und Operation Journal integrieren
- [ ] Update-/Versionsregressionstests einrichten
- [ ] Nur nach Nachweisen produktive Funktionen freigeben
