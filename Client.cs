using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hkmp.Api.Client;
using Hkmp.Api.Client.Networking;
using Hkmp.Networking;
using Hkmp.Networking.Packet;
using DreamChaser.Packets;

namespace DreamChaser
{
    public class Client : ClientAddon
    {
        public override bool NeedsNetwork => true;

        protected override string Name => "Dream Chaser Multiplayer";

        protected override string Version => "1.0.0.0";

        public IClientAddonNetworkReceiver<ClientPacketType> netReceiver;

        public IClientAddonNetworkSender<ServerPacketType> netSender;

        public override void Initialize(IClientApi clientApi)
        {
            netReceiver = clientApi.NetClient.GetNetworkReceiver<ClientPacketType>(this, InstantiatePacket);

            netSender = clientApi.NetClient.GetNetworkSender<ServerPacketType>(this);

            netReceiver.RegisterPacketHandler<SummonBoss>(ClientPacketType.SummonBoss, packet => DreamChaser.Summon(packet.pos.ToU3(), packet.Phase, packet.Scene));
            netReceiver.RegisterPacketHandler<KillBoss>(ClientPacketType.KillBoss, packet => ClientBoss.instance.Die());
            netReceiver.RegisterPacketHandler<BossPosition>(ClientPacketType.BossPosition, packet => ClientBoss.instance.SetPosition(packet.pos.ToU2()));
            netReceiver.RegisterPacketHandler<KillBoss>(ClientPacketType.WarpOut, packet => ClientBoss.instance.TeleOut());
            netReceiver.RegisterPacketHandler<KillBoss>(ClientPacketType.WarpIn, packet => ClientBoss.instance.TeleIn());
            netReceiver.RegisterPacketHandler<GorbNails>(ClientPacketType.GorbNails, packet => ClientBoss.instance.GorbNails(packet.Angle));
            netReceiver.RegisterPacketHandler<MarkothNailSpawn>(ClientPacketType.MarkothNailSpawn, packet => ClientBoss.instance.MarkothNailSpawn(packet.pos.ToU2(), packet.angle));
            netReceiver.RegisterPacketHandler<MarkothShield>(ClientPacketType.MarkothShield, packet => ClientBoss.instance.MarkothShield(packet.cw));
            netReceiver.RegisterPacketHandler<GrimmPillar>(ClientPacketType.GrimmPillar, packet => ClientBoss.instance.GrimmPillar(packet.pos.ToU2()));
            netReceiver.RegisterPacketHandler<RadianceCross1>(ClientPacketType.RadianceCross1, packet => ClientBoss.instance.RadianceBurst1(packet.angle, packet.type));
            netReceiver.RegisterPacketHandler<RadianceCross2>(ClientPacketType.RadianceCross2, packet => ClientBoss.instance.RadianceBurst2(packet.type));
            netReceiver.RegisterPacketHandler<RadianceCross3>(ClientPacketType.RadianceCross3, packet => ClientBoss.instance.RadianceBurst3(packet.type));
            netReceiver.RegisterPacketHandler<RadiancePillarShoot>(ClientPacketType.RadiancePillarShoot, packet => ClientBoss.instance.RadiancePillarShoot(packet.right));
            netReceiver.RegisterPacketHandler<Animation>(ClientPacketType.Animation, packet => ClientBoss.instance.Animation(packet.name));
            netReceiver.RegisterPacketHandler<PhaseIntro>(ClientPacketType.PhaseIntro, packet => ClientBoss.instance.StartCoroutine(ClientBoss.instance.ChangePhase(packet.Phase)));
        }

        public static void TryHandlePacket<T>(T packet, Action action) where T : IPacketData
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                DreamChaser.Instance.Log("Exception when handling packet, may be intended: " + e);
            }
        }

        public static IPacketData InstantiatePacket(ClientPacketType type)
        {
            switch (type)
            {
                case ClientPacketType.SummonBoss:
                    return new SummonBoss();
                case ClientPacketType.KillBoss:
                    return new KillBoss();
                case ClientPacketType.BossPosition:
                    return new BossPosition();
                case ClientPacketType.GorbNails:
                    return new GorbNails();
                case ClientPacketType.MarkothNailSpawn:
                    return new MarkothNailSpawn();
                case ClientPacketType.MarkothShield:
                    return new MarkothShield();
                case ClientPacketType.GrimmPillar:
                    return new GrimmPillar();
                case ClientPacketType.RadianceCross1:
                    return new RadianceCross1();
                case ClientPacketType.RadianceCross2:
                    return new RadianceCross2();
                case ClientPacketType.RadianceCross3:
                    return new RadianceCross3();
                case ClientPacketType.RadiancePillarShoot:
                    return new RadiancePillarShoot();
                case ClientPacketType.Animation:
                    return new Animation();
                case ClientPacketType.PhaseIntro:
                    return new PhaseIntro();
            }
            return null;
        }
    }
}
