using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    public StructureAI self;
    
    private void Awake() {
        self = GetComponent<StructureAI>();
        InvokeRepeating("CreateUnit", 10, 10);
    }
    
    public void CreateUnit() {
        self.GenerateUnit(1);
    }
}
