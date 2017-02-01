using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public Camera newcamera;
    public float speed = 60f;            // The speed that the player will move at.

    //public GameObject hole;            //The position of the hole

    Vector3 movement;                   // The vector to store the direction of the player's movement.
   // Animator anim;                      // Reference to the animator component.
    Rigidbody playerRigidbody;          // Reference to the player's rigidbody.
    public LayerMask floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    float camRayLength = 100000000.0f;          // The length of the ray from the camera into the scene.

    void Awake()
    {
        // Create a layer mask for the floor layer.
        //floorMask = LayerMask.GetMask("Floor");

        // Set up references.
       // anim = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
    }


    void FixedUpdate()
    {
        // Store the input axes.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        int hit_action = 0;

        if (Input.GetButton("Fire1"))
        {
            hit_action=1;          

        }

        // Move the player around the scene.
        Move(h, v);

        // Turn the player to face the mouse cursor.
        Turning();

        // Animate the player.
        //Animating(h, v, hit_action);
        

    }

    void Move(float h, float v)
    {
        // Set the movement vector based on the axis input.
        movement.Set(h, 0f, v);

        // Normalise the movement vector and make it proportional to the speed per second.
        movement = movement.normalized * speed * Time.deltaTime;

        // Move the player to it's current position plus the movement.
        playerRigidbody.MovePosition(transform.position + movement);

        //Check if the character is on the hole
        GameObject aguj = GameObject.FindGameObjectWithTag("Hole");
        Scene scene = SceneManager.GetActiveScene();

        //if ((playerRigidbody.transform.position.z > aguj.transform.position.z - (aguj.GetComponent<Collider>().bounds.size.z / 2)) && (playerRigidbody.transform.position.z < aguj.transform.position.z + (aguj.GetComponent<Collider>().bounds.size.z / 2)))
        //{
        //    Debug.Log("DENTRO");
        //    //Deberíamos esperar cierto tiempo para que el jugador vea la caída del personaje en el agujero, y después reiniciar.
        //    //    SceneManager.LoadScene(scene.name);
        //}
        

        if (playerRigidbody.transform.position.y < -1.81f)
            playerRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Turning()
    {
        // Create a ray from the mouse cursor on screen in the direction of the camera.

        Ray camRay = newcamera.ScreenPointToRay(Input.mousePosition);

        // Create a RaycastHit variable to store information about what was hit by the ray.
        RaycastHit floorHit;

        // Perform the raycast and if it hits something on the floor layer...
        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
        {
            // Create a vector from the player to the point on the floor the raycast from the mouse hit.
            Vector3 playerToMouse = floorHit.point - transform.position;

            // Ensure the vector is entirely along the floor plane.
            playerToMouse.y = 0f;

            // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);

            // Set the player's rotation to this new rotation.
            playerRigidbody.MoveRotation(newRotation);
        }
    }
    /*
    void Animating(float h, float v, int hit_action)
    {
        // Create a boolean that is true if either of the input axes is non-zero.
        bool walking = h != 0f || v != 0f;
        bool hitting = hit_action != 0f;

        // Tell the animator whether or not the player is walking.


        anim.SetBool("IsWalking", walking);
        anim.SetBool("Hit2", hitting);

        
    }*/
}