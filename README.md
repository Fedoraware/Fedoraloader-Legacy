# Fedoraloader
Fedoraloader is a simple loader for Fedoraware that automatically downloads and injects the latest Fedoraware build.
It was created so you don't have to fiddle around with injectors, GitHub and VAC Bypasses.

*Please note that this program was designed for Windows 10 and higher and might not work on older OS versions!*

<p align="center">
  <img src="https://i.imgur.com/DQZDrI4.png" />
</p>
  
## Download
You can download the latest Fedoraloader build from the [releases](https://github.com/lnx00/Fedoraloader/releases/latest/) or compile the source yourself.

## Usage
- Run Fedoraloader as an administrator
- Enable or disable VAC Bypass according to your preferences
- Click the **LOAD** button

## How it works
This program downloads the latest Fedoraware artifact using [nightly.link](nightly.link) and extracts it in your TEMP-Folder.
Then the .dll file is injected into TF2 using an internal LoadLibrary injector.

Optionally, you can let the loader create exceptions for Microsoft Defender so it won't block the Fedoraware DLL or enable Daniel Krupiński's VAC Bypass that reduces the chance of getting VAC banned.

## Credits
- [lnx00](https://github.com/lnx00)
- [Daniel Krupiński](https://github.com/danielkrupinski) (VAC Bypass)
- [SP1K3](https://www.unknowncheats.me/forum/members/954168.html) (LoadLibrary injector)
