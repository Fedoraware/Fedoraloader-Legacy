# Fedoraloader
Fedoraloader is a simple loader for [Fedoraware](https://github.com/tf2cheater2013/Fedoraware) that automatically downloads and injects the latest Fedoraware build.
It was created so you don't have to fiddle around with injectors, GitHub and VAC Bypasses.

*Please note that this program was designed for Windows 10 and higher and might not work on older OS versions!*

<p align="center">
  <img src="https://i.imgur.com/IjSdW29.png" />
</p>
  
## Download
You can download the latest Fedoraloader build from the [releases](https://github.com/lnx00/Fedoraloader/releases/latest/) or compile the source yourself.

## Usage
- Run Fedoraloader as an administrator
- Enable or disable VAC Bypass according to your preferences
- Click the **LOAD** button

## How it works
Fedoraloader downloads the latest Fedoraware artifact using [nightly.link](https://nightly.link/) and extracts it in your TEMP-Folder.
Then the .dll file is injected into TF2 using an internal LoadLibrary injector.

Optionally, you can let the loader create exceptions for Microsoft Defender so it won't block the Fedoraware DLL or enable [Daniel Krupiński's VAC Bypass](https://github.com/danielkrupinski/VAC-Bypass) that reduces the chance of getting VAC banned.

## Can't run Fedoraloader?
<p align="center">
  <img src="https://i.imgur.com/OtZwqIr.png" />
</p>
If you get this error message, then you've most likely tried running Fedoraloader on Windows 7 and below. Please note that Windows 7 is not supported anymore and you should switch to Windows 10. If you really want to keep using Windows 7, try using <a href="https://github.com/DarthTon/Xenos">Xenos</a> or a similar injector. <a href="https://www.youtube.com/watch?v=PT3kVA053IY">(Xenos Tutorial)</a>

## Issues with Fedoraware?
Post all issues and problems regarding Fedoraware into the [Fedoraware Issues section](https://github.com/tf2cheater2013/Fedoraware/issues) on the GitHub repository.
This repository is only for the Fedoraloader that loads and injects Fedoraware!

## Credits
- [lnx00](https://github.com/lnx00)
- [Daniel Krupiński](https://github.com/danielkrupinski) (VAC Bypass)
- [SP1K3](https://www.unknowncheats.me/forum/members/954168.html) (LoadLibrary injector)
