FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MyProject.sln ./
COPY MyProject/MyProject.csproj MyProject/
COPY Application/*.csproj Application/
COPY Domain/*.csproj Domain/
COPY Infrastructure/*.csproj Infrastructure/

RUN dotnet restore MyProject/MyProject.csproj

COPY . .
RUN dotnet publish MyProject/MyProject.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet","MyProject.dll"]
