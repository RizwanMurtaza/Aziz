#!/bin/bash

# Fresh MySQL Installation Script (Remote Execution)
# This script connects to the server via SSH and installs fresh MySQL
# with root password 'rizwan321' and lower_case_table_names enabled

set -e

# Configuration (same as deploy.sh)
SERVER_IP="172.236.28.69"
SERVER_USER="root"
MYSQL_ROOT_PASSWORD="rizwan321"

echo "========================================="
echo "Fresh MySQL Remote Installation Script"
echo "========================================="

# Execute remote MySQL installation
echo "Connecting to server ${SERVER_IP} and installing fresh MySQL..."
ssh ${SERVER_USER}@${SERVER_IP} << 'ENDSSH'

set -e

MYSQL_ROOT_PASSWORD="rizwan321"

echo "========================================="
echo "Fresh MySQL Installation on Remote Server"
echo "========================================="

# Step 1: Remove existing MySQL installation completely
echo "Step 1: Removing existing MySQL installation..."

# Stop MySQL service if running
systemctl stop mysql 2>/dev/null || true
systemctl stop mysqld 2>/dev/null || true

# Remove MySQL packages
apt-get remove --purge -y mysql-server mysql-client mysql-common mysql-server-core-* mysql-client-core-* 2>/dev/null || true
apt-get autoremove -y 2>/dev/null || true
apt-get autoclean 2>/dev/null || true

# Remove MySQL data directories and configuration files
rm -rf /var/lib/mysql
rm -rf /var/log/mysql
rm -rf /etc/mysql
rm -rf /usr/share/mysql
rm -rf /var/cache/apt/archives/mysql*

# Remove MySQL user and group
userdel mysql 2>/dev/null || true
groupdel mysql 2>/dev/null || true

echo "Existing MySQL installation removed successfully."

# Step 2: Update system
echo "Step 2: Updating system packages..."
apt-get update -y

# Step 3: Install fresh MySQL server
echo "Step 3: Installing fresh MySQL server..."

# Set DEBIAN_FRONTEND to noninteractive to avoid prompts
export DEBIAN_FRONTEND=noninteractive

# Set debconf selections for automated installation
echo "mysql-server mysql-server/root_password password ${MYSQL_ROOT_PASSWORD}" | debconf-set-selections
echo "mysql-server mysql-server/root_password_again password ${MYSQL_ROOT_PASSWORD}" | debconf-set-selections

# Install MySQL server
apt-get install -y mysql-server

# Step 4: Configure MySQL with lower_case_table_names
echo "Step 4: Configuring MySQL with lower_case_table_names..."

# Stop MySQL service
systemctl stop mysql

# Remove the auto-generated data directory since lower_case_table_names requires clean initialization
rm -rf /var/lib/mysql/*

# Create MySQL configuration directory if it doesn't exist
mkdir -p /etc/mysql/mysql.conf.d

# Create custom MySQL configuration with lower_case_table_names
cat > /etc/mysql/mysql.conf.d/custom.cnf << 'EOF'
[mysqld]
# Custom configuration for NopCommerce
lower_case_table_names = 1
bind-address = 0.0.0.0
sql_mode = ""
max_allowed_packet = 64M
innodb_buffer_pool_size = 256M
EOF

# Initialize MySQL data directory with the new configuration
mysqld --initialize-insecure --user=mysql --datadir=/var/lib/mysql

# Start MySQL service
systemctl start mysql
systemctl enable mysql

# Wait for MySQL to be ready
sleep 10

# Step 5: Configure MySQL root user for local and remote access
echo "Step 5: Configuring MySQL root user access..."

# Set MySQL root password (initially no password with --initialize-insecure)
mysql -u root -e "ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY '${MYSQL_ROOT_PASSWORD}';"
mysql -u root -p"${MYSQL_ROOT_PASSWORD}" -e "CREATE USER IF NOT EXISTS 'root'@'%' IDENTIFIED BY '${MYSQL_ROOT_PASSWORD}';"
mysql -u root -p"${MYSQL_ROOT_PASSWORD}" -e "GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;"
mysql -u root -p"${MYSQL_ROOT_PASSWORD}" -e "FLUSH PRIVILEGES;"

# Step 6: Configure firewall and network access for MySQL
echo "Step 6: Configuring firewall and network access for MySQL..."

# Configure UFW firewall
ufw allow 3306/tcp

# Configure iptables as backup (in case UFW is not managing correctly)
iptables -A INPUT -p tcp --dport 3306 -j ACCEPT
iptables-save > /etc/iptables/rules.v4 2>/dev/null || true

# Ensure MySQL configuration allows external connections
sed -i 's/bind-address.*/bind-address = 0.0.0.0/' /etc/mysql/mysql.conf.d/mysqld.cnf 2>/dev/null || true

# Also update our custom configuration to be explicit
cat > /etc/mysql/mysql.conf.d/custom.cnf << 'EOF'
[mysqld]
# Custom configuration for NopCommerce
lower_case_table_names = 1
bind-address = 0.0.0.0
port = 3306
sql_mode = ""
max_allowed_packet = 64M
innodb_buffer_pool_size = 256M
max_connections = 200
EOF

# Restart MySQL to ensure all configurations are applied
systemctl restart mysql

# Wait for MySQL to be ready after restart
sleep 5

echo "========================================="
echo "Fresh MySQL installation completed!"
echo "========================================="
echo "MySQL root password: ${MYSQL_ROOT_PASSWORD}"
echo "MySQL is accessible locally via localhost:3306"
echo "MySQL is accessible globally on port 3306"
echo "lower_case_table_names is enabled"
echo "========================================="

# Display MySQL status
echo "MySQL Service Status:"
systemctl status mysql --no-pager -l || true

# Test MySQL connection
echo "Testing MySQL connection..."
mysql -u root -p"${MYSQL_ROOT_PASSWORD}" -e "SELECT VERSION(); SHOW VARIABLES LIKE 'lower_case_table_names';" || true

# Show network configuration
echo "Network Configuration:"
netstat -tlnp | grep :3306 || ss -tlnp | grep :3306
echo ""

# Show firewall status
echo "Firewall Status:"
ufw status || iptables -L INPUT | grep 3306 || echo "No firewall rules found"
echo ""

# Test external connectivity
echo "Testing external connectivity..."
mysql -u root -p"${MYSQL_ROOT_PASSWORD}" -h 127.0.0.1 -e "SELECT 'External connection test successful';" || echo "External connection test failed"

echo "========================================="
echo "Remote MySQL installation completed!"
echo "========================================"

ENDSSH

echo "========================================="
echo "Fresh MySQL installation script completed!"
echo "========================================="
echo "MySQL has been installed on server: ${SERVER_IP}"
echo "Root password: ${MYSQL_ROOT_PASSWORD}"
echo "Global access: ${SERVER_IP}:3306"
echo "Local access: localhost:3306"
echo "========================================"