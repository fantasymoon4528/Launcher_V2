using System;
using System.Collections.Generic;
using System.IO;

namespace KartRider;

/// <summary>
/// 技能映射設定項（包含目標道具ID和觸發機率）
/// </summary>
public class SkillMappingConfig
{
    /// <summary>
    /// 目標道具ID
    /// </summary>
    public short TargetItemId { get; set; }

    /// <summary>
    /// 觸發機率（0-100，100表示100%觸發）
    /// </summary>
    public byte Probability { get; set; } = 100;
}

/// <summary>
/// 特殊道具車設定類別
/// </summary>
public class SpecialKartConfig
{
    /// <summary>
    /// 特殊道具車：將指定道具變更為特殊道具
    /// </summary>
    public string SkillChangeDesc { get; set; }
    public Dictionary<ushort, Dictionary<short, SkillMappingConfig>> SkillChange { get; set; } = new();

    /// <summary>
    /// 特殊道具車：使用指定道具後獲得特殊道具
    /// </summary>
    public string SkillMappingsDesc { get; set; }
    public Dictionary<ushort, Dictionary<short, SkillMappingConfig>> SkillMappings { get; set; } = new();

    /// <summary>
    /// 特殊道具車：被指定道具攻擊後獲得特殊道具
    /// </summary>
    public string SkillAttackedDesc { get; set; }
    public Dictionary<ushort, Dictionary<short, SkillMappingConfig>> SkillAttacked { get; set; } = new();

    /// <summary>
    /// 將特殊道具車設定儲存到 JSON 檔案（存在時補充缺失內容，保留額外內容）
    /// </summary>
    /// <param name="filePath">檔案路徑（如：./Config/SpecialKartConfig.json）</param>
    public static void SaveConfigToFile(string filePath)
    {
        // 1. 建立預設設定模板
        var defaultConfig = GetDefaultConfig();

        // 2. 確保目錄存在
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 3. 處理檔案內容
        SpecialKartConfig finalConfig;
        if (File.Exists(filePath))
        {
            // 3.1 讀取現有設定
            var existingConfig = JsonHelper.DeserializeNoBom<SpecialKartConfig>(filePath) ?? new SpecialKartConfig();

            // 3.2 初始化現有設定的字典（避免 null 參考）
            existingConfig.SkillChange ??= new Dictionary<ushort, Dictionary<short, SkillMappingConfig>>();
            existingConfig.SkillMappings ??= new Dictionary<ushort, Dictionary<short, SkillMappingConfig>>();
            existingConfig.SkillAttacked ??= new Dictionary<ushort, Dictionary<short, SkillMappingConfig>>();

            // 3.3 補充缺失的描述文字
            existingConfig.SkillChangeDesc ??= defaultConfig.SkillChangeDesc;
            existingConfig.SkillMappingsDesc ??= defaultConfig.SkillMappingsDesc;
            existingConfig.SkillAttackedDesc ??= defaultConfig.SkillAttackedDesc;

            // 3.4 補充 SkillChange 中缺失的設定
            foreach (var (key, valueDict) in defaultConfig.SkillChange)
            {
                if (!existingConfig.SkillChange.ContainsKey(key))
                {
                    existingConfig.SkillChange[key] = new Dictionary<short, SkillMappingConfig>(valueDict);
                }
                else
                {
                    foreach (var (innerKey, innerValue) in valueDict)
                    {
                        if (!existingConfig.SkillChange[key].ContainsKey(innerKey))
                        {
                            existingConfig.SkillChange[key][innerKey] = new SkillMappingConfig
                            {
                                TargetItemId = innerValue.TargetItemId,
                                Probability = innerValue.Probability
                            };
                        }
                    }
                }
            }

            // 3.5 補充 SkillMappings 中缺失的設定
            foreach (var (key, valueDict) in defaultConfig.SkillMappings)
            {
                if (!existingConfig.SkillMappings.ContainsKey(key))
                {
                    existingConfig.SkillMappings[key] = new Dictionary<short, SkillMappingConfig>(valueDict);
                }
                else
                {
                    foreach (var (innerKey, innerValue) in valueDict)
                    {
                        if (!existingConfig.SkillMappings[key].ContainsKey(innerKey))
                        {
                            existingConfig.SkillMappings[key][innerKey] = new SkillMappingConfig
                            {
                                TargetItemId = innerValue.TargetItemId,
                                Probability = innerValue.Probability
                            };
                        }
                    }
                }
            }

            // 3.6 補充 SkillAttacked 中缺失的設定
            foreach (var (key, valueDict) in defaultConfig.SkillAttacked)
            {
                if (!existingConfig.SkillAttacked.ContainsKey(key))
                {
                    existingConfig.SkillAttacked[key] = new Dictionary<short, SkillMappingConfig>(valueDict);
                }
                else
                {
                    foreach (var (innerKey, innerValue) in valueDict)
                    {
                        if (!existingConfig.SkillAttacked[key].ContainsKey(innerKey))
                        {
                            existingConfig.SkillAttacked[key][innerKey] = new SkillMappingConfig
                            {
                                TargetItemId = innerValue.TargetItemId,
                                Probability = innerValue.Probability
                            };
                        }
                    }
                }
            }

            finalConfig = existingConfig;
            Console.WriteLine($"設定已更新（補充缺失內容）：{filePath}");
        }
        else
        {
            // 4. 檔案不存在時直接使用預設設定
            finalConfig = defaultConfig;
            Console.WriteLine($"設定已建立：{filePath}");
        }

        // 5. 寫入最終設定
        File.WriteAllText(filePath, JsonHelper.Serialize(finalConfig));
    }

