# Forschungsauftrag: Enshrouded Modding Suite
## Technische Machbarkeit, Runtime-Modifikation und World/Object Toolkit

**Dokumenttyp:** Verbindlicher Forschungs- und Machbarkeitsauftrag  
**Priorität:** Hoch  
**Projektphase:** Research & Feasibility – vor der eigentlichen Implementierung  
**Zielplattform:** Windows 10/11, C# / .NET 10 / WPF  
**Projektstatus:** Bestehende Projektstruktur und bereits implementierte Funktionen müssen zunächst ermittelt werden.

---

# 1. Ziel des Forschungsauftrags

Untersuche systematisch, welche Möglichkeiten die aktuelle Version von Enshrouded für eine eigenständige Modding Suite bietet.

Im Mittelpunkt stehen drei große Aufgabenbereiche:

1. **Savegame & Data Engine:** Spielstände, Gegenstände, Inventare und Lagerstrukturen lesen, analysieren und kontrolliert bearbeiten.
2. **Runtime Engine:** Laufzeitfunktionen wie God Mode, unbegrenzte Ausdauer, unbegrenztes Mana und Flugsteuerung ermöglichen.
3. **World & Object Toolkit:** Spielobjekte identifizieren, katalogisieren und untersuchen sowie einen Prop Swapper entwickeln, mit dem sich Objekte aus einer Auswahl durch andere Objekte ersetzen lassen – einschließlich geeigneter Objekte, die regulär nicht für Spieler verfügbar sind.

Das Ergebnis soll keine bloße Sammlung von Internetlinks oder Vermutungen sein. Erstelle eine nachvollziehbare technische Entscheidungsgrundlage mit überprüfbaren Erkenntnissen, reproduzierbaren Tests, klar benannten Risiken und einer priorisierten Umsetzungsempfehlung.

**Wichtig:** Der Forschungsauftrag autorisiert zunächst Untersuchung und begrenzte Machbarkeitstests, nicht die ungeprüfte Implementierung sämtlicher Funktionen.

# 2. Verbindliche Architekturentscheidungen

## 2.1 Vollständige Eigenständigkeit

Die Enshrouded Modding Suite muss unabhängig von folgenden Projekten funktionieren:

- Cheat Engine und vorhandenen Cheat Tables
- Builder’s Companion
- Embervale Toolkit
- Fremden Prop-Swapper-Implementierungen
- Sonstigen externen Modding-Tools, deren laufende Installation oder Ausführung vorausgesetzt werden müsste

Diese Projekte dürfen als technische und funktionale Referenzen untersucht werden. Ihre Existenz darf jedoch nicht zur Voraussetzung für die Funktionsfähigkeit unserer Suite werden.

Insbesondere gilt:

- Keine Cheat-Engine-Installation als Voraussetzung.
- Keine externen Tabellen, die zur Laufzeit geladen werden müssen.
- Keine versteckten Aufrufe fremder Programme.
- Keine Übernahme fremder Komponenten ohne vorherige Lizenzprüfung.
- Keine bloße Umbenennung oder Verpackung eines bestehenden Tools.
- Eigene Schnittstellen, eigene Datenmodelle, eigene Validierung und eigene Fehlerbehandlung.

Falls eine bestimmte Funktion nur mit einem externen Tool möglich sein sollte, ist das als Einschränkung zu dokumentieren. Es darf nicht durch eine vermeintlich eigene Implementierung kaschiert werden.

## 2.2 Klare Trennung der Engines

Die Untersuchung muss folgende Bereiche getrennt betrachten:

**A. Savegame & Data Engine**

Arbeitet primär offline mit Spielständen, Spieldaten, Inventaren, Truhen und Gegenstandsdefinitionen.

**B. Runtime Engine**

Untersucht und steuert geeignete Eigenschaften einer laufenden lokalen Spielinstanz.

**C. World & Object Toolkit**

Verwaltet Objektdaten, Kataloge, Ersetzungsregeln und gegebenenfalls Operationen an Objekten in der laufenden Spielwelt.

