# Database Migration Instructions

## Generate Migrations

After setting up your development environment with .NET SDK, run the following command to generate migrations:

```bash
cd src/Masroof.Ecommerce.EntityFrameworkCore
dotnet ef migrations add AddEcommerceEntities --startup-project ../Masroof.Ecommerce.DbMigrator
```

## Apply Migrations

To apply migrations to your database:

```bash
cd src/Masroof.Ecommerce.DbMigrator
dotnet run
```

Or directly update the database:

```bash
cd src/Masroof.Ecommerce.EntityFrameworkCore
dotnet ef database update --startup-project ../Masroof.Ecommerce.DbMigrator
```

## Entities Included

The migration will create tables for:
- Products
- Categories
- Customers
- Addresses
- Orders and OrderItems
- Payments
- ShoppingCarts and CartItems

All tables are prefixed with "App" as configured in EcommerceConsts.
