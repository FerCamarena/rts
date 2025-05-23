using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class _GameManager : MonoBehaviour {
    // ? DEBUG======================================================================================================================================
    [SerializeField] protected bool DEBUG = false;

    // ? PARAMETERS=================================================================================================================================
    // * VISUALS

    // * REFERENCES
    [SerializeField] private GameObject ui;

    // * COMPONENTS
    
    // * ATTRIBUTES
    [SerializeField] private LayerMask clickMask;
    [SerializeField] private Vector2 startClickPos;
    [SerializeField] private Vector2 endClickPos;
    [SerializeField] public int maxUnitPerPlayer = 20;
    [SerializeField] public int player1Units = 4;
    [SerializeField] public int player2Units = 4;
    public int player1Resources = 0;

    public TextMeshProUGUI resources;
    public TextMeshProUGUI population;

    // * INTERNAL
    [SerializeField] private Transform level;
    [SerializeField] private List<UnitAI> selectedUnits;

    // ? BASE METHODS===============================================================================================================================
    private void Awake() {
        this.level ??= GameObject.Find("level").transform;
        
        InGameEvent.OnGameOver += EndGame;
        InGameEvent.OnGameWon += WinGame;

        InvokeRepeating("AddResource", 1, 1);
    }

    private void Update() {
        population.text = player1Units.ToString();
        resources.text = player1Resources.ToString();

        //Storing ray
        Ray pointerRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        Physics.queriesHitTriggers = false;
        this.UnitSelection(pointerRay);

        if (Input.GetMouseButtonDown(1)) {
            this.UnitMovement(pointerRay);
        }
        Physics.queriesHitTriggers = true;

        if (selectedUnits.Count > 0) LoadButtons();
    }

    // ? CUSTOM METHODS=============================================================================================================================
    private void UnitSelection(Ray ray) {
        if (EventSystem.current.IsPointerOverGameObject()) {
            // Pointer is over UI — don't raycast
            return;
        }
        if (Physics.Raycast(ray, out RaycastHit selHit, 32.0f, this.clickMask)) {
            if (Input.GetMouseButton(0)) {
                this.endClickPos = Input.mousePosition;

                 if (Input.GetMouseButtonDown(0)) {
                    this.startClickPos = Input.mousePosition;

                    if (!Input.GetKey(KeyCode.LeftControl)) {
                        this.selectedUnits.Clear();
                    }

                    if (selHit.collider.CompareTag("Player1") && selHit.collider.TryGetComponent<UnitAI>(out UnitAI unit)) {
                        this.SelectUnit(unit);
                    }
                 }
            } else if (Input.GetMouseButtonUp(0)) {
                float pointerDrag = Vector2.Distance(startClickPos, endClickPos);

                if (pointerDrag >= 5.0f) {
                    BoxSelection();
                }
            }
        }
    }
    
    private void UnitMovement(Ray ray) {
        if (Physics.Raycast(ray, out RaycastHit actHit, 32.0f, this.clickMask)) {
            if (this.selectedUnits.Count > 0 && actHit.collider.CompareTag("Environment")) {
                foreach (UnitAI unit in this.selectedUnits) {
                    if (!unit) continue;
                    if (unit.TryGetComponent<CrowdAI>(out CrowdAI crowd)) {
                        crowd.destination = actHit.point;
                        crowd.state = CrowdAI.State.Move;
                    } else if (unit && unit.TryGetComponent<StructureAI>(out StructureAI structure)) {
                        structure.targetLocation = actHit.point;
                    }
                }
            } else if (this.selectedUnits.Count > 0 && actHit.collider.tag != this.selectedUnits[0].tag) {
                foreach (UnitAI unit in this.selectedUnits) {
                    if (!unit) continue;
                    if (unit.TryGetComponent<CrowdAI>(out CrowdAI crowd)) {
                        crowd.SetTarget(actHit.collider.transform);
                    }
                }
            }
        }
    }

    private void SelectUnit(UnitAI unit) {
        if (!this.selectedUnits.Contains(unit)) {
            this.selectedUnits.Add(unit);
        }
    }

    private void BoxSelection() {
        GameObject[] unitList = GameObject.FindGameObjectsWithTag("Player1");

        Vector2 pos = new(
            this.startClickPos.x < this.endClickPos.x ? startClickPos.x : endClickPos.x,
            this.startClickPos.y < this.endClickPos.y ? startClickPos.y : endClickPos.y
        );
        Vector2 size = new(
            Mathf.Abs(this.startClickPos.x - this.endClickPos.x),
            Mathf.Abs(this.startClickPos.y - this.endClickPos.y)
        );

        Rect area = new(pos, size);

        foreach(GameObject unitObject in unitList) {
            Vector2 unitPos = Camera.main.WorldToScreenPoint(unitObject.transform.position);

            if (area.Contains(unitPos)) {
                if (unitObject.TryGetComponent<UnitAI>(out UnitAI unit)) {
                    this.SelectUnit(unit);
                }
            }
        }
    }

    private System.Type CheckSelection() {

        bool same = true;
        var type = selectedUnits[0].GetType();

        foreach (UnitAI unit in selectedUnits) {
            if (unit.GetType() != type) {
                same = false;
                break;
            }
        }

        foreach (Transform child in ui.transform) {
            child.gameObject.SetActive(false);
        }

        if (same) return type;
        return null;
    }

    private void LoadButtons() {
        System.Type type = CheckSelection();

        if (type == typeof(StructureAI)) {
            ui.transform.Find("Structure").gameObject.SetActive(true);
        } else if (type == typeof(CrowdAI)) {
            ui.transform.Find("Crowd").gameObject.SetActive(true);
        }
    }

    private void AddResource() {
        player1Resources ++;
    }
    // ? EVENT METHODS==============================================================================================================================
    public void CreateUnit() {
        if (CheckSelection() != typeof(StructureAI) || this.player1Units >= maxUnitPerPlayer) return;

        foreach (UnitAI unit in selectedUnits) {
            if (unit.TryGetComponent<StructureAI>(out StructureAI structure)) {
                structure.GenerateUnit(1);
            }
        }
    }

    public void DivideUnit() {
        if (CheckSelection() != typeof(CrowdAI)) return;

        foreach (UnitAI unit in selectedUnits) {
            if (unit.TryGetComponent<CrowdAI>(out CrowdAI crowd)) {
                crowd.DivideUnit();
            }
        }
    }

    private void EndGame() {
        SceneManager.LoadScene("GameOver");
    }
    
    private void WinGame() {
        SceneManager.LoadScene("GameWon");
    }
}