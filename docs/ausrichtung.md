# Verbindliche Produktausrichtung: Spielerorientierte Benutzeroberfläche

## 1. Grundprinzip: Ein Werkzeug für Spieler, kein technisches Analyseprogramm

Die Enshrouded Modding Suite wird als benutzerfreundliches Werkzeug für Enshrouded-Spieler entwickelt. Sie ist kein allgemeiner Dateianalysator, kein Hex-Editor und keine Entwicklerkonsole mit grafischer Oberfläche.

Die technische Architektur darf hochkomplex sein. Die Benutzeroberfläche darf es nicht sein.

**In der regulären Benutzeroberfläche dürfen ausschließlich Informationen, Funktionen und Einstellungen erscheinen, die für Spieler bei der Nutzung, Verwaltung oder Bearbeitung ihrer Enshrouded-Spielinhalte relevant sind.**

Technische Informationen über interne Parser, Binärstrukturen, Schema-Fingerprints, Serialisierungsverfahren, unbekannte Bytebereiche, interne Datenbanktabellen oder Implementierungsdetails gehören nicht in die reguläre UI.

Solche Informationen bleiben intern verfügbar, soweit sie für Entwicklung, Fehleranalyse und Wartung erforderlich sind. Sie dürfen jedoch nicht die Navigation, die Begriffe oder die Standardansichten für Spieler bestimmen.

Die Benutzeroberfläche soll sich an den Fragen orientieren, die ein Spieler beantworten möchte:

- Welche Spielstände habe ich?
- Was befindet sich in meinem Spielstand?
- Welche Items gibt es und wo finde ich sie?
- Welche Eigenschaften besitzt eine Waffe?
- Was liegt in meinem Inventar oder in einer Kiste?
- Welche Gegenstände kann ich verändern?
- Was würde eine Änderung bewirken?
- Ist mein Spielstand sicher bearbeitbar?
- Was hat sich verändert, und wurde meine Änderung erfolgreich übernommen?

Der Spieler soll nicht wissen müssen, wie ein Savegame intern aufgebaut ist, um diese Aufgaben auszuführen.

## 2. Trennung von interner Technik und Spieleroberfläche

Die Anwendung wird in zwei klar getrennte Verantwortungsebenen gegliedert.

### Ebene A: Interne technische Plattform

Diese Ebene umfasst sämtliche komplexen technischen Funktionen:

- Dateierkennung und Datenanalyse;
- Parser und Decoder;
- Schema Registry und Definitionen;
- strukturelle Fingerprints;
- Schema Evolution;
- Format- und Versionskompatibilität;
- Erkennung unbekannter Daten;
- technische Validierung;
- Serialisierung und Deserialisierung;
- Datenbank und interne Indizes;
- detaillierte Diagnose und Fehlerprotokollierung.

Diese Funktionen bilden das Fundament der Anwendung. Sie werden nicht als Hauptnavigation oder als technische Arbeitsbereiche für normale Spieler angeboten.

### Ebene B: Spielerorientierte Benutzeroberfläche

Diese Ebene präsentiert die Ergebnisse der technischen Plattform in verständlicher, spielbezogener Form.

Beispiele:

| Internes Ergebnis | Darstellung für Spieler |
|---|---|
| Savegame-Parser unterstützt das Format | Spielstand kann gelesen werden |
| Schreibfähigkeit für diese Version fehlt | Bearbeitung ist für diesen Spielstand noch nicht verfügbar |
| Neues unbekanntes Datenfeld erkannt | Zusätzliche Spieldaten wurden erkannt und werden vorerst unverändert erhalten |
| Validierung erfolgreich | Änderungen wurden überprüft |
| Export und erneutes Einlesen erfolgreich | Die bearbeitete Datei wurde erfolgreich erstellt und überprüft |
| Formatänderung erkannt | Dieses Spielupdate benötigt eine Anpassung, bevor die betroffene Funktion wieder verfügbar ist |

Die UI übersetzt technische Zustände in verständliche Aussagen, ohne deren Bedeutung zu verfälschen.

Wenn eine Einschränkung besteht, muss sie ehrlich und verständlich erklärt werden. Die Anwendung darf weder falsche Sicherheit vermitteln noch den Spieler mit unnötigen Implementierungsdetails belasten.

## 3. Verbindliche Informationsregeln

Für jede sichtbare Information gelten folgende Fragen:

