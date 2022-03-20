using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using FrogCore.Fsm;
using Modding;

namespace DreamChaser.Singleplayer
{
    public class DreamChaserAI : FromFsmBehaviour
    {
        public static void Log(object o) => DreamChaser.Instance.Log(o);
        #region Fields
        public static DreamChaserAI instance;

        public static Vector3 P1 = new Vector2(0f, 7.5f);
        public static Vector3 P2 = new Vector2(0f, 2.5f);
        public static Vector3 P3 = new Vector2(-9.65f, 2.5f);
        public static Vector3 P4 = new Vector2(9.72f, 2.5f);
        public static Vector3 P5 = new Vector2(0f, 4.2f);
        public static Vector3 P6 = new Vector2(6.36f, 4.2f);
        public static Vector3 P7 = new Vector2(-6.26f, 4.2f);

        public float[] Weights = new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f};
        public int[] Ct = new int[] {0, 0, 0, 0, 0, 0, 0};
        public int[] CtMax = new int[] {1, 1, 1, 1, 1, 1, 1};
        public int[] Ms = new int[] {0, 0, 0, 0, 0, 0, 0};
        public int[] MsMax = new int[] {7, 7, 7, 7, 7, 7, 7};

        public Rigidbody2D rb2d;
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
        public Rigidbody2D BeamSweeperRb2d;

        public int HP1 = 1200;
        public int HP2 = 900;
        public int HP3 = 600;
        public float Angle;
        public int Phase = 1;
        public bool CanSpecialAttack = true;
        public bool UsingSpecialAttack = false;
        public int faceIndex = -1;
        public int chaseIndex = -1;

        public Coroutine MovementCo;
        public Coroutine AttackCo;
        public Coroutine MarkNailCo;
        public Coroutine Rad8Co;
        public Coroutine RadPillarCo;
        public Coroutine GorbAttackCo;
        public Coroutine GrimmPillarCo;
        #endregion

        #region Misc
        public void Start()
        {
            instance = this;

            rb2d = GetComponent<Rigidbody2D>();
            col2d = GetComponent<Collider2D>();
            mr = GetComponent<MeshRenderer>();
            hm = GetComponent<HealthManager>();
            anim = GetComponent<tk2dSpriteAnimator>();

            Target = HeroController.instance.gameObject;

            hm.hp = HP1;

            BeamSweeperRb2d = new GameObject().AddComponent<Rigidbody2D>();

            PlayMakerFSM Movement = gameObject.LocateMyFSM("Movement");
            AttackPtParticleSystem = Movement.GetFsmGameObject("Attack Pt").Value.GetComponent<ParticleSystem>();
            WarpGO = Movement.GetFsmGameObject("Warp").Value;
            WhiteFlash = Movement.GetFsmGameObject("White Flash").Value;

            foreach (PlayMakerFSM fsm in GetComponents<PlayMakerFSM>())
                Destroy(fsm);

            Shield1 = GameObject.Instantiate(DreamChaser.ShieldPrefab, transform);
            Shield1.transform.localPosition = Vector3.zero;
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
            MovementCo = StartCoroutine(NextFrame(MovementWarpIn()));
            AttackCo = StartCoroutine(NextFrame(AttackCycle()));
        }

