#!/bin/sh
bin_dir="/usr/local/bin"

echo "Uninstalling Tiempito..."

# Stop services
systemctl --user stop tiempitod.service 2>/dev/null || true
systemctl --user disable tiempitod.service 2>/dev/null || true

# Remove bin files
sudo rm -f "$bin_dir/tp"
sudo rm -f "$bin_dir/tiempitod"

# Remove user service
rm -f "$HOME/.config/systemd/user/tiempitod.service"
systemctl --user daemon-reload

# Remove global config files
sudo rm -rf /usr/share/tiempito

# Remove user config files
read -p "Do you want to remove user configuration files? (y/N): " removeConfig
if [ "$removeConfig" = "y" ] || [ "$removeConfig" = "Y" ]; then
    rm -rf "$HOME/.config/tiempito"
    echo "User configuration files removed."
else
    echo "User configuration files retained."
fi

echo "Uninstallation complete."
