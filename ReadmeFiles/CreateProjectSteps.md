
### Step 1: Clone or download this repository

From your shell or command line:

```console
    git clone https://github.com/Azure-Samples/ms-identity-dotnet-blazor-azure-sql.git
```

or download and extract the repository .zip file.

>:warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Setup SQL Database and grant user permissions for managed identity

1. Create [Azure SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/database/single-database-create-quickstart) and add your Tenant user as Admin **OR** [install local SQL server](https://www.microsoft.com/sql-server/sql-server-downloads) (Express edition is enough)
2. Install [SQL Server Management Studio](https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) or find another way to manipulate the database if you prefer.
3. [Create](https://docs.microsoft.com/sql/relational-databases/databases/create-a-database) an empty Database
4. On the created database run next commands

   ```sql
   CREATE TABLE [dbo].[Summary](
   [Summary] [nvarchar](50) NOT NULL) 
   GO;
   Insert into [dbo].Summary values ('Freezing'),('Bracing'),('Chilly'),('Cool'),('Mild'),('Warm'),('Balmy'),('Hot'),('Sweltering'),('Scorching')
   GO;
   CREATE FUNCTION [dbo].[UsernamePrintFn]()
   RETURNS nvarchar(500)
   AS
   BEGIN
       declare @host nvarchar(100), @user nvarchar(100);
       SELECT @host = HOST_NAME() , @user = SUSER_NAME()
       declare @result nvarchar(500) = cast(@user + ' at ' + @host as nvarchar(500))
       -- Return the result of the function
       return @result
   END
   GO
   ```

5. Create a user from your Tenant inside the database and grant EXECUTE permission by running next commands inside query window

   ```sql
   CREATE USER [tenant_user_name (like alexbeyd@kkaad.onmicrosof.com)] FROM EXTERNAL PROVIDER; 
   EXECUTE sp_addrolemember db_datareader, [tenant_user_name (like alexbeyd@kkaad.onmicrosof.com)];
   grant execute to [tenant_user_name (like alexbeyd@kkaad.onmicrosof.com)]
   ```

6. Add connection string to [appsettings.json](https://github.com/aremo-ms/ms-identity-dotnet-blazor-azure-sql/blob/master/appsettings.json)
