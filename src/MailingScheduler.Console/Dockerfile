FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/MailingScheduler.Console/MailingScheduler.Console.csproj", "src/MailingScheduler.Console/"]
COPY ["src/MailingScheduler.Core/MailingScheduler.Core.csproj", "src/MailingScheduler.Core/"]
COPY ["src/MailingScheduler.Database/MailingScheduler.Database.csproj", "src/MailingScheduler.Database/"]
COPY ["src/MailingScheduler.PlanningStrategy/MailingScheduler.PlanningStrategy.csproj", "src/MailingScheduler.PlanningStrategy/"]
COPY ["src/MailingScheduler.Statistics/MailingScheduler.Statistics.csproj", "src/MailingScheduler.Statistics/"]
RUN dotnet restore "src/MailingScheduler.Console/MailingScheduler.Console.csproj"
COPY . .
WORKDIR "/src/src/MailingScheduler.Console"
RUN dotnet build "MailingScheduler.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MailingScheduler.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MailingScheduler.Console.dll"]
