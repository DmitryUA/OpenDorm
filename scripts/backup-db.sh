#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

if ! command -v docker >/dev/null 2>&1 || ! docker compose version >/dev/null 2>&1; then
  echo "Error: Docker Engine and the Compose plugin are required." >&2
  exit 1
fi

if ! docker compose exec -T db sh -c 'pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB"' >/dev/null; then
  echo "Error: the database container is not running or is not ready." >&2
  exit 1
fi

BACKUP_DIR="$ROOT_DIR/backups"
umask 077
mkdir -p -- "$BACKUP_DIR"
chmod 0700 "$BACKUP_DIR"

timestamp="$(date -u +%Y%m%dT%H%M%SZ)"
backup_file="$BACKUP_DIR/opendorm_${timestamp}_$$.dump"
tmp_file="$(mktemp "$BACKUP_DIR/.opendorm-backup.XXXXXX")"

cleanup() {
  if [[ -n "$tmp_file" && -e "$tmp_file" ]]; then
    rm -f -- "$tmp_file"
  fi
}
trap cleanup EXIT

chmod 0600 "$tmp_file"
docker compose exec -T db sh -c \
  'export PGPASSWORD="$(cat /run/secrets/postgres_password)"; exec pg_dump --username="$POSTGRES_USER" --dbname="$POSTGRES_DB" --format=custom' \
  > "$tmp_file"

if [[ ! -s "$tmp_file" ]]; then
  echo "Error: pg_dump produced an empty backup." >&2
  exit 1
fi

docker compose exec -T db pg_restore --list < "$tmp_file" >/dev/null
mv -- "$tmp_file" "$backup_file"
tmp_file=''

echo "Database backup created: $backup_file"
