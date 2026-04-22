FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY .tool-versions .
RUN BUF_VERSION=$(grep '^buf ' .tool-versions | awk '{print $2}') \
 && curl -fsSL "https://github.com/bufbuild/buf/releases/download/v${BUF_VERSION}/buf-$(uname -s)-$(uname -m)" \
      -o /usr/local/bin/buf \
 && chmod +x /usr/local/bin/buf

COPY Directory.Build.props .
COPY Directory.Build.targets .
COPY Directory.Packages.props .

COPY src/Peerly.Auth/Peerly.Auth.csproj                                         src/Peerly.Auth/
COPY src/Peerly.Auth.Api/Peerly.Auth.Api.csproj                                 src/Peerly.Auth.Api/
COPY src/Peerly.Auth.ApplicationServices/Peerly.Auth.ApplicationServices.csproj src/Peerly.Auth.ApplicationServices/
COPY src/Peerly.Auth.Hosting/Peerly.Auth.Hosting.csproj                         src/Peerly.Auth.Hosting/
COPY src/Peerly.Auth.Persistence/Peerly.Auth.Persistence.csproj                 src/Peerly.Auth.Persistence/
COPY src/Peerly.Auth.Tools/Peerly.Auth.Tools.csproj                             src/Peerly.Auth.Tools/

RUN dotnet restore src/Peerly.Auth.Hosting/Peerly.Auth.Hosting.csproj

COPY . .

RUN buf generate

RUN dotnet publish src/Peerly.Auth.Hosting/Peerly.Auth.Hosting.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Peerly.Auth.Hosting.dll"]
