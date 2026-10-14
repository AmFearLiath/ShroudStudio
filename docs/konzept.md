# Enshrouded Modding Suite
## Verbindliches Architektur- und Entwicklungskonzept

**Dokumenttyp:** Masterkonzept und Entwicklungsgrundlage  
**Version:** 1.0 – Architekturentwurf  
**Zielplattform:** Windows 10/11  
**Technologie:** C#, .NET 10, WPF  
**Projektstatus:** Technische Konzeption und schrittweise Implementierung  
**Leitprinzip:** Datengetrieben, modular, erweiterbar, mehrsprachig und nicht destruktiv

---

# 1. Projektvision und Zielsetzung

## 1.1 Vision

Die Enshrouded Modding Suite ist eine modulare Desktop-Anwendung zur Untersuchung, Analyse, Darstellung und kontrollierten Bearbeitung von Enshrouded-Spiel- und Savegame-Daten.

Das Tool soll sich nicht auf die derzeit bekannten Items, Datenstrukturen, Dateiformate oder Spielversionen beschränken. Stattdessen wird eine generische technische Grundlage geschaffen, die neue oder veränderte Datenstrukturen erkennen, beschreiben, validieren und – soweit technisch sicher möglich – ohne Anpassungen am Anwendungscode verarbeiten kann.

Die Software soll langfristig als zentrale Plattform für verschiedene Funktionen dienen:

- Savegame Explorer und Savegame Editor
- Item Browser und Item Editor
- Inventar- und Containerverwaltung
- Analyse von Spielressourcen und Spieldaten
- Waffen- und Ausrüstungseditor
- Datenbank für Items, Kategorien, Eigenschaften und Referenzen
- Strukturanalyse und Versionsvergleich
- Diagnose unbekannter Daten und Formatänderungen
- Spätere Erweiterungen durch zusätzliche Module und Werkzeuge

Diese Funktionen werden nicht als voneinander unabhängige Anwendungen entwickelt. Sie greifen auf gemeinsame Datenmodelle, Parser, Definitionen, Validierungen, UI-Komponenten und Sicherheitsmechanismen zurück.

## 1.2 Oberstes Entwicklungsziel

Neue Spielinhalte sollen möglichst als Datenänderungen behandelt werden und nicht als Anlass, neue Anwendungskomponenten programmieren zu müssen.

Wenn beispielsweise ein Spielupdate neue Items einführt, soll das Tool diese – abhängig davon, welche Informationen sich tatsächlich aus den verfügbaren Spieldateien gewinnen lassen – erkennen, katalogisieren und in generischen Tabellen darstellen können.

Wenn ein bekanntes Objekt ein neues optionales Feld erhält, soll die Software dieses Feld erkennen und zunächst verlustfrei erhalten können. Ist seine Struktur ausreichend verstanden, kann es zusätzlich generisch dargestellt werden.

Wenn dagegen die grundlegende Savegame-Serialisierung geändert wurde, muss das Tool diese Inkompatibilität erkennen, den betroffenen Parser isoliert behandeln und einen nachvollziehbaren Diagnosebericht erzeugen.

**Grundsatz:** Neue Inhalte dürfen nicht automatisch neue C#-Klassen, neue XAML-Dateien oder individuelle Sonderbehandlungen erfordern.

## 1.3 Was das Tool nicht voraussetzen darf

Die Entwicklung darf nicht davon ausgehen, dass:

- Savegames in einem bestimmten lesbaren Textformat vorliegen.
- alle Spielinformationen in einer einzigen Datei enthalten sind.
- Itemnamen, interne IDs und Beschreibungen immer gemeinsam gespeichert werden.
- jede bekannte Struktur vollständig verstanden ist.
- ein erkennbarer Datenwert automatisch eine bekannte Spielmechanik repräsentiert.
- eine erfolgreiche Dekodierung automatisch eine sichere Bearbeitung ermöglicht.
- alle Dateiformate mit einem einzigen Parser abgedeckt werden können.
- sich interne Strukturen bei Updates nicht verändern.

Die tatsächlichen Dateiformate, Speicherorte, Datenstrukturen und Bearbeitungsmöglichkeiten müssen zunächst untersucht und durch nachvollziehbare Tests bestätigt werden.

## 1.4 Definition of Done für das Gesamtprojekt

Das Projekt gilt nicht bereits dann als erfolgreich, wenn Dateien gelesen und in Tabellen angezeigt werden können.

Die Zielarchitektur muss nachweisen, dass:

1. neue Inhalte innerhalb bekannter Strukturen ohne Änderung am Anwendungscode erkannt werden können;
2. unbekannte Felder und Datenbereiche möglichst verlustfrei erhalten bleiben;
3. Definitionen versioniert, validiert und reproduzierbar geladen werden;
4. Parser austauschbar sind und mehrere Formatversionen unterstützen können;
5. die Benutzeroberfläche aus Daten- und Felddefinitionen erzeugt werden kann;
6. jede Schreiboperation explizit auf Kompatibilität, Berechtigungen und Validierung geprüft wird;
7. fehlgeschlagene Analysen keine Originaldateien verändern;
8. Spielupdates strukturell verglichen und diagnostiziert werden können;
9. die Oberfläche vollständig lokalisierbar ist;
10. Module unabhängig getestet und schrittweise erweitert werden können.

# 2. Verbindliche Architekturgrundsätze

Die folgenden Regeln gelten für alle Entwicklungsagenten, Module, Pull Requests und Architekturentscheidungen.

## Regel 01 – Modulare Architektur

Jedes Modul besitzt einen klar definierten Verantwortungsbereich. Abhängigkeiten werden über explizite Schnittstellen geregelt.

Keine fachliche Funktion darf unkontrolliert direkt auf interne Implementierungen anderer Module zugreifen.

## Regel 02 – Datengetriebene Verarbeitung

Spieldaten, Kategorien, Felder, Tabellen, Filter, Eigenschaften und generische Editoransichten werden so weit wie technisch sinnvoll aus versionierten Definitionen und zur Laufzeit ermittelten Informationen abgeleitet.

Neue Inhalte innerhalb unterstützter Strukturen dürfen keine individuelle Codeänderung voraussetzen.

## Regel 03 – Unbekannte Daten erhalten

Unbekannte Felder, nicht interpretierte Properties und nicht zuordenbare Datenbereiche müssen erfasst und möglichst verlustfrei erhalten werden.

Ein Parser darf unbekannte Daten nicht stillschweigend verwerfen.

## Regel 04 – Keine unbelegte Semantik

Technische Eigenschaften und fachliche Bedeutungen werden getrennt behandelt.

Ein Zahlenwert ist nicht automatisch ein Schadenwert. Ein Feld namens `Level` ist nicht automatisch die Charakterstufe. Eine interne ID ist nicht automatisch eine eindeutige, stabile öffentliche Item-ID.

Interpretationen müssen als beobachtet, abgeleitet oder bestätigt gekennzeichnet werden.

## Regel 05 – Sichere Bearbeitung

Das Erkennen, Anzeigen oder Dekodieren einer Struktur begründet keine automatische Schreibberechtigung.

