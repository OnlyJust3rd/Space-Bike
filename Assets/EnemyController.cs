using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public EnemyInfo enemyInfo;
    public Transform sprite;
    public GameObject effect;
    public bool isBoss;

    [SerializeField]
    private Image hpLabel;

    [HideInInspector]
    public bool isActivated = false;

    // [HideInInspector]
    public float hp;

    private EnemyBehavior eb;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        eb = GetComponent<EnemyBehavior>();
        hp = enemyInfo.maxHp;

        if (isBoss)
        {
            enemyInfo.maxHp = 3000;
            hp = enemyInfo.maxHp;
            GameObject label = GameManager.instance.bossLabel;
            label.SetActive(true);
            hpLabel = label.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            label.transform.GetChild(1).GetComponent<Text>().text = gameObject.name;
        }
    }
    private bool oneTime = true;
    private void FixedUpdate()
    {
        hpLabel.fillAmount = hp / enemyInfo.maxHp;

        if(isActivated) eb.MovementHandler(enemyInfo);

        // die
        if (hp <= 0 && oneTime)
        {
            oneTime = false;
            isActivated = false;

            GameManager.instance.bossLabel.SetActive(false);

            if (isBoss)
            {
                print("boos dead");
                StartCoroutine(DeadBoss());
            }
            else
            {
                Destroy(gameObject);
                DeadEffect(Random.Range(170, 190));
            }
            GameManager.instance.GivePoint(enemyInfo.point);

            if (enemyInfo.type == EnemyType.ItemGiveAway)
            {
                GameObject newItem = Instantiate(GameManager.instance.itemBase, transform.position, Quaternion.identity);
                newItem.GetComponent<Items>().type = (ItemTypes)Random.Range(0, System.Enum.GetValues(typeof(ItemTypes)).Length); ;
                newItem.GetComponent<Items>().ActivateItem();

                return;
            }

            int itemLenght = System.Enum.GetValues(typeof(ItemTypes)).Length;
            int maxRate = Mathf.CeilToInt(itemLenght * 100 / 15);
            int token = Random.Range(0, maxRate);
            if(token < itemLenght)
            {
                GameObject newItem = Instantiate(GameManager.instance.itemBase, transform.position, Quaternion.identity);
                newItem.GetComponent<Items>().type = (ItemTypes)token;
                newItem.GetComponent<Items>().ActivateItem();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (other.CompareTag("Bullet/PlayerBullet"))
        //{
        //    hp -= other.GetComponent<PlayerBullet>().bulletDamage;
        //}
        if (other.CompareTag("Finish") && !isBoss)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.CompareTag("Player") && !isBoss)
        {
            PlayerController p = c.collider.GetComponent<PlayerController>();
            if (!p.shieldSprite.activeInHierarchy)
            {
                p.TakeDamage(enemyInfo.crashDamage);
            }
            else p.ResetShield();

            Destroy(gameObject);
            Vector3 v = c.collider.transform.position - transform.position;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            DeadEffect(angle + 90);
        }
    }

    private void DeadEffect(float angle)
    {
        effect.transform.parent = null;
        effect.transform.rotation = Quaternion.Euler(0, 0, angle);
        effect.GetComponent<ParticleSystem>().Play();
        Destroy(effect, .5f);
    }

    private float deadEffectCounter = .2f;
    private IEnumerator DeadBoss()
    {
        FindObjectOfType<AudioManager>().Stop("boss music");

        GameManager g = GameManager.instance;
        if (g.difficulty < g.formations.Length - 1) g.difficulty++;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                print("fuck");
                Vector2 offset = new Vector2(Random.Range(-1.6f, 1.6f), Random.Range(-1.2f, 1.2f));
                GameObject neweffect = Instantiate(effect, rb.position + offset, Quaternion.identity);
                neweffect.transform.localScale = Vector3.one * Random.Range(.8f, 1.1f);
                Destroy(neweffect, .5f);

                FindObjectOfType<AudioManager>().Play("Bomb");
            }

            yield return new WaitForSeconds(deadEffectCounter);
            deadEffectCounter -= .02f;
        }

        yield return new WaitForSeconds(1.5f);

        // Mega Bomb
        for (int j = 0; j < 10; j++)
        {
            Vector2 offset = new Vector2(Random.Range(-1.6f, 1.6f), Random.Range(-1.2f, 1.2f));
            GameObject neweffect = Instantiate(effect, rb.position + offset, Quaternion.identity);
            neweffect.transform.localScale = Vector3.one * Random.Range(.8f, 1.1f);
            Destroy(neweffect, .5f);

            FindObjectOfType<AudioManager>().Play("Bomb");
        }

        Destroy(gameObject);

        FindObjectOfType<AudioManager>().Play("bgm");

        // Spawn Item
        for (int i = 0; i < 3; i++)
        {
            Vector2 offset = new Vector2(Random.Range(-1.6f, 1.6f), Random.Range(-1.2f, 1.2f));

            GameObject newItem = Instantiate(GameManager.instance.itemBase, rb.position + offset, Quaternion.identity);
            newItem.GetComponent<Items>().type = (ItemTypes)Random.Range(0, System.Enum.GetValues(typeof(ItemTypes)).Length);
            newItem.GetComponent<Items>().ActivateItem();

            offset = new Vector2(Random.Range(-1.6f, 1.6f), Random.Range(-1.2f, 1.2f));
            GameObject medkit = Instantiate(GameManager.instance.itemBase, rb.position + offset, Quaternion.identity);
            medkit.GetComponent<Items>().type = 0;
            medkit.GetComponent<Items>().ActivateItem();
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isActivated) hp -= dmg;
    }
}
