./build-image.ps1 -ProjectPath "../../src/Masroof.Ecommerce.DbMigrator/Masroof.Ecommerce.DbMigrator.csproj" -ImageName ecommerce/dbmigrator
./build-image.ps1 -ProjectPath "../../src/Masroof.Ecommerce.HttpApi.Host/Masroof.Ecommerce.HttpApi.Host.csproj" -ImageName ecommerce/httpapihost
./build-image.ps1 -ProjectPath "../../angular" -ImageName ecommerce/angular -ProjectType "angular"
