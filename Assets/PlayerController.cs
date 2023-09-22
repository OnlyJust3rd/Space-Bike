using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed = 10f, maxHp = 100, maxBattery = 100, missileCooldown = 10f;

    public BulletType currentBulletType;
    public Image bulletBatteryLabel, hpLabel, missileCooldownLabel, explosivePowerLabel;
    public Image shieldLabel, multiplierLabel, littleGuyLabel, coffeeLabel;
    public Text itemLabel;
    public GameObject shieldSprite;
    public GameObject littleGuyParent;
    public Transform sprite, tilt;

    public Bullet[] bulletArray;

    [HideInInspector]
    public float hp;

    public GameObject deadLabel;

    private Rigidbody2D rb;
    private float inputX;
    private bool missileInput, shootInput, keyUpInput;
    private float bulletCooldownCounter, currentBattery, missileCooldownCounter, cooldownMultilplier = 1f;
    private float shieldCounter, multiplierCounter, coffeeCounter, littleGuyCounter;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        explosivePowerLabel.transform.parent.gameObject.SetActive(false);
        ResetShield();

        FindObjectOfType<AudioManager>().Play("bgm");
        Time.timeScale = 1;

        hp = maxHp;
        currentBattery = maxBattery;
        missileCooldownCounter = missileCooldown;
    }

    private void Update()
    {
        // if (!GameManager.instance.GetComponent<ArduinoController>().enabled)
        // {
            inputX = Input.GetAxis("Horizontal");
            missileInput = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Joystick1Button0);
            shootInput = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Joystick1Button5);
            keyUpInput = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Joystick1Button5);
            RechargeBattery(25);
        // }

        // UI stuff
        bulletBatteryLabel.fillAmount = currentBattery / maxBattery;
        missileCooldownLabel.fillAmount = missileCooldownCounter / missileCooldown;
        hpLabel.fillAmount = hp / maxHp;

        // important shlt
        Shoot();
        // except for this one
        TiltTheShip();

        if(hp <= 0)
        {
            FindObjectOfType<AudioManager>().Stop("bgm");
            FindObjectOfType<AudioManager>().Stop("boss music");

            Time.timeScale = 0;

            deadLabel.SetActive(true);
        }

        // cheat code
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentBulletType = BulletType.Barrage;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentBulletType = BulletType.Gatling;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentBulletType = BulletType.Laser;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentBulletType = BulletType.Explosive;

        // even more ui stuff
        if (littleGuyParent.gameObject.activeInHierarchy) LittleGuyController();
        if (shieldLabel.gameObject.activeInHierarchy) ShieldEffect();
        if (multiplierLabel.gameObject.activeInHierarchy) ScoreMultiplier();
        if (coffeeLabel.gameObject.activeInHierarchy) LuckyCoffeeEffect();
    }

    bool lastShootBool;
    float pedelSpeed;
    public void InputManager(float x, bool missile, bool shoot)
    {
        inputX = x;
        missileInput = missile;
        shootInput = shoot;

        if (lastShootBool && !shootInput) keyUpInput = true;
        lastShootBool = shootInput;
    }

    private void FixedUpdate()
    {
        Movement();
    }

    // private Vector2 shipTip;
    private void TiltTheShip()
    {
        Vector2 offset = Vector2.up * 5;
        tilt.position = Vector2.Lerp(tilt.position, rb.position + offset, Time.deltaTime * 7.5f);

        Vector2 shipVector = tilt.position - transform.position;
        float angle = Mathf.Atan2(shipVector.x, shipVector.y) * Mathf.Rad2Deg;
        sprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        //Debug.DrawRay(transform.position, shipVector.normalized * 3);
        //Debug.DrawRay(transform.position, Vector2.up * 3);
    }

    private void Movement()
    {
        Vector2 moveDir = new Vector2(inputX * movementSpeed * Time.fixedDeltaTime, 0);
        rb.MovePosition(rb.position + moveDir);
    }

    private float explosivePowerCharger;

    private void Shoot()
    {
        Bullet b = bulletArray[(int)currentBulletType];
        Bullet m = bulletArray[3];

        if (Input.GetKeyDown(KeyCode.Return) && b.level < b.prefab.Length) b.level += 1;
        if (Input.GetKeyDown(KeyCode.Escape) && m.level < m.prefab.Length) m.level += 1;

        if (currentBulletType != BulletType.Laser)
        {
            foreach (Transform c in littleGuyParent.transform) c.GetComponent<LittleGuyController>().ResetLaser(BulletType.Laser);
            GetComponent<BulletTypeController>().ResetLaser(bulletArray[(int)BulletType.Laser]);
        }
        if(currentBulletType != BulletType.Explosive)
        {
            explosivePowerLabel.transform.parent.gameObject.SetActive(false);
        }

    // Laser Beep Beep
        if (currentBulletType == BulletType.Laser)
        {
            if (shootInput)
            {
                if (currentBattery > b.batteryCost * 2)
                {
                    GetComponent<BulletTypeController>().ShootTheShot(b);
                    currentBattery -= b.batteryCost + (b.level * b.batteryCost * .2f / b.prefab.Length);

                    if (!FindObjectOfType<AudioManager>().IsPlaying("Laser")) FindObjectOfType<AudioManager>().Play("Laser");

                    // Little Guy
                    if (littleGuyParent.activeInHierarchy)
                    {
                        foreach (Transform c in littleGuyParent.transform)
                        {
                            c.GetComponent<LittleGuyController>().HelpMeShootLittleGuy(b.type);
                        }
                    }
                }
                else
                {
                    GetComponent<BulletTypeController>().ResetLaser(b);
                    // Little Guy
                    if (littleGuyParent.activeInHierarchy) foreach (Transform c in littleGuyParent.transform) c.GetComponent<LittleGuyController>().ResetLaser(BulletType.Laser);
                }
            }
            else
            {
                GetComponent<BulletTypeController>().ResetLaser(b);

                // Little Guy
                if (littleGuyParent.activeInHierarchy) foreach (Transform c in littleGuyParent.transform)c.GetComponent<LittleGuyController>().ResetLaser(BulletType.Laser);
            }
        }
    // Explosive
        else if (currentBulletType == BulletType.Explosive && currentBattery > b.batteryCost)
        {
            explosivePowerLabel.transform.parent.gameObject.SetActive(true);
            explosivePowerLabel.fillAmount = explosivePowerCharger / b.cooldown;

            // Explosive Charging
            if (shootInput)
            {
                if (explosivePowerCharger < b.cooldown) explosivePowerCharger += Time.deltaTime * 1.75f * cooldownMultilplier;
            }
            // Explosive Shoot
            else if (keyUpInput)
            {
                GetComponent<BulletTypeController>().ExplosiveLauncher(b, explosivePowerCharger);
                FindObjectOfType<AudioManager>().Play("ExplosiveLaunch");

                // Little Guy
                if (littleGuyParent.activeInHierarchy)
                {
                    foreach (Transform c in littleGuyParent.transform)
                    {
                        c.GetComponent<LittleGuyController>().HelpMeShootLittleGuy(b.type, explosivePowerCharger);
                    }
                }
                explosivePowerCharger = 0;

                currentBattery -= b.batteryCost + (b.level * b.batteryCost * .2f / b.prefab.Length);
            }
        }
    // Other Bullet
        else if (shootInput && bulletCooldownCounter > b.cooldown && currentBattery > b.batteryCost)
        {
            explosivePowerLabel.transform.parent.gameObject.SetActive(false);
            GetComponent<BulletTypeController>().ShootTheShot(b);

            // Little Guy
            if (littleGuyParent.activeInHierarchy)
            {
                foreach (Transform c in littleGuyParent.transform)
                {
                    c.GetComponent<LittleGuyController>().HelpMeShootLittleGuy(b.type);
                }
            }

            bulletCooldownCounter = 0;
            currentBattery -= b.batteryCost + (b.level * b.batteryCost * .2f / b.prefab.Length);
        }
        bulletCooldownCounter += Time.deltaTime * cooldownMultilplier;

    // Missile
        if (missileInput && missileCooldownCounter > missileCooldown && currentBattery > m.batteryCost)
        {
            GetComponent<BulletTypeController>().MissileRaid(m);
            FindObjectOfType<AudioManager>().Play("ExplosiveLaunch");

            missileCooldownCounter = 0;
            currentBattery -= m.batteryCost + (m.level * m.batteryCost * .3f / m.prefab.Length);
        }
        missileCooldownCounter += Time.deltaTime * cooldownMultilplier;
    }

    private float batteryMultiplier = 1f;
    public void RechargeBattery(float b)
    {
        itemLabel.text = $"SPEED {b}";

        if (currentBattery < maxBattery)
        {
            currentBattery += b * batteryMultiplier / 350;
        }
    }

    private void ScoreMultiplier()
    {
        multiplierCounter -= Time.deltaTime;
        if (multiplierCounter < 0) multiplierLabel.gameObject.SetActive(false);
        multiplierLabel.fillAmount = multiplierCounter / 10;
    }

    private void ResetLuckyCoffee()
    {
        batteryMultiplier = 1f;
        cooldownMultilplier = 1f;

        coffeeLabel.gameObject.SetActive(false);
    }

    private void LuckyCoffeeEffect()
    {
        coffeeCounter -= Time.deltaTime;
        if (coffeeCounter < 0) coffeeLabel.gameObject.SetActive(false);
        coffeeLabel.fillAmount = coffeeCounter / 10;
    }

    public void ResetShield()
    {
        shieldSprite.SetActive(false);
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<PolygonCollider2D>().enabled = true;

        shieldLabel.gameObject.SetActive(false);
    }

    private void ShieldEffect()
    {
        shieldCounter -= Time.deltaTime;
        if (shieldCounter < 0) shieldLabel.gameObject.SetActive(false);
        shieldLabel.fillAmount = shieldCounter / 10;
    }

    private void ResetLittleGuy()
    {
        littleGuyParent.gameObject.SetActive(false);
        littleGuyLabel.gameObject.SetActive(false);

        // Little Guy
        if (currentBulletType == BulletType.Laser) foreach (Transform c in littleGuyParent.transform) c.GetComponent<LittleGuyController>().ResetLaser(BulletType.Laser);
    }

    private void LittleGuyController()
    {
        littleGuyParent.transform.position = Vector3.Lerp(littleGuyParent.transform.position, transform.position, Time.deltaTime * 50);

        littleGuyCounter -= Time.deltaTime;
        if (littleGuyCounter < 0) littleGuyLabel.gameObject.SetActive(false);
        littleGuyLabel.fillAmount = littleGuyCounter / 10;
    }

    private bool isAbleToTakeDamage = true;
    private IEnumerator BlinkCount()
    {
        isAbleToTakeDamage = false;
        sprite.GetComponent<Animator>().SetBool("TakeDamage", true);
        yield return new WaitForSeconds(1);
        sprite.GetComponent<Animator>().SetBool("TakeDamage", false);
        isAbleToTakeDamage = true;
    }
    public void TakeDamage(float dmg)
    {
        if (isAbleToTakeDamage)
        {
            hp -= dmg;
            StartCoroutine(BlinkCount());

            FindObjectOfType<AudioManager>().Play("Hurt");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Items"))
        {
            Destroy(other.gameObject);
            FindObjectOfType<AudioManager>().Play("Get Item");

            Items i = other.GetComponent<Items>();
            GameManager.instance.fpsLabel.text = i.type.ToString();
            // Medkit
            if(i.type == ItemTypes.Medkit)
            {
                if (hp < maxHp) hp += 25;
                if (hp > maxHp) hp = maxHp;

            }
            // BatteryCharge
            if (i.type == ItemTypes.LuckyCoffee)
            {
                coffeeCounter = 10;
                coffeeLabel.gameObject.SetActive(true);

                batteryMultiplier = 2.5f;
                cooldownMultilplier = 2f;
                Invoke("ResetLuckyCoffee", 10);
            }
            // Multiplier
            if (i.type == ItemTypes.Multiplier)
            {
                multiplierCounter = 10;
                multiplierLabel.gameObject.SetActive(true);

                GameManager.instance.scoreMultiplier = 2;
                GameManager.instance.Invoke("ResetScoreMultiplier", 10);
            }
            // MoreMissile
            if (i.type == ItemTypes.MoreMissile)
            {
                Bullet b = bulletArray[3];
                if (b.level < b.prefab.Length - 1) b.level += 1;
            }
            // Shield
            if (i.type == ItemTypes.Shield)
            {
                shieldSprite.SetActive(true);
                shieldLabel.gameObject.SetActive(true);
                shieldCounter = 10;

                GetComponent<CircleCollider2D>().enabled = true;
                GetComponent<PolygonCollider2D>().enabled = false;
                Invoke("ResetShield", 10);
            }
            // Little Guy
            if (i.type == ItemTypes.LittleGuy)
            {
                littleGuyCounter = 10;
                littleGuyLabel.gameObject.SetActive(true);

                littleGuyParent.SetActive(true);
                Invoke("ResetLittleGuy", 10);
            }
            // Barrage
            if (i.type == ItemTypes.Barrage)
            {
                if (currentBulletType == BulletType.Barrage)
                {
                    Bullet b = bulletArray[(int)currentBulletType];
                    if(b.level < b.prefab.Length-1) b.level++;
                }
                else currentBulletType = BulletType.Barrage;
            }
            // Gatling
            if (i.type == ItemTypes.Gatling)
            {
                if (currentBulletType == BulletType.Gatling)
                {
                    Bullet b = bulletArray[(int)currentBulletType];
                    if (b.level < b.prefab.Length-1) b.level++;
                }
                else currentBulletType = BulletType.Gatling;
            }
            // Laser
            if (i.type == ItemTypes.Laser)
            {
                if (currentBulletType == BulletType.Laser)
                {
                    Bullet b = bulletArray[(int)currentBulletType];
                    GetComponent<BulletTypeController>().ResetLaser(b);
                    if (b.level < b.prefab.Length - 1) b.level++;
                }
                else currentBulletType = BulletType.Laser;
            }
            // Explosive
            if (i.type == ItemTypes.Explosive)
            {
                if (currentBulletType == BulletType.Explosive)
                {
                    Bullet b = bulletArray[(int)currentBulletType];
                    if (b.level < b.prefab.Length - 1) b.level++;
                }
                else currentBulletType = BulletType.Explosive;
            }
        }
    }
}
