FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PRN232.LAB_1.API/PRN232.LAB_1.API.csproj PRN232.LAB_1.API/
COPY PRN232.LAB_1.Services/PRN232.LAB_1.Services.csproj PRN232.LAB_1.Services/
COPY PRN232.LAB_1.Repositories/PRN232.LAB_1.Repositories.csproj PRN232.LAB_1.Repositories/
COPY Lab1.sln .
RUN dotnet restore PRN232.LAB_1.API/PRN232.LAB_1.API.csproj

COPY . .
RUN dotnet publish PRN232.LAB_1.API/PRN232.LAB_1.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PRN232.LAB_1.API.dll"]
