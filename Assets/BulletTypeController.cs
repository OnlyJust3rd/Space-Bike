using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletType
{
    Barrage,
    Gatling,
    Laser,
    Missile,
    Explosive
}

[System.Serializable]
public class Bullet
{
    public string name;
    public BulletType type;
    public GameObject[] prefab;
    public int level = 0;
    public float cooldown = .5f;
    public int bulletPerShot = 2;
    public float batteryCost;
}

public class BulletTypeController : MonoBehaviour
{
    public void ShootTheShot(Bullet bullet)
    {
        if (bullet.type == BulletType.Barrage) BarrageShot(bullet);
        if (bullet.type == BulletType.Gatling) GatlingGun(bullet);
        if (bullet.type == BulletType.Laser) LaserBeam(bullet);
    }

    public void BarrageShot(Bullet b)
    {
        FindObjectOfType<AudioManager>().Play("Barrage");

        Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + .3f);
        float rx = Random.Range(.05f, .5f);
        Vector2 offset = new Vector2(rx, 0);

        GameObject newBullet = Instantiate(b.prefab[b.level], spawnPos, Quaternion.identity);
        newBullet.name = b.prefab[b.level].name;
        Destroy(newBullet, .5f);

        //Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + .3f);
        //for (int i = 0; i < b.bulletPerShot; i++)
        //{
        //    float rx = Random.Range(.05f, .5f);
        //    Vector2 offset = new Vector2(rx, 0);
        //    int m = 1;
        //    if (i % 2 == 0) m = -1;
        //    GameObject newBullet = Instantiate(b.prefab, spawnPos + offset * m, Quaternion.identity);
        //    newBullet.name = b.prefab.name;
        //    Destroy(newBullet, 2);
        //}
    }

    private void GatlingGun(Bullet b)
    {
        FindObjectOfType<AudioManager>().Play("Gatling");

        Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + .3f);

        float rx = Random.Range(-.25f, .25f);
        Vector2 offset = new Vector2(rx, 0);
        GameObject newBullet = Instantiate(b.prefab[b.level], spawnPos + offset, Quaternion.identity);
        newBullet.name = b.prefab[b.level].name;
        Destroy(newBullet, .5f);
    }
    public Transform laserTip;
    private void LaserBeam(Bullet b)
    {
        b.prefab[b.level].SetActive(true);

        Vector2 offset = Vector2.up * 10;
        Vector2 playerPos = GetComponent<Rigidbody2D>().position;
        laserTip.position = Vector3.Lerp(laserTip.position, playerPos + offset, Time.deltaTime * 5);

        Vector2 laserVector = laserTip.position - transform.position;
        float angle = Mathf.Atan2(laserVector.x, laserVector.y) * Mathf.Rad2Deg;
        b.prefab[b.level].transform.rotation = Quaternion.Euler(new Vector3(0, 0, -angle));

        b.prefab[b.level].transform.position = transform.position;

        //Debug.DrawRay(transform.position, (laserTip.position - transform.position).normalized * 10, Color.red);
        //Debug.DrawRay(transform.position, Vector2.up * 10, Color.green);
    }

    public void MissileRaid(Bullet b)
    {
        GameObject newBullet = Instantiate(b.prefab[b.level], transform.position, Quaternion.identity);
        newBullet.name = b.prefab[b.level].name;
        Destroy(newBullet, 3);
    }
    public void ExplosiveLauncher(Bullet b, float power)
    {
        Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + .3f);

        GameObject newBullet = Instantiate(b.prefab[b.level], spawnPos, Quaternion.identity);
        newBullet.name = b.prefab[b.level].name;
        foreach (Transform child in newBullet.transform)
        {
            child.GetComponent<PlayerBullet>().explosiveCap = power;
        }

        Destroy(newBullet, 2.5f);
    }

    public void ResetLaser(Bullet b)
    {
        foreach (GameObject p in b.prefab) p.SetActive(false);
        FindObjectOfType<AudioManager>().Stop("Laser");
    }
}
