using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FrogCore.Fsm;

namespace DreamChaser
{
    public class ClientBoss : MonoBehaviour // class receiving events from ServerBoss, together they make DreamChaser AI
    {
        public static ClientBoss instance;

        public Collider2D col2d;
        public MeshRenderer mr;
        public HealthManager hm;
        public tk2dSpriteAnimator anim;

        public GameObject Shield1;
        public GameObject EyeBeam1Clone;
        public GameObject EyeBeam2Clone;
        public GameObject EyeBeam3Clone;
        public GameObject WarpGO;
        public GameObject WhiteFlash;
        public ParticleSystem AttackPtParticleSystem;
        public GameObject Target;
        public RadiancePillar BeamSweeper;

        public int Phase = 1;

        public void Start()
        {
            instance = this;

            col2d = GetComponent<Collider2D>();
            mr = GetComponent<MeshRenderer>();
            hm = GetComponent<HealthManager>();
            anim = GetComponent<tk2dSpriteAnimator>();
            GetComponent<tk2dSprite>().color = Color.red;

            Target = HeroController.instance.gameObject;

            hm.hp = 999999999;
            hm.damageOverride = true;

            GameObject sweeper = new GameObject();
            BeamSweeper = sweeper.AddComponent<RadiancePillar>();
            BeamSweeper.rb2d = sweeper.AddComponent<Rigidbody2D>();
            BeamSweeper.rb2d.gravityScale = 0f;

            PlayMakerFSM Movement = gameObject.LocateMyFSM("Movement");
            AttackPtParticleSystem = Movement.GetFsmGameObject("Attack Pt").Value.GetComponent<ParticleSystem>();
            WarpGO = Movement.GetFsmGameObject("Warp").Value;
            WhiteFlash = Movement.GetFsmGameObject("White Flash").Value;

            foreach (PlayMakerFSM fsm in GetComponents<PlayMakerFSM>())
                Destroy(fsm);

            Destroy(GetComponent<Recoil>());

            Shield1 = GameObject.Instantiate(DreamChaser.ShieldPrefab, transform);
            Shield1.transform.localPosition = Vector3.zero;
            Shield1.LocateMyFSM("Control").ChangeTransition("Choose", "CCW", "Spinning CW");
            col2d.isTrigger = true;
            EyeBeam1Clone = GameObject.Instantiate(DreamChaser.Radiance81Pref, transform);
            EyeBeam1Clone.SetActive(true);
            EyeBeam1Clone.transform.localPosition = Vector3.zero;
            EyeBeam2Clone = GameObject.Instantiate(DreamChaser.Radiance82Pref, transform);
            EyeBeam2Clone.SetActive(true);
            EyeBeam2Clone.transform.localPosition = Vector3.zero;
            EyeBeam3Clone = GameObject.Instantiate(DreamChaser.Radiance83Pref, transform);
            EyeBeam3Clone.SetActive(true);
            EyeBeam3Clone.transform.localPosition = Vector3.zero;

            switch (Phase)
            {
                case 2:
                    FSMUtility.SendEventToGameObject(Shield1, "SUMMON SHIELD", false);
                    break;
                case 3:
                    break;
            }
        }

        public void Die() => hm.Die(null, AttackTypes.Nail, true);

        public void SetPosition(Vector2 pos) => transform.position = pos;

        public void TeleOut()
        {
            WarpGO.SetActive(true);
            WhiteFlash.SetActive(true);
            col2d.enabled = false;
            mr.enabled = false;
        }

        public void TeleIn()
        {
            WarpGO.SetActive(true);
            WhiteFlash.SetActive(true);
            col2d.enabled = true;
            mr.enabled = true;
        }

        public void GorbNails(float Angle)
        {
            DreamChaser.AudioPlayerActor.Spawn(transform.position).GetComponent<AudioSource>().PlayOneShot(DreamChaser.mage_knight_projectile_shoot);
            for (int i = 0; i < 8; i++)
            {
                DreamChaser.ShotSlugSpear.Spawn(transform.position, Quaternion.Euler(0f, 0f, Angle));
                Angle += 45f;
            }
        }

        public void MarkothNailSpawn(Vector2 pos, float angle)
        {
            GameObject nail = DreamChaser.ShotMarkothNail.Spawn(pos);
            MarkothNail mn = nail.GetComponent<MarkothNail>();
            mn.TelePos = pos;
            mn.angle = angle;
            nail.SetActive(true);
        }

        public void MarkothShield(bool cw)
        {
            FSMUtility.SendEventToGameObject(Shield1, cw ? "ATTACK CW" : "ATTACK CCW", false);
            // reset it, didn't want to go through syncing it all the time so this is the easiest way
            Shield1.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        public void GrimmPillar(Vector2 pos)
        {
            DreamChaser.GrimmPillarPref.Spawn(new Vector3(pos.x, pos.y, DreamChaser.GrimmPillarPref.transform.position.z));
        }

        public void RadianceBurst1(float angle, int type)
        {
            switch (type)
            {
                case 1:
                    EyeBeam1Clone.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    FSMUtility.SendEventToGameObject(EyeBeam1Clone, "ANTIC", true);
                    break;
                case 2:
                    EyeBeam2Clone.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    FSMUtility.SendEventToGameObject(EyeBeam2Clone, "ANTIC", true);
                    break;
                case 3:
                    EyeBeam3Clone.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    FSMUtility.SendEventToGameObject(EyeBeam3Clone, "ANTIC", true);
                    break;
            }
        }

        public void RadianceBurst2(int type)
        {
            switch (type)
            {
                case 1:
                    FSMUtility.SendEventToGameObject(EyeBeam1Clone, "FIRE", true);
                    break;
                case 2:
                    FSMUtility.SendEventToGameObject(EyeBeam2Clone, "FIRE", true);
                    break;
                case 3:
                    FSMUtility.SendEventToGameObject(EyeBeam3Clone, "FIRE", true);
                    break;
            }
            FSMUtility.SendEventToGameObject(GameCameras.instance.gameObject, "EnemyKillShake", true);
        }

        public void RadianceBurst3(int type)
        {
            switch (type)
            {
                case 1:
                    FSMUtility.SendEventToGameObject(EyeBeam1Clone, "END", true);
                    break;
                case 2:
                    FSMUtility.SendEventToGameObject(EyeBeam2Clone, "END", true);
                    break;
                case 3:
                    FSMUtility.SendEventToGameObject(EyeBeam3Clone, "END", true);
                    break;
            }
        }

        public void RadiancePillarShoot(bool right)
        {
            int mod = right ? 1 : -1;
            BeamSweeper.StopAllCoroutines();
            BeamSweeper.StartCoroutine(BeamSweeper.Shoot(new Vector3(transform.position.x + (20f * mod), transform.position.y - 20f, 0f), mod));
        }

        public void Animation(string name) => anim.Play(name);

        public IEnumerator ChangePhase(int Phase)
        {
            this.Phase = Phase;
            switch (Phase)
            {
                case 2:
                    yield return new WaitForSecondsRealtime(1f);
                    FSMUtility.SendEventToGameObject(Shield1, "SUMMON SHIELD", false);
                    AttackPtParticleSystem.Play();
                    yield return new WaitForSecondsRealtime(0.5f);
                    break;
                case 3:
                    break;
            }
        }
    }
}
