using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{

    public float force_of_push;
    public bool isBlackHole = false;
    public float eventHorizon = 10f;

    public bool enableTrap = true;
    // private void OnTriggerEnter2D(Collider2D collider2D) {
    //     if (collider2D.gameObject.name != "Triangle") return;
    //     StartCoroutine(ExecuteEffect(collider2D));
    // }

    // private void ExecuteEffect(Collider2D collider2D) {

    // }

    // public Rigidbody2D player_rigid_body; 
    // // Start is called before the first frame update

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Config.isInPlanet = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Config.isInPlanet = false;
        if(collision.name.Contains("FollowEnemy") || collision.name.Contains("Moving Enemy")) {
            EnemyController ec = collision.GetComponent<EnemyController>();
            ec.movementType = MovementType.Follow;
            ec.enableShooting = true;
        }
        else if (collision.name.Contains("BossEnemy")) {
            BossEnemy be = collision.GetComponent<BossEnemy>();
            be.enableShooting = true;
            be.enableFollow = true;
        }
    }

    private bool withinCollider(Collider2D other) {
        bool isCompletelyWithinCircle = true;
        Bounds bounds = other.GetComponent<Renderer>().bounds;
        Vector2[] vertices = new Vector2[4] {
            new Vector2(bounds.min.x, bounds.min.y),
            new Vector2(bounds.min.x, bounds.max.y),
            new Vector2(bounds.max.x, bounds.min.y),
            new Vector2(bounds.max.x, bounds.max.y)
        };

        foreach (Vector2 vertex in vertices)
        {
            if (!GetComponent<CircleCollider2D>().OverlapPoint(vertex))
            {
                isCompletelyWithinCircle = false;
                break;
            }
        }

        return isCompletelyWithinCircle;
    }

    void OnTriggerStay2D(Collider2D ship) {
        if (ship.name.Contains("Player")) {
            float x_error = transform.position.x - ship.transform.position.x;
            float y_error = transform.position.y - ship.transform.position.y;
            float magnitude = Mathf.Pow((Mathf.Pow(x_error, 2f) + Mathf.Pow(y_error, 2f)), 0.5f);

            if(isBlackHole) {
                float distance = Vector2.Distance(transform.position,ship.transform.position);
                if(distance <= eventHorizon) {
                    Config.isDead = true;
                    Destroy(ship.gameObject);
                }
            }

            Vector2 unit_vector = new Vector2((force_of_push*x_error/magnitude), (force_of_push*y_error/magnitude));
            
            ship.attachedRigidbody.AddForce(unit_vector);
            // Debug.Log(force_of_push * unit_vector); 
        }
        
        if(withinCollider(ship)) {
            if(enableTrap && ship.name.Contains("FollowEnemy") || ship.name.Contains("Moving Enemy")) {
                EnemyController ec = ship.GetComponent<EnemyController>();
                ec.movementType = MovementType.None;
                ec.enableShooting = false;
            }
            else if (enableTrap && ship.name.Contains("BossEnemy")) {
                BossEnemy be = ship.GetComponent<BossEnemy>();
                be.enableShooting = false;
                be.enableFollow = false;
            }
        }


    }

}
