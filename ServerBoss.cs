using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DreamChaser.Packets;
using Hkmp.Api.Server;
using System.Collections;
using Animation = DreamChaser.Packets.Animation;

namespace DreamChaser
{
    public class ServerBoss : MonoBehaviour // can't rely on any fsm or unity stuff other than update
    {
        private bool deceleratingv2 = false;
        private float deceleratev2speed = 0f;

        private List<(IServerPlayer target, float speedMax, float accelerationForce, float offsetX, float offsetY)> chaseV2s = new();

        public static Vector3 P1 = new Vector2(0f, 7.5f);
        public static Vector3 P2 = new Vector2(0f, 2.5f);
        public static Vector3 P3 = new Vector2(-9.65f, 2.5f);
        public static Vector3 P4 = new Vector2(9.72f, 2.5f);
        public static Vector3 P5 = new Vector2(0f, 4.2f);
        public static Vector3 P6 = new Vector2(6.36f, 4.2f);
        public static Vector3 P7 = new Vector2(-6.26f, 4.2f);

        public string Scene = "Town";
        public List<IServerPlayer> Targets = new List<IServerPlayer>();
        public IServerPlayer Target;

        public Vector2 Position = Vector2.zero;
        public Vector2 Velocity = Vector2.zero;

        public float[] Weights = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        public int[] Ct = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        public int[] CtMax = new int[] { 1, 1, 1, 1, 1, 1, 1 };
        public int[] Ms = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        public int[] MsMax = new int[] { 7, 7, 7, 7, 7, 7, 7 };

        public int HP = 1200;
        public int HP1 = 1200;
        public int HP2 = 900;
        public int HP3 = 600;
        public float Angle;
        public int Phase = 1;
        public bool CanSpecialAttack = true;
        public bool UsingSpecialAttack = false;
        public int chaseIndex = -1;

        public Coroutine MovementCo;
        public Coroutine AttackCo;
		public Coroutine TargetChangeCo;
        public Coroutine RegenCo;
        public Coroutine MarkNailCo;
        public Coroutine Rad8Co;
        public Coroutine RadPillarCo;
        public Coroutine GorbAttackCo;
        public Coroutine GrimmPillarCo;

        public void Start()
        {
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
			AttackCo = StartCoroutine(NextFrame(AttackCycle()));
			TargetChangeCo = StartCoroutine(NextFrame(TargetChange()));
            RegenCo = StartCoroutine(NextFrame(Regen()));
        }

        private void FixedUpdate()
        {
            int length = chaseV2s.Count;
            for (int i = 0; i < length; i++)
                try { DoChaseForv2(chaseV2s[i]); } catch (Exception e) { Modding.Logger.Log(e); }
            if (deceleratingv2)
                try { Deceleratev2(); } catch (Exception e) { Modding.Logger.Log(e); }
        }

        public IEnumerator TargetChange()
		{
			yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(6f, 30f));
			Target = Targets.GetRandom();
			TargetChangeCo = StartCoroutine(NextFrame(TargetChange()));
        }

        public IEnumerator Regen()
        {
            yield return new WaitForSeconds(2f);
            if (HP < 1200)
                HP++;
        }

        public void ResetTools()
        {
            if (chaseIndex != -1)
            {
                StopChaseObjectv2(chaseIndex);
                chaseIndex = -1;
            }
            StopDeceleratev2();
        }

