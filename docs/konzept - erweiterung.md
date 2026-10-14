# Enshrouded Modding Suite
## Verbindliche Architekturerweiterung – Runtime Engine & World Toolkit

**Dokumentstatus:** Verbindliche Ergänzung zum Masterkonzept  
**Technologie:** C#, .NET 10, WPF, Windows 10/11  
**Produktprinzip:** Eigenständig, modular, spielerorientiert und datengetrieben

---

## 1. Erweiterte Produktvision

Die Enshrouded Modding Suite ist nicht nur ein Savegame-Editor. Sie ist eine umfassende, eigenständige Modding- und Managementplattform für Enshrouded.

Die Anwendung vereint drei große Funktionsbereiche:

1. **Savegame & Data Editing:** Spielstände, Items, Inventare, Container, Ausrüstung und unterstützte Weltinformationen untersuchen und bearbeiten.
2. **Ingame Control:** Unterstützte Spieler- und Laufzeitfunktionen während einer aktiven Spielsitzung steuern.
3. **World & Object Toolkit:** Spielobjekte untersuchen, katalogisieren und – sofern technisch möglich – im laufenden Spiel ersetzen, platzieren oder bearbeiten.

Alle drei Bereiche verwenden gemeinsame Datenmodelle, Definitionen, Validierungsregeln und ein einheitliches Designsystem. Ihre technischen Ausführungswege bleiben jedoch voneinander getrennt.

Die interne Architektur darf komplex sein. Die Benutzeroberfläche muss für normale Spieler verständlich bleiben.

## 2. Verbindliche Unabhängigkeit

Die Anwendung muss von Anfang bis Ende eigenständig funktionieren.

Folgende externe Tools dürfen nicht als Laufzeitvoraussetzung dienen:

- Cheat Engine oder fremde Cheat-Engine-Tabellen
- Builder's Companion von Turk
- Embervale Toolkit
- Andere externe Editoren oder Werkzeuge, deren Installation für unsere Kernfunktionen zwingend erforderlich wäre

Diese Projekte dürfen als funktionale und technische Referenzen untersucht werden. Ihre beobachtbaren Arbeitsweisen können analysiert und durch eigene Implementierungen nachgebildet werden, soweit dies rechtlich zulässig ist.

**Nicht zulässig als Architekturgrundlage sind unkontrollierte Abhängigkeiten von deren Prozessen, Tabellen, internen Datenstrukturen oder Bedienoberflächen.**

Die Suite muss eigene Schnittstellen, eigene Funktionsdefinitionen, eigene Laufzeitkontrollen und eine eigene Benutzeroberfläche bereitstellen.

Die Verwendung unterstützter Spielschnittstellen, eigener Mods oder selbst entwickelter Laufzeitintegrationen ist davon nicht ausgeschlossen. Die technische Umsetzung muss für jede Funktion einzeln untersucht und begründet werden.

## 3. Architekturübersicht

Die bisherige modulare Architektur wird um eine eigenständige Laufzeit- und Weltbearbeitungsschicht ergänzt.

### 3.1 Gemeinsame Plattform

Die bestehenden Kernkomponenten bleiben verantwortlich für:

- Game Data Catalog und Schema Registry
- Parser und Decoder
- Discovery Engine und Schema Evolution
- Canonical Data Model
- Capability-System und Validierung
- Lokalisierung und Designsystem
- Persistenz, Diagnose und Testinfrastruktur

Sie liefern die gemeinsame Grundlage für sämtliche Editoren.

### 3.2 Neue Funktionsmodule

**Runtime Engine**

Verwaltet die Verbindung zur laufenden Spielsitzung, die Erkennung des Spielprozesses, die Versionskompatibilität, unterstützte Laufzeitfunktionen und deren kontrollierte Aktivierung.

**Runtime Adapters**

Kapseln unterschiedliche technische Zugriffswege hinter einheitlichen Schnittstellen. Ein Adapter darf nur Funktionen bereitstellen, deren Voraussetzungen und Verhalten ausreichend untersucht und validiert wurden.

**Ingame Control Center**

Stellt aktivierbare Laufzeitfunktionen, Statusanzeigen, Profile und Hotkeys bereit.

**World & Object Toolkit**

Verwaltet platzierbare Objekte, den Prop-Swapper, Objektinformationen und weitere unterstützte Weltbearbeitungsfunktionen.

**Object Catalog**