Schreiboperationen benötigen passende Formatunterstützung, bekannte Schreibregeln, erfolgreiche Validierung und eine nachvollziehbare Sicherheitsprüfung.

## Regel 06 – Mehrsprachigkeit

Alle nutzerseitig sichtbaren Texte werden über ein zentrales Lokalisierungssystem verwaltet.

Keine fachliche Funktion darf deutsche oder englische UI-Texte fest in ihrer Implementierung voraussetzen.

## Regel 07 – Wiederverwendbares Designsystem

Layouts, Controls, Tabellen, Filter, Dialoge, Zustände und Animationen werden zentral definiert und wiederverwendet.

Einheitliche Abstände, Größen, Farben, Übergänge und Interaktionsmuster gelten anwendungsweit.

## Regel 08 – Nicht destruktive Arbeitsweise

Originaldateien werden standardmäßig niemals direkt verändert.

Bearbeitungen erfolgen in einem Arbeitsmodell und werden erst nach expliziter Bestätigung, Validierung und Sicherung in eine neue Ausgabedatei geschrieben.

## Regel 09 – Versionierte Definitionen

Parser, Schemata, Datenkataloge, UI-Definitionen und Capability-Regeln besitzen eigene Versionsinformationen.

Änderungen müssen nachvollziehbar und auf frühere Stände zurückführbar sein.

## Regel 10 – Keine unkontrollierte automatische Codegenerierung

Neue Daten oder Schemaabweichungen dürfen nicht automatisch beliebigen C#-Code generieren oder ausführen.

Automatische Anpassungen erfolgen vorrangig über validierte Daten- und Formatdefinitionen. Neue ausführbare Parserlogik wird als kontrollierte Softwareänderung entwickelt, getestet und freigegeben.

## Regel 11 – Nachvollziehbarkeit

Jede automatisch erkannte Änderung erhält einen Herkunftsnachweis, einen Erkennungsstatus und eine nachvollziehbare Diagnose.

Der Benutzer muss erkennen können, was die Software tatsächlich festgestellt hat und was lediglich eine Vermutung ist.

## Regel 12 – Keine vorschnellen technischen Annahmen

Unbekannte Enshrouded-Formate werden zunächst untersucht. Konkrete Decoder, Verschlüsselungsverfahren, interne Feldnamen oder Speicherlayouts dürfen erst als gesicherte Tatsachen in die produktive Architektur einfließen, wenn sie technisch nachgewiesen wurden.

# 3. Systemarchitektur

## 3.1 Architekturschichten

Die Anwendung wird in klar abgegrenzte Schichten unterteilt.

### Schicht A – Desktop Application

Verantwortlich für:

- Fenster, Navigation und Ansichten
- Benutzerinteraktionen
- Projekt- und Sitzungskontext
- Darstellung von Analyseergebnissen
- Benachrichtigungen und Dialoge
- Benutzerpräferenzen

Die Anwendungsschicht enthält keine eigenständige Implementierung von Savegame-Binärformaten oder Item-Semantik.

### Schicht B – Application Services

Verantwortlich für die Koordination von Anwendungsfällen:

- Spielinstallation erkennen
- Dateien und Savegames untersuchen
- Analysen starten
- Daten katalogisieren
- Änderungen vergleichen
- Bearbeitungsoperationen planen
- Validierungen ausführen
- Export und Wiederherstellung koordinieren

Die Services orchestrieren Fachlogik, führen jedoch keine unkontrollierten Dateiveränderungen durch.

### Schicht C – Domain Model

Enthält technologiearme fachliche Modelle:

- Datenobjekte
- Felder und Properties
- Referenzen
- Inventareinträge
- Container
- Itemdefinitionen
- Schema- und Formatversionen
- Validierungsergebnisse
- Änderungsoperationen
- Analysezustände

Diese Schicht darf nicht von WPF-Controls oder konkreten Oberflächenelementen abhängen.

### Schicht D – Infrastructure

Enthält technische Implementierungen:

- Dateisystemzugriff
- SQLite-Persistenz
- Hashing und Fingerprinting
- Parserimplementierungen
- Serialisierung und Deserialisierung
- Kompressionsadapter, sofern erforderlich
- Datei- und Archivzugriff
- Protokollierung
- Import und Export

### Schicht E – Schema und Capabilities

Beschreibt, welche Daten und Operationen tatsächlich unterstützt werden:

- Dateiformate
- Schemata
- Datenkataloge
- UI-Definitionen
- Validierungsregeln
- Lese- und Schreibfähigkeiten
- Kompatibilitätsregeln
- Versionszuordnungen

Die Definitionen dieser Schicht sollen überwiegend als Daten vorliegen und unabhängig von der Benutzeroberfläche verwaltet werden.

## 3.2 Abhängigkeitsrichtung

Die Abhängigkeiten folgen einer klaren Richtung:

- UI → Application Services
- Application Services → Domain und definierte Interfaces
- Infrastructure → Domain und definierte Interfaces
- Parserimplementierungen → Parser-Abstraktionen und Domain
- Schema Registry → Definitionsmodelle und Validierungsmechanismen

Die Domain darf nicht von WPF, SQLite oder einer konkreten Parserimplementierung abhängen.

Ein UI-Update darf nicht die Funktionsweise eines Binärparsers verändern müssen. Ein neuer Parser darf nicht die Darstellung jedes einzelnen Items erfordern.

## 3.3 Empfohlene Solution-Struktur

Die folgende Struktur ist die angestrebte logische Trennung. Ob jede Gruppe ein eigenes Projekt erhält, wird anhand tatsächlicher Abhängigkeiten entschieden. Es sollen keine künstlichen Projekte ohne eigenständigen Verantwortungsbereich entstehen.

```text
EnshroudedModdingSuite.sln

src/
├── EMS.App/
├── EMS.Presentation/
├── EMS.Application/
├── EMS.Domain/
├── EMS.Infrastructure/
├── EMS.Localization/
├── EMS.DesignSystem/
├── EMS.Persistence/
├── EMS.SchemaRegistry/
├── EMS.SchemaEvolution/
├── EMS.Discovery/
├── EMS.DynamicUI/
├── EMS.Capabilities/
├── EMS.Validation/
├── EMS.Parser.Abstractions/
├── EMS.Parser.Generic/
├── EMS.Parser.Enshrouded/
├── EMS.Savegames/
├── EMS.GameData/
├── EMS.Items/
├── EMS.Inventories/
└── EMS.Diagnostics/

tests/
├── EMS.Domain.Tests/
├── EMS.SchemaRegistry.Tests/
├── EMS.SchemaEvolution.Tests/
├── EMS.Parser.Tests/
├── EMS.Savegames.Tests/
├── EMS.DynamicUI.Tests/
├── EMS.Integration.Tests/
└── EMS.Compatibility.Tests/

definitions/
├── formats/
├── schemas/
├── catalogs/
├── ui/
├── capabilities/
├── localization/
└── tests/

docs/
├── architecture/
├── decisions/
├── formats/
├── compatibility/
└── development/
```

Die endgültige Projektstruktur darf angepasst werden, sofern die Abhängigkeiten und Verantwortlichkeiten erhalten bleiben.

# 4. Discovery Engine – automatische Datenerkennung

