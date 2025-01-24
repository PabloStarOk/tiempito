<img src="./assets/icon/icon.svg" alt="A clock icon with a gradient color" width="100" />

# Tiempito

"Tiempito" is a customizable timer designed to help you focus on work or study tasks while taking regular breaks to boost productivity. It's built with .NET and uses a daemon to run and a CLI for user interaction.

## Features

- A daemon/background service that does the main functionality of the app.
- A CLI to configure the app.
- Focus times.
- Break times.
- Customizable session profile files to save or load user preferences of its focus sessions.

## Technologies Used

- .NET Core.
- C#.
- A Worker service project for the daemon/background service.
- A Console application project for the CLI.

## Target OS

- Linux.
- Windows.

## Install/Uninstall

### Linux

Go to releases and download the linux-x64 zip file.

Decompress it in a directory and in the terminal execute the bash script.

``` bash
# To install
./install.sh

# To uninstall
./uninstall.sh
```

## Acknowledgements

- **Why a daemon if it's so simple?** Because this project is part of my learning process to understand the basics of daemons in linux and background services and how they can be used with CLIs.

## License

This project is open-source and available under the the **[MIT License](/LICENSE.md)**.
