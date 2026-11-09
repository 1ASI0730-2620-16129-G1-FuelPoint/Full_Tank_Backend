FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TechnoSac.FullTank.Platform/TechnoSac.FullTank.Platform.csproj TechnoSac.FullTank.Platform/
RUN dotnet restore TechnoSac.FullTank.Platform/TechnoSac.FullTank.Platform.csproj
COPY TechnoSac.FullTank.Platform/ TechnoSac.FullTank.Platform/
RUN dotnet publish TechnoSac.FullTank.Platform/TechnoSac.FullTank.Platform.csproj \
    --configuration Release --no-restore --output /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=10000
COPY --from=build /out .
USER app
EXPOSE 10000
ENTRYPOINT ["dotnet", "TechnoSac.FullTank.Platform.dll"]