## 4.1 Aufgabe

Die Discovery Engine untersucht definierte Quellen, erkennt Dateien und Veränderungen und liefert strukturierte Untersuchungsergebnisse.

Mögliche Quellen sind:

- die bestätigte Spielinstallation;
- bekannte Verzeichnisse mit Spieldaten;
- vom Benutzer ausgewählte Dateien;
- vorhandene Savegame-Verzeichnisse;
- vom Benutzer bereitgestellte Vergleichsdateien;
- später optional weitere explizit freigegebene Datenquellen.

Die Discovery Engine darf nur auf tatsächlich vorhandene Dateien und zugängliche Daten schließen. Sie darf keine erfolgreiche Erkennung vortäuschen, wenn eine Datei nicht lesbar oder ein Format unbekannt ist.

## 4.2 Erkennung von Änderungen

Für relevante Dateien werden mindestens folgende Informationen erfasst:

- kanonischer Dateipfad;
- Dateigröße;
- Änderungszeitpunkt;
- kryptografischer Datei-Hash;
- erkannter Dateityp;
- erkannte Formatversion, sofern verfügbar;
- zugeordneter Parser;
- Parserversion;
- struktureller Fingerprint, sofern berechenbar;
- letzter erfolgreicher Scan;
- Ergebnis und Fehlerstatus des letzten Scans.

Dateihash und struktureller Fingerprint sind unterschiedliche Konzepte.

Ein geänderter Datei-Hash beweist nur eine Inhaltsänderung. Er beweist nicht, dass sich das Dateiformat geändert hat.

## 4.3 Scan-Zustände

Jeder Scan besitzt einen expliziten Status, beispielsweise:

- `Pending`
- `Running`
- `Completed`
- `CompletedWithWarnings`
- `UnsupportedFormat`
- `Failed`
- `Cancelled`

Ein Scan darf bei Fehlern keine unvollständigen Ergebnisse als vollständig bestätigt veröffentlichen.

## 4.4 Scan-Ablauf

1. Quelle validieren.
2. Dateien inventarisieren.
3. Dateiänderungen anhand vorhandener Metadaten erkennen.
4. Dateiformate und verfügbare Parser ermitteln.
5. Unterstützte Dateien analysieren.
6. Strukturen und Referenzen erfassen.
7. Erkannte Inhalte mit bestehenden Definitionen vergleichen.
8. Abweichungen klassifizieren.
9. Ergebnisse und Rohdatenreferenzen persistieren.
10. Einen nachvollziehbaren Bericht erstellen.

Unbekannte Dateien bleiben als unbekannt registriert. Ein nicht erkannter Dateityp ist ein Diagnoseergebnis und kein Grund, die Datei zu ignorieren.

# 5. Schema Registry und Definitionssystem

## 5.1 Grundidee

Die Schema Registry ist das zentrale Wissenssystem der Anwendung.

Sie beschreibt, welche Strukturen bekannt sind, wie Felder interpretiert werden, welche Beziehungen bestehen und welche Operationen unter welchen Voraussetzungen zulässig sind.

Die Registry muss unabhängig von konkreten UI-Ansichten und möglichst unabhängig von einzelnen Spielversionen funktionieren.

## 5.2 Definitionstypen

Es werden mehrere logisch getrennte Definitionsarten eingeführt.

### Format Definitions

Beschreiben die technische Struktur eines Dateiformats:

- Dateisignaturen;
- Containerstrukturen;
- Header;
- Versionserkennung;
- bekannte Kompressionsverfahren;
- Längenpräfixe und Blockgrenzen;
- Serialisierungsregeln;
- Prüfsummen, sofern nachgewiesen;
- bekannte Decoderanforderungen.

### Schema Definitions

Beschreiben die Struktur interpretierter Daten:

- Objekte und Feldnamen;
- Datentypen;
- optionale und erforderliche Felder;
- Listen und verschachtelte Objekte;
- Referenzen und Beziehungen;
- bekannte Wertebereiche;
- Validierungsregeln;
- bekannte Versionen.

### Catalog Definitions

Beschreiben katalogisierte Inhalte:

- Items;
- Kategorien;
- Eigenschaften;
- Ressourcen;
- bekannte interne Kennungen;
- lokalisierte Anzeigenamen;
- Zuordnungen zwischen Definitionen und gespeicherten Instanzen.

### UI Definitions

Beschreiben die generische Darstellung:

- Feldbeschriftungen;
- Reihenfolge;
- Gruppierung;
- Spalten;
- Filter;
- Sichtbarkeit;
- Formatierung;
- geeignete generische Editor-Control-Typen.

UI-Definitionen dürfen keine Schreibberechtigungen vergeben.

### Capability Definitions

Beschreiben explizit, welche Operationen unterstützt werden:

- Lesen;
- Dekodieren;
- Anzeigen;
- Bearbeiten;
- Validieren;
- Serialisieren;
- Exportieren;
- kontrolliertes Schreiben.

Eine Fähigkeit muss immer auf die relevanten Format- und Schema-Versionen bezogen sein.

## 5.3 Statusmodell für Erkenntnisse

Jede Definition und wichtige Interpretation muss ihren Erkenntnisstatus führen können.

| Status | Bedeutung |
|---|---|
| `Observed` | Direkt in den untersuchten Daten festgestellt |
| `Inferred` | Aus Indizien abgeleitet, noch nicht ausreichend bestätigt |
| `Verified` | Durch reproduzierbare Tests bestätigt |
| `Active` | Für die entsprechende Verarbeitung freigegeben |
| `Deprecated` | Veraltet, aber weiterhin dokumentiert |
| `Incompatible` | Mit der aktuellen Verarbeitung nicht sicher kompatibel |
| `Archived` | Historisch aufbewahrt und nicht aktiv verwendet |

Die konkrete Statusübergangslogik wird zentral definiert. Ein einzelner Scan darf nicht eigenständig eine riskante Definition auf `Active` setzen.

## 5.4 Pflichtfelder einer Definition

Jede Definition enthält, soweit für ihren Typ sinnvoll, mindestens:

- stabile Definitions-ID;
- Definitionstyp;
- Definitionsversion;
- zugehörige Format- und Schemaversionen;
- Quelle und Herkunft;
- Erkenntnisstatus;
- Erstellungs- und Änderungszeitpunkt;
- Fingerprint oder Referenz auf den Fingerprint;
- Abhängigkeiten;
- Validierungsergebnis;
- Kompatibilitätsinformationen;
- Änderungsverlauf.

Nicht anwendbare Felder dürfen entfallen. Es dürfen keine künstlichen Werte erfunden werden, um ein einheitliches Schema zu erzwingen.

## 5.5 Definitionen sind Daten, kein ausführbarer Code

Definitionen werden bevorzugt als validierte JSON-Dokumente verwaltet. SQLite dient als persistenter Index für aktive Definitionen, Versionen, Beziehungen und Scanergebnisse.

JSON ist das Austausch- und Exportformat, SQLite die operative Datenhaltung. Es darf nicht unkontrolliert zwei konkurrierende Wahrheiten geben.

Die Anwendung definiert eindeutig, welche Version einer Definition aktiv ist und wie Änderungen persistiert werden.

