# Wyn Integration Notes

## 1) Build and publish

```bash
dotnet build -c Release
```

Use output from:

- `src/CustomSecProvider.RA/bin/Release/net8.0/`

## 2) Copy provider assembly and dependencies

Copy to Wyn SecurityProviders folder:

- Windows (Server identity provider):
  - `C:\Program Files\Wyn Enterprise\Server\SecurityProviders`
- Windows (Portal identity provider):
  - `C:\Program Files\Wyn Enterprise\Portal\SecurityProviders`
- Linux:
  - `/opt/Wyn/Server/SecurityProviders`

Also copy dependency DLLs required by your provider (for example SQL client DLLs) if they are not already present.

## 3) Restart service

Stop/start Wyn service before and after deployment.

Windows example:

```cmd
net stop WynService
net start WynService
```

## 4) Register provider in Admin Portal

- Navigate to `Configuration > Security Providers`
- Click `Add Provider`
- Select your deployed provider
- Enter settings
- Save and restart if prompted

## 5) Validate flows

- Free user: viewer-only access
- Pro user: export/scheduling claims
- Enterprise designer: authoring role
- Disabled user: denied
- Suspended tenant: denied
- Seat overage: denied
- Incident mode: read-only
