using System;
using KartRider;

namespace ExcData
{
	public class SpeedPatch
	{
        public static float DragFactor = 0f; // 最高速度 (空氣阻力係數，數值影響平跑極速)
        public static float ForwardAccelForce = 0f; // 前進加速度 (平跑加速力)
        public static float DriftEscapeForce = 0f; // 飄移脫離力 (影響拖飄速度與出彎順滑度)
        public static float CornerDrawFactor = 0f; // 彎道加速度 (過彎時的抓地轉向加速)
        public static float DriftMaxGauge = 0f; // 集氣量 (氮氣條累積速度)
        public static float TransAccelFactor = 0f; // 變形氮氣加速度 (二段加速/變形時的加速力)
        public static float BoostAccelFactor = 0f; // 氮氣加速度 (一般開啟氮氣時的加速力)
        public static float StartForwardAccelForceItem = 0f; // 起步氮氣加速度 (道具賽)
        public static float StartForwardAccelForceSpeed = 0f; // 起步氮氣加速度 (競速賽)

        public static void SpeedPatcData()
		{
			if (Program.SpeedPatch)
			{
				SpeedPatch.DragFactor = -0.003f;
				SpeedPatch.ForwardAccelForce = 30f;
				SpeedPatch.DriftEscapeForce = 200f;
				SpeedPatch.CornerDrawFactor = 0.0015f;
				SpeedPatch.DriftMaxGauge = -70f;
				SpeedPatch.TransAccelFactor = 0.005f;
				SpeedPatch.BoostAccelFactor = 0.005f;
				SpeedPatch.StartForwardAccelForceItem = 100f;
				SpeedPatch.StartForwardAccelForceSpeed = 100f;
			}
			else
			{
				SpeedPatch.DragFactor = 0f;
				SpeedPatch.ForwardAccelForce = 0f;
				SpeedPatch.DriftEscapeForce = 0f;
				SpeedPatch.CornerDrawFactor = 0f;
				SpeedPatch.DriftMaxGauge = 0f;
				SpeedPatch.TransAccelFactor = 0f;
				SpeedPatch.BoostAccelFactor = 0f;
				SpeedPatch.StartForwardAccelForceItem = 0f;
				SpeedPatch.StartForwardAccelForceSpeed = 0f;
			}
		}
	}
}