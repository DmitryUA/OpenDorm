#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

if ! command -v docker >/dev/null 2>&1; then
  echo "Error: Docker is required. Install Docker Engine and the Compose plugin first." >&2
  exit 1
fi
if ! docker compose version >/dev/null 2>&1; then
  echo "Error: the Docker Compose plugin is required." >&2
  exit 1
fi
if [[ ! -f "$ROOT_DIR/.env" ]]; then
  echo "Error: .env was not found. Create it from .env.example and review its settings." >&2
  exit 1
fi

required_secrets=(
  postgres_password
  search_index_hmac_key
  jwt_signing_key
  encryption_key
  open_dorm_connection_string
)
for secret_file in "${required_secrets[@]}"; do
  if [[ ! -s "$ROOT_DIR/secrets/$secret_file" ]]; then
    echo "Error: secrets/$secret_file is missing or empty." >&2
    echo "For a new installation, run ./scripts/create-secrets.sh first." >&2
    exit 1
  fi
done

docker compose config --quiet
docker compose up -d --build
docker compose ps
