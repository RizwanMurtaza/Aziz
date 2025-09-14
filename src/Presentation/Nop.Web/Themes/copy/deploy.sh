#!/bin/bash

# HTML Template Deployment Script
# This script deploys HTML template to Ubuntu server with Nginx

set -e

# Configuration
SERVER_IP="172.236.28.69"
SERVER_USER="root"
DOMAIN_NAME="design.gamingpcrehab.co.uk"
APP_NAME="design"
REMOTE_APP_DIR="/var/www/${APP_NAME}"

echo "========================================="
echo "HTML Template Deployment Script"
echo "========================================="

# Step 1: Create deployment package (excluding deploy script and README)
echo "Step 1: Creating deployment package..."
tar -czf deploy.tar.gz --exclude='deploy.sh' --exclude='README.md' --exclude='.git' .

# Step 2: Copy deployment package to server
echo "Step 2: Copying deployment package to server..."
scp deploy.tar.gz ${SERVER_USER}@${SERVER_IP}:/tmp/

# Step 3: Execute remote deployment
echo "Step 3: Executing remote deployment..."
ssh ${SERVER_USER}@${SERVER_IP} << 'ENDSSH'

set -e

DOMAIN_NAME="design.gamingpcrehab.co.uk"
APP_NAME="design"
REMOTE_APP_DIR="/var/www/${APP_NAME}"

echo "========================================="
echo "Remote deployment started"
echo "========================================="

# Check if nginx is installed
if ! command -v nginx &> /dev/null; then
    echo "Nginx not found. Installing nginx..."
    apt-get update -y
    apt-get install -y nginx
else
    echo "Nginx is already installed"
fi

# Install certbot if not already installed
if ! command -v certbot &> /dev/null; then
    echo "Installing certbot..."
    apt-get update -y
    apt-get install -y certbot python3-certbot-nginx
fi

# Create application directory
echo "Creating application directory..."
mkdir -p ${REMOTE_APP_DIR}

# Extract deployment package
echo "Extracting deployment package..."
tar -xzf /tmp/deploy.tar.gz -C ${REMOTE_APP_DIR}
chown -R www-data:www-data ${REMOTE_APP_DIR}
chmod -R 755 ${REMOTE_APP_DIR}

# Create Nginx configuration for static HTML
echo "Creating Nginx configuration..."
cat > /etc/nginx/sites-available/${APP_NAME} << EOF
server {
    listen 80;
    server_name ${DOMAIN_NAME} www.${DOMAIN_NAME};
    
    root ${REMOTE_APP_DIR};
    index index.html index.htm;

    location / {
        try_files \$uri \$uri/ =404;
    }

    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 365d;
        add_header Cache-Control "public, immutable";
        access_log off;
    }

    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/javascript application/xml+rss application/json;
}
EOF

# Enable Nginx site
ln -sf /etc/nginx/sites-available/${APP_NAME} /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx

# Obtain SSL certificate
echo "Obtaining SSL certificate..."
certbot --nginx -d ${DOMAIN_NAME} -d www.${DOMAIN_NAME} --non-interactive --agree-tos --email admin@gamingpcrehab.co.uk --redirect

# Configure firewall
echo "Configuring firewall..."
ufw allow 'Nginx Full'
ufw allow OpenSSH
ufw --force enable

# Clean up
rm -f /tmp/deploy.tar.gz

echo "========================================="
echo "Deployment completed successfully!"
echo "========================================="
echo "Website URL: https://${DOMAIN_NAME}"
echo "========================================="

ENDSSH

# Clean up local files
echo "Cleaning up local files..."
rm -f deploy.tar.gz

echo "========================================="
echo "Deployment script completed!"
echo "========================================="
echo "Your HTML template should now be available at:"
echo "https://${DOMAIN_NAME}"
echo "========================================="