using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider), typeof(Animator))]
public class UnitAI : MonoBehaviour {
    //TODO: Destroy itself on no health
    // ? DEBUG======================================================================================================================================
    [SerializeField] protected bool DEBUG = false;

    // ? PARAMETERS=================================================================================================================================
    // * VISUALS
    [SerializeField] private Transform healthBar;

    // * REFERENCES
    [SerializeField] private Transform skin;
    public _GameManager manager;
    [SerializeField] protected List<Color> colors;

    // * COMPONENTS

    // * ATTRIBUTES
    [SerializeField, Min(0)] protected float baseHealth = 10.0f;
    [SerializeField, Min(0)] protected float currHealth = 0.0f;

    // * INTERNAL
    [SerializeField, Min(0)] public int team = 0;
    [SerializeField, Min(0)] public int faction = 0;

    // ? BASE METHODS===============================================================================================================================
    protected virtual void Awake() {

        this.currHealth = this.baseHealth;
    }
    
    protected virtual void Start() {
        if (this.team == 0) this.team = 1;
        if (this.faction == 0) this.faction = Random.Range(1, 3);

        if (this.team == 1) GetComponentInChildren<SpriteRenderer>().color = colors[1];
        else GetComponentInChildren<SpriteRenderer>().color = colors[0];
        this.gameObject.tag = faction == 1 ? "Player1" : faction == 2 ? "Player2" : "Untagged";
    }

    protected virtual void Update() {
        this.skin.eulerAngles = new(0.0f, 45.0f, 0.0f);
        this.healthBar.parent.eulerAngles = new(0.0f, 45.0f, 0.0f);
        this.healthBar.parent.localPosition = new(0.0f, 1.1f, 0.0f);
    }

    protected virtual void FixedUpdate() {
    }

    // ? CUSTOM METHODS=============================================================================================================================
    protected virtual void UpdateHealthBar() {
        this.healthBar.localPosition = new(-0.5f + (this.currHealth / this.baseHealth) / 2, this.healthBar.localPosition.y, this.healthBar.localPosition.z);
        this.healthBar.localScale = new(this.currHealth / this.baseHealth, this.healthBar.localScale.y, this.healthBar.localScale.z);
    }

    public virtual void TakeDamage(float damageAmount) {
        this.currHealth -= damageAmount;
        if (this.currHealth > this.baseHealth) this.currHealth = this.baseHealth;
        else if (this.currHealth <= 0.0f) {
            if (this.currHealth < 0.0f) this.currHealth = 0.0f;
            Destroy(this.gameObject);
        }

        this.UpdateHealthBar();
    }
    // ? EVENT METHODS==============================================================================================================================

}