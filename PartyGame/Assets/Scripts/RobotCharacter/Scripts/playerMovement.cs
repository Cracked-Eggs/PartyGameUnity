using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour
{
    private GroundCheck gc;
    private InputSystem_Actions inputSystem;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;

    private Rigidbody _rb;
    private Collider playerCollider;

    public bool canRetach;

    [SerializeField] public float _speed;
    [SerializeField] public float _jumpForce;
    [SerializeField] public float customGravity = -9.81f;
    [SerializeField] public float maxFallSpeed = -20f;
    [SerializeField] public bool _isGrounded = false;
    [SerializeField] private Transform cameraTransform; 
    [SerializeField] private float sensitivityX = 10f; 
    [SerializeField] private float sensitivityY = 10f;  
    [SerializeField] private float minYAngle = -60f;  
    [SerializeField] private float maxYAngle = 60f;    
    [SerializeField] private float cameraDistance = 5f; 
    [SerializeField] private float rotationSmoothSpeed = 0.1f;
    [SerializeField] public float _rotationSpeed = 5f;
    [SerializeField] public float rotationThreshold = 1f;

    private Vector2 lookInput;                          
    private Vector3 currentRotation;                   
    private Vector3 rotationSmoothVelocity;
    private float yRot = 0f; 
    private float xRot = 0f; 
    public float ogJumpForce;


    private bool customGravityActive = true;
    public GameObject head, torso, r_Leg, l_Leg, r_Arm, l_Arm, parent;
    public bool isDetached = false;
    private int partsReattachedCount = 0;
    private int totalParts = 6;
    private Vector3 referencePosition;

    public bool _isL_ArmDetached = false;
    public bool _isR_ArmDetached = false;
    public bool _isL_LegDetached = false;
    public bool _isR_LegDetached = false;


    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Quaternion> originalRotations = new Dictionary<GameObject, Quaternion>();
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Transform[]> originalBones = new Dictionary<GameObject, Transform[]>();
    private Dictionary<GameObject, Transform> originalRootBones = new Dictionary<GameObject, Transform>();
    private Dictionary<GameObject, Mesh> originalMeshes = new Dictionary<GameObject, Mesh>();
    private Dictionary<GameObject, ColliderData> originalCollidersData = new Dictionary<GameObject, ColliderData>();

    [System.Serializable]
    public struct ColliderData
    {
        public Vector3 size;
        public Vector3 center;
        public bool isTrigger;

        public ColliderData(BoxCollider boxCollider)
        {
            size = boxCollider.size;
            center = boxCollider.center;
            isTrigger = boxCollider.isTrigger;
        }
    }

    private void Awake()
    {
        inputSystem = new InputSystem_Actions();
        _rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);

        playerCollider.enabled = false;
        head = GameObject.FindGameObjectWithTag("Head");
        torso = GameObject.FindGameObjectWithTag("Torso");
        r_Leg = GameObject.FindGameObjectWithTag("R_Leg");
        l_Leg = GameObject.FindGameObjectWithTag("L_Leg");
        r_Arm = GameObject.FindGameObjectWithTag("R_Arm");
        l_Arm = GameObject.FindGameObjectWithTag("L_Arm");
        parent = GameObject.FindGameObjectWithTag("Parent");
        ogJumpForce = _jumpForce;

        StoreOriginalTransforms(head);
        StoreOriginalTransforms(torso);
        StoreOriginalTransforms(r_Leg);
        StoreOriginalTransforms(l_Leg);
        StoreOriginalTransforms(r_Arm);
        StoreOriginalTransforms(l_Arm);
    }

    private void StoreOriginalTransforms(GameObject part)
    {

        originalPositions[part] = part.transform.localPosition;
        originalRotations[part] = part.transform.localRotation;
        originalScales[part] = part.transform.localScale;

        SkinnedMeshRenderer renderer = part.GetComponent<SkinnedMeshRenderer>();
        if (renderer != null)
        {
            originalBones[part] = renderer.bones;
            originalRootBones[part] = renderer.rootBone;
            originalMeshes[part] = renderer.sharedMesh;
        }
    }

    private void OnEnable()
    {
        _moveAction = inputSystem.Player.Move;
        _moveAction.Enable();
        _lookAction = inputSystem.Player.Look;
        _lookAction.Enable();
        _jumpAction = inputSystem.Player.Jump;
        _jumpAction.performed += OnJump;
        _jumpAction.Enable();

        inputSystem.Player.DetachHead.performed += DetachHead;

        inputSystem.Player.DetachHead.Enable();
        inputSystem.Player.DetachTorso.performed += DetachTorso;
        inputSystem.Player.DetachTorso.Enable();
        inputSystem.Player.DetachRightLeg.performed += DetachRightLeg;
        inputSystem.Player.DetachRightLeg.Enable();
        inputSystem.Player.DetachLeftLeg.performed += DetachLeftLeg;
        inputSystem.Player.DetachLeftLeg.Enable();
        inputSystem.Player.DetachRightArm.performed += DetachRightArm;
        inputSystem.Player.DetachRightArm.Enable();
        inputSystem.Player.DetachLeftArm.performed += DetachLeftArm;
        inputSystem.Player.DetachLeftArm.Enable();
        inputSystem.Player.Detach.performed += OnDetach;
        inputSystem.Player.Detach.Enable();
        inputSystem.Player.Reattach.performed += OnReattach;
        inputSystem.Player.Reattach.Enable();
        inputSystem.Player.ShootRightArm.performed += ShootRightArm;
        inputSystem.Player.ShootRightArm.Enable();
        inputSystem.Player.ShootLeftArm.performed += ShootLeftArm;
        inputSystem.Player.ShootLeftArm.Enable();
        inputSystem.Player.ReattachLeftArm.performed += ReattachLeftArm;
        inputSystem.Player.ReattachLeftArm.Enable();
        inputSystem.Player.ReattachRightArm.performed += ReattachRightArm;
        inputSystem.Player.ReattachRightArm.Enable();
        ;
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _jumpAction.Disable();
        inputSystem.Player.Detach.Disable();
        inputSystem.Player.Reattach.Disable();
        inputSystem.Player.DetachHead.Disable();
        inputSystem.Player.DetachTorso.Disable();
        inputSystem.Player.DetachRightLeg.Disable();
        inputSystem.Player.DetachLeftLeg.Disable();
        inputSystem.Player.DetachRightArm.Disable();
        inputSystem.Player.DetachLeftArm.Disable();
        inputSystem.Player.ReattachLeftArm.Disable();
        inputSystem.Player.ReattachRightArm.Disable();
 
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        Jump();
    }
   

    private void LookAround()
    {
        
        Vector2 lookInput = _lookAction.ReadValue<Vector2>();
        float mouseX = lookInput.x * sensitivityX * Time.deltaTime; 
        float mouseY = lookInput.y * sensitivityY * Time.deltaTime;

        
        yRot += mouseX;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, minYAngle, maxYAngle); 
        
        cameraTransform.localRotation = Quaternion.Euler(xRot, yRot, 0f);
    }



    private void Jump()
    {
        Debug.Log("jumping");
        if (_isGrounded)
        {
            Vector3 jumpForce = Vector3.up * _jumpForce;
            _rb.AddForce(jumpForce);
            SetGrounded(false);
        }
    }

    private void OnDetach(InputAction.CallbackContext context)
    {
        if (!isDetached)
        {
            referencePosition = transform.position;
            DetachPart(head);
            DetachPart(torso);
            DetachPart(r_Leg);
            DetachPart(l_Leg);
            DetachPart(r_Arm);
            DetachPart(l_Arm);
            playerCollider.enabled = true;
            _rb.constraints = RigidbodyConstraints.FreezeAll;

            isDetached = true;
            customGravityActive = false;
        }
    }

    private void OnReattach(InputAction.CallbackContext context)
    {
        if (isDetached || canRetach)
        {
            StartCoroutine(ShakeAndReattach(head));
            StartCoroutine(ShakeAndReattach(torso));
            StartCoroutine(ShakeAndReattach(r_Leg));
            StartCoroutine(ShakeAndReattach(l_Leg));
            StartCoroutine(ShakeAndReattach(r_Arm));
            StartCoroutine(ShakeAndReattach(l_Arm));

            canRetach = false;

            isDetached = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            playerCollider.enabled = false;
            _isR_LegDetached = false;
            _isL_LegDetached = false;
            _jumpForce = ogJumpForce;

        }
    }

    private void DetachPart(GameObject part)
    {
        if (part == null) return;

        canRetach = true;

        // Store the world scale before detaching (used for the collider only)
        Vector3 worldScale = part.transform.lossyScale;

        // Detach part from parent without affecting its local transformation
        part.transform.SetParent(null); // Keeps the world transformation

        // Adjust only the collider, not the object's scale
        BoxCollider partCollider = part.GetComponent<BoxCollider>();
        if (partCollider != null)
        {
            // Scale the collider's size and center based on world scale
            //partCollider.size = Vector3.Scale(partCollider.size, worldScale);
            //partCollider.center = Vector3.Scale(partCollider.center, worldScale);

            Debug.Log($"After Detach - Collider Size: {partCollider.size}, Collider Center: {partCollider.center}");
        }

        // SkinnedMeshRenderer baking
        SkinnedMeshRenderer skinnedMesh = part.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMesh != null)
        {
            Mesh bakedMesh = new Mesh();
            skinnedMesh.BakeMesh(bakedMesh);
            MeshFilter meshFilter = part.AddComponent<MeshFilter>();
            meshFilter.mesh = bakedMesh;
            MeshRenderer meshRenderer = part.AddComponent<MeshRenderer>();
            meshRenderer.materials = skinnedMesh.materials;
            Destroy(skinnedMesh);
        }

        // Add Rigidbody for physics interaction
        Rigidbody rb = part.AddComponent<Rigidbody>();
        rb.mass = 1f;

        // No need to modify part's localScale
    }

    private void ReattachPart(GameObject part)
    {
        if (part == null) return;

       
        MeshRenderer meshRenderer = part.GetComponent<MeshRenderer>();
        if (meshRenderer != null) Destroy(meshRenderer);

        MeshFilter meshFilter = part.GetComponent<MeshFilter>();
        if (meshFilter != null) Destroy(meshFilter);

        SkinnedMeshRenderer skinnedMesh = part.AddComponent<SkinnedMeshRenderer>();
        skinnedMesh.bones = originalBones[part];
        skinnedMesh.rootBone = originalRootBones[part];
        skinnedMesh.sharedMesh = originalMeshes[part];

        BoxCollider partCollider = part.GetComponent<BoxCollider>();
        if (partCollider != null)
        {
           
            ColliderData originalColliderData = originalCollidersData[part];
            partCollider.size = originalColliderData.size;
            partCollider.center = originalColliderData.center;
        }

        
        part.transform.SetParent(parent.transform);
        part.transform.localPosition = originalPositions[part];
        part.transform.localRotation = originalRotations[part];
        partsReattachedCount++;

      
        if (partsReattachedCount >= totalParts)
        {
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }
            partsReattachedCount = 0;
        }
    }


    private void DetachHead(InputAction.CallbackContext context)
    {
        Debug.Log("DetachHead called");
        DetachPart(head);
    }

    private void DetachTorso(InputAction.CallbackContext context)
    {
        DetachPart(torso);
    }

    private void DetachRightLeg(InputAction.CallbackContext context)
    {
        DetachPart(r_Leg);
        _isR_LegDetached = true;
    }

    private void DetachLeftLeg(InputAction.CallbackContext context)
    {
        DetachPart(l_Leg);
        _isL_LegDetached = true;
    }

    private void DetachRightArm(InputAction.CallbackContext context)
    {
        DetachPart(r_Arm);
    }

    private void DetachLeftArm(InputAction.CallbackContext context)
    {
        DetachPart(l_Arm);
    }

    private void ShootRightArm(InputAction.CallbackContext context)
    {
        if (!_isR_ArmDetached)
        {
            DetachPart(r_Arm);  // Detach the right arm
            Rigidbody rb = r_Arm.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * 5000f);  // Adjust force as needed
            }
            _isR_ArmDetached = true;
        }
    }

    private void ShootLeftArm(InputAction.CallbackContext context)
    {
        if (!_isL_ArmDetached)
        {
            DetachPart(l_Arm);
            Rigidbody rb = l_Arm.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * 5000f);
            }
            _isL_ArmDetached = true;
            
        }
    }

    private void ReattachRightArm(InputAction.CallbackContext context)
    {
        if (_isR_ArmDetached)
        {
            
            StartCoroutine(ShakeAndReattach(r_Arm));
           

            canRetach = false;
            _isR_ArmDetached = false;


        }

    }
    private void ReattachLeftArm(InputAction.CallbackContext context)
    {
        
        if (_isL_ArmDetached)
        {
            Debug.Log("com");
            StartCoroutine(ShakeAndReattach(l_Arm));
            canRetach = false;

            _isL_ArmDetached = false;
        }

    }



    public void SetGrounded(bool grounded)
    {
        _isGrounded = grounded;
    }
    private IEnumerator ShakeAndReattach(GameObject part)
    {
        
        Rigidbody partRb = part.GetComponent<Rigidbody>();
        if (partRb != null)
        {
            Destroy(partRb); 
        }

       
        part.transform.SetParent(parent.transform);

        Vector3 originalPosition = originalPositions[part];
        Quaternion originalRotation = originalRotations[part];
        Vector3 shakeOffset = new Vector3(0.1f, 0, 0);  
        float reattachSpeed = 5f;  
        float rotationSpeed = 5f;  
        float reattachDistanceThreshold = 0.1f;

        while (Vector3.Distance(part.transform.localPosition, originalPosition) > reattachDistanceThreshold)
        {
           
            part.transform.localPosition = Vector3.Lerp(part.transform.localPosition, originalPosition, reattachSpeed * Time.deltaTime);

            
            part.transform.localRotation = Quaternion.Slerp(part.transform.localRotation, originalRotation, rotationSpeed * Time.deltaTime);

            yield return null;  
        }

       
        part.transform.localPosition = originalPosition;
        part.transform.localRotation = originalRotation;
    }



    private void Update()
    {

        
        if (_moveAction != null)
        {
            Vector2 movementInput = _moveAction.ReadValue<Vector2>();

            
            Vector3 forward = transform.forward; 
            Vector3 right = transform.right;

            
            Vector3 movement = (forward * movementInput.y + right * movementInput.x).normalized * _speed;

            
            Vector3 targetVelocity = new Vector3(movement.x, _rb.velocity.y, movement.z); 
            _rb.velocity = Vector3.Lerp(_rb.velocity, targetVelocity, 0.1f);

            

           
            if (movement.magnitude > 0.1f) 
            {
                
                Quaternion targetRotation = Quaternion.LookRotation(movement);

                
                if (Mathf.Abs(movementInput.y) > rotationThreshold)
                {
                    
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
                }
                else if (Mathf.Abs(movementInput.x) > rotationThreshold)
                {
                    
                    Quaternion horizontalRotation = Quaternion.LookRotation(movement);
                    transform.rotation = Quaternion.Slerp(transform.rotation, horizontalRotation, Time.deltaTime * _rotationSpeed);
                }
            }

            if(_isR_LegDetached && !_isL_LegDetached || !_isR_LegDetached && _isL_LegDetached)
            {

                _jumpForce = 110f;
                Jump();

            }

            if(_isL_LegDetached && _isR_LegDetached)
            {

                _jumpForce = ogJumpForce;
            }
        }






        //LookAround();

        if (customGravityActive)
        {
            if (_rb.velocity.y > maxFallSpeed)
            {
                _rb.AddForce(Vector3.up * customGravity, ForceMode.Acceleration);
            }
        }
        else
        {
            _rb.AddForce(Vector3.up * customGravity, ForceMode.Acceleration);
        }

        
    }

}