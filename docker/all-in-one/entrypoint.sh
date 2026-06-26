#!/bin/bash
set -e

export MSSQL_SA_PASSWORD="${MSSQL_SA_PASSWORD:-Toka@123456}"
export ACCEPT_EULA="${ACCEPT_EULA:-Y}"

echo "=== Toka All-in-One Container Starting ==="

mkdir -p /data/db /var/log/supervisor /var/log/rabbitmq /var/run/mongodb
chown -R mongodb:mongodb /data/db 2>/dev/null || true

# SQL Server first-time setup
if [ ! -f /var/opt/mssql/.toka-initialized ]; then
  echo "Configuring SQL Server..."
  /opt/mssql/bin/mssql-conf -n setup accept-eula=Y sa-password="$MSSQL_SA_PASSWORD" 2>/dev/null || true
  touch /var/opt/mssql/.toka-initialized
fi

echo "Starting SQL Server..."
gosu mssql /opt/mssql/bin/sqlservr &
sleep 5

echo "Starting Redis..."
redis-server --daemonize yes --bind 127.0.0.1 --port 6379

echo "Starting MongoDB..."
mongod --bind_ip 127.0.0.1 --dbpath /data/db --fork --logpath /var/log/mongodb.log

echo "Starting RabbitMQ..."
rabbitmq-server -detached

/app/wait-infra.sh

echo "Starting microservices and nginx..."
exec /usr/bin/supervisord -n -c /etc/supervisor/supervisord.conf
