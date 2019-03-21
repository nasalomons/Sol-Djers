using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathfindingScript : MonoBehaviour {

    private NavMeshAgent agent;
    private GameObject targetIndicatorPrefab;
    private GameObject targetIndicator;

    public LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        targetIndicatorPrefab = Resources.Load("TargetPoint") as GameObject;
        targetIndicator = null;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit clickPosition, 100)) {
                agent.destination = clickPosition.point;
                Vector3 placement = new Vector3(clickPosition.point.x, 0.5f, clickPosition.point.z);
                if (targetIndicator != null) {
                    GameObject.Destroy(targetIndicator);
                    lineRenderer.positionCount = 0;
                }
                targetIndicator = Instantiate(targetIndicatorPrefab, placement, Quaternion.identity);
                lineRenderer.positionCount = agent.path.corners.Length;
                lineRenderer.SetPositions(agent.path.corners);
            }
        }

        if (targetIndicator != null) {
            if (agent.transform.position.x == agent.destination.x && agent.transform.position.z == agent.destination.z) {
                GameObject.Destroy(targetIndicator);
                targetIndicator = null;
                lineRenderer.positionCount = 0;
            } else {
                lineRenderer.positionCount = agent.path.corners.Length;
                lineRenderer.SetPositions(agent.path.corners);
            }
        }
    }
}