**D. Shared Game Data Catalog**

Stellt gemeinsame, normalisierte Definitionen für Items, Objekte, Varianten, Kategorien und bekannte Eigenschaften bereit.

Die Engines dürfen gemeinsame Datenmodelle und geprüfte Dienste verwenden. Sie dürfen jedoch nicht ungeprüft dieselben Schreibmechanismen verwenden. Eine Änderung im Arbeitsspeicher ist keine Savegame-Änderung, und eine Änderung im Savegame ist kein Beweis dafür, dass ein Objekt zur Laufzeit korrekt erzeugt oder angezeigt werden kann.

# 3. Phase R0 – Projektinventur und Referenzanalyse

## 3.1 Bestehendes Projekt untersuchen

Prüfe zuerst den tatsächlichen Zustand des vorhandenen Repositories.

Erfasse:

- Verzeichnisstruktur und Projektdateien
- Ziel-Framework und verwendete Bibliotheken
- Bereits vorhandene Parser und Datenmodelle
- Vorhandene Savegame-Analyse und Schreibfunktionen
- Vorhandene Item- und Objektlisten
- Bestehende Tests und Testdaten
- Bereits getroffene Architekturentscheidungen
- Offene Fehler, technische Schulden und erkennbare Blockaden

Erstelle keine parallele Architektur, bevor du geprüft hast, welche Teile bereits existieren.

**Ergebnis:** `research/00-project-inventory.md`

## 3.2 Builder’s Companion und Embervale Toolkit analysieren

Untersuche die verfügbaren Versionen und öffentlich zugänglichen Informationen zu Builder’s Companion und Embervale Toolkit, insbesondere die Funktionen im Bereich Objektanpassung und Prop Swapping.

Beantworte, soweit sich das anhand belastbarer Quellen feststellen lässt:

- Welche Benutzeraktionen sind möglich?
- Wie wird ein Objekt ausgewählt?
- Wird ein vorhandenes Objekt ersetzt oder ein neues erzeugt?
- Wie werden nicht regulär verfügbare Objekte ausgewählt?
- Welche Einschränkungen gelten für Objekttypen, Positionen, Rotation und Skalierung?
- Erfolgt die Änderung lokal, serverseitig oder über einen anderen Mechanismus?
- Bleibt die Änderung nach Speichern und erneutem Laden bestehen?
- Welche Spielversionen werden unterstützt?
- Welche Voraussetzungen, Abhängigkeiten und bekannten Fehler gibt es?
- Welche Lizenzbedingungen gelten für Quellcode, Daten, Assets und Dokumentation?

Trenne dokumentierte Fakten, eigene Beobachtungen und bloße Hypothesen.

Übernimm weder fremde Implementierungen noch ungeprüfte technische Annahmen.

**Ergebnis:** `research/01-reference-analysis.md`

## 3.3 Verfügbare Objekt- und Itemlisten untersuchen

Durchsuche die bereits vorhandenen Projektdateien und vom Auftraggeber bereitgestellten Listen.

Ermittle:

- Welche Quellen vorhanden sind und woher sie stammen.
- Welche Kennungen und Namenskonventionen verwendet werden.
- Welche Einträge Items, Weltobjekte, Bauteile, Dekorationen oder andere Kategorien darstellen.
- Ob Varianten, Platzierungsdefinitionen oder zusätzliche Metadaten enthalten sind.
- Ob doppelte, veraltete oder widersprüchliche Einträge existieren.
- Welche Informationen sich mit aktuellen Spieldaten abgleichen lassen.
- Welche rechtlichen oder lizenzbezogenen Einschränkungen bei einer Übernahme gelten könnten.

**Wichtig:** Eine Objekt-ID allein ist kein Nachweis dafür, dass das Objekt erzeugt, platziert, ersetzt, bearbeitet oder dauerhaft gespeichert werden kann.

**Ergebnis:** `research/02-data-source-inventory.md`

# 4. Phase R1 – Runtime Machbarkeitsnachweis

