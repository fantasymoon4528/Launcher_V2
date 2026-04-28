using System;
using KartRider.IO.Packet;
using ExcData;
using Profile;

namespace KartRider
{
    public class StartGameData
    {
        public static void Start_KartSpac(SessionGroup Parent, string Nickname, byte StartType, byte StartTimeAttack_StartType, int Unk1, uint Track, byte StartTimeAttack_SpeedType)
        {
            if (StartType == 1)
            {
                Console.WriteLine("故事模式");
                StartGameData.PrKartSpec(Parent, Nickname, StartTimeAttack_SpeedType);
            }
            else if (StartType == 2)
            {
                Console.WriteLine("挑戰者");
                StartGameData.PrchallengerKartSpec(Parent, Nickname, StartTimeAttack_SpeedType);
            }
            else if (StartType == 3)
            {
                Console.WriteLine("排行计时");
                if (StartTimeAttack_StartType == 0)
                {
                    StartGameData.PrStartTimeAttack(Parent, Nickname, Unk1, Track, StartTimeAttack_SpeedType);
                }
                else
                {
                    StartGameData.PrStartTimeAttack_QuestType(Parent, Nickname, Unk1, Track, StartTimeAttack_SpeedType);
                }
            }
            else
            {
                GameSupport.OnDisconnect(Parent);
            }
        }

        public static void PrStartTimeAttack(SessionGroup Parent, string Nickname, int Unk1, uint Track, byte StartTimeAttack_SpeedType)
        {
            using (OutPacket oPacket = new OutPacket("PrStartTimeAttack"))
            {
                oPacket.WriteInt(Unk1);
                oPacket.WriteInt(0);
                GetKartSpac(oPacket, Nickname, StartTimeAttack_SpeedType);
                oPacket.WriteByte(0);
                oPacket.WriteInt(0);
                oPacket.WriteInt(0);
                oPacket.WriteUInt(ProfileService.GetProfileConfig(Nickname).Rider.Lucci);
                oPacket.WriteUInt(ProfileService.GetProfileConfig(Nickname).Rider.Koin);
                oPacket.WriteUInt(Track);
                Parent.Client.Send(oPacket);
            }
        }

        public static void PrchallengerKartSpec(SessionGroup Parent, string Nickname, byte StartTimeAttack_SpeedType)
        {
            using (OutPacket oPacket = new OutPacket("PrchallengerKartSpec"))
            {
                oPacket.WriteByte(1);
                GetKartSpac(oPacket, Nickname, StartTimeAttack_SpeedType);
                oPacket.WriteInt(0);
                oPacket.WriteByte(0);
                Parent.Client.Send(oPacket);
            }
        }

        public static void PrKartSpec(SessionGroup Parent, string Nickname, byte StartTimeAttack_SpeedType)
        {
            using (OutPacket oPacket = new OutPacket("PrKartSpec"))
            {
                oPacket.WriteByte(1);
                GetDefaultSpac(oPacket, Nickname, StartTimeAttack_SpeedType);
                oPacket.WriteByte(0);
                Parent.Client.Send(oPacket);
            }
        }

        public static void PrStartTimeAttack_QuestType(SessionGroup Parent, string Nickname, int Unk1, uint Track, byte StartTimeAttack_SpeedType)
        {
            using (OutPacket oPacket = new OutPacket("PrStartTimeAttack"))
            {
                oPacket.WriteInt(Unk1);
                oPacket.WriteInt(0);
                GetDefaultSpac(oPacket, Nickname, StartTimeAttack_SpeedType);
                oPacket.WriteByte(0);
                oPacket.WriteInt(0);
                oPacket.WriteInt(0);
                oPacket.WriteUInt(ProfileService.GetProfileConfig(Nickname).Rider.Lucci);
                oPacket.WriteUInt(ProfileService.GetProfileConfig(Nickname).Rider.Koin);
                oPacket.WriteUInt(Track);
                Parent.Client.Send(oPacket);
            }
        }

