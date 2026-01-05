# MobyPark v2 - Setup & Run

## Vereisten

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)

## Database Setup

1. Start PostgreSQL

2. Maak database aan:
bash
psql -U postgres
CREATE DATABASE mobypark;
\q


3. Run migraties:
bash
cd v2
dotnet ef database update


Als dotnet ef niet werkt:
bash
dotnet tool install --global dotnet-ef


## Applicatie Runnen

1. Installeer dependencies:
bash
dotnet restore


2. Start de applicatie:
bash
dotnet run


Of met testdata:
bash
dotnet run seed


3. Open je browser:
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

## Tests Runnen

bash
cd ..
dotnet test


## Authenticatie in Swagger

1. Ga naar http://localhost:5000/swagger
2. Login via /api/auth/login met:
   json
   {
     "email": "admin@mobypark.nl",
     "password": "Admin123!"
   }
   
3. Kopieer de token
4. Klik op "Authorize" knop
5. Voer in: Bearer [jouw-token]