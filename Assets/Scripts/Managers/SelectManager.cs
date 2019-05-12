using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectManager : MonoBehaviour
{
    private static SelectManager selectManager;
    private Vector3 boxStartPosition;
    private Vector3 boxEndPosition;
    public Texture selectTexture;
    private SelectableCharacter[] selectableChars;
    private Camera mainCamera;
    private CameraScript mainCameraScript;
    private bool abilityReady;
    private int numSelected;
    private bool overButton;

    public static SelectManager Instance {
        get {
            if (selectManager == null) {
                selectManager = FindObjectOfType(typeof(SelectManager)) as SelectManager;
                selectManager.SetUp();
            }

            return selectManager;
        }
    }

    private void SetUp() {
        boxStartPosition = Vector3.zero;
        boxEndPosition = Vector3.zero;
        selectableChars = FindObjectsOfType<SelectableCharacter>();
        mainCamera = Camera.main;
        mainCameraScript = mainCamera.GetComponent<CameraScript>();
        numSelected = 0;
        overButton = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!abilityReady && !overButton) {
            if (Input.GetMouseButton(0))
            {
                // Called on the first update where the user has pressed the mouse button.
                if (Input.GetMouseButtonDown(0))
                    boxStartPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
                else  // Else we must be in "drag" mode.
                    boxEndPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
            }
            else
            {
                // Handle the case where the player had been drawing a box but has now released.
                if (boxEndPosition != Vector3.zero && boxStartPosition != Vector3.zero)
                    HandleUnitSelection(boxStartPosition, boxEndPosition);
                // Reset box positions.
                boxEndPosition = boxStartPosition = Vector3.zero;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            HandleUnitSelection(Vector3.zero, Vector3.zero);
            selectableChars[0].SetSelected(true);
            mainCameraScript.SetPlayer(selectableChars[0].gameObject);
            selectableChars[0].gameObject.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
            selectableChars[1].SetSelected(false);
            selectableChars[1].gameObject.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Diffuse");
            numSelected = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            HandleUnitSelection(Vector3.zero, Vector3.zero);
            selectableChars[1].SetSelected(true);
            numSelected = 1;
            mainCameraScript.SetPlayer(selectableChars[1].gameObject);
            selectableChars[1].gameObject.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
            selectableChars[0].SetSelected(false);
            selectableChars[0].gameObject.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Diffuse");
        }
    }

    public void HandleUnitSelection(Vector3 boxStartPosition, Vector3 boxEndPosition)
    {

        numSelected = 0;
        Rect rect;
        float width = Mathf.Abs(boxStartPosition.x - boxEndPosition.x);
        float height = Mathf.Abs(boxStartPosition.y - boxEndPosition.y);
        if (boxStartPosition.x <= boxEndPosition.x)
        {
            if (boxStartPosition.y <= boxEndPosition.y)
            {
                rect = new Rect(boxStartPosition.x, boxStartPosition.y, width, height);
            }
            else
            {
                rect = new Rect(boxStartPosition.x, boxEndPosition.y, width, height);
            }
        }
        else
        {
            if (boxStartPosition.y <= boxEndPosition.y)
            {
                rect = new Rect(boxEndPosition.x, boxStartPosition.y, width, height);
            }
            else
            {
                rect = new Rect(boxEndPosition.x, boxEndPosition.y, width, height);
            }
        }

        foreach (SelectableCharacter selChar in selectableChars)
        {
            Vector2 position = mainCamera.WorldToScreenPoint(selChar.gameObject.transform.position);
            Rect charRect = new Rect(position.x - 50, position.y, 100, 100);
            if (charRect.Overlaps(rect)) {
                selChar.SetSelected(true);
                numSelected++;
                mainCameraScript.SetPlayer(selChar.gameObject);
                selChar.gameObject.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
            } else {
                selChar.SetSelected(false);
                selChar.gameObject.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Diffuse");
            }
        }

        if (numSelected == 0)
        {
            foreach (SelectableCharacter selChar in selectableChars)
            {
                selChar.SetSelected(false);
                selChar.gameObject.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Diffuse");
                if (selChar.GetTargetIndicator() != null)
                {
                    GameObject.Destroy(selChar.GetTargetIndicator());
                    selChar.SetTargetIndicator(null);
                    selChar.GetLineRenderer().positionCount = 0;
                }
            }
        }
    }

    public void SetAbilityReady(bool val)
    {
        abilityReady = val;
    }

    public bool GetAbilityReady(bool val)
    {
        return abilityReady;
    }

    public void SetOverButton(bool val) {
        overButton = val;
    }

    public int GetNumSelected()
    {
        return numSelected;
    }

    public void SetNumSelected(int val)
    {
        numSelected = val;
    }

    public List<SelectableCharacter> GetCurrentlySelected() {
        List<SelectableCharacter> chars = new List<SelectableCharacter>();
        foreach (SelectableCharacter selChar in selectableChars) {
            if (selChar.GetSelected()) {
                chars.Add(selChar);
            }
        }
        return chars;
    }

    public void OnGUI()
    {

        // If we are in the middle of a selection draw the texture.
        if (boxStartPosition != Vector3.zero && boxEndPosition != Vector3.zero)
        {

            GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.5f); //0.5 is half opacity

            // Create a rectangle object out of the start and end position while transforming it
            // to the screen's cordinates.
            var rect = new Rect(boxStartPosition.x, Screen.height - boxStartPosition.y,
                                boxEndPosition.x - boxStartPosition.x,
                                -1 * (boxEndPosition.y - boxStartPosition.y));
            // Draw the texture.
            GUI.DrawTexture(rect, selectTexture);
        }
        else
        {
            GUI.color = Color.white;
        }
    }
}
