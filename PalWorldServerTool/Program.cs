using System.Text.Json;
using LoongEgg.LoongLogger;
using PalWorldServerTool.DataModels;
using PalWorldServerTool.Models;
using Spectre.Console;

Logger.Enable(LoggerType.File, LoggerLevel.Debug);//注册Log日志函数

try
{
    Console.Title = "PalWorldServerTool";
    //PalWorldServerTool.Models.Consoles.DisableMouseInteraction();
    //Console.SetWindowSize(Console.WindowWidth, Console.WindowHeight); // 设置窗口大小与缓冲区大小一致
    //Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight); // 设置缓冲区大小与窗口一致


    ConfigDataModel configData = new ConfigDataModel();


    // 启动器获取当前目录
    string thisDirPath = AppDomain.CurrentDomain.BaseDirectory;
    string steamCMDDirPath = Path.Combine(thisDirPath, "SteamCMD");
    string palWorldServerDirPath = Path.Combine(steamCMDDirPath, "steamapps", "common", "PalServer");
    string palWorldServerSavedDirPath = Path.Combine(palWorldServerDirPath, "Saved");

    var rule = new Rule("[yellow]环境检测[/]");
    AnsiConsole.Write(rule);
    if (PalWorldServerTool.Models.Environment.CheckPathSafety(thisDirPath))
    {
        Logger.WriteWarn($"部署路径不合法: {thisDirPath}");
        AnsiConsole.MarkupLine("[gold3_1]部署路径不可包含[red]非英文[/]字符 请换个地方部署[/]");
        AnsiConsole.MarkupLine("按任意键退出程序");
        Console.ReadKey();
        System.Environment.Exit(0);
    }
    /*if (PalWorldServerTool.Models.Environment.CheckDirectX() == null)
    {
        Logger.WriteInfor("开始安装DirectX");
        Logger.WriteDebug("下载DirectX");
        await Download.DownloadFileProgressBar("https://cloud.445720.xyz/f/pJ4Tn/DirectX_Repair.zip", "Data/DirectX_Repair.zip");

        await AnsiConsole.Status()
            .StartAsync("安装DirectX", async ctx =>
            {
                Logger.WriteDebug("解压DirectX");
                ZipFile.ExtractToDirectory(Path.Combine(thisDirPath, "Data", "DirectX_Repair.zip"), Path.Combine(thisDirPath, "Data", "DirectX_Repair"));
                Logger.WriteDebug("运行DirectX安装");
                ExternalAppController externalAppController = new ExternalAppController();
                externalAppController.ProcessExited += (s, e) =>
                {
                    Logger.WriteDebug("DirectX安装完毕");
                    AnsiConsole.MarkupLine("[chartreuse2]DirectX安装完毕[/]");
                    externalAppController.Dispose();
                };
                await externalAppController.StartAppAsync(Path.Combine(thisDirPath, "Data", "DirectX_Repair", "DirectX Repair.exe"), "");

            });
    }
    if (!PalWorldServerTool.Models.Environment.CheckVC2015x86())
    {
        Logger.WriteInfor("开始安装VC C++ 2015 x86");
        Logger.WriteDebug("下载VC C++ 2015 x86");
        await Download.DownloadFileProgressBar("https://cloud.445720.xyz/f/yJDsr/VC_redist.x86.exe", "Data/VC_redist.x86.exe");

        await AnsiConsole.Status()
            .StartAsync("安装VC C++ 2015 x86", async ctx =>
            {
                Logger.WriteDebug("运行VC C++ 2015 x86安装");
                ExternalAppController externalAppController = new ExternalAppController();
                externalAppController.ProcessExited += (s, e) =>
                {
                    Logger.WriteDebug("VC C++ 2015 x86安装完毕");
                    AnsiConsole.MarkupLine("[chartreuse2]VC C++ 2015 x86安装完毕[/]");
                    externalAppController.Dispose();
                };
                await externalAppController.StartAppAsync(Path.Combine(thisDirPath, "Data", "VC_redist.x86.exe"), "/install /passive");
            });
    }*/
    if (!PalWorldServerTool.Models.Environment.CheckVC2015x64())
    {
        Logger.WriteInfor("开始安装VC C++ 2015 x64");
        Logger.WriteDebug("下载VC C++ 2015 x64");
        await Download.DownloadFileProgressBar("https://cloud.445720.xyz/f/VdPhm/VC_redist.x64.exe", "Data/VC_redist.x64.exe");

        await AnsiConsole.Status()
            .StartAsync("安装VC C++ 2015 x64", async ctx =>
            {
                Logger.WriteDebug("运行VC C++ 2015 x64安装");
                ExternalAppController externalAppController = new ExternalAppController();
                externalAppController.ProcessExited += (s, e) =>
                {
                    Logger.WriteDebug("VC C++ 2015 x64安装完毕");
                    AnsiConsole.MarkupLine("[chartreuse2]VC C++ 2015 x64安装完毕[/]");
                    externalAppController.Dispose();
                };
                await externalAppController.StartAppAsync(Path.Combine(thisDirPath, "Data", "VC_redist.x64.exe"), "/install /passive");
            });
    }
    Console.Clear();


    if (!File.Exists("Config.json"))
    {
        rule = new Rule("[yellow]PalWorldTool配置[/]");
        rule.Justification = Justify.Left;
        AnsiConsole.Write(rule);

        if (AnsiConsole.Confirm("是否想使用高级功能?", false))
        {
            configData.RCON = AnsiConsole.Confirm("是否想使用RCON?", false);
            //configData.AdminPassword = AnsiConsole.Prompt(
            //    new TextPrompt<string>("设置管理员密码")
            //    .Validate(pwd =>
            //        {
            //            return !string.IsNullOrWhiteSpace(pwd);
            //        })
            //    );
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
            configData.CloseWaitingTime = AnsiConsole.Prompt(
                new TextPrompt<int>("关闭服务器等待时间(单位秒)推荐值20秒 RCON模式下有效")
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
        //configData.AdminPassword ??= Consoles.RandomString(8).Standardize();
        File.WriteAllText("Config.json", JsonSerializer.Serialize(configData, jsonTypeInfo: MyJsonContext.Default.ConfigDataModel));
    }
    string fileContents = File.ReadAllText("Config.json");
    configData = JsonSerializer.Deserialize<ConfigDataModel>(fileContents, MyJsonContext.Default.ConfigDataModel)!;
    Console.Clear();

    PalWorldServerTool.Models.SteamCMD steamCMD = new();
    PalWorldServerTool.Models.PalWorldServer palWorldServer = new(palWorldServerDirPath, steamCMD, configData);
    Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPressHandler);
    await palWorldServer.Initialization();
    while (true)
    {
        await Task.Delay(5000);
        //await palWorldServer.SendCommand("Broadcast 6");
    }

    void CancelKeyPressHandler(object? sender, ConsoleCancelEventArgs args)
    {
        Logger.WriteInfor("正在关闭服务器");
        args.Cancel = true;

        palWorldServer.Shutdown().Wait();
        Logger.WriteInfor("服务器已关闭");
        Logger.Disable();
        System.Environment.Exit(0);
    }
}
catch (Exception ex)
{
    Logger.WriteError($"运行时发生错误 因为:{ex.Message}");
    AnsiConsole.MarkupInterpolated($"[red]运行时发生错误 因为:{ex.Message}[/]");
    Console.ReadKey();
}