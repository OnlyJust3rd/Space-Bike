using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerBulletTrajectory
{
    Straight,
    Missile,
    Explosive,
    ClusterExplosive,
    LaserPewPew
}

public class PlayerBullet : MonoBehaviour
{ 
    public float bulletSpeed = 1f;
    public float bulletDamage = 1f;
    public int penetratePower = 1;
    public PlayerBulletTrajectory trajectory;

    public GameObject cluster;
    public GameObject effect;

    [HideInInspector]
    public Vector2 targetPos;
    [HideInInspector]
    public float explosiveCap = 2;

    private int penetrateCounter;
    private Rigidbody2D rb;
    private float missileActivateCounter, missileWaitTime = 1;
    private float explosiveCounter;
    private bool isMissileLock;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void TargetLock()
    {
        EnemyController[] e = FindObjectsOfType<EnemyController>();
        if (e.Length > 0)
        {
            targetPos = e[Random.Range(0, e.Length)].transform.position;

            targetPos = (targetPos - rb.position).normalized;
        }
        else targetPos = Vector2.up;
    }

    private void FixedUpdate()
    {
        // Normal People
        if (trajectory == PlayerBulletTrajectory.Straight)
        {
            Vector2 dir = new Vector2(transform.up.x, transform.up.y);
            // Debug.DrawRay(transform.position, dir);
            rb.MovePosition(rb.position + dir.normalized * bulletSpeed * Time.fixedDeltaTime);
        }
        // Missile
        if (trajectory == PlayerBulletTrajectory.Missile)
        {
            if(missileActivateCounter < missileWaitTime)
            {
                if (missileWaitTime - missileActivateCounter < 0) missileActivateCounter = missileWaitTime;

                Vector2 dir = new Vector2(transform.up.x, transform.up.y);
                // Debug.DrawRay(transform.position, dir);
                rb.MovePosition(rb.position + dir.normalized * (missileWaitTime - missileActivateCounter) / 15);

                missileActivateCounter += Time.deltaTime;
            }
            else
            {
                if (!isMissileLock)
                {
                    TargetLock();
                    foreach (Transform child in cluster.transform) child.GetComponent<ParticleSystem>().Play();
                }
                isMissileLock = true;

                float angle = Mathf.Atan2(targetPos.y, targetPos.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));

                rb.MovePosition(rb.position + targetPos.normalized * bulletSpeed * Time.fixedDeltaTime);
                // Debug.DrawRay(transform.position, targetPos);
            }
        }
        // Explosive
        if(trajectory == PlayerBulletTrajectory.Explosive)
        {
            if (explosiveCounter < explosiveCap)
            {
                if (explosiveCap - explosiveCounter < 0) explosiveCap = explosiveCounter;

                Vector2 dir = new Vector2(transform.up.x, transform.up.y);
                // Debug.DrawRay(transform.position, dir);
                rb.MovePosition(rb.position + dir.normalized * (explosiveCap - explosiveCounter) / 4f);

                explosiveCounter += Time.deltaTime;
            }
            else JustNormalExplode(1.5f, bulletDamage);
        }
        // Cluster
        if (trajectory == PlayerBulletTrajectory.ClusterExplosive)
        {
            if (explosiveCounter < explosiveCap)
            {
                if (explosiveCap - explosiveCounter < 0) explosiveCap = explosiveCounter;

                Vector2 dir = new Vector2(transform.up.x, transform.up.y);
                // Debug.DrawRay(transform.position, dir);
                rb.MovePosition(rb.position + dir.normalized * (explosiveCap - explosiveCounter) / 4.2f);

                explosiveCounter += Time.deltaTime;
            }
            else
            {
                JustNormalExplode(1.5f, bulletDamage);

                GameObject newCluster = Instantiate(cluster, transform.position, Quaternion.identity);
                float angle = Random.Range(0, 360);
                newCluster.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

                foreach (Transform child in newCluster.transform)
                {
                    child.GetComponent<PlayerBullet>().explosiveCap = .5f;
                    child.GetComponent<PlayerBullet>().bulletDamage = 30;
                }
                Destroy(newCluster, 2.5f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag.Substring(0,5) == "Enemy" && trajectory != PlayerBulletTrajectory.LaserPewPew)
        {
            if(trajectory == PlayerBulletTrajectory.Straight) BulletHitEffect();

            Destroy(gameObject);

            other.GetComponent<EnemyController>().TakeDamage(bulletDamage);;

            // Missile
            if(trajectory == PlayerBulletTrajectory.Missile)
            {
                JustNormalExplode(1, bulletDamage * 2 / 3);

                cluster.transform.parent = null;
                foreach(Transform child in cluster.transform) child.GetComponent<ParticleSystem>().Stop();
                Destroy(cluster, 2.5f);
            }
        }

        if (other.CompareTag("Respawn") && trajectory != PlayerBulletTrajectory.Missile)
        {
            Destroy(gameObject);
        }

        if(other.CompareTag("Finish") && trajectory == PlayerBulletTrajectory.Missile)
        {
            Destroy(gameObject);
        }
    }

    private float laserCounter, laserCooldown = .05f;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (trajectory == PlayerBulletTrajectory.LaserPewPew && other.tag.Substring(0, 5) == "Enemy")
        {
            // Do damage
            if (laserCounter > laserCooldown)
            {
                other.GetComponent<EnemyController>().TakeDamage(bulletDamage);;
                laserCounter = 0;

                Quaternion rot = Quaternion.Euler(0, 0, Random.Range(170, 190));
                RaycastHit2D[] hit = Physics2D.CircleCastAll(transform.position, .5f, transform.up * 10, LayerMask.GetMask("Enemy"));

                for (int i = 0; i < hit.Length; i++)
                {
                    GameObject eo = Instantiate(effect, hit[i].point, rot);
                    Destroy(eo, .25f);

                    FindObjectOfType<AudioManager>().Play("Enemy Hit");
                }
            }
            else laserCounter += Time.deltaTime;
        }
    }

    private void JustNormalExplode(float r, float dmg)
    {
        FindObjectOfType<AudioManager>().Play("Bomb");

        Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, r, LayerMask.GetMask("Enemy"));
        if (col.Length > 0)
        {
            FindObjectOfType<AudioManager>().Play("Enemy Hit");

            foreach (Collider2D c in col)
            {
                c.GetComponent<EnemyController>().TakeDamage(dmg);
            }
        }
        GameObject e = Instantiate(effect, transform.position, Quaternion.identity);
        Destroy(e, 1);

        Destroy(gameObject);
    }

    private void BulletHitEffect()
    {
        FindObjectOfType<AudioManager>().Play("Enemy Hit");
        effect.transform.parent = null;
        effect.transform.rotation = Quaternion.Euler(0, 0, Random.Range(172.5f, 187.5f));
        effect.transform.position += Vector3.down * .5f;
        effect.GetComponent<ParticleSystem>().Play();
        Destroy(effect, .25f);
    }

    private void OnDrawGizmos()
    {
        if(trajectory == PlayerBulletTrajectory.Missile) Gizmos.DrawWireSphere(transform.position, 1);
        if(trajectory == PlayerBulletTrajectory.Explosive) Gizmos.DrawWireSphere(transform.position, 1);
    }
}
