using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DreamChaser
{
    public class FromFsmBehaviour : MonoBehaviour
    {
		private bool deceleratingv2 = false;
		private float deceleratev2speed = 0f;

		private List<(Rigidbody2D rb2d, GameObject target, float speedMax, float accelerationForce, float offsetX, float offsetY)> chaseV2s = new();

		private List<(GameObject A, GameObject B, bool spriteFacesRight, bool playNewAnimation, string newAnimationClip, bool resetFrame, float xScale, tk2dSpriteAnimator _sprite)> faceObjects = new();

		protected string lastEvent = "";
		private bool usedLastEvent = false;

		public Rigidbody2D MyRigidbody;

		public void Awake()
        {
			MyRigidbody = GetComponent<Rigidbody2D>();
        }

		private void FixedUpdate()
        {
			int length = chaseV2s.Count;
			for (int i = 0; i < length; i++)
				try { DoChaseForv2(chaseV2s[i]); } catch (Exception e) { Modding.Logger.Log(e); }
			if (deceleratingv2)
				try { Deceleratev2(); } catch (Exception e) { Modding.Logger.Log(e); }
        }

		private void Update()
        {
			int length = faceObjects.Count;
			for (int i = 0; i < length; i++)
				try
				{
					var settings = faceObjects[i];
					DoFace(settings.A, settings.B, settings.spriteFacesRight, settings.playNewAnimation, settings.newAnimationClip, settings.resetFrame, settings.xScale, settings._sprite);
				}
				catch (Exception e) { Modding.Logger.Log(e); }
		}

        #region "Fsm"
        public IEnumerator WaitForEvent(params string[] names)
        {
            while(!usedLastEvent && !names.Contains(lastEvent))
                yield return null;
			usedLastEvent = true;
		}
		
		public IEnumerator NextFrame(IEnumerator enumerator)
        {
			yield return null;
			yield return enumerator;
        }

		public Coroutine AddEventListener(Action action, params string[] names)
		{
			IEnumerator Wait()
			{
				while (!usedLastEvent || !names.Contains(lastEvent))
					yield return null;
				usedLastEvent = true;
				action();
			}
			return StartCoroutine(Wait());
		}

		public void SendEvent(string name)
		{
			lastEvent = name;
			usedLastEvent = false;
		}

		public GameObject GetOwnerDefaultTarget(FsmOwnerDefault ownerDefault)
		{
			if (ownerDefault == null)
			{
				return null;
			}
			if (ownerDefault.OwnerOption != OwnerDefaultOption.UseOwner)
			{
				return ownerDefault.GameObject.Value;
			}
			return gameObject;
		}

		public void BroadcastEventToGameObject(GameObject go, string fsmEvent, bool sendToChildren)
		{
			if (go == null)
			{
				return;
			}
			/*List<Fsm> list = new List<Fsm>();
			foreach (PlayMakerFSM playMakerFSM in PlayMakerFSM.FsmList)
			{
				if (playMakerFSM != null && playMakerFSM.gameObject == go)
				{
					list.Add(playMakerFSM.Fsm);
				}
			}
			foreach (Fsm fsm in list)
			{
				fsm.ProcessEvent(fsmEvent);
			}*/
			foreach (PlayMakerFSM fsm in go.GetComponents<PlayMakerFSM>())
				fsm.SendEvent(fsmEvent);
			foreach (FromFsmBehaviour fromFsmBehaviour in go.GetComponents<FromFsmBehaviour>())
				fromFsmBehaviour.SendEvent(fsmEvent);
			if (sendToChildren)
			{
				for (int i = 0; i < go.transform.childCount; i++)
				{
					BroadcastEventToGameObject(go.transform.GetChild(i).gameObject, fsmEvent, true);
				}
			}
		}
		#endregion

		#region iTween
		private void iTweenMoveBy(GameObject go, Vector3? vector, float? speed, float? time, float? delay, iTween.EaseType easeType, iTween.LoopType loopType, int itweenID, bool? realTime, Space space, string id /*= ""*/, iTweenFsmAction.AxisRestriction axis, bool? orientToPath, GameObject lookAtObject, Vector3? lookAtVector, float? lookTime, string startEvent, string finishEvent)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("amount", !vector.HasValue ? Vector3.zero : vector.Value);
			hashtable.Add(!speed.HasValue ? "time" : "speed", !speed.HasValue ? (!time.HasValue ? 1f : time.Value) : speed.Value);
			hashtable.Add("delay", !delay.HasValue ? 0f : delay.Value);
			hashtable.Add("easetype", easeType);
			hashtable.Add("looptype", loopType);
			hashtable.Add("oncomplete", "SendEvent");
			hashtable.Add("oncompleteparams", finishEvent);
			hashtable.Add("onstart", "SendEvent");
			hashtable.Add("onstartparams", startEvent);
			hashtable.Add("ignoretimescale", realTime.HasValue && realTime.Value);
			hashtable.Add("space", space);
			hashtable.Add("name", id);
			hashtable.Add("axis", (axis == iTweenFsmAction.AxisRestriction.none) ? "" : Enum.GetName(typeof(iTweenFsmAction.AxisRestriction), axis));
			if (orientToPath.HasValue)
			{
				hashtable.Add("orienttopath", orientToPath.Value);
			}
			if (lookAtObject)
			{
				hashtable.Add("looktarget", !lookAtVector.HasValue ? lookAtObject.transform.position : (lookAtObject.transform.position + lookAtVector.Value));
			}
			else if (lookAtVector.HasValue)
			{
				hashtable.Add("looktarget", lookAtVector.Value);
			}
			if (lookAtObject || lookAtVector.HasValue)
			{
				hashtable.Add("looktime", lookTime.HasValue ? 0f : lookTime.Value);
			}
			iTween.MoveBy(go, hashtable);
		}

		public void iTweenMoveBy(GameObject go, string id, Vector3? vector, float? time, float? delay, float? speed, iTween.EaseType easeType, iTween.LoopType loopType, Space space, bool? orientToPath, GameObject lookAtObject, Vector3? lookAtVector, float? lookTime, iTweenFsmAction.AxisRestriction axis, string startEvent, string finishEvent, bool realTime)
			=> iTweenMoveBy(go, vector, speed, time, delay, easeType, iTween.LoopType.loop, iTweenFSMEvents.itweenIDCount++, realTime, space, id, axis, orientToPath, lookAtObject, lookAtVector, lookTime, startEvent, finishEvent);

		public void iTweenExit(GameObject go) => iTween.Stop(gameObject, "move");
		#endregion

		#region ChaseObject
		private void DoChaseForv2((Rigidbody2D rb2d, GameObject target, float speedMax, float accelerationForce, float offsetX, float offsetY) settings)
		{
			Vector3 oPosition = settings.rb2d.transform.position;
			Vector3 tPosition = settings.target.transform.position;
			Vector2 vector = Vector2.ClampMagnitude(new Vector2(tPosition.x + settings.offsetX - oPosition.x, tPosition.y + settings.offsetY - oPosition.y), 1f);
			vector = new Vector2(vector.x * settings.accelerationForce, vector.y * settings.accelerationForce);
			settings.rb2d.AddForce(vector);
			settings.rb2d.velocity = Vector2.ClampMagnitude(settings.rb2d.velocity, settings.speedMax);
		}

		public int ChaseObjectv2((Rigidbody2D rb2d, GameObject target, float speedMax, float accelerationForce, float offsetX, float offsetY) settings)
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
			Vector2 velocity = MyRigidbody.velocity;
			if (velocity.x < 0f)
			{
				velocity.x *= deceleratev2speed;
				if (velocity.x > 0f)
				{
					velocity.x = 0f;
				}
			}
			else if (velocity.x > 0f)
			{
				velocity.x *= deceleratev2speed;
				if (velocity.x < 0f)
				{
					velocity.x = 0f;
				}
			}
			if (velocity.y < 0f)
			{
				velocity.y *= deceleratev2speed;
				if (velocity.y > 0f)
				{
					velocity.y = 0f;
				}
			}
			else if (velocity.y > 0f)
			{
				velocity.y *= deceleratev2speed;
				if (velocity.y < 0f)
				{
					velocity.y = 0f;
				}
			}
			MyRigidbody.velocity = velocity;
		}
		#endregion

		#region FaceObject
		public int FaceObject(GameObject A, GameObject B, bool spriteFacesRight, bool playNewAnimation, string newAnimationClip, bool resetFrame, bool everyFrame, tk2dSpriteAnimator sprite)
        {
			int index = everyFrame ? faceObjects.Count : -1;
			float xScale = A.transform.localScale.x;
			DoFace(A, B, spriteFacesRight, playNewAnimation, newAnimationClip, resetFrame, xScale, sprite);
			faceObjects.Add((A, B, spriteFacesRight, playNewAnimation, newAnimationClip, resetFrame, xScale, sprite));
			return index;
        }
		public void StopFaceObject(int index) => faceObjects.RemoveAt(index);
		private void DoFace(GameObject A, GameObject B, bool spriteFacesRight, bool playNewAnimation, string newAnimationClip, bool resetFrame, float xScale, tk2dSpriteAnimator _sprite)
		{
			Vector3 localScale = A.transform.localScale;
			if (A.transform.position.x < B.transform.position.x)
			{
				if (spriteFacesRight)
				{
					if (localScale.x != xScale)
					{
						localScale.x = xScale;
						if (resetFrame)
						{
							_sprite.PlayFromFrame(0);
						}
						if (playNewAnimation)
						{
							_sprite.Play(newAnimationClip);
						}
					}
				}
				else if (localScale.x != -xScale)
				{
					localScale.x = -xScale;
					if (resetFrame)
					{
						_sprite.PlayFromFrame(0);
					}
					if (playNewAnimation)
					{
						_sprite.Play(newAnimationClip);
					}
				}
			}
			else if (spriteFacesRight)
			{
				if (localScale.x != -xScale)
				{
					localScale.x = -xScale;
					if (resetFrame)
					{
						_sprite.PlayFromFrame(0);
					}
					if (playNewAnimation)
					{
						_sprite.Play(newAnimationClip);
					}
				}
			}
			else if (localScale.x != xScale)
			{
				localScale.x = xScale;
				if (resetFrame)
				{
					_sprite.PlayFromFrame(0);
				}
				if (playNewAnimation)
				{
					_sprite.Play(newAnimationClip);
				}
			}
			A.transform.localScale = new Vector3(localScale.x, A.transform.localScale.y, A.transform.localScale.z);
		}
        #endregion
    }
}
