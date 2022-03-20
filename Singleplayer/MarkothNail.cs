using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DreamChaser.Singleplayer
{
    public class MarkothNail : FromFsmBehaviour
    {
        public GameObject Target;
        public ParticleSystem IdlePtParticleSystem;
        public int times;
        public float HeroX;
        public float HeroY;
        public float TeleX;
        public float TeleY;
        public Vector2 HeroPos;
        public Vector2 TelePos;
        public float angle;

        public static GameObject MakeCustomNail(GameObject orig)
        {
            GameObject custom = GameObject.Instantiate(orig);
            foreach (PlayMakerFSM fsm in custom.GetComponents<PlayMakerFSM>())
                Destroy(fsm);
            custom.AddComponent<MarkothNail>();
            custom.SetActive(false);
            GameObject.DontDestroyOnLoad(custom);
            return custom;
        }

        public new void Awake() => base.Awake();

        public void OnEnable()
        {
            Target = DreamChaserAI.instance.Target;
            MyRigidbody.velocity = Vector2.zero;
            HeroPos = Target.transform.position;
            HeroX = HeroPos.x;
            HeroY = HeroPos.y;
            times = 0;
            TeleX = HeroX + UnityEngine.Random.Range(5f, 22f) - 11f;
            TeleY = HeroY + UnityEngine.Random.Range(1f, 15f) - 5f;
            TelePos = new Vector2(TeleX, TeleY);
            transform.position = TelePos;
            StartCoroutine(AnticPoint());
        }

        public IEnumerator AnticPoint()
        {
            Coroutine c = StartCoroutine(NextFrame(AttackPointEveryFrame()));
            angle = Mathf.Atan2(Target.transform.position.y - 0.5f - TeleY, Target.transform.position.x - TeleX) * 57.295776f;
            while (angle < 0f)
                angle += 360f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            angle += 180;
            MyRigidbody.velocity = new Vector2(Mathf.Cos(angle * 0.017453292f) * 5f, Mathf.Sin(angle * 0.017453292f) * 5f);
            DreamChaser.AudioPlayerActor.Spawn(TelePos).GetComponent<AudioSource>().PlayOneShot(DreamChaser.mage_knight_teleport);
            yield return new WaitForSecondsRealtime(0.5f);
            StopCoroutine(c);
            StartDeceleratev2(0.9f);
            yield return new WaitForSecondsRealtime(0.4f);
            StopDeceleratev2();
            MyRigidbody.velocity = new Vector2(Mathf.Cos(angle * 0.017453292f) * 30f, Mathf.Sin(angle * 0.017453292f) * 30f);
            DreamChaser.AudioPlayerActor.Spawn(TelePos).GetComponent<AudioSource>().PlayOneShot(DreamChaser.mage_knight_sword);
            DreamChaser.AudioPlayerActor.Spawn(TelePos).GetComponent<AudioSource>().PlayOneShot(DreamChaser.mage_appear);
            yield return new WaitForSecondsRealtime(3f);
            gameObject.Recycle();
        }

        public IEnumerator AttackPointEveryFrame()
        {
            while (true)
            {
                angle = Mathf.Atan2(Target.transform.position.y - 0.5f - TeleY, Target.transform.position.x - TeleX) * 57.295776f;
                while (angle < 0f)
                    angle += 360f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }
        }
    }
}
