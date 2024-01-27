using System.Reflection;
using IniParser;
using IniParser.Model;
using LoongEgg.LoongLogger;
using Spectre.Console;

namespace PalWorldServerTool.DataModels
{
    public class PalWorldSettingsDataModel
    {
        /// <summary>
        /// 难度
        /// </summary>
        public string Difficulty { get; set; } = "\"None\"";

        /// <summary>
        /// 白天时间速度
        /// </summary>
        public double DayTimeSpeedRate { get; set; } = 1.0;

        /// <summary>
        /// 夜晚时间速度
        /// </summary>
        public double NightTimeSpeedRate { get; set; } = 1.0;

        /// <summary>
        /// 经验值获取率
        /// </summary>
        public double ExpRate { get; set; } = 1.0;

        /// <summary>
        /// Pal 捕获率
        /// </summary>
        public double PalCaptureRate { get; set; } = 1.0;

        /// <summary>
        /// Pal 出现率
        /// </summary>
        public double PalSpawnNumRate { get; set; } = 1.0;

        /// <summary>
        /// 受到的 Pal 伤害倍率
        /// </summary>
        public double PalDamageRateAttack { get; set; } = 1.0;

        /// <summary>
        /// 对 Pal 造成的伤害倍率
        /// </summary>
        public double PalDamageRateDefense { get; set; } = 1.0;

        /// <summary>
        /// 受到的玩家伤害倍率
        /// </summary>
        public double PlayerDamageRateAttack { get; set; } = 1.0;

        /// <summary>
        /// 对玩家造成的伤害倍率
        /// </summary>
        public double PlayerDamageRateDefense { get; set; } = 1.0;

        /// <summary>
        /// 玩家饥饿值下降速度
        /// </summary>
        public double PlayerStomachDecreaceRate { get; set; } = 1.0;

        /// <summary>
        /// 玩家耐力下降速度
        /// </summary>
        public double PlayerStaminaDecreaceRate { get; set; } = 1.0;

        /// <summary>
        /// 玩家自动生命恢复率
        /// </summary>
        public double PlayerAutoHPRegeneRate { get; set; } = 1.0;

        /// <summary>
        /// 玩家睡眠中的生命恢复率
        /// </summary>
        public double PlayerAutoHpRegeneRateInSleep { get; set; } = 1.0;

        /// <summary>
        /// Pal 饥饿值下降速度
        /// </summary>
        public double PalStomachDecreaceRate { get; set; } = 1.0;

        /// <summary>
        /// Pal 耐力下降速度
        /// </summary>
        public double PalStaminaDecreaceRate { get; set; } = 1.0;

        /// <summary>
        /// Pal 自动生命恢复率
        /// </summary>
        public double PalAutoHPRegeneRate { get; set; } = 1.0;

        /// <summary>
        /// Pal 睡眠中的生命恢复率
        /// </summary>
        public double PalAutoHpRegeneRateInSleep { get; set; } = 1.0;

        /// <summary>
        /// 对结构的伤害倍率
        /// </summary>
        public double BuildObjectDamageRate { get; set; } = 1.0;

        /// <summary>
        /// 结构恶化伤害率
        /// </summary>
        public double BuildObjectDeteriorationDamageRate { get; set; } = 1.0;

        /// <summary>
        /// 采集物品倍率
        /// </summary>
        public double CollectionDropRate { get; set; } = 1.0;

        /// <summary>
        /// 采集对象的生命值倍率
        /// </summary>
        public double CollectionObjectHpRate { get; set; } = 1.0;

        /// <summary>
        /// 采集对象的重生间隔
        /// </summary>
        public double CollectionObjectRespawnSpeedRate { get; set; } = 1.0;

        /// <summary>
        /// 敌人掉落物品倍率
        /// </summary>
        public double EnemyDropItemRate { get; set; } = 1.0;

        /// <summary>
        /// 死亡惩罚
        /// </summary>
        public string DeathPenalty { get; set; } = "\"All\"";

        /// <summary>
        /// 启用玩家对玩家伤害
        /// </summary>
        public bool bEnablePlayerToPlayerDamage { get; set; } = false;