        public IEnumerator AttackCycle()
        {
            switch (Phase)
            {
                case 1:
                case 2:
                    switch (UnityEngine.Random.Range(0, 2))
                    {
                        case 0:
                            StartGorbAttack();
                            break;
                        case 1:
                            StartMarkNailAttack();
                            break;
                    }
                    break;
                case 3:
                    switch (UnityEngine.Random.Range(0, 3))
                    {
                        case 0:
                            StartMarkNailAttack();
                            StartGrimmShoot();
                            break;
                        case 1:
                            StartGorbAttack();
                            StartGrimmShoot();
                            break;
                        case 2:
                            StartGorbAttack();
                            StartMarkNailAttack();
                            break;
                    }
                    break;
            }
            Log("Attack cycle wait");
            yield return new WaitForSeconds(UnityEngine.Random.Range(10f, 15f));
            Log("Attack cycle wait 2");
            if (!CanSpecialAttack)
                yield return new WaitUntil(() => CanSpecialAttack);
            Log("Attack cycle special start");
            UsingSpecialAttack = true;
            if (MovementCo != null)
                StopCoroutine(MovementCo);
            if (MarkNailCo != null)
                StopMarkNailAttack();
            if (Rad8Co != null)
                StopRad8Shoot();
            if (RadPillarCo != null)
                StopRadPillarShoot();
            if (GorbAttackCo != null)
                StopGorbAttack();
            if (GrimmPillarCo != null)
                StopGrimmShoot();
            // special attack start
            ResetTools();
            StartDeceleratev2(0.9f);
            switch (Phase)
            {
                case 1:
                    yield return MarkothShieldReady();
                    break;
                case 2:
                    switch (UnityEngine.Random.Range(0, 2))
                    {
                        case 0:
                            StartGorbAttack();
                            break;
                        case 1:
                            StartMarkNailAttack();
                            break;
                    }
                    switch (UnityEngine.Random.Range(0, 2))
                    {
                        case 0:
                            yield return MarkothShieldReady();
                            break;
                        case 1:
                            yield return GrimmShoot();
                            break;
                    }
                    break;
                case 3:
                    switch (UnityEngine.Random.Range(0, 3))
                    {
                        case 0:
                            StartGorbAttack();
                            break;
                        case 1:
                            StartMarkNailAttack();
                            break;
                        case 2:
                            StartGrimmShoot();
                            break;
                    }
                    switch (UnityEngine.Random.Range(0, 3))
                    {
                        case 0:
                            yield return MarkothShieldReady();
                            break;
                        case 1:
                            yield return Radiance8Shoot();
                            break;
                        case 2:
                            yield return RadiancePillarShoot();
                            break;
                    }
                    break;
            }
            if (MovementCo != null)
                StopCoroutine(MovementCo);
            if (MarkNailCo != null)
                StopMarkNailAttack();
            if (Rad8Co != null)
                StopRad8Shoot();
            if (RadPillarCo != null)
                StopRadPillarShoot();
            if (GorbAttackCo != null)
                StopGorbAttack();
            if (GrimmPillarCo != null)
                StopGrimmShoot();
            Log("Attack cycle done special attack");
            UsingSpecialAttack = false;
            StopDeceleratev2();
            if (UnityEngine.Random.Range(0, 4) == 0)
            {
                FaceObject(gameObject, Target, false, true, "TurnToIdle", true, false, anim);
                WarpGO.SetActive(true);
                WhiteFlash.SetActive(true);
                CanSpecialAttack = false;
                rb2d.velocity = Vector3.zero;
                col2d.enabled = false;
                mr.enabled = false;
                yield return new WaitForSeconds(1f);
                transform.position = Target.transform.position + new Vector3(0, 7.5f, 0f);
                WarpGO.SetActive(true);
                WhiteFlash.SetActive(true);
                CanSpecialAttack = true;
                col2d.enabled = true;
                mr.enabled = true;
                MovementChooseTarget();
                AttackCo = StartCoroutine(NextFrame(AttackCycle()));
            }
            else
            {
                MovementCo = StartCoroutine(NextFrame(MovementHover()));
                AttackCo = StartCoroutine(NextFrame(AttackCycle()));
            }
        }

        public void OnHMDamageTaken()
        {
            switch (Phase)
            {
                case 1:
                    if (hm.hp <= HP2)
                    {
                        Phase = 2;
                        StartCoroutine(NextFrame(StartRageShieldPhase()));
                    }
                    break;
                case 2:
                    if (hm.hp <= HP3)
                        Phase = 3;
                    break;
            }
        }

        public void ResetTools()
        {
            if (faceIndex != -1)
            {
                StopFaceObject(faceIndex);
                faceIndex = -1;
            }
            if (chaseIndex != -1)
            {
                StopChaseObjectv2(chaseIndex);
                chaseIndex = -1;
            }
            StopDeceleratev2();
        }
        #endregion