1. Hilft sie dem Spieler bei einer konkreten Aufgabe?
2. Hilft sie ihm, eine Entscheidung zu treffen?
3. Erklärt sie eine Einschränkung, einen Fehler oder ein Ergebnis?
4. Ist sie in verständlicher Spielersprache formuliert?

Wenn keine dieser Fragen zutrifft, gehört die Information standardmäßig nicht in die reguläre UI.

Technische Informationen dürfen nicht allein deshalb angezeigt werden, weil sie intern verfügbar sind.

Ebenso darf die datengetriebene Architektur nicht dazu führen, dass automatisch jedes erkannte Feld, jede Property oder jede interne Datenstruktur in einem Spielereditor erscheint.

**Datengetrieben bedeutet, dass sich die Software an neue Inhalte anpasst. Es bedeutet nicht, dass die Software dem Spieler sämtliche internen Daten ungefiltert präsentiert.**

---

# 4. Verbindlicher Aufbau der Benutzeroberfläche

## 4.1 Grundlayout

Die Anwendung erhält ein einheitliches Hauptfenster mit einer dauerhaft konsistenten Struktur.

<escape>
┌──────────────────────────────────────────────────────────────────────┐
│ LOGO   Enshrouded Modding Suite       Spielstand / Status   Settings │
├───────────────┬──────────────────────────────────────────────────────┤
│               │                                                      │
│ HAUPTNAVIGATION│  TITEL DES AKTUELLEN BEREICHS                       │
│               │  Kurze Beschreibung der aktuellen Aufgabe           │
│ Übersicht     │                                                      │
│ Spielstände   │  SUCHEN / FILTER / AKTIONEN                          │
│ Items         │                                                      │
│ Inventar      │  HAUPTINHALT                                        │
│ Waffen        │  Tabelle, Itemkarten, Inventar oder Detailansicht    │
│               │                                                      │
│               │                                                      │
├───────────────┴──────────────────────────────────────────────────────┤
│ Bereitschaft / Hinweise / laufende Aufgabe            Sprache       │
└──────────────────────────────────────────────────────────────────────┘
</escape>

Die Darstellung ist schematisch. Die konkrete Gestaltung erfolgt im zentralen Designsystem.

### Kopfzeile

Die Kopfzeile enthält ausschließlich global relevante Funktionen:

- Anwendungslogo und Produktname;
- aktuell ausgewählter Spielstand, sofern vorhanden;
- verständlicher Status der ausgewählten Datenquelle;
- Zugriff auf Einstellungen;
- optional kontextabhängige Hauptaktionen.

Technische Versions- und Parserinformationen gehören nicht in die Kopfzeile.

### Hauptnavigation

Die Navigation gruppiert Funktionen nach Spieleraufgaben, nicht nach internen Modulen.

Die Anzahl der Haupteinträge soll überschaubar bleiben. Unterfunktionen erscheinen innerhalb des jeweils passenden Bereichs.

### Arbeitsbereich

Der Arbeitsbereich zeigt immer den Inhalt des ausgewählten Bereichs. Jede Seite besitzt einen klaren Titel, eine kurze Orientierung und eindeutig erkennbare Hauptaktionen.

Tabellen, Filter, Detailansichten und Dialoge verwenden einheitliche Muster.

### Statusleiste

Die Statusleiste zeigt nur unmittelbar nützliche Informationen:

- ausgewählter Spielstand;
- Speicher- oder Bearbeitungsstatus;
- laufende Aufgaben;
- verständliche Warnungen;
- Ergebnisse abgeschlossener Aktionen.

Keine Rohlogs, Bytepositionen, Stacktraces oder technischen Fingerprints.

---

# 5. Die Bereiche der Hauptnavigation

Die folgende Struktur ist die verbindliche Zielstruktur. Bereiche dürfen intern auf gemeinsame Komponenten zurückgreifen, müssen für den Spieler aber eine klar verständliche Aufgabe erfüllen.

## 5.1 Übersicht

**Zweck:** Einstiegspunkt und zentraler Überblick über den aktuellen Arbeitszustand.

Die Übersicht soll dem Spieler direkt zeigen, womit er arbeiten kann und welche wichtigen Aktionen verfügbar sind.

### Inhalte

- zuletzt verwendeter Spielstand;
- erkannte Spielstände;
- zuletzt bearbeitete Gegenstände;
- Status der aktuellen Datenquelle;
- wichtige Hinweise zu verfügbaren Funktionen;
- zuletzt durchgeführte Aktionen;
- Schnellzugriffe auf häufig verwendete Aufgaben.