        /// <summary>
        /// 启用友军伤害
        /// </summary>
        public bool bEnableFriendlyFire { get; set; } = false;

        /// <summary>
        /// 启用侵略者敌人
        /// </summary>
        public bool bEnableInvaderEnemy { get; set; } = true;

        /// <summary>
        /// 看不懂
        /// </summary>
        public bool bActiveUNKO { get; set; } = false;

        /// <summary>
        /// 启用手柄辅助瞄准
        /// </summary>
        public bool bEnableAimAssistPad { get; set; } = true;

        /// <summary>
        /// 启用键盘辅助瞄准
        /// </summary>
        public bool bEnableAimAssistKeyboard { get; set; } = false;

        /// <summary>
        /// 掉落物品最大数量
        /// </summary>
        public int DropItemMaxNum { get; set; } = 3000;

        /// <summary>
        /// 看不懂 掉落物相关
        /// </summary>
        public int DropItemMaxNum_UNKO { get; set; } = 100;

        /// <summary>
        /// 基地最大数量
        /// </summary>
        public int BaseCampMaxNum { get; set; } = 128;

        /// <summary>
        /// 基地最大 Pal 数量
        /// </summary>
        public int BaseCampWorkerMaxNum { get; set; } = 15;

        /// <summary>
        /// 掉落物品最大存活小时数
        /// </summary>
        public double DropItemAliveMaxHours { get; set; } = 1.0;

        /// <summary>
        /// 当无在线玩家时是否自动重置公会
        /// </summary>
        public bool bAutoResetGuildNoOnlinePlayers { get; set; } = false;

        /// <summary>
        /// 公会无在线玩家自动重置时间
        /// </summary>
        public double AutoResetGuildTimeNoOnlinePlayers { get; set; } = 72.0;

        /// <summary>
        /// 公会最大玩家数
        /// </summary>
        public int GuildPlayerMaxNum { get; set; } = 20;

        /// <summary>
        /// Pal 蛋孵化时间
        /// </summary>
        public double PalEggDefaultHatchingTime { get; set; } = 72.0;

        /// <summary>
        /// 工作速度倍率
        /// </summary>
        public double WorkSpeedRate { get; set; } = 1.0;

        /// <summary>
        /// 是否为多人游戏
        /// </summary>
        public bool bIsMultiplay { get; set; } = false;

        /// <summary>
        /// 是否为玩家对战玩家
        /// </summary>
        public bool bIsPvP { get; set; } = false;

        /// <summary>
        /// 是否能够拾取其他公会的死亡惩罚掉落物品
        /// </summary>
        public bool bCanPickupOtherGuildDeathPenaltyDrop { get; set; } = false;

        /// <summary>
        /// 启用非登录惩罚 正版验证?
        /// </summary>
        public bool bEnableNonLoginPenalty { get; set; } = true;

        /// <summary>
        /// 启用传送锚点
        /// </summary>
        public bool bEnableFastTravel { get; set; } = true;

        /// <summary>
        /// 是否通过地图选择起始位置
        /// </summary>
        public bool bIsStartLocationSelectByMap { get; set; } = true;

        /// <summary>
        /// 玩家登出后角色是否仍然存在于游戏中
        /// </summary>
        public bool bExistPlayerAfterLogout { get; set; } = false;

        /// <summary>
        /// 是否启用对其他公会玩家的防御
        /// </summary>
        public bool bEnableDefenseOtherGuildPlayer { get; set; } = false;

        /// <summary>
        /// 合作模式最大玩家数量
        /// </summary>
        public int CoopPlayerMaxNum { get; set; } = 4;

        /// <summary>
        /// 服务器最大玩家数
        /// </summary>
        public int ServerPlayerMaxNum { get; set; } = 32;

        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName { get; set; } = "\"Default Palworld Server\"";

        /// <summary>
        /// 服务器描述
        /// </summary>
        public string ServerDescription { get; set; } = "\"\"";

        /// <summary>
        /// 管理员密码
        /// </summary>
        public string AdminPassword { get; set; } = "\"\"";

        /// <summary>
        /// 服务器密码
        /// </summary>
        public string ServerPassword { get; set; } = "\"\"";

        /// <summary>
        /// 公共端口号
        /// </summary>
        public int PublicPort { get; set; } = 8211;

