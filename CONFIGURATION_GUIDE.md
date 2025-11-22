# Configuration Guide

This guide explains how to configure the School Management System using **environment variables** (recommended) or `appsettings.json` files.

## ⚠️ Important: Use Environment Variables for Production

**For production deployments, always use environment variables** instead of `appsettings.json`. This ensures:
- Secrets are not committed to version control
- Different configurations for different environments
- Better security practices
- Easy deployment to cloud platforms

The `appsettings.json` file should only contain **development defaults** and should never include production secrets.

## Configuration Methods

The system supports configuration in this priority order (highest to lowest):
1. **Environment Variables** (recommended for production)
2. `appsettings.{Environment}.json` (e.g., `appsettings.Production.json`)
3. `appsettings.json` (development defaults only)

Environment variables automatically override values in `appsettings.json`.

## Environment Variable Naming Convention

.NET uses double underscores (`__`) to represent nested configuration:

| appsettings.json | Environment Variable |
|------------------|---------------------|
| `School:Name` | `School__Name` |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |
| `Jwt:Key` | `Jwt__Key` |
| `Smtp:Host` | `Smtp__Host` |

For arrays, use indexed notation:
- `Cors:AllowedOrigins[0]` → `Cors__AllowedOrigins__0`
- `Cors:AllowedOrigins[1]` → `Cors__AllowedOrigins__1`

## Required Environment Variables

See `environment.example` for a complete list. Here are the essential variables:

### Database Connection

**For Local Docker SQL Server:**
```bash
# Start SQL Server container
docker-compose up -d

# Connection string (already configured in appsettings.json)
ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=SchoolDB;User Id=sa;Password=scherp@2025;TrustServerCertificate=True"
```

**For Other SQL Server Instances:**
```bash
ConnectionStrings__DefaultConnection="Server=your-server;Database=SchoolDB;User Id=user;Password=pass;TrustServerCertificate=True"
```

### JWT Configuration
```bash
# Generate a secure key: openssl rand -base64 32
Jwt__Key="your-secure-random-key-at-least-32-characters-long"
Jwt__Issuer="YourSchoolManagementSystem"
Jwt__Audience="YourSchoolManagementSystem"
```

### School Branding
```bash
School__Name="Your School Name"
School__Email="info@yourschool.com"
School__Website="www.yourschool.com"
School__LogoPath="./Media/logo.png"
School__WatermarkLogoPath="./Media/logo-wm.png"
```

### CORS Configuration
```bash
Cors__AllowedOrigins__0="https://app.yourschool.com"
Cors__AllowedOrigins__1="https://www.yourschool.com"
Cors__AllowedOrigins__2="http://localhost:5173"
```

### SMTP Configuration
```bash
Smtp__Host="smtp.yourschool.com"
Smtp__Port="587"
Smtp__Username="noreply@yourschool.com"
Smtp__Password="your-email-password"
Smtp__From="noreply@yourschool.com"
ReportCardEmail="reports@yourschool.com"
```

## Setting Environment Variables

### Linux/macOS
```bash
export School__Name="Your School Name"
export School__Email="info@yourschool.com"
export ConnectionStrings__DefaultConnection="your-connection-string"
```

### Windows (PowerShell)
```powershell
$env:School__Name="Your School Name"
$env:School__Email="info@yourschool.com"
$env:ConnectionStrings__DefaultConnection="your-connection-string"
```

### Windows (Command Prompt)
```cmd
set School__Name=Your School Name
set School__Email=info@yourschool.com
set ConnectionStrings__DefaultConnection=your-connection-string
```

### Using .env Files (Local Development)

For local development convenience, you can use a `.env` file with a package like `DotNetEnv`:

1. Install the package (optional):
```bash
dotnet add package DotNetEnv
```

2. Load .env file in `Program.cs` (add at the very beginning):
```csharp
using DotNetEnv;

// Load .env file if it exists (development only)
if (builder.Environment.IsDevelopment())
{
    Env.Load();
}
```

3. Create a `.env` file (copy from `environment.example`):
```bash
School__Name=Your School Name
School__Email=info@yourschool.com
ConnectionStrings__DefaultConnection=Server=localhost;Database=SchoolDB;...
```

**Important**: Add `.env` to `.gitignore` to prevent committing secrets!

## Platform-Specific Configuration

### Azure App Service
Set environment variables in:
- Portal: Configuration → Application settings
- Azure CLI:
```bash
az webapp config appsettings set --name <app-name> --resource-group <rg-name> --settings School__Name="Your School Name"
```

### AWS Elastic Beanstalk
Use `.ebextensions` or set in the AWS Console:
```yaml
option_settings:
  aws:elasticbeanstalk:application:environment:
    School__Name: "Your School Name"
    School__Email: "info@yourschool.com"
```

### Docker
Use `-e` flags or `docker-compose.yml`:
```yaml
environment:
  - School__Name=Your School Name
  - School__Email=info@yourschool.com
  - ConnectionStrings__DefaultConnection=Server=db;Database=SchoolDB;...
```