### Hauptaktionen

- Spielstand öffnen;
- Spielstand auswählen;
- Items durchsuchen;
- Inventar öffnen;
- letzte Bearbeitung fortsetzen.

Die Übersicht ist kein technisches Dashboard. Sie zeigt weder Parserstatistiken noch interne Datenbankzustände oder strukturelle Änderungsberichte.

Wenn eine Funktion aufgrund einer inkompatiblen Spielversion nicht verfügbar ist, wird dies verständlich erklärt und mit einer passenden Handlungsmöglichkeit verbunden.

## 5.2 Spielstände

**Zweck:** Spielstände finden, auswählen, untersuchen und – soweit unterstützt – bearbeiten.

Dieser Bereich ist die zentrale Anlaufstelle für die eigenen Enshrouded-Spielstände.

### Hauptansicht

Eine übersichtliche Liste oder Kartenansicht zeigt pro Spielstand die verfügbaren spielrelevanten Informationen:

- Name oder identifizierbare Bezeichnung;
- Welt oder Charakter, sofern zuverlässig erkennbar;
- letzter Änderungszeitpunkt;
- Dateigröße nur, wenn sie für den Spieler einen erkennbaren Nutzen hat;
- Bearbeitungsstatus;
- verständlicher Hinweis auf die unterstützten Funktionen.

Nicht zuverlässig auslesbare Informationen werden nicht erfunden.

### Detailansicht

Nach Auswahl eines Spielstands stehen spielbezogene Unterbereiche zur Verfügung:

- **Überblick:** erkannte Welt- und Charakterinformationen, sofern verfügbar;
- **Inventar:** unterstützte Inventare und Gegenstände;
- **Kisten und Container:** bekannte Lager und deren Inhalte;
- **Items:** Gegenstände, die dem Spielstand zugeordnet werden können;
- **Änderungen:** vom Tool vorgenommene oder geplante Änderungen;
- **Sicherung:** Backups, Wiederherstellung und Export.

Nicht jeder Unterbereich muss für jede Datei verfügbar sein. Die Oberfläche zeigt nur Funktionen an, die für den jeweiligen Spielstand tatsächlich angeboten werden können.

### Hauptaktionen

- Öffnen;
- Sicherung erstellen;
- unterstützte Daten untersuchen;
- Änderungen vorbereiten;
- bearbeitete Kopie exportieren;
- vorherigen Zustand wiederherstellen.

Technische Datei- und Formatdiagnosen bleiben interne Informationen. Spieler erhalten stattdessen einen verständlichen Kompatibilitätsstatus.

## 5.3 Items

**Zweck:** Spielgegenstände finden, vergleichen und ihre spielrelevanten Eigenschaften untersuchen.

Der Item-Bereich ist ein zentraler Katalog für Gegenstände, deren Informationen aus bestätigten Datenquellen gewonnen werden konnten.

### Hauptansicht

Die Itemliste soll abhängig vom erkannten Datensatz unter anderem folgende Informationen zeigen:

- Itemname;
- Kategorie;
- Gegenstandsart;
- relevante Eigenschaften;
- Verfügbarkeit weiterer Details;
- optional Icon oder Darstellung, sofern eine verlässliche Quelle vorhanden ist.

### Suche und Filter

Spieler sollen Gegenstände über verständliche Kriterien finden können:

- Name;
- Kategorie;
- Gegenstandsart;
- relevante spielbezogene Eigenschaften;
- weitere tatsächlich unterstützte Merkmale.

Die Filter werden aus den verfügbaren, für Spieler sinnvollen Informationen abgeleitet. Technische Feldnamen sind keine Standardfilter.

### Item-Detailansicht

Die Detailansicht zeigt die Informationen, die ein Spieler benötigt, um den Gegenstand zu verstehen oder zu bearbeiten.

Je nach Gegenstandsart können dies sein:

- Name und Beschreibung;
- Kategorie und Gegenstandsart;
- relevante Eigenschaften;
- Anforderungen;
- bekannte Effekte;
- verfügbare Varianten;
- bestätigte Beziehungen zu anderen Spielinhalten.

Nicht jede Eigenschaft ist bei jedem Item vorhanden. Leere technische Felder sollen nicht als leere Formularfelder erscheinen.

