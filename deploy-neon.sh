#!/bin/bash
set -e

# 1. Cargar variables de entorno
if [ -f ./.env ]; then
    while IFS='=' read -r key value; do
        if [[ ! -z "$key" && ! "${key:0:1}" == "#" ]]; then
            export "$key"="$value"
        fi
    done < ./.env
else
    echo ".env file not found!"
    exit 1
fi

if [ -z "$PEM_PATH" ] || [ -z "$SERVER_ADDRESS" ] || [ -z "$NEON" ]; then
    echo "PEM_PATH, SERVER_ADDRESS, or NEON not set in .env file."
    exit 1
fi

# Convert Neon URI to .NET Connection String if necessary
if [[ $NEON == postgresql://* ]]; then
    echo "Converting Neon URI to .NET Connection String format..."
    NEON_CLEAN=$(echo $NEON | sed 's/postgresql:\/\///')
    USER_PASS=$(echo $NEON_CLEAN | cut -d'@' -f1)
    HOST_DB_OPTS=$(echo $NEON_CLEAN | cut -d'@' -f2)
    USER=$(echo $USER_PASS | cut -d':' -f1)
    PASS=$(echo $USER_PASS | cut -d':' -f2)
    HOST_DB=$(echo $HOST_DB_OPTS | cut -d'?' -f1)
    HOST=$(echo $HOST_DB | cut -d'/' -f1)
    DB=$(echo $HOST_DB | cut -d'/' -f2)
    NEON="Host=$HOST;Database=$DB;Username=$USER;Password=$PASS;SSL Mode=Require;Trust Server Certificate=true;"
fi

TARGET=${1:-all}
echo "Deploying target: $TARGET to $SERVER_ADDRESS"

# Limpiar tars locales
rm -f xposx-backend.tar xposx-frontend.tar

# 2. Build local
if [ "$TARGET" == "all" ] || [ "$TARGET" == "backend" ]; then
    echo "--- Building Backend ---"
    docker build -t xposx-backend:local -f ./backend/Dockerfile ./backend
    docker save -o xposx-backend.tar xposx-backend:local
fi

if [ "$TARGET" == "all" ] || [ "$TARGET" == "frontend" ]; then
    echo "--- Building Frontend ---"
    # Build with NO CACHE to force latest changes
    # Now using relative URL /api because Nginx will proxy it
    docker build --build-arg VITE_API_URL="/api" -t xposx-frontend:local -f ./frontend/Dockerfile ./frontend
    docker save -o xposx-frontend.tar xposx-frontend:local
fi

# 3. Transferir al servidor
echo "--- Transferring files ---"
# Eliminar tars antiguos en el servidor para evitar corrupciones
ssh -i "$PEM_PATH" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null "ubuntu@$SERVER_ADDRESS" "mkdir -p ~/xposx_deploy && rm -f ~/xposx_deploy/*.tar"

rsync -avzP -e "ssh -i \"$PEM_PATH\" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null" \
    ./docker-compose.yml \
    ./docker-compose.prod.yml \
    ./docker-compose.neon.yml \
    ubuntu@"$SERVER_ADDRESS":~/xposx_deploy/

if [ -f xposx-backend.tar ]; then
    rsync -avzP -e "ssh -i \"$PEM_PATH\" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null" \
        xposx-backend.tar \
        ubuntu@"$SERVER_ADDRESS":~/xposx_deploy/
fi

if [ -f xposx-frontend.tar ]; then
    rsync -avzP -e "ssh -i \"$PEM_PATH\" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null" \
        xposx-frontend.tar \
        ubuntu@"$SERVER_ADDRESS":~/xposx_deploy/
fi

# 4. Despliegue en el servidor
echo "--- Remote Deployment ---"
ssh -i "$PEM_PATH" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null "ubuntu@$SERVER_ADDRESS" \
    "NEON='$NEON' JWT_KEY='$JWT_KEY' JWT_ISSUER='$JWT_ISSUER' JWT_AUDIENCE='$JWT_AUDIENCE' JWT_EXPIRE_DAYS='$JWT_EXPIRE_DAYS' bash -s" << 'EOF'
    set -e
    cd ~/xposx_deploy

    echo "Loading images..."
    if [ -f xposx-backend.tar ]; then
        docker load -i xposx-backend.tar
    fi
    if [ -f xposx-frontend.tar ]; then
        docker load -i xposx-frontend.tar
    fi

    echo "Restarting services with Neon DB..."
    # The variables are already in the environment from the SSH command prefix
    docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.neon.yml up -d --remove-orphans

    echo "Status:"
    docker compose ps

    echo "Cleaning up tar files..."
    rm -f *.tar
EOF

# Cleanup local
rm -f xposx-backend.tar xposx-frontend.tar

echo "Deployment complete!"
