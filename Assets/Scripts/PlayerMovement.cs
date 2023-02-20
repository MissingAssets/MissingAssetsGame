using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

    public float playerSpeed = 0f; 
    public float orientation; 
    

    public Rigidbody2D player_rigid_body;
    private float thrustPower = 1f; // 0.9f for local testing, 1.8f otherwise

    public int maxEnemiesLimit = 3;

    public PlayerWeapon weapon;

    public bool isDead = false;
    public int numberOfEnemiesKilled = 0;
    public bool isInvulnerable = false;

    private static long health = 100;
    private static int regHealthReduction = 5;

    // Thrust
    public ThrustController tc;
    private bool isReducingThrust = false;
    private float thrustReductionStartTime = 0f;
    private bool isThrustKeyReleased = true;

    public void resetHealth()
    {
        health = 100;
    }

    public long getHealth()
    {
        return health;
    }
    
    public void reduceHealth(int value)
    {
        health = health - value;
        if(health == 0) {
            isDead = true;
            // Analytics to be sent here
        }
    }

    public void increaseEnemyKills(){
        numberOfEnemiesKilled+=1;
    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        if (!isInvulnerable) {
            reduceHealth(regHealthReduction);
            if (collision.gameObject.name.Contains("Asteroid")) {
                reduceHealth((int) (collision.relativeVelocity.magnitude));
            }
        }
        Debug.Log(health);
        if(health <= 0) {
            Destroy(gameObject);
            StatisticsManager.buildAnaltyicsDataObjAndPush(0,1,"DeathByEnemy","0%",0,"player_termination","enemy");
            StatisticsManager.buildAnaltyicsDataObjAndPush(numberOfEnemiesKilled,1,"NumEnemiesKilled","0%",0,"numEnemiesKilled","enemy");
        }
    }
    
    void enableRotation() {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        Vector2 aimDirection = mousePosition - player_rigid_body.position;
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        player_rigid_body.rotation = aimAngle;

        Vector2 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation= Quaternion.AngleAxis(angle - 90, Vector3.forward);
        transform.rotation = rotation;
        orientation = aimAngle; 
    }

    public void restart()
   {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        resetHealth();
   }

    void gameOver()
    {
        Invoke ("restart", 5);
    }

    private void applyForceOnPlayer(){
        Vector2 force = new Vector2(-thrustPower * Mathf.Sin(Mathf.Deg2Rad * orientation), thrustPower * Mathf.Cos(Mathf.Deg2Rad * orientation)); 
        player_rigid_body.AddForce(force);
    }

    private void allowLimitedThrust(){
        // Now you can continuously press W key and keep applying force to 
        // change direction and movement. After every second the thrust reduces
        // if you long press W
        if (tc.getThrust() > 0.04){
            if (Input.GetKeyDown(KeyCode.W)) {
                // Reduce thrust instantly
                applyForceOnPlayer();
                tc.reduceThrust(Config.thrustReductionAmount);
                isThrustKeyReleased = false;
            } else if (Input.GetKey(KeyCode.W)) {
                // Reduce thrust continuously if key is held down
                if (!isReducingThrust) {
                    isReducingThrust = true;
                    thrustReductionStartTime = Time.time;
                }
                if (Time.time - thrustReductionStartTime >= Config.thrustReductionDelay) {
                    applyForceOnPlayer();
                    tc.reduceThrust(Config.thrustReductionAmount*Time.deltaTime);
                }
                isThrustKeyReleased = false;
            } else if(!isThrustKeyReleased){
                // Reset variables when key is released
                isReducingThrust = false;
                thrustReductionStartTime = 0f;
                isThrustKeyReleased = true;
            }
        }
    }
    
    void Start(){
        player_rigid_body = this.GetComponent<Rigidbody2D>();
        // player_rigid_body.velocity = Vector3.right * 2;
        tc = new ThrustController();      
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log(orientation);

        enableRotation(); 

        if(numberOfEnemiesKilled == maxEnemiesLimit)
        {
            gameOver();
        }

        if (Config.useThrustControl){
            allowLimitedThrust();
        }
        else {
            if(Input.GetKeyDown(KeyCode.W)) {
                applyForceOnPlayer();
            }
        }
        

        // if (Input.GetKey("down")) {

        // }

        // if (Input.GetKey("right") || Input.GetKey(KeyCode.D)) {
        //     Vector2 force = new Vector2(thrustPower * Mathf.Cos(Mathf.Deg2Rad * orientation), thrustPower * Mathf.Sin(Mathf.Deg2Rad * orientation)); 
        //     player_rigid_body.AddForce(force);
        // }

        // if (Input.GetKey("down") || Input.GetKey(KeyCode.S)) {
        //     Vector2 force = new Vector2(thrustPower * Mathf.Sin(Mathf.Deg2Rad * orientation), -thrustPower * Mathf.Cos(Mathf.Deg2Rad * orientation)); 
        //     player_rigid_body.AddForce(force);
        // }
        
        // if (Input.GetMouseButton(0)) {
        //     Vector2 force = new Vector2(-thrustPower * Mathf.Sin(Mathf.Deg2Rad * orientation), thrustPower * Mathf.Cos(Mathf.Deg2Rad * orientation)); 
        //     player_rigid_body.AddForce(force);
        //     // StatisticsManager.buildAnaltyicsDataObjAndPush(level:0, type:"ThrustPress")
        // }

        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            weapon.Fire();
        }
    }
}
