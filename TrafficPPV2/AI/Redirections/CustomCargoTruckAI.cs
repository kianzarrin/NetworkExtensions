using System;
using System.Runtime.CompilerServices;
using ColossalFramework;
using ColossalFramework.Math;
using Transit.Framework.Network;
using UnityEngine;
using Transit.Framework.Redirection;

namespace CSL_Traffic
{
    public class CustomCargoTruckAI : CarAI
    {
        [RedirectFrom(typeof(CargoTruckAI))]
        public override void SimulationStep(ushort vehicleID, ref Vehicle data, Vector3 physicsLodRefPos)
        {
            if ((TrafficMod.Options & OptionsManager.ModOptions.UseRealisticSpeeds) == OptionsManager.ModOptions.UseRealisticSpeeds)
            {
                var speedData = CarSpeedData.Of(vehicleID);

                if (speedData.SpeedMultiplier == 0 || speedData.CurrentPath != data.m_path)
                {
                    speedData.CurrentPath = data.m_path;
                    speedData.SetRandomSpeedMultiplier(0.7f, 1.1f);
                }
                m_info.ApplySpeedMultiplier(CarSpeedData.Of(vehicleID));
            }

            if ((data.m_flags & Vehicle.Flags.Congestion) != Vehicle.Flags.None)
            {
                if ((TrafficMod.Options & OptionsManager.ModOptions.NoDespawn) != OptionsManager.ModOptions.NoDespawn)
                {
                    Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                }
            }
            else
            {
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None && (data.m_waitCounter += 1) > 20)
                {
                    this.RemoveOffers(vehicleID, ref data);
                    data.m_flags &= ~Vehicle.Flags.WaitingTarget;
                    data.m_flags |= Vehicle.Flags.GoingBack;
                    data.m_waitCounter = 0;
                    if (!this.StartPathFind(vehicleID, ref data))
                    {
                        data.Unspawn(vehicleID);
                    }
                }
                base.SimulationStep(vehicleID, ref data, physicsLodRefPos);
            }

            if ((TrafficMod.Options & OptionsManager.ModOptions.UseRealisticSpeeds) == OptionsManager.ModOptions.UseRealisticSpeeds)
            {
                m_info.RestoreVehicleSpeed(CarSpeedData.Of(vehicleID));
            }
        }

