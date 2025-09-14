#!/bin/bash

# NopCommerce Deployment Script for Ubuntu Server
# This script automates the deployment of NopCommerce to Ubuntu server with Nginx and MySQL

set -e

# Configuration Variables
SERVER_IP="172.236.28.69"
SERVER_USER="root"
DOMAIN="gamingpcrehab.co.uk"
MYSQL_ROOT_PASSWORD="rizwan321"
PROJECT_PATH="$(dirname "$0")"
REMOTE_APP_PATH="/var/www/gamingpcrehab"
NGINX_SITE_CONFIG="/etc/nginx/sites-available/gamingpcrehab"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting NopCommerce deployment to $SERVER_IP${NC}"

# Step 1: Build the project locally
echo -e "${YELLOW}Step 1: Building NopCommerce project...${NC}"
cd "$PROJECT_PATH/src/Presentation/Nop.Web"
dotnet publish -c Release -o ../../../publish || {
    echo -e "${RED}Failed to build the project${NC}"
    exit 1
}

# Step 2: Create deployment package
echo -e "${YELLOW}Step 2: Creating deployment package...${NC}"
cd "$PROJECT_PATH"
tar -czf nopcommerce-deploy.tar.gz -C ./publish .

# Step 3: Deploy to server
echo -e "${YELLOW}Step 3: Deploying to server...${NC}"

ssh $SERVER_USER@$SERVER_IP << 'ENDSSH'
set -e

# Update system
echo "Updating system packages..."
apt update && apt upgrade -y

# Install required packages
echo "Installing required packages..."
# Check if nginx is installed
if ! command -v nginx &> /dev/null; then
    echo "Nginx not found. Installing nginx..."
    apt install -y nginx
else
    echo "Nginx is already installed"
fi
# Install other required packages
apt install -y mysql-server certbot python3-certbot-nginx
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

# Set MySQL root password and configure for remote access
mysql -e "ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY 'rizwan321';"
mysql -u root -prizwan321 -e "CREATE USER IF NOT EXISTS 'root'@'%' IDENTIFIED BY 'rizwan321';"
mysql -u root -prizwan321 -e "GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;"
mysql -u root -prizwan321 -e "FLUSH PRIVILEGES;"

# Update MySQL configuration for remote access
sed -i 's/bind-address\s*=\s*127.0.0.1/bind-address = 0.0.0.0/' /etc/mysql/mysql.conf.d/mysqld.cnf
systemctl restart mysql

# Configure firewall for MySQL
ufw allow 3306/tcp
ufw allow 'Nginx Full'
ufw allow 'OpenSSH'
echo "y" | ufw enable

# Create application directory
echo "Creating application directory..."
mkdir -p /var/www/gamingpcrehab
chown -R www-data:www-data /var/www/gamingpcrehab

# Create NopCommerce database
echo "Creating NopCommerce database..."
mysql -u root -prizwan321 -e "CREATE DATABASE IF NOT EXISTS nopcommerce;"

ENDSSH

# Step 4: Upload and extract application
echo -e "${YELLOW}Step 4: Uploading application files...${NC}"
scp nopcommerce-deploy.tar.gz $SERVER_USER@$SERVER_IP:/tmp/
ssh $SERVER_USER@$SERVER_IP "tar -xzf /tmp/nopcommerce-deploy.tar.gz -C $REMOTE_APP_PATH && rm /tmp/nopcommerce-deploy.tar.gz"

# Step 5: Configure application and Nginx
echo -e "${YELLOW}Step 5: Configuring application and Nginx...${NC}"

ssh $SERVER_USER@$SERVER_IP << ENDSSH
set -e

# Set permissions
chown -R www-data:www-data $REMOTE_APP_PATH
chmod -R 755 $REMOTE_APP_PATH

# Create systemd service for NopCommerce
cat > /etc/systemd/system/nopcommerce.service << 'EOF'
[Unit]
Description=NopCommerce
After=network.target

[Service]
WorkingDirectory=$REMOTE_APP_PATH
ExecStart=/usr/bin/dotnet $REMOTE_APP_PATH/Nop.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=nopcommerce
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
EOF

# Replace variables in service file
sed -i "s|\$REMOTE_APP_PATH|$REMOTE_APP_PATH|g" /etc/systemd/system/nopcommerce.service

# Create Nginx configuration
cat > $NGINX_SITE_CONFIG << 'EOF'
server {
    listen 80;
    server_name $DOMAIN www.$DOMAIN;
    
    location / {
        proxy_pass http://localhost:5000;
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
    
    location ~ /\. {
        deny all;
    }
}
EOF

# Replace domain variable
sed -i "s/\$DOMAIN/$DOMAIN/g" $NGINX_SITE_CONFIG

# Enable site
ln -sf $NGINX_SITE_CONFIG /etc/nginx/sites-enabled/
nginx -t && systemctl reload nginx

# Start NopCommerce service
systemctl daemon-reload
systemctl enable nopcommerce
systemctl start nopcommerce

# Wait for service to start
sleep 10

# Configure SSL with Let's Encrypt
echo "Configuring SSL certificate..."
certbot --nginx -d $DOMAIN -d www.$DOMAIN --non-interactive --agree-tos --email admin@$DOMAIN --redirect

# Restart services
systemctl restart nginx
systemctl restart nopcommerce

echo "Deployment completed successfully!"
echo "MySQL is configured with:"
echo "  - Root password: rizwan321"
echo "  - Remote access enabled on port 3306"
echo ""
echo "Your NopCommerce site should be accessible at:"
echo "  - https://$DOMAIN"
echo "  - https://www.$DOMAIN"

ENDSSH

# Step 6: Clean up local files
echo -e "${YELLOW}Step 6: Cleaning up...${NC}"
rm -f nopcommerce-deploy.tar.gz
rm -rf ./publish

echo -e "${GREEN}Deployment completed successfully!${NC}"
echo -e "${GREEN}Your website is now live at: https://$DOMAIN${NC}"
echo -e "${YELLOW}MySQL Details:${NC}"
echo -e "  Host: $SERVER_IP"
echo -e "  Port: 3306"
echo -e "  Username: root"
echo -e "  Password: rizwan321"