        public static void GetKartSpac(OutPacket oPacket, string Nickname, byte StartTimeAttack_SpeedType)
        {
            var speedType = new SpeedType();
            string version = ProfileService.SettingConfig.Version;
            byte speed = ProfileService.SettingConfig.SpeedType;
            int roomId = RoomManager.TryGetRoomId(Nickname);
            if (roomId != -1)
            {
                var room = RoomManager.GetRoom(roomId);
                StartTimeAttack_SpeedType = room.SpeedType;
                var parsed = SpeedType.Parse(room.RoomName);
                if (parsed.HasValue)
                {
                    if (parsed.Value.infinite != byte.MaxValue)
                    {
                        StartTimeAttack_SpeedType = parsed.Value.infinite;
                    }
                    if (parsed.Value.speed != byte.MaxValue)
                    {
                        speed = parsed.Value.speed;
                        version = parsed.Value.version;
                    }
                    Console.WriteLine($"RoomName: {room.RoomName}, 速度類型: {StartTimeAttack_SpeedType}, 版本: {version}, 速度: {speed}");
                }
            }

            speedType.SpeedTypeData(version, speed);

            int StartPosition = oPacket.Position;
            var FlyingPet = new FlyingPetSpec();
            FlyingPet.FlyingPet_Spec(Nickname);

            var Kart = new KartSpec();
            Kart.GetKartSpec(Nickname);

            var excSpecs = new ExcSpecs();
            ExcSpec.Use_TuneSpec(Nickname, excSpecs);
            ExcSpec.Use_PlantSpec(Nickname, excSpecs);
            ExcSpec.Use_KartLevelSpec(Nickname, excSpecs);
            ExcSpec.Use_PartsSpec(Nickname, excSpecs);

            var V2Spec = new V2Specs();
            V2Spec.ExceedSpec(Nickname, Kart);

            float DriftEscapeForce = FlyingPet.DriftEscapeForce + excSpecs.Tune_DriftEscapeForce + excSpecs.Plant45_DriftEscapeForce + excSpecs.KartLevel_DriftEscapeForce + SpeedPatch.DriftEscapeForce + V2Spec.V2Parts_DriftEscapeForce + V2Spec.V2Level_DriftEscapeForce;
            float NormalBoosterTime = FlyingPet.NormalBoosterTime + excSpecs.Tune_NormalBoosterTime + excSpecs.Plant46_NormalBoosterTime + V2Spec.V2Parts_NormalBoosterTime + V2Spec.V2Level_NormalBoosterTime;
            float TransAccelFactor = excSpecs.Tune_TransAccelFactor + excSpecs.Plant43_TransAccelFactor + excSpecs.KartLevel_TransAccelFactor + SpeedPatch.TransAccelFactor + V2Spec.V2Parts_TransAccelFactor + V2Spec.V2Level_TransAccelFactor;
            //------------------------------------------------------------------------KartSpac Start
            oPacket.WriteEncFloat(Kart.draftMulAccelFactor);
            oPacket.WriteEncInt(Kart.draftTick);
            oPacket.WriteEncFloat(Kart.driftBoostMulAccelFactor);
            oPacket.WriteEncInt(Kart.driftBoostTick);
            oPacket.WriteEncFloat(Kart.chargeBoostBySpeed);
            oPacket.WriteEncByte((byte)(Kart.SpeedSlotCapacity + excSpecs.Plant46_SpeedSlotCapacity));
            oPacket.WriteEncByte((byte)(Kart.ItemSlotCapacity + excSpecs.Plant46_ItemSlotCapacity));
            oPacket.WriteEncByte(Kart.SpecialSlotCapacity);
            oPacket.WriteEncByte(Kart.UseTransformBooster);
            oPacket.WriteEncByte(Kart.motorcycleType);
            oPacket.WriteEncByte(Kart.BikeRearWheel);
            oPacket.WriteEncFloat(Kart.Mass);
            oPacket.WriteEncFloat(Kart.AirFriction);
            oPacket.WriteEncFloat(speedType.DragFactor + Kart.DragFactor + FlyingPet.DragFactor + SpeedPatch.DragFactor + excSpecs.Tune_DragFactor + excSpecs.Plant43_DragFactor + excSpecs.Plant45_DragFactor + excSpecs.KartLevel_DragFactor);
            oPacket.WriteEncFloat(speedType.ForwardAccelForce + Kart.ForwardAccelForce + FlyingPet.ForwardAccelForce + excSpecs.Tune_ForwardAccel + excSpecs.Plant43_ForwardAccel + excSpecs.Plant46_ForwardAccel + excSpecs.KartLevel_ForwardAccel + SpeedPatch.ForwardAccelForce + V2Spec.V2Level_ForwardAccelForce);
            oPacket.WriteEncFloat(speedType.BackwardAccelForce + Kart.BackwardAccelForce);
            oPacket.WriteEncFloat(speedType.GripBrakeForce + Kart.GripBrakeForce + excSpecs.Plant44_GripBrake + excSpecs.Plant46_GripBrake);
            oPacket.WriteEncFloat(speedType.SlipBrakeForce + Kart.SlipBrakeForce + excSpecs.Plant44_SlipBrake + excSpecs.Plant45_SlipBrake + excSpecs.Plant46_SlipBrake);
            oPacket.WriteEncFloat(Kart.MaxSteerAngle);
            if (excSpecs.PartSpec_SteerConstraint == 0f)
            {
                oPacket.WriteEncFloat(speedType.SteerConstraint + Kart.SteerConstraint + excSpecs.Plant44_SteerConstraint + excSpecs.KartLevel_SteerConstraint + V2Spec.V2Parts_SteerConstraint);
            }
            else
            {
                oPacket.WriteEncFloat(excSpecs.PartSpec_SteerConstraint + speedType.AddSpec_SteerConstraint + excSpecs.Plant44_SteerConstraint + excSpecs.KartLevel_SteerConstraint + V2Spec.V2Parts_SteerConstraint);
            }
            oPacket.WriteEncFloat(Kart.FrontGripFactor + excSpecs.Plant44_FrontGripFactor);
            oPacket.WriteEncFloat(Kart.RearGripFactor + excSpecs.Plant44_RearGripFactor);
            oPacket.WriteEncFloat(Kart.DriftTriggerFactor);
            oPacket.WriteEncFloat(Kart.DriftTriggerTime);
            oPacket.WriteEncFloat(Kart.DriftSlipFactor + excSpecs.Plant46_DriftSlipFactor);
            if (excSpecs.PartSpec_DriftEscapeForce == 0f)
            {
                oPacket.WriteEncFloat(speedType.DriftEscapeForce + Kart.DriftEscapeForce + DriftEscapeForce);
            }
            else
            {
                oPacket.WriteEncFloat(excSpecs.PartSpec_DriftEscapeForce + speedType.AddSpec_DriftEscapeForce + DriftEscapeForce);
            }
            oPacket.WriteEncFloat(speedType.CornerDrawFactor + Kart.CornerDrawFactor + FlyingPet.CornerDrawFactor + excSpecs.Tune_CornerDrawFactor + excSpecs.Plant44_CornerDrawFactor + excSpecs.Plant45_CornerDrawFactor + excSpecs.KartLevel_CornerDrawFactor + SpeedPatch.CornerDrawFactor + V2Spec.V2Level_CornerDrawFactor);
            oPacket.WriteEncFloat(Kart.DriftLeanFactor);
            oPacket.WriteEncFloat(Kart.SteerLeanFactor);
            if (speed == 4 || speed == 6 || StartTimeAttack_SpeedType == 4 || StartTimeAttack_SpeedType == 6)
            {
                oPacket.WriteEncFloat(GameType.S4_DriftMaxGauge);
            }
            else
            {
                oPacket.WriteEncFloat(speedType.DriftMaxGauge + Kart.DriftMaxGauge + excSpecs.Tune_DriftMaxGauge + excSpecs.Plant45_DriftMaxGauge + excSpecs.Plant46_DriftMaxGauge + SpeedPatch.DriftMaxGauge + V2Spec.V2Level_DriftMaxGauge);
            }
            if (speed == 6 || StartTimeAttack_SpeedType == 6)
            {
                oPacket.WriteEncFloat(GameType.S6_BoosterTime);
            }
            else
            {
                if (excSpecs.PartSpec_NormalBoosterTime == 0f)
                {
                    oPacket.WriteEncFloat(Kart.NormalBoosterTime + NormalBoosterTime);
                }
                else
                {
                    oPacket.WriteEncFloat(excSpecs.PartSpec_NormalBoosterTime + NormalBoosterTime);
                }
            }
            oPacket.WriteEncFloat(Kart.ItemBoosterTime + FlyingPet.ItemBoosterTime);
            if (speed == 6 || StartTimeAttack_SpeedType == 6)
            {
                oPacket.WriteEncFloat(GameType.S6_BoosterTime);
            }
            else
            {
                oPacket.WriteEncFloat(Kart.TeamBoosterTime + FlyingPet.TeamBoosterTime + excSpecs.Tune_TeamBoosterTime + excSpecs.Plant46_TeamBoosterTime + V2Spec.V2Level_TeamBoosterTime);
            }
            oPacket.WriteEncFloat(Kart.AnimalBoosterTime + excSpecs.Plant45_AnimalBoosterTime + excSpecs.Plant46_AnimalBoosterTime);
            oPacket.WriteEncFloat(Kart.SuperBoosterTime);
            if (excSpecs.PartSpec_TransAccelFactor == 0f)
            {
                oPacket.WriteEncFloat(speedType.TransAccelFactor + Kart.TransAccelFactor + TransAccelFactor);
            }
            else
            {
                oPacket.WriteEncFloat(excSpecs.PartSpec_TransAccelFactor + speedType.AddSpec_TransAccelFactor + TransAccelFactor);
            }
            oPacket.WriteEncFloat(speedType.BoostAccelFactor + Kart.BoostAccelFactor + SpeedPatch.BoostAccelFactor);
            oPacket.WriteEncFloat(Kart.StartBoosterTimeItem + excSpecs.KartLevel_StartBoosterTimeItem + excSpecs.Plant46_StartBoosterTimeItem);
            oPacket.WriteEncFloat(Kart.StartBoosterTimeSpeed + excSpecs.Tune_StartBoosterTimeSpeed + excSpecs.Plant43_StartBoosterTimeSpeed + excSpecs.Plant46_StartBoosterTimeSpeed + excSpecs.KartLevel_StartBoosterTimeSpeed + V2Spec.V2Level_StartBoosterTimeSpeed);
            oPacket.WriteEncFloat(speedType.StartForwardAccelForceItem + Kart.StartForwardAccelForceItem + FlyingPet.StartForwardAccelForceItem + SpeedPatch.StartForwardAccelForceItem + excSpecs.Plant46_StartForwardAccelItem);
            oPacket.WriteEncFloat(speedType.StartForwardAccelForceSpeed + Kart.StartForwardAccelForceSpeed + FlyingPet.StartForwardAccelForceSpeed + SpeedPatch.StartForwardAccelForceSpeed + excSpecs.Plant43_StartForwardAccelSpeed + excSpecs.Plant46_StartForwardAccelSpeed);
            oPacket.WriteEncFloat(Kart.DriftGaguePreservePercent);
            oPacket.WriteEncByte(Kart.UseExtendedAfterBooster);
            oPacket.WriteEncFloat(Kart.BoostAccelFactorOnlyItem + excSpecs.KartLevel_BoostAccelFactorOnlyItem);
            oPacket.WriteEncFloat(Kart.antiCollideBalance + excSpecs.Plant45_AntiCollideBalance);
            oPacket.WriteEncByte(Kart.dualBoosterSetAuto);
            oPacket.WriteEncInt(Kart.dualBoosterTickMin);
            oPacket.WriteEncInt(Kart.dualBoosterTickMax);
            oPacket.WriteEncFloat(Kart.dualMulAccelFactor);
            oPacket.WriteEncFloat(Kart.dualTransLowSpeed);
            oPacket.WriteEncByte(Kart.PartsEngineLock);
            oPacket.WriteEncByte(Kart.PartsWheelLock);
            oPacket.WriteEncByte(Kart.PartsSteeringLock);
            oPacket.WriteEncByte(Kart.PartsBoosterLock);
            oPacket.WriteEncByte(Kart.PartsCoatingLock);
            oPacket.WriteEncByte(Kart.PartsTailLampLock);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByBoost);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByGrip);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByWall);
            oPacket.WriteEncFloat(Kart.instAccelFactor);
            oPacket.WriteEncInt(Kart.instAccelGaugeCooldownTime);
            oPacket.WriteEncFloat(Kart.instAccelGaugeLength);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinUsable);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinVelBound);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinVelLoss);
            oPacket.WriteEncByte(Kart.useExtendedAfterBoosterMore);
            oPacket.WriteEncInt(Kart.wallCollGaugeCooldownTime);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMaxVelLoss);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMinVelBound);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMinVelLoss);
            oPacket.WriteEncFloat(Kart.modelMaxX);
            oPacket.WriteEncFloat(Kart.modelMaxY);
            oPacket.WriteEncInt(Kart.defaultExceedType);
            oPacket.WriteEncByte(Kart.defaultEngineType);
            oPacket.WriteEncByte(Kart.EngineType);
            oPacket.WriteEncByte(Kart.defaultHandleType);
            oPacket.WriteEncByte(Kart.HandleType);
            oPacket.WriteEncByte(Kart.defaultWheelType);
            oPacket.WriteEncByte(Kart.WheelType);
            oPacket.WriteEncByte(Kart.defaultBoosterType);
            oPacket.WriteEncByte(Kart.BoosterType);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByWallAdded);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByBoostAdded);
            oPacket.WriteEncInt(Kart.chargerSystemboosterUseCount);
            oPacket.WriteEncFloat(Kart.chargerSystemUseTime);
            oPacket.WriteEncFloat(Kart.chargeBoostBySpeedAdded);
            oPacket.WriteEncFloat(Kart.driftGaugeFactor);
            oPacket.WriteEncFloat(Kart.chargeAntiCollideBalance);
            oPacket.WriteEncInt(Kart.startItemTableId);
            oPacket.WriteEncInt(Kart.startItemId);
            oPacket.WriteEncFloat(Kart.Unknown1);
            oPacket.WriteEncByte(Kart.PartsBoosterEffectLock);
            //------------------------------------------------------------------------KartSpac End
            KartSpecLog(oPacket, StartPosition);
        }

        public static void GetDefaultSpac(OutPacket oPacket, string Nickname, byte StartTimeAttack_SpeedType)
        {
            var speedType = new SpeedType();
            speedType.SpeedTypeData(ProfileService.SettingConfig.Version, ProfileService.SettingConfig.SpeedType);

            var FlyingPet = new FlyingPetSpec();
            FlyingPet.FlyingPet_Spec(Nickname);
            var Kart = new KartSpec();
            Kart.GetKartSpec(Nickname);

            var V2Spec = new V2Specs();
            V2Spec.ExceedSpec(Nickname, Kart);

            //------------------------------------------------------------------------KartSpac Start
            oPacket.WriteEncFloat(Kart.draftMulAccelFactor);
            oPacket.WriteEncInt(Kart.draftTick);
            oPacket.WriteEncFloat(Kart.driftBoostMulAccelFactor);
            oPacket.WriteEncInt(Kart.driftBoostTick);
            oPacket.WriteEncFloat(Kart.chargeBoostBySpeed);
            oPacket.WriteEncByte(Kart.SpeedSlotCapacity);
            oPacket.WriteEncByte(Kart.ItemSlotCapacity);
            oPacket.WriteEncByte(Kart.SpecialSlotCapacity);
            oPacket.WriteEncByte(Kart.UseTransformBooster);
            oPacket.WriteEncByte(Kart.motorcycleType);
            oPacket.WriteEncByte(Kart.BikeRearWheel);
            oPacket.WriteEncFloat(Kart.Mass);
            oPacket.WriteEncFloat(Kart.AirFriction);
            oPacket.WriteEncFloat(speedType.DragFactor + Kart.DragFactor + FlyingPet.DragFactor + SpeedPatch.DragFactor);
            oPacket.WriteEncFloat(speedType.ForwardAccelForce + Kart.ForwardAccelForce + FlyingPet.ForwardAccelForce + SpeedPatch.ForwardAccelForce);
            oPacket.WriteEncFloat(speedType.BackwardAccelForce + Kart.BackwardAccelForce);
            oPacket.WriteEncFloat(speedType.GripBrakeForce + Kart.GripBrakeForce);
            oPacket.WriteEncFloat(speedType.SlipBrakeForce + Kart.SlipBrakeForce);
            oPacket.WriteEncFloat(Kart.MaxSteerAngle);
            oPacket.WriteEncFloat(speedType.SteerConstraint + Kart.SteerConstraint + V2Spec.V2Default_SteerConstraint);
            oPacket.WriteEncFloat(Kart.FrontGripFactor);
            oPacket.WriteEncFloat(Kart.RearGripFactor);
            oPacket.WriteEncFloat(Kart.DriftTriggerFactor);
            oPacket.WriteEncFloat(Kart.DriftTriggerTime);
            oPacket.WriteEncFloat(Kart.DriftSlipFactor);
            oPacket.WriteEncFloat(speedType.DriftEscapeForce + Kart.DriftEscapeForce + FlyingPet.DriftEscapeForce + SpeedPatch.DriftEscapeForce + V2Spec.V2Default_DriftEscapeForce);
            oPacket.WriteEncFloat(speedType.CornerDrawFactor + Kart.CornerDrawFactor + FlyingPet.CornerDrawFactor + SpeedPatch.CornerDrawFactor);
            oPacket.WriteEncFloat(Kart.DriftLeanFactor);
            oPacket.WriteEncFloat(Kart.SteerLeanFactor);
            if (ProfileService.SettingConfig.SpeedType == 4 || ProfileService.SettingConfig.SpeedType == 6 || StartTimeAttack_SpeedType == 4 || StartTimeAttack_SpeedType == 6)
            {
                oPacket.WriteEncFloat(GameType.S4_DriftMaxGauge);
            }
            else
            {
                oPacket.WriteEncFloat(speedType.DriftMaxGauge + Kart.DriftMaxGauge + SpeedPatch.DriftMaxGauge);
            }
            if (ProfileService.SettingConfig.SpeedType == 6 || StartTimeAttack_SpeedType == 6)
            {
                oPacket.WriteEncFloat(GameType.S6_BoosterTime);
            }
            else
            {
                oPacket.WriteEncFloat(Kart.NormalBoosterTime + FlyingPet.NormalBoosterTime + V2Spec.V2Default_NormalBoosterTime);
            }
            oPacket.WriteEncFloat(Kart.ItemBoosterTime + FlyingPet.ItemBoosterTime);
            if (ProfileService.SettingConfig.SpeedType == 6 || StartTimeAttack_SpeedType == 6)
            {
                oPacket.WriteEncFloat(GameType.S6_BoosterTime);
            }
            else
            {
                oPacket.WriteEncFloat(Kart.TeamBoosterTime + FlyingPet.TeamBoosterTime);
            }
            oPacket.WriteEncFloat(Kart.AnimalBoosterTime);
            oPacket.WriteEncFloat(Kart.SuperBoosterTime);
            oPacket.WriteEncFloat(speedType.TransAccelFactor + Kart.TransAccelFactor + SpeedPatch.TransAccelFactor + V2Spec.V2Default_TransAccelFactor);
            oPacket.WriteEncFloat(speedType.BoostAccelFactor + Kart.BoostAccelFactor + SpeedPatch.BoostAccelFactor);
            oPacket.WriteEncFloat(Kart.StartBoosterTimeItem);
            oPacket.WriteEncFloat(Kart.StartBoosterTimeSpeed);
            oPacket.WriteEncFloat(speedType.StartForwardAccelForceItem + Kart.StartForwardAccelForceItem + FlyingPet.StartForwardAccelForceItem + SpeedPatch.StartForwardAccelForceItem);
            oPacket.WriteEncFloat(speedType.StartForwardAccelForceSpeed + Kart.StartForwardAccelForceSpeed + FlyingPet.StartForwardAccelForceSpeed + SpeedPatch.StartForwardAccelForceSpeed);
            oPacket.WriteEncFloat(Kart.DriftGaguePreservePercent);
            oPacket.WriteEncByte(Kart.UseExtendedAfterBooster);
            oPacket.WriteEncFloat(Kart.BoostAccelFactorOnlyItem);
            oPacket.WriteEncFloat(Kart.antiCollideBalance);
            oPacket.WriteEncByte(Kart.dualBoosterSetAuto);
            oPacket.WriteEncInt(Kart.dualBoosterTickMin);
            oPacket.WriteEncInt(Kart.dualBoosterTickMax);
            oPacket.WriteEncFloat(Kart.dualMulAccelFactor);
            oPacket.WriteEncFloat(Kart.dualTransLowSpeed);
            oPacket.WriteEncByte(Kart.PartsEngineLock);
            oPacket.WriteEncByte(Kart.PartsWheelLock);
            oPacket.WriteEncByte(Kart.PartsSteeringLock);
            oPacket.WriteEncByte(Kart.PartsBoosterLock);
            oPacket.WriteEncByte(Kart.PartsCoatingLock);
            oPacket.WriteEncByte(Kart.PartsTailLampLock);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByBoost);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByGrip);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByWall);
            oPacket.WriteEncFloat(Kart.instAccelFactor);
            oPacket.WriteEncInt(Kart.instAccelGaugeCooldownTime);
            oPacket.WriteEncFloat(Kart.instAccelGaugeLength);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinUsable);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinVelBound);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinVelLoss);
            oPacket.WriteEncByte(Kart.useExtendedAfterBoosterMore);
            oPacket.WriteEncInt(Kart.wallCollGaugeCooldownTime);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMaxVelLoss);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMinVelBound);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMinVelLoss);
            oPacket.WriteEncFloat(Kart.modelMaxX);
            oPacket.WriteEncFloat(Kart.modelMaxY);
            oPacket.WriteEncInt(Kart.defaultExceedType);
            oPacket.WriteEncByte(Kart.defaultEngineType);
            oPacket.WriteEncByte(Kart.EngineType);
            oPacket.WriteEncByte(Kart.defaultHandleType);
            oPacket.WriteEncByte(Kart.HandleType);
            oPacket.WriteEncByte(Kart.defaultWheelType);
            oPacket.WriteEncByte(Kart.WheelType);
            oPacket.WriteEncByte(Kart.defaultBoosterType);
            oPacket.WriteEncByte(Kart.BoosterType);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByWallAdded);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByBoostAdded);
            oPacket.WriteEncInt(Kart.chargerSystemboosterUseCount);
            oPacket.WriteEncFloat(Kart.chargerSystemUseTime);
            oPacket.WriteEncFloat(Kart.chargeBoostBySpeedAdded);
            oPacket.WriteEncFloat(Kart.driftGaugeFactor);
            oPacket.WriteEncFloat(Kart.chargeAntiCollideBalance);
            oPacket.WriteEncInt(Kart.startItemTableId);
            oPacket.WriteEncInt(Kart.startItemId);
            oPacket.WriteEncFloat(Kart.Unknown1);
            oPacket.WriteEncByte(Kart.PartsBoosterEffectLock);
            //------------------------------------------------------------------------KartSpac End
        }

        public static void GetSchoolSpac(OutPacket oPacket, string Nickname)
        {
            var Kart = new KartSpec();
            Kart.GetKartSpec(Nickname, true);

            //------------------------------------------------------------------------KartSpac Start
            oPacket.WriteEncFloat(Kart.draftMulAccelFactor);
            oPacket.WriteEncInt(Kart.draftTick);
            oPacket.WriteEncFloat(Kart.driftBoostMulAccelFactor);
            oPacket.WriteEncInt(Kart.driftBoostTick);
            oPacket.WriteEncFloat(Kart.chargeBoostBySpeed);
            oPacket.WriteEncByte(Kart.SpeedSlotCapacity);
            oPacket.WriteEncByte(Kart.ItemSlotCapacity);
            oPacket.WriteEncByte(Kart.SpecialSlotCapacity);
            oPacket.WriteEncByte(Kart.UseTransformBooster);
            oPacket.WriteEncByte(Kart.motorcycleType);
            oPacket.WriteEncByte(Kart.BikeRearWheel);
            oPacket.WriteEncFloat(Kart.Mass);
            oPacket.WriteEncFloat(Kart.AirFriction);
            oPacket.WriteEncFloat(Kart.DragFactor);
            oPacket.WriteEncFloat(Kart.ForwardAccelForce + SpeedPatch.ForwardAccelForce);
            oPacket.WriteEncFloat(Kart.BackwardAccelForce);
            oPacket.WriteEncFloat(Kart.GripBrakeForce);
            oPacket.WriteEncFloat(Kart.SlipBrakeForce);
            oPacket.WriteEncFloat(Kart.MaxSteerAngle);
            oPacket.WriteEncFloat(Kart.SteerConstraint);
            oPacket.WriteEncFloat(Kart.FrontGripFactor);
            oPacket.WriteEncFloat(Kart.RearGripFactor);
            oPacket.WriteEncFloat(Kart.DriftTriggerFactor);
            oPacket.WriteEncFloat(Kart.DriftTriggerTime);
            oPacket.WriteEncFloat(Kart.DriftSlipFactor);
            oPacket.WriteEncFloat(Kart.DriftEscapeForce + SpeedPatch.DriftEscapeForce);
            oPacket.WriteEncFloat(Kart.CornerDrawFactor + SpeedPatch.CornerDrawFactor);
            oPacket.WriteEncFloat(Kart.DriftLeanFactor);
            oPacket.WriteEncFloat(Kart.SteerLeanFactor);
            oPacket.WriteEncFloat(Kart.DriftMaxGauge + SpeedPatch.DriftMaxGauge);
            oPacket.WriteEncFloat(Kart.NormalBoosterTime);
            oPacket.WriteEncFloat(Kart.ItemBoosterTime);
            oPacket.WriteEncFloat(Kart.TeamBoosterTime);
            oPacket.WriteEncFloat(Kart.AnimalBoosterTime);
            oPacket.WriteEncFloat(Kart.SuperBoosterTime);
            oPacket.WriteEncFloat(Kart.TransAccelFactor + SpeedPatch.TransAccelFactor);
            oPacket.WriteEncFloat(Kart.BoostAccelFactor + SpeedPatch.BoostAccelFactor);
            oPacket.WriteEncFloat(Kart.StartBoosterTimeItem);
            oPacket.WriteEncFloat(Kart.StartBoosterTimeSpeed);
            oPacket.WriteEncFloat(Kart.StartForwardAccelForceItem + SpeedPatch.StartForwardAccelForceItem);
            oPacket.WriteEncFloat(Kart.StartForwardAccelForceSpeed + SpeedPatch.StartForwardAccelForceSpeed);
            oPacket.WriteEncFloat(Kart.DriftGaguePreservePercent);
            oPacket.WriteEncByte(Kart.UseExtendedAfterBooster);
            oPacket.WriteEncFloat(Kart.BoostAccelFactorOnlyItem);
            oPacket.WriteEncFloat(Kart.antiCollideBalance);
            oPacket.WriteEncByte(Kart.dualBoosterSetAuto);
            oPacket.WriteEncInt(Kart.dualBoosterTickMin);
            oPacket.WriteEncInt(Kart.dualBoosterTickMax);
            oPacket.WriteEncFloat(Kart.dualMulAccelFactor);
            oPacket.WriteEncFloat(Kart.dualTransLowSpeed);
            oPacket.WriteEncByte(Kart.PartsEngineLock);
            oPacket.WriteEncByte(Kart.PartsWheelLock);
            oPacket.WriteEncByte(Kart.PartsSteeringLock);
            oPacket.WriteEncByte(Kart.PartsBoosterLock);
            oPacket.WriteEncByte(Kart.PartsCoatingLock);
            oPacket.WriteEncByte(Kart.PartsTailLampLock);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByBoost);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByGrip);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByWall);
            oPacket.WriteEncFloat(Kart.instAccelFactor);
            oPacket.WriteEncInt(Kart.instAccelGaugeCooldownTime);
            oPacket.WriteEncFloat(Kart.instAccelGaugeLength);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinUsable);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinVelBound);
            oPacket.WriteEncFloat(Kart.instAccelGaugeMinVelLoss);
            oPacket.WriteEncByte(Kart.useExtendedAfterBoosterMore);
            oPacket.WriteEncInt(Kart.wallCollGaugeCooldownTime);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMaxVelLoss);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMinVelBound);
            oPacket.WriteEncFloat(Kart.wallCollGaugeMinVelLoss);
            oPacket.WriteEncFloat(Kart.modelMaxX);
            oPacket.WriteEncFloat(Kart.modelMaxY);
            oPacket.WriteEncInt(Kart.defaultExceedType);
            oPacket.WriteEncByte(Kart.defaultEngineType);
            oPacket.WriteEncByte(Kart.EngineType);
            oPacket.WriteEncByte(Kart.defaultHandleType);
            oPacket.WriteEncByte(Kart.HandleType);
            oPacket.WriteEncByte(Kart.defaultWheelType);
            oPacket.WriteEncByte(Kart.WheelType);
            oPacket.WriteEncByte(Kart.defaultBoosterType);
            oPacket.WriteEncByte(Kart.BoosterType);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByWallAdded);
            oPacket.WriteEncFloat(Kart.chargeInstAccelGaugeByBoostAdded);
            oPacket.WriteEncInt(Kart.chargerSystemboosterUseCount);
            oPacket.WriteEncFloat(Kart.chargerSystemUseTime);
            oPacket.WriteEncFloat(Kart.chargeBoostBySpeedAdded);
            oPacket.WriteEncFloat(Kart.driftGaugeFactor);
            oPacket.WriteEncFloat(Kart.chargeAntiCollideBalance);
            oPacket.WriteEncInt(Kart.startItemTableId);
            oPacket.WriteEncInt(Kart.startItemId);
            oPacket.WriteEncFloat(Kart.Unknown1);
            oPacket.WriteEncByte(Kart.PartsBoosterEffectLock);
            //------------------------------------------------------------------------KartSpac End
        }

        public static void KartSpecLog(OutPacket oPacket, int StartPosition)
        {
            InPacket iPacket = new InPacket(oPacket.ToArray());
            iPacket.Position = StartPosition;
            Console.WriteLine($"-------------------------------------------------------------");
            Console.WriteLine($"尾流加速係數 (draftMulAccelFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"尾流持續時間 (draftTick):{iPacket.ReadEncodedInt()}");
            Console.WriteLine($"飄移加速/雙噴係數 (driftBoostMulAccelFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"飄移加速/雙噴時間 (driftBoostTick):{iPacket.ReadEncodedInt()}");
            Console.WriteLine($"速度集氣係數 (chargeBoostBySpeed):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"競速道具槽容量 (SpeedSlotCapacity):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"道具槽容量 (ItemSlotCapacity):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"特殊道具槽容量 (SpecialSlotCapacity):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"使用變形氮氣 (UseTransformBooster):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"摩托車類型 (motorcycleType):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"摩托車後輪 (BikeRearWheel):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"質量/車重 (Mass):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"空氣摩擦力 (AirFriction):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"空氣阻力 (DragFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"平跑加速力 (ForwardAccelForce):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"倒車加速力 (BackwardAccelForce):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"抓地煞車力 (GripBrakeForce):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"飄移煞車力 (SlipBrakeForce):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"最大轉向角 (MaxSteerAngle):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"轉向靈敏度 (SteerConstraint):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"前輪抓地力 (FrontGripFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"後輪抓地力 (RearGripFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"飄移觸發係數 (DriftTriggerFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"飄移觸發時間 (DriftTriggerTime):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"飄移滑動係數 (DriftSlipFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"飄移脫離力/拖飄力 (DriftEscapeForce):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"彎道加速 (CornerDrawFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"飄移傾斜係數 (DriftLeanFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"轉向傾斜係數 (SteerLeanFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"集氣量上限 (DriftMaxGauge):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"一般氮氣時間 (NormalBoosterTime):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"道具氮氣時間 (ItemBoosterTime):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"團隊氮氣時間 (TeamBoosterTime):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"騎寵氮氣時間 (AnimalBoosterTime):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"超級氮氣時間 (SuperBoosterTime):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"變形加速力 (TransAccelFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"氮氣加速力 (BoostAccelFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"道具起步氮氣時間 (StartBoosterTimeItem):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"競速起步氮氣時間 (StartBoosterTimeSpeed):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"道具起步加速力 (StartForwardAccelForceItem):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"競速起步加速力 (StartForwardAccelForceSpeed):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"防撞保留集氣比例 (DriftGaguePreservePercent):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"使用延伸尾流 (UseExtendedAfterBooster):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"純道具氮氣加速力 (BoostAccelFactorOnlyItem):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"防撞平衡 (antiCollideBalance):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"自動雙噴 (dualBoosterSetAuto):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"雙噴最小觸發時間 (dualBoosterTickMin):{iPacket.ReadEncodedInt()}");
            Console.WriteLine($"雙噴最大觸發時間 (dualBoosterTickMax):{iPacket.ReadEncodedInt()}");
            Console.WriteLine($"雙噴加速係數 (dualMulAccelFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"雙噴轉換低速閾值 (dualTransLowSpeed):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"引擎零件鎖定 (PartsEngineLock):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"輪胎零件鎖定 (PartsWheelLock):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"方向盤零件鎖定 (PartsSteeringLock):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"推進器零件鎖定 (PartsBoosterLock):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"塗裝零件鎖定 (PartsCoatingLock):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"尾燈零件鎖定 (PartsTailLampLock):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"氮氣增加超越集氣 (chargeInstAccelGaugeByBoost):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"抓地增加超越集氣 (chargeInstAccelGaugeByGrip):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"撞牆增加超越集氣 (chargeInstAccelGaugeByWall):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"超越加速係數 (instAccelFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"超越冷卻時間 (instAccelGaugeCooldownTime):{iPacket.ReadEncodedInt()}");
            Console.WriteLine($"超越能量槽長度 (instAccelGaugeLength):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"超越最低可用閾值 (instAccelGaugeMinUsable):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"超越最低速度限制 (instAccelGaugeMinVelBound):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"超越最低速度損失 (instAccelGaugeMinVelLoss):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"使用更多延伸尾流 (useExtendedAfterBoosterMore):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"撞牆集氣冷卻時間 (wallCollGaugeCooldownTime):{iPacket.ReadEncodedInt()}");
            Console.WriteLine($"撞牆集氣最大速度損失 (wallCollGaugeMaxVelLoss):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"撞牆集氣最小速度限制 (wallCollGaugeMinVelBound):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"撞牆集氣最小速度損失 (wallCollGaugeMinVelLoss):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"模型最大X軸 (modelMaxX):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"模型最大Y軸 (modelMaxY):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"預設超越類型 (defaultExceedType):{iPacket.ReadEncodedInt()}");
            Console.WriteLine($"預設引擎類型 (defaultEngineType):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"引擎類型 (EngineType):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"預設方向盤類型 (defaultHandleType):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"方向盤類型 (HandleType):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"預設輪胎類型 (defaultWheelType):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"輪胎類型 (WheelType):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"預設推進器類型 (defaultBoosterType):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"推進器類型 (BoosterType):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"撞牆額外超越集氣 (chargeInstAccelGaugeByWallAdded):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"氮氣額外超越集氣 (chargeInstAccelGaugeByBoostAdded):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"充能系統氮氣使用次數 (chargerSystemboosterUseCount):{iPacket.ReadEncodedInt()}");
            Console.WriteLine($"充能系統使用時間 (chargerSystemUseTime):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"速度額外集氣增加 (chargeBoostBySpeedAdded):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"飄移集氣係數 (driftGaugeFactor):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"充能防撞平衡 (chargeAntiCollideBalance):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"起步道具表ID (startItemTableId):{iPacket.ReadEncodedInt()}");
            Console.WriteLine($"起步道具ID (startItemId):{iPacket.ReadEncodedInt()}");
            Console.WriteLine($"未知參數1 (Unknown1):{iPacket.ReadEncodedFloat()}");
            Console.WriteLine($"推進器特效鎖定 (PartsBoosterEffectLock):{iPacket.ReadEncodedByte()}");
            Console.WriteLine($"-------------------------------------------------------------");
        }
    }
}