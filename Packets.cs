using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hkmp.Networking.Packet;
using Hkmp.Math;

namespace DreamChaser.Packets
{
    public enum ServerPacketType
    {
        DamagedBoss // float damage
    }
    public enum ClientPacketType
    {
        SummonBoss, // Vector2 pos, int Phase, string Scene
        KillBoss,
        BossPosition, // Vector2 pos
        WarpOut,
        WarpIn,
        GorbNails, // float Angle
        MarkothNailSpawn, // Vector2 pos, int targetId, int id
        MarkothShield, // bool cw
        GrimmPillar, // Vector2 pos
        RadianceCross1, // float angle, int type
        RadianceCross2, // int type
        RadianceCross3, // int type
        RadiancePillarShoot, // bool right
        Animation, // string name
        PhaseIntro, // int Phase
    }
    public class DamagedBoss : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => false;

        public int damage;

        public void ReadData(IPacket packet) 
        {
            damage = packet.ReadInt();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(damage);
        }
    }
    public class SummonBoss : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => true;

        public Vector2 pos;

        public int Phase;

        public string Scene;

        public void ReadData(IPacket packet)
        {
            pos = packet.ReadVector2();
            Phase = packet.ReadInt();
            Scene = packet.ReadString();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(pos);
            packet.Write(Phase);
            packet.Write(Scene);
        }
    }
    public class KillBoss : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => true;

        public void ReadData(IPacket packet) { }

        public void WriteData(IPacket packet) { }
    }
    public class BossPosition : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => true;

        public Vector2 pos;

        public void ReadData(IPacket packet)
        {
            pos = packet.ReadVector2();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(pos);
        }
    }
    public class WarpOut : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => true;

        public void ReadData(IPacket packet) { }

        public void WriteData(IPacket packet) { }
    }
    public class WarpIn : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => true;

        public void ReadData(IPacket packet) { }

        public void WriteData(IPacket packet) { }
    }
    public class GorbNails : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => false;

        public float Angle;

        public void ReadData(IPacket packet)
        {
            Angle = packet.ReadFloat();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(Angle);
        }
    }
    public class MarkothNailSpawn : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => false;

        public Vector2 pos;
        public float angle;

        public void ReadData(IPacket packet)
        {
            pos = packet.ReadVector2();
            angle = packet.ReadFloat();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(pos);
            packet.Write(angle);
        }
    }
    public class MarkothShield : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => true;

        public bool cw;

        public void ReadData(IPacket packet)
        {
            cw = packet.ReadBool();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(cw);
        }
    }
    public class GrimmPillar : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => false;

        public Vector2 pos;

        public void ReadData(IPacket packet)
        {
            pos = packet.ReadVector2();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(pos);
        }
    }
    public class RadianceCross1 : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => true;

        public float angle;
        public int type;

        public void ReadData(IPacket packet)
        {
            angle = packet.ReadFloat();
            type = packet.ReadInt();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(angle);
            packet.Write(type);
        }
    }
    public class RadianceCross2 : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => true;

        public int type;

        public void ReadData(IPacket packet)
        {
            type = packet.ReadInt();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(type);
        }
    }
    public class RadianceCross3 : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => true;

        public int type;

        public void ReadData(IPacket packet)
        {
            type = packet.ReadInt();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(type);
        }
    }
    public class RadiancePillarShoot : IPacketData
    {
        public bool IsReliable => true;

        public bool DropReliableDataIfNewerExists => true;

        public bool right;

        public void ReadData(IPacket packet)
        {
            right = packet.ReadBool();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(right);
        }
    }
    public class Animation : IPacketData
    {
        public bool IsReliable => throw new NotImplementedException();

        public bool DropReliableDataIfNewerExists => throw new NotImplementedException();

        public string name;

        public void ReadData(IPacket packet)
        {
            name = packet.ReadString();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(name);
        }
    }
    public class PhaseIntro : IPacketData
    {
        public bool IsReliable => throw new NotImplementedException();

        public bool DropReliableDataIfNewerExists => throw new NotImplementedException();

        public int Phase;

        public void ReadData(IPacket packet)
        {
            Phase = packet.ReadInt();
        }

        public void WriteData(IPacket packet)
        {
            packet.Write(Phase);
        }
    }
}