Enthält normalisierte Definitionen von Props, Bauteilen, Materialien und weiteren Spielobjekten. Einträge enthalten, soweit bekannt, Kategorie, Herkunft, Spielversion, verfügbare Eigenschaften und Verwendungsbeschränkungen.

**Compatibility Center**

Verwaltet die Kompatibilität zwischen Spielversionen, Datenquellen, Laufzeitadaptern und einzelnen Funktionen.

**Operation & Recovery Service**

Verwaltet kontrollierte Operationen, Vorbedingungen, Änderungsprotokolle, Wiederherstellungsmöglichkeiten und nachvollziehbare Ergebnisse.

Die genaue Aufteilung in Projekte oder Assemblies wird nach Prüfung der bestehenden Solution festgelegt. Neue Module dürfen nicht ohne funktionalen Grund zusätzliche Abhängigkeitsebenen erzeugen.

## 4. Ingame Control Center

Das Ingame Control Center ermöglicht die Steuerung unterstützter Funktionen während des Spiels.

### 4.1 Vorgesehener Funktionsumfang

- God Mode beziehungsweise Unverwundbarkeit
- Unbegrenzte Ausdauer
- Unbegrenztes Mana
- Freies Fliegen und alternative Flugmodi
- Weitere unterstützte Bewegungsoptionen
- Weitere Spieler- und Laufzeitfunktionen
- Eigene Hotkeys
- Funktionsprofile und gespeicherte Einstellungen
- Übersicht über aktive Funktionen
- Hinweise zu Voraussetzungen und Kompatibilität

Diese Liste definiert das Produktziel, nicht den bereits nachgewiesenen technischen Funktionsumfang. Jede Funktion benötigt einen eigenen Machbarkeitsnachweis.

### 4.2 Einheitlicher Funktionsvertrag

Jede Laufzeitfunktion muss über eine standardisierte Definition verfügen, die mindestens folgende Informationen enthält:

- Eindeutige interne Funktionskennung
- Spielerfreundlicher Name und Beschreibung
- Voraussetzungen
- Unterstützte Spielversionen
- Benötigter Adapter und erforderliche Fähigkeiten
- Aktivierungs- und Deaktivierungsverhalten
- Erwartete Auswirkungen und bekannte Einschränkungen
- Validierungsstatus
- Fehler- und Wiederherstellungsverhalten

Die UI darf eine Funktion nicht als verfügbar oder aktiv anzeigen, wenn ihre Voraussetzungen nicht erfüllt beziehungsweise ihr Zustand nicht zuverlässig feststellbar sind.

### 4.3 Aktivierung und Deaktivierung

Funktionen werden über einen gemeinsamen Kontrollmechanismus aktiviert und deaktiviert.

Dabei gilt:

- Vor jeder Operation werden die Voraussetzungen geprüft.
- Der tatsächliche Zustand wird, soweit technisch möglich, verifiziert.
- Fehlgeschlagene Aktivierungen werden verständlich gemeldet.
- Deaktivierbare Funktionen müssen einen definierten Rückweg besitzen.
- Nicht sicher rückgängig zu machende Änderungen müssen vor der Aktivierung eindeutig gekennzeichnet werden.
- Die Anwendung darf keinen Erfolg vortäuschen, wenn lediglich ein Befehl gesendet wurde.

Hotkeys und Profile verwenden dieselben Sicherheits- und Validierungsregeln wie die manuelle Bedienung.

## 5. World & Object Toolkit

Dieser Bereich macht die Untersuchung und Bearbeitung von Weltobjekten zu einer zentralen Funktion der Suite.

### 5.1 Objektkatalog

Der Katalog soll möglichst alle zuverlässig identifizierbaren relevanten Spielobjekte erfassen.

Vorgesehene Kategorien:

- Möbel und Dekoration
- Werkbänke und interaktive Objekte
- Türen und andere Bauelemente
- Baublöcke und Baumaterialien
- Platzierbare Gegenstände
- Verborgene oder normalerweise nicht verfügbare Objekte
- Entwicklerexklusive Objekte, sofern identifizierbar und technisch nutzbar
- Weitere Objektklassen, die sich aus den Spieldaten ableiten lassen

Die Kategorien müssen erweiterbar sein. Neue Kategorien dürfen nicht jedes Mal eine grundlegende Änderung des UI-Frameworks erfordern.

### 5.2 Import und Normalisierung

Vom Nutzer bereitgestellte Objektlisten sind wichtige Datenquellen.

