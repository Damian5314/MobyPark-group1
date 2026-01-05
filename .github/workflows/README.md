# CI/CD Pipeline Documentation

Deze repository heeft twee automatische workflows voor Continuous Integration en Continuous Deployment.

## CI Pipeline (ci.yml)

De CI pipeline wordt automatisch uitgevoerd bij:
- Elke push naar `main` of `Development` branch
- Elke pull request naar `main` of `Development` branch

### Wat doet de CI pipeline?

**Build and Test Job:**
1. Checkt de code uit
2. Installeert .NET 8.0
3. Herstelt dependencies (`dotnet restore`)
4. Bouwt de solution (`dotnet build`)
5. Voert alle tests uit (`dotnet test`)
6. Genereert code coverage rapporten
7. Upload test resultaten als artifacts

**Code Quality Job:**
1. Checkt code formatting (`dotnet format --verify-no-changes`)
2. Controleert op build warnings (behandelt warnings als errors)

### CI Pipeline Status

Je kunt de status van je builds zien door:
1. Naar de "Actions" tab op GitHub te gaan
2. De badge aan je README toe te voegen:

```markdown
![CI Pipeline](https://github.com/[Your_GitHub_Name]/MobyPark_[group]/workflows/CI%20Pipeline/badge.svg)
```

## CD Pipeline (cd.yml)

De CD pipeline maakt automatisch releases aan wanneer je een version tag pusht.

### Een release maken

1. **Commit je laatste wijzigingen:**
   ```bash
   git add .
   git commit -m "Prepare for release v1.0.0"
   ```

2. **Maak een version tag:**
   ```bash
   git tag v1.0.0
   ```

3. **Push de tag naar GitHub:**
   ```bash
   git push origin v1.0.0
   ```

### Wat gebeurt er tijdens een release?

1. De pipeline wordt getriggerd door de version tag
2. Code wordt gebouwd en tests worden uitgevoerd
3. Applicatie wordt gepubliceerd voor Linux (x64) en Windows (x64)
4. Release archives worden gemaakt (`.tar.gz` voor Linux, `.zip` voor Windows)
5. Automatische changelog wordt gegenereerd op basis van commits
6. GitHub Release wordt aangemaakt met:
   - Version tag als titel
   - Changelog met alle commits sinds vorige release
   - Downloadbare archives voor beide platforms

### Version Tag Formaat

Tags moeten het formaat `v*.*.*` volgen:
- `v1.0.0` - Major release
- `v1.1.0` - Minor release (nieuwe features)
- `v1.1.1` - Patch release (bugfixes)

Voorbeelden:
```bash
git tag v1.0.0    # Eerste release
git tag v1.1.0    # Feature update
git tag v1.1.1    # Bugfix
git tag v2.0.0    # Breaking changes
```

## Code Formatting

Voordat je code commit, zorg ervoor dat deze correct geformatteerd is:

```bash
# Check formatting
dotnet format --verify-no-changes

# Auto-format code
dotnet format
```

## Lokaal testen van de pipeline

Je kunt de stappen van de pipeline lokaal uitvoeren:

```bash
# Restore, build, en test
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release

# Format check
dotnet format --verify-no-changes

# Build met warnings als errors
dotnet build -warnaserror
```

## Troubleshooting

### CI Pipeline faalt

**Build errors:**
- Check de build logs in de Actions tab
- Voer `dotnet build` lokaal uit om errors te zien
- Zorg dat alle dependencies correct zijn geïnstalleerd

**Test failures:**
- Check de test logs in de Actions tab
- Voer `dotnet test` lokaal uit
- Download test artifacts voor gedetailleerde resultaten

**Format errors:**
- Voer `dotnet format` uit om code automatisch te formatteren
- Commit de wijzigingen en push opnieuw

### CD Pipeline faalt

**Tag niet herkend:**
- Zorg dat tag formaat `v*.*.*` volgt (bijv. `v1.0.0`)
- Check of tag correct gepusht is met `git tag -l`

**Tests falen:**
- CD pipeline voert ook tests uit
- Fix de tests voordat je een release maakt

**Publish errors:**
- Check of de project paths in `cd.yml` kloppen
- Verifieer dat `v2/v2.csproj` bestaat

## Best Practices

1. **Werk altijd op een feature branch** en maak pull requests naar Development
2. **Laat de CI pipeline slagen** voordat je een PR merged
3. **Fix failing tests onmiddellijk** om de main/Development branch groen te houden
4. **Maak releases vanaf main branch** na grondige testing op Development
5. **Gebruik semantic versioning** voor je tags (major.minor.patch)
6. **Schrijf duidelijke commit messages** - deze verschijnen in de changelog

## GitHub Secrets (optioneel)

Voor geavanceerde deployment (Azure, Docker, etc.) kun je secrets toevoegen:
1. Ga naar Settings → Secrets and variables → Actions
2. Voeg secrets toe zoals `AZURE_CREDENTIALS`, `DOCKER_USERNAME`, etc.
3. Update de workflow files om deze secrets te gebruiken