Definitionen dürfen keine beliebigen Skripte oder dynamisch ausgeführten Code enthalten. Wo deklarative Regeln nicht ausreichen, muss eine explizite, getestete Erweiterung über eine definierte Schnittstelle erfolgen.

# 6. Canonical Data Model

## 6.1 Zweck

Das Canonical Data Model bildet unterschiedliche technische Datenstrukturen auf ein gemeinsames internes Modell ab.

Damit können Tabellen, Filter, Suche, Referenzen, Validierung und Bearbeitung unabhängig vom ursprünglichen Speicherformat arbeiten.

Das kanonische Modell ersetzt jedoch nicht die ursprünglichen Daten. Es ergänzt sie um eine einheitliche Interpretation.

## 6.2 Zentrale Modelltypen

Vorgesehen sind unter anderem:

- `DataSource`
- `FileDescriptor`
- `ParsedDocument`
- `DataObject`
- `DataField`
- `DataValue`
- `DataReference`
- `CollectionDefinition`
- `ItemDefinition`
- `InventoryEntry`
- `ContainerDefinition`
- `SchemaDefinition`
- `FormatDefinition`
- `ValidationResult`
- `CompatibilityReport`
- `ChangeSet`
- `RawDataSegment`

Diese Typen sind Architekturvorschläge. Vor ihrer Implementierung müssen die Verantwortlichkeiten, Identitäten und Beziehungen in den Domain-Modellen präzisiert werden.

## 6.3 Feldinformationen

Ein generisches Feld soll bei Bedarf folgende Informationen transportieren können:

- Feld-ID und ursprünglicher Feldname;
- Datentyp;
- aktueller Wert;
- ursprüngliche Rohdarstellung;
- Quellposition oder Bytebereich, sofern verfügbar;
- zugeordnete Schema-Definition;
- Erkenntnisstatus;
- Validierungsstatus;
- Änderungsstatus;
- Referenzen auf abhängige Objekte;
- Information, ob das Feld bekannt, unbekannt oder nur teilweise interpretiert ist.

Nicht jedes Format liefert diese Informationen. Nicht verfügbare Eigenschaften müssen als nicht verfügbar behandelt werden und dürfen nicht durch Vermutungen ersetzt werden.

## 6.4 Unbekannte Daten

Unbekannte Felder werden nicht automatisch in den nächsten bekannten Typ umgewandelt.

Sie werden möglichst mit ihren ursprünglichen Daten, ihrer Position und ihrem Kontext erhalten.

Die Anwendung muss unterscheiden zwischen:

1. vollständig bekannten und interpretierten Feldern;
2. strukturell bekannten, aber semantisch unbekannten Feldern;
3. teilweise interpretierten Feldern;
4. vollständig unbekannten Datenbereichen.

Bei einem Export muss klar sein, ob unbekannte Daten exakt erhalten, verlustfrei rekonstruiert oder lediglich dokumentiert werden konnten.

# 7. Parser- und Decoder-Architektur

## 7.1 Ziel

Parser müssen austauschbar sein. Fachliche Funktionen dürfen nicht von einem einzelnen Parser oder einer bestimmten Dateiversion abhängig sein.

Die Architektur unterstützt:

- generische Parser für nachgewiesene Standardformate;
- deklarative Formatdefinitionen;
- spezialisierte Parser für komplexe oder proprietäre Strukturen;
- mehrere Parserversionen;
- explizite Unterstützung mehrerer Savegame-Generationen.

## 7.2 Parser-Abstraktion

Als Ausgangspunkt wird ein Interface dieser Art vorgesehen:

```csharp
public interface IGameFileParser
{
    string ParserId { get; }

    string ParserVersion { get; }

    Task<FormatDetectionResult> DetectAsync(
        Stream source,
        ParserContext context,
        CancellationToken cancellationToken);

    Task<ParsedDocument> ReadAsync(
        Stream source,
        ParserContext context,
        CancellationToken cancellationToken);
}
```

`FormatDetectionResult`, `ParserContext` und `ParsedDocument` müssen im Rahmen der Implementierung als konkrete, dokumentierte Verträge definiert werden.

Das Beispiel ist ein Architekturvorschlag und kein bereits implementierter oder auf Enshrouded getesteter Parser.

## 7.3 Schreiben als separate Fähigkeit

Lese- und Schreiblogik werden getrennt modelliert.

Ein Parser, der Dateien lesen kann, ist nicht automatisch zum Schreiben berechtigt.

Eine Schreibschnittstelle muss explizit definieren:

- unterstützte Formate;
- unterstützte Versionen;
- Erhaltung unbekannter Daten;
- Serialisierungsregeln;
- Validierungsanforderungen;
- Fehlerbehandlung;
- bekannte Einschränkungen.

Die Schreibfähigkeit darf nicht allein aus dem Vorhandensein einer Lesefunktion abgeleitet werden.

## 7.4 Parserauswahl

Die Parser Registry entscheidet anhand nachvollziehbarer Kriterien:

- erkannte Dateisignatur;
- Dateiformat;
- bekannte Formatversion;
- bestätigte Kompatibilität;
- erforderliche Decoder;
- Parserpriorität und Erkennungssicherheit.

Bei mehreren plausiblen Kandidaten muss das Ergebnis eindeutig auflösbar oder als mehrdeutig gemeldet werden.

Unbekannte oder widersprüchliche Formate werden nicht durch einen beliebigen Fallback-Parser als erfolgreich interpretiert.

## 7.5 Formatänderungen

Wenn ein Update die Struktur verändert, soll die Software möglichst feststellen, an welcher Schicht die Änderung auftritt:

- Dateierkennung;
- Container;
- Kompression;
- Serialisierung;
- Objektstruktur;
- Feldtyp;
- Referenzmodell;
- fachliche Semantik.

Dadurch wird verhindert, dass eine Änderung des Dateiformats unnötig Änderungen am Item Editor, an der UI oder am Inventarmodell erfordert.

# 8. Schema Evolution und automatische Anpassung

## 8.1 Ziel

Die Schema-Evolution-Komponente vergleicht neu erkannte Strukturen mit bekannten Definitionen.

Sie soll nicht nur melden, dass sich eine Datei verändert hat, sondern die wahrscheinlich betroffene Strukturebene identifizieren und eine begründete Kompatibilitätsbewertung liefern.

## 8.2 Änderungsklassen

### Klasse A – Inhaltliche Änderung

Beispiele:

- neues Item in einer bekannten Struktur;
- zusätzlicher Datensatz;
- neuer Wert eines bekannten Feldes;
- weitere Instanz eines bereits bekannten Objekttyps.

**Reaktion:** automatisch indexieren und darstellen, sofern die bestehenden Definitionen ausreichen.

### Klasse B – Unbekannte, lokal begrenzte Erweiterung

Beispiele:

- neues optionales Feld;
- unbekannter Enum-Wert;
- zusätzliche Property;
- neues verschachteltes Objekt.

**Reaktion:** erkennen, Rohdaten erhalten, als unbekannt darstellen und eine vorläufige Definition vorschlagen. Schreiboperationen bleiben gesperrt, sofern die vorhandenen Regeln die Erweiterung nicht nachweislich sicher abdecken.

