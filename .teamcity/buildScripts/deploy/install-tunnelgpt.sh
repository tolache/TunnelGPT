#!/bin/bash
set -euo pipefail

if [ $# -lt 1 ]; then
  echo "Usage: $0 <TunnelGPT_build_number>"
  exit 1
fi

application_user="tunnelgpt"
application_home="/opt/tunnelgpt"
install_script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
build_number="$1"

# Uninstall application
if systemctl list-unit-files | grep '^tunnelgpt.service' &>/dev/null; then
  systemctl stop tunnelgpt
fi
if [ -d $application_home ]; then
  rm -rf $application_home
fi

# Initialize user
if ! id "$application_user" &>/dev/null; then
  useradd -m -s /bin/bash "$application_user"
fi

# Initialize application home
unzip "$install_script_dir/TunnelGPT_build$build_number.zip" -d $application_home
chown -R $application_user:$application_user $application_home
chmod 600 $application_home/appsettings*.json $application_home/tunnelgpt-cert.*

# Allow dotnet to bind to well-known ports (required if environment is Production and application user is non-root)
setcap CAP_NET_BIND_SERVICE=+eip "$(readlink -f /usr/bin/dotnet)"

# Add firewall rules
for port in 80 443; do
  if iptables -C INPUT -m state --state NEW -p tcp --dport $port -j ACCEPT 2>/dev/null; then
    echo "A rule allowing port $port already exists. No changes made.";
  else
    echo "No rule found for port $port. Adding rule...";
    iptables -I INPUT 6 -m state --state NEW -p tcp --dport $port -j ACCEPT
    netfilter-persistent save
  fi
done

# Register service
tee /etc/systemd/system/tunnelgpt.service > /dev/null <<EOF
[Unit]
Description=TunnelGPT Telegram Bot
After=network.target

[Service]
Type=simple
User=$application_user
WorkingDirectory=$application_home
ExecStart=/usr/bin/dotnet $application_home/TunnelGPT.dll
Restart=always
RestartSec=30

[Install]
WantedBy=multi-user.target
EOF

# Start service
systemctl daemon-reload
systemctl enable tunnelgpt
systemctl start tunnelgpt