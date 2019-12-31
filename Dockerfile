FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine as build

WORKDIR /app

COPY . .
RUN dotnet publish Destiny.ScrimTracker.sln -c Release -o /app/out/

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine as run
WORKDIR /app

COPY --from=build /app/out/ ./
EXPOSE 80
ENTRYPOINT ["dotnet","./Destiny.ScrimTracker.Api.dll"]