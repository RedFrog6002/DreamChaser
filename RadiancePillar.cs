using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DreamChaser
{
    public class RadiancePillar : FromFsmBehaviour
    {
        public Rigidbody2D rb2d;

        public IEnumerator Shoot(Vector3 startPos, int mod)
        {
            transform.position = startPos;
            rb2d.velocity = new Vector2(-10f * mod, 0f);
            if (mod == 1)
            {
                float targetx = startPos.x - 20f;
                while (transform.position.x > targetx)
                {
                    yield return new WaitForSeconds(0.075f);
                    DreamChaser.RadiancePillarPref.Spawn(transform.position, Quaternion.Euler(0f, 0f, 90f));
                }
            }
            else
            {
                float targetx = startPos.x + 20f;
                while (transform.position.x < targetx)
                {
                    yield return new WaitForSeconds(0.075f);
                    DreamChaser.RadiancePillarPref.Spawn(transform.position, Quaternion.Euler(0f, 0f, 90f));
                }
            }
        }
    }
}