Ziel ist eine belastbare Aussage darüber, ob und auf welchem Weg eine eigenständige Runtime Engine realisierbar ist.

## 4.1 Mögliche technische Ansätze untersuchen

Prüfe geeignete, rechtlich und technisch vertretbare Verfahren für die Kommunikation mit einer laufenden lokalen Spielinstanz.

Untersuche insbesondere:

- Welche Spielprozesse und Spielzustände zuverlässig identifiziert werden können.
- Welche relevanten Informationen zur Laufzeit zugänglich sind.
- Ob geeignete offizielle oder dokumentierte Schnittstellen existieren.
- Welche Unterschiede zwischen direkter Prozessinteraktion, unterstützten Modding-Schnittstellen und anderen Ansätzen bestehen.
- Ob Änderungen an Spielzuständen tatsächlich wirksam werden.
- Welche Auswirkungen Updates auf die Zuverlässigkeit haben.
- Welche Voraussetzungen und Sicherheitsrisiken entstehen.

Verwende zunächst den risikoärmsten geeigneten Ansatz. Implementiere keine invasive oder umfangreiche Runtime-Infrastruktur, bevor ein kleiner Machbarkeitsnachweis vorliegt.

## 4.2 Erster Proof of Concept

Wähle eine möglichst risikoarme Funktion, anhand derer sich der grundlegende Runtime-Zugriff überprüfen lässt.

Der Test soll nach Möglichkeit zeigen:

1. Die lokale Spielinstanz kann zuverlässig erkannt werden.
2. Der relevante Zustand kann ermittelt werden.
3. Eine klar begrenzte Änderung ist technisch möglich.
4. Die Wirkung der Änderung kann unabhängig überprüft werden.
5. Die Änderung lässt sich wieder deaktivieren oder sicher zurücksetzen.
6. Ein Spielupdate oder eine fehlende Voraussetzung führt nicht zu einer fälschlich als erfolgreich gemeldeten Operation.

Verwende ein Testprofil oder eine entbehrliche Testumgebung. Vermeide Änderungen, die einen Spielstand beschädigen oder den Verlust von Fortschritt verursachen könnten.

## 4.3 Multiplayer separat untersuchen

Unterscheide ausdrücklich zwischen:

- Einzelspieler
- Lokal gehostetem Spiel
- Dediziertem Server
- Clientseitiger Darstellung
- Serverseitig autoritativem Spielzustand

Leite aus einem erfolgreichen Einzelspielertest keine Unterstützung für dedizierte Server ab.

Dokumentiere, welche Funktionen möglicherweise nur lokal wirken, welche eine Serverunterstützung benötigen und welche noch ungeklärt sind.

**Ergebnis:** `research/03-runtime-feasibility.md`

# 5. Phase R2 – Objektkatalog und Datenmodell

Entwirf auf Grundlage der tatsächlichen Datenquellen ein normalisiertes Datenmodell für den Object Catalog.

## 5.1 Vorgesehene Informationen

Prüfe, welche der folgenden Informationen für einzelne Objekte verfügbar sind:

- Stabile interne Kennung
- Anzeigename und Lokalisierung
- Objektkategorie
- Herkunft und Datenquelle
- Bekannte Spielversionen
- Zugehörige Varianten
- Platzierungs- oder Erzeugungsinformationen
- Positionierungs- und Rotationsmöglichkeiten
- Skalierung, sofern unterstützt
- Benötigte Voraussetzungen
- Bekannte Abhängigkeiten
- Bekannter Runtime-Status
- Bekannter Speicherstatus
- Validierungs- und Kompatibilitätsergebnisse

Nicht verfügbare Informationen sind als unbekannt zu kennzeichnen. Fehlende Werte dürfen nicht durch erfundene Standardwerte ersetzt werden.

## 5.2 Herkunft und Vertrauenswürdigkeit

Jeder Eintrag muss nach Möglichkeit nachvollziehbar machen, woher er stammt und wie zuverlässig er geprüft wurde.

