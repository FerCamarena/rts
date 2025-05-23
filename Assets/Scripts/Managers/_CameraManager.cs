using System;
using UnityEngine;

public class _CameraManager : MonoBehaviour {
    // ? DEBUG======================================================================================================================================
    [SerializeField] protected bool DEBUG = false;

    // ? PARAMETERS=================================================================================================================================
    // * VISUALS

    // * REFERENCES
    [SerializeField] private GameObject cam;

    // * COMPONENTS
    
    // * ATTRIBUTES
    [Header("General padding settings")]
    [SerializeField] private int paddingArea = 198;
    [Header("Horizontal padding settings")]
    [SerializeField, Range(0.0f, 10.0f)] private float maxHPanScale = 1.0f;
    [SerializeField, Range(0.0f, 10.0f)] private float panHSpeedScale = 1.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float panHAccelScale = 0.1f;
    [Header("Vertical padding settings")]
    [SerializeField, Range(0.0f, 10.0f)] private float maxVPanScale = 1.0f;
    [SerializeField, Range(0.0f, 10.0f)] private float panVSpeedScale = 1.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float panVAccelScale = 0.1f;
    [Header("Zooming settings")]
    [SerializeField, Range(0.0f, 10.0f)] private float maxZoomScale = 1.0f;
    [SerializeField, Range(0.0f, 10.0f)] private float zoomSpeedScale = 1.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float zoomAccelScale = 0.1f;

    // * INTERNAL
    [SerializeField] private Vector2 currPanAccel = new(0.0f, 0.0f);
    [SerializeField] private float currZoomAccel = 0.0f;

    // ? BASE METHODS===============================================================================================================================
    private void Start() {
        this.cam ??= Camera.main.gameObject;

        this.paddingArea = (int)(Screen.width * 0.05f);
    }

    private void Update() {
        this.CameraPanning();
        this.CameraZooming();
    }

    // ? CUSTOM METHODS=============================================================================================================================
    private void CameraPanning() {
        this.currPanAccel.x +=
            (Input.mousePosition.x < paddingArea || Input.GetKey(KeyCode.LeftArrow)) ?
                -this.panHSpeedScale : (Input.mousePosition.x > (Screen.width - this.paddingArea) || Input.GetKey(KeyCode.RightArrow)) ?
                    this.panHSpeedScale : 0.0f;
        
        this.currPanAccel.y +=
            (Input.mousePosition.y < paddingArea || Input.GetKey(KeyCode.DownArrow)) ?
                -this.panVSpeedScale : (Input.mousePosition.y > (Screen.height - this.paddingArea) || Input.GetKey(KeyCode.UpArrow)) ?
                    this.panVSpeedScale : 0.0f;
        
        this.currPanAccel.x = Mathf.Clamp(this.currPanAccel.x, -this.maxHPanScale, this.maxHPanScale);
        this.currPanAccel.y = Mathf.Clamp(this.currPanAccel.y, -this.maxVPanScale, this.maxVPanScale);
        Vector3 currPanVel = new Vector3(this.currPanAccel.x, 0.0f,  this.currPanAccel.y) * Time.deltaTime * 5;
        this.cam.transform.localPosition += currPanVel;
        
        if (this.cam.transform.position.x < -10) this.cam.transform.localPosition = new Vector3(this.cam.transform.localPosition.x+0.05f, this.cam.transform.localPosition.y, this.cam.transform.localPosition.z);
        if (this.cam.transform.position.x > 10) this.cam.transform.localPosition = new Vector3(this.cam.transform.localPosition.x-0.05f, this.cam.transform.localPosition.y, this.cam.transform.localPosition.z);

        if (this.cam.transform.position.z < -10) this.cam.transform.localPosition = new Vector3(this.cam.transform.localPosition.x, this.cam.transform.localPosition.y, this.cam.transform.localPosition.z+0.05f);
        if (this.cam.transform.position.z > 10) this.cam.transform.localPosition = new Vector3(this.cam.transform.localPosition.x, this.cam.transform.localPosition.y, this.cam.transform.localPosition.z-0.05f);

        this.currPanAccel.x *=
            (Mathf.Abs(this.currPanAccel.x) < 1.0f) ?
                (Mathf.Abs(this.currPanAccel.x) < 0.1f) ?
                    0.0f : 1 - (this.panHAccelScale / 10) : 1 - (this.panHAccelScale / 10);
        this.currPanAccel.y *=
            (Mathf.Abs(this.currPanAccel.y) < 1.0f) ?
                (Mathf.Abs(this.currPanAccel.y) < 0.1f) ?
                    0.0f : 1 - (this.panVAccelScale / 10) : 1 - (this.panVAccelScale / 10);
    }
    
    private void CameraZooming() {
        this.currZoomAccel += ((Input.mouseScrollDelta.y < 0.0f) ? this.zoomSpeedScale : (Input.mouseScrollDelta.y > 0.0f) ? -this.zoomSpeedScale : 0.0f) * 100;
        
        this.currZoomAccel = Mathf.Clamp(this.currZoomAccel, -this.maxZoomScale, this.maxZoomScale);
        float currZoomVel = this.currZoomAccel * Time.deltaTime * 5;

        this.cam.GetComponent<Camera>().orthographicSize += currZoomVel != 0.0f ? currZoomVel : 0.0f;
        this.cam.GetComponent<Camera>().orthographicSize = Mathf.Clamp(this.cam.GetComponent<Camera>().orthographicSize, 2, 8);

        this.currZoomAccel *= Mathf.Abs(this.currZoomAccel) < 0.1f ?
            (Mathf.Abs(this.currZoomAccel) < 0.01f) ?
                0.0f : 1 - this.zoomAccelScale : 1 - (this.zoomAccelScale / 10);
    }

    // ? EVENT METHODS==============================================================================================================================

}