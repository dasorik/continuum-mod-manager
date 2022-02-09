# 🎮 Continuum Mod Manager

Continuum Mod Manager is a game agnostic mod manager and modification engine that allowings users to install/uninstall mods that either they or other users have authored. Built from the ground up to allow for modding support of any game, Continuum's aims is to allow modders to easily share their creations with others.

## 📋 Features
* **Mod With Ease**
  * Easily organize, configure and install mods with ease, all from the one place.
* **Game Integrations**
  * Extensibility is at the core of Continuum. Through community based game integraions, Continuum can be made to support any game.
* **Mod Collision Tracking**
  * Continuum tracks all file modifications made and will attempt to resolve any conflicts that is comes across. Due to the unique way the installation process is handled (which doesn't run mod installation steps in sequence) Continuum can gracfully resolve conflicts where other mod installers would fail.
* **Modificiation Engine** (Continuum.Core)
  * In addition to the graphical mod manager interface, Continuum also features a rich file modiication engine. *Continuum.Core* is a standalone modification library which acts as the backbone for Continuum's mod installation. Continuum.Core can be integrated into any .NET application, allowing for other applications to make use of it's features.

## 📦 Download
You can download the latest releases automatically via the installer on the [Releases](https://github.com/dasorik/continuum-mod-manager/releases) page.

## 💬 Let's Chat
Want to chat, or need some help? Feel free to join us over on the [Continuum Mod Manager Discord](https://discord.gg/VbSBYYRA5y). From here you'll also be able to discover mods and integrations lovingly handcrafted by members of the community.

## ⚙️ Making Mods / Integrations
Want to get started making mods with Continuum? Or perhaps you'd like to add support to Continuum for your favourite game!
* [Creating Mods](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/CreatingMods.md)
* [Creating Game Integrations](https://github.com/dasorik/continuum-mod-manager/blob/main/Wiki/CreatingIntegrations.md)

## 🙋‍♀️🙋‍♂ Contributing
Please feel free to submit a pull request if you find any bugs (A list of the active isues can be located [here](https://github.com/dasorik/continuum-mod-manager/issues)).
Continuum is also a growing tool, and if you'd like to contibute to it's ongoing development please drop us a message over on the [Continuum Mod Manager Discord](https://discord.gg/VbSBYYRA5y).

## 👷‍♂️ Building
This tool runs in Blazor via [Electron.NET](https://github.com/ElectronNET/Electron.NET), this requires the Electron CLI to installed if you are wanting to make local builds of the application.
```
dotnet tool install ElectronNET.CLI -g
```
For development builds run:
```
cd Continuum.GUI
electronize start
````
## 📑 License
Continuum Mod Manager is licensed under the [GNU General Public License v3.0](https://github.com/dasorik/continuum-mod-manager/blob/main/LICENSE).

> As an aside to this (but not covered in the license above) if you choose to include Continuum.Core in your application we would kindly request that you include the text *'Powered By Continuum'* accompanied by the Continuum logo somewhere in your app.<br><br>Please also drop us a message to let us know about your projects incorperating Continuum, we'd love to hear about them!

## 📜 Additional Credits
This mod tool uses external tools, with attributions below:

[QuickBMS](https://aluigi.altervista.org/quickbms.htm) - Released under [GPL-2.0](http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt).

[Unluac (.Net port)](https://github.com/HansWessels/unluac) - Released under the following [license](https://github.com/dasorik/infinity-mod-tool/blob/master/InfinityModTool/Lib/UnluacNet/UnluacNet-LICENSE.txt).
