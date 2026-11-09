# Auto Backup Scheduler v2

A VB.NET Windows Forms app that zips up to three directories (**Master Control**, **CrowdGate** and
**Alarm**) into timestamped backups, keeps the newest 10 of each, and uploads them to a Nextcloud server.
Leave a directory blank to skip it. All three share one backup storage folder.

## Open / build
Open `AutoBackupScheduler.slnx` in Visual Studio and press F5. Requires the .NET 10 SDK.

## Features
1. **Manual backup**: the *Back Up Now* button (also in the tray icon's right-click menu).
2. **Backup on change**: watches the folder and backs up after it has been quiet for N seconds,
   so a burst of edits produces one backup.
3. **Scheduled backup**: weekly (day + time) or monthly (day of month + time). If the PC was off
   at the scheduled time, the backup runs the next time the app starts.

Manual and scheduled backups back up every directory that is filled in; a backup on change only
backs up the directory that changed.

Backups are named `Backup_<ComputerName>_<Directory>_yyyy-MM-dd_HH-mm-ss.zip`, where `<Directory>` is
`MasterControl`, `CrowdGate` or `Alarm`. Only the newest 10 of each directory are kept, both locally and
on Nextcloud, and the oldest are deleted first. Because the computer name
is part of the file name, each computer only cleans up its own backups, even if several locations
upload to the same Nextcloud folder.

## Installing at several locations
- Set up each computer separately. The Nextcloud server URL, username and password are encrypted for
  the Windows account and computer they were entered on, so they can't be copied to another PC. If
  settings are copied anyway, or an administrator resets the Windows password, the app warns you to
  re-enter them.
- Recommended: give each location its own remote folder (e.g. `Backups/NorthFarm`) and its own
  Nextcloud app password.

## Nextcloud setup
- **Server URL**: e.g. `https://cloud.myhome.lan` (or `http://192.168.1.10`).
- **Username**: your Nextcloud user ID. If you log in with an email address, find your user ID in
  Nextcloud under *Settings > Files > WebDAV* (it's the part after `/dav/files/`).
- **App password**: create one in Nextcloud under *Settings > Security > Devices & sessions*.
- The server URL, username and app password are hidden on screen, stored encrypted with Windows
  DPAPI, and removed from error messages in the log.
- **Remote folder**: always inside the `Work stuff/CCW/Backups` folder on the server (e.g. `PC` means
  `Work stuff/CCW/Backups/PC`; leave it empty to upload straight into `Work stuff/CCW/Backups`).
  Click *Browse...* to pick a folder or create new folders inside `Work stuff/CCW/Backups`.
  Typed folders are created automatically.
- Turn on *Allow self-signed certificate* if your home server uses a self-signed HTTPS certificate.

If an upload fails, the local backup is kept, and missing backups are uploaded after the next successful backup.

Settings and `backup.log` are stored in `%AppData%\AutoBackupSchedulerV2`. On first run, the
Nextcloud, schedule and backup storage settings are copied from version 1 (`%AppData%\AutoBackupScheduler`);
the three directories need to be chosen again.