Der Importprozess muss:

1. Die Quelldatei und ihre Herkunft dokumentieren.
2. Das Dateiformat erkennen und validieren.
3. Einträge normalisieren und Duplikate identifizieren.
4. Bekannte Eigenschaften den kanonischen Feldern zuordnen.
5. Unbekannte Felder nach Möglichkeit erhalten.
6. Die Datenquelle und den Validierungsstatus jedes Eintrags nachvollziehbar halten.
7. Unbestätigte Informationen von verifizierten Eigenschaften unterscheiden.

Eine vorhandene Objekt-ID ist allein kein Beweis dafür, dass ein Objekt in der aktuellen Spielversion sicher platziert oder verändert werden kann.

### 5.3 Der Prop-Swapper

Der Prop-Swapper ist ein eigenständiges Kernfeature und kein bloßer Unterpunkt des Item Editors.

Der angestrebte Ablauf:

1. Der Spieler öffnet den Objektkatalog.
2. Er sucht das gewünschte Zielobjekt.
3. Er wählt das Zielobjekt aus.
4. Er nimmt einen geeigneten Platzhaltergegenstand in die Hand oder markiert ein unterstütztes Objekt.
5. Er führt die Ersetzung über die vorgesehene Ingame-Aktion aus.
6. Die Suite prüft und meldet das Ergebnis.

Der konkrete Ablauf kann von den technischen Möglichkeiten des Spiels abhängen. Die Implementierung muss zunächst für eine einzelne Objektklasse nachgewiesen werden.

Vor der Ersetzung müssen, soweit technisch feststellbar, folgende Punkte geprüft werden:

- Ist das Zielobjekt für die aktuelle Version bekannt?
- Ist die gewünschte Operation technisch unterstützt?
- Stimmen die erforderlichen Objekt- und Platzierungsvoraussetzungen?
- Kann die Operation die Welt oder den Spielstand beschädigen?
- Ist die resultierende Objektinstanz tatsächlich entstanden?
- Ist das Ergebnis nur temporär oder voraussichtlich persistent?

### 5.4 Objektklassen nicht vermischen

Props, Baublöcke, Terrain, Kreaturen und NPCs dürfen nicht pauschal als gleichartige Objekte behandelt werden.

Sie können unterschiedliche Datenmodelle, Platzierungsregeln, Laufzeitverhalten und Persistenzmechanismen besitzen. Jede Klasse benötigt daher eine passende Verarbeitung und eigene Tests.

### 5.5 Erweiterte Platzierungsfunktionen

Nach dem erfolgreichen Nachweis des Prop-Swappers können weitere Funktionen untersucht werden:

- Auswahl alternativer Objektvarianten
- Anpassung unterstützter Rotation und Position
- Erweiterte Platzierungsmöglichkeiten
- Untersuchung besonderer Objektzustände
- Wiederverwendung häufig genutzter Objekte und Einstellungen

Diese Funktionen werden nur freigeschaltet, wenn ihre technische Umsetzbarkeit nachgewiesen wurde.

## 6. Gemeinsamer Game-Data-Katalog

Die Runtime Engine, der Savegame Editor und das World & Object Toolkit verwenden eine gemeinsame Quelle normalisierter Spieldaten.

Die Definition eines Objekts muss von einer konkreten Instanz in der Welt oder im Inventar unterscheidbar bleiben.

Der Katalog trennt insbesondere:

- **Definition:** Was ein Objekt grundsätzlich ist.
- **Eigenschaften:** Welche bekannten Merkmale und Parameter es besitzt.
- **Darstellung:** Wie das Objekt in der UI angezeigt wird.
- **Laufzeitfähigkeit:** Welche Operationen für das Objekt nachgewiesen wurden.
- **Persistenz:** Welche Aussagen über die Speicherung des Objekts gesichert sind.
- **Herkunft und Version:** Woher die Daten stammen und für welche Spielversion sie gelten.

Die Datenmodelle müssen erweiterbar bleiben. Neue Felder oder Objektklassen sollen möglichst über versionierte Definitionen ergänzt werden können.

Die UI darf jedoch nicht automatisch jedes gefundene Feld anzeigen. Die Darstellung richtet sich nach der Spielerrelevanz, dem jeweiligen Kontext und dem Validierungsstatus.

## 7. Strikte Trennung von Laufzeit- und Savegame-Engine

