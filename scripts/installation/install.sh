#!/bin/sh
bin_dir="/usr/local/bin"
set -e

echo "Installing Tiempito..."

# Create dirs
sudo mkdir -p /usr/share/tiempito
mkdir -p "$HOME/.config/tiempito"
mkdir -p "$HOME/.config/systemd/user"

# Copy config files
sudo cp -r ./config/* /usr/share/tiempito/

# Copy default user config
cp -r ./user/* "$HOME/.config/tiempito/"

# Copy user service (standardizing spaces)
cp ./tiempitod.service "$HOME/.config/systemd/user/"
systemctl --user daemon-reload

# Copy bin files
sudo cp ./bin/tp "$bin_dir/tp"
sudo cp ./bin/tiempitod "$bin_dir/tiempitod"
sudo chmod +x "$bin_dir/tp" "$bin_dir/tiempitod"

echo "Installation complete! Run 'tp' to start."
