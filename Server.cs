using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hkmp.Api.Server;
using Hkmp.Api.Server.Networking;
using Hkmp.Networking;
using Hkmp.Networking.Packet;
using DreamChaser.Packets;
using UnityEngine;

namespace DreamChaser
{
    public class Server : ServerAddon
    {
        public override bool NeedsNetwork => true;

        protected override string Name => "Dream Chaser Multiplayer";

        protected override string Version => "1.0.0.0";

        public IServerAddonNetworkReceiver<ServerPacketType> netReceiver;

        public IServerAddonNetworkSender<ClientPacketType> netSender;

        public override void Initialize(IServerApi serverApi)
        {
            netReceiver = serverApi.NetServer.GetNetworkReceiver<ServerPacketType>(this, _ => new DamagedBoss());

            netSender = serverApi.NetServer.GetNetworkSender<ClientPacketType>(this);

            netReceiver.RegisterPacketHandler<DamagedBoss>(ServerPacketType.DamagedBoss, (_, packet) => boss.Damage(packet.damage));

            serverApi.ServerManager.PlayerEnterSceneEvent += ServerManager_PlayerEnterSceneEvent;
            serverApi.ServerManager.PlayerLeaveSceneEvent += ServerManager_PlayerLeaveSceneEvent;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            DreamChaser.Instance.Log("Host player left scene " + arg0.name + " goinf to " + arg1.name);
            if (arg1.name == arg0.name)
                return;
            if (bossStarted && ServerApi.ServerManager.TryGetPlayer(0, out IServerPlayer obj))
            {
                if (arg0.name == boss.Scene)
                {
                    DreamChaser.Instance.Log(obj.Username);

                    boss.Targets.Remove(obj);

                    if (!ServerApi.ServerManager.Players.Any(p => p.CurrentScene == boss.Scene))
                    {
                        boss.Scene = ServerApi.ServerManager.Players.ElementAt(UnityEngine.Random.Range(0, ServerApi.ServerManager.Players.Count)).CurrentScene;
                        netSender.BroadcastSingleData(ClientPacketType.SummonBoss, new SummonBoss() { pos = boss.Position.ToH2(), Phase = boss.Phase, Scene = boss.Scene });
                        boss.Targets.Clear();
                        boss.Targets.AddRange(ServerApi.ServerManager.Players.Where(p => p.CurrentScene == boss.Scene));
                        boss.Target = boss.Targets.GetRandom();
                    }
                    else if (boss.Target == obj)
                        boss.Target = boss.Targets.GetRandom();
                }
                else if (arg1.name == boss.Scene)
                    boss.Targets.Add(obj);
            }
        }

        private void ServerManager_PlayerLeaveSceneEvent(IServerPlayer obj)
        {
            DreamChaser.Instance.Log(obj.Username + " left scene " + obj.CurrentScene);
            if (bossStarted)
            {
                boss.Targets.Remove(obj);

                if (!ServerApi.ServerManager.Players.Any(p => p.CurrentScene == boss.Scene))
                {
                    boss.Scene = ServerApi.ServerManager.Players.ElementAt(UnityEngine.Random.Range(0, ServerApi.ServerManager.Players.Count)).CurrentScene;
                    netSender.BroadcastSingleData(ClientPacketType.SummonBoss, new SummonBoss() { pos = boss.Position.ToH2(), Phase = boss.Phase, Scene = boss.Scene });
                    boss.Targets.Clear();
                    boss.Targets.AddRange(ServerApi.ServerManager.Players.Where(p => p.CurrentScene == boss.Scene));
                    boss.Target = boss.Targets.GetRandom();
                }
                else if (boss.Target == obj)
                    boss.Target = boss.Targets.GetRandom();
            }
        }

        private void ServerManager_PlayerEnterSceneEvent(IServerPlayer obj)
        {
            DreamChaser.Instance.Log(obj.Username + " entered scene " + obj.CurrentScene);
            if (bossStarted && obj.CurrentScene == boss.Scene)
            {
                boss.Targets.Add(obj);
            }
            else if (!bossStarted && obj.CurrentScene == "Town")
            {
                DreamChaser.Instance.Log("Spawning boss...");
                GameObject bossthing = new GameObject("ServerBoss");
                GameObject.DontDestroyOnLoad(bossthing);
                boss = bossthing.AddComponent<ServerBoss>();
                netSender.BroadcastSingleData(ClientPacketType.SummonBoss, new SummonBoss() { pos = boss.Position.ToH2(), Phase = boss.Phase, Scene = boss.Scene });
                boss.Targets.AddRange(ServerApi.ServerManager.Players.Where(p => p.CurrentScene == boss.Scene));
                boss.Target = boss.Targets.GetRandom();
                DreamChaser.Instance.Log("Spawned boss with target " + boss.Target.Username);
            }
        }

        public bool bossStarted = false;

        public ServerBoss boss;
    }
}
