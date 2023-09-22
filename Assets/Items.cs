using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemTypes
{
    Medkit,
    LuckyCoffee,
    Multiplier,
    MoreMissile,
    Shield,
    LittleGuy,
    Barrage,
    Gatling,
    Laser,
    Explosive,
}

[System.Serializable]
public class ItemData
{
    public string name;
    public ItemTypes type;
    public Sprite sprite;
    public Color tempColor;
}

public class Items : MonoBehaviour
{
    public float speed;
    public ItemData[] data;
    public ItemTypes type;

    public void ActivateItem()
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = data[(int)type].sprite;
        gameObject.name = type.ToString();
        //transform.GetChild(0).GetComponent<SpriteRenderer>().color = data[(int)type].tempColor;
    }

    private void FixedUpdate()
    {
        transform.position -= Vector3.up * speed * Time.fixedDeltaTime;

        if (transform.position.y < -7) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish")) Destroy(gameObject);
    }
}