Die beiden Engines verwenden gemeinsame Datenmodelle, besitzen aber getrennte Verantwortlichkeiten.

### Savegame-Engine

- Liest unterstützte Spielstandformate.
- Analysiert Items, Inventare, Container und gespeicherte Weltinformationen.
- Ermöglicht kontrollierte Bearbeitung unterstützter Daten.
- Schreibt Änderungen zunächst in eine Arbeitskopie.
- Prüft exportierte Dateien durch erneutes Einlesen und geeignete Vergleichstests.

### Runtime Engine

- Erkennt und überwacht die aktive Spielsitzung.
- Prüft die Kompatibilität einzelner Laufzeitfunktionen.
- Verwaltet die verfügbaren Runtime Adapters.
- Führt unterstützte Operationen kontrolliert aus.
- Verifiziert Ergebnisse und meldet Einschränkungen.

Eine erfolgreiche Operation im laufenden Spiel beweist nicht, dass die Änderung dauerhaft gespeichert wurde. Umgekehrt beweist eine erfolgreiche Änderung im Savegame nicht, dass das Spiel die Änderung zur Laufzeit akzeptiert.

Beide Ergebnisse müssen getrennt bewertet werden.

## 8. Sicherheits- und Kompatibilitätsmodell

Das bestehende Capability-System wird auf Laufzeitfunktionen und Weltoperationen erweitert.

Jede Operation erhält einen überprüfbaren Status, beispielsweise:

- Nicht untersucht
- Technisch erkannt
- Experimentell
- Für bestimmte Versionen validiert
- Unterstützt
- Nicht unterstützt
- Für die aktuelle Version gesperrt

Diese Statuswerte müssen klar definiert sein. Eine Funktion darf nicht allein aufgrund einer erkannten ID oder einer erfolgreichen Speicheroperation als vollständig unterstützt gelten.

Zusätzliche Regeln:

- Unbekannte Spielversionen führen zu einer konservativen Freigabeentscheidung.
- Nicht validierte Laufzeitoperationen dürfen nicht automatisch aktiviert werden.
- Die Anwendung muss zwischen lokalem Spiel und Mehrspielerumgebungen unterscheiden, soweit dies zuverlässig erkennbar ist.
- Bei unsicherem Zustand werden kritische Operationen blockiert oder ausdrücklich als experimentell gekennzeichnet.
- Der Schutz eigener Spielstände und die Wiederherstellbarkeit bleiben verbindliche Anforderungen.
- Technische Fehlerdetails werden intern protokolliert; die Spieleroberfläche erhält verständliche Fehlermeldungen.

## 9. Spielerorientierte UI-Erweiterung

Die Hauptnavigation wird um zwei neue Bereiche ergänzt:

**Ingame Control**

Ein zentraler Ort für unterstützte Laufzeitfunktionen, Aktivierungszustände, Hotkeys und Profile.

**World & Objects**

Ein zentraler Ort für die Objektsuche, Kategorien, Favoriten, Objektinformationen und den Prop-Swapper.

Die bisher vorgesehenen Bereiche Übersicht, Spielstände, Items, Inventar & Kisten, Waffen & Ausrüstung und Einstellungen bleiben erhalten.

Die Anwendung zeigt keine internen Speicheradressen, Bytebereiche, Parserdetails oder technischen Prozessinformationen im normalen Spieler-Workflow. Eine verständliche Statusanzeige reicht aus, etwa:

- Spiel erkannt
- Funktion verfügbar
- Funktion aktiv
- Diese Spielversion wird nicht unterstützt
- Objekt erfolgreich ersetzt
- Ergebnis konnte nicht verifiziert werden

Das Designsystem und die Lokalisierung gelten für alle neuen Bereiche gleichermaßen.

## 10. Technischer Forschungs- und Machbarkeitsplan

Vor der Implementierung der Laufzeit-Engine wird eine gezielte Untersuchung durchgeführt.

### Phase R0 – Referenzanalyse

- Funktionen von Builder's Companion und Embervale Toolkit erfassen.
- Vorhandene Objektlisten und deren Formate untersuchen.
- Bereits bekannte technische Grundlagen und offene Fragen dokumentieren.
- Eigenständige Implementierungswege identifizieren.
- Lizenz- und Nutzungsbedingungen relevanter Referenzen prüfen.

### Phase R1 – Laufzeitnachweis

