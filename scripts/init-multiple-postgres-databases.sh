#!/bin/bash
# Cria um database para cada nome listado em POSTGRES_MULTIPLE_DATABASES
# (separados por vírgula). Roda automaticamente no primeiro boot do container
# Postgres, via /docker-entrypoint-initdb.d/.
#
# O database definido em POSTGRES_DB já é criado pela imagem oficial,
# então esse script ignora duplicata se ele aparecer na lista.
set -e
set -u
function create_database() {
        local database=$1
        echo "  -> Verificando/criando database '$database'"
        psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" <<-EOSQL
            SELECT 'CREATE DATABASE "$database"'
            WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$database')\gexec
EOSQL
}
if [ -n "${POSTGRES_MULTIPLE_DATABASES:-}" ]; then
        echo "Databases múltiplos solicitados: $POSTGRES_MULTIPLE_DATABASES"
        for db in $(echo "$POSTGRES_MULTIPLE_DATABASES" | tr ',' ' '); do
                create_database "$db"
        done
        echo "Databases prontos."
fi
