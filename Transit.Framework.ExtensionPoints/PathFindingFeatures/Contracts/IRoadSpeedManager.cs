﻿using ColossalFramework;
using Transit.Framework.Network;

namespace Transit.Framework.ExtensionPoints.PathFindingFeatures.Contracts
{
    public interface IRoadSpeedManager : IPathFindFeature
    {
        float GetLaneSpeedLimit(ref NetSegment segment, NetInfo.Lane laneInfo, ExtendedUnitType unitType);
    }

    public static class ExtendedRoadSpeedManagerExtensions
    {
        public static float GetLaneSpeedLimit(this IRoadSpeedManager roadSpeedManager, ushort segmentId, NetInfo.Lane laneInfo, ExtendedUnitType unitType)
        {
            var segment = Singleton<NetManager>.instance.m_segments.m_buffer[segmentId];

            return roadSpeedManager.GetLaneSpeedLimit(ref segment, laneInfo, unitType);
        }
    }
}