# Create Solution
dotnet --version
dotnet new sln -n PaymentLite

# Create API project
dotnet new webapi -n PaymentLite.Api --framework net8.0
dotnet sln add PaymentLite.API/PaymentLite.API.csproj

# Add Packages
dotnet add package Microsoft.EntityFrameworkCore --version=8.0.3
dotnet add package Microsoft.EntityFrameworkCore.Design --version=8.0.3
dotnet add package Pomelo.EntityFrameworkCore.MySql --version=8.0.2 

# Environment setup
export MYSQL_ROOT_PASSWORD_RAILWAY=your_password"
echo 'export MYSQL_ROOT_PASSWORD="your_password_here"' >> ~/.zshrc && source ~/.zshrc

# Build and Run Project
dotnet build
dotnet run

# Migrations
dotnet ef migrations add initmigration
dotnet ef database update

# Publish
dotnet publish -c Release