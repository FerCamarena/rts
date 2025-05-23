using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CrowdAI : UnitAI {
    // ? DEBUG======================================================================================================================================

    // ? PARAMETERS=================================================================================================================================
    // * VISUALS

    // * REFERENCES

    // * COMPONENTS
    [SerializeField] private NavMeshAgent selfAgent;
    [SerializeField] private Animator ap;

    // * ATTRIBUTES
    [SerializeField] public Transform target;
    [SerializeField] public State state = State.Idle;
    [SerializeField] public Vector3 destination = Vector3.zero;
    [SerializeField] private float speed = 1.0f;
    [SerializeField, Min(0)] protected float baseCD = 1.0f;
    [SerializeField, Min(0)] protected float currCD = 0.0f;
    public int tier = 1;

    // * INTERNAL

    // ? BASE METHODS===============================================================================================================================
    protected override void Awake() {
        base.Awake();

        //Storing defaults
        this.selfAgent ??= this.GetComponent<NavMeshAgent>();
        this.ap ??= this.GetComponent<Animator>();
    }

    protected override void Start() {
        base.Start();
    }

    protected override void Update() {
        base.Update();

        if (this.state == State.Move && 
        Vector3.Distance(this.transform.position, this.destination) <= 0.5f) {
            this.state = State.Idle;
        }
        
        //Animate
        this.selfAgent.speed = this.state == State.Idle ? 0.0f : this.speed;
        this.selfAgent.stoppingDistance = this.state == State.Attack ? 1.0f : 0.0f;
        if (this.target && this.state == State.Attack) {
            this.destination = target.position;
            this.selfAgent.destination = this.destination;
            this.ap.Play("attack");
        } else if (this.state == State.Move) {
            this.ap.Play("move");
            this.selfAgent.destination = this.destination;
        }


        if (this.currCD <= 0.0f && this.state == State.Attack) {
            if (!this.target) this.state = State.Idle;
            else if (Vector3.Distance(this.transform.position, this.target.position) <= 2.0f) {
                UnitAI enemy = this.target.GetComponent<UnitAI>();
                if (enemy) enemy.TakeDamage(1.0f);
                this.currCD = this.baseCD;
            }
        }

        if (this.currCD > 0.0f) this.currCD -= Time.deltaTime;
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
    }

    // ? CUSTOM METHODS=============================================================================================================================
    protected void CompleteJump() {
        //Detect movement
        if (this.selfAgent.velocity.magnitude > this.selfAgent.speed / 10.0f) {
            this.selfAgent.velocity *= 0.2f;
            return;
        }

        this.ap.Play("idle");
    }

    public void SetTarget(Transform newTarget) {
        this.state = State.Attack;
        this.target = newTarget;
    }
    
    public void DivideUnit() {
        if (tier > 1) {
            CrowdAI copy_1 = Instantiate(this.gameObject, transform.position, Quaternion.identity).GetComponent<CrowdAI>();
            CrowdAI copy_2 = Instantiate(this.gameObject, transform.position, Quaternion.identity).GetComponent<CrowdAI>();

            copy_1.destination = this.transform.position + Vector3.right;
            copy_1.state = CrowdAI.State.Move;
            copy_1.tier = this.tier-1;
            copy_1.team = this.team;
            copy_1.faction = this.faction;
            
            copy_2.destination = this.transform.position - Vector3.left;
            copy_2.state = CrowdAI.State.Move;
            copy_2.tier = this.tier-1;
            copy_2.team = this.team;
            copy_2.faction = this.faction;
            
            if (team == 1) manager.player1Units += 2;

            Destroy(this.gameObject);
        }
    }

    // ? EVENT METHODS==============================================================================================================================

    protected virtual void OnTriggerStay(Collider detection) {
        if (detection.TryGetComponent<UnitAI>(out UnitAI unit) && unit.team != this.team) {
            if (!this.target) SetTarget(unit.transform);
        }
    }

    protected virtual void OnTriggerExit(Collider detection) {
        if (detection.TryGetComponent<UnitAI>(out UnitAI unit) && unit.team != this.team) {
            this.state = State.Idle;
            this.target = null;
        }
    }
    public override void TakeDamage(float damageAmount)
    {
        base.TakeDamage(damageAmount);

        if (this.currHealth == 0) {
            if (team == 1) manager.player1Units--;
            else if (team == 2) manager.player1Units--;
        }
    }

    public enum State {
        Idle,
        Move,
        Attack
    }
}