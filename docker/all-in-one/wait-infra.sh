#!/bin/bash
set -e

echo "Waiting for infrastructure..."

wait_port() {
  local host=$1 port=$2 name=$3
  for i in $(seq 1 90); do
    if bash -c "echo > /dev/tcp/$host/$port" 2>/dev/null; then
      echo "$name is ready on $host:$port"
      return 0
    fi
    sleep 2
  done
  echo "ERROR: $name did not start on $host:$port"
  exit 1
}

wait_port 127.0.0.1 1433 "SQL Server"
wait_port 127.0.0.1 6379 "Redis"
wait_port 127.0.0.1 27017 "MongoDB"
wait_port 127.0.0.1 5672 "RabbitMQ"

echo "Infrastructure ready. Microservices will start..."
sleep 5