Unterscheide mindestens:

- Aus einer Quelle übernommen, noch nicht validiert
- Gegen aktuelle Spieldaten abgeglichen
- Technisch identifiziert
- Experimentell getestet
- Erfolgreich erzeugt oder ersetzt
- Nach Speichern und erneutem Laden bestätigt
- Veraltet oder inkompatibel
- Ungeklärt

Die Datenherkunft muss auch bei Aktualisierungen nachvollziehbar bleiben.

## 5.3 Versionierung und Aktualisierung

Untersuche, wie Änderungen zwischen Spielversionen erkannt und eingepflegt werden können.

Der Katalog soll langfristig aktualisierbar sein, ohne dass jede Änderung eine manuelle Neuentwicklung der Benutzeroberfläche erfordert.

**Ergebnis:** `research/04-object-catalog-design.md`

# 6. Phase R3 – Prop Swapper Machbarkeitsnachweis

Dies ist einer der wichtigsten Teile des Auftrags.

Untersuche, ob sich ein Objekt, das der Spieler hält oder gezielt auswählt, durch ein anderes Objekt ersetzen lässt. Als Referenz dient die gewünschte Funktionalität des früheren Builder’s Companion beziehungsweise des Embervale Toolkit.

## 6.1 Konkreter Testfall

Verwende eine gewöhnliche, in der Testumgebung verfügbare Sitzgelegenheit, beispielsweise einen Stuhl, als Ausgangsobjekt.

Wähle aus dem eigenen Objektkatalog ein anderes Objekt aus. Wenn technisch und rechtlich vertretbar, soll darunter auch ein Objekt sein, das im normalen Spielablauf nicht regulär verfügbar ist.

Untersuche anschließend, ob sich das gewünschte Ergebnis mit einer eigenständigen Implementierung erreichen lässt.

Prüfe getrennt:

1. Ist die Zielkennung gültig?
2. Kann das Zielobjekt technisch erzeugt oder ausgewählt werden?
3. Kann das bestehende Objekt tatsächlich ersetzt werden?
4. Bleiben Position und Orientierung erhalten?
5. Werden unerwartete Nebenwirkungen auf benachbarte Objekte vermieden?
6. Ist das Ergebnis visuell korrekt?
7. Kann der Spieler das Objekt weiterhin erwartungsgemäß verwenden?
8. Bleibt das Ergebnis nach Speichern und erneutem Laden bestehen?
9. Was passiert, wenn das Zielobjekt ungültig oder inkompatibel ist?
10. Kann der Vorgang rückgängig gemacht werden?

Nicht alle Punkte müssen im ersten Prototyp erfolgreich sein. Sie müssen aber einzeln bewertet werden.

## 6.2 Ersatz und Erzeugung nicht verwechseln

Ein neu erzeugtes Objekt neben dem alten Objekt ist kein erfolgreicher Ersatz.

Ebenso ist eine geänderte Darstellung kein Beweis dafür, dass der zugrunde liegende Weltzustand korrekt geändert wurde.

Dokumentiere deshalb die tatsächlich beobachtete Wirkung und unterscheide zwischen:

- Reiner Darstellung
- Erzeugung eines zusätzlichen Objekts
- Tatsächlichem Austausch eines vorhandenen Objekts
- Veränderung des zugrunde liegenden Weltzustands
- Dauerhafter Speicherung

## 6.3 Fehlerbehandlung und Wiederherstellung

Untersuche, wie sich fehlgeschlagene Ersetzungen erkennen und behandeln lassen.

Der Prototyp darf bei einem Fehler nicht kommentarlos das Ausgangsobjekt löschen oder einen inkonsistenten Zustand hinterlassen.

Wo eine sichere Rücknahme technisch nicht gewährleistet werden kann, muss der Versuch entsprechend begrenzt und als experimentell gekennzeichnet werden.

**Ergebnis:** `research/05-prop-swapper-poc.md`

# 7. Phase R4 – Funktionskatalog für die Runtime Engine

