using System.Collections;
using System.Collections.Generic;
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


    private bool customGravityActive = true;
    public GameObject head, torso, r_Leg, l_Leg, r_Arm, l_Arm, parent;
    private bool isDetached = false;
    private int partsReattachedCount = 0;
    private int totalParts = 6;




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
        playerCollider.enabled = false;
        head = GameObject.FindGameObjectWithTag("Head");
        torso = GameObject.FindGameObjectWithTag("Torso");
        r_Leg = GameObject.FindGameObjectWithTag("R_Leg");
        l_Leg = GameObject.FindGameObjectWithTag("L_Leg");
        r_Arm = GameObject.FindGameObjectWithTag("R_Arm");
        l_Arm = GameObject.FindGameObjectWithTag("L_Arm");
        parent = GameObject.FindGameObjectWithTag("Parent");


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
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        Jump();
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
            //DetachPart(head);
            DetachPart(torso);
            DetachPart(r_Leg);
            DetachPart(l_Leg);
            DetachPart(r_Arm);
            DetachPart(l_Arm);
            //playerCollider.enabled = true;
            //_rb.constraints = RigidbodyConstraints.FreezeAll;

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

        }
    }

    private void DetachPart(GameObject part)
    {
        canRetach = true;

        if (part == null) return;

        part.transform.SetParent(null);

        BoxCollider partCollider = part.GetComponent<BoxCollider>();
        if (partCollider != null)
        {

            ColliderData colliderData = new ColliderData(partCollider);

            Debug.Log($"Before Detach - Original Size: {colliderData.size}, Original Center: {colliderData.center}");


            partCollider.size = new Vector3(colliderData.size.x / 10, colliderData.size.y / 10, colliderData.size.z / 10);
            partCollider.center = new Vector3(colliderData.center.x / 10, colliderData.center.y / 10, colliderData.center.z / 10);

            Debug.Log($"After Detach - Modified Size: {partCollider.size}, Modified Center: {partCollider.center}");
        }


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

        Rigidbody rb = part.AddComponent<Rigidbody>();
        rb.mass = 1f;
        part.transform.localScale = originalScales[part];
    }

    private void ReattachPart(GameObject part)
    {
        if (part == null) return;

        


        MeshRenderer meshRenderer = part.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Destroy(meshRenderer);
        }

        MeshFilter meshFilter = part.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Destroy(meshFilter);
        }


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


        part.transform.parent = parent.transform;


        part.transform.localPosition = originalPositions[part];
        part.transform.localRotation = originalRotations[part];
        part.transform.localScale = originalScales[part];


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
    }

    private void DetachLeftLeg(InputAction.CallbackContext context)
    {
        DetachPart(l_Leg);
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
        if (!isDetached)
        {
            DetachPart(r_Arm);  // Detach the right arm
            Rigidbody rb = r_Arm.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * 2000f);  // Adjust force as needed
            }
        }
    }

    private void ShootLeftArm(InputAction.CallbackContext context)
    {
        if (!isDetached)
        {
            DetachPart(l_Arm);
            Rigidbody rb = l_Arm.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * 2000f);
            }
        }
    }

    public void SetGrounded(bool grounded)
    {
        _isGrounded = grounded;
    }
    private IEnumerator ShakeAndReattach(GameObject part)
    {
        part.transform.SetParent(parent.transform);
        Destroy(part.GetComponent<Rigidbody>());
        Vector3 originalPosition = originalPositions[part];
        Quaternion originalRotation = originalRotations[part];
        Vector3 shakeOffset = new Vector3(0.1f, 0, 0);
        float shakeDuration = 0.3f;
        float shakeFrequency = 10f;

        // Shaking effect
        for (float t = 0; t < shakeDuration; t += Time.deltaTime)
        {
            float x = Mathf.Sin(t * shakeFrequency) * shakeOffset.x;
            part.transform.localPosition = originalPosition + new Vector3(x, 0, 0);
            yield return null;
        }

        // Directly set back to original position and rotation after shaking
        part.transform.localPosition = originalPosition;
        part.transform.localRotation = originalRotation;

        // Reparent to the parent object
       
    }

    private void FixedUpdate()
    {
        if (_moveAction != null)
        {
            Vector2 movementInput = _moveAction.ReadValue<Vector2>();
            Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y).normalized * _speed;
            movement.y = _rb.velocity.y;
            _rb.velocity = movement;
        }

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

        if (_rb.velocity.y <= 0)
        {

            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
            {
                SetGrounded(true);
            }
            else
            {
                SetGrounded(false);
            }
        }
        else
        {
            SetGrounded(false);
        }
    }
}