        [RedirectFrom(typeof(CargoTruckAI))]
        protected override bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            if ((vehicleData.m_flags & Vehicle.Flags.WaitingTarget) != Vehicle.Flags.None)
            {
                return true;
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != Vehicle.Flags.None)
            {
                if (vehicleData.m_sourceBuilding != 0)
                {
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    BuildingInfo info = instance.m_buildings.m_buffer[(int)vehicleData.m_sourceBuilding].Info;
                    Randomizer randomizer = new Randomizer((int)vehicleID);
                    Vector3 vector;
                    Vector3 endPos;
                    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[(int)vehicleData.m_sourceBuilding], ref randomizer, this.m_info, out vector, out endPos);
                    return this.StartPathFind(ExtendedVehicleType.CargoTruck, vehicleID, ref vehicleData, vehicleData.m_targetPos3, endPos, IsHeavyVehicle(), IgnoreBlocked(vehicleID, ref vehicleData));
                }
            }
            else if (vehicleData.m_targetBuilding != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance2.m_buildings.m_buffer[(int)vehicleData.m_targetBuilding].Info;
                Randomizer randomizer2 = new Randomizer((int)vehicleID);
                Vector3 vector2;
                Vector3 endPos2;
                info2.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref instance2.m_buildings.m_buffer[(int)vehicleData.m_targetBuilding], ref randomizer2, this.m_info, out vector2, out endPos2);
                return this.StartPathFind(ExtendedVehicleType.CargoTruck, vehicleID, ref vehicleData, vehicleData.m_targetPos3, endPos2, IsHeavyVehicle(), IgnoreBlocked(vehicleID, ref vehicleData));
            }
            return false;
        }

        [RedirectFrom(typeof(CargoTruckAI))]
        protected override bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget)
        {
            if ((vehicleData.m_flags & (Vehicle.Flags.TransferToSource | Vehicle.Flags.GoingBack)) != Vehicle.Flags.None)
            {
                return this.StartPathFind(ExtendedVehicleType.CargoTruck, vehicleID, ref vehicleData, startPos, endPos, startBothWays, endBothWays, undergroundTarget, IsHeavyVehicle(), IgnoreBlocked(vehicleID, ref vehicleData));
            }
            bool allowUnderground = (vehicleData.m_flags & (Vehicle.Flags.Underground | Vehicle.Flags.Transition)) != Vehicle.Flags.None;
            PathUnit.Position startPosA;
            PathUnit.Position startPosB;
            float num;
            float num2;
            bool flag = CustomPathManager.FindPathPosition(ExtendedVehicleType.CargoTruck, startPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, allowUnderground, false, 32f, out startPosA, out startPosB, out num, out num2);
            PathUnit.Position position;
            PathUnit.Position position2;
            float num3;
            float num4;
            if (CustomPathManager.FindPathPosition(ExtendedVehicleType.CargoTruck, startPos, ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship, allowUnderground, false, 32f, out position, out position2, out num3, out num4))
            {
                if (!flag || num3 < num)
                {
                    startPosA = position;
                    startPosB = position2;
                    num = num3;
                    num2 = num4;
                }
                flag = true;
            }
            PathUnit.Position endPosA;
            PathUnit.Position endPosB;
            float num5;
            float num6;
            bool flag2 = CustomPathManager.FindPathPosition(ExtendedVehicleType.CargoTruck, endPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, undergroundTarget, false, 32f, out endPosA, out endPosB, out num5, out num6);
            PathUnit.Position position3;
            PathUnit.Position position4;
            float num7;
            float num8;
            if (CustomPathManager.FindPathPosition(ExtendedVehicleType.CargoTruck, endPos, ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship, undergroundTarget, false, 32f, out position3, out position4, out num7, out num8))
            {
                if (!flag2 || num7 < num5)
                {
                    endPosA = position3;
                    endPosB = position4;
                    num5 = num7;
                    num6 = num8;
                }
                flag2 = true;
            }
            if (flag && flag2)
            {
                PathManager instance = Singleton<PathManager>.instance;
                if (!startBothWays || num < 10f)
                {
                    startPosB = default(PathUnit.Position);
                }
                if (!endBothWays || num5 < 10f)
                {
                    endPosB = default(PathUnit.Position);
                }
                NetInfo.LaneType laneTypes = NetInfo.LaneType.Vehicle | NetInfo.LaneType.CargoVehicle;
                VehicleInfo.VehicleType vehicleTypes = VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship;
                uint path;
                bool createPathResult = Singleton<PathManager>.instance.CreatePath(ExtendedVehicleType.CargoTruck, out path, ref Singleton<SimulationManager>.instance.m_randomizer, Singleton<SimulationManager>.instance.m_currentBuildIndex, startPosA, startPosB, endPosA, endPosB, laneTypes, vehicleTypes, 20000f, this.IsHeavyVehicle(), this.IgnoreBlocked(vehicleID, ref vehicleData), false, false);
                if (createPathResult)
                {
                    if (vehicleData.m_path != 0u)
                    {
                        instance.ReleasePath(vehicleData.m_path);
                    }
                    vehicleData.m_path = path;
                    vehicleData.m_flags |= Vehicle.Flags.WaitingPath;
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [RedirectTo(typeof(CargoTruckAI))]
        private void RemoveOffers(ushort vehicleID, ref Vehicle data)
        {
            throw new NotImplementedException("RemoveOffers is target of redirection and is not implemented.");
        }
    }
}
