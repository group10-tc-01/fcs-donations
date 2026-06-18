FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/Fcs.Donations.Messages/Fcs.Donations.Messages.csproj", "src/Fcs.Donations.Messages/"]
COPY ["src/Fcs.Donations.Domain/Fcs.Donations.Domain.csproj", "src/Fcs.Donations.Domain/"]
COPY ["src/Fcs.Donations.Application/Fcs.Donations.Application.csproj", "src/Fcs.Donations.Application/"]
COPY ["src/Fcs.Donations.Infrastructure.Kafka/Fcs.Donations.Infrastructure.Kafka.csproj", "src/Fcs.Donations.Infrastructure.Kafka/"]
COPY ["src/Fcs.Donations.Infrastructure.SqlServer/Fcs.Donations.Infrastructure.SqlServer.csproj", "src/Fcs.Donations.Infrastructure.SqlServer/"]
COPY ["src/Fcs.Donations.Infrastructure.Http/Fcs.Donations.Infrastructure.Http.csproj", "src/Fcs.Donations.Infrastructure.Http/"]
COPY ["src/Fcs.Donations.Infrastructure.Auth/Fcs.Donations.Infrastructure.Auth.csproj", "src/Fcs.Donations.Infrastructure.Auth/"]
COPY ["src/Fcs.Donations.Infrastructure.MongoDb/Fcs.Donations.Infrastructure.MongoDb.csproj", "src/Fcs.Donations.Infrastructure.MongoDb/"]
COPY ["src/Fcs.Donations.WebApi/Fcs.Donations.WebApi.csproj", "src/Fcs.Donations.WebApi/"]

RUN dotnet restore "./src/Fcs.Donations.WebApi/Fcs.Donations.WebApi.csproj"

COPY . .
WORKDIR "/src/src/Fcs.Donations.WebApi"
RUN dotnet publish "./Fcs.Donations.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fcs.Donations.WebApi.dll"]
