# 09 – KSC1-Savegame-Formatanalyse

## Quellen

Die wichtigste technische Referenz ist [enshrouded-save-transfer FORMAT.md](https://github.com/o-shabashov/enshrouded-save-transfer/blob/main/FORMAT.md). Sie beschreibt den KSC1-Container als reverse-engineered Format und nennt verifizierte Parser-/Rebuild-/Character-Transfer-Tests. Ergänzend wurden [Brabb3l/kfc-parser](https://github.com/Brabb3l/kfc-parser), der read-only [EnshroudedSaveExplorer](https://github.com/Fab550010/EnshroudedSaveExplorer) und der [Enshrouded Character Workshop](https://github.com/RechiTaun/enshrouded-character-editor) geprüft.

Der kfc-parser ist laut README ein Parser für KFC-/Descriptor-Dateien und die CLI ist als deprecated bezeichnet. EnshroudedSaveExplorer implementiert laut README einen eigenen read-only Parser und steht unter 0BSD. Der Character Workshop ist MIT-lizenziert und unterstützt bereits validierte Charakter-/Inventaroperationen, warnt aber selbst vor Spielversions- und Cloud-Grenzen. Keine fremden Quelltexte oder Daten werden übernommen.

## Verifiziertes Containerlayout

| Offset | Größe | Bedeutung |
|---|---:|---|
| `0x00` | 4 | ASCII `KSC1` |
| `0x04` | 4 | little-endian Blob-Anzahl |
| `0x08` | 8 | CRC-64 der Blob-Tabelle |
| `0x10` | 8 | CRC-64 der Payloaddaten |
| `0x18` | `12*n` | Tabelle: Owner-ID, 4-Byte-Tag, komprimierte Größe |
| danach | variabel | Payloads, laut Referenz raw-zstd-Frames |

Die Payloadgrenzen sind aus den komprimierten Größen direkt berechenbar. Ein Tag ist nicht automatisch eine editierbare Datenstruktur; die Semantik kommt erst nach sicherer Dekompression und weiterer Feldanalyse.

## Lokaler Reproduktionsnachweis

Für `data\savegames\34d85178` wurden unabhängig vom .NET-Parser geprüft:

- Dateigröße: 3.715 Bytes
- Blob-Anzahl: 8
- Tabellenoffset: `0x18`
- Tabellenlänge: 96 Bytes
- Payloadoffset: `0x78`
- Tabelle-CRC und Payload-CRC stimmen mit den gespeicherten Werten überein, wenn die reflektierte CRC-64-Berechnung mit `init=0xFFFFFFFFFFFFFFFF` und `xorOut=0xFFFFFFFFFFFFFFFF` ausgeführt wird.
- Erkannte Blobs:
  - Owner `0x539`, `EXTS`, 25 Bytes
  - Owner `0x0`, `KNOW`, 60 Bytes
  - Owner `0x0`, `USER`, 280 Bytes
  - Owner `0x539`, `SRSG`, 1087 Bytes
  - Owner `0x0`, `CHNK`, 2002 Bytes
  - Owner `0x0`, `WETR`, 88 Bytes
  - Owner `0x0`, `CSTR`, 34 Bytes
  - Owner `0x0`, `SNAP`, 19 Bytes

Damit sind Containergrenzen, Tabelleninterpretation, Tags und Integritätsprüfung für mindestens einen realen bereitgestellten Weltstand reproduzierbar bestätigt. Der Parser ist read-only.

## Was noch nicht bewiesen ist

- Die internen Strukturen von `CHNK`, `USER`, `CSTR` oder anderen Welt-Blobs sind noch nicht semantisch dekodiert.
- Es gibt noch keinen Nachweis, dass eine Änderung an einem Blob vom Spiel akzeptiert und dauerhaft geladen wird.
- Die Referenz beschreibt Charakter-Blobs wie `COUT`, `CHAR`, `FOWR` und `KNPL`; diese wurden in den aktuell untersuchten Weltdateien nicht erwartet und nicht als fehlend interpretiert.
- Für Änderungen müssen CRCs, Kompression, Slot-Ring, Steam-Cloud-Katalog und spielinterne Validierung gemeinsam berücksichtigt werden.

## Umsetzungsempfehlung

Als nächstes sollte ein isolierter zstd-Read-Service für einzelne Payloads ergänzt werden. Er muss Dekompression, unkomprimierte Hashes und Größen protokollieren, aber keine Repack-/Schreibfunktion enthalten. Erst wenn mindestens ein Blob über mehrere Versionen strukturell korreliert werden kann, ist ein Feldparser für Inventar oder Weltobjekte vertretbar.

## Dekompressionsnachweis

ShroudStudio enthält inzwischen einen isolierten `Ksc1PayloadReader`. Er akzeptiert nur CRC-validierte Container, liest die deklarierte Payloadgrenze, erkennt zstd-Frames und liefert unkomprimierte Größe sowie SHA-256 zurück. Der Integrationstest hat den bereitgestellten Weltstand `34d85178` mit allen 8 Blobs erfolgreich dekomprimiert. Dabei wurden keine Savegame-Dateien geschrieben.

Das Ergebnis ist ein wichtiger Fortschritt, aber noch kein Inventar- oder Weltobjektparser: Die unkomprimierten Bytes sind nun zugänglich, ihre Felder und Referenzen müssen erst durch Vergleiche mehrerer Versionen und gegen kontrollierte Ingame-Aktionen zugeordnet werden.

## Charakterdaten: BDB1-Hinweise und lokaler Nachweis

Der öffentlich einsehbare [Enshrouded Character Workshop](https://github.com/RechiTaun/enshrouded-character-editor)
behandelt dekomprimierte `CHAR`-Blobs als `BDB1`-Objektarchive. Diese Archive
enthalten getrennte Tabellen für Node-Typen, 32-Bit-Werte, einen String-Pool,
Objekt-Key-Hashes und Objekt-Links. Feldnamen werden über FNV-1a-Hashes
aufgelöst. Als konkrete Kandidaten werden unter anderem Root-Felder `name`
(UTF-8-String) und `level` (UInt32) beschrieben.

ShroudStudio übernimmt keinen fremden Quellcode. Der eigene read-only
`BdbCharacterAnalyzer` validiert die BDB1-Grenzen, referenzierten Nodes,
String-Referenzen und Objektlinks. Der `CharacterPayloadService` folgt dem
aktiven Ring-Slot aus `characters-index` und untersucht nur dessen `CHAR`-Blobs.
Gegen die bereitgestellten Charakterdaten werden mehrere `CHAR`-Blobs mit
BDB1-Node-/Objektstruktur, beobachteten Feldern und Name-/Level-Kandidaten
erkannt. Unbekannte Hash-Felder bleiben ausdrücklich unklassifiziert.

Das ist ein reproduzierbarer Struktur- und Feldnachweis, aber noch keine
Schreibfreigabe. Inventar-, Skill-, Quest- und Charakter-ID-Felder müssen über
kontrollierte Vorher-/Nachher-Savegames und Konsistenzprüfungen verifiziert
werden. Beim Charaktertransfer müssen laut Referenz insbesondere die
Charakter-ID und abhängige Referenzen konsistent bleiben.

**Status:** KSC1-Containerlayout, CRC-Verifikation, zstd-Dekompression und
CHAR/BDB1-Strukturinspektion read-only verifiziert; Welt-Payloadsemantik und
Schreibbarkeit bleiben offen.

## Vollständige Workspace-Analyse

Die bereitgestellten Backups wurden über alle vorhandenen Welt- und
Charakterversionen read-only geprüft:

- 78 KSC1-Container und 688 Blobs wurden analysiert.
- Tabellen- und Payload-CRC waren in allen geprüften Containern gültig.
- Alle erkannten Payloads konnten als zstd-Frames gelesen werden.
- Die Weltblobs treten versionabhängig als `EXTS`, `KNOW`, `USER`, `SRSG`,
  `CHNK`, `WETR`, `CSTR` und `SNAP` auf.
- Charakterdateien enthalten je nach Version zusätzlich `COUT`, `CHAR`,
  `DC8ED5F0`, `FOWR`, `KNPL` und weitere zustandsabhängige Blobs.

Die Weltname-Auflösung entspricht nun dem Verfahren aus ShroudKeeper:
`_info-index` bestimmt die aktive Datei; anschließend werden zstd-Frames
innerhalb der gesamten Info-Datei gesucht und deren Strings nach
technischen Preset-/Enum-Werten gefiltert. Für die vorhandenen Backups wurden
unter anderem folgende Namen reproduzierbar ermittelt:

| ID | Weltname |
|---|---|
| `3ad85aea` | Dungeon World |
| `3bd85c7d` | Enchanted Diadwyn |
| `38d857c4` | Eigenes Lager |
| `39d85957` | Solo bauen |
| `36d8549e` | Solo Run |
| `37d85631` | Testwelt |
| `34d85178` | Neustart |

### Charakterdateien: wichtige Korrektur

Eine einzelne `characters[-N]`-Datei ist kein einzelnes Charakterobjekt.
Sie kann mehrere `CHAR`-Blobs enthalten. In den bereitgestellten Daten wurden
beispielsweise innerhalb eines Containers mehrere plausible Objekte mit
unterschiedlichen Namen und Stufen beobachtet:

- `Liath`, Stufe 26
- `Liath`, Stufe 45
- `Liath`, Stufe 2
- `Kaerwen`, Stufe 22

Die bisherige Anwendung wählte das erste Objekt mit Name/Stufe. Das ist nicht
semantisch belastbar und erklärt die falschen Charakteranzeigen. Künftig muss
die Auswahl über Charakter-ID/Owner-Referenzen und den aktiven Datensatz
erfolgen; ein bloßes `FirstOrDefault` über alle `CHAR`-Blobs ist nicht
zulässig.

### Weltfortschritt und Questdaten

In Weltpayloads wurden spielbezogene Strings wie `Altar Upgrade^4`,
`Mega Store`, `Gem Forge`, `Lake Liath` und `Liath` beobachtet. Diese Strings
belegen Inhalte, aber noch keine Feldzuordnung. Insbesondere ist
`Altar Upgrade^4` kein ausreichender Nachweis für die aktuelle globale
Flammenstufe oder die Anzahl benannter Flammenaltäre.

Im Charakter-BDB1 ist `activeQuestId` als Feld vorhanden, dessen
Objekt-/Referenzinhalt aber noch nicht auf eine Questdefinition abgebildet
werden kann. Die extern dokumentierten Queststrukturen stammen aus statischen
KFC-Ressourcen und dürfen nicht direkt als Savegame-Fortschritt interpretiert
werden.

### Schreibfähigkeit

Als mögliche Kandidaten für spätere, isolierte Tests wurden `name`, `level`,
`activeQuestId`, `items`, `setup` sowie Welt-`USER`-Felder erkannt. Keines
davon ist derzeit für produktives Schreiben freigegeben. Vor einem Schreibpfad
sind vollständige Ring-Backups, atomarer Austausch, CRC-/zstd-Rebuild,
erneutes Einlesen und eine separate Testwelt erforderlich. Unbekannte
BDB1-Typen, Links und Objektarrays müssen unverändert erhalten bleiben.

## Vergleich der vorhandenen Parser- und Vergleichstools

Die relevanten Projekte arbeiten auf unterschiedlichen Ebenen und ersetzen
einander nicht:

- `enshrouded-save-transfer` liest und baut KSC1-Container generisch. Es
  tauscht Blobs und Owner-IDs, interpretiert aber keine Items.
- `EnshroudedSaveExplorer` ist read-only und konzentriert sich auf Welt-,
  Journal-, Quest-, Lore- und Lokalisierungsdaten. Es liefert keine fertige
  Inventar-Item-Tabelle.
- Der `enshrouded-character-editor` dekodiert CHAR-Daten deutlich tiefer:
  BDB1 enthält einen Objektgraphen, dessen `data`-Bereich verschachtelte
  `EHD0`-/`ESC2`-Entity- und `CTCB`-Komponentenschemas enthalten kann.
  Inventarobjekte werden dort über Komponenten wie `ItemState`, `Level`,
  `OwnerRelationship`, `PerkContainerNew`, `UsedItem` und
  `StaticTransform` beschrieben.
- `kfc-parser` verarbeitet statische `.kfc`-/`.kfc_resources`-Daten und
  Reflection-Metadaten, nicht direkt die Savegame-Payloads. Diese Daten können
  später zur Auflösung von Item-IDs und Anzeigenamen aus der lokalen
  Installation dienen; sie dürfen nicht ungeprüft als Savegamewerte
  interpretiert werden.

Damit ist die nächste Item-Stufe nicht ein größerer Stringfilter. Sie muss den
BDB1-Objektgraphen bis in `EHD0`/`ESC2`/`CTCB` verfolgen, Komponenten anhand
ihres Schemas auswerten und erst danach lokale statische Definitionen für
spielerfreundliche Itemnamen verwenden. Bis diese Kette verifiziert ist,
werden numerische Referenzen und unbekannte Komponenten getrennt angezeigt.

### Lokale Itemdefinitionen und Spieldatenexporte

Die lokale Installation enthält neben `enshrouded.kfc` und
`enshrouded.kfc_resources` bereits erzeugte, read-only nutzbare Exporte unter
`export\`. Besonders relevant sind:

- `mapping_discovery\item_index.json` mit `item_id`, `debug_name`, GUID und
  Ressourcentyp;
- `structured_dump\items_iteminfo.json` mit derselben Item-ID-/Namensbeziehung;
- `weapons_dump\weapons_unified.json` mit Item-ID, internem Namen, Kategorie,
  Seltenheit, Ausrüstungsslot und Levelbereich.

ShroudStudio liest diese JSON-Deskriptoren nur aus dem lokalen
Installationspfad. Eine Itemdefinition wird nur bei einer eindeutigen
Item-ID-/Namensbeziehung übernommen. Die vorhandenen `debug_name`-Werte sind
offizielle interne Spieldaten, aber nicht automatisch lokalisierte deutsche
Anzeigenamen; sie werden daher als bestätigte lokale Definition mit sichtbarem
Zuordnungsstatus behandelt. Nicht aufgelöste Savegame-Referenzen bleiben
ausdrücklich unbekannt.

Die reale Analyse des aktiven Charaktercontainers zeigt derzeit, dass
`itemLastUsageOrders` und `itemFavoritesOrders` als BDB1-Objekt-/Arrayknoten
vorliegen. Die bisherige Extraktion konnte daraus nur technische
`node:<index>`-Platzhalter erzeugen. Diese Platzhalter werden nicht mehr als
Items in der Spieleroberfläche angezeigt. Erst die noch ausstehende Auflösung
der BDB1-Array- und Entitytabellen darf konkrete Iteminstanzen erzeugen und
anschließend mit dem lokalen Katalog verbinden.

Die Containeranalyse erfasst inzwischen die beobachteten Wurzeln
`itemLastUsageOrders` und `itemFavoritesOrders` sowie deren Untercontainer
`blueprintMaterialsRoof`, `terrainMaterials` und
`blueprintMaterialsDefault`. Diese Einträge werden als Containerstatus mit
BDB1-Arrayreferenz geführt, nicht als erfundene Itemnamen. Die nächste
technische Aufgabe ist die reproduzierbare Dekodierung der Arrayelemente und
ihrer EHD0-/ESC2-/CTCB-Referenzen.

Für die weitere Analyse besitzt ShroudStudio nun einen versionierten
read-only-JSON-Analyseexport. Er enthält Charakterauswahl, BDB1-Feldbeobachtungen,
erkannte Container, Itemkandidaten, lokale Definitionen und Evidenzstatus. Der
Export wird außerhalb des Savegame-Verzeichnisses geschrieben und prüft per
Regressionstest, dass die Quelldateien unverändert bleiben. Er ist bewusst
kein Rückschreibformat: unbekannte Bytes, Arraysemantik und Referenzen werden
nicht durch vermeintliche JSON-Werte ersetzt.

### Validierte Entitäts- und Inventarkette

Die BDB1-`data`-Referenz lässt sich für die bereitgestellten Charakterdaten
als Byte-Array dekodieren. Dieses Byte-Array enthält ein `EHD0`-Archiv mit
`ESC2`-Komponentendefinitionen und eingebetteten `CTCB`-Schemas. ShroudStudio
liest diese Schichten read-only mit Grenz-, Größen- und Schema-Prüfungen.

Dabei wurden echte `Inventory`-Komponenten bestätigt. Ihr `slots`-Feld ist ein
statisches Array aus acht Strukturen; jede Struktur enthält eine 32-Bit-
Itemreferenz. Die aktiven Backups enthalten mehrere solche
Inventarcontainer. 78 nichtleere Referenzen korrelieren direkt mit
numerischen Item-IDs im lokalen Spieldatenexport. Der Spieler sieht daher im
Inventargrid bestätigte lokale Namen, sofern eine Definition existiert. Das
verschachtelte Feld `Inventory.slots[].data.count` ist ein benanntes
32-Bit-Feld und wird als Menge angezeigt.

`InventorySetup.linksEntities` gruppiert die zugehörigen Inventory-Entitäten
und wird für die Anzeige als zusammenhängende, neutrale
`Inventargruppe n` verwendet. Die Containernamen (Rucksack, einzelne
Aktionsleisten, Ausrüstung oder Kiste) sind daraus noch nicht semantisch
beweisbar.

`Inventory.slots[].data.pide` ist bei nichtleeren Slots eine Entitätsreferenz,
wenn die referenzierte EHD0-Entität existiert und ihr
`ItemState.itemId` exakt der Slot-Itemreferenz entspricht. Nur bei diesem
zusätzlichen Abgleich erzeugt ShroudStudio eine Iteminstanz. Die so
korrelierten Entitäten enthalten beobachtete Komponenten `Level`,
`Durability`, `PerkContainerNew`, `WeaponGemSlot` und `ColorPaletteItem`.

Die UI liest für verknüpfte Iteminstanzen inzwischen nicht nur die Existenz
dieser Komponenten. Alle skalar lesbaren Schemafelder (1, 2, 4 oder 8 Byte)
werden mit Komponente, Feldpfad, Wert und Byteoffset als
`ItemPropertyObservation` erfasst. Verschachtelte Arrays werden dabei mit
ihrem Elementindex aufgelöst. Dadurch bleiben tatsächliche Savegamewerte
sichtbar, auch wenn ein Feld semantisch noch nicht als Perk-, Gem- oder
Haltbarkeitswert klassifiziert werden kann. Nicht numerische oder noch
unbekannte Payloads werden weiterhin nicht als erfundene Spielerwerte
ausgegeben.

### Kontrollierte Before-/After-Analyse

Für den nächsten Entschlüsselungsschritt besitzt ShroudStudio nun einen
semantischen Diff-Service. Er vergleicht zwei bereits dekodierte EHD0-Archive
nach Entitäts-ID und Komponentennamen. Änderungen werden bis auf einzelne
Schemafelder und verschachtelte Arraypfade zurückgeführt und enthalten jeweils
Vorherwert, Nachherwert, Änderungstyp und Evidenzstatus. Komponenten ohne
bekanntes Schema werden trotzdem als hinzugefügt, entfernt oder geändert
gemeldet; ihre unbekannten Bytes werden nicht verworfen.

Der zugehörige JSON-Export darf nicht in einem Savegame-Verzeichnis liegen.
Damit können kontrollierte Experimente dokumentiert werden, etwa:

1. unveränderten Charakter sichern;
2. genau ein Item verschieben, ausrüsten oder verändern;
3. zweiten Stand sichern;
4. semantischen Diff exportieren;
5. nur Felder mit stabiler Änderung als neue Bedeutung klassifizieren.

Der Diff ist ausdrücklich noch keine automatische Bedeutungsbehauptung. Er
liefert die Evidenz, aus der Feldbedeutungen über mehrere unabhängige
Vorher-/Nachher-Paare abgeleitet werden können.

### Kontrollvergleich `characters-8` / `characters-9`

Die bereitgestellten Stände wurden als derselbe spielbare Charakter
(`Liath`, Stufe 45) erkannt. Ein Vergleich ausschließlich über EHD0-
Entity-IDs ist dabei irreführend, weil das Spiel beim Speichern große Teile
der Entitäts-IDs neu vergeben kann. Der zusätzliche Inventar-Diff vergleicht
deshalb die stabile logische Adresse aus Containername und Slotindex.

Für diesen Vergleich wurden genau zwei Änderungen erkannt:

| Container | Slot | Vorher | Nachher | Menge |
|---|---:|---|---|---:|
| Inventargruppe 1 | 39 | `Armor_T1_Ranger_Boots_Ministory_12` (3888365414) | `Armor_T5_Mage_Healer_Boots` (2100904811) | 1 → 1 |
| Inventargruppe 1 | 60 | `Armor_T5_Mage_Healer_Boots` (2100904811) | `Armor_T1_Ranger_Boots_Ministory_12` (3888365414) | 1 → 1 |

Damit ist der vom Benutzer beschriebene Wechsel als **Tausch zweier
Inventar-/Ausrüstungsreferenzen** nachgewiesen. Die internen Namen passen zu
den beschriebenen Stiefeln; offizielle lokalisierte Anzeigenamen sind wegen
der weiterhin leeren lokalen Lokalisierungsmap noch nicht beweisbar.

### Ausrüstung und Lokalisierung

Die beiden neuen Bootsdefinitionen sind im lokalen `item_index.json` mit
GUID und numerischer ID vorhanden. Die Weapon-/Item-Exporte liefern für diese
Armor-Einträge jedoch keine befüllten `name_text`- oder
`description_text`-Felder. Der laufende Lokalisierungsexport erkennt zwar
zwölf mögliche Ressourcenklassen, findet darin aber null GUID-zu-Text-Paare.
ShroudStudio ergänzt deshalb nur nachweisbare Metadaten aus den internen
Descriptors, etwa Kategorie `Rüstung` und Equipmentplatz `Stiefel`; offizielle
Texte werden nicht aus internen Namen erfunden.

Die Oberfläche besitzt nun eine spielähnliche Ausrüstungsanordnung mit
Kopf-, Brust-, Handschuh-, Stiefel-, Haupt- und Nebenhand-, Rücken- und
Ringplatz. Ein Toggle trennt die Darstellung der effektiv erkannten
Ausrüstung vom Prestige-Modus. Für den Prestige-Modus werden derzeit bewusst
keine effektiven Items dupliziert: In den untersuchten Daten ist noch keine
separate Prestige-Referenz bestätigt. Die mittlere Charakterfläche ist als
Liveansicht vorbereitet, enthält aber noch kein gerendertes 3D-Modell.
Die Oberfläche zeigt deshalb ausschließlich deren bestätigte Präsenz sowie
die benannten `ItemState`-Felder Itemstufe und Basisschaden. Namen einzelner
Perks, Edelsteine, Haltbarkeitswerte, Seltenheitscodes sowie die
Ausrüstungszuordnung werden erst nach unabhängiger Payload-Validierung
dargestellt.

### Weltwissen und Lokalisierung

Weltcontainer enthalten einen `KNOW`-Blob mit einer ID-/Wert-Tabelle. Im
Workspace wurden zwei Headerlayouts beobachtet: eine kompakte Version mit
`version, entryCount` und eine ältere Variante mit zusätzlichem
Zwischenwort. Beide werden nur bei exakt passender deklarierter Länge
akzeptiert. Die Eintragsbedeutung bleibt ohne lokale Definition offen; eine
ID wird nicht automatisch als Quest, Rezept oder Fortschritt bezeichnet.

Lokalisierte Itemtexte liegen nicht im Savegame. Der Itemkatalog verwendet
optional `export\localization_dump\localization_map.json` aus der lokalen
Installation, falls dort eine validierte GUID-zu-Text-Zuordnung vorhanden
ist. Ohne diese Map zeigt die Anwendung einen bestätigten internen
Spieldatennamen, niemals eine erratene Übersetzung.

Die Spieldatenanalyse erzeugt für `enshrouded.kfc`,
`enshrouded.kfc_resources` und die relevanten lokalen JSON-Exporte
SHA-256-Fingerprints. Bei einer erneuten Analyse werden geänderte Quellen
explizit gemeldet und der Katalog erneut aufgebaut. Damit führen reguläre
Datenänderungen nach Spielupdates nicht zu stillschweigend weiterverwendeten
Definitionen.

### Vergleichswerkzeuge und Schreibfreigabe

Die Recherche zu Enshrouded Character Workshop und EnshroudedSaveExplorer
bestätigt die verwendete Kette sowie folgende Schreibschutzregeln:
atomare Ersetzung erst bei beendetem Spiel, vollständige Ring-Backups,
Steam-Cloud-Katalogprüfung, Erhalt unveränderter komprimierter Blobs,
CRC-/Kompressions- und Reparse-Roundtrip sowie explizite
Ingame-Akzeptanzprüfung. Weltobjekte, Flammenaltäre und eine separate
Aktionsleisten-Semantik sind in den untersuchten Quellen nicht vollständig
dokumentiert und bleiben deshalb read-only bis zur eigenen Validierung.

Verwendete unabhängige Referenzen:

- [Enshrouded Character Workshop](https://github.com/RechiTaun/enshrouded-character-editor)
  für BDB1-, EHD0-, ESC2-, CTCB- und Roundtrip-Sicherheitsbefunde;
- [EnshroudedSaveExplorer](https://github.com/Fab550010/EnshroudedSaveExplorer)
  für KSC1-, KNOW-, Journal- und Lokalisierungsbefunde;
- [kfc-parser](https://github.com/Brabb3l/kfc-parser) als Referenz für die
  lokale KFC-/Ressourcenanalyse.

Die Implementierung in ShroudStudio ist eigenständig; weder fremder
Quellcode noch Spieldaten werden in das Projekt übernommen.

Die Spieleroberfläche besitzt nun eine Container-Auswahl sowie getrennte
Inventar- und Equipment-Grids. Die Grids verwenden ausschließlich bestätigte
`ItemCandidate`-Instanzen; solange der aktive Charakter nur BDB1-Arrayknoten
und keine konkreten Instanzen liefert, bleiben die Plätze leer und der Status
weist auf die noch fehlende Arrayauflösung hin. Damit werden keine Container-
oder Node-Referenzen fälschlich als Gegenstände angezeigt.
