#!/bin/bash
set -euo pipefail

if [ $# -lt 2 ]; then
  echo "Usage: $0 <upload_dir> <TunnelGPT_build_number>"
  exit 1
fi

application_user="tunnelgpt"
application_home="/opt/tunnelgpt"
upload_dir="$1"
build_number="$2"

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
unzip "$upload_dir/TunnelGPT_build$build_number.zip" -d $application_home
chown -R $application_user:$application_user $application_home
chmod 600 $application_home/appsettings*.json tunnelgpt-cert.*

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