Erstelle einen priorisierten Katalog möglicher Laufzeitfunktionen.

Mindestens folgende Funktionen sind zu untersuchen:

| Funktion | Zu untersuchende Fragen |
|---|---|
| God Mode | Welche Schadensarten können beeinflusst werden? |
| Unbegrenzte Ausdauer | Ist eine Änderung möglich und bleibt sie wirksam? |
| Unbegrenztes Mana | Welche Ressourcenmechanismen sind betroffen? |
| Freies beziehungsweise unbegrenztes Fliegen | Welche Flug- und Bewegungsmechanismen existieren? |
| Bewegungssteuerung | Lassen sich Geschwindigkeit oder weitere Parameter zuverlässig beeinflussen? |
| Ressourcensteuerung | Welche Spielerressourcen sind technisch zugänglich? |
| Zustandskontrolle | Welche weiteren nützlichen Spielerzustände sind beeinflussbar? |
| Runtime-Objektoperationen | Welche Objektoperationen können sicher ausgeführt werden? |

Diese Liste ist ein Untersuchungsumfang, keine Zusage, dass jede Funktion implementierbar ist.

Für jede Funktion muss ein eigener Datensatz angelegt werden.

Erfasse mindestens:

- Funktionsname und eindeutige Kennung
- Technischer Ansatz
- Unterstützte Spielversionen
- Voraussetzungen
- Einzelspieler- und Multiplayerstatus
- Aktivierungs- und Deaktivierungsverhalten
- Nachweis der tatsächlichen Wirkung
- Bekannte Einschränkungen
- Risiken für Spielstand und Spielzustand
- Verhalten nach einem Neustart
- Rücksetzbarkeit
- Teststatus und letzte Überprüfung

Unterscheide dabei insbesondere zwischen einer vorübergehenden Laufzeitwirkung und einer dauerhaft gespeicherten Änderung.

**Ergebnis:** `research/06-runtime-feature-matrix.md`

# 8. Phase R5 – Architektur- und Implementierungsempfehlung

Erst wenn die wesentlichen Machbarkeitstests abgeschlossen sind, soll eine konkrete technische Architektur für die neuen Komponenten vorgeschlagen werden.

Berücksichtige dabei folgende Module:

- `EMS.RuntimeEngine`
- `EMS.RuntimeAdapters`
- `EMS.IngameControlCenter`
- `EMS.WorldObjectToolkit`
- `EMS.ObjectCatalog`
- `EMS.CompatibilityCenter`
- `EMS.OperationRecovery`

Die Namen sind Vorschläge und müssen mit der tatsächlich vorhandenen Projektstruktur abgeglichen werden.

## 8.1 Runtime Engine

Verantwortlich für die kontrollierte Ausführung und Überprüfung von Laufzeitoperationen.

Sie soll nicht an eine bestimmte Spielversion oder einen einzelnen technischen Ansatz gekoppelt sein, sofern die Untersuchung mehrere geeignete Varianten ergibt.

## 8.2 Runtime Adapters

Kapseln konkrete technische Zugriffswege. Nicht unterstützte oder inkompatible Varianten müssen erkannt werden, bevor eine riskante Operation gestartet wird.

## 8.3 Ingame Control Center

Bietet dem Spieler eine verständliche Oberfläche für verfügbare Laufzeitfunktionen.

Es zeigt nur Funktionen an, deren Voraussetzungen geprüft wurden, und macht ihren tatsächlichen Status nachvollziehbar.

## 8.4 World & Object Toolkit

Bündelt die Auswahl und Untersuchung von Objekten sowie nachgewiesene Operationen zur Erzeugung, Veränderung oder Ersetzung.

Es darf keine erfolgreiche Bearbeitung suggerieren, wenn lediglich eine Objektkennung vorhanden ist.

## 8.5 Compatibility Center

Verwaltet die geprüfte Kompatibilität mit Spielversionen, Voraussetzungen und bekannten Einschränkungen.

