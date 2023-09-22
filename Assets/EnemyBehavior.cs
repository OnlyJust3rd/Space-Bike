using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    SmallFry,
    Charger,
    Missile,
    Shooter,
    MegaShooter,
    ItemGiveAway,
    LaserBoss
}

[System.Serializable]
public class EnemyInfo
{
    public EnemyType type;
    public float movementSpeed = 5f;
    public float waitingTime = 3;
    public float crashDamage, cooldown;
    public float maxHp = 100;
    public int point = 150;
    public GameObject bulletPrefab;
    public GameObject[] bossBulletPrefab;
}


public class EnemyBehavior : MonoBehaviour
{
    private float timeCounter = 0;
    private Vector2 playerPos;
    private Rigidbody2D rb;
    private float shootCounter;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MovementHandler(EnemyInfo info)
    {
        if (info.type == EnemyType.SmallFry) SmallFryMovement(info);
        if (info.type == EnemyType.Charger) ChargerMovement(info);
        if (info.type == EnemyType.Shooter) ShooterMovement(info);
        if (info.type == EnemyType.MegaShooter) ShooterMovement(info);
        if (info.type == EnemyType.ItemGiveAway) Feeder(info);
        if (info.type == EnemyType.LaserBoss) LaserBoi(info);
    }

    private bool isCharging = false;
    public IEnumerator UpdatePlayerPos()
    {
        playerPos = FindObjectOfType<PlayerController>().transform.position;
        yield return new WaitForSeconds(1);
        StartCoroutine(UpdatePlayerPos());
    }

    private void SmallFryMovement(EnemyInfo e)
    {
        // Shoot
        if (timeCounter < e.waitingTime)
        {
            if (shootCounter > e.cooldown)
            {
                Vector3 offset = Vector3.down * .3f;
                GameObject bullet = Instantiate(e.bulletPrefab, transform.position + offset, Quaternion.identity);
                bullet.name = e.bulletPrefab.name;
                Destroy(bullet, 4);

                shootCounter = 0;
            }
            shootCounter += Time.deltaTime;
            timeCounter += Time.deltaTime;
        }
        // Charge
        else
        {
            if (!isCharging) StartCoroutine(UpdatePlayerPos());
            isCharging = true;

            Vector2 moveDir = playerPos - rb.position;
            // Debug.DrawRay(rb.position, moveDir.normalized * 10, Color.red);
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            GetComponent<EnemyController>().sprite.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));

