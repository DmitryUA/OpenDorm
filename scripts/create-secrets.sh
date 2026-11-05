mkdir -p ../secrets
openssl rand -hex 32 > ../secrets/postgres_password
openssl rand -base64 32 > ../secrets/search_index_hmac_key
openssl rand -base64 48 > ../secrets/jwt_signing_key
openssl rand -base64 32 > ../secrets/encryption_key
cat ../secrets/postgres_password | awk '{ print "Host=db;Port=5432;Database=open_dorm_db;Username=open_dorm;Password=" $0 }' \
  > ../secrets/open_dorm_connection_string
chmod 0700 ../secrets
chmod 0444 ../secrets/*
