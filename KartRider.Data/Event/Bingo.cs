using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using KartRider.IO.Packet;

namespace KartRider
{
    public class Reward
    {
        public int StockId { get; set; }
        public int Probability { get; set; }

        public Reward(int stockId, int probability)
        {
            StockId = stockId;
            Probability = probability;
        }
    }

    public static class LotteryManager
    {
        // 静态存储所有奖励项及其概率
        public static List<Reward> RewardList = new List<Reward>();

        // 总概率，用于计算抽取概率
        public static int TotalProbability = 0;

        /**
         * 初始化方法，读取XML文件并加载数据
         * @param xmlFilePath XML文件路径
         */
        public static void Initialize(XmlNodeList rewardNodes)
        {
            try
            {
                // 清空现有数据
                RewardList.Clear();
                TotalProbability = 0;

                // 遍历所有奖励项
                foreach (XmlNode node in rewardNodes)
                {
                    XmlElement rewardElement = node as XmlElement;
                    if (rewardElement == null) continue;

                    // 获取stockId和概率
                    if (int.TryParse(rewardElement.GetAttribute("stockId"), out int stockId) &&
                        int.TryParse(rewardElement.GetAttribute("prob"), out int prob))
                    {
                        // 创建奖励对象并添加到列表
                        Reward reward = new Reward(stockId, prob);
                        RewardList.Add(reward);

                        // 累加总概率
                        TotalProbability += prob;
                    }
                }

                Console.WriteLine($"成功載入 {RewardList.Count} 個獎勵項，總機率為: {TotalProbability}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"載入XML文件出錯: {ex.Message}");
            }
        }

        /**
         * 随机获取指定数量的stockId，考虑概率因素
         * @param count 要获取的数量
         * @return 随机选中的stockId列表
         */
        public static List<int> GetRandomStockIds(int count)
        {
            List<int> result = new List<int>();

            // 检查是否已初始化
            if (RewardList.Count == 0)
            {
                Console.WriteLine("請先呼叫Initialize方法載入數據");
                return result;
            }

            // 检查数量是否合理
            if (count <= 0)
            {
                Console.WriteLine("請指定有效的獲取數量");
                return result;
            }

            // 获取不重复的奖励ID集合
            HashSet<int> uniqueStockIds = new HashSet<int>(RewardList.Select(r => r.StockId));
            int uniqueRewardCount = uniqueStockIds.Count;

            // 检查请求数量是否超过可用奖励数量
            if (count > uniqueRewardCount)
            {
                Console.WriteLine($"請求數量超過可用的不重複獎勵數量，最多只能返回 {uniqueRewardCount} 個結果");
                count = uniqueRewardCount;
            }

            // 创建奖励列表的副本用于操作，避免修改原始数据
            List<Reward> availableRewards = new List<Reward>(RewardList);
            // 使用当前时间作为种子创建Random实例，避免短时间内多次调用产生相同序列
            Random random = new Random(Guid.NewGuid().GetHashCode());

            // 已选中的奖励ID集合，用于双重保证不重复
            HashSet<int> selectedIds = new HashSet<int>();

            // 循环获取指定数量的不重复奖励
            while (selectedIds.Count < count && availableRewards.Count > 0)
            {
                // 计算当前可用奖励的总概率
                int currentTotalProb = availableRewards.Sum(r => r.Probability);
                if (currentTotalProb <= 0) break;

                // 生成0到总概率之间的随机数
                int randomValue = random.Next(0, currentTotalProb);
                int currentSum = 0;
                Reward selectedReward = null;

                // 根据概率分布查找选中的奖励
                foreach (var reward in availableRewards)
                {
                    currentSum += reward.Probability;
                    if (randomValue < currentSum)
                    {
                        selectedReward = reward;
                        break;
                    }
                }

                // 添加选中的奖励ID到结果
                if (selectedReward != null && selectedIds.Add(selectedReward.StockId))
                {
                    result.Add(selectedReward.StockId);
                    // 移除所有相同StockId的奖励，确保不会重复选中
                    availableRewards.RemoveAll(r => r.StockId == selectedReward.StockId);
                }
            }

            // 检查最终结果数量是否符合预期
            if (result.Count < count)
            {
                Console.WriteLine($"警告：實際返回 {result.Count} 個結果，少於請求的 {count} 個");
            }
            return result;
        }
    }

    public static class Bingo
    {
        public static short[] BingoLotteryIDs = new short[] { 1219, 1220 };
        public static byte BingoItem = 0;
        public static byte BingoNum = 0;
        public static short BingoCount = 0;
        public static Dictionary<byte, byte> BingoNums = new Dictionary<byte, byte>();
        public static List<byte> BingoNumsList = new List<byte>();
        public static Dictionary<int, byte> BingoItems = new Dictionary<int, byte>();
        public static List<int> BingoItemsList = new List<int>();

