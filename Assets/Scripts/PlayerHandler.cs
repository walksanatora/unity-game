using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerHandler : MonoBehaviour
{
    #region structs
    [Serializable]
    public struct RespawnValues{
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Camera;
    }
    [Serializable]
    public struct Objects{
        public GameObject camera;
        public AudioSource jumpSfx;
        //public WorldQLClient temp;
    }
    [Serializable]
    public struct MovementValues{
        public bool useMouse;
        public float sensitivity;
        public float speedModifier;
        public float jumpModifier;
        public float maxSpeed;
        public float lowest;
    }
    #endregion
    //public external variable
    public MovementValues MovementVariables;
    public RespawnValues RespawnVariables;
    public Objects GameObjects;

    private Rigidbody rb;
    int fromBool(bool val){
        if(val){
            return(1);
        }else{
            return(0);
        }
    }

    void respawn(){
        transform.position = RespawnVariables.Position;
        rb.velocity = new Vector3(0,0,0);
        Quaternion l = new Quaternion(0,0,0,1);
        l.eulerAngles = RespawnVariables.Camera;
        GameObjects.camera.transform.rotation = l;
        Quaternion ll = new Quaternion(0,0,0,1);
        ll.eulerAngles = RespawnVariables.Rotation;
        transform.rotation = ll;
    }

    // Start is called before the first frame update
    void Start()
    {
        //get the component ridigib body
        rb = gameObject.GetComponent<Rigidbody>();
        //lock the mouse to the screen
        Cursor.lockState = CursorLockMode.Locked;
        print("Starting Player Controller");
    }

    // Update is called once per frame
    void Update()
    {
        #region exit
        //exit the game when `esc` is pressed
        if(Input.GetKey(KeyCode.Escape)){
            print("exiting Game");
            /*if (GameObjects.temp != null){
                GameObjects.temp.close();
            }*/
            Application.Quit(0);
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        #endregion
        #region debug
        #if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        //respawn the player if z is pressed in editor/debug
        if(Input.GetKey(KeyCode.Z)){
            respawn();
        }
        #endif
        #endregion
        #region movement
        //get the vertical and horizontal axises for movement
        float cx = Input.GetAxis("Horizontal");
        float cy = Input.GetAxis("Vertical");
        int jump = fromBool(Input.GetKey(KeyCode.Space));

        //construct Vectors for movement
        Vector3 horizontal = Vector3.zero;
        if (MovementVariables.useMouse){
            horizontal = transform.right * cx * MovementVariables.speedModifier;
        }
        Vector3 forward = transform.forward * cy * MovementVariables.speedModifier;
        Vector3 vertical;
        bool jumping = Physics.Raycast(
                transform.position, //the position the raycast starts at
                Vector3.down, //the direction of the raycast
                1 //length of the ray
            );
        if(jumping &! GameObjects.jumpSfx.isPlaying)
        {
            vertical = transform.up * jump * MovementVariables.jumpModifier;
            Vector3 rbVelocity = rb.velocity;
            if(rbVelocity.y < 0 & jump != 0){
                rbVelocity.y = 0;
                rb.velocity = rbVelocity;
            }
        }
        else
        {
            vertical = Vector3.zero;
        }
        //combine movement from the axis
        Vector3 combine = horizontal + forward + vertical;
        //slow down the player if they are not pressing any movement keys
        if ((combine == Vector3.zero) &! (rb.velocity == Vector3.zero)){
            combine = rb.velocity * -0.1f;
            combine.y = 0f;//except on the y-axis
        }

        if (jump == 1 & jumping &! GameObjects.jumpSfx.isPlaying ){
            GameObjects.jumpSfx.Play();
        }
        //apply forces to the gameObject
        rb.AddForce(combine,ForceMode.Impulse);
        #endregion
        #region camera
        float rx = 0;
        float ry = 0;
        if (MovementVariables.useMouse){
            rx = MovementVariables.sensitivity * Input.GetAxis("Mouse X");
            ry = MovementVariables.sensitivity * Input.GetAxis("Mouse Y") * -1;
        } else {
            rx = MovementVariables.sensitivity * cx;
            //ry = MovementVariables.sensitivity * Input.GetAxis("Mouse Y");
        }
        //rotate the player across the y-axis
        Quaternion ro = transform.rotation;
        Vector3 rot = transform.rotation.eulerAngles;
        rot += new Vector3(0,rx,0);
        ro.eulerAngles = rot;
        transform.rotation = ro;

        //rotate the GameObjects.camera across the x-axis
        Quaternion crot = GameObjects.camera.transform.rotation;
        Vector3 crotc = crot.eulerAngles;
        crotc += new Vector3(ry,0,0);
        crot.eulerAngles = crotc;
        GameObjects.camera.transform.rotation = crot;
        #endregion
        #region speed limit
        //limit max speed to MovementVariables.maxSpeed
        if(rb.velocity.magnitude > MovementVariables.maxSpeed){
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, MovementVariables.maxSpeed);
        }
        #endregion
        #region respawn
        //if transform.position falls below MovementVariables.lowest teleport to respawnPos
        if(transform.position.y < MovementVariables.lowest){
            respawn();
        }
        #endregion
    }
}
