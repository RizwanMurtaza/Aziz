#!/bin/bash

# Fix NopCommerce deployment issues

SERVER_IP="172.236.28.69"
SERVER_USER="root"

echo "Fixing deployment issues on server..."

ssh $SERVER_USER@$SERVER_IP << 'ENDSSH'
set -e

echo "1. Checking application files..."
ls -la /var/www/gamingpcrehab/ | head -20

echo "2. Installing .NET 9.0 runtime..."
# Remove any existing Microsoft package repository
rm -f /etc/apt/sources.list.d/microsoft-prod.list
# Add Microsoft package repository for .NET 9.0
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
# Update and install .NET 9.0
apt-get update
# Check available packages
apt-cache search dotnet | grep -i runtime | grep -E "9\.0|9-"
# Try different package naming conventions
apt-get install -y dotnet-sdk-9.0 || apt-get install -y dotnet-9.0 || apt-get install -y dotnet-host-9.0 dotnet-hostfxr-9.0 dotnet-runtime-9.0 aspnetcore-runtime-9.0
echo "Installed .NET versions:"
dotnet --list-runtimes

echo "3. Creating appsettings.json if missing..."
if [ ! -f /var/www/gamingpcrehab/appsettings.json ]; then
    cat > /var/www/gamingpcrehab/appsettings.json << 'EOF'
{
  "Hosting": {
    "UseHttpClusterHttps": false,
    "UseHttpXForwardedProto": false,
    "ForwardedHttpHeader": ""
  },
  "Nop": {
    "DisplayFullErrorStack": false,
    "UserAgentStringsPath": "~/App_Data/browscap.xml",
    "StaticFilesCacheControl": "public,max-age=604800",
    "SupportPreviousNopcommerceVersions": true,
    "PluginStaticFileExtensionsBlacklist": "",
    "UseAutofac": false
  }
}
EOF
fi

echo "4. Creating dataSettings.json for MySQL connection..."
mkdir -p /var/www/gamingpcrehab/App_Data
cat > /var/www/gamingpcrehab/App_Data/dataSettings.json << 'EOF'
{
  "DataProvider": "mysql",
  "ConnectionString": "server=localhost;uid=root;pwd=rizwan321;database=nopcommerce;allowuservariables=True;persistsecurityinfo=True",
  "SQLCommandTimeout": null,
  "WithNoLock": false
}
EOF

echo "5. Setting correct permissions..."
chown -R www-data:www-data /var/www/gamingpcrehab
chmod -R 755 /var/www/gamingpcrehab
chmod -R 777 /var/www/gamingpcrehab/App_Data

echo "6. Installing MySQL connector for .NET..."
cd /var/www/gamingpcrehab
if [ ! -f MySql.Data.dll ]; then
    echo "MySQL connector might be missing"
fi

echo "7. Checking required directories..."
mkdir -p /var/www/gamingpcrehab/wwwroot
mkdir -p /var/www/gamingpcrehab/Plugins
mkdir -p /var/www/gamingpcrehab/App_Data

echo "8. Testing application manually..."
cd /var/www/gamingpcrehab
sudo -u www-data dotnet Nop.Web.dll --urls "http://localhost:5001" &
DOTNET_PID=$!
sleep 10
kill $DOTNET_PID 2>/dev/null || true

echo "9. Restarting service..."
systemctl daemon-reload
systemctl restart gamingpcrehab

echo "10. Checking service status..."
sleep 5
systemctl status gamingpcrehab

echo "11. Checking logs..."
journalctl -u gamingpcrehab -n 50 --no-pager

ENDSSH