FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY *.sln *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c release -o /app --no-restore --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:DebugType=None -p:DebugSymbols=false

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-noble-chiseled
WORKDIR /app
COPY --from=build /app ./

ENV URLS="http://+:5000"

EXPOSE 5000

ENTRYPOINT ["/app/webring"]