### Klasse C – Strukturelle Änderung

Beispiele:

- geänderte Verschachtelung;
- geänderte Feldtypen;
- neue Referenzregeln;
- Umbenennung eines Feldes;
- Änderung der Kardinalität einer Sammlung.

**Reaktion:** neue Schema-Version vorschlagen, Unterschiede dokumentieren und erforderliche Migrationen oder Parseranpassungen identifizieren.

### Klasse D – Grundlegender Formatbruch

Beispiele:

- inkompatible Serialisierung;
- geänderte Containerstruktur;
- geänderte Kompressions- oder Dekodierungsregeln;
- nicht mehr zuverlässig erkennbare Dateiversion.

**Reaktion:** betroffene Verarbeitung sperren, Diagnose erstellen und eine gezielte technische Anpassung verlangen.

Diese Klassen sind keine Garantie, dass jede Änderung automatisch korrekt erkannt wird. Sie definieren die gewünschte Reaktion, sobald ausreichende Erkennungsinformationen vorliegen.

## 8.3 Struktureller Fingerprint

Der strukturelle Fingerprint soll nicht nur den Dateiinhalt hashen. Er soll – soweit aus dem Format ableitbar – relevante Eigenschaften der Struktur berücksichtigen:

- Format- und Schemaidentität;
- Objekt- und Feldstruktur;
- Datentypen;
- Container und Verschachtelung;
- Referenzregeln;
- relevante Serialisierungseigenschaften;
- Versionen und Decoderanforderungen.

Reine Wertänderungen dürfen nicht automatisch als Strukturbruch gewertet werden.

Der Fingerprint ist ein Vergleichsinstrument und kein alleiniger Beweis für Kompatibilität.

## 8.4 Vorschläge statt unkontrollierter Aktivierung

Automatisch erkannte Änderungen werden zunächst als Vorschläge erfasst.

Der Freigabeprozess umfasst:

1. strukturelle Differenz ermitteln;
2. betroffene Definitionen identifizieren;
3. Abhängigkeiten analysieren;
4. vorgeschlagene Änderung validieren;
5. automatisierte Tests ausführen;
6. Kompatibilitätsbericht erzeugen;
7. Änderung abhängig von Risikoklasse und Freigaberegeln aktivieren oder zur Prüfung vorlegen.

Bekannte, risikofreie Inhaltsänderungen benötigen keinen manuellen Freigabeprozess für jede einzelne Instanz. Neue, nicht verstandene Schreibregeln dagegen schon.

# 9. Dynamische Benutzeroberfläche

## 9.1 Grundidee

Die Benutzeroberfläche wird aus wiederverwendbaren Controls und Definitionen aufgebaut.

Die Anwendung soll nicht für jede Itemart und jedes zusätzliche Feld eine neue View benötigen.

Ein generischer Editor erhält die Felddefinition, den Wert, die Validierungsinformationen, die Bearbeitungsfähigkeit und die Lokalisierungsressourcen. Er bestimmt daraus eine geeignete Darstellung.

## 9.2 Generische Control-Typen

Vorgesehen sind unter anderem:

- Textfeld;
- mehrzeiliges Textfeld;
- numerisches Eingabefeld;
- Boolean-Toggle;
- Enum-Auswahl;
- Referenzauswahl;
- Liste;
- generische Tabelle;
- verschachtelte Objektgruppe;
- schreibgeschützte Eigenschaft;
- Rohdatenansicht;
- unbekanntes Feld;
- Validierungsanzeige.

Die Control-Auswahl erfolgt über deklarative Regeln. Ein Zahlenfeld wird nicht automatisch editierbar, nur weil es numerische Daten enthält.

## 9.3 SchemaDrivenEditorService

Ein zentraler Service koordiniert:

- Schemaauflösung;
- Auswahl des Controls;
- Lokalisierung;
- Formatierung;
- Validierung;
- Bearbeitungsstatus;
- Schreibfähigkeiten;
- Anzeige unbekannter Felder.

Spezielle Controls dürfen ergänzt werden, müssen jedoch über eine gemeinsame Registrierung und dokumentierte Schnittstellen eingebunden werden.

## 9.4 Generische Tabellen

Tabellen werden aus Datenmodellen und Definitionen erzeugt.

Sie unterstützen als gemeinsame Grundlage:

- Sortierung;
- Filter;
- Suche;
- Spaltenauswahl;
- Spaltenreihenfolge;
- Gruppierung;
- virtuelle Darstellung großer Datenmengen;
- Export;
- Lokalisierung;
- verständliche Anzeige unbekannter Werte.

Tabellen dürfen keine Daten als sicher bearbeitbar kennzeichnen, wenn dies nicht durch Capability-Regeln bestätigt ist.

## 9.5 Einheitliches Designsystem

Das Design ist modern, professionell und modular.

Verbindliche Gestaltungsvorgaben:

- Dark-Theme als primäres Erscheinungsbild;
- dezente Neonakzente;
- kontrollierte Glow- und Glaseffekte;
- einheitliche Abstände und Ausrichtung;
- abgestimmte Hintergründe für Dropdowns und ComboBoxen;
- wiederverwendbare, visuell konsistente Slider;
- definierte Hover-, Fokus-, Auswahl- und Disabled-Zustände;
- subtile Animationen und Übergänge;
- klare Hierarchie zwischen Hauptansichten, Werkzeugleisten, Tabellen und Detailbereichen;
- gute Lesbarkeit auch bei hoher Datendichte.

Effekte dürfen die Lesbarkeit, Performance oder Barrierefreiheit nicht beeinträchtigen.

Animationen müssen zentral konfigurierbar sein. Die Benutzeroberfläche soll auch bei deaktivierten Animationen vollständig bedienbar bleiben.

## 9.6 Persistente Layouts

Die Anwendung ermöglicht das Speichern und Wiederherstellen von Layouts und Ansichtspräferenzen, soweit die jeweilige Ansicht dies unterstützt.

Gespeichert werden können beispielsweise:

- Fenster- und Panelanordnung;
- Spaltenauswahl und Spaltenreihenfolge;
- Filter und Sortierung;
- Größen von Detailbereichen;
- bevorzugte Editoransichten;
- ausgewählte Werkzeuge.

Layouts werden versioniert. Nicht mehr verfügbare Felder oder Panels dürfen beim Laden eines älteren Layouts nicht zum Absturz führen.

# 10. Lokalisierung und Internationalisierung

## 10.1 Verbindliche Mehrsprachigkeit

Die Anwendung wird von Anfang an mehrsprachig entwickelt.

Mindestens Deutsch und Englisch werden als Zielsprachen vorgesehen. Weitere Sprachen sollen ohne Änderungen an der fachlichen Logik ergänzt werden können.

## 10.2 Technische Vorgaben

- Keine fest codierten UI-Texte in fachlichen Services.
- Zentrale Ressourcenverwaltung.
- Stabile Lokalisierungsschlüssel.
- Lokalisierbare Fehlermeldungen und Validierungsinformationen.
- Lokalisierbare Tooltips und Dialoge.
- Berücksichtigung unterschiedlicher Textlängen.
- Kulturabhängige Zahlen- und Datumsdarstellung.
- Definierter Fallback für fehlende Übersetzungen.
- Keine Übersetzung technischer IDs, sofern diese als unveränderliche Kennungen dienen.

