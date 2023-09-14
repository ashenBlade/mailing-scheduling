FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MailingExample.Console/MailingExample.Console.csproj", "MailingExample.Console/"]
RUN dotnet restore "MailingExample.Console/MailingExample.Console.csproj"
COPY . .
WORKDIR "/src/MailingExample.Console"
RUN dotnet build "MailingExample.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MailingExample.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MailingExample.Console.dll"]
