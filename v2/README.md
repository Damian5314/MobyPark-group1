# MobyPark v2 - Setup & Run

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)

## Database Setup

1. Start PostgreSQL

2. Create database:
```bash
psql -U postgres
CREATE DATABASE mobypark;
\q
```

3. Run migrations:
```bash
cd v2
dotnet ef database update
```

If dotnet ef doesn't work:
```bash
dotnet tool install --global dotnet-ef
```

## Running the Application

1. Install dependencies:
```bash
dotnet restore
```

2. Start the application:
```bash
dotnet run
```

Or with test data:
```bash
dotnet run seed
```

3. Open your browser:
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

## Running Tests

```bash
cd ..
dotnet test
```

## Authentication in Swagger

1. Navigate to http://localhost:5000/swagger
2. Login via /api/auth/login with:
   ```json
   {
     "email": "admin@mobypark.nl",
     "password": "Admin123!"
   }
   ```
3. Copy the token
4. Click the "Authorize" button
5. Enter: Bearer [your-token]
