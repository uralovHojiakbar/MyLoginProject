FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ../MyProject.sln .
COPY ../MyProject/MyProject.csproj MyProject/
RUN dotnet restore

COPY ../ .
WORKDIR /src/MyProject
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyProject.dll"]
