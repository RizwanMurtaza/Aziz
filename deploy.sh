#!/bin/bash

# NopCommerce Deployment Script
# This script builds and deploys NopCommerce to Ubuntu server with Nginx and MySQL

set -e

# Configuration
SERVER_IP="172.236.28.69"
SERVER_USER="root"
DOMAIN_NAME="gamingpcrehab.co.uk"
MYSQL_ROOT_PASSWORD="rizwan321"
APP_NAME="gamingpcrehab"
APP_PORT="5001"
PUBLISH_DIR="./publish"
REMOTE_APP_DIR="/var/www/${APP_NAME}"

echo "========================================="
echo "NopCommerce Deployment Script"
echo "========================================="

# Step 1: Build the project locally
echo "Step 1: Building NopCommerce project..."
rm -rf ${PUBLISH_DIR}
cd src/Presentation/Nop.Web
dotnet publish -c Release -o ../../../${PUBLISH_DIR} --self-contained false
cd ../../..

# Step 2: Create deployment package
echo "Step 2: Creating deployment package..."
tar -czf deploy.tar.gz -C ${PUBLISH_DIR} .

# Step 3: Copy deployment package to server
echo "Step 3: Copying deployment package to server..."
scp deploy.tar.gz ${SERVER_USER}@${SERVER_IP}:/tmp/

# Step 4: Execute remote deployment
echo "Step 4: Executing remote deployment..."
ssh ${SERVER_USER}@${SERVER_IP} << 'ENDSSH'

set -e

DOMAIN_NAME="gamingpcrehab.co.uk"
MYSQL_ROOT_PASSWORD="rizwan321"
APP_NAME="gamingpcrehab"
APP_PORT="5001"
REMOTE_APP_DIR="/var/www/${APP_NAME}"

echo "========================================="
echo "Remote deployment started"
echo "========================================="

# Update system
echo "Updating system packages..."
apt-get update -y
apt-get upgrade -y

# Install required packages
echo "Installing required packages..."
# Check if nginx is installed
if ! command -v nginx &> /dev/null; then
    echo "Nginx not found. Installing nginx..."
    apt-get install -y nginx
else
    echo "Nginx is already installed"
fi
# Install other required packages
apt-get install -y certbot python3-certbot-nginx mysql-server
# Install .NET 9.0 runtime
rm -f /etc/apt/sources.list.d/microsoft-prod.list
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt-get update
apt-get install -y dotnet-runtime-9.0 aspnetcore-runtime-9.0

# Configure MySQL
echo "Configuring MySQL..."
systemctl start mysql
systemctl enable mysql

# Set MySQL root password and configure for public access
mysql -e "ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY '${MYSQL_ROOT_PASSWORD}';"
mysql -u root -p${MYSQL_ROOT_PASSWORD} -e "CREATE USER IF NOT EXISTS 'root'@'%' IDENTIFIED BY '${MYSQL_ROOT_PASSWORD}';"
mysql -u root -p${MYSQL_ROOT_PASSWORD} -e "GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;"
mysql -u root -p${MYSQL_ROOT_PASSWORD} -e "FLUSH PRIVILEGES;"

# Update MySQL configuration for public access
sed -i 's/bind-address.*=.*/bind-address = 0.0.0.0/' /etc/mysql/mysql.conf.d/mysqld.cnf
systemctl restart mysql

# Configure firewall for MySQL
ufw allow 3306/tcp

# Create application directory
echo "Creating application directory..."
mkdir -p ${REMOTE_APP_DIR}
cd ${REMOTE_APP_DIR}

# Extract deployment package
echo "Extracting deployment package..."
tar -xzf /tmp/deploy.tar.gz -C ${REMOTE_APP_DIR}
chown -R www-data:www-data ${REMOTE_APP_DIR}
chmod -R 755 ${REMOTE_APP_DIR}

# Create systemd service
echo "Creating systemd service..."
cat > /etc/systemd/system/${APP_NAME}.service << EOF
[Unit]
Description=NopCommerce ${APP_NAME}
After=network.target

[Service]
WorkingDirectory=${REMOTE_APP_DIR}
ExecStart=/usr/bin/dotnet ${REMOTE_APP_DIR}/Nop.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=${APP_NAME}
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:${APP_PORT}

[Install]
WantedBy=multi-user.target
EOF

# Create Nginx configuration
echo "Creating Nginx configuration..."
cat > /etc/nginx/sites-available/${APP_NAME} << EOF
server {
    listen 80;
    server_name ${DOMAIN_NAME} www.${DOMAIN_NAME};

    location / {
        proxy_pass http://localhost:${APP_PORT};
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
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
    }
}
EOF

# Enable Nginx site
ln -sf /etc/nginx/sites-available/${APP_NAME} /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx

# Start application service
echo "Starting application service..."
systemctl daemon-reload
systemctl enable ${APP_NAME}.service
systemctl start ${APP_NAME}.service

# Wait for service to start
sleep 10

# Obtain SSL certificate
echo "Obtaining SSL certificate..."
certbot --nginx -d ${DOMAIN_NAME} -d www.${DOMAIN_NAME} --non-interactive --agree-tos --email admin@${DOMAIN_NAME} --redirect

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
echo "Application URL: https://${DOMAIN_NAME}"
echo "MySQL root password: ${MYSQL_ROOT_PASSWORD}"
echo "MySQL is accessible publicly on port 3306"
echo "========================================="

ENDSSH

# Clean up local files
echo "Cleaning up local files..."
rm -f deploy.tar.gz

echo "========================================="
echo "Deployment script completed!"
echo "========================================="
echo "Your NopCommerce site should now be available at:"
echo "https://${DOMAIN_NAME}"
echo "========================================="