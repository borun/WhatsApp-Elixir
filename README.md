# WhatsApp-Elixir

## What
This project is basically focused on viewing and searching the WhatsApp backup taken by **iTunes** in a desktop environment. The project and its idea is not mine at all, I just made this project usable to me and giving it back to the community in case anyone find it more useful than the original one.

## Why
I think we all know why this project is necessary! Applications like WhatsApp and Viber DO NOT provide any mechanism (at the time of this writing) to transfer data/history when someone change their OS (from iOS to Android or vice versa). Besides, I couldn't find any trustworthy third party to transfer this for me. So I just kept my iTunes backup in my desktop. Tune this App so that I can view my whatsapp messages, images and search them. Then I shifted to Android again!

## How to

First, you need to backup your iPhone data using iTunes. 

* Under Windows Vista, Windows 7, 8 and Windows 10 iTunes will store backups in  Users\\[USERNAME]\AppData\Roaming\Apple Computer\MobileSync\Backup.
* The Microsoft Store version of iTunes stores its backups in  Users\\[USERNAME]\Apple\MobileSync\Backup.
* Under Windows XP, iTunes will store backups in Documents and Settings\\[USERNAME]\Application Data\Apple Computer\MobileSync\Backup.

Inside that backup directory there will be a alphanumeric named directory. You need to select that directory from the application. To process all the messages you may need several minutes to wait.  

## Major Changes
* It was basically for both Android and iOS. I made it only for iOS.
* I have included images in message viewer
* Search has been improved a lot
* Everything is loading in memory for faster processing
* A installer project added

## Disclaimer
All credit goes to the author of [this project](https://github.com/impersoft/Securcube-Whatsapp-Viewer). For me, it was just a fast tweaking of a C# project so that I can shift from my old iPhone to a new Android! And I am doing a C# project almost after a decade!

## Links
You will find the original project [here](https://github.com/impersoft/Securcube-Whatsapp-Viewer).
If you want to go further and implement it for Android, you may check [these](https://github.com/EliteAndroidApps?tab=repositories) projects out.