        #region Movement
        public IEnumerator MovementWarpIn()
        {
            WarpGO.SetActive(true);
            WhiteFlash.SetActive(true);
            AttackPtParticleSystem.Play();
            yield return new WaitForSeconds(1f);
            switch (UnityEngine.Random.Range(0, 3))
            {
                case 0:
                    MovementSet5();
                    break;
                case 2:
                    MovementSet6();
                    break;
                case 3:
                    MovementSet7();
                    break;
            }
        }

        public void MovementSet1()
        {
            transform.position = P1 + Target.transform.position;
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet2()
        {
            transform.position = P2 + Target.transform.position;
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet3()
        {
            transform.position = P3 + Target.transform.position;
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet4()
        {
            transform.position = P4 + Target.transform.position;
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet5()
        {
            transform.position = P5 + Target.transform.position;
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet6()
        {
            transform.position = P6 + Target.transform.position;
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet7()
        {
            transform.position = P7 + Target.transform.position;
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementChooseTarget()
        {
            switch (SendRandomEventv3(Weights, Ct, CtMax, Ms, MsMax))
            {
                case 0:
                    MovementSet1();
                    break;
                case 1:
                    MovementSet2();
                    break;
                case 2:
                    MovementSet3();
                    break;
                case 3:
                    MovementSet4();
                    break;
                case 4:
                    MovementSet5();
                    break;
                case 5:
                    MovementSet6();
                    break;
                case 6:
                    MovementSet7();
                    break;
            }
        }

        public IEnumerator MovementHover()
        {
            CanSpecialAttack = true;
            anim.Play("Idle");
            float x = transform.position.x;
            float heroX = Target.transform.position.x;
            if (Mathf.Abs(heroX - x) > 20f)
            {
                FaceObject(gameObject, Target, false, true, "TurnToIdle", true, false, anim);
                WarpGO.SetActive(true);
                WhiteFlash.SetActive(true);
                CanSpecialAttack = false;
                rb2d.velocity = Vector3.zero;
                col2d.enabled = false;
                mr.enabled = false;
                yield return new WaitForSeconds(1f);
                WarpGO.SetActive(true);
                WhiteFlash.SetActive(true);
                col2d.enabled = true;
                mr.enabled = true;
                MovementChooseTarget();
            }
            else
            {
                faceIndex = FaceObject(gameObject, Target, true, true, "TurnToIdle", true, true, anim);
                chaseIndex = ChaseObjectv2((rb2d, Target, 6f, 0.5f, 0.5f, 0f));
                yield return new WaitForSeconds(UnityEngine.Random.Range(4f, 5f));
                CanSpecialAttack = false;
                StopChaseObjectv2(chaseIndex);
                StopFaceObject(faceIndex);
                faceIndex = -1;
                chaseIndex = -1;
                WarpGO.SetActive(true);
                WhiteFlash.SetActive(true);
                CanSpecialAttack = false;
                rb2d.velocity = Vector3.zero;
                col2d.enabled = false;
                mr.enabled = false;
                yield return new WaitForSeconds(1f);
                WarpGO.SetActive(true);
                WhiteFlash.SetActive(true);
                col2d.enabled = true;
                mr.enabled = true;
                MovementChooseTarget();
            }
        }
        #endregion

        #region Grimm Pillar
        public void StartGrimmShoot() => GrimmPillarCo = StartCoroutine(NextFrame(GrimmShoot()));

        public void StopGrimmShoot() => StopCoroutine(GrimmPillarCo);

        public IEnumerator GrimmShoot()
        {
            yield return new WaitForSecondsRealtime(0.5f);

            for (int i = 4; i > 0; i--)
            {
                DreamChaser.GrimmPillarPref.Spawn(new Vector3(Target.transform.position.x, Target.transform.position.y, DreamChaser.GrimmPillarPref.transform.position.z));

                yield return new WaitForSecondsRealtime(0.75f);
            }

            GrimmPillarCo = StartCoroutine(NextFrame(GrimmShoot()));
        }
        #endregion

        #region Markoth Nail
        public void StartMarkNailAttack() => MarkNailCo = StartCoroutine(NextFrame(MarkothNailAttack()));

        public void StopMarkNailAttack() => StopCoroutine(MarkNailCo);

        public IEnumerator MarkothNailAttack()
        {
            switch (Phase)
            {
                case 1:
                    yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(1f, 1.75f));
                    break;
                case 2:
                    yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.8f, 1.25f));
                    break;
                case 3:
                    yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.4f, 0.8f));
                    break;
            }
            DreamChaser.ShotMarkothNail.Spawn().SetActive(true);
            MarkNailCo = StartCoroutine(NextFrame(MarkothNailAttack()));
        }
        #endregion

        #region Radiance 8 Beams
        public void StartRad8Shoot() => Rad8Co = StartCoroutine(NextFrame(Radiance8Shoot()));

        public void StopRad8Shoot() => StopCoroutine(Rad8Co);

        public IEnumerator Radiance8Shoot()
        {
            yield return new WaitForSeconds(0.3f);

            //DreamChaser.Radiance8Pref.Spawn(transform.position);
            float Rotation = UnityEngine.Random.Range(0f, 360f);
            EyeBeam1Clone.transform.rotation = Quaternion.Euler(0f, 0f, Rotation);
            BroadcastEventToGameObject(EyeBeam1Clone, "ANTIC", true);
            yield return new WaitForSeconds(0.425f);
            BroadcastEventToGameObject(EyeBeam1Clone, "FIRE", true);
            BroadcastEventToGameObject(GameCameras.instance.gameObject, "EnemyKillShake", true);
            yield return new WaitForSeconds(0.35f);
            BroadcastEventToGameObject(EyeBeam1Clone, "END", true);
            yield return new WaitForSeconds(0.3f);

            Rotation += UnityEngine.Random.Range(10f, 30f);
            EyeBeam2Clone.transform.rotation = Quaternion.Euler(0f, 0f, Rotation);
            BroadcastEventToGameObject(EyeBeam2Clone, "ANTIC", true);
            yield return new WaitForSeconds(0.425f);
            BroadcastEventToGameObject(EyeBeam2Clone, "FIRE", true);
            BroadcastEventToGameObject(GameCameras.instance.gameObject, "EnemyKillShake", true);
            yield return new WaitForSeconds(0.35f);
            BroadcastEventToGameObject(EyeBeam2Clone, "END", true);
            yield return new WaitForSeconds(0.3f);

            Rotation += UnityEngine.Random.Range(10f, 30f);
            EyeBeam1Clone.transform.rotation = Quaternion.Euler(0f, 0f, Rotation);
            BroadcastEventToGameObject(EyeBeam1Clone, "ANTIC", true);
            yield return new WaitForSeconds(0.425f);
            BroadcastEventToGameObject(EyeBeam1Clone, "FIRE", true);
            BroadcastEventToGameObject(GameCameras.instance.gameObject, "EnemyKillShake", true);
            yield return new WaitForSeconds(0.35f);
            BroadcastEventToGameObject(EyeBeam1Clone, "END", true);
            yield return new WaitForSeconds(2f);

            Rad8Co = StartCoroutine(NextFrame(Radiance8Shoot()));
        }
        #endregion

        #region Radiance Pillar
        public void StartRadPillarShoot() => RadPillarCo = StartCoroutine(NextFrame(RadiancePillarShoot()));

        public void StopRadPillarShoot() => StopCoroutine(RadPillarCo);

        public IEnumerator RadiancePillarShoot()
        {
            int mod = UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
            BeamSweeperRb2d.transform.position = new Vector3(transform.position.x + (20f * mod), transform.position.y - 20f, 0f);
            BeamSweeperRb2d.drag = 0f;
            BeamSweeperRb2d.gravityScale = 0f;
            BeamSweeperRb2d.velocity = new Vector2(-10f * mod, 0f);
            if (mod == 1)
            {
                float targetx = transform.position.x - 20f;
                while (BeamSweeperRb2d.transform.position.x > targetx)
                {
                    yield return new WaitForSeconds(0.075f); 
                    DreamChaser.RadiancePillarPref.Spawn(BeamSweeperRb2d.transform.position, Quaternion.Euler(0f, 0f, 90f));
                }
            }
            else
            {
                float targetx = transform.position.x + 20f;
                while (BeamSweeperRb2d.transform.position.x < targetx)
                {
                    yield return new WaitForSeconds(0.075f);
                    DreamChaser.RadiancePillarPref.Spawn(BeamSweeperRb2d.transform.position, Quaternion.Euler(0f, 0f, 90f));
                }
            }
            RadPillarCo = StartCoroutine(NextFrame(RadiancePillarShoot()));
        }
        #endregion

        #region Gorb Attack
        public void StartGorbAttack() => StartCoroutine(NextFrame(GorbAttackWait()));

        public void StopGorbAttack() => StopCoroutine(GorbAttackCo);

        public IEnumerator GorbAttackWait()
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(1f, 2f));
            yield return anim.PlayAnimWait("Attack Antic");
            DreamChaser.AudioPlayerActor.Spawn(transform.position).GetComponent<AudioSource>().PlayOneShot(DreamChaser.mage_knight_projectile_shoot);
            AttackPtParticleSystem.Play();
            anim.Play("Attack");
            Angle = UnityEngine.Random.Range(0f, 360f);
            SpearCircle();
            if (Phase >= 2)
            {
                anim.Play("Idle");
                yield return new WaitForSecondsRealtime(0.25f);
                DreamChaser.AudioPlayerActor.Spawn(transform.position).GetComponent<AudioSource>().PlayOneShot(DreamChaser.mage_knight_projectile_shoot);
                Angle += 22.5f;
                SpearCircle();
                if (Phase == 3)
                {
                    yield return anim.PlayAnimWait("Attack");
                    anim.Play("Idle");
                    yield return new WaitForSecondsRealtime(0.1f);
                    DreamChaser.AudioPlayerActor.Spawn(transform.position).GetComponent<AudioSource>().PlayOneShot(DreamChaser.mage_knight_projectile_shoot);
                    Angle += 22.5f;
                    SpearCircle();
                }
            }
            anim.Play("Idle");
            yield return new WaitForSecondsRealtime(0.5f);
            GorbAttackCo = StartCoroutine(NextFrame(GorbAttackWait()));
        }

