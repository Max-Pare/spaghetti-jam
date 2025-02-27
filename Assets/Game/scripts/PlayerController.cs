using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb = null;
    private Camera cam = null;
    private BodyMono feet = null;
    private BodyMono body = null;
    private UnityEvent shootEvent = new UnityEvent();
    private UnityEvent wallJumpEvent = new UnityEvent();
    private GameObject bullet = null;
    private Transform bulletOrigin = null;
    private Transform bulletOriginPivot = null;

    private void Awake() {
        rb = this.GetComponent<Rigidbody>();
        cam = this.GetComponentInChildren<Camera>();
        feet = GameObject.Find("Feet").GetComponent<BodyMono>();
        body = GameObject.Find("Body").GetComponent<BodyMono>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start() {
        baseSpeed *= 1000f;
        shootEvent.AddListener(Shoot);
        bullet = GameObject.Find("Bullet");
        bullet.transform.localScale *= 0.15f;
        bulletOriginPivot = GameObject.Find("BulletOriginPivot").transform;
        bulletOrigin = GameObject.Find("BulletOrigin").transform;
        _wasGrounded = feet.isTouching;
    }

    private void Update() {
        CameraInputHandler();
        WeaponInputHandler();
        HandleAirborneGravity();
        MovementHandler();
        HandleLanding();
    }

    private void WeaponInputHandler(){
        if (Input.GetMouseButtonDown(0)) {
            shootEvent.Invoke();
        }
    }

    void Shoot(){
        GameObject newBullet = GameObject.Instantiate<GameObject>(bullet);
        newBullet.transform.position = bulletOrigin.position;
        newBullet.SetActive(true);
        newBullet.GetComponent<Rigidbody>().AddForce(cam.transform.forward * 1000f);
    }

    private bool _wasGrounded = false;
    void HandleAirborneGravity(){ // cursed
        if (!feet.isTouching && rb.linearVelocity.y >= -9.81f && Time.time >= 1.2f){
            rb.AddForce(Vector3.down * 1150f * (rb.mass * 0.75f) * Time.deltaTime, ForceMode.Acceleration);
        }
    }

    private void HandleLanding(){ // cursed
        if (feet.isTouching && !_wasGrounded) {
            _wasGrounded = feet.isTouching;
            wallJumps = 0;            
        }
        if (!feet.isTouching && _wasGrounded) { _wasGrounded = feet.isTouching; }
    }



    [SerializeField]
    private float baseSpeed = 15f;
    private Vector3 direction = Vector3.zero;
    private float lastJump = 0f;
    private const float JUMP_COOLDOWN = 0.42f;
    private Vector3 _vel = Vector3.zero;
    private float jumpMult = 1f;
    private const int MAX_WALL_JUMPS = 3;
    private int wallJumps = 0;
    private void MovementHandler() {
        HandleJump();
        HandleDirection();
        
        void HandleWallJump(){
            Log("Wall Jumped!");
        }

        void HandleDirection(){
            _vel = rb.linearVelocity;
            if (!Input.anyKey) {
                return;
            }
            if (Input.GetKey(KeyCode.W)) {
                Move(transform.forward);
            }
            if (Input.GetKey(KeyCode.S)) {
                Move(-transform.forward);
            }
            if (Input.GetKey(KeyCode.A)){
                Move(-transform.right * 0.80f);
            }
            if (Input.GetKey(KeyCode.D)){
                Move(transform.right * 0.80f);
            }
        }

        void HandleJump(){
            bool wallJump = false;
            if (!feet.isTouching && !body.isTouching) return;
            if (body.isTouching && !feet.isTouching) {
                if (wallJumps >= MAX_WALL_JUMPS) {
                    return;
                }
                wallJump = true;
            }
            if (!Input.GetKeyDown(KeyCode.Space)) return;
            if(Time.time - lastJump <= JUMP_COOLDOWN) return;
            if (wallJump) { 
                wallJumpEvent.Invoke();
                jumpMult = 1.66f;
                wallJumps++;
            }
            Jump();
            lastJump = Time.time;
            jumpMult = 1f;
            void Jump(){
                rb.AddForce(Vector3.up * (9.5f * rb.mass) * jumpMult, ForceMode.Impulse);
            }
        }

        void Move(Vector3 _dir, float multiplier = 1f) {
            direction = direction.normalized + _dir;
            multiplier = Mathf.Max(0, 1 - (VecAvg(Vec3toVec2(rb.linearVelocity)) / 15f));
            RaycastHit hit;
            if (Physics.Raycast(this.transform.position, direction, out hit, (transform.localScale.x + transform.localScale.z) / 2 * 1.1f) && hit.collider.CompareTag("Ground")) {
                multiplier *= Mathf.Log(hit.distance + 1);
            }
            if (VecMax(Vec3toVec2(rb.linearVelocity)) <= 15f){
                rb.AddForce(direction.normalized * baseSpeed * multiplier * Time.deltaTime, ForceMode.Acceleration);
            }
        }
    }

    Vector2 Vec3toVec2(Vector3 vec) {
        return new Vector2(vec.x, vec.z);
    }

    float VecAvg(Vector3 vec, bool abs = true) {
        if (abs) {
            return (Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z)) / 3;
        }
        return (vec.x + vec.y + vec.z) / 3;
    }

    float VecAvg(Vector2 vec, bool abs = true) {
        if (abs) {
            return (Mathf.Abs(vec.x) + Mathf.Abs(vec.y)) / 2;
        }
        return (vec.x + vec.y) / 2;
    }

    float VecMax(Vector3 vec, bool abs = true) {
        if (abs) {
            return Mathf.Max(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
        }
        return Mathf.Max(vec.x, vec.y, vec.z);
    }

    [SerializeField]
    private float sensitivity = 2.0f;
    private Vector2 rotation = Vector2.zero;
    private void CameraInputHandler(){
		rotation.x += Input.GetAxis("Mouse X") * sensitivity;
		rotation.y += Input.GetAxis("Mouse Y") * sensitivity;
		rotation.y = Mathf.Clamp(rotation.y, -89, 89f);
		var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
		var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

		transform.localRotation = xQuat;
        cam.transform.localRotation = yQuat;
        bulletOriginPivot.transform.localRotation = yQuat;
    }

    private void Log(string message) {
        Debug.Log(message);
    }
}
