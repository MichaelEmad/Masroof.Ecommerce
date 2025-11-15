#!/bin/bash
# QUICK START DEPLOYMENT SCRIPT
# Run this on your development machine

echo "🚀 Masroof E-Commerce - Quick Deployment"
echo "========================================"

# Step 1: Database Migration
echo ""
echo "📦 Step 1/4: Creating Database Migration..."
cd src/Masroof.Ecommerce.EntityFrameworkCore
dotnet ef migrations add "Added_Payment_Module" --startup-project ../Masroof.Ecommerce.HttpApi.Host/Masroof.Ecommerce.HttpApi.Host.csproj

# Step 2: Apply Migration
echo ""
echo "📦 Step 2/4: Applying Migration to Database..."
cd ../Masroof.Ecommerce.DbMigrator
dotnet run

# Step 3: Build Backend
echo ""
echo "🔨 Step 3/4: Building Backend..."
cd ../Masroof.Ecommerce.HttpApi.Host
dotnet build -c Release

# Step 4: Run Tests
echo ""
echo "✅ Step 4/4: Running Tests..."
cd ../../test/Masroof.Ecommerce.Application.Tests
dotnet test --no-build

echo ""
echo "✅ DONE! Now:"
echo "1. Update Stripe keys in appsettings.json"
echo "2. Update SMTP settings in appsettings.json"
echo "3. Run: dotnet run (in HttpApi.Host folder)"
echo "4. Test checkout with card: 4242 4242 4242 4242"
