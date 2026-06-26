#!/bin/sh
set -e

OLLAMA_HOST="${OLLAMA_HOST:-http://ollama:11434}"
CHAT_MODEL="${OLLAMA_CHAT_MODEL:-llama3.2:1b}"
EMBED_MODEL="${OLLAMA_EMBED_MODEL:-nomic-embed-text}"

export OLLAMA_HOST

echo "Waiting for Ollama at $OLLAMA_HOST..."
until ollama list >/dev/null 2>&1; do
  sleep 3
done

echo "Pulling chat model: $CHAT_MODEL (first run may take several minutes)..."
ollama pull "$CHAT_MODEL"

echo "Pulling embedding model: $EMBED_MODEL..."
ollama pull "$EMBED_MODEL"

echo "Ollama models ready."