Ungeprüfte Funktionen müssen als experimentell oder nicht verifiziert erscheinen.

## 8.6 Operation & Recovery Service

Verwaltet risikobehaftete Operationen, deren Ergebnisse und – soweit technisch möglich – Wiederherstellungsinformationen.

Die Architektur soll nachvollziehbar festhalten können, was angefordert wurde, was tatsächlich ausgeführt wurde und welches Ergebnis bestätigt werden konnte.

**Ergebnis:** `research/07-architecture-recommendation.md`

# 9. Verbindliches Sicherheits- und Kompatibilitätsmodell

Entwickle ein einheitliches Modell, das für Runtime- und Weltobjektoperationen verwendet werden kann.

Jede Funktion benötigt einen überprüfbaren Capability-Vertrag mit mindestens:

- Voraussetzungen
- Unterstützten Spielversionen
- Unterstützten Betriebsarten
- Aktivierungsbedingungen
- Deaktivierungsbedingungen
- Verifikationsmethode
- Bekannten Einschränkungen
- Fehlerzuständen
- Wiederherstellungsstrategie
- Status der letzten erfolgreichen Prüfung

Die Benutzeroberfläche muss zwischen folgenden Zuständen unterscheiden können:

- Nicht verfügbar
- Voraussetzungen fehlen
- Untersucht, aber nicht getestet
- Experimentell
- Erfolgreich getestet
- Für die aktuelle Spielversion bestätigt
- Inkompatibel
- Fehlerhaft oder vorübergehend deaktiviert

**Eine abgesendete Operation ist kein Erfolgsnachweis.** Erst eine geeignete Überprüfung darf den Status auf erfolgreich setzen.

Die Lösung muss auch mit unerwarteten Spielversionen, fehlenden Daten, ungültigen Objektkennungen und abgebrochenen Operationen kontrolliert umgehen.

# 10. Anforderungen an die Benutzeroberfläche

Die Suite richtet sich an Spieler, nicht an Entwickler von Parsern oder Runtime-Adaptern.

Die Untersuchung soll deshalb bereits die spätere Benutzerführung berücksichtigen.

Vorgesehene Hauptbereiche:

- Übersicht
- Spielstände
- Items
- Inventar & Truhen
- Waffen & Ausrüstung
- Ingame Control
- Welt & Objekte
- Einstellungen

Technische Diagnosen, interne Speicherstrukturen und Parserdetails gehören in einen getrennten Diagnosebereich. Sie dürfen nicht die normale Spieleroberfläche dominieren.

Der Object Catalog soll sinnvoll durchsuchbar und filterbar sein. Wo möglich, sollen Objekte mit verständlichen Namen, Kategorien, Varianten und visuellen Vorschauen dargestellt werden.

Runtime-Funktionen benötigen eine verständliche Zustandsanzeige sowie klare Hinweise auf fehlende Voraussetzungen und Einschränkungen.

Die Untersuchung darf die UI-Konzeption vorbereiten, aber keine umfangreiche Oberfläche auf Grundlage noch unbestätigter Funktionen bauen.

# 11. Arbeitsweise und Regeln für den ausführenden Agenten

## 11.1 Belege statt Annahmen

Kennzeichne jede wesentliche Erkenntnis als eine der folgenden Kategorien:

- **Verifiziert:** durch belastbare Quelle oder reproduzierbaren Test bestätigt.
- **Plausibel:** durch Indizien gestützt, aber noch nicht ausreichend bestätigt.
- **Hypothese:** möglicher Ansatz, der erst getestet werden muss.
- **Widerlegt:** durch Quellen oder Tests widerlegt.
- **Unbekannt:** derzeit nicht ausreichend geklärt.

Führe für technische Aussagen die Quelle oder den reproduzierbaren Test an.

## 11.2 Keine vorgetäuschten Ergebnisse

Behaupte nicht, eine Funktion getestet zu haben, wenn lediglich Dokumentation, Quellcode oder eine Objektliste untersucht wurden.