Spielinterne Namen werden getrennt von den UI-Ressourcen gespeichert. Ein lokalisierter Anzeigename darf die interne Identität eines Datensatzes nicht verändern.

# 11. Capability-System und Bearbeitungssicherheit

## 11.1 Zweck

Das Capability-System beantwortet für jede Operation die Frage, ob sie für die konkrete Datei, Struktur und Version tatsächlich unterstützt wird.

Es unterscheidet mindestens zwischen:

- Lesen;
- Dekodieren;
- Anzeigen;
- Suchen;
- Vergleichen;
- Bearbeiten im internen Arbeitsmodell;
- Validieren;
- Serialisieren;
- Exportieren;
- Schreiben einer geänderten Datei.

Diese Fähigkeiten können unabhängig voneinander vorhanden oder eingeschränkt sein.

## 11.2 Voraussetzungen für Schreiboperationen

Vor einer Schreiboperation müssen mindestens folgende Voraussetzungen erfüllt sein:

1. Das Zielformat ist eindeutig identifiziert.
2. Die relevante Formatversion ist bekannt oder ausdrücklich als kompatibel bestätigt.
3. Die betroffene Struktur wird durch ein geeignetes Schema beschrieben.
4. Die Serialisierung für diese Struktur ist unterstützt.
5. Die erforderlichen Validierungen sind erfolgreich.
6. Unbekannte Daten werden gemäß einer dokumentierten Strategie behandelt.
7. Die Ausgabe wird zunächst in eine separate Datei geschrieben.
8. Die erzeugte Datei wird erneut geprüft.
9. Das Ergebnis wird mit dem Arbeitsmodell verglichen.
10. Die Originaldatei bleibt bis zur ausdrücklichen, gesondert abgesicherten Übernahme unverändert.

Nicht alle Validierungen können in jedem Format vollständig durchgeführt werden. Fehlende Prüfmöglichkeiten müssen sichtbar ausgewiesen werden.

## 11.3 Transaktionsmodell

Bearbeitungen werden als Änderungen an einem Arbeitsmodell durchgeführt.

Vorgesehen sind:

- Änderungsprotokoll;
- Undo und Redo;
- gruppierte Bearbeitungsoperationen;
- Validierung vor dem Commit;
- Erstellung einer neuen Ausgabedatei;
- erneutes Einlesen der Ausgabe;
- Vergleich zwischen erwartetem und tatsächlich serialisiertem Ergebnis.

Wenn ein Schritt fehlschlägt, bleibt das Original unangetastet. Unvollständige Ausgabedateien werden nicht als erfolgreiche Exporte präsentiert.

## 11.4 Backups und Wiederherstellung

Vor jeder kontrollierten Übernahme einer bearbeiteten Datei muss eine Wiederherstellungsstrategie existieren.

Backups werden mit Herkunft, Zeitpunkt, Dateihash und zugehörigem Vorgang dokumentiert.

Das Löschen, Überschreiben oder automatische Ersetzen von Originaldateien ist kein Standardverhalten.

# 12. Persistenz und Datenhaltung

## 12.1 SQLite

SQLite dient als lokale Datenbank für operative Metadaten und indexierte Erkenntnisse.

Mögliche Inhalte:

- Schema-Definitionen und Versionen;
- Formatdefinitionen;
- Katalogeinträge;
- Scan-Historie;
- Fingerprints;
- Referenzbeziehungen;
- Kompatibilitätsberichte;
- Validierungsergebnisse;
- Benutzerlayouts;
- Bearbeitungssitzungen;
- Änderungsprotokolle.

Die konkreten Tabellen und Beziehungen werden aus den fachlichen Anforderungen abgeleitet und über versionierte Datenbankmigrationen verwaltet.

## 12.2 Rohdaten

Binärdaten und große Rohdatenbereiche werden nicht unnötig in redundanter Form in der Datenbank abgelegt.

Die Speicherung richtet sich nach dem jeweiligen Format und dem Bedarf an exakter Wiederherstellung. Wo möglich, werden stabile Referenzen, Bytebereiche oder Hashes genutzt.

Eine Rohdatenreferenz ist nur dann ausreichend, wenn die referenzierte Quelle zuverlässig verfügbar bleibt. Andernfalls muss eine geeignete Kopie oder ein anderer Erhaltungsmechanismus verwendet werden.

## 12.3 Datenbankmigrationen

Jede Änderung am internen Datenbankschema erhält eine Migration.

Die Anwendung muss erkennen, ob eine vorhandene Datenbank unterstützt wird, migriert werden kann oder eine manuelle Wiederherstellung benötigt.

Vor Migrationen mit potenziell destruktiven Folgen wird ein Sicherungspunkt angelegt.

# 13. Spielinstallation und Savegame-Erkennung

## 13.1 Spielpfad

Der Benutzer kann die Spielinstallation einmalig bestätigen.

Die Anwendung speichert die bestätigte Quelle und prüft sie bei späteren Starts erneut.

Wenn der Pfad gültig ist, sollen bekannte Datenquellen automatisch eingebunden und Änderungen erkannt werden. Ist der Pfad nicht mehr gültig, wird eine verständliche Wiederherstellungsmöglichkeit angeboten.

Die Anwendung darf einen früher bestätigten Pfad nicht als weiterhin gültig voraussetzen, ohne ihn zu prüfen.

## 13.2 Savegame-Discovery

Die Software soll bekannte Savegame-Speicherorte für die unterstützte Installation untersuchen und erkannte Savegames katalogisieren.

Die Erkennung umfasst:

- Dateiidentität;
- Änderungszeitpunkt;
- Größe;
- Hash;
- unterstützte Formatversion;
- Parserstatus;
- bekannte Welt- oder Speicherinformationen, soweit auslesbar;
- Fehler und offene Fragen.

Es dürfen keine konkreten Speicherpfade, Namensmuster oder Datenfelder als gesichert angenommen werden, bevor sie für die tatsächliche Enshrouded-Installation verifiziert wurden.

## 13.3 Aktualisierung

Beim erneuten Start werden bereits bekannte Dateien nicht unnötig vollständig neu analysiert, sofern zuverlässige Metadaten und Fingerprints eine Wiederverwendung erlauben.

Bei Änderungen wird nur so viel erneut untersucht, wie für eine zuverlässige Aktualisierung erforderlich ist.

Die Software muss trotzdem eine vollständige Neuanalyse ermöglichen, um beschädigte oder veraltete Indizes korrigieren zu können.

# 14. Modulkonzept

## 14.1 Savegame Explorer

Verantwortlich für:

- Savegame-Katalog;
- Metadaten;
- Strukturansicht;
- Referenzen;
- unbekannte Bereiche;
- Kompatibilitätsstatus;
- Änderungen und Diagnosen.

Der Explorer ist zunächst eine Analysefunktion. Editierbarkeit wird nur dort angeboten, wo die notwendigen Schreibfähigkeiten bestätigt sind.

## 14.2 Item Browser

