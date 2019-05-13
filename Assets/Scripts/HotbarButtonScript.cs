using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private GameObject description;

    void Start() {
        description = transform.GetChild(2).gameObject;
    }

    public void OnPointerEnter(PointerEventData data) {
        SelectManager.Instance.SetOverButton(true);
        description.SetActive(true);
    }

    public void OnPointerExit(PointerEventData data) {
        SelectManager.Instance.SetOverButton(false);
        description.SetActive(false);
    }
}
