using MelonLoader;
using UnityEngine;
using BoneLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.Barracuda;
using JetBrains.Annotations;
using SLZ.Rig;
using SLZ.Bonelab;
using SLZ.Marrow;
using SLZ.Marrow.Input;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace runwall
{
    public class Class1
    {
        private LayerMask whatIsWall;
        private LayerMask whatIsGround;
        public float wallRunForce;
        public float maxWallRunTime;
        private float wallRunTimer;

        public float wallCheckDistance;
        private RaycastHit leftWallhit;
        private RaycastHit rightWallhit;
        private bool wallLeft;
        private bool wallRight;
        private float horizontalInput;
        private float verticalInput;

        public Transform orientation;
        public Transform headTransform;
        private Rigidbody rb;
        public GameObject player;
        private Vector3 playerHeadOrigin;
        private Vector3 playerFeetOrigin;
        private bool isMoving;
        public float wallRunSpeed;
        public Vector2 controllerStick;
        

        public movementState State;
        public enum movementState
        {
            iswallrunning
        }
        public bool wallRunning;

        private SLZ.Marrow.Input.XRController playerLcontroller;

        private void Awake()
        {
            
        }

        
        
        

        private void Start()
        {
            Transform headTransform = BoneLib.Player.playerHead.GetComponent<Transform>();
            playerHeadOrigin = new Vector3(headTransform.transform.position.x, headTransform.transform.position.y, headTransform.transform.position.z);
            playerFeetOrigin = new Vector3(headTransform.transform.position.x, headTransform.transform.position.y - BoneLib.Player.rigManager.avatar.height + 0.3f, headTransform.transform.position.z);
            // takes players head transform and subtracts avatar height, but adds 0.3 as a buffer just in case
            wallCheckDistance = 0.3f;



        }
        private void Update()
        {
            CheckForWall();
            PlayerMovingCheck();
            StateMachine();
            wallRunSpeed = BoneLib.Player.rigManager.avatar.speed;
            player = Player.rigManager.gameObject;
            rb = Player.rigManager.gameObject.GetComponent<Rigidbody>();
            wallRunForce = 200;

            playerLcontroller = Player.rigManager.gameObject.GetComponent<SLZ.Marrow.Input.XRController>();

            controllerStick = Player.controllerRig.leftController._thumbstickAxis;
            horizontalInput = controllerStick.x;
            verticalInput = controllerStick.y;
            

            

        }
        private void PlayerMovingCheck()
        {
            if (rb.velocity.magnitude > 1)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
        }
        private void CheckForWall()
        {
            wallRight = Physics.Raycast(BoneLib.Player.playerHead.transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
            wallLeft = Physics.Raycast(BoneLib.Player.playerHead.transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
        }

        private bool AboveGround()
        {
            return !Physics.Raycast(playerFeetOrigin, Vector3.down, 0.2f * BoneLib.Player.rigManager.avatar.height, 0x2000);
        }

        private void StateMachine()
        {
            if((wallLeft || wallRight) && isMoving == true && AboveGround())
            {
                if(wallRunning == false)
                    StartWallRun();
            }
            else
            {
                if(wallRunning == true)
                    StopWallRun();
            }
        }

        private void FixedUpdate()
        {

        }

        private void StartWallRun()
        {
            wallRunning = true;
        }

        private void WallRunningMovement()
        {
            rb.useGravity = false;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

            Vector3 wallForward = Vector3.Cross(wallNormal, player.transform.up);

            if ((headTransform.forward - wallForward).magnitude > (headTransform.forward - -wallForward).magnitude)
                wallForward = -wallForward;

            rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

            if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))

                rb.AddForce(-wallNormal * 100, ForceMode.Force);

        }
        
        private void StopWallRun()
        {
            wallRunning = false;
        }
    }
}