        public void SpearCircle()
        {
            for (int i = 0; i < 8; i++)
            {
                DreamChaser.ShotSlugSpear.Spawn(transform.position, Quaternion.Euler(0f, 0f, Angle));
                Angle += 45f;
            }
        }
        #endregion

        #region Markoth Shield
        public IEnumerator StartRageShieldPhase()
        {
            if (UsingSpecialAttack || !CanSpecialAttack)
                yield return new WaitWhile(() => UsingSpecialAttack || !CanSpecialAttack);
            StopAllCoroutines();
            ResetTools();
            rb2d.velocity = Vector2.zero;
            StartCoroutine(NextFrame(MarkothShieldRageAnim()));
        }

        public IEnumerator MarkothShieldRageAnim()
        {
            UsingSpecialAttack = true;
            yield return new WaitForSeconds(0.5f);
            anim.Play("Attack");
            yield return new WaitForSecondsRealtime(0.5f);
            BroadcastEventToGameObject(Shield1, "SUMMON SHIELD", false);
            AttackPtParticleSystem.Play();
            yield return new WaitForSecondsRealtime(0.5f);
            UsingSpecialAttack = false;
            anim.Play("Attack End");
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
            AttackCo = StartCoroutine(NextFrame(AttackCycle()));
        }

        public IEnumerator MarkothShieldReady()
        {
            Log("Mark Shield Ready begin");
            anim.Play("Attack");
            yield return new WaitForSecondsRealtime(0.75f);
            Log("Mark Shield Ready 2");
            BroadcastEventToGameObject(Shield1, UnityEngine.Random.Range(0, 2) == 0 ? "ATTACK CW" : "ATTACK CCW", false);
            anim.Play("Attack End");
            yield return new WaitForSecondsRealtime(5f); // originally 4.5, but I think markoth ends eary
            Log("Mark Shield Ready end");
        }
        #endregion
    }
}