# Build psiconv first

# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
# FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build

LABEL maintainer="kian@orangetentacle.co.uk"

RUN apt update && apt install -y git wget build-essential

WORKDIR /root
RUN wget https://github.com/jgm/pandoc/releases/download/3.1.2/pandoc-3.1.2-1-amd64.deb

WORKDIR /opt
RUN git clone https://github.com/kianryan/psiconv
RUN cd psiconv && ./configure
RUN cd psiconv && make

WORKDIR /source
# copy csproj and restore as distinct layers
COPY app/*.csproj .
RUN dotnet restore --use-current-runtime

# copy everything else and build app
COPY app/. .
RUN dotnet publish --use-current-runtime --self-contained false --no-restore -o /app

# final stage/image
# FROM mcr.microsoft.com/dotnet/aspnet:7.0
FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim

WORKDIR /root
COPY --from=build /root/pandoc-3.1.2-1-amd64.deb .
RUN dpkg -i pandoc-3.1.2-1-amd64.deb

WORKDIR /opt
RUN mkdir psiconv
COPY --from=build /opt/psiconv psiconv

WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "app.dll"]