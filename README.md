# Fedoraloader
Fedoraloader is a simple loader for Fedoraware that automatically downloads and injects the latest Fedoraware build.
It was created so you don't have to fiddle around with injectors and GitHub.

## Usage
- Run Fedoraloader as an administrator
- Click the **LOAD** button

## How it works
This program downloads the latest Fedoraware artifact using [nightly.link](nightly.link) and extracts it in your TEMP-Folder.
Then the .dll file is injected into TF2 using a LoadLibrary injector.

## Credits
- [lnx00](https://github.com/lnx00)
- [SP1K3](https://www.unknowncheats.me/forum/members/954168.html) (LoadLibrary injector)
