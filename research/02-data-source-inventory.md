# 02 – Datenquelleninventur

## Vorhandene Quellen

1. `data\savegames`: Weltversionen, Info-Dateien und JSON-Indizes; Herkunft sind bereitgestellte Backups.
2. `D:\SteamLibrary\steamapps\common\Enshrouded`: installierte Binär-/Resource-Pakete und lokale Logs.
3. `docs\images`: reine UI-Referenzen, keine Spieldaten.
4. Konzeptdokumente: Anforderungen und gewünschte Kategorien, keine validierten IDs.

Es gibt keine Itemliste, Objektliste, CSV, API-Spezifikation oder exportierte Spieldatenbank im Projekt.

## Beobachtungen

Die Savegame-Indizes sind JSON und enthalten mindestens `latest`, Unix-Zeit `time` und `deleted`. Weltdateien und `_info` beginnen mit `KSC1` und enthalten keine verlässlich lesbaren Namen. Versionssuffixe zeigen, dass mehrere Snapshots pro Welt vorliegen. Die Dateien sind daher wertvoll für byteweise Vergleichstests, aber noch kein dekodiertes Datenmodell.

Ein read-only Strukturtest an mehreren Dateien zeigt wiederholt: `KSC1`, danach der little-endian-Wert `8`, ein 16-Byte-Feld ohne bisher bestätigte Semantik und ein weiterer little-endian-Wert. Ab Offset 28 sind in den untersuchten Dateien lesbare Vier-Byte-Marker wie `EXTS`, `KNOW` und `USER` sichtbar, gefolgt von weiteren little-endian-Werten. Diese Beobachtung ist reproduzierbar und wurde als Strukturinspektor implementiert. Die Folgewerte sind nur als Kandidatenlängen/-werte markiert; eine semantische Zuordnung oder Dekodierung ist nicht bewiesen.

Die Installationsdateien enthalten große `.dat`-Pakete und `.kfc`-Dateien. Eine Extraktion oder Übernahme von Assets/Definitionen wurde nicht durchgeführt. Binär gefundene oder aus dem Spiel extrahierte Kennungen dürfen nicht automatisch als Platzierungs- oder Ersetzungsfähigkeit gelten.

## Empfohlenes Katalogschema

Der eigene Katalog sollte pro Eintrag mindestens `id`, `displayName`, `category`, `source`, `sourceVersion`, `variant`, `placementCapability`, `runtimeCapability`, `persistenceStatus`, `evidence` und `licenseNote` führen. Fähigkeitswerte sollten dreistufig sein: `unknown`, `observed`, `verified`.

Dubletten werden über normalisierte ID plus Quellversion erkannt; Namensänderungen bleiben als Alias erhalten. Ein Katalogeintrag darf ohne Capability-Nachweis nicht als „platzierbar“ erscheinen.

**Status:** Quelleninventur und beobachtete KSC1-Struktur verifiziert; tatsächliche Item-/Objektsemantik, Lizenz der Spieldaten und aktuelle ID-Abdeckung unbekannt.
