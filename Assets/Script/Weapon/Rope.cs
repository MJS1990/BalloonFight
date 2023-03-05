using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public EdgeCollider2D EdgeCollider;
    public int JointCount = 25;
    public int ConstraintLoop = 15;
    public float JointLength = 0.04f;
    public float RophWidth = 0.01f;

    public Vector2 Weight = new Vector2(0.0f, -4.5f);
    Vector2[] colliderPositions;


    [Space(10f)]
    public Transform StartTransform;

    private List<Joint> Joints = new List<Joint>();

    private void Reset()
    {
        TryGetComponent(out lineRenderer);
        TryGetComponent(out EdgeCollider);
    }
    
    void Start()
    {
        colliderPositions = new Vector2[Joints.Count];
    }

    private void Awake()
    {
        //colliderPositions = new Vector2[Joints.Count];

        Vector2 jointPos = StartTransform.position;
        for(int i = 0; i < JointCount; i++)
        {
            Joints.Add(new Joint(jointPos));
            jointPos.y -= JointLength;

            //colliderPositions[i] = jointPos;
            //colliderPositions[i].y -= JointLength;
        }
    }

    private void FixedUpdate()
    {
        UpdateJoints();

        for(int i = 0; i < ConstraintLoop; i++)
        {
            ApplyConstraint();
            AdjustCollision();
        }

        DrawRope();
    }

    private void DrawRope()
    {
        lineRenderer.startWidth = RophWidth;
        lineRenderer.endWidth = RophWidth;

        Vector3[] JointPositions = new Vector3[Joints.Count]; //Vector3
        
        for (int i = 0; i < Joints.Count; i++)
        {
            JointPositions[i] = Joints[i].currentPos;
            colliderPositions[i].y = Joints[i].currentPos.y;
            //colliderPositions[i] = JointPositions[i];
        }
        lineRenderer.positionCount = JointPositions.Length;
        lineRenderer.SetPositions(JointPositions);

        if(EdgeCollider)
        {
            EdgeCollider.edgeRadius = RophWidth;
            EdgeCollider.points = colliderPositions;
        }
    }

    private void UpdateJoints()
    {
        for(int i = 0; i < Joints.Count; i++)
        {
            Joints[i].velocity = Joints[i].currentPos - Joints[i].prevPos;
            Joints[i].prevPos = Joints[i].currentPos;
            Joints[i].currentPos += Weight * Time.fixedDeltaTime * Time.fixedDeltaTime;
            Joints[i].currentPos += Joints[i].velocity;
        }
    }

    private void ApplyConstraint()
    {
        Joints[0].currentPos = StartTransform.position;
        colliderPositions[0] = StartTransform.position;
        for(int i = 0; i < Joints.Count - 1; i++)
        {
            float distance = (Joints[i].currentPos - Joints[i + 1].currentPos).magnitude;
            float offset = JointLength - distance;
            Vector2 dir = (Joints[i + 1].currentPos - Joints[i].currentPos).normalized;

            Vector2 movement = dir * offset;

            if (i == 0)
            {
                Joints[i + 1].currentPos += movement;
            }
            else
            {
                Joints[i].currentPos -= movement * 0.5f;
                Joints[i + 1].currentPos += movement * 0.5f;
            }
        }

    }

    private void AdjustCollision()
    {
        for(int i = 0; i < Joints.Count; i++)
        {
            Vector2 dir = Joints[i].currentPos - Joints[i].prevPos;
            RaycastHit2D hit = Physics2D.CircleCast(Joints[i].currentPos, RophWidth * 0.5f, dir.normalized, 0.0f);

            if(hit)
            {
                Joints[i].currentPos = hit.point + hit.normal * RophWidth * 0.5f;
                Joints[i].prevPos = Joints[i].currentPos;
            }
        }
    }

    public class Joint
    {
        public Vector2 prevPos;
        public Vector2 currentPos;
        public Vector2 velocity;

        public Joint(Vector2 pos)
        {
            prevPos = pos;
            currentPos = pos;
            velocity = Vector2.zero;
        }
    }

}