### Kubernetes
Use ConfigMaps or Secrets:
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: school-config
data:
  School__Name: "Your School Name"
  School__Email: "info@yourschool.com"
```

## School Branding Configuration

### Settings Explained

- **School__Name**: The school name displayed on report cards and documents
- **School__Email**: Contact email address displayed on report cards
- **School__Website**: School website URL displayed on report cards
- **School__LogoPath**: Path to the main school logo (relative to application root)
- **School__WatermarkLogoPath**: Path to the watermark logo used as background

### Logo Setup

1. Place your school logo files in the `Media` folder:
   - Main logo: `Media/logo.png` (recommended: 200x200px)
   - Watermark logo: `Media/logo-wm.png` (recommended: semi-transparent, 500x500px)

2. Update the paths via environment variables if using different filenames:
```bash
School__LogoPath="./Media/my-logo.png"
School__WatermarkLogoPath="./Media/my-watermark.png"
```

## Development Configuration

### Local Database Setup (Docker)

If you're using the provided Docker Compose setup for local SQL Server:

```bash
# Start SQL Server container
cd /path/to/project
docker-compose up -d

# Verify it's running
docker ps
```

The connection string is already configured in `appsettings.json` and `appsettings.Development.json`:
```
Server=localhost,1433;Database=SchoolDB;User Id=sa;Password=scherp@2025;TrustServerCertificate=True
```

### Development Settings

For local development, you can use `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SchoolDB;User Id=sa;Password=scherp@2025;TrustServerCertificate=True"
  },
  "School": {
    "Name": "Development School",
    "Email": "dev@school.com"
  }
}
```

**Note**: Environment variables still override these values.

## First-Time Setup Checklist

- [ ] Copy `environment.example` to create your environment variables list
- [ ] Set all required environment variables (see `environment.example`)
- [ ] Generate a secure JWT key: `openssl rand -base64 32`
- [ ] Add your school logos to the `Media` folder
- [ ] Configure database connection string
- [ ] Set up SMTP settings for email functionality
- [ ] Add your frontend URLs to CORS allowed origins
- [ ] Test configuration by starting the application
- [ ] Generate a test report card to verify branding

## Verifying Configuration

To verify your environment variables are being read correctly:

1. Check application startup logs
2. Generate a test report card - school name should appear correctly
3. Check Swagger UI - contact email should match your configuration
4. Test API endpoints from your frontend (CORS should work)

## Security Best Practices

1. ✅ **Never commit secrets** to version control
2. ✅ **Use environment variables** for all production configurations
3. ✅ **Rotate JWT keys** regularly
4. ✅ **Use strong passwords** for database and SMTP
5. ✅ **Limit CORS origins** to only necessary domains
6. ✅ **Use HTTPS** in production
7. ✅ **Review logs** regularly for exposed secrets
8. ✅ **Use secret management** services (Azure Key Vault, AWS Secrets Manager) for production

## Troubleshooting

### Configuration Not Loading

- Verify environment variable names use double underscores (`__`)
- Check that variables are set before the application starts
- Restart the application after setting environment variables
- Check application logs for configuration errors

### Environment Variables Not Overriding appsettings.json

- Ensure variable names match exactly (case-sensitive on Linux)
- Verify double underscore syntax (`School__Name` not `School_Name`)
- Check that variables are exported in the same shell session running the app

### CORS Errors

- Verify your frontend URL is in `Cors__AllowedOrigins__*` variables
- Check URLs match exactly (including http/https and ports)
- Restart application after changing CORS settings

### Logos Not Displaying

- Verify logo files exist at specified paths
- Check file permissions
- Ensure paths are relative to application root (start with `./`)
- Verify image formats are supported (PNG, JPG)

### Email Not Sending

- Verify SMTP credentials via environment variables
- Check firewall allows outbound SMTP connections
- Test SMTP connection separately
- Review application logs for detailed errors

## Example: Complete Production Setup

```bash
# Set all environment variables
export ConnectionStrings__DefaultConnection="Server=prod-db;Database=SchoolDB;User Id=app_user;Password=secure_password;TrustServerCertificate=True"
export Jwt__Key="$(openssl rand -base64 32)"
export Jwt__Issuer="ProductionSchoolSystem"
export Jwt__Audience="ProductionSchoolSystem"
export School__Name="Springfield Elementary School"
export School__Email="info@springfield.edu"
export School__Website="www.springfield.edu"
export School__LogoPath="./Media/logo.png"
export School__WatermarkLogoPath="./Media/logo-wm.png"
export Cors__AllowedOrigins__0="https://app.springfield.edu"
export Cors__AllowedOrigins__1="https://www.springfield.edu"
export Smtp__Host="smtp.springfield.edu"
export Smtp__Port="587"
export Smtp__Username="noreply@springfield.edu"
export Smtp__Password="smtp_password"
export Smtp__From="noreply@springfield.edu"
export ReportCardEmail="reports@springfield.edu"
export ASPNETCORE_ENVIRONMENT="Production"

# Run the application
dotnet run
```
