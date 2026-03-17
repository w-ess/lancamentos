#!/usr/bin/env bash
set -euo pipefail

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres <<'EOSQL'
CREATE DATABASE lancamentos_db;
CREATE DATABASE consolidado_db;
EOSQL
