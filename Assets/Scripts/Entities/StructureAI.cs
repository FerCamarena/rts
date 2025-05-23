
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class StructureAI : UnitAI {
    // ? DEBUG======================================================================================================================================

    // ? PARAMETERS=================================================================================================================================
    // * VISUALS
    //[SerializeField] private GameObject cooldownBar;

    // * REFERENCES
    public GameObject unitPrefab;

    // * COMPONENTS

    // * ATTRIBUTES
    [SerializeField, Min(0)] protected float baseCD = 10.0f;
    [SerializeField, Min(0)] protected float currCD = 0.0f;
    public Vector3 targetLocation = Vector3.zero;
    public bool fortress = false;
    // * INTERNAL

    // ? BASE METHODS===============================================================================================================================
    protected override void Awake() {
        base.Awake();

        this.currCD = this.baseCD;
        this.targetLocation = this.transform.position + (this.transform.forward * 2);
    }

    protected override void FixedUpdate() {
        this.UpdateCooldowns();
    }

    // ? CUSTOM METHODS=============================================================================================================================
    protected virtual void UpdateCooldowns() {
        if (this.currCD > 0.0f) this.currCD -= Time.fixedDeltaTime;
        else if (this.currCD < 0.0f) this.currCD = 0.0f;
    }

    public void GenerateUnit(int tier) {
        if (this.currCD != 0.0f || manager.player1Units >= manager.maxUnitPerPlayer || manager.player1Resources < 1) return;
        manager.player1Units++;
        manager.player1Resources--;
        this.currCD = this.baseCD;

        CrowdAI unit = Instantiate(unitPrefab, transform.position, Quaternion.identity).GetComponent<CrowdAI>();

        unit.team = this.team;
        unit.faction = this.faction;
        unit.tier = tier;
        unit.destination = this.targetLocation;
        unit.state = CrowdAI.State.Move;
    }
    // ? EVENT METHODS==============================================================================================================================
    public override void TakeDamage(float damageAmount) {
        this.currHealth -= damageAmount;
        if (this.currHealth > this.baseHealth) this.currHealth = this.baseHealth;
        else if (this.currHealth <= 0.0f) {
            if (this.currHealth < 0.0f) this.currHealth = 0.0f;

            if (fortress) {
                if (team == 2) InGameEvent.OnGameWon();
                else if (team == 1) InGameEvent.OnGameOver();
            }
            
        }

        this.UpdateHealthBar();

        if (currHealth == 0) Destroy(this.gameObject);
    }
}