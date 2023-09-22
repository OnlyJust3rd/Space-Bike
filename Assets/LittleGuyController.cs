using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleGuyController : MonoBehaviour
{
    public Bullet[] miniBullets;

    public void HelpMeShootLittleGuy(BulletType type)
    {
        if (type == BulletType.Barrage) BarrageShot(miniBullets[(int)type]);
        if (type == BulletType.Gatling) GatlingGun(miniBullets[(int)type]);
        if (type == BulletType.Laser) LaserBeam(miniBullets[(int)type]);
    }

    public void HelpMeShootLittleGuy(BulletType type, float power)
    {
        if (type == BulletType.Explosive) ExplosiveLauncher(miniBullets[(int)type], power);
    }

    private void BarrageShot(Bullet b)
    {
        Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + .3f);
        float rx = Random.Range(.05f, .5f);
        Vector2 offset = new Vector2(rx, 0);

        GameObject newBullet = Instantiate(b.prefab[b.level], spawnPos, Quaternion.identity);
        newBullet.name = b.prefab[b.level].name;
        Destroy(newBullet, .5f);
    }
    private void GatlingGun(Bullet b)
    {
        Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + .3f);

        float rx = Random.Range(-.25f, .25f);
        Vector2 offset = new Vector2(rx, 0);
        GameObject newBullet = Instantiate(b.prefab[b.level], spawnPos + offset, Quaternion.identity);
        newBullet.name = b.prefab[b.level].name;
        Destroy(newBullet, .5f);
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

    public Transform laserTip;
    private void LaserBeam(Bullet b)
    {
        b.prefab[b.level].SetActive(true);

        Vector2 offset = Vector2.up * 10;
        Vector2 playerPos = new Vector2(transform.position.x, transform.position.y);
        laserTip.position = Vector3.Lerp(laserTip.position, playerPos + offset, Time.deltaTime * 5);

        Vector2 laserVector = laserTip.position - transform.position;
        float angle = Mathf.Atan2(laserVector.x, laserVector.y) * Mathf.Rad2Deg;
        b.prefab[b.level].transform.rotation = Quaternion.Euler(new Vector3(0, 0, -angle));
        b.prefab[b.level].transform.position = transform.position;
    }

    public void ResetLaser(BulletType b)
    {
        foreach (GameObject p in miniBullets[(int)b].prefab) p.SetActive(false);
    }
}