- Erkennen einer unterstützten Spielsitzung.
- Ermitteln relevanter Versionsinformationen.
- Eine einzelne, risikoarme Laufzeitfunktion technisch untersuchen.
- Voraussetzungen, Grenzen und Wiederherstellung testen.
- Ergebnisse reproduzierbar dokumentieren.

### Phase R2 – Objektkatalog

- Repräsentative Objektlisten importieren.
- Einträge normalisieren und kategorisieren.
- Datenherkunft und Versionsbezug erfassen.
- Zwischen identifizierten, verfügbaren und tatsächlich nutzbaren Objekten unterscheiden.

### Phase R3 – Prop-Swapper-Prototyp

- Eine einzelne geeignete Objektklasse auswählen.
- Den Ersetzungsmechanismus untersuchen.
- Das Ergebnis im laufenden Spiel überprüfen.
- Persistenz nach Speichern und erneutem Laden testen, sofern die Operation dies ermöglicht.
- Fehlerfälle und Wiederherstellung untersuchen.

### Phase R4 – Gemeinsame Runtime Engine

Erst nach den vorherigen Nachweisen werden die abstrahierten Schnittstellen, das Capability-System und das Ingame Control Center umfassend implementiert.

### Phase R5 – Erweiterung

Weitere Laufzeitfunktionen, Objektklassen und Platzierungsoptionen werden einzeln ergänzt, getestet und für die jeweils unterstützten Versionen freigegeben.

## 11. Verbindliche Regeln für Entwicklungsagenten

Jeder Agent muss:

1. Vorhandene Implementierungen und Datenquellen zuerst untersuchen.
2. Zwischen bestätigten Tatsachen, Hypothesen und experimentellen Ergebnissen unterscheiden.
3. Keine technische Funktionsfähigkeit allein aus einer vorhandenen Objekt-ID ableiten.
4. Keine externen Tools als versteckte Laufzeitabhängigkeit einführen.
5. Vor Änderungen an gemeinsamen Schnittstellen deren Auswirkungen auf andere Module prüfen.
6. Tests und Build-Ergebnisse tatsächlich ausführen und korrekt dokumentieren.
7. Fehlerfälle, Grenzen und offene Fragen ausdrücklich benennen.
8. Neue Laufzeitfunktionen nur über die definierten Schnittstellen integrieren.
9. Die Spieleroberfläche frei von unnötigen technischen Details halten.
10. Unbekannte oder nicht unterstützte Zustände sicher behandeln.

## 12. Abnahmekriterien

Die Architekturerweiterung gilt erst dann als erfolgreich umgesetzt, wenn:

- Die Anwendung ohne Installation oder Start externer Modding-Werkzeuge funktioniert.
- Die Runtime Engine und die Savegame-Engine klar getrennt sind.
- Der Game-Data-Katalog von mehreren Modulen verwendet werden kann.
- Jede freigegebene Laufzeitfunktion einen dokumentierten und getesteten Funktionsvertrag besitzt.
- Der Objektkatalog Datenquellen, Versionen und Validierungsstatus nachvollziehbar verwaltet.
- Der Prop-Swapper mindestens für eine ausgewählte Objektklasse reproduzierbar getestet wurde, bevor er als unterstützt ausgewiesen wird.
- Nicht unterstützte Operationen zuverlässig erkannt und verständlich kommuniziert werden.
- Die Anwendung keine erfolgreiche Ausführung oder Persistenz vortäuscht.
- Die Oberfläche für Spieler verständlich bleibt.
- Die bestehenden Sicherheitsregeln für Savegames und unbekannte Daten weiterhin gelten.

---

## Abschließende Architekturentscheidung

Die Enshrouded Modding Suite wird als eigenständige, modulare Plattform entwickelt, die Savegame-Bearbeitung, Ingame-Steuerung und Weltobjektbearbeitung unter einer gemeinsamen, spielerorientierten Oberfläche vereint.

Bestehende Tools dienen als Referenz für Funktionen und Vorgehensweisen. Die Implementierung, Laufzeitkontrolle, Datenmodelle und Benutzeroberfläche werden eigenständig entwickelt.

Die technische Forschung geht der umfassenden Implementierung voraus. Jede Funktion muss ihre Machbarkeit, Kompatibilität und ihr Verhalten nachweisen, bevor sie als unterstützt freigegeben wird.

**Leitprinzip: Eine gemeinsame Plattform, getrennte technische Engines, wiederverwendbare Datenmodelle und eine einfache Bedienung für Spieler.**