Behaupte nicht, dass ein Objekt dauerhaft gespeichert wird, solange kein entsprechender Speichern-und-Neuladen-Test durchgeführt wurde.

Wenn ein Test aufgrund fehlender Voraussetzungen nicht möglich ist, dokumentiere das Hindernis, statt ein positives Ergebnis zu unterstellen.

## 11.3 Kontrollierte Implementierung

Kleine, isolierte Proofs of Concept sind erlaubt und erwünscht.

Vor größeren Implementierungen müssen die entsprechenden Machbarkeitsnachweise vorliegen.

Verändere bestehende produktive Savegames nicht ohne ausdrückliche Freigabe und sichere Testumgebung. Überschreibe keine Originaldateien, wenn der Test mit einer Arbeitskopie durchgeführt werden kann.

## 11.4 Lizenz- und Quellenprüfung

Prüfe bei fremden Projekten, Quellcodes, Listen und Assets, welche Nutzung zulässig ist.

Unterscheide zwischen:

- Eigenständiger Implementierung eines beobachteten Verhaltens
- Nutzung öffentlich dokumentierter Schnittstellen
- Übernahme von Daten
- Übernahme von Quellcode
- Übernahme von Assets

Wenn die rechtliche Situation unklar ist, dokumentiere die Unsicherheit und vermeide eine ungeprüfte Übernahme.

## 11.5 Nachvollziehbare Entwicklung

Falls Code angelegt oder verändert wird:

- Bestehende Konventionen beachten.
- Änderungen klein und überprüfbar halten.
- Tests für neue Logik ergänzen.
- Projekt bauen, soweit die Umgebung dies ermöglicht.
- Testergebnisse und nicht ausführbare Tests ausdrücklich dokumentieren.
- Keine ungenutzten Abstraktionen allein auf Verdacht einführen.
- Keine unbestätigten Funktionen als fertig oder unterstützt ausgeben.

# 12. Erwartete Ergebnisse

Lege die Forschungsdokumente im bestehenden Repository unter `research/` ab, sofern dort kein passenderer, bereits etablierter Dokumentationsbereich existiert.

Mindestens folgende Ergebnisse sind erforderlich:

1. `00-project-inventory.md` – Zustand des vorhandenen Projekts.
2. `01-reference-analysis.md` – Analyse der Referenzwerkzeuge.
3. `02-data-source-inventory.md` – Untersuchung vorhandener Objekt- und Itemdaten.
4. `03-runtime-feasibility.md` – Runtime-Machbarkeit und erster Proof of Concept.
5. `04-object-catalog-design.md` – Vorschlag für den Objektkatalog.
6. `05-prop-swapper-poc.md` – Machbarkeit des Objektaustauschs.
7. `06-runtime-feature-matrix.md` – Funktionsmatrix für Runtime-Features.
8. `07-architecture-recommendation.md` – Architektur und Umsetzungsempfehlung.
9. `08-final-research-report.md` – Zusammenfassung, Entscheidungen und nächste Schritte.

Wenn einzelne Untersuchungen nicht durchführbar sind, erstelle trotzdem den zugehörigen Bericht mit den bereits gewonnenen Erkenntnissen und den noch offenen Fragen.

Jeder Bericht soll Quellen, Testbedingungen, Ergebnisse, Einschränkungen und den aktuellen Erkenntnisstatus enthalten.

# 13. Priorisierung und Abbruchkriterien

Die Reihenfolge ist verbindlich:

1. Bestehendes Projekt und Datenquellen verstehen.
2. Referenzwerkzeuge und Nutzungsbedingungen untersuchen.
3. Einen risikoarmen Runtime-Proof-of-Concept durchführen.
4. Objektkatalog und Datenmodell vorbereiten.
5. Einen begrenzten Prop-Swapper-Prototyp testen.
6. Erst danach die gemeinsame Architektur konkretisieren.
7. Weitere Funktionen anhand der Ergebnisse priorisieren.

Wenn eine zentrale Funktion technisch nicht machbar ist oder sich nicht zuverlässig überprüfen lässt, muss das frühzeitig gemeldet werden.

