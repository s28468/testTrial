```
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Never send appsettings.json or other config files with vulnerable data to public repository.

You lost .csproj file. If something similar will happen on this test, it will be 0 for that