Verantwortlich für den zentralen Katalog bekannter Itemdefinitionen.

Er soll vorhandene Daten aus verschiedenen unterstützten Quellen zusammenführen, ohne unbelegte Eigenschaften zu ergänzen.

Der Browser unterstützt Suche, Kategorien, Filter, Referenzen und Herkunftsnachweise.

## 14.3 Item Editor

Verantwortlich für die kontrollierte Bearbeitung von Itemdefinitionen und Iteminstanzen, soweit die jeweilige Datenstruktur dies erlaubt.

Definition und Instanz müssen klar unterschieden werden. Änderungen an einer Itemdefinition dürfen nicht unbemerkt alle Iteminstanzen oder andere Dateien verändern.

## 14.4 Inventory und Container Editor

Verantwortlich für unterstützte Inventare und Container:

- Einträge darstellen;
- Positionen und Mengen bearbeiten, wenn bestätigt unterstützt;
- Referenzen auflösen;
- erlaubte Operationen validieren;
- Änderungen nachvollziehbar protokollieren.

Das spätere Verschieben von Gegenständen zwischen Kisten und Inventaren ist als eigener Anwendungsfall zu modellieren. Es darf nicht voraussetzen, dass alle Datenstrukturen identisch sind oder beliebig zusammengeführt werden können.

## 14.5 Weapon und Equipment Editor

Waffen und Ausrüstung werden als spezialisierte Ansichten auf den generischen Editor aufgebaut.

Spezielle Fachlogik ist zulässig, sofern sie auf bestätigten Datenmodellen und klar definierten Regeln beruht.

Der Editor darf nicht allein aufgrund sichtbarer numerischer Werte beliebige Änderungen als gültige Spielwerte deklarieren.

## 14.6 Diagnostics und Compatibility Center

Zentrale Ansicht für:

- unbekannte Dateitypen;
- Schemaabweichungen;
- Parserfehler;
- neue Felder;
- Versionskonflikte;
- nicht unterstützte Schreiboperationen;
- fehlgeschlagene Validierungen;
- offene technische Fragen;
- vorgeschlagene Definitionserweiterungen.

Die Diagnoseansicht soll dem Entwickler möglichst präzise zeigen, welche Komponente angepasst werden müsste.

# 15. Teststrategie und Qualitätssicherung

## 15.1 Testebenen

### Unit-Tests

Prüfen einzelne Funktionen und Regeln:

- Schemaauflösung;
- Typkonvertierung;
- Validierung;
- Fingerprinting;
- Kompatibilitätsklassifikation;
- Lokalisierungsfallbacks;
- Änderungsoperationen.

### Integrationstests

Prüfen die Zusammenarbeit von:

- Parser und Schema Registry;
- Discovery und Persistenz;
- Schema Evolution und Definitionsverwaltung;
- Dynamic UI und Domain-Modellen;
- Serializer und Validierung.

### Regressionstests

Bereits unterstützte Formate und Strukturen müssen mit bekannten Testdateien erneut geprüft werden.

Jede bestätigte Fehlerkorrektur soll, wo möglich, durch einen Regressionstest abgesichert werden.

### Roundtrip-Tests

Für unterstützte Schreibformate wird geprüft:

1. Originaldatei einlesen.
2. Internes Modell erstellen.
3. Ohne inhaltliche Änderung serialisieren.
4. Ergebnis erneut einlesen.
5. Strukturelle und semantische Gleichwertigkeit prüfen.
6. Unbekannte Daten und relevante Originalinformationen vergleichen.

Bytegenaue Gleichheit ist nicht für jedes Format erforderlich oder möglich. Die erwartete Gleichheit muss pro Format dokumentiert werden.

### Kompatibilitätstests

Unterstützte Formatversionen werden mit repräsentativen Testdateien geprüft.

Wenn alte Versionen weiterhin unterstützt werden sollen, dürfen neue Parseränderungen diese nicht unbeabsichtigt brechen.

## 15.2 Update-Simulation

Die Architektur benötigt kontrollierte Testfälle für:

- neues Item in bekannter Struktur;
- neuer Enum-Wert;
- neues optionales Feld;
- unbekanntes verschachteltes Objekt;
- geänderten Feldtyp;
- geänderte Referenz;
- inkompatible Formatversion;
- beschädigte Datei;
- fehlenden Decoder.

Für jeden Fall wird festgelegt, welches Ergebnis erwartet wird und ob Lesen, Anzeigen, Bearbeiten oder Schreiben erlaubt sein darf.

## 15.3 Qualitätskriterien

Ein Feature ist erst abgeschlossen, wenn:

- die Implementierung dokumentiert ist;
- relevante Tests bestehen;
- Fehlerfälle berücksichtigt sind;
- Lokalisierung vorhanden ist;
- UI-Komponenten dem Designsystem entsprechen;
- unbekannte Daten angemessen behandelt werden;
- Berechtigungen und Capabilities korrekt geprüft werden;
- bestehende Funktionen nicht unbeabsichtigt beeinträchtigt werden.

# 16. Entwicklungsphasen

## Phase 0 – Bestandsaufnahme und technischer Machbarkeitsnachweis

Ziele:

- vorhandene Enshrouded-Dateien und bekannte Werkzeuge untersuchen;
- tatsächliche Formate und bekannte Parsermöglichkeiten dokumentieren;
- repräsentative Testdateien auswählen;
- sichere Kopien und Hashes anlegen;
- offene Fragen und Annahmen kennzeichnen;
- einen ersten erfolgreich analysierbaren Datensatz identifizieren.

Ergebnis: ein überprüfbarer technischer Ausgangspunkt, keine Sammlung unbestätigter Annahmen.

## Phase 1 – Domain Model und Schema Registry

Ziele:

- zentrale Definitionsmodelle;
- Status- und Versionssystem;
- JSON-Import und -Export;
- Definitionsvalidierung;
- SQLite-Persistenz;
- Herkunftsnachweise;
- automatisierte Tests.

Abnahmekriterium: Eine Definition kann erstellt, gespeichert, erneut geladen, versioniert und gegen ein Schema validiert werden.

## Phase 2 – Parser-Abstraktionen und Discovery

Ziele:

- Parserinterfaces;
- Parser Registry;
- Dateiinventarisierung;
- Hashing und Fingerprinting;
- Scan-Historie;
- Fehlerzustände;
- erste verifizierte Parserintegration.

Abnahmekriterium: Unterstützte Dateien werden reproduzierbar erkannt und analysiert. Unbekannte Dateien werden nachvollziehbar dokumentiert.

## Phase 3 – Canonical Data Model

Ziele:

- generische Objekte und Felder;
- Referenzen;
- Rohdatenbezüge;
- unbekannte Felder;
- Änderungsstatus;
- Validierungsergebnisse.

Abnahmekriterium: Ein realer Datensatz kann ohne unnötige Bindung an ein einzelnes Dateiformat im kanonischen Modell dargestellt werden.

## Phase 4 – Schema Evolution

Ziele:

- strukturelle Differenzen;
- neue Felder;
- unbekannte Werte;
- Schema-Versionierung;
- Kompatibilitätsklassifikation;
- Vorschläge für Definitionsänderungen.

