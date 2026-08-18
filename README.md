
# Desktop Gremlin - Avalonia Port
<img width="925" height="436" alt="image" src="https://github.com/user-attachments/assets/7f3f1631-b2d2-4b8e-adc4-b287dd81d784" />
<br>

[![Ko-fi](https://img.shields.io/badge/support_me_on_ko--fi-F16061?style=for-the-badge&logo=kofi&logoColor=f5f5f5)](https://ko-fi.com/dgdev91)

Forked from [KurtValesco's Desktop Gremlin](https://github.com/KurtVelasco/Desktop_Gremlin), this version uses Avalonia instead WPF and LibVLC for audio, so it can run also on Linux and Mac

A fun little .Net application that displays an animated interactive character on your desktop.

# Android
<p align="center"><img width="316" height="580" alt="immagine" src="https://github.com/user-attachments/assets/cbcad66b-3566-40c2-9d80-4ea7e7a18c22" /></p>

Since version 4.3.1, the project also supports Android, you can find the apk in [Releases](https://github.com/DGdev91/Desktop_Gremlin_Avalonia/releases)

# Linux
This project uses LibVLC to handle sounds. While on Windows and Mac it's already included, on Linux you need to have libVLC-dev installed on your system, if not already installed.

Ubuntu
```
sudo apt install libvlc-dev
```
Arch
```
sudo pacman -S libvlc
```
Fedora
```
dnf install vlc-libs
```
# Characters
This project has initially been forked from Desktop Gremlin 2.8 and contains the sprites and sounds for MatikaneTannhäuser (Mambo) from UmaMusume, more characters can be found on [the original projects's page](https://github.com/KurtVelasco/Desktop_Gremlin).
You should be able to use sprites and sounds from a different character, as long as they follow the same format (old characters will likely not work)