### Bearbeitung

Wenn eine Eigenschaft sicher bearbeitet werden kann, wird sie als normale Spielerfunktion angeboten.

Wenn sie nur lesbar oder noch nicht ausreichend verstanden ist, wird sie nicht als frei editierbares Feld dargestellt.

Der Item-Katalog darf nicht mit den tatsächlich im ausgewählten Spielstand vorhandenen Gegenständen verwechselt werden. Definitionen und konkrete Iteminstanzen müssen in der UI klar unterscheidbar sein.

## 5.4 Inventar und Kisten

**Zweck:** Gegenstände im Spielstand organisieren und unterstützte Container verwalten.

Dieser Bereich richtet sich an Spieler, die wissen möchten, welche Gegenstände sich wo befinden, und die ihre Lagerbestände effizient verwalten wollen.

### Hauptansicht

Die Darstellung orientiert sich an einem vertrauten Inventar- und Lagerkonzept:

- Inventarplätze;
- Gegenstandssymbole, sofern verfügbar;
- Itemname;
- Anzahl;
- relevante Kurzdetails;
- ausgewählter Container oder Lagerort.

Die genaue Darstellung richtet sich nach den tatsächlich unterstützten Inventarstrukturen.

### Containeransicht

Unterstützte Kisten und Container werden in einer verständlichen Liste oder Übersicht dargestellt.

Pro Container werden nach Möglichkeit angezeigt:

- Name oder erkennbare Bezeichnung;
- zugehörige Welt oder Bereich;
- enthaltene Gegenstände;
- Mengen;
- bekannte Kapazität, sofern diese zuverlässig auslesbar ist.

Die UI darf keine feste Kapazität annehmen, wenn diese für den konkreten Container nicht bestätigt ist.

### Hauptaktionen

- Gegenstände suchen;
- Inventare und Kisten durchsuchen;
- Gegenstände auswählen;
- Mengen ändern, sofern unterstützt;
- Gegenstände zwischen unterstützten Inventaren und Containern verschieben;
- mehrere Gegenstände gesammelt bearbeiten;
- Änderungen prüfen und exportieren.

Massenoperationen müssen nachvollziehbar sein. Vor umfangreichen Änderungen werden die Auswirkungen verständlich zusammengefasst.

Technische Referenz-IDs und interne Speicherpositionen bleiben verborgen, solange sie nicht für eine konkrete Spieleraufgabe notwendig sind.

## 5.5 Waffen und Ausrüstung

**Zweck:** Waffen und Ausrüstung untersuchen, vergleichen und unterstützte Eigenschaften bearbeiten.

Dieser Bereich ist eine spezialisierte Spieleransicht auf Basis der generischen Item- und Bearbeitungsarchitektur.

### Waffen

Je nach verlässlich verfügbaren Informationen können angezeigt werden:

- Waffenname und Kategorie;
- relevante Kampfwerte;
- bekannte Effekte;
- Anforderungen;
- Verbesserungen oder Modifikationen;
- weitere spielbezogene Eigenschaften.

### Ausrüstung

Je nach Gegenstandsart können dargestellt werden:

- Ausrüstungstyp;
- Schutz- oder sonstige relevante Werte;
- Effekte und Boni;
- Anforderungen;
- verfügbare Varianten.

Es werden nur Werte angezeigt, deren Bedeutung ausreichend belegt ist.

### Bearbeitung

Die Bearbeitung erfolgt über verständliche Controls mit klaren Bezeichnungen und nachvollziehbaren Wertebereichen.

Wo bestätigte Regeln existieren, werden zulässige Wertebereiche und Auswirkungen angezeigt.

Unbekannte Zahlenwerte oder technische Properties werden nicht automatisch als Waffenstatistiken interpretiert.

Der Bereich soll sich wie ein professioneller Gegenstandseditor für Spieler anfühlen, nicht wie ein Debugger.

## 5.6 Einstellungen

**Zweck:** Die Anwendung an die Bedürfnisse des Spielers anpassen.

### Benutzeroberfläche

- Sprache;
- Theme und Erscheinungsbild;
- Animationen;
- Darstellung und Layout;
- gespeicherte Ansichten;
- Spalten- und Filterpräferenzen.

### Spiel und Dateien

- bestätigte Spielinstallation;
- erkannte Speicherorte;
- Speicher- und Exportverhalten;
- Backup-Einstellungen;
- Verhalten bei neuen Spielupdates.