        public static void BingoNumber()
        {
            // 创建随机数生成器实例
            Random random = new Random();

            // 存储不重复随机数的集合
            HashSet<byte> uniqueNumbers = new HashSet<byte>();

            // 生成25个不重复的随机数
            while (uniqueNumbers.Count < 25)
            {
                // 生成1到50之间的随机数
                byte number = (byte)random.Next(1, 50);

                // 只有当集合中不包含该数字时才会添加成功
                uniqueNumbers.Add(number);
            }
            foreach (byte num in uniqueNumbers)
            {
                BingoNumsList.Add(num);
                BingoNums.TryAdd(num, 0);
            }
        }

        public static void SpRpLotteryPacket(SessionGroup Parent)
        {
            int stock1 = LotteryManager.GetRandomStockIds(1)[0];
            Random random = new Random();
            BingoNum = (byte)random.Next(1, 50);
            if (BingoNums.Count == 0 && BingoNumsList.Count == 0)
            {
                BingoNumber();
                if (BingoItems.Count == 0 && BingoItemsList.Count == 0)
                {
                    var srocks = LotteryManager.GetRandomStockIds(12);
                    foreach (int stock in srocks)
                    {
                        BingoItemsList.Add(stock);
                        BingoItems.TryAdd(stock, 0);
                    }
                }
            }
            using (OutPacket outPacket = new OutPacket("SpRpLotteryPacket"))
            {
                outPacket.WriteInt(0);
                outPacket.WriteInt(stock1);
                outPacket.WriteHexString("FFFFFFFF");
                outPacket.WriteByte(0);
                outPacket.WriteByte(BingoNum);
                outPacket.WriteBytes(new byte[11]);
                Parent.Client.Send(outPacket);
            }
            if (BingoNums.ContainsKey(BingoNum) && BingoNumsList.Contains(BingoNum))
            {
                BingoNums[BingoNum] = 1;
                CheckLinesAsArray();
            }
            Bingo.BingoCount++;
        }

        public static void CheckLinesAsArray()
        {
            // 驗證字典包含25個元素
            if (BingoNums == null)
                Console.WriteLine("BingoNums字典不能為null");

            if (BingoNums.Count != 25)
                Console.WriteLine("Bingo宮格必須包含25個數字");

            // 將字典轉換為5x5的二維數組，左上角為坐標原點(0,0)
            byte[,] grid = new byte[5, 5];

            // 字典的Key是按從左上角開始的行優先順序排列：0-24
            // 0: (0,0)  1: (0,1)  2: (0,2)  3: (0,3)  4: (0,4)  第一行(最上方)
            // 5: (1,0)  6: (1,1)  ...      9: (1,4)  第二行
            // ...
            // 20: (4,0) ...     24: (4,4)  第五行(最下方)
            // 使用列表填充網格（確保索引順序正確）
            for (int index = 0; index < BingoNumsList.Count; index++)
            {
                // 只處理前 25 個元素
                if (index >= 25)
                    break;

                int row = index / 5; // 計算行（0-4）
                int col = index % 5; // 計算列（0-4）

                // 二次校驗：確保row和col在有效範圍內
                if (row < 0 || row >= 5 || col < 0 || col >= 5)
                {
                    Console.WriteLine($"計算出無效的網格座標：index={index}, row={row}, col={col}");
                }

                grid[row, col] = BingoNums[BingoNumsList[index]]; // 0 = 未選中，1 = 已選中
            }

            // 檢查橫線(0-4) - 0是最上方，4是最下方
            // 與grid的行直接對應，無需反轉
            for (int row = 0; row < 5; row++)
            {
                bool isCompleted = true;
                for (int col = 0; col < 5; col++)
                {
                    if (grid[row, col] != 1)
                    {
                        isCompleted = false;
                        break;
                    }
                }
                var item1 = BingoItemsList[row];
                BingoItems[item1] = (byte)(isCompleted ? 1 : 0);
            }

            // 檢查右上到左下對角線(索引5)
            // 對應座標: (0,4), (1,3), (2,2), (3,1), (4,0)
            bool antiDiagonalCompleted = true;
            for (int i = 0; i < 5; i++)
            {
                if (grid[i, 4 - i] != 1)
                {
                    antiDiagonalCompleted = false;
                    break;
                }
            }
            var item5 = BingoItemsList[5];
            BingoItems[item5] = (byte)(antiDiagonalCompleted ? 1 : 0);

            // 檢查直線(6-10) - 6是最左方，10是最右方
            for (int col = 0; col < 5; col++)
            {
                bool isCompleted = true;
                for (int row = 0; row < 5; row++)
                {
                    if (grid[row, col] != 1)
                    {
                        isCompleted = false;
                        break;
                    }
                }
                var item6 = BingoItemsList[6 + col];
                BingoItems[item6] = (byte)(isCompleted ? 1 : 0);
            }

            // 檢查左上到右下對角線(索引11)
            // 對應座標: (0,0), (1,1), (2,2), (3,3), (4,4)
            bool mainDiagonalCompleted = true;
            for (int i = 0; i < 5; i++)
            {
                if (grid[i, i] != 1)
                {
                    mainDiagonalCompleted = false;
                    break;
                }
            }
            var item11 = BingoItemsList[11];
            BingoItems[item11] = (byte)(mainDiagonalCompleted ? 1 : 0);
        }
    }
}

