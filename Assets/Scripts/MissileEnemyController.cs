using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class MissileEnemyController : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject missilePrefab;
    public Transform missileSpawnPoint;
    public Transform target;
    public PlayerMovement pm;
    public float initialMissileSpeed = 1f;
    public float maxMissileSpeed = 100f;
    public float accelerationTime = 1f;
    public float fireRate = 1.0f;

    // public float pauseTime = 0.5f;

    private float currentMissileSpeed;
    private float timeUntilNextFire;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // currentMissileSpeed = initialMissileSpeed;
        // timeUntilNextFire = fireRate;
        InvokeRepeating("FireMissile", 0f, fireRate);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
        pm.increaseEnemyKills();
    }

    private void FixedUpdate()
    {
        if (target)
        {
            Vector3 aimDirection = new Vector2(target.position.x, target.position.y) - rb.position;
            float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            rb.rotation = aimAngle;
        }
    }

    async void FireMissile()
    {
        currentMissileSpeed = initialMissileSpeed;
        timeUntilNextFire = fireRate;
        GameObject missile = Instantiate(
            missilePrefab,
            missileSpawnPoint.position + new Vector3(0, 1, 0), // add an offset to the spawn position
            missileSpawnPoint.rotation
        );

        Rigidbody2D missileRigidbody = missile.GetComponent<Rigidbody2D>();
        missileRigidbody.velocity =
            (target.position - missile.transform.position).normalized * initialMissileSpeed;

        await Task.Delay(500);
        AccelerateMissile();
        // Debug.Log("BEFORE:::::: AccelerateMissile invoked " + currentMissileSpeed);
    }

    void AccelerateMissile()
    {
        Debug.Log("AccelerateMissile invoked");
        float elapsedTime = 0f;
        float startSpeed = 0f;
        float targetSpeed = maxMissileSpeed;
        while (elapsedTime < accelerationTime)
        {
            currentMissileSpeed = Mathf.Lerp(
                startSpeed,
                targetSpeed,
                Mathf.Pow(elapsedTime / accelerationTime, 2)
            );
            elapsedTime += Time.deltaTime;
        }
        currentMissileSpeed = targetSpeed;
    }
}
