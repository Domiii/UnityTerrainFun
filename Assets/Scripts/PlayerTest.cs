using UnityEngine;
using System.Collections;

public class PlayerTest : MonoBehaviour {
    public float Speed = 8;
    public float ShootForce = 30;
    public Transform ShootTransform;
    public GameObject BulletPrefab;
    public float ShootDelay = 100;
    float lastShootTime;

	// Use this for initialization
	void Start () {
        lastShootTime = Time.time;
    }
	
	// Update is called once per frame
	void Update () {
        if (lastShootTime + ShootDelay < Time.time)
        {
            Shoot();
        }
        //transform.localPosition += transform.forward * Time.deltaTime * Speed;
	}

    void Shoot()
    {
        lastShootTime = Time.time;
        var bullet = (GameObject)Instantiate(BulletPrefab, ShootTransform.position, ShootTransform.rotation);
        var body = bullet.GetComponent<Rigidbody>();
        if (body == null)
        {
            body = bullet.AddComponent<Rigidbody>();
        }
        body.AddForce(ShootTransform.forward * ShootForce);

        Destroy(bullet, 10);        // life-time of 10s
    }
}
