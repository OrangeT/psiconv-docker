# Build psiconv first
# FROM ubuntu:20.04 AS psiconv
FROM alpine:3.14 AS psiconv
FROM debian:bullseye-slim AS psiconv
LABEL maintainer="kian@orangetentacle.co.uk"
# RUN apk add --no-cache git
# RUN apk add --no-cache build-base
RUN apt update && apt install -y git build-essential
WORKDIR /opt
RUN git clone https://github.com/kianryan/psiconv
RUN cd psiconv && ./configure
RUN cd psiconv && make

# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
# FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build

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
WORKDIR /opt
RUN mkdir psiconv
COPY --from=psiconv /opt/psiconv psiconv
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "app.dll"]