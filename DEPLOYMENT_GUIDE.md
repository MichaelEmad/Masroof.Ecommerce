# Masroof E-Commerce Platform - Deployment Guide

**Version:** 1.0.0
**Last Updated:** 2025-11-15
**Target Deployment:** Production-Ready

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Database Setup](#database-setup)
4. [Configuration](#configuration)
5. [Running the Application](#running-the-application)
6. [Production Deployment](#production-deployment)
7. [Post-Deployment Tasks](#post-deployment-tasks)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+** and **npm** - [Download](https://nodejs.org/)
- **SQL Server 2019+** or **SQL Server Express** - [Download](https://www.microsoft.com/sql-server/sql-server-downloads)
- **Visual Studio 2022** or **VS Code** (optional but recommended)

### Verify Installations

```bash
dotnet --version  # Should show 9.0.x
node --version    # Should show 18.x or higher
npm --version     # Should show 9.x or higher
```

---

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/MichaelEmad/Masroof.Ecommerce.git
cd Masroof.Ecommerce
git checkout claude/task-division-workflow-01STJUNTtsv2Yq9MK7nro5ei
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Install Angular Dependencies

```bash
cd angular
npm install
cd ..
```

---

## Database Setup

### Option 1: Automatic Setup (Recommended)

1. **Update Connection String** in `src/Masroof.Ecommerce.DbMigrator/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=Masroof_Ecommerce;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

2. **Run Database Migrator**:

```bash
cd src/Masroof.Ecommerce.DbMigrator
dotnet run
```

This will:
- Create the database
- Run all migrations
- Seed initial data (admin user, sample products, categories, coupons)

### Option 2: Manual Migration

```bash
cd src/Masroof.Ecommerce.EntityFrameworkCore
dotnet ef database update
```

### Default Admin Credentials

After running the DbMigrator, you can log in with:

- **Email:** admin@abp.io
- **Password:** 1q2w3E*

**⚠️ IMPORTANT:** Change the admin password immediately in production!

---

## Configuration

### 1. Backend Configuration

**File:** `src/Masroof.Ecommerce.HttpApi.Host/appsettings.json`

#### Database Connection

```json
{
  "ConnectionStrings": {
    "Default": "Server=YOUR_SERVER;Database=Masroof_Ecommerce;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}
```

#### Email Settings (SMTP)

```json
{
  "Settings": {
    "Abp.Mailing.Smtp.Host": "smtp.gmail.com",
    "Abp.Mailing.Smtp.Port": "587",
    "Abp.Mailing.Smtp.UserName": "your-email@gmail.com",
    "Abp.Mailing.Smtp.Password": "your-app-password",
    "Abp.Mailing.Smtp.EnableSsl": "true",
    "Abp.Mailing.DefaultFromAddress": "noreply@masroof.com",
    "Abp.Mailing.DefaultFromDisplayName": "Masroof E-Commerce"
  }
}
```

**Gmail Setup:**
1. Go to Google Account → Security
2. Enable 2-Step Verification
3. Generate an "App Password" for mail
4. Use the app password in the config

#### Blob Storage (File Upload)

Default: **File System** (stores images in `wwwroot/file-containers/product-images/`)

For **Azure Blob Storage** (recommended for production):

1. Install package:
```bash
cd src/Masroof.Ecommerce.HttpApi.Host
dotnet add package Volo.Abp.BlobStoring.Azure
```

2. Update `EcommerceHttpApiHostModule.cs`:
```csharp
Configure<AbpBlobStoringOptions>(options =>
{
    options.Containers.Configure<ProductImageContainer>(container =>
    {
        container.UseAzure(azure =>
        {
            azure.ConnectionString = "YOUR_AZURE_STORAGE_CONNECTION_STRING";
            azure.ContainerName = "product-images";
            azure.CreateContainerIfNotExists = true;
        });
    });
});
```

3. Add to appsettings.json:
```json
{
  "Azure": {
    "BlobStorage": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
    }
  }
}
```

#### CORS Settings

```json
{
  "App": {
    "CorsOrigins": "http://localhost:4200,https://your-production-domain.com",
    "RedirectAllowedUrls": "http://localhost:4200,https://your-production-domain.com"
  }
}
```

### 2. Frontend Configuration

**File:** `angular/src/environments/environment.prod.ts`

```typescript
export const environment = {
  production: true,
  application: {
    baseUrl: 'https://your-production-domain.com',
    name: 'Masroof',
  },
  oAuthConfig: {
    issuer: 'https://your-api-domain.com',
    redirectUri: 'https://your-production-domain.com',
    clientId: 'Ecommerce_App',
    responseType: 'code',
    scope: 'offline_access Ecommerce',
    requireHttps: true
  },
  apis: {
    default: {
      url: 'https://your-api-domain.com',
      rootNamespace: 'Masroof.Ecommerce',
    },
  },
};
```

---

## Running the Application

### Development Mode

#### 1. Run Backend

```bash
cd src/Masroof.Ecommerce.HttpApi.Host
dotnet run
```

Backend will run on: `https://localhost:44397`

#### 2. Run Frontend (New Terminal)

```bash
cd angular
npm start
```

Frontend will run on: `http://localhost:4200`

### Access the Application

- **Frontend:** http://localhost:4200
- **Backend API:** https://localhost:44397
- **Swagger (API Docs):** https://localhost:44397/swagger

---

## Production Deployment

### Backend Deployment (ASP.NET Core)

#### 1. Publish the Backend

```bash
cd src/Masroof.Ecommerce.HttpApi.Host
dotnet publish -c Release -o ./publish
```

#### 2. Deploy to Server

**Option A: IIS (Windows Server)**

1. Install .NET 9.0 Hosting Bundle
2. Create IIS Application Pool (.NET CLR Version: No Managed Code)
3. Create IIS Website pointing to publish folder
4. Configure bindings (HTTP/HTTPS)
5. Ensure `web.config` exists in publish folder

**Option B: Linux (Nginx + Systemd)**

1. Copy publish folder to server: `/var/www/masroof-api`
2. Create systemd service: `/etc/systemd/system/masroof-api.service`

```ini
[Unit]
Description=Masroof E-Commerce API

[Service]
WorkingDirectory=/var/www/masroof-api
ExecStart=/usr/bin/dotnet /var/www/masroof-api/Masroof.Ecommerce.HttpApi.Host.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=masroof-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

3. Start the service:
```bash
sudo systemctl enable masroof-api
sudo systemctl start masroof-api
sudo systemctl status masroof-api
```

4. Configure Nginx as reverse proxy:

```nginx
server {
    listen 80;
    server_name api.masroof.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

**Option C: Azure App Service**

1. Create App Service (Windows or Linux)
2. Configure deployment:
   - Method: GitHub Actions, Azure DevOps, or FTP
   - Runtime: .NET 9
3. Set environment variables in Configuration
4. Deploy from publish folder

**Option D: Docker**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Masroof.Ecommerce.HttpApi.Host.dll"]
```

Build and run:
```bash
docker build -t masroof-api .
docker run -d -p 5000:80 --name masroof-api masroof-api
```

### Frontend Deployment (Angular)

#### 1. Build for Production

```bash
cd angular
npm run build:prod
```

Output folder: `angular/dist/ecommerce`

#### 2. Deploy Frontend

**Option A: Static Hosting (Netlify, Vercel, Azure Static Web Apps)**

1. Connect GitHub repository
2. Set build command: `npm run build:prod`
3. Set publish directory: `dist/ecommerce`
4. Deploy

**Option B: Nginx (Self-hosted)**

1. Copy `dist/ecommerce` to `/var/www/masroof`
2. Configure Nginx:

```nginx
server {
    listen 80;
    server_name masroof.com www.masroof.com;
    root /var/www/masroof;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    # Caching for static assets
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

**Option C: Azure Static Web Apps**

```bash
# Install Azure Static Web Apps CLI
npm install -g @azure/static-web-apps-cli

# Deploy
swa deploy ./dist/ecommerce --deployment-token YOUR_DEPLOYMENT_TOKEN
```

**Option D: AWS S3 + CloudFront**

1. Create S3 bucket
2. Upload `dist/ecommerce` contents
3. Enable static website hosting
4. Create CloudFront distribution
5. Configure custom domain

### Database Migration for Production

1. Backup existing database (if applicable)
2. Update connection string in DbMigrator
3. Run migrations:

```bash
cd src/Masroof.Ecommerce.DbMigrator
dotnet run --environment Production
```

---

## Post-Deployment Tasks

### 1. Security Hardening

- [ ] Change default admin password
- [ ] Configure HTTPS/SSL certificates (Let's Encrypt or commercial)
- [ ] Set up firewall rules
- [ ] Enable rate limiting
- [ ] Configure CORS properly
- [ ] Disable Swagger in production (optional)

### 2. Create Initial Data

- [ ] Add product categories
- [ ] Upload products with images
- [ ] Configure shipping zones/rates
- [ ] Set up payment gateway (Stripe/PayPal)
- [ ] Create promotional coupons

### 3. Testing

- [ ] Test user registration and login
- [ ] Test product browsing and search
- [ ] Test add to cart and checkout flow
- [ ] Test order placement
- [ ] Verify email notifications work
- [ ] Test admin product/category/order management
- [ ] Test invoice PDF generation
- [ ] Test product image uploads

### 4. Monitoring & Logging

**Application Insights (Recommended for Azure)**

1. Add package:
```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

2. Configure in `Program.cs`:
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

3. Add to appsettings.json:
```json
{
  "ApplicationInsights": {
    "ConnectionString": "YOUR_APP_INSIGHTS_CONNECTION_STRING"
  }
}
```

**Sentry (Error Tracking)**

1. Add package:
```bash
dotnet add package Sentry.AspNetCore
```

2. Configure in `Program.cs`:
```csharp
builder.WebHost.UseSentry(options =>
{
    options.Dsn = "YOUR_SENTRY_DSN";
    options.Environment = "production";
});
```

### 5. Performance Optimization

- [ ] Enable response compression
- [ ] Configure caching (Redis recommended)
- [ ] Set up CDN for static assets
- [ ] Optimize database indexes
- [ ] Enable image optimization

---

## Troubleshooting

### Common Issues

#### 1. Database Connection Errors

**Problem:** Can't connect to SQL Server

**Solution:**
- Verify SQL Server is running
- Check connection string format
- Ensure firewall allows connection
- For SQL Server Express, ensure TCP/IP is enabled in SQL Server Configuration Manager

#### 2. Email Not Sending

**Problem:** Order confirmation emails not received

**Solution:**
- Check SMTP settings in appsettings.json
- Verify SMTP credentials
- Check spam folder
- For Gmail, use App Password instead of account password
- Enable "Less secure app access" (not recommended) or use OAuth2

#### 3. CORS Errors

**Problem:** API calls blocked by CORS policy

**Solution:**
- Add frontend URL to `CorsOrigins` in appsettings.json
- Ensure backend is running
- Clear browser cache
- Check browser console for specific error

#### 4. Image Upload Fails

**Problem:** Cannot upload product images

**Solution:**
- Check file size (max 5MB)
- Verify file type (JPEG, PNG, WEBP, GIF only)
- Ensure blob storage is configured
- Check folder permissions (for FileSystem storage)
- Verify `wwwroot/file-containers` folder exists

#### 5. PDF Invoice Not Generating

**Problem:** Download invoice returns error

**Solution:**
- Ensure QuestPDF package is installed
- Check order exists and user has permission
- Verify fonts are available on server
- Check server logs for detailed error

#### 6. Angular Build Fails

**Problem:** `npm run build:prod` fails

**Solution:**
```bash
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm install
npm run build:prod
```

### Log Locations

- **Backend Logs:** `src/Masroof.Ecommerce.HttpApi.Host/Logs/`
- **Browser Console:** F12 → Console tab
- **Network Errors:** F12 → Network tab

### Support

For issues not covered here:
1. Check project documentation
2. Review error logs
3. Search GitHub Issues
4. Contact development team

---

## Environment Variables Checklist

### Backend (.NET)

- [ ] `ConnectionStrings__Default` - Database connection
- [ ] `Settings__Abp.Mailing.Smtp.Host` - SMTP host
- [ ] `Settings__Abp.Mailing.Smtp.UserName` - SMTP username
- [ ] `Settings__Abp.Mailing.Smtp.Password` - SMTP password
- [ ] `App__CorsOrigins` - Allowed CORS origins
- [ ] `Azure__BlobStorage__ConnectionString` - Azure Blob Storage (if using)

### Frontend (Angular)

- [ ] `API_URL` - Backend API URL
- [ ] `OAUTH_ISSUER` - OAuth issuer URL
- [ ] `CLIENT_ID` - OAuth client ID

---

## Deployment Checklist

### Pre-Deployment

- [ ] Code is committed and pushed to repository
- [ ] All tests pass locally
- [ ] Database migrations are prepared
- [ ] Configuration files updated for production
- [ ] Secrets are stored securely (not in source control)

### Deployment

- [ ] Backend deployed and running
- [ ] Frontend deployed and accessible
- [ ] Database migrated successfully
- [ ] SSL certificate configured
- [ ] DNS records updated

### Post-Deployment

- [ ] Admin can log in
- [ ] Users can register and log in
- [ ] Products are visible
- [ ] Orders can be placed
- [ ] Emails are being sent
- [ ] Invoices can be downloaded
- [ ] Images can be uploaded
- [ ] All critical workflows tested

---

## Performance Benchmarks

Expected performance on moderate hardware (4 CPU, 8GB RAM):

- **Page Load Time:** < 2 seconds
- **API Response Time:** < 200ms (average)
- **Concurrent Users:** 100+ (with caching)
- **Product Catalog:** 10,000+ products
- **Orders per Hour:** 500+

---

## Backup Strategy

### Database Backup

**Automated Daily Backups:**
```sql
-- SQL Server Agent Job (Daily at 2 AM)
BACKUP DATABASE [Masroof_Ecommerce]
TO DISK = 'C:\Backups\Masroof_Ecommerce_$(DATE).bak'
WITH COMPRESSION, INIT;
```

**Manual Backup:**
```bash
sqlcmd -S localhost -Q "BACKUP DATABASE [Masroof_Ecommerce] TO DISK = 'C:\Backups\backup.bak'"
```

### File Storage Backup

- If using **File System**: Backup `wwwroot/file-containers` folder
- If using **Azure Blob Storage**: Enable soft delete and versioning
- If using **AWS S3**: Enable versioning and cross-region replication

---

## Scaling Recommendations

### Horizontal Scaling

1. **Load Balancer** (Azure Load Balancer, AWS ELB, Nginx)
2. **Multiple Backend Instances** (stateless design allows easy scaling)
3. **Redis for Distributed Caching**
4. **Database Replication** (read replicas)

### Vertical Scaling

- Increase CPU/RAM as needed
- Optimize database queries
- Add database indexes
- Enable caching

---

## License

This deployment guide is part of the Masroof E-Commerce Platform.

---

**🎉 Congratulations!** Your e-commerce platform is now ready for production deployment.

For questions or support, please contact the development team.
