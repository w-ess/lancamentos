FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG PROJETO
ARG DLL_NAME
WORKDIR /src

COPY FluxoDeCaixa.sln ./
COPY src ./src

RUN dotnet restore "$PROJETO"
RUN dotnet publish "$PROJETO" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
ARG DLL_NAME
ENV DLL_NAME=$DLL_NAME
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl procps \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

ENTRYPOINT ["sh", "-c", "dotnet \"$DLL_NAME\""]
