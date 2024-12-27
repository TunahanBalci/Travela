# Travel Explorer Web Application

## Overview

Travela is a dynamic web application built with ASP.NET Core 8.0 MVC that allows users to explore various travel destinations and accommodations. Users can sign up, manage their profiles, favorite destinations, book accommodations, and access detailed reviews and statistics. Administrators have access to a comprehensive admin panel to manage entities within the application.

## Features

- **User Authentication:**
  - Sign up and sign in with secure authentication.
  - Password management and profile updates.

- **Destination & Accommodation Management:**
  - Browse and explore destinations and accommodations.
  - View detailed information, reviews, and statistics.
  - Favorite destinations and book accommodations.

- **Admin Panel:**
  - List, create, edit, and delete entities.
  - Accessible to users with admin privileges.

- **User Dashboard:**
  - View favorites, past travels, and user preferences.
  - Receive notifications for unreviewed visits.

- **Logging:**
  - Comprehensive logging of application activities.
  - Logs are accessible via the terminal and saved to files.

## Libraries Used

- **Authentication & Security:**
  - `Microsoft.AspNetCore.Authentication.Cookies` (v2.2.0): Provides cookie-based authentication mechanisms.
  - `Microsoft.AspNetCore.Authentication.JwtBearer` (v8.0.11): Supports JSON Web Token (JWT) authentication.

- **Entity Framework Core:**
  - `Microsoft.EntityFrameworkCore.Design` (v9.0.0): Design-time tools for Entity Framework Core.
  - `Microsoft.EntityFrameworkCore.SqlServer` (v9.0.0): Enables EF Core to work with SQL Server databases.
  - `Microsoft.EntityFrameworkCore.Tools` (v9.0.0): CLI commands for Entity Framework Core.

- **Scaffolding & Code Generation:**
  - `Microsoft.VisualStudio.Web.CodeGeneration.Design` (v8.0.7): Tools for scaffolding and code generation in ASP.NET Core projects.

- **Logging:**
  - `Serilog` (v4.2.0): A powerful library for logging.
  - `Serilog.AspNetCore` (v9.0.0): Integrates Serilog with ASP.NET Core applications.
  - `Serilog.Sinks.File` (v6.0.0): Allows logging data to be written to files.

## Prerequisites

- **.NET SDK:** Ensure you have the .NET SDK installed. [Download .NET](https://dotnet.microsoft.com/download)
- **SQL Server:** A running instance of SQL Server is required. [Download SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **IDE:** Visual Studio 2022 or later is recommended for best compatibility.

## Setup Instructions

1. **Download The Repository**
2. **Configure appsettings.json:**
Replace YOUR_SERVER_NAME with your MSSQL Server name:

 ``` appsettings.json
 "DatabaseConnection": "Server=YOUR_SERVER_NAME;Database=mainDB;Trusted_Connection=True;TrustServerCertificate=True;Integrated Security=True;" 
```

3. **Create Database Through Migrations**

Delete all migrations inside the migrations folder.
In the package manager console, use these commands in order:

```Package Manager Console
add-migration migration_name 
```
```Package Manager Console
update-database
```

4. **Add An Admin**

Adding an admin is possible only through directly accessing the database.

Use SQL query to add an admin with the selected email.
For this, replace admin@app.com with admin's email in database.

``` SQL Query
UPDATE Users
SET isAdmin = 1
WHERE email = 'admin@app.com';
```
