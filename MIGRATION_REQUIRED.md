# Database Migration Required

## Error
```
Microsoft.Data.SqlClient.SqlException: Invalid column name 'DiscountType'.
```

## Cause
The database schema is out of sync. New tables and columns have been added to the code but not migrated to the database yet:
1. **Coupons table** - completely new
2. **ProductImages table** - completely new

## Solution

You need to create and run a database migration. Follow these steps:

### Option 1: Using EF Core Migrations (Recommended)

1. **Open a command prompt** in the solution directory
2. **Navigate to the EntityFrameworkCore project:**
   ```bash
   cd src\Masroof.Ecommerce.EntityFrameworkCore
   ```

3. **Create a new migration:**
   ```bash
   dotnet ef migrations add AddCouponsAndProductImages --startup-project ..\Masroof.Ecommerce.DbMigrator
   ```

4. **Run the DbMigrator to apply the migration:**
   ```bash
   cd ..\Masroof.Ecommerce.DbMigrator
   dotnet run
   ```

### Option 2: Manual SQL Script (If Option 1 doesn't work)

If you can't run migrations, execute the following SQL script directly in SQL Server Management Studio or Azure Data Studio:

```sql
-- =============================================
-- Add Coupons Table
-- =============================================
CREATE TABLE [dbo].[AppCoupons] (
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [Code] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [DiscountType] INT NOT NULL,
    [DiscountValue] DECIMAL(18,2) NOT NULL,
    [MinimumOrderAmount] DECIMAL(18,2) NULL,
    [MaximumDiscountAmount] DECIMAL(18,2) NULL,
    [MaxUsageCount] INT NULL,
    [UsageCount] INT NOT NULL,
    [ValidFrom] DATETIME2 NOT NULL,
    [ValidTo] DATETIME2 NOT NULL,
    [IsActive] BIT NOT NULL,
    [IsOneTimeUse] BIT NOT NULL,
    [ExtraProperties] NVARCHAR(MAX) NULL,
    [ConcurrencyStamp] NVARCHAR(40) NULL,
    [CreationTime] DATETIME2 NOT NULL,
    [CreatorId] UNIQUEIDENTIFIER NULL,
    [LastModificationTime] DATETIME2 NULL,
    [LastModifierId] UNIQUEIDENTIFIER NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeleterId] UNIQUEIDENTIFIER NULL,
    [DeletionTime] DATETIME2 NULL,
    CONSTRAINT [PK_AppCoupons] PRIMARY KEY ([Id])
);

-- Indexes for Coupons
CREATE UNIQUE INDEX [IX_AppCoupons_Code] ON [dbo].[AppCoupons] ([Code]);
CREATE INDEX [IX_AppCoupons_IsActive] ON [dbo].[AppCoupons] ([IsActive]);
CREATE INDEX [IX_AppCoupons_ValidFrom] ON [dbo].[AppCoupons] ([ValidFrom]);
CREATE INDEX [IX_AppCoupons_ValidTo] ON [dbo].[AppCoupons] ([ValidTo]);

-- =============================================
-- Add ProductImages Table
-- =============================================
CREATE TABLE [dbo].[AppProductImages] (
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [BlobName] NVARCHAR(256) NOT NULL,
    [FileName] NVARCHAR(256) NOT NULL,
    [ContentType] NVARCHAR(100) NOT NULL,
    [SizeInBytes] BIGINT NOT NULL,
    [Url] NVARCHAR(1000) NOT NULL,
    [DisplayOrder] INT NOT NULL,
    [IsPrimary] BIT NOT NULL,
    [ExtraProperties] NVARCHAR(MAX) NULL,
    [ConcurrencyStamp] NVARCHAR(40) NULL,
    [CreationTime] DATETIME2 NOT NULL,
    [CreatorId] UNIQUEIDENTIFIER NULL,
    [LastModificationTime] DATETIME2 NULL,
    [LastModifierId] UNIQUEIDENTIFIER NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeleterId] UNIQUEIDENTIFIER NULL,
    [DeletionTime] DATETIME2 NULL,
    CONSTRAINT [PK_AppProductImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AppProductImages_AppProducts_ProductId] FOREIGN KEY ([ProductId])
        REFERENCES [dbo].[AppProducts] ([Id]) ON DELETE CASCADE
);

-- Indexes for ProductImages
CREATE INDEX [IX_AppProductImages_ProductId] ON [dbo].[AppProductImages] ([ProductId]);
CREATE INDEX [IX_AppProductImages_IsPrimary] ON [dbo].[AppProductImages] ([IsPrimary]);
```

### Option 3: Reset Database (Development Only - DATA LOSS)

If this is a development environment and you don't mind losing data:

1. **Drop the database:**
   ```sql
   DROP DATABASE YourDatabaseName;
   ```

2. **Run DbMigrator again:**
   ```bash
   cd src\Masroof.Ecommerce.DbMigrator
   dotnet run
   ```
   This will recreate the database with all tables including the new ones.

## Verification

After running the migration, verify the tables exist:

```sql
-- Check if Coupons table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AppCoupons';

-- Check if ProductImages table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AppProductImages';

-- Verify columns in Coupons
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AppCoupons';
```

## After Migration

Once the migration is complete, the DbMigrator will:
1. Create the new tables (AppCoupons, AppProductImages)
2. Seed sample data:
   - 5 active coupons (WELCOME10, SUMMER15, SAVE25, VIP20, FLASH30)
   - 4 categories with 5 subcategories
   - 6 products

Then your application should run without errors!
