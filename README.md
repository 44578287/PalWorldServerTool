# 幻兽帕鲁开服器

## 主要特性

- **支持一键开服**
- **自动检测环境缺失文件并补齐**
- **自动安装PalWorldServer**

## 附加功能

在首次启动并安装完成服务器后，会提供一些可选的附加功能：

1. `AutoServerLive` (bool): **服务器保活** - 当服务器意外终止时支持自动重启。
2. `TimedRebootServer` (int): **支持定时重启服务器**。
3. `TimedBackupSaved` (int): **支持定时备份服务器存档**。
4. `ShutdownBackupSaved` (bool): **支持关闭服务器时备份存档**。
5. `BackupSavedPath` (string): **支持自定备份存档位置**。

## 配置文件

- `Config.json` 是配置文件，其位置与开服器目录相同。
- 更改配置后需重启开服器以使更改生效。

## 注意事项

- 关闭服务器时建议使用 `Ctrl+C` 来关闭服务器，以防服务器进程无法被正常关闭。

## 照片
![image](https://github.com/44578287/PalWorldServerTool/assets/49640015/b610f5ec-2809-42ae-8767-dbeaa5be7d01)
![image](https://github.com/44578287/PalWorldServerTool/assets/49640015/ac0c06fc-aec7-4d8e-b9ce-17e4d72593f6)
