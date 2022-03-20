using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using FrogCore.Fsm;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Hkmp.Api.Addon;

namespace DreamChaser
{
    public class DreamChaser : Mod
    {
        internal static DreamChaser Instance;

        public static GameObject GorbPref;
        public static GameObject MarkothPref;
        public static GameObject GrimmPillarPref;
        public static GameObject Radiance81Pref;
        public static GameObject Radiance82Pref;
        public static GameObject Radiance83Pref;
        public static GameObject RadiancePillarPref;
        public static GameObject ShieldPrefab;
        public static GameObject ShotMarkothNail;
        public static GameObject ShotSlugSpear;
        public static GameObject AudioPlayerActor;
        public static AudioClip mage_knight_projectile_shoot;
        public static AudioClip mage_knight_teleport;
        public static AudioClip mage_knight_sword;
        public static AudioClip mage_appear;
        public static GameObject Chaser;

        public static Client client;
        public static Server server;

        public override List<ValueTuple<string, string>> GetPreloadNames()
        {
            return new List<ValueTuple<string, string>>
            {
                new ValueTuple<string, string>("GG_Ghost_Gorb", "Warrior/Ghost Warrior Slug"),
                new ValueTuple<string, string>("GG_Ghost_Markoth", "Warrior/Ghost Warrior Markoth"),
                new ValueTuple<string, string>("GG_Grimm_Nightmare", "Grimm Control/Nightmare Grimm Boss"),
                new ValueTuple<string, string>("GG_Radiance", "Boss Control/Absolute Radiance"),
                new ValueTuple<string, string>("GG_Radiance", "Boss Control/Beam Sweeper"),
            };
        }

        public DreamChaser() : base("Dream Chaser")
        {
            Instance = this;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            GorbPref = preloadedObjects["GG_Ghost_Gorb"]["Warrior/Ghost Warrior Slug"];
            MarkothPref = preloadedObjects["GG_Ghost_Markoth"]["Warrior/Ghost Warrior Markoth"];
            GrimmPillarPref = preloadedObjects["GG_Grimm_Nightmare"]["Grimm Control/Nightmare Grimm Boss"].LocateMyFSM("Control").GetAction<SpawnObjectFromGlobalPool>("Pillar", 0).gameObject.Value;
            PlayMakerFSM radAttacks = preloadedObjects["GG_Radiance"]["Boss Control/Absolute Radiance"].LocateMyFSM("Attack Commands");
            Radiance81Pref = radAttacks.GetFsmGameObject("Eye Beam Burst1").Value;
            Radiance82Pref = radAttacks.GetFsmGameObject("Eye Beam Burst2").Value;
            Radiance83Pref = radAttacks.GetFsmGameObject("Eye Beam Burst3").Value;
            RadiancePillarPref = preloadedObjects["GG_Radiance"]["Boss Control/Beam Sweeper"].LocateMyFSM("Control").GetAction<SpawnObjectFromGlobalPoolOverTime>("Beam Sweep L", 4).gameObject.Value;
            ShieldPrefab = MarkothPref.LocateMyFSM("Shield Attack").GetAction<CreateObject>("Init", 1).gameObject.Value;
            ShotMarkothNail = MarkothPref.LocateMyFSM("Attacking").GetAction<SpawnObjectFromGlobalPool>("Nail", 0).gameObject.Value;
            FsmState Attack = GorbPref.LocateMyFSM("Attacking").GetState("Attack");
            ShotSlugSpear = Attack.GetAction<SpawnObjectFromGlobalPool>(6).gameObject.Value;
            AudioPlayerOneShotSingle Warp = GorbPref.LocateMyFSM("Movement").GetAction<AudioPlayerOneShotSingle>("Warp", 6);
            AudioPlayerActor = Warp.audioPlayer.Value;
            mage_knight_teleport = Warp.audioClip.Value as AudioClip;
            mage_knight_projectile_shoot = Attack.GetAction<AudioPlayerOneShotSingle>(2).audioClip.Value as AudioClip;
            FsmState Fire = ShotMarkothNail.LocateMyFSM("Control").GetState("Fire");
            mage_knight_sword = Fire.GetAction<AudioPlayerOneShot>(2).audioClips[0];
            mage_appear = Fire.GetAction<AudioPlayerOneShot>(3).audioClips[0];
            ShotMarkothNail = MarkothNail.MakeCustomNail(ShotMarkothNail);

            On.FSMUtility.SendEventToGameObject_GameObject_FsmEvent_bool += FSMUtility_SendEventToGameObject_GameObject_FsmEvent_bool;
            On.HealthManager.TakeDamage += HealthManager_TakeDamage;

            //ModHooks.CharmUpdateHook += ModHooks_CharmUpdateHook;
            client = new Client();
            server = new Server();
            Hkmp.Api.Client.ClientAddon.RegisterAddon(client);
            Hkmp.Api.Server.ServerAddon.RegisterAddon(server);

            Log("Initialized");
        }

        public static void Summon(Vector3 pos, int Phase, string Scene)
        {
            if (!Chaser && Scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                Chaser = GameObject.Instantiate(GorbPref, new Vector3(pos.x, pos.y, GorbPref.transform.position.z), Quaternion.Euler(0f, 0f, 0f));
                Chaser.AddComponent<ClientBoss>().Phase = Phase;
                Chaser.SetActive(true);
            }
        }

        private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (self.gameObject != Chaser)
            {
                orig(self, hitInstance);
            }
            else
            {
                client.netSender.SendSingleData(Packets.ServerPacketType.DamagedBoss, new Packets.DamagedBoss());
                self.GetComponent<IHitEffectReciever>()?.RecieveHitEffect(hitInstance.GetActualDirection(self.transform));
            }
            //self.gameObject.SendMessage("OnHMDamageTaken");
        }

        private void FSMUtility_SendEventToGameObject_GameObject_FsmEvent_bool(On.FSMUtility.orig_SendEventToGameObject_GameObject_FsmEvent_bool orig, GameObject go, HutongGames.PlayMaker.FsmEvent ev, bool isRecursive)
        {
            try
            {
                if (go && ev != null)
                    foreach (FromFsmBehaviour fsm in go.GetComponents<FromFsmBehaviour>())
                        fsm.SendEvent(ev.Name);
            }
            catch (Exception e)
            {
                Log(e);
                Log(ev);
                Log(go);
            }
            orig(go, ev, isRecursive);
        }
    }
}