Es ist ausdrücklich erlaubt, zu dem Ergebnis zu kommen, dass eine Funktion mit den aktuell verfügbaren Methoden nicht zuverlässig umsetzbar ist. Ein frühzeitiges negatives Ergebnis ist wertvoller als eine aufwendige, unzuverlässige Implementierung.

# 14. Definition of Done

Der Forschungsauftrag gilt erst dann als abgeschlossen, wenn:

- Die bestehende Projektstruktur und die vorhandenen Datenquellen dokumentiert sind.
- Die relevanten Referenzwerkzeuge und ihre Lizenzbedingungen untersucht wurden.
- Die technische Machbarkeit des Runtime-Zugriffs nachvollziehbar bewertet wurde.
- Ein konkreter Runtime-Test durchgeführt wurde oder nachvollziehbar dokumentiert ist, weshalb er nicht durchgeführt werden konnte.
- Der Object Catalog als Datenmodell begründet vorgeschlagen wurde.
- Der Prop-Swapper-Ansatz anhand eines konkreten Testfalls untersucht wurde.
- Runtime-Funktionen nach Machbarkeit, Risiken und Priorität bewertet wurden.
- Die Unterschiede zwischen Laufzeitwirkung, Objekterzeugung, tatsächlichem Austausch und dauerhafter Speicherung dokumentiert sind.
- Eine mit der bestehenden Architektur vereinbare Implementierungsempfehlung vorliegt.
- Offene Fragen, technische Blockaden und die nächsten Arbeitsschritte klar benannt sind.
- Alle durchgeführten Tests ehrlich und reproduzierbar dokumentiert wurden.

# 15. Abschlussbericht: Entscheidungsvorlage

Der Abschlussbericht muss mit einer klaren Entscheidungsvorlage enden.

Beantworte darin ausdrücklich:

1. Ist eine eigenständige Runtime Engine grundsätzlich realisierbar?
2. Welche Runtime-Funktion eignet sich am besten als erster produktiver Kandidat?
3. Ist ein eigenständiger Prop Swapper technisch machbar?
4. Welche Objektklassen sind nachweislich geeignet?
5. Welche Voraussetzungen gelten für nicht regulär verfügbare Objekte?
6. Welche Ergebnisse bleiben nach einem Neustart beziehungsweise erneutem Laden erhalten?
7. Welche Funktionen sind lokal möglich und welche erfordern weitere Untersuchungen für Multiplayer?
8. Welche Datenquellen können zuverlässig für einen eigenen Object Catalog verwendet werden?
9. Welche Teile der bestehenden Architektur können wiederverwendet werden?
10. Welche Implementierungen sollten als Nächstes beginnen?
11. Welche Risiken oder offenen Fragen könnten das Projekt wesentlich verändern?
12. Welche Funktionen sollten vorerst ausdrücklich nicht implementiert werden?

Ordne jede geplante Funktion einer Kategorie zu:

- **Jetzt implementieren**
- **Nach weiterem Proof of Concept implementieren**
- **Weitere Forschung erforderlich**
- **Derzeit nicht empfehlenswert**

Begründe jede Einstufung.

---

## Abschließende Anweisung

Arbeite wie ein technischer Research Engineer, nicht wie ein Agent, der möglichst schnell möglichst viel Code produziert.

Das Ziel ist eine belastbare Grundlage für eine professionelle, eigenständige Enshrouded Modding Suite. Technische Neugier und experimentelle Ansätze sind ausdrücklich erwünscht. Sie müssen jedoch durch überprüfbare Ergebnisse, saubere Architekturentscheidungen und eine ehrliche Bewertung der Grenzen abgesichert werden.

**Beginne mit R0. Lege zunächst den tatsächlichen Projektstand offen, untersuche die verfügbaren Quellen und arbeite dich anschließend systematisch durch die Machbarkeitsphasen. Überspringe keine Phase, nur weil eine spätere Funktion besonders interessant erscheint.**