        /// <summary>
        /// 公共IP
        /// </summary>
        public string PublicIP { get; set; } = "\"\"";

        /// <summary>
        /// 是否启用 RCON
        /// </summary>
        public bool RCONEnabled { get; set; } = true;

        /// <summary>
        /// RCON 端口号
        /// </summary>
        public int RCONPort { get; set; } = 25575;

        /// <summary>
        /// 地域
        /// </summary>
        public string Region { get; set; } = "\"\"";

        /// <summary>
        /// 是否使用身份验证
        /// </summary>
        public bool bUseAuth { get; set; } = true;

        /// <summary>
        /// 封禁列表URL
        /// </summary>
        public string BanListURL { get; set; } = "\"https://api.palworldgame.com/api/banlist.txt\"";

        ///// <summary>
        ///// 引索器
        ///// </summary>
        ///// <param name="Name"></param>
        ///// <returns></returns>
        //public object? this[string Name]
        //{
        //    get
        //    {
        //        object? obj = this.GetType().GetProperty(Name)?.GetValue(this, null);
        //        //object? obj = new();
        //        //this.GetProperties().Values.Where(N => N.Name == Name).First().TryGetValue(this, out obj);
        //        //var d = this.GetProperties<PalWorldSettingsDataModel>();
        //        Logger.WriteDebug($"获取PalGameWorldSettings.ini配置项: {Name}=>{obj}");
        //        return obj;
        //    }
        //    set
        //    {
        //        this.GetType().GetProperty(Name)?.SetValue(this, TypeTo[this[Name]?.GetType()!](value));
        //        //this.GetProperties().Values.Where(N => N.Name == Name).First().TrySetValue(this, TypeTo[this[Name]?.GetType()!](value));
        //        Logger.WriteDebug($"设置PalGameWorldSettings.ini配置项: {Name}=>{value}");
        //    }
        //}

        public bool TryGetValue(string Name, out object? value)
        {
            this.GetProperties().Values.Where(N => N.Name == Name).First().TryGetValue(this, out value);
            return value != null;
        }
        public bool TrySetValue(string Name, object? value)
        {
            this.GetProperties().Values.Where(N => N.Name == Name).First().TrySetValue(this, value);
            return true;
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="path">INI路径</param>
        public bool Load(string path)
        {
            try
            {
                var parser = new FileIniDataParser();
                IniData iniData = parser.ReadFile(path);
                string? optionSettingsData = iniData["/Script/Pal.PalGameWorldSettings"]["OptionSettings"]?.Trim(new char[] { '(', ')' });
                Logger.WriteDebug($"加载PalGameWorldSettings.ini文件: {optionSettingsData}");
                string[]? parts = optionSettingsData?.Split(',');
                if (parts != null && parts.Length != 1)
                {
                    foreach (var part in parts)
                    {
                        string[]? keyValue = part.Split('=');
                        //AnsiConsole.MarkupLine($"[green]{keyValue[0]}[/]: [yellow]{keyValue[1]}[/] {this[keyValue[0]]?.GetType()}");
                        //this.GetType().GetProperty(keyValue[0])?.SetValue(this, TypeTo[this[keyValue[0]]?.GetType()!](keyValue[1]));
                        //this[keyValue[0]] = keyValue[1];
                        TrySetValue(keyValue[0], keyValue[1]);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"加载PalGameWorldSettings.ini文件时发生错误 因为:{ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <param name="path">INI路径</param>
        public void Save(string path)
        {
            IniData iniData = new();
            var classList = this.GetType().GetProperties();
            var nonIndexerProperties = classList.Where(prop =>
            {
                MethodInfo[] accessors = prop.GetAccessors();
                return accessors.Any(accessor => accessor.IsSpecialName && accessor.Name.StartsWith("get_") && accessor.Name != "get_Item");
            });
            iniData["/Script/Pal.PalGameWorldSettings"]["OptionSettings"] = $"({string.Join(",", nonIndexerProperties.Select(x => $"{x.Name}={x.GetValue(this)?.ToString()}"))})";
            var parser = new FileIniDataParser();
            parser.WriteFile(path, iniData);
        }
    }
}