        public void Update()
        {
            Vector2 newPos = Position + (Velocity * Time.unscaledDeltaTime);
            if (newPos != Position)
            {
                Position = newPos;
                DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.BossPosition, new BossPosition() {pos = Position.ToH2()});
            }
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
            yield return new WaitForSeconds(UnityEngine.Random.Range(10f, 15f));
            if (!CanSpecialAttack)
                yield return new WaitUntil(() => CanSpecialAttack);
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
            UsingSpecialAttack = false;
            StopDeceleratev2();
            if (UnityEngine.Random.Range(0, 4) == 0)
            {
                DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.WarpOut, new WarpOut() { });
                CanSpecialAttack = false;
                Velocity = Vector3.zero;
                yield return new WaitForSeconds(1f);
                Position = (Target.Position.ToU3() + new Vector3(0, 7.5f, 0f));
                DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.WarpOut, new WarpOut() { });
                CanSpecialAttack = true;
                MovementChooseTarget();
                AttackCo = StartCoroutine(NextFrame(AttackCycle()));
            }
            else
            {
                MovementCo = StartCoroutine(NextFrame(MovementHover()));
                AttackCo = StartCoroutine(NextFrame(AttackCycle()));
            }
        }

        public void Damage(int damage)
        {
            HP -= damage;
            switch (Phase)
            {
                case 1:
                    if (HP <= HP2)
                    {
                        Phase = 2;
                        DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.PhaseIntro, new PhaseIntro() { Phase = 2 });
                    }
                    break;
                case 2:
                    if (HP <= HP3)
                    {
                        Phase = 3;
                        DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.PhaseIntro, new PhaseIntro() { Phase = 3 });
                    }
                    break;
            }
        }

        #region Movement

        public void MovementSet1()
        {
            Position = P1 + Target.Position.ToU3();
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet2()
        {
            Position = P2 + Target.Position.ToU3();
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet3()
        {
            Position = P3 + Target.Position.ToU3();
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet4()
        {
            Position = P4 + Target.Position.ToU3();
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet5()
        {
            Position = P5 + Target.Position.ToU3();
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet6()
        {
            Position = P6 + Target.Position.ToU3();
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
        }

        public void MovementSet7()
        {
            Position = P7 + Target.Position.ToU3();
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
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Idle" });
            float x = Position.x;
            float heroX = Target.Position.X;
            if (Mathf.Abs(heroX - x) > 20f)
            {
                DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.WarpOut, new WarpOut() { });
                CanSpecialAttack = false;
                Velocity = Vector3.zero;
                yield return new WaitForSeconds(1f);
                DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.WarpIn, new WarpIn() { });
                MovementChooseTarget();
            }
            else
            {
                chaseIndex = ChaseObjectv2((Target, 6f, 0.5f, 0.5f, 0f));
                yield return new WaitForSeconds(UnityEngine.Random.Range(4f, 5f));
                CanSpecialAttack = false;
                StopChaseObjectv2(chaseIndex);
                chaseIndex = -1;
                Velocity = Vector2.zero;
                DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.WarpOut, new WarpOut() { });
                yield return new WaitForSeconds(1f);
                DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.WarpIn, new WarpIn() { });
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
                DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.GrimmPillar, new GrimmPillar() { pos = Target.Position });

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
            float TeleX = Target.Position.X + UnityEngine.Random.Range(5f, 22f) - 11f;
            float TeleY = Target.Position.Y + UnityEngine.Random.Range(1f, 15f) - 5f;
            float angle = Mathf.Atan2(Target.Position.Y - 0.5f - TeleY, Target.Position.X - TeleX) * 57.295776f;
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.MarkothNailSpawn, new MarkothNailSpawn() { pos = new Hkmp.Math.Vector2(TeleX, TeleY), angle = angle});
            MarkNailCo = StartCoroutine(NextFrame(MarkothNailAttack()));
        }
        #endregion

        #region Radiance 8 Beams
        public void StartRad8Shoot() => Rad8Co = StartCoroutine(NextFrame(Radiance8Shoot()));

        public void StopRad8Shoot() => StopCoroutine(Rad8Co);

        public IEnumerator Radiance8Shoot()
        {
            yield return new WaitForSeconds(0.3f);

            float Rotation = UnityEngine.Random.Range(0f, 360f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.RadianceCross1, new RadianceCross1() { angle = Rotation, type = 1});
            yield return new WaitForSeconds(0.425f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.RadianceCross2, new RadianceCross2() { type = 1 });
            yield return new WaitForSeconds(0.35f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.RadianceCross3, new RadianceCross3() { type = 1 });
            yield return new WaitForSeconds(0.3f);

            Rotation += UnityEngine.Random.Range(10f, 30f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.RadianceCross1, new RadianceCross1() { angle = Rotation, type = 2 });
            yield return new WaitForSeconds(0.425f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.RadianceCross2, new RadianceCross2() { type = 2 });
            yield return new WaitForSeconds(0.35f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.RadianceCross3, new RadianceCross3() { type = 2 });
            yield return new WaitForSeconds(0.3f);

            Rotation += UnityEngine.Random.Range(10f, 30f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.RadianceCross1, new RadianceCross1() { angle = Rotation, type = 3 });
            yield return new WaitForSeconds(0.425f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.RadianceCross2, new RadianceCross2() { type = 3 });
            yield return new WaitForSeconds(0.35f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.RadianceCross3, new RadianceCross3() { type = 3 });
            yield return new WaitForSeconds(2f);

            Rad8Co = StartCoroutine(NextFrame(Radiance8Shoot()));
        }
        #endregion

        #region Radiance Pillar
        public void StartRadPillarShoot() => RadPillarCo = StartCoroutine(NextFrame(RadiancePillarShoot()));

        public void StopRadPillarShoot() => StopCoroutine(RadPillarCo);

        public IEnumerator RadiancePillarShoot()
        {
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.RadiancePillarShoot, new RadiancePillarShoot() { right = UnityEngine.Random.Range(0, 2) == 0 });
            yield return new WaitForSecondsRealtime(4f);
            RadPillarCo = StartCoroutine(NextFrame(RadiancePillarShoot()));
        }
        #endregion

        #region Gorb Attack
        public void StartGorbAttack() => StartCoroutine(NextFrame(GorbAttackWait()));

        public void StopGorbAttack() => StopCoroutine(GorbAttackCo);

        public IEnumerator GorbAttackWait()
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(1f, 2f));
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Attack Antic" });
            yield return new WaitForSecondsRealtime(3f/12f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Attack" });
            Angle = UnityEngine.Random.Range(0f, 360f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.GorbNails, new GorbNails() { Angle = Angle });
            if (Phase >= 2)
            {
                DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Idle" });
                yield return new WaitForSecondsRealtime(0.25f);
                Angle += 22.5f;
                DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.GorbNails, new GorbNails() { Angle = Angle });
                if (Phase == 3)
                {
                    DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Attack" });
                    yield return new WaitForSecondsRealtime(5f/12f);
                    DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Idle" });
                    yield return new WaitForSecondsRealtime(0.1f);
                    Angle += 22.5f;
                    DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.GorbNails, new GorbNails() { Angle = Angle });
                }
            }
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Idle" });
            yield return new WaitForSecondsRealtime(0.5f);
            GorbAttackCo = StartCoroutine(NextFrame(GorbAttackWait()));
        }
        #endregion

        #region Markoth Shield
        public IEnumerator StartRageShieldPhase()
        {
            if (UsingSpecialAttack || !CanSpecialAttack)
                yield return new WaitWhile(() => UsingSpecialAttack || !CanSpecialAttack);
            if (MovementCo != null)
                StopCoroutine(MovementCo);
            if (AttackCo != null)
                StopCoroutine(AttackCo);
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
            ResetTools();
            Velocity = Vector2.zero;
            StartCoroutine(NextFrame(MarkothShieldRageAnim()));
        }

        public IEnumerator MarkothShieldRageAnim()
        {
            UsingSpecialAttack = true;
            yield return new WaitForSeconds(0.5f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Attack" });
            yield return new WaitForSecondsRealtime(1f);
            UsingSpecialAttack = false;
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Attack End" });
            MovementCo = StartCoroutine(NextFrame(MovementHover()));
            AttackCo = StartCoroutine(NextFrame(AttackCycle()));
        }

        public IEnumerator MarkothShieldReady()
        {
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Attack" });
            yield return new WaitForSecondsRealtime(0.75f);
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.MarkothShield, new MarkothShield() { cw = UnityEngine.Random.Range(0, 2) == 0 });
            DreamChaser.server.netSender.BroadcastSingleData(ClientPacketType.Animation, new Animation() { name = "Attack End" });
            yield return new WaitForSecondsRealtime(5f); // originally 4.5, but I think markoth ends eary
        }
        #endregion

        #region Tools
        #region "Fsm"
        public IEnumerator NextFrame(IEnumerator enumerator)
		{
			yield return null;
			yield return enumerator;
		}
		#endregion

		#region ChaseObject
		private void DoChaseForv2((IServerPlayer target, float speedMax, float accelerationForce, float offsetX, float offsetY) settings)
		{
			Vector3 oPosition = Position;
			Vector3 tPosition = settings.target.Position.ToU3();
			Vector2 vector = Vector2.ClampMagnitude(new Vector2(tPosition.x + settings.offsetX - oPosition.x, tPosition.y + settings.offsetY - oPosition.y), 1f);
			vector = new Vector2(vector.x * settings.accelerationForce, vector.y * settings.accelerationForce);
			Velocity += vector;
			Velocity = Vector2.ClampMagnitude(Velocity, settings.speedMax) * Time.fixedUnscaledDeltaTime;
		}

		public int ChaseObjectv2((IServerPlayer target, float speedMax, float accelerationForce, float offsetX, float offsetY) settings)
		{
			int index = chaseV2s.Count;
			chaseV2s.Add(settings);
			return index;
		}

		public void StopChaseObjectv2(int index)
		{
			chaseV2s.RemoveAt(index);
		}
		#endregion

		#region SendRandomEvent
		public static int GetRandomWeightedIndex(float[] weights)
		{
			float num = 0f;
			foreach (float fsmFloat in weights)
			{
				num += fsmFloat;
			}
			float num2 = UnityEngine.Random.Range(0f, num);
			for (int j = 0; j < weights.Length; j++)
			{
				if (num2 < weights[j])
				{
					return j;
				}
				num2 -= weights[j];
			}
			return -1;
		}

		public int SendRandomEventv3(float[] weights, int[] trackingInts, int[] eventMax, int[] trackingIntsMissed, int[] missedMax)
		{
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			int loops = 0;
			while (!flag)
			{
				int randomWeightedIndex = GetRandomWeightedIndex(weights);
				if (randomWeightedIndex != -1)
				{
					for (int i = 0; i < trackingIntsMissed.Length; i++)
					{
						if (trackingIntsMissed[i] >= missedMax[i])
						{
							flag2 = true;
							num = i;
						}
					}
					if (flag2)
					{
						flag = true;
						for (int j = 0; j < trackingInts.Length; j++)
						{
							trackingInts[j] = 0;
							trackingIntsMissed[j]++;
						}
						trackingIntsMissed[num] = 0;
						trackingInts[num] = 1;
					}
					else if (trackingInts[randomWeightedIndex] < eventMax[randomWeightedIndex])
					{
						int value = ++trackingInts[randomWeightedIndex];
						for (int k = 0; k < trackingInts.Length; k++)
						{
							trackingInts[k] = 0;
							trackingIntsMissed[k]++;
						}
						trackingInts[randomWeightedIndex] = value;
						trackingIntsMissed[randomWeightedIndex] = 0;
						flag = true;
					}
				}
				loops++;
				if (loops > 100)
				{
					return 0;
				}
			}
			return num;
		}
		#endregion

		#region Decelerate
		public void StartDeceleratev2(float deceleration)
		{
			deceleratingv2 = true;
			deceleratev2speed = deceleration;
			Deceleratev2();
		}
		public void StopDeceleratev2() => deceleratingv2 = false;
		private void Deceleratev2()
		{
			if (Velocity.x < 0f)
			{
				Velocity.x *= deceleratev2speed * Time.fixedUnscaledDeltaTime;
				if (Velocity.x > 0f)
				{
					Velocity.x = 0f;
				}
			}
			else if (Velocity.x > 0f)
			{
				Velocity.x *= deceleratev2speed * Time.fixedUnscaledDeltaTime;
				if (Velocity.x < 0f)
				{
					Velocity.x = 0f;
				}
			}
			if (Velocity.y < 0f)
			{
				Velocity.y *= deceleratev2speed * Time.fixedUnscaledDeltaTime;
				if (Velocity.y > 0f)
				{
					Velocity.y = 0f;
				}
			}
			else if (Velocity.y > 0f)
			{
				Velocity.y *= deceleratev2speed * Time.fixedUnscaledDeltaTime;
				if (Velocity.y < 0f)
				{
					Velocity.y = 0f;
				}
			}
		}
        #endregion
        #endregion
    }
}
