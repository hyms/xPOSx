#!/bin/bash

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

if [ -z "$PEM_PATH" ] || [ -z "$SERVER_ADDRESS" ]; then
    echo "PEM_PATH or SERVER_ADDRESS not set in .env file."
    exit 1
fi

echo "--- Preparing Remote Server: $SERVER_ADDRESS ---"

# 2. Execute preparation script on remote server via SSH Here-Document
ssh -i "$PEM_PATH" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null "ubuntu@$SERVER_ADDRESS" bash -s << 'EOF'
    set -e

    echo "--- Running on remote server ---"

    # 2.1. Check and configure swap space
    echo "Checking and configuring swap space..."
    CURRENT_SWAP_GB=$(free -g | awk '/Swap:/ {print $2}')

    if [[ "$CURRENT_SWAP_GB" -ge 3 ]]; then
        echo "✅ Swap space check: OK ($CURRENT_SWAP_GB GB found, no changes needed)"
    else
        echo "⚠️ Insufficient swap space ($CURRENT_SWAP_GB GB found, 3GB required). Configuring 3GB swap..."
        if [ -f /swapfile ]; then
            sudo swapoff /swapfile || true
            sudo rm /swapfile
        fi
        sudo fallocate -l 3G /swapfile
        sudo chmod 600 /swapfile
        sudo mkswap /swapfile
        sudo swapon /swapfile
        # Make persistent
        if ! grep -q "/swapfile swap swap defaults 0 0" /etc/fstab; then
            echo "/swapfile swap swap defaults 0 0" | sudo tee -a /etc/fstab
        fi
        echo "✅ Swap space configured to 3GB."
    fi

    # 2.2. Check Docker Installation
    echo "Checking Docker installation..."
    NEEDS_RELOG_DOCKER=false
    if which docker > /dev/null 2>&1; then
        echo "✅ Docker is installed."
    else
        echo "⚠️ Docker not found. Installing Docker Engine..."
        sudo apt-get update
        sudo apt-get install -y ca-certificates curl gnupg
        sudo install -m 0755 -d /etc/apt/keyrings
        curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
        sudo chmod a+r /etc/apt/keyrings/docker.gpg

        echo \
          "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
          $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
          sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
        sudo apt-get update
        sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
        
        sudo usermod -aG docker ${USER}
        NEEDS_RELOG_DOCKER=true
        echo "✅ Docker Engine installed."
    fi

    # 2.3. Check Docker group membership for current user
    if ! groups ${USER} | grep -q '\bdocker\b'; then
        echo "⚠️ User '${USER}' is not in the docker group. Adding..."
        sudo usermod -aG docker ${USER}
        NEEDS_RELOG_DOCKER=true
    else
        echo "✅ User '${USER}' is already in the docker group."
    fi

    # 2.4. Update and Upgrade System
    echo "Updating and upgrading system packages..."
    sudo apt-get update
    sudo apt-get upgrade -y
    sudo apt-get autoremove -y
    echo "✅ System updated and upgraded."

    echo "--- Remote script finished ---"

    if [ "$NEEDS_RELOG_DOCKER" = true ]; then
        echo "Exit code 1: User added to docker group, re-login required."
        exit 1
    fi
EOF

# 3. Check the exit code from the remote script
SSH_EXIT_CODE=$?
if [ $SSH_EXIT_CODE -eq 1 ]; then
    echo
    echo "----------------------------------------------------------------------------------"
    echo "✅ Docker setup complete. A RE-LOGIN IS REQUIRED."
    echo "The user 'ubuntu' was added to the 'docker' group."
    echo "For the changes to take effect, the SSH session must be restarted."
    echo "Please re-run this script (./prepare-server.sh) after a few moments to complete verification."
    echo "----------------------------------------------------------------------------------"
    exit 0
elif [ $SSH_EXIT_CODE -ne 0 ]; then
    echo "❌ An error occurred on the remote server. SSH exit code: $SSH_EXIT_CODE"
    exit $SSH_EXIT_CODE
fi

echo "✅ Server preparation finished successfully!"
