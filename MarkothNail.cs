using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DreamChaser
{
    public class MarkothNail : FromFsmBehaviour
    {
        public ParticleSystem IdlePtParticleSystem;
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
            MyRigidbody.velocity = Vector2.zero;
            transform.position = TelePos;
            TeleX = TelePos.x;
            TeleY = TelePos.y;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            DreamChaser.AudioPlayerActor.Spawn(TelePos).GetComponent<AudioSource>().PlayOneShot(DreamChaser.mage_knight_teleport);
            StartCoroutine(AnticPoint());
        }

        public IEnumerator AnticPoint()
        {
            yield return new WaitForSecondsRealtime(1f);
            MyRigidbody.velocity = new Vector2(Mathf.Cos(angle * 0.017453292f) * 30f, Mathf.Sin(angle * 0.017453292f) * 30f);
            DreamChaser.AudioPlayerActor.Spawn(TelePos).GetComponent<AudioSource>().PlayOneShot(DreamChaser.mage_knight_sword);
            DreamChaser.AudioPlayerActor.Spawn(TelePos).GetComponent<AudioSource>().PlayOneShot(DreamChaser.mage_appear);
            yield return new WaitForSecondsRealtime(3f);
            gameObject.Recycle();
        }
    }
}