            rb.MovePosition(rb.position + moveDir.normalized * e.movementSpeed * Time.fixedDeltaTime);
        }
    }

    private void ChargerMovement(EnemyInfo e)
    {
        // Shoot
        if (timeCounter < e.waitingTime)
        {
            timeCounter += Time.deltaTime;
            GetComponent<EnemyController>().sprite.GetChild(0).gameObject.SetActive(true);
            GetComponent<EnemyController>().sprite.GetChild(1).gameObject.SetActive(false);
        }
        // Charge
        else
        {
            // Debug.DrawRay(rb.position, moveDir.normalized * 10, Color.red);
            GetComponent<EnemyController>().sprite.GetChild(0).gameObject.SetActive(false);
            GetComponent<EnemyController>().sprite.GetChild(1).gameObject.SetActive(true);
            rb.MovePosition(rb.position - Vector2.up * e.movementSpeed * Time.fixedDeltaTime);
        }
    }

    private void ShooterMovement(EnemyInfo e)
    {
        if (!isCharging) StartCoroutine(UpdatePlayerPos());
        isCharging = true;

        if (shootCounter > e.cooldown)
        {
            Vector3 offset = Vector3.down * .3f;
            GameObject bullet = Instantiate(e.bulletPrefab, transform.position + offset, Quaternion.identity);
            bullet.GetComponent<EnemyBullet>().targetPos = playerPos;
            bullet.name = e.bulletPrefab.name;
            Destroy(bullet, 4);
            shootCounter = 0;

            Vector2 t = playerPos - GetComponent<Rigidbody2D>().position;
            float angle = Mathf.Atan2(t.y, t.x) * Mathf.Rad2Deg;
            GetComponent<EnemyController>().sprite.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
        }
        shootCounter += Time.deltaTime;

        // Charge
        if (timeCounter > e.waitingTime)
        {
            Vector2 moveDir = playerPos - rb.position;
            // Debug.DrawRay(rb.position, moveDir.normalized * 10, Color.red);
            //float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            //GetComponent<EnemyController>().sprite.GetChild(0).rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
            rb.MovePosition(rb.position + moveDir.normalized * e.movementSpeed * Time.fixedDeltaTime);

        }
        else timeCounter += Time.deltaTime;
    }
    public bool swap;
    private void Feeder(EnemyInfo e)
    {
        // Shoot
        if (timeCounter < e.waitingTime)
        {
            if (swap)
            {
                rb.MovePosition(rb.position + Vector2.right * e.movementSpeed * Time.fixedDeltaTime);

                float angle = Mathf.Atan2(Vector3.right.y, Vector3.right.x) * Mathf.Rad2Deg;
                GetComponent<EnemyController>().sprite.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
            }
            else
            {
                rb.MovePosition(rb.position - Vector2.right * e.movementSpeed * Time.fixedDeltaTime);

                float angle = Mathf.Atan2(-Vector3.right.y, -Vector3.right.x) * Mathf.Rad2Deg;
                GetComponent<EnemyController>().sprite.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
            }

            timeCounter += Time.deltaTime;
        }
        // Charge
        else
        {
            // Debug.DrawRay(rb.position, moveDir.normalized * 10, Color.red);
            rb.MovePosition(rb.position - Vector2.up * e.movementSpeed * Time.fixedDeltaTime);

            GetComponent<EnemyController>().sprite.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
    }

    public Vector2[] bulletSpawnPoint;
    private float stateCounter;
    private int state;
    private void LaserBoi(EnemyInfo e)
    {
        if (!isCharging) StartCoroutine(UpdatePlayerPos());
        isCharging = true;

        // shoot shoot
        if (state == 0)
        {
            e.bossBulletPrefab[2].SetActive(false);

            if (shootCounter > e.cooldown / 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 offset = Vector3.down * .3f;
                    GameObject bullet = Instantiate(e.bossBulletPrefab[0], GetComponent<Rigidbody2D>().position + bulletSpawnPoint[i], Quaternion.identity);
                    bullet.GetComponent<EnemyBullet>().targetPos = playerPos;
                    bullet.name = e.bossBulletPrefab[0].name;
                    Destroy(bullet, 4);
                }

                shootCounter = 0;
            }
            shootCounter += Time.deltaTime;
        }
        // deploy charger
        if(state == 1)
        {
            if (shootCounter > e.cooldown)
            {
                Vector2 offset = Vector2.up * 4.5f;
                GameObject bullet = Instantiate(e.bossBulletPrefab[1], playerPos + offset, Quaternion.identity);
                bullet.name = e.bossBulletPrefab[1].name;
                bullet.GetComponent<EnemyController>().isActivated = true;
                bullet.GetComponent<EnemyController>().enemyInfo.waitingTime = 1;
                // Destroy(bullet, 4);
                shootCounter = 0;
            }
            shootCounter += Time.deltaTime;
        }
        // LASSSERRRR
        if(state == 2)
        {
            if (shootCounter > 1)
            {
                if (GetComponent<EnemyController>().hp <= 0) e.bossBulletPrefab[2].SetActive(false);
                else e.bossBulletPrefab[2].SetActive(true);

                e.bossBulletPrefab[3].SetActive(false);
            }
            else
            {
                shootCounter += Time.deltaTime;
                e.bossBulletPrefab[3].SetActive(true);
            }
        }

        if(stateCounter > 5)
        {
            if (state < 2) state++;
            else state = 0;

            stateCounter = 0;
            shootCounter = 0;
        }
        else stateCounter += Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        swap = !swap;
    }
}
