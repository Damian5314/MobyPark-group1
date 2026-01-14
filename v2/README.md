# MobyPark v2 - Setup & Run

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)

### Installing PostgreSQL

#### Windows
1. Download the installer from [PostgreSQL Downloads](https://www.postgresql.org/download/windows/)
2. Run the installer and follow the setup wizard
3. Remember the password you set for the postgres user
4. Default port is 5432 (keep this unless you have a conflict)
5. Add PostgreSQL to your PATH during installation

#### macOS
Using Homebrew (recommended):
```bash
brew install postgresql@14
brew services start postgresql@14
```

Or download from [Postgres.app](https://postgresapp.com/)

#### Linux (Ubuntu/Debian)
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

#### Verify Installation
```bash
psql --version
```

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