### Bedienung

- Standardaktionen;
- Bestätigungen für umfangreiche Änderungen;
- Verhalten beim Öffnen von Spielständen;
- optionale Komfortfunktionen.

### Wartung

Für Spieler verständliche Funktionen wie:

- Daten neu einlesen;
- Erkennung aktualisieren;
- lokale Indizes erneuern;
- gespeicherte Einstellungen zurücksetzen.

Technische Debug-Optionen und interne Parserkonfigurationen werden nicht als gewöhnliche Spielereinstellungen angeboten.

---

# 6. Kontextabhängige Detailansichten statt zusätzlicher Hauptbereiche

Nicht jede Funktion benötigt einen eigenen Navigationseintrag.

Zusätzliche Informationen sollen dort erscheinen, wo der Spieler sie erwartet.

Beispiele:

- Itemdetails innerhalb des Item-Bereichs;
- Inventardetails innerhalb des ausgewählten Spielstands;
- Kisteninhalte innerhalb der Containeransicht;
- Änderungsvorschau direkt vor einer Bearbeitung;
- Exportergebnis nach Abschluss einer Aktion;
- Wiederherstellung innerhalb der Sicherungsverwaltung.

Dadurch bleibt die Hauptnavigation übersichtlich, obwohl die Anwendung intern umfangreiche Funktionen besitzt.

## 6.1 Einheitliches Detailpanel

Für Tabellen und Listen wird ein wiederverwendbares Detailpanel vorgesehen.

Es kann je nach Kontext rechts neben der Liste, unterhalb der Liste oder als separate Detailseite erscheinen.

Es zeigt:

- den Namen des ausgewählten Objekts;
- die wichtigsten spielrelevanten Informationen;
- verfügbare Aktionen;
- verständliche Hinweise zu Einschränkungen.

Technische Rohdaten werden nicht automatisch in dieses Panel aufgenommen.

## 6.2 Einheitliche Bearbeitungsdialoge

Bearbeitungsdialoge folgen demselben Muster:

1. Objekt und aktuelle Werte anzeigen.
2. Änderbare Eigenschaften verständlich präsentieren.
3. Ungültige Eingaben direkt kennzeichnen.
4. Auswirkungen und wichtige Einschränkungen erklären.
5. Änderungen überprüfen.
6. Bearbeitung bestätigen oder verwerfen.

Dialoge dürfen nicht mit technischen Informationen überladen werden. Die Oberfläche soll die nötige Entscheidung ermöglichen, ohne die interne Implementierung offenzulegen.

---

# 7. Spielerorientierte Darstellung von Fehlern und Einschränkungen

Die Anwendung muss technische Fehler intern detailliert protokollieren, aber nach außen verständlich kommunizieren.

Beispiele:

**Nicht unterstützt**

„Dieser Spielstand kann bereits untersucht werden, aber das sichere Schreiben dieses Formats wird noch nicht unterstützt. Deine Originaldatei wurde nicht verändert.“

**Unbekannte Eigenschaften**

„Für diesen Gegenstand wurden zusätzliche Eigenschaften erkannt. Da ihre Bedeutung noch nicht bestätigt ist, bleiben sie unverändert.“

**Erfolgreiche Bearbeitung**

„Die Änderungen wurden geprüft und als neue Datei gespeichert. Dein ursprünglicher Spielstand ist unverändert.“

**Inkompatibilität nach einem Update**

„Dieses Spielupdate hat die Datenstruktur verändert. Einige Funktionen sind vorübergehend nicht verfügbar, bis die Kompatibilität geprüft wurde.“

Die technischen Ursachen werden intern mit ausreichenden Details protokolliert. Die reguläre UI zeigt keine Stacktraces, Bytepositionen oder internen Klassennamen.

---

# 8. Dynamische UI ohne ungefilterte technische Daten

Die dynamische UI bleibt ein zentraler Bestandteil der Architektur. Sie erhält jedoch eine zusätzliche Präsentationsregel.

Ein erkanntes Feld wird nicht automatisch zu einem sichtbaren UI-Element.

Für jedes Feld muss die Anwendung unterscheiden zwischen:

- intern erkannt;
- strukturell verstanden;
- fachlich verstanden;
- für Spieler relevant;
- sinnvoll darstellbar;
- sicher bearbeitbar.

Diese Eigenschaften sind voneinander unabhängig.

