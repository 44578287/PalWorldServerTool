using System.Reflection;
using Config;
using LoongEgg.LoongLogger;
using PalWorldServerTool.DataModels;
using Spectre.Console;

Logger.Enable(LoggerType.File, LoggerLevel.Debug);//注册Log日志函数
//Console.OutputEncoding = Encoding.UTF8;

try
{
    ConfigHelper config = new(Assembly.GetExecutingAssembly());
    ConfigDataModel configData = new ConfigDataModel();

    // 启动器获取当前目录
    string thisDirPath = AppDomain.CurrentDomain.BaseDirectory;
    string steamCMDDirPath = Path.Combine(thisDirPath, "SteamCMD");
    string palWorldServerDirPath = Path.Combine(steamCMDDirPath, "steamapps", "common", "PalServer");
    string palWorldServerSavedDirPath = Path.Combine(palWorldServerDirPath, "Saved");

    if (!File.Exists("Config.json"))
    {
        var rule = new Rule("[yellow]PalWorldTool配置[/]");
        rule.Justification = Justify.Left;
        AnsiConsole.Write(rule);

        if (AnsiConsole.Confirm("是否想使用高级功能?", false))
        {
            configData.AutoServerLive = AnsiConsole.Confirm("是否启用服务器保活?", false);
            configData.TimedRebootServer = AnsiConsole.Prompt(
                new TextPrompt<int>("是否要定时重启服务器?(单位分钟 0为关闭)")
                    .Validate(time =>
                    {
                        if (time <= 0)
                            time = 0;
                        return true;
                    })
                );
            configData.TimedBackupSaved = AnsiConsole.Prompt(
                new TextPrompt<int>("是否要定时备份存档?(单位分钟 0为关闭)")
                    .Validate(time =>
                    {
                        if (time <= 0)
                            time = 0;
                        return true;
                    })
                );
            configData.ShutdownBackupSaved = AnsiConsole.Confirm("是否要在服务器关闭时备份存档?", false);
            if (configData.ShutdownBackupSaved || configData.TimedBackupSaved > 0)
            {
                if (AnsiConsole.Confirm("是否要更改备份存档位置?", false))
                    configData.BackupSavedPath = AnsiConsole.Prompt(
                        new TextPrompt<string>("请输入备份存档的完整路径")
                        .ValidationErrorMessage("[red]此路径不存在[/]")
                        .Validate(path =>
                        {
                            return Directory.Exists(path);
                        })
                        );
            }

        }
        config["Tool", "Data"] = configData;
        await config.FileSaveAsync();
    }
    await config.FileLondAsync();
    configData = config["Tool", "Data"]!;

    PalWorldServerTool.Models.SteamCMD steamCMD = new();
    PalWorldServerTool.Models.PalWorldServer palWorldServer = new(palWorldServerDirPath, steamCMD, configData);
    Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPressHandler);
    await palWorldServer.Initialization();
    while (true)
    {
        await Task.Delay(1000);
    }

    void CancelKeyPressHandler(object? sender, ConsoleCancelEventArgs args)
    {
        args.Cancel = true;

        palWorldServer.Shutdown().Wait();
        Environment.Exit(0);
    }
}
catch (Exception ex)
{
    Logger.WriteError($"运行时发生错误 因为:{ex.Message}");
    AnsiConsole.MarkupInterpolated($"[red]运行时发生错误 因为:{ex.Message}[/]");
    Console.ReadKey();
}