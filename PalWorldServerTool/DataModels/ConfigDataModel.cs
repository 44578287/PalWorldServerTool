using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PalWorldServerTool.DataModels
{
    /// <summary>
    /// 配置文件数据模型
    /// </summary>
    public class ConfigDataModel
    {
        /// <summary>
        /// 服务器保活
        /// </summary>
        public bool AutoServerLive { get; set; } = false;
        /// <summary>
        /// 定时重启服务器(分钟) 0 不使用
        /// </summary>
        public int TimedRebootServer { get; set; } = 0;
        /// <summary>
        /// 定时备份存档(分钟) 0 不使用
        /// </summary>
        public int TimedBackupSaved { get; set; } = 0;
        /// <summary>
        /// 关闭服务器时备份存档
        /// </summary>
        public bool ShutdownBackupSaved { get; set; } = false;
        /// <summary>
        /// 备份存档位置
        /// </summary>
        public string BackupSavedPath { get; set; } = "BackupSaved";
        /// <summary>
        /// 管理员密码
        /// </summary>
        public string? AdminPassword { get; set; }
        /// <summary>
        /// 关闭服务器等待时间(秒) RCON模式下有效
        /// </summary>
        public int CloseWaitingTime { get; set; } = 20;
    }
    [JsonSerializable(typeof(ConfigDataModel))]
    public partial class MyJsonContext : JsonSerializerContext
    {
    }
}