    /// <summary>
    /// 從 JSON 檔案讀取特殊道具車設定
    /// </summary>
    /// <param name="filePath">設定檔路徑</param>
    /// <returns>特殊道具車設定物件（SpecialKartConfig）</returns>
    public static SpecialKartConfig LoadConfigFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("特殊道具車設定檔不存在", filePath);
        }

        SpecialKartConfig config;
        try
        {
            config = JsonHelper.DeserializeNoBom<SpecialKartConfig>(filePath);
            if (config == null)
            {
                throw new Exception("設定檔解析結果為 null");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"設定檔格式不正確或解析失敗: {ex.Message}");
            Console.WriteLine("將使用預設設定覆蓋本機檔案...");

            // 使用預設設定覆蓋本機檔案
            config = GetDefaultConfig();
            File.WriteAllText(filePath, JsonHelper.Serialize(config));
            Console.WriteLine($"本機檔案已用預設設定覆蓋: {filePath}");

            return config;
        }

        // 確保字典不為 null（避免後續使用時的 null 參考例外）
        config.SkillChange ??= new Dictionary<ushort, Dictionary<short, SkillMappingConfig>>();
        config.SkillMappings ??= new Dictionary<ushort, Dictionary<short, SkillMappingConfig>>();
        config.SkillAttacked ??= new Dictionary<ushort, Dictionary<short, SkillMappingConfig>>();

        Console.WriteLine($"道具車特性設定已成功從 {filePath} 讀取");
        return config;
    }

    /// <summary>
    /// 建立預設設定模板（提取為獨立方法便於維護）
    /// </summary>
    private static SpecialKartConfig GetDefaultConfig()
    {
        return new SpecialKartConfig
        {
            SkillChangeDesc = "特殊道具車：將指定道具變更為特殊道具",
            SkillChange = new Dictionary<ushort, Dictionary<short, SkillMappingConfig>>
            {
                { 1615, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 103, Probability = 100 }}, {7, new SkillMappingConfig { TargetItemId = 99, Probability = 100 }} } },
                { 1612, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 32, Probability = 100 }} } },
                { 1610, new Dictionary<short, SkillMappingConfig> { {9, new SkillMappingConfig { TargetItemId = 34, Probability = 100 }} } },
                { 1605, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 118, Probability = 100 }} } },
                { 1601, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 131, Probability = 100 }}, {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1600, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 103, Probability = 100 }}, {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1597, new Dictionary<short, SkillMappingConfig> { {9, new SkillMappingConfig { TargetItemId = 34, Probability = 100 }} } },
                { 1594, new Dictionary<short, SkillMappingConfig> { {3, new SkillMappingConfig { TargetItemId = 112, Probability = 100 }} } },
                { 1593, new Dictionary<short, SkillMappingConfig> { {3, new SkillMappingConfig { TargetItemId = 112, Probability = 100 }} } },
                { 1592, new Dictionary<short, SkillMappingConfig> { {3, new SkillMappingConfig { TargetItemId = 112, Probability = 100 }} } },
                { 1591, new Dictionary<short, SkillMappingConfig> { {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }}, {5, new SkillMappingConfig { TargetItemId = 24, Probability = 100 }} } },
                { 1590, new Dictionary<short, SkillMappingConfig> { {8, new SkillMappingConfig { TargetItemId = 17, Probability = 100 }} } },
                { 1588, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 32, Probability = 100 }}, {127, new SkillMappingConfig { TargetItemId = 32, Probability = 100 }} } },
                { 1585, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 118, Probability = 100 }} } },
                { 1579, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 103, Probability = 100 }}, {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1575, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 119, Probability = 100 }}, {9, new SkillMappingConfig { TargetItemId = 27, Probability = 100 }} } },
                { 1571, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 32, Probability = 100 }} } },
                { 1569, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 7, Probability = 100 }} } },
                { 1567, new Dictionary<short, SkillMappingConfig> { {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1565, new Dictionary<short, SkillMappingConfig> { {33, new SkillMappingConfig { TargetItemId = 137, Probability = 100 }}, {3, new SkillMappingConfig { TargetItemId = 137, Probability = 100 }} } },
                { 1563, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 136, Probability = 100 }}, {114, new SkillMappingConfig { TargetItemId = 16, Probability = 100 }} } },
                { 1561, new Dictionary<short, SkillMappingConfig> { {8, new SkillMappingConfig { TargetItemId = 37, Probability = 100 }}, {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1551, new Dictionary<short, SkillMappingConfig> { {8, new SkillMappingConfig { TargetItemId = 25, Probability = 100 }} } },
                { 1543, new Dictionary<short, SkillMappingConfig> { {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1548, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 132, Probability = 100 }} } },
                { 1536, new Dictionary<short, SkillMappingConfig> { {8, new SkillMappingConfig { TargetItemId = 17, Probability = 100 }}, {5, new SkillMappingConfig { TargetItemId = 103, Probability = 100 }} } },
                { 1526, new Dictionary<short, SkillMappingConfig> { {9, new SkillMappingConfig { TargetItemId = 27, Probability = 100 }} } },
                { 1522, new Dictionary<short, SkillMappingConfig> { {9, new SkillMappingConfig { TargetItemId = 34, Probability = 100 }}, {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1511, new Dictionary<short, SkillMappingConfig> { {2, new SkillMappingConfig { TargetItemId = 38, Probability = 100 }} } },
                { 1510, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 32, Probability = 100 }} } },
                { 1509, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 32, Probability = 100 }} } },
                { 1507, new Dictionary<short, SkillMappingConfig> { {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1506, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 103, Probability = 100 }} } },
                { 1505, new Dictionary<short, SkillMappingConfig> { {8, new SkillMappingConfig { TargetItemId = 129, Probability = 100 }}, {4, new SkillMappingConfig { TargetItemId = 120, Probability = 100 }} } },
                { 1502, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 4, Probability = 100 }} } },
                { 1500, new Dictionary<short, SkillMappingConfig> { {9, new SkillMappingConfig { TargetItemId = 34, Probability = 100 }}, {113, new SkillMappingConfig { TargetItemId = 135, Probability = 100 }}, {33, new SkillMappingConfig { TargetItemId = 135, Probability = 100 }} } },
                { 1496, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 134, Probability = 100 }}, {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1494, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 132, Probability = 100 }}, {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1491, new Dictionary<short, SkillMappingConfig> { {8, new SkillMappingConfig { TargetItemId = 82, Probability = 100 }}, {9, new SkillMappingConfig { TargetItemId = 27, Probability = 100 }}, {13, new SkillMappingConfig { TargetItemId = 28, Probability = 100 }} } },
                { 1489, new Dictionary<short, SkillMappingConfig> { {9, new SkillMappingConfig { TargetItemId = 111, Probability = 100 }}, {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1487, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 103, Probability = 100 }}, {10, new SkillMappingConfig { TargetItemId = 36, Probability = 100 }} } },
                { 1484, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 32, Probability = 100 }}, {6, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1482, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }} } },
                { 1481, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 102, Probability = 100 }}, {9, new SkillMappingConfig { TargetItemId = 34, Probability = 100 }} } },
                { 1479, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 131, Probability = 100 }} } }
            },

            SkillMappingsDesc = "特殊道具車：使用指定道具後獲得特殊道具",
            SkillMappings = new Dictionary<ushort, Dictionary<short, SkillMappingConfig>>
            {
                { 1607, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }} } },
                { 1601, new Dictionary<short, SkillMappingConfig> { {131, new SkillMappingConfig { TargetItemId = 5, Probability = 100 }} } },
                { 1597, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }} } },
                { 1590, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 10, Probability = 100 }} } },
                { 1569, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 7, Probability = 100 }} } },
                { 1567, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1450, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 5, Probability = 100 }}, {5, new SkillMappingConfig { TargetItemId = 24, Probability = 100 }} } },
                { 1563, new Dictionary<short, SkillMappingConfig> { {136, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }} } },
                { 1548, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }} } },
                { 1510, new Dictionary<short, SkillMappingConfig> { {32, new SkillMappingConfig { TargetItemId = 32, Probability = 60 }} } },
                { 1507, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1496, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 24, Probability = 100 }} } },
                { 1489, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 10, Probability = 100 }} } },
                { 1479, new Dictionary<short, SkillMappingConfig> { {131, new SkillMappingConfig { TargetItemId = 5, Probability = 100 }} } }
            },

            SkillAttackedDesc = "特殊道具車：被指定道具攻擊後獲得特殊道具",
            SkillAttacked = new Dictionary<ushort, Dictionary<short, SkillMappingConfig>>
            {
                { 1613, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 10, Probability = 100 }} } },
                { 1610, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }} } },
                { 1607, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 5, Probability = 100 }} } },
                { 1605, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 111, Probability = 100 }} } },
                { 1600, new Dictionary<short, SkillMappingConfig> { {32, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }}, {99, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1588, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 32, Probability = 100 }} } },
                { 1581, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }}, {7, new SkillMappingConfig { TargetItemId = 31, Probability = 100 }} } },
                { 1571, new Dictionary<short, SkillMappingConfig> { {8, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }} } },
                { 1561, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 111, Probability = 40 }} } },
                { 1557, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 32, Probability = 100 }}, {5, new SkillMappingConfig { TargetItemId = 103, Probability = 100 }} } },
                { 1555, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }}, {9, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }} } },
                { 1551, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }} } },
                { 1524, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 103, Probability = 100 }} } },
                { 1511, new Dictionary<short, SkillMappingConfig> { {7, new SkillMappingConfig { TargetItemId = 5, Probability = 100 }} } },
                { 1510, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 10, Probability = 100 }} } },
                { 1509, new Dictionary<short, SkillMappingConfig> { {5, new SkillMappingConfig { TargetItemId = 10, Probability = 100 }} } },
                { 1506, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }}, {9, new SkillMappingConfig { TargetItemId = 6, Probability = 100 }} } },
                { 1502, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 9, Probability = 100 }} } },
                { 1482, new Dictionary<short, SkillMappingConfig> { {4, new SkillMappingConfig { TargetItemId = 119, Probability = 100 }}, {9, new SkillMappingConfig { TargetItemId = 119, Probability = 100 }} } }
            }
        };
    }
}