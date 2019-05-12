using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public void OnPointerEnter(PointerEventData data) {
        SelectManager.Instance.SetOverButton(true);
    }

    public void OnPointerExit(PointerEventData data) {
        SelectManager.Instance.SetOverButton(false);
    }
}
