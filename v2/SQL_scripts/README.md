README.txt
===========

Titel: Uitvoeren van het database-migratiescript voor MobyPark (PostgreSQL 9.3)

Beschrijving:
-------------
Dit script voegt bedrijven en hotels toe aan de database, koppelt gebruikers met meerdere voertuigen aan een bedrijf, en koppelt een selectie van parkeerplaatsen aan hotels met gratis parkeren. Het script is compatibel met PostgreSQL versie 9.3.

Bestandsnaam:
-------------
migration_9_3.sql

Inhoud van het script:
----------------------
1. Creëert de tabel 'companies' (indien nog niet aanwezig)
2. Voegt een optionele kolom 'company_id' toe aan de tabel 'Users'
3. Voegt enkele bedrijven toe en koppelt gebruikers met meerdere voertuigen aan een willekeurig bedrijf
4. Creëert de tabel 'hotels' (indien nog niet aanwezig)
5. Voegt een optionele kolom 'hotel_id' toe aan de tabel 'ParkingLots'
6. Voegt enkele hotels toe en koppelt een paar parkeerplaatsen aan hotels met gratis tarief

---

Instructies voor uitvoering in Visual Studio Code:
--------------------------------------------------

1. Zorg dat de PostgreSQL-extensie geïnstalleerd is in VS Code:
   - Open Extensions (`Ctrl+Shift+X`)
   - Zoek op "PostgreSQL" en installeer de extensie van Microsoft of Chris Kolkman
   - **Verwijder of deactiveer de MSSQL-extensie** als die nog actief is

2. Maak verbinding met de database:
   - Open de PostgreSQL-view in de linkerzijbalk
   - Klik op 'Connect' bij je database of voeg een nieuwe verbinding toe:
     ```
     Host: localhost
     Port: 5432
     Database: mobypark
     Username: postgres
     Password: postgres
     ```
   - Zorg dat de actieve verbinding ingesteld is in de onderste statusbalk

3. Open het SQL-script:
   - File → Open → migration_9_3.sql

4. Stel de actieve verbinding in voor het script:
   - Klik in de **statusbalk rechtsonder** op “No active PostgreSQL connection” of de huidige verbinding
   - Selecteer `localhost:5432 → mobypark`

5. Voer het script uit:
   - Selecteer alle SQL (`Ctrl+A`) en klik op **Execute Query** (▶️)  
   - Of plaats de cursor in het script en druk op `Ctrl+Shift+E`

6. Controleer de resultaten:
   - Controleer de nieuwe tabellen en kolommen:
     ```sql
     SELECT * FROM companies;
     SELECT id, username, company_id FROM "Users" LIMIT 10;
     SELECT * FROM hotels;
     SELECT id, name, hotel_id, tariff FROM "ParkingLots" LIMIT 10;
     ```

7. Opmerkingen:
   - Het script is **idempotent**, dus je kunt het meerdere keren uitvoeren zonder duplicaten
   - Random koppelingen (bedrijven → gebruikers, hotels → parkeerplaatsen) kunnen bij elke run verschillen

---

Einde van README