Ein Feld kann beispielsweise strukturell vollständig bekannt, aber für Spieler bedeutungslos sein. Ein anderes Feld kann für Spieler interessant sein, aber noch nicht sicher bearbeitet werden können.

Die Darstellung wird deshalb durch ein Spieler-Präsentationsmodell gesteuert.

Dieses Modell beschreibt unter anderem:

- verständliche Beschriftung;
- spielerbezogene Bedeutung;
- Kategorie und Gruppierung;
- Relevanz für eine Spieleransicht;
- Darstellungsform;
- Sichtbarkeit im jeweiligen Kontext;
- Lesbarkeit oder Bearbeitbarkeit;
- optionale Erklärung und Hinweise.

Technische Rohinformationen dürfen intern vollständig verfügbar bleiben, ohne in der normalen UI aufzutauchen.

**Wichtig:** Die Präsentationsregeln müssen selbst datengetrieben sein. Neue Inhalte sollen nicht nur erkannt, sondern – sofern ihre Spielerrelevanz und Bedeutung ausreichend bekannt sind – automatisch in passende Spieleransichten integriert werden können.

---

# 9. Design, Bedienbarkeit und visuelle Konsistenz

Die bisher festgelegten Designgrundsätze bleiben verbindlich:

- modernes Dark-Design;
- dezente Neonakzente;
- kontrollierte Glow- und Glaseffekte;
- hochwertige, wiederverwendbare Controls;
- einheitliche Abstände;
- konsistente Dropdowns, Tabellen und Slider;
- flüssige, subtile Animationen;
- persistente Layouts;
- vollständige Lokalisierung;
- klare visuelle Trennung von Information, Aktion und Warnung.

Ergänzend gilt:

Die visuelle Gestaltung darf keine technische Komplexität vortäuschen, nur um professionell zu wirken. Eine klare, ruhige Oberfläche ist wichtiger als möglichst viele Panels, Kennzahlen oder Effekte.

Häufig benötigte Aktionen müssen leicht erreichbar sein. Seltene Aktionen sollen nicht die Hauptansicht dominieren.

Tabellen und Inventare müssen auch bei großen Datenmengen übersichtlich bleiben. Relevante Informationen erhalten Vorrang vor der vollständigen Anzeige sämtlicher verfügbarer Daten.

---

# 10. Verbindliche Abnahmekriterien für die Spieleroberfläche

Ein UI-Feature gilt nur dann als abgeschlossen, wenn die folgenden Anforderungen erfüllt sind:

1. Die Funktion ist aus Sicht eines Enshrouded-Spielers verständlich.
2. Alle sichtbaren Informationen haben einen nachvollziehbaren spielbezogenen Nutzen.
3. Technische Implementierungsdetails werden nicht ungefiltert angezeigt.
4. Die Navigation richtet sich nach Spieleraufgaben.
5. Die Oberfläche zeigt nur tatsächlich verfügbare Informationen.
6. Unbekannte oder unsichere Daten werden nicht als bestätigte Spielwerte dargestellt.
7. Lesbare, aber nicht sicher bearbeitbare Informationen werden entsprechend behandelt.
8. Aktionen, Fehler und Einschränkungen sind verständlich formuliert.
9. Gemeinsame Controls und Layouts werden wiederverwendet.
10. Sämtliche sichtbaren Texte sind lokalisierbar.
11. Die Oberfläche bleibt bei zusätzlichen Feldern und neuen Inhalten übersichtlich.
12. Die interne technische Diagnose bleibt ausreichend detailliert, ohne die reguläre Spieleroberfläche zu überladen.

## Abschließende Produktentscheidung

Die Enshrouded Modding Suite wird intern wie eine leistungsfähige, datengetriebene Analyse- und Bearbeitungsplattform entwickelt, präsentiert sich nach außen jedoch als einfach zu bedienendes Werkzeug für Spieler.

Die technische Architektur bestimmt, was die Anwendung erkennen und sicher ausführen kann. Die Spieleroberfläche bestimmt, welche dieser Möglichkeiten sinnvoll, verständlich und aufgabenorientiert angeboten werden.

**Die UI ist kein Spiegel der internen Datenstruktur. Sie ist eine gezielte, verständliche Darstellung der für Spieler relevanten Möglichkeiten.**

Dieser Grundsatz gilt verbindlich für sämtliche bestehenden und zukünftigen Module.