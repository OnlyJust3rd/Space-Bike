using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public enum EnemyBulletTrajectory
    {
        Straight,
        Target,
        MegaLaser
    }

    public EnemyBulletTrajectory trajectory;
    [HideInInspector]
    public Vector2 targetPos;
    public float bulletSpeed = 1f;
    public float bulletDamage = 1f;

    private Rigidbody2D rb;

    private void Start()
    {
        if (trajectory == EnemyBulletTrajectory.MegaLaser) return;

        rb = GetComponent<Rigidbody2D>();

        targetPos = (targetPos - rb.position).normalized;

        if (trajectory == EnemyBulletTrajectory.Target)
        {
            float angle = Mathf.Atan2(targetPos.y, targetPos.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
        }
    }

    private void FixedUpdate()
    {
        if (trajectory == EnemyBulletTrajectory.MegaLaser) return;

        if (trajectory == EnemyBulletTrajectory.Straight)
        {
            rb.MovePosition(rb.position - Vector2.up * bulletSpeed * Time.fixedDeltaTime);
        }
        if (trajectory == EnemyBulletTrajectory.Target)
        {
            Debug.DrawRay(transform.position, transform.up.normalized);
            Debug.DrawRay(transform.position, targetPos);

            rb.MovePosition(rb.position + targetPos * bulletSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (trajectory == EnemyBulletTrajectory.MegaLaser) return;

        if (other.CompareTag("Player"))
        {
            PlayerController p = other.GetComponent<PlayerController>();
            if (!p.shieldSprite.activeInHierarchy)
            {
                p.TakeDamage(bulletDamage);
            }

            Destroy(gameObject);
        }
        if (other.CompareTag("Bullet/PlayerBullet"))
        {
            Destroy(gameObject);
        }

    }

    private float laserCounter;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && trajectory == EnemyBulletTrajectory.MegaLaser)
        {
            if (laserCounter > .1f)
            {
                other.GetComponent<PlayerController>().TakeDamage(bulletDamage);
                if(FindObjectOfType<AudioManager>().IsPlaying("Laser")) FindObjectOfType<AudioManager>().Play("Laser");

                laserCounter = 0;
            }
            else laserCounter += Time.deltaTime;
        }
    }
}
