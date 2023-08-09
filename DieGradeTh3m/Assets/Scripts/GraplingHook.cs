using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraplingHook : MonoBehaviour
{
    public float maxGrappleDistance;
    public Transform gunTip, cameraPos, playerPos;
    [Range(0, 1)]
    public float maxLineDistanceIndicator; //how much is max distance between player and object when on line?
    [Range(0, 1)]
    public float minLineDistanceIndicator;// how much is min distance between player and object when on line?
    public float springForce;
    public float springDamper;
    public float springMassScale;
    
    public LayerMask whatIsGrappable;



    private LineRenderer lr;
    private Vector3 grapplePoint;
    private SpringJoint joint;


    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }


    private void Update()
    { 
        if (Input.GetMouseButtonDown(1)) StartGrapple();
        else if (Input.GetMouseButtonUp(1)) StopGrapple();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraPos.position, cameraPos.forward, out hit, maxGrappleDistance, whatIsGrappable))
        {
            grapplePoint = hit.point;
            joint = playerPos.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float disctanceFromPoint = Vector3.Distance(playerPos.position, grapplePoint);

            joint.maxDistance = disctanceFromPoint * maxLineDistanceIndicator;
            joint.minDistance = disctanceFromPoint * minLineDistanceIndicator;

            //To adjust
            joint.spring = springForce;
            joint.damper = springDamper;
            joint.massScale = springMassScale;

            lr.positionCount = 2;
        }

    }

    private void DrawRope()
    {
        if (!joint) return;
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
    }
}
