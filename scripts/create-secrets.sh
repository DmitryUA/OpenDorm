#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
SECRETS_DIR="$ROOT_DIR/secrets"
SECRET_FILES=(
  postgres_password
  search_index_hmac_key
  jwt_signing_key
  encryption_key
  open_dorm_connection_string
)

read_env_value() {
  local key="$1"
  local default_value="$2"
  local value=''

  if [[ -f "$ROOT_DIR/.env" ]]; then
    value="$(awk -F= -v key="$key" '$1 == key { sub(/^[^=]*=/, ""); print; exit }' "$ROOT_DIR/.env")"
  fi

  printf '%s' "${value:-$default_value}"
}

POSTGRES_DB="$(read_env_value POSTGRES_DB open_dorm_db)"
POSTGRES_USER="$(read_env_value POSTGRES_USER open_dorm)"
if [[ ! "$POSTGRES_DB" =~ ^[A-Za-z0-9_]+$ || ! "$POSTGRES_USER" =~ ^[A-Za-z0-9_]+$ ]]; then
  echo "Error: POSTGRES_DB and POSTGRES_USER must contain only letters, digits, or underscores." >&2
  exit 1
fi

if ! command -v openssl >/dev/null 2>&1; then
  echo "Error: openssl is required to generate secrets." >&2
  exit 1
fi

if [[ -e "$SECRETS_DIR" ]]; then
  if [[ -d "$SECRETS_DIR" && ! -L "$SECRETS_DIR" ]]; then
    complete=true
    for secret_file in "${SECRET_FILES[@]}"; do
      if [[ ! -f "$SECRETS_DIR/$secret_file" || -L "$SECRETS_DIR/$secret_file" || ! -s "$SECRETS_DIR/$secret_file" ]]; then
        complete=false
        break
      fi
    done
    if [[ "$complete" == true ]]; then
      echo "Secrets already exist; leaving them unchanged."
      exit 0
    fi
  fi

  echo "Error: $SECRETS_DIR already exists and is incomplete or invalid." >&2
  echo "Do not replace secrets for an existing database; restore the original secret files." >&2
  exit 1
fi

umask 077
mkdir -m 0700 -- "$SECRETS_DIR"
openssl rand -hex 32 > "$SECRETS_DIR/postgres_password"
openssl rand -base64 32 > "$SECRETS_DIR/search_index_hmac_key"
openssl rand -base64 48 > "$SECRETS_DIR/jwt_signing_key"
openssl rand -base64 32 > "$SECRETS_DIR/encryption_key"
awk -v database="$POSTGRES_DB" -v username="$POSTGRES_USER" \
  '{ print "Host=db;Port=5432;Database=" database ";Username=" username ";Password=" $0 }' \
  "$SECRETS_DIR/postgres_password" > "$SECRETS_DIR/open_dorm_connection_string"
chmod 0444 "$SECRETS_DIR"/*

echo "Created secrets in $SECRETS_DIR."
