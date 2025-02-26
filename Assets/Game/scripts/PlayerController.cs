using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb = null;
    private Camera cam = null;
    private BodyMono feet = null;
    private BodyMono body = null;
    private UnityEvent shootEvent = new UnityEvent();
    private UnityEvent landEvent = new UnityEvent();
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
        landEvent.AddListener(ResetJump);
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
        MovementInputHandler();
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
    void HandleAirborneGravity(){
        if (!feet.isTouching && rb.linearVelocity.y >= -9.81f && Time.time >= 1.2f){
            rb.AddForce(Vector3.down * 1150f * (rb.mass * 0.75f) * Time.deltaTime, ForceMode.Acceleration);
        }
    }

    private void HandleLanding(){
        if (feet.isTouching && !_wasGrounded) {
            _wasGrounded = feet.isTouching;
            Log("Landed");
            wallJumps = 0;            
        }
        if (!feet.isTouching && _wasGrounded) { _wasGrounded = feet.isTouching; }
    }

    private void ResetJump(){ // landEvent

    }

    [SerializeField]
    private float baseSpeed = 20f;
    private Vector3 direction = Vector3.zero;
    private float lastJump = 0f;
    private const float jumpCooldown = 0.2f;
    public Vector3 _vel = Vector3.zero;
    float jumpMult = 1f;
    const int MAX_WALL_JUMPS = 3;
    private int wallJumps = 0;
    void MovementInputHandler() {
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

        HandleJump();
        
        void HandleJump(){
            bool wallJump = false;
            if (body.isTouching && !feet.isTouching) {
                if (wallJumps >= MAX_WALL_JUMPS) {
                    return;
                }
                wallJump = true;
            }
            if (!Input.GetKeyDown(KeyCode.Space)) return;
            if(Time.time - lastJump <= jumpCooldown) return;
            if (wallJump) { 
                jumpMult = 2f;
                wallJumps++;
            }
            Jump();
            lastJump = Time.time;
            jumpMult = 1f;
            void Jump(){
                rb.AddForce(Vector3.up * (11f * rb.mass) * jumpMult, ForceMode.Impulse);
            }
        }

        void Move(Vector3 _dir, float multiplier = 1f) {
            float _startMult = multiplier;
            direction = direction.normalized + _dir;
            RaycastHit hit;
            if (Physics.Raycast(this.transform.position, direction, out hit, (transform.localScale.x + transform.localScale.z) / 2 * 1.1f) && hit.collider.CompareTag("Ground")) {
                multiplier *= Mathf.Log(hit.distance + 1);
            }
            multiplier = Mathf.Max(0, 1 - (VecAvg(rb.linearVelocity) / 15f));
            if (VecMax(rb.linearVelocity) <= 15f){
                rb.AddForce(direction.normalized * baseSpeed * multiplier * Time.deltaTime, ForceMode.Acceleration);
            }
            multiplier = _startMult;
        }
    }

    float VecAvg(Vector3 vec, bool abs = true) {
        if (abs) {
            return (Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z)) / 3;
        }
        return (vec.x + vec.y + vec.z) / 3;
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