Abnahmekriterium: Eine kontrolliert veränderte Teststruktur wird erkannt und korrekt klassifiziert, ohne eine unsichere Schreibfreigabe zu erteilen.

## Phase 5 – Designsystem und Dynamic UI

Ziele:

- zentrale WPF-Controls;
- Design-Tokens;
- Lokalisierung;
- dynamischer Eigenschafteneditor;
- generische Tabellen;
- Filter und Suche;
- persistente Layouts.

Abnahmekriterium: Ein zusätzliches, unterstütztes Feld erscheint durch eine Definitionsänderung im generischen Editor, ohne Änderung an C#- oder XAML-Code.

## Phase 6 – Savegame Explorer und Item Browser

Ziele:

- bekannte Savegames katalogisieren;
- unterstützte Strukturen anzeigen;
- Itemdaten aus verifizierten Quellen indexieren;
- Referenzen und Diagnosen darstellen.

Abnahmekriterium: Repräsentative echte Dateien können sicher untersucht werden. Nicht unterstützte Daten werden nicht fälschlich als vollständig verstanden ausgegeben.

## Phase 7 – Kontrollierte Bearbeitung

Ziele:

- Änderungsmodell;
- Undo und Redo;
- Capability-Prüfung;
- Validierung;
- separate Ausgabe;
- Roundtrip-Tests;
- Backup und Wiederherstellung.

Abnahmekriterium: Eine bestätigte, unterstützte Änderung kann ausgeführt, exportiert, erneut eingelesen und überprüft werden, ohne das Original zu verändern.

## Phase 8 – Spezialisierte Editoren

Ziele:

- Item Editor;
- Waffen- und Ausrüstungseditor;
- Inventar- und Containereditor;
- erweiterte Such- und Massenoperationen.

Diese Phase beginnt erst, wenn die darunterliegenden Modelle und Schreibfähigkeiten für die jeweiligen Anwendungsfälle ausreichend verifiziert sind.

## Phase 9 – Update-Resilienz und Stabilisierung

Ziele:

- Regressionstestbestand erweitern;
- Kompatibilitätsberichte verbessern;
- Performance optimieren;
- Migrationen testen;
- Wiederherstellung überprüfen;
- Dokumentation vervollständigen.

Abnahmekriterium: Ein simuliertes Inhaltsupdate wird ohne Codeänderung verarbeitet, während eine simulierte inkompatible Formatänderung zuverlässig erkannt und sicher abgefangen wird.

# 17. Arbeitsregeln für Entwicklungsagenten

## 17.1 Vor jeder Implementierung

Der Agent muss:

1. die relevante Architektur und bestehende Implementierungen untersuchen;
2. vorhandene Interfaces und Modelle wiederverwenden;
3. Abhängigkeiten und Seiteneffekte prüfen;
4. Annahmen von bestätigten Tatsachen trennen;
5. bei unklaren Dateiformaten zunächst Diagnose- oder Analysecode erstellen;
6. die kleinste sinnvolle, testbare Änderung planen.

Es darf keine parallele alternative Implementierung einer bereits vorhandenen Funktion entstehen, nur weil ein Agent den bestehenden Code nicht untersucht hat.

## 17.2 Während der Implementierung

- Bestehende Verträge dürfen nicht stillschweigend verändert werden.
- Gemeinsame Funktionen gehören in gemeinsame Komponenten.
- Fachlogik gehört nicht in UI-Eventhandler.
- WPF-Code-behind bleibt auf notwendige UI-Integration beschränkt.
- Keine unkontrollierten globalen Zustände.
- Keine unvalidierten automatischen Schreiboperationen.
- Keine stillschweigende Verwerfung unbekannter Daten.
- Keine fest codierten UI-Texte.
- Keine unbestätigten Spielinformationen als Tatsachen.
- Keine neuen Abhängigkeiten ohne nachvollziehbaren Nutzen.
- Keine automatisch generierten oder ausgeführten Plugins ohne explizite Sicherheitsarchitektur.

## 17.3 Nach jeder Implementierung

Der Agent liefert:

- kurze Zusammenfassung der Änderung;
- betroffene Projekte und Dateien;
- relevante Architekturentscheidungen;
- ausgeführte Tests;
- Testergebnisse;
- bekannte Einschränkungen;
- offene Fragen;
- notwendige Folgearbeiten.

Nicht ausgeführte Tests dürfen nicht als bestanden gemeldet werden.

## 17.4 Umgang mit Architekturänderungen

Größere Architekturänderungen benötigen eine dokumentierte Entscheidung mit:

- Problemstellung;
- Alternativen;
- Vor- und Nachteilen;
- Auswirkungen auf vorhandene Module;
- Migrationsbedarf;
- Sicherheitsauswirkungen;
- Teststrategie.

Die Entscheidung wird in `docs/decisions/` dokumentiert.

# 18. Definitionen für den ersten vertikalen Prototyp

Der erste Prototyp soll nicht sofort einen vollständigen Item Editor implementieren.

Er soll einen vollständigen, kleinen Ablauf nachweisen:

1. Eine tatsächlich vorhandene, lesbare Teststruktur wird untersucht.
2. Der passende Parser erzeugt ein generisches Datenmodell.
3. Eine Schema-Definition beschreibt die bekannten Felder.
4. Das Modell wird in SQLite indexiert.
5. Der generische Editor zeigt die Felder an.
6. Ein zusätzliches Testfeld wird in einer kontrollierten Definition ergänzt.
7. Die Anwendung erkennt und zeigt das Feld ohne Änderung am Anwendungscode.
8. Ein unbekanntes Feld wird als unbekannt dargestellt und erhalten.
9. Ein Kompatibilitätsbericht beschreibt die erkannten Unterschiede.
10. Eine nicht unterstützte Schreiboperation wird zuverlässig blockiert.

Dieser Prototyp beweist das wichtigste Architekturprinzip, bevor umfangreiche spezialisierte Oberflächen gebaut werden.

# 19. Abschließende Architekturentscheidung

Die Enshrouded Modding Suite wird als datengetriebene, modulare und versionsbewusste Plattform entwickelt.

Die zentralen Bausteine sind:

- Discovery Engine;
- Parser Registry;
- Schema Registry;
- Canonical Data Model;
- Schema Evolution;
- Dynamic UI;
- Capability-System;
- Validierungs- und Transaktionsmodell;
- persistente Definitions- und Analysehistorie.

Diese Bausteine bilden das Fundament sämtlicher spezialisierter Werkzeuge.

Die Anwendung soll möglichst viele Inhaltsänderungen selbstständig verarbeiten und neue Strukturen zuverlässig erkennen. Sie muss zugleich klar unterscheiden, was sie weiß, was sie vermutet und was sie noch nicht sicher bearbeiten kann.

**Die Leitlinie für sämtliche Entwicklungsentscheidungen lautet:**

> Neue Daten sollen möglichst neue Möglichkeiten eröffnen, nicht neue Sonderfälle im Anwendungscode erzeugen. Unbekannte Daten werden erhalten, Erkenntnisse werden nachvollziehbar dokumentiert und Änderungen werden nur dann geschrieben, wenn Format, Struktur und Operation ausreichend verstanden und validiert sind.
