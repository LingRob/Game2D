using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class CharacterController2D : MonoBehaviour
{
    public float raycastDistance = 0.2f;
    public LayerMask layerMask;
    public float slopeAngleLimit = 45f;
    public float downForceAdjustment = 1.2f;

    //flags
    public bool below;
    public bool left;
    public bool right;
    public bool above;

    public GroundType groundType;
    public bool hitGroundThisFrame;
    public bool hitWallThisFrame;

    private Vector2 _moveAmount;
    private Vector2 _currentPostion;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;

    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3];

    private bool _disableGroundCheck;

    //TODO: Change to private
    private Vector2 _slopeNormal;
    private float _slopeAngle;

    private bool _inAirLastFrame;
    private bool _noSideCollisionLastFrame;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _inAirLastFrame = !below;

        _noSideCollisionLastFrame = (!right && !left);

        _lastPosition = _rigidbody.position;

        if (_slopeAngle != 0 && below == true)
        {
            if ((_moveAmount.x > 0f && _slopeAngle > 0f) || (_moveAmount.x < 0f && _slopeAngle < 0f))
            {
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(_slopeAngle * Mathf.Deg2Rad) * _moveAmount.x);
                _moveAmount.y *= downForceAdjustment;
            }

        }

        _currentPostion = _lastPosition + _moveAmount;

        _rigidbody.MovePosition(_currentPostion);

        _moveAmount = Vector2.zero;

        if (!_disableGroundCheck)
        {
            CheckGrounded();
        }

        CheckOtherCollisions();

        if (below && _inAirLastFrame)
        {
            hitGroundThisFrame = true;
        }
        else
        {
            hitGroundThisFrame = false;
        }

        if ((right || left) && _noSideCollisionLastFrame)
        {
            hitWallThisFrame = true;
        }
        else
        {
            hitWallThisFrame = false;
        }
    }

    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    private void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical,
           0f, Vector2.down, raycastDistance, layerMask);

        if (hit.collider)
        {
            groundType = DetermineGroundType(hit.collider);

            _slopeNormal = hit.normal;
            _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);

            if (_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
            {
                below = false;
            }
            else
            {
                below = true;
            }


        }
        else
        {
            groundType = GroundType.None;
            below = false;
        }

    }

    private void CheckOtherCollisions()
    {
        //check left
        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, Vector2.left,
            raycastDistance * 2, layerMask);

        if (leftHit.collider)
        {
            left = true;
        }
        else
        {
            left = false;
        }


        //check right
        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, Vector2.right,
            raycastDistance * 2, layerMask);

        if (rightHit.collider)
        {
            right = true;
        }
        else
        {
            right = false;
        }

        //check above
        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical,
           0f, Vector2.up, raycastDistance, layerMask);

        if (aboveHit.collider)
        {
            above = true;
        }
        else
        {
            above = false;
        }
    }

    /*
    private void CheckGrounded()
    {
        Vector2 raycastOrigin = _rigidbody.position - new Vector2(0, _capsuleCollider.size.y * 0.5f);

        _raycastPosition[0] = raycastOrigin + (Vector2.left * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f);
        _raycastPosition[1] = raycastOrigin;
        _raycastPosition[2] = raycastOrigin + (Vector2.right * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f);

        DrawDebugRays(Vector2.down, Color.green);

        int numberOfGroundHits = 0;

        for (int i = 0; i < _raycastPosition.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(_raycastPosition[i], Vector2.down, raycastDistance, layerMask);

            if (hit.collider)
            {
                _raycastHits[i] = hit;
                numberOfGroundHits++;
            }
        }

        if (numberOfGroundHits > 0)
        {
            if (_raycastHits[1].collider)
            {
                groundType = DetermineGroundType(_raycastHits[1].collider);
                _slopeNormal = _raycastHits[1].normal;
                _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);
            }
            else
            {
                for (int i = 0; i  < _raycastHits.Length; i++)
                {
                    if (_raycastHits[i].collider)
                    {
                        groundType = DetermineGroundType(_raycastHits[i].collider);
                        _slopeNormal = _raycastHits[i].normal;
                        _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);
                    }
                }
            }

            if (_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
            {
                below = false;
            }
            else
            {
                below = true;
            }

        }
        else
        {
            groundType = GroundType.None;
            below = false;
        }

        System.Array.Clear(_raycastHits, 0, _raycastHits.Length);

    }*/

    private void DrawDebugRays(Vector2 direction, Color color)
    {
        for (int i = 0; i < _raycastPosition.Length; i++)
        {
            Debug.DrawRay(_raycastPosition[i], direction * raycastDistance, color);
        }
    }

    public void DisableGroundCheck()
    {
        below = false;
        _disableGroundCheck = true;
        StartCoroutine("EnableGroundCheck");
    }

    IEnumerator EnableGroundCheck()
    {
        yield return new WaitForSeconds(0.1f);
        _disableGroundCheck = false;
    }

    private GroundType DetermineGroundType(Collider2D collider)
    {
        if (collider.GetComponent<GroundEffector>())
        {
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();
            return groundEffector.groundType;
        }
        else
        {
            return GroundType.LevelGeometry;
        }
    }
}
