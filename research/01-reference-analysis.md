# 01 – Referenzanalyse

## Builder's Companion

Die öffentlich auffindbare Nexus-Mod-Seite „Builder's Companion - CT“ ist als Funktionsreferenz relevant, war bei der Recherche aber HTTP-seitig nicht abrufbar (403). Daher werden dortige Funktionsbehauptungen nicht als verifiziert übernommen. Screenshots oder Beschreibungen allein würden außerdem nicht beweisen, ob eine Änderung ein bestehendes Objekt ersetzt, ein neues Objekt erzeugt oder nur die Darstellung verändert.

**Erkenntnisstatus:** plausibel als UI-/Funktionsreferenz, nicht als technische Implementierungsquelle. Lizenz und Quellcode-Nutzungsrecht sind nicht geklärt; kein Code, keine Tabellen und keine Assets werden übernommen.

## Embervale Toolkit

Für das Embervale Toolkit/den Prop Swapper konnte keine öffentlich dokumentierte Open-Source-Lizenz oder reproduzierbare technische Spezifikation verifiziert werden. Eine proprietäre Verteilung ist daher anzunehmen, aber nicht als Eigentums- oder Lizenzurteil zu verstehen. Nutzung als Beobachtungs- und UX-Referenz ist von Übernahme von Code, Daten oder Assets zu trennen.

## Verifizierter angrenzender Referenzpunkt

Das öffentliche Projekt [SMM-Enshrouded](https://github.com/SYSOFTEK/SMM-Enshrouded) ist MIT-lizenziert und beschreibt Serverüberwachung, Konsolenanzeige und JSON-Konfigurationsbearbeitung. Es injiziert laut eigener Dokumentation keinen Code. Es ist kein Prop Swapper und kein Runtime-Modifikationsnachweis; seine Lizenz gilt nicht für andere Projekte.

Die offizielle Serverdokumentation beschreibt `enshrouded_server.json`, `saveDirectory`, `logDirectory`, `queryPort`, Spielerplätze und Gameplay-Presets: [Dedicated Server Configuration](https://enshrouded.zendesk.com/hc/en-us/articles/16055441447709-Dedicated-Server-Configuration).

## Konsequenz

ShroudStudio implementiert eigene Datenmodelle, Validierung, Fehlerbehandlung und Protokollierung. Referenzfunktionen werden erst dann als unterstützt angezeigt, wenn Auswahl, tatsächliche Operation, Speicherung und erneutes Laden getrennt nachgewiesen sind.

**Status:** offizielle Serverkonfiguration verifiziert; Referenz-Prop-Swap-Technik und Lizenzstatus unbekannt.

## Neue Referenzen: ShroudForge und Shroudtopia

Das vom Auftraggeber genannte [ShroudForge](https://github.com/bonsaibauer/shroudforge) wurde als öffentliches GitHub-Projekt geprüft. Es ist als Referenz für Enshrouded-Modding und die Struktur eines Modprojekts relevant. Ein README-Pfad unter `main` war nicht abrufbar; deshalb werden aus der Repository-Präsenz keine ungeprüften Funktions- oder Lizenzbehauptungen abgeleitet. Eine Übernahme von Code oder Daten findet nicht statt.

[Shroudtopia](https://github.com/s0t7x/shroudtopia) beschreibt einen DLL-Modloader mit `mods`-Ordner, `ModContext`, Konfigurationssystem und Lifecycle `Load`, `Unload`, `Activate`, `Deactivate`, `Update`. Die README nennt Beispielmodi wie Flug, keine Ausdauerkosten und Ressourcenverhalten. Gleichzeitig steht dort, dass im `ModContext` derzeit keine wirklich spielbezogenen Funktionen implementiert seien. Das ist ein belastbarer Architekturhinweis, aber kein Beweis, dass eine konkrete Funktion in einer beliebigen Spielversion wirksam oder dauerhaft speicherbar ist. Das Projekt ist laut [LICENSE](https://github.com/s0t7x/shroudtopia/blob/0.1-stable/LICENSE) MIT-lizenziert; diese Lizenz gilt nur für dieses Projekt.

**Neuer Status:** Shroudtopia-Lifecycle und Lizenz verifiziert; konkrete Spielzustandswirkung, Multiplayer-Sicherheit und Persistenz weiterhin ungeklärt.
