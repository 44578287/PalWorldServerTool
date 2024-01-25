using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Downloader;
using Spectre.Console;

namespace PalWorldServerTool.Models
{
    /// <summary>
    /// 下载相关
    /// </summary>
    public class Download
    {
        /// <summary>
        /// 下载配置
        /// </summary>
        private static DownloadConfiguration DownloadOpt = new()
        {
            // 通常，主机支持的最大字节数为8000，默认值为8000
            BufferBlockSize = 10240,  // (缓冲块大小，用于优化下载性能)

            // 要下载的文件部分数，默认值为1
            //ChunkCount = 8,  // (要下载的文件分块数)

            // 限制下载速度为2MB/s，默认值为零或无限制
            //MaximumBytesPerSecond = 1024 * 1024 * 2,  // (下载速度限制，以字节/秒为单位)

            // 失败时的最大重试次数
            MaxTryAgainOnFailover = 2,  // (失败时的最大重试次数)

            // 是否并行下载文件的部分，默认值为false
            ParallelDownload = true,  // (是否并行下载文件的不同部分)

            // 并行下载的数量。默认值与分块数相同
            ParallelCount = 8,  // (并行下载的数量)

            // 当下载完成但失败时，清除包块数据， 默认值为false
            ClearPackageOnCompletionWithFailure = true,  // (在下载失败时是否清除包块数据)

            // 在开始下载之前，预留文件大小的存储空间，默认值为false
            ReserveStorageSpaceBeforeStartingDownload = true,  // (在开始下载之前是否为文件预留存储空间)
        };
        /// <summary>
        /// 下载文件进度条
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="Name"></param>
        /// <param name="RequestConfiguration"></param>
        /// <returns></returns>
        public static async Task<DownloadFileData> DownloadFileProgressBar(string Url, string Name, RequestConfiguration? RequestConfiguration = null)
        {
            if (RequestConfiguration != null)
                DownloadOpt.RequestConfiguration = RequestConfiguration;

            DownloadFileData downloadFileData = new();
            var downloader = new DownloadService(DownloadOpt);
            await AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                            new TaskDescriptionColumn(),    // 任务描述
                            new ProgressBarColumn(),        // 进度栏
                            new PercentageColumn(),         // 百分比
                            new RemainingTimeColumn(),      // 余下的时间
                            new SpinnerColumn(),            // 旋转器
                })
                .StartAsync(async ctx =>
                {
                    ProgressTask? task = null;
                    // 在每次下载开始时提供`FileName`和`TotalBytesToReceive`信息
                    downloader.DownloadStarted += (sender, e) =>
                    {
                        downloadFileData.Name = e.FileName;
                        downloadFileData.MaxLength = e.TotalBytesToReceive;
                        AnsiConsole.MarkupLine($"[yellow]开始下载文件[/] [dodgerblue2]{Name}[/] ({Consoles.ConvertByteUnits(e.TotalBytesToReceive)})");
                        //创建进度条
                        task = ctx.AddTask($"[green]{downloadFileData.Name}[/]");
                        task.MaxValue = downloadFileData.MaxLength;
                    };
                    // (下载开始事件：当下载开始时触发此事件，提供下载文件名和总字节数)
                    // 提供有关分块下载的任何信息，如每个分块的进度百分比、速度、总接收字节数和接收字节数组以进行实时流传输
                    /*downloader.ChunkDownloadProgressChanged += (sender, e) =>
                    {
                        AnsiConsole.WriteLine($"{e.ActiveChunks}---{Text.ConvertByteUnits(e.AverageBytesPerSecondSpeed)}");
                    };*/
                    // (分块下载进度更改事件：提供分块下载的进度信息，包括每个分块的进度百分比、速度等)
                    // 提供有关下载进度的任何信息，如所有分块进度的总进度百分比、总速度、平均速度、总接收字节数和接收字节数组以进行实时流传输
                    downloader.DownloadProgressChanged += (sender, e) =>
                    {
                        task!.Value = e.ReceivedBytesSize;
                        task.Description = $"[green]{downloadFileData.Name}[/] [yellow]{Consoles.ConvertByteUnits(e.BytesPerSecondSpeed)}[/]";
                        //Console.WriteLine(e.ProgressPercentage);
                    };
                    // (下载进度更改事件：提供下载的总体进度信息，包括总进度百分比、速度等)
                    // 下载完成事件，可以包括发生的错误、取消或成功完成的下载
                    downloader.DownloadFileCompleted += (sender, e) =>
                    {
                        if (e.Error != null)
                        {
                            downloadFileData.State = false;
                            task!.Description = $"[red]{Name} 下载失败![/]";
                        }
                        else
                        {
                            downloadFileData.State = true;
                            task!.Description = $"[green]{Name} 下载成功![/]";
                        }
                    };

                    // (下载文件完成事件：在下载完成时触发此事件，可能包括错误信息、取消或成功完成的下载)
                    await downloader.DownloadFileTaskAsync(Url, Name);
                });
            return downloadFileData;
        }

        /// <summary>
        /// 文件下载数据集
        /// </summary>
        public List<DownloadFileData?> DownloadFileDatas { get; } = new();
        /// <summary>
        /// 文件下载任务集
        /// </summary>
        private List<Task<bool>?>? DownloadFileTasks;
        /// <summary>
        /// 文件下载 基类
        /// </summary>
        /// <param name="Url">文件地址</param>
        /// <param name="Name">文件名称</param>
        /// <param name="RequestConfiguration">请求配置</param>
        /// <returns>任务成功与否</returns>
        public async Task<bool> DownloadFileBase(string Url, string Name, RequestConfiguration? RequestConfiguration = null)
        {
            if (RequestConfiguration != null)
                DownloadOpt.RequestConfiguration = RequestConfiguration;

            DownloadFileData downloadFileData = new();
            DownloadFileDatas.Add(downloadFileData);
            var downloader = new DownloadService(DownloadOpt);
            downloader.DownloadStarted += (sender, e) =>
            {
                downloadFileData.Name = e.FileName;
                downloadFileData.MaxLength = e.TotalBytesToReceive;
            };
            downloader.DownloadProgressChanged += (sender, e) =>
            {
                downloadFileData.CurrentLength = e.ReceivedBytesSize;
            };
            downloader.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    downloadFileData.State = false;
                    downloadFileData.Error = e.Error;
                }
                else
                    downloadFileData.State = true;
            };
            await downloader.DownloadFileTaskAsync(Url, Name);
            return downloadFileData.State;
        }

        /// <summary>
        /// 文件下载 基类
        /// </summary>
        /// <param name="DownloadInfo">下载信息</param>
        /// <returns>任务成功与否</returns>
        public async Task<bool> DownloadFileBase(DownloadInfo DownloadInfo)
        {
            return await DownloadFileBase(DownloadInfo.Url, DownloadInfo.Name, DownloadInfo.RequestConfiguration);
        }

        /// <summary>
        /// 文件下载 基类
        /// </summary>
        /// <param name="DownloadInfo">下载信息</param>
        /// <param name="Task">进步条信息</param>
        /// <returns>任务成功与否</returns>
        private async Task<bool> DownloadFileLineBase(DownloadInfo DownloadInfo, ProgressTask Task)
        {
            if (DownloadInfo.RequestConfiguration != null)
                DownloadOpt.RequestConfiguration = DownloadInfo.RequestConfiguration;

            DownloadFileData downloadFileData = new();
            DownloadFileDatas.Add(downloadFileData);
            var downloader = new DownloadService(DownloadOpt);
            downloader.DownloadStarted += (sender, e) =>
            {
                downloadFileData.Name = e.FileName;
                downloadFileData.MaxLength = e.TotalBytesToReceive;
                Task.MaxValue = e.TotalBytesToReceive;
                AnsiConsole.MarkupLine($"[yellow]开始下载文件[/] [dodgerblue2]{downloadFileData.Name}[/] ({Consoles.ConvertByteUnits(downloadFileData.MaxLength)})");
            };
            downloader.DownloadProgressChanged += (sender, e) =>
            {
                downloadFileData.CurrentLength = e.ReceivedBytesSize;
                Task.Value = e.ReceivedBytesSize;
                Task.Description = $"[green]{downloadFileData.Name}[/] [yellow]{Consoles.ConvertByteUnits(e.BytesPerSecondSpeed)}[/]";
            };
            downloader.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    downloadFileData.State = false;
                    downloadFileData.Error = e.Error;
                    Task!.Description = $"[red]{downloadFileData.Name} 下载失败![/]";
                }
                else
                {
                    downloadFileData.State = true;
                    Task!.Description = $"[green]{downloadFileData.Name} 下载成功![/]";
                }
            };
            await downloader.DownloadFileTaskAsync(DownloadInfo.Url, DownloadInfo.Name);
            return downloadFileData.State;
        }

        /// <summary>
        /// 下载列队
        /// </summary>
        /// <param name="DownloadInfos">下载信息列表</param>
        /// <returns>任务</returns>
        //public async Task DownloadFileLine(List<DownloadInfo> DownloadInfos)
        //{
        //    await AnsiConsole.Progress()
        //        .Columns(new ProgressColumn[]
        //        {
        //                    new TaskDescriptionColumn(),    // 任务描述
        //                    new ProgressBarColumn(),        // 进度栏
        //                    new PercentageColumn(),         // 百分比
        //                    new RemainingTimeColumn(),      // 余下的时间
        //                    new SpinnerColumn(),            // 旋转器
        //        })
        //        .StartAsync(async ctx =>
        //        {
        //            //遍历添加任务
        //            DownloadFileTasks = new();
        //            for (int i = 0; i < DownloadInfos.Count; i++)
        //            {
        //                var Task = ctx.AddTask($"[green]{DownloadInfos[i].Name}[/]");
        //                DownloadFileTasks.Add(DownloadFileLineBase(DownloadInfos[i], Task));
        //            }
        //            //等待所有下载任务结束
        //            Task.WaitAll(DownloadFileTasks.ToArray()!);
        //        });
        //}
        /// <summary>
        /// 下载信息
        /// </summary>
        public class DownloadInfo
        {
            /// <summary>
            /// 文件地址
            /// </summary>
            public required string Url { get; set; }
            /// <summary>
            /// 文件名称
            /// </summary>
            public required string Name { get; set; }
            /// <summary>
            /// 请求配置
            /// </summary>
            public RequestConfiguration? RequestConfiguration { get; set; } = null;
        }

        /// <summary>
        /// 下载数据内容
        /// </summary>
        public class DownloadFileData
        {
            /// <summary>
            /// 文件名
            /// </summary>
            public string? Name { get; set; }
            /// <summary>
            /// 文件大小
            /// </summary>
            public long MaxLength { get; set; }
            /// <summary>
            /// 目前大小
            /// </summary>
            public long CurrentLength { get; set; }
            /// <summary>
            /// 状态
            /// </summary>
            public bool State { get; set; }
            /// <summary>
            /// 错误
            /// </summary>
            public Exception? Error { get; set; }
        }
    }

}
