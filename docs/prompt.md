Ich möchte, dass du den Funktionsumfang jetzt fertigstellst. Entschlüssele die Savegames, nimm dir dafür externe Quellen zu Hilfe und schau was die machen. Nutze die Erkenntnisse für unser Tool und baue alle Funktionen anhand der gewonnenen Erkenntnisse. Kopiere nichts, sondern baue alles in einem eigenen Stil.

Schau in anderen Tools, wie die Seiten dort angeordnet sind, welche Informationen geholt werden, was alles bearbeitet werden kann. Vor allem wichtig ist, dass wir ähnliche Funktionen übernehmen/anbieten. Versuche alles so Spielerfreundlich wie möglich zu halten.

Ich möchte einen vollständigen Welt- und Charaktereditor aus den Savegames und den Spieldaten am Ende bekommen.

---

Ganz WICHTIG:
- das Tool muss erweiterbar sein, mehr oder weniger modular aufgebaut sein. Nicht so modular, dass man unabhängig Mods entwickeln kann, aber dass sich das System selbst weiter entwickeln kann
- das Tool muss in der Lage sein, die Spieldaten und die Savegames neu zu analysieren, Änderungen zu erkennen und Anpassungen in eigenen Vorgehensweisen vornehmen kann. Das hat den Hintergrund, dass das Tool sich nach einem Update selbst durch die Spieldaten kämpfen, Unterschiede feststellen und verwenden kann, ohne dass ich ständig Updates nachschieben muss. Nur wenn generelle Spieländerungen sind oder Mechaniken dazukommen, sollte ich ein Update erstellen müssen. Spieldaten sollen aber immer automatisch ausgelesen, erkannt und zugeordnet werden.
- es muss immer alles Multilingual sein, in allen Sprachen die Enshrouded unterstützt. (Da wir auf dessen Spieldaten zurückgreifen können)

Die UI muss aussergewöhnlich sein:
- Animierter Hintergrund
- realistische Glasoptik
- wiederverwendete Layouts
- generell genutztes Design
- vollständig Dark-themed (auch Comoboxen, Dropdown)
- eigene Slidebars
- absolut professionell
- verwende weiterhin `docs\images` als Zielvorgabe des Designs und Layouts. Die Anordnung, Informationen können angepasst werden, wie es am sinnvollsten ist. Das Logo wurde ebenfalls neu erstellt
- das bisherige Design/Layout gefällt mir ganz gut, aber es fehlt der rote Faden, der Pepp, das Alleinstellungsmerkmal, die Animationen (ich sehe zumindest keine), die Übergänge, das Gesamtkonzept für ein Mega-Tool. Ich will auch kein Tool wie die bisherigen, sondern etwas aussergewöhnliches, nicht nur vom Aussehen her, sondern auch vom Funktionsumfang.

Der Funktionsumfang muss mindestens bieten:
- sobald das Tool auf GitHub ist, soll es eine Auto-Update-Funktion haben
  - wenn das Tool fertig ist, veröffentlichen wir es auf GitHub
  - ab dann soll eine fertige und funktionierende Autoupdate-Funktion greifen, die automatisch erkennt wenn ein Update verfügbar ist und dem Benutzer anbietet es zu installieren
  - der Hinweis muss dezent sein und nicht zu aufdringlich sein
- alles muss zu 100% Multilingual sein
- Overview
  - schaue hier, welche interessanten Informationen aus den geladenen Savegames für Welt und Charakter hier zusammengetragen werden können.
  - wir können auch einen Bereich einbauen, in dem wir Quick Actions anbieten
- Equipment
  - hier möchte ich wie gesagt ein Equipment-Slot-System abgebildet sehen. Zugeschnitten auf Enshrouded. Beachtet werden müssen die beiden Aktionsleisten berücksichtigt und einzeln dargestellt werden
  - ebenfalls berücksichtigt werden muss, dass es auch Prestige-Kleidung gibt
    - es gibt die effektive Kleidung mit Boni/Buffs/Stats
    - es gibt die optische Kleidung, die über der effektiven Kleidung liegt, aber keine Effekte hat
    - Kleidung kann eingefärbt werden
  - Waffen haben verschiedene Stufen, Verbesserungen, Slots für Edelsteine mit Buffs (immer nur 1 Edelstein)
  - Bei Waffen wird je nach Stufe ein weiterer Buf freigeschaltet. Die Buffs müssen erkannt und mit Name und Beschreibung dargestellt werden
  - Idealerweise kann man die Buffs auf Waffen über unser Tool anpassen
- Inventar
  - neben den Aktionsleisten, die ja auch eine Art Inventar darstellen, gibt es noch den Rucksack mit dem eigentlichen Inventar. Dieses muss in einem Grid dargestellt werden.
  - Die vorhandene Grid-Größe wird durch die Art von Rucksack bestimmt und muss aus den Daten ermittelt werden
  - es wäre schön, wenn man die Grid-Größe manipulieren kann, in dem man dem Charakter die entsprechenden Rucksäcke gibt
    - wenn es möglich ist, die Rucksackgröße per Hack/Cheat zu modifizieren, könnte man die Option mit einbauen/darstellen
- Tools
  - in den Tools möchte ich später Hacks anbieten wie:
    - Items duplizieren
    - kostenlos bauen
    - Godmode, Unendlich Ausdauer, Unendlich Mana, Unendlich Shroud-Time, Unendlich Out-Of-Border-Time, Free exploring, free flight mode, break it all, Prop Swatch, etc.
- ---
- Savegames
  - hier werden die Savegames verwaltet, geladen, gespeichert
- Settings
  - alle relevanten Einstellungen die wir anbieten
- Debug
  - hier können wir alle möglichen Charts, Details, Informationen, Protokolle/Logs, Einstellungen, etc anzeigen, die für Developer oder Neugierige interessant sind
  - hier könnten wir eine Zusammenfassung aller bekannten Informationen bieten, die per Button kopiert werden kann, und die Spieler/Nutzer uns schicken können und wir zum debuggen nutzen können

WICHTIG: ALLES relevante muss auf das Spiel bezogen sein. Wir zeigen nur Dinge, die für Spieler interessant sind, nicht für Entwickler. (Ausser im Debug-Tab) 

Welche Daten der Items wir speziell ermitteln müssen:
- Waffen
  - Name
  - Beschreibung
  - Kategorie
  - Seltenheit
  - Schaden
  - Attribute
  - Boni/Buffs
  - Levelupgrade-Boni/Buffs
  - Sockel (mit oder Edelstein)
  - Besonderheiten
- Rüstung
  - welcher Typ (effektiv/prestige). Muss dann in das jeweilige Equipment sortiert werden, am besten per Switch/Umschalter auf der Equipment-Seite
  - Name
  - Beschreibung
  - Kategorie/Typ
  - Stufe
  - Seltenheit
  - Attribute
  - Boni/Buffs
  - Levelupgrade-Boni/Buffs
  - Besonderheiten
- dann müssen wir noch schauen, ob es sich zum Beispiel um Bufffood, Munition (un/endlich), Loot, Materialien oder sonstiges handelt. Alles kann seine eigenen Eigenschaften haben.