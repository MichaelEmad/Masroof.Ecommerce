-- =============================================
-- ABP PAYMENT MODULE - DIRECT SQL MIGRATION
-- Run this if dotnet CLI migration fails
-- =============================================

USE [YourDatabaseName];
GO

-- Payment Requests Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayPaymentRequests')
BEGIN
    CREATE TABLE [dbo].[PayPaymentRequests] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [TenantId] UNIQUEIDENTIFIER NULL,
        [Gateway] NVARCHAR(64) NOT NULL,
        [Currency] NVARCHAR(3) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [State] INT NOT NULL DEFAULT 0,
        [FailReason] NVARCHAR(512) NULL,
        [ExternalSubscriptionId] NVARCHAR(256) NULL,
        [CustomerId] UNIQUEIDENTIFIER NULL,
        [Email] NVARCHAR(256) NULL,
        [ExtraProperties] NVARCHAR(MAX) NULL,
        [ConcurrencyStamp] NVARCHAR(40) NULL,
        [CreationTime] DATETIME2 NOT NULL,
        [CreatorId] UNIQUEIDENTIFIER NULL,
        [LastModificationTime] DATETIME2 NULL,
        [LastModifierId] UNIQUEIDENTIFIER NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeleterId] UNIQUEIDENTIFIER NULL,
        [DeletionTime] DATETIME2 NULL,
        CONSTRAINT [PK_PayPaymentRequests] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_PayPaymentRequests_TenantId] ON [dbo].[PayPaymentRequests] ([TenantId]);
    CREATE INDEX [IX_PayPaymentRequests_Gateway] ON [dbo].[PayPaymentRequests] ([Gateway]);
    CREATE INDEX [IX_PayPaymentRequests_State] ON [dbo].[PayPaymentRequests] ([State]);
    CREATE INDEX [IX_PayPaymentRequests_CustomerId] ON [dbo].[PayPaymentRequests] ([CustomerId]);

    PRINT 'Created PayPaymentRequests table';
END

-- Payment Request Products Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayPaymentRequestProducts')
BEGIN
    CREATE TABLE [dbo].[PayPaymentRequestProducts] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [PaymentRequestId] UNIQUEIDENTIFIER NOT NULL,
        [Code] NVARCHAR(256) NOT NULL,
        [Name] NVARCHAR(256) NOT NULL,
        [UnitPrice] DECIMAL(18,2) NOT NULL,
        [Count] INT NOT NULL,
        [TotalPrice] DECIMAL(18,2) NOT NULL,
        [ExtraProperties] NVARCHAR(MAX) NULL,
        [PlanId] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_PayPaymentRequestProducts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayPaymentRequestProducts_PayPaymentRequests_PaymentRequestId]
            FOREIGN KEY ([PaymentRequestId]) REFERENCES [dbo].[PayPaymentRequests] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_PayPaymentRequestProducts_PaymentRequestId] ON [dbo].[PayPaymentRequestProducts] ([PaymentRequestId]);
    CREATE INDEX [IX_PayPaymentRequestProducts_PlanId] ON [dbo].[PayPaymentRequestProducts] ([PlanId]);

    PRINT 'Created PayPaymentRequestProducts table';
END

-- Plans Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayPlans')
BEGIN
    CREATE TABLE [dbo].[PayPlans] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [TenantId] UNIQUEIDENTIFIER NULL,
        [Name] NVARCHAR(128) NOT NULL,
        [ExtraProperties] NVARCHAR(MAX) NULL,
        [ConcurrencyStamp] NVARCHAR(40) NULL,
        [CreationTime] DATETIME2 NOT NULL,
        [CreatorId] UNIQUEIDENTIFIER NULL,
        [LastModificationTime] DATETIME2 NULL,
        [LastModifierId] UNIQUEIDENTIFIER NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeleterId] UNIQUEIDENTIFIER NULL,
        [DeletionTime] DATETIME2 NULL,
        CONSTRAINT [PK_PayPlans] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_PayPlans_TenantId] ON [dbo].[PayPlans] ([TenantId]);
    CREATE INDEX [IX_PayPlans_Name] ON [dbo].[PayPlans] ([Name]);

    PRINT 'Created PayPlans table';
END

-- Gateway Plans Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayGatewayPlans')
BEGIN
    CREATE TABLE [dbo].[PayGatewayPlans] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [PlanId] UNIQUEIDENTIFIER NOT NULL,
        [Gateway] NVARCHAR(64) NOT NULL,
        [ExternalId] NVARCHAR(256) NOT NULL,
        [ExtraProperties] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_PayGatewayPlans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayGatewayPlans_PayPlans_PlanId]
            FOREIGN KEY ([PlanId]) REFERENCES [dbo].[PayPlans] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_PayGatewayPlans_PlanId] ON [dbo].[PayGatewayPlans] ([PlanId]);
    CREATE INDEX [IX_PayGatewayPlans_Gateway] ON [dbo].[PayGatewayPlans] ([Gateway]);
    CREATE UNIQUE INDEX [IX_PayGatewayPlans_Gateway_ExternalId] ON [dbo].[PayGatewayPlans] ([Gateway], [ExternalId]);

    PRINT 'Created PayGatewayPlans table';
END

-- Verification
PRINT '';
PRINT '✅ Migration Complete! Verifying tables...';
PRINT '';

SELECT
    TABLE_NAME as 'Payment Tables Created',
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = INFORMATION_SCHEMA.TABLES.TABLE_NAME) as 'Columns'
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE 'Pay%'
ORDER BY TABLE_NAME;

PRINT '';
PRINT '✅ Done! You can now run the application.';
