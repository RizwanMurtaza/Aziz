#!/bin/bash

# Website Deployment Script
# This script builds and deploys the website to server

set -e

# Configuration
SERVER_IP="172.236.28.69"
SERVER_USER="root"
APP_NAME="gamingpcrehab"
PUBLISH_DIR="./publish"
REMOTE_APP_DIR="/var/www/${APP_NAME}"

echo "========================================="
echo "Website Deployment Script"
echo "========================================="

# Step 1: Build the project locally
echo "Step 1: Building project..."
rm -rf ${PUBLISH_DIR}
cd src/Presentation/Nop.Web
dotnet publish -c Release -o ../../../${PUBLISH_DIR} --self-contained false
cd ../../..

# Step 2: Ensure app_data folder is included in build output
echo "Step 2: Ensuring app_data folder is included..."
if [ -d "src/Presentation/Nop.Web/app_data" ]; then
    echo "Copying app_data from source..."
    cp -r src/Presentation/Nop.Web/app_data ${PUBLISH_DIR}/
fi

# Step 3: Create deployment package (including all app_data files)
echo "Step 3: Creating deployment package..."
tar -czf deploy.tar.gz -C ${PUBLISH_DIR} .

# Step 4: Copy deployment package to server
echo "Step 4: Copying deployment package to server..."
scp deploy.tar.gz ${SERVER_USER}@${SERVER_IP}:/tmp/

# Step 5: Execute remote deployment
echo "Step 5: Executing remote deployment..."
ssh ${SERVER_USER}@${SERVER_IP} << 'ENDSSH'

set -e

APP_NAME="gamingpcrehab"
REMOTE_APP_DIR="/var/www/${APP_NAME}"

echo "========================================="
echo "Remote deployment started"
echo "========================================="

# Stop the application service
echo "Stopping application service..."
systemctl stop ${APP_NAME}.service || true

# Backup current app_data if it exists
echo "Backing up app_data folder..."
if [ -d "${REMOTE_APP_DIR}/app_data" ]; then
    cp -r ${REMOTE_APP_DIR}/app_data /tmp/app_data_backup
    echo "app_data backed up to /tmp/app_data_backup"
fi

# Create application directory
echo "Creating application directory..."
mkdir -p ${REMOTE_APP_DIR}
cd ${REMOTE_APP_DIR}

# Remove old files but keep app_data
echo "Removing old files (preserving app_data)..."
find ${REMOTE_APP_DIR} -mindepth 1 -maxdepth 1 ! -name 'app_data' -exec rm -rf {} +

# Extract deployment package
echo "Extracting deployment package..."
tar -xzf /tmp/deploy.tar.gz -C ${REMOTE_APP_DIR}

# Restore app_data if backup exists
if [ -d "/tmp/app_data_backup" ]; then
    echo "Restoring app_data from backup..."
    cp -r /tmp/app_data_backup ${REMOTE_APP_DIR}/app_data
    rm -rf /tmp/app_data_backup
fi

# Set permissions
chown -R www-data:www-data ${REMOTE_APP_DIR}
chmod -R 755 ${REMOTE_APP_DIR}

# Start application service
echo "Starting application service..."
systemctl start ${APP_NAME}.service

# Wait for service to start
sleep 5

# Check service status
if systemctl is-active --quiet ${APP_NAME}.service; then
    echo "Service started successfully"
else
    echo "Warning: Service may not have started properly"
    systemctl status ${APP_NAME}.service
fi

# Setup SSL with proper Nginx configuration
echo "Setting up SSL and Nginx configuration..."
DOMAIN_NAME="gamingpcrehab.co.uk"
APP_PORT="5001"

# Create proper Nginx configuration
cat > /etc/nginx/sites-available/${APP_NAME} << EOF
server {
    listen 80;
    server_name ${DOMAIN_NAME} www.${DOMAIN_NAME};
    return 301 https://\$host\$request_uri;
}

server {
    listen 443 ssl http2;
    server_name ${DOMAIN_NAME} www.${DOMAIN_NAME};

    ssl_certificate /etc/letsencrypt/live/${DOMAIN_NAME}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${DOMAIN_NAME}/privkey.pem;
    include /etc/letsencrypt/options-ssl-nginx.conf;
    ssl_dhparam /etc/letsencrypt/ssl-dhparams.pem;

    location / {
        proxy_pass http://localhost:${APP_PORT};
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;
        proxy_set_header X-Forwarded-Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_buffer_size 128k;
        proxy_buffers 4 256k;
        proxy_busy_buffers_size 256k;
        client_max_body_size 50M;
    }

    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        proxy_pass http://localhost:${APP_PORT};
        proxy_cache_valid 200 1d;
        proxy_cache_bypass \$http_upgrade;
        expires 365d;
        add_header Cache-Control "public, immutable";
        proxy_set_header X-Forwarded-Proto https;
    }
}
EOF

# Enable Nginx site and test configuration
ln -sf /etc/nginx/sites-available/${APP_NAME} /etc/nginx/sites-enabled/
if nginx -t; then
    systemctl reload nginx
    echo "Nginx configuration updated successfully"
else
    echo "Error: Nginx configuration test failed"
    exit 1
fi

# Obtain or renew SSL certificate if needed
if [ ! -f "/etc/letsencrypt/live/${DOMAIN_NAME}/fullchain.pem" ]; then
    echo "Obtaining SSL certificate..."
    certbot --nginx -d ${DOMAIN_NAME} -d www.${DOMAIN_NAME} --non-interactive --agree-tos --email admin@${DOMAIN_NAME} --redirect
else
    echo "SSL certificate already exists, renewing if needed..."
    certbot renew --quiet
fi

# Clean up
rm -f /tmp/deploy.tar.gz

echo "========================================="
echo "Website deployment completed!"
echo "========================================="

ENDSSH

# Clean up local files
echo "Cleaning up local files..."
rm -f deploy.tar.gz

echo "========================================="
echo "Website deployment script completed!"
echo "========================================="