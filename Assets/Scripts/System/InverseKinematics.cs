/* Author name: Donavan Sirois
 * Date: 2025/04/29
 * Goal: Program kinematics, which are used on the creatures. This code is based on this video:
 * https://youtu.be/df5YwVsekmE?si=F3WbuXtsmIxb3dDf
 * 2025/05/11:
 *      Author Name: Donavan Sirois
 *      Goal: Changed the tragetMouse to targetPlayer
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class InverseKinematics : MonoBehaviour
{
    public Vector2 _targetPosition;
    private Vector2 _linePosition;
    public int _segmentAmount = 10;
    public float _segmentLength = 1f;
    Segment[] _segments;
    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] EdgeCollider2D _creatureCollider;
    public bool _followPlayer;
    public bool _isFixed;
    public float _range = 30;
    private Transform[] _players;

    private void Awake()
    {
        InitializeSegments();
        _lineRenderer.useWorldSpace = false;
    }

    private void Start()
    {
     FindPlayers();
    }

    public void FindPlayers()
    {
        var playerObject = GameObject.FindGameObjectsWithTag("Player");
        _players = new Transform[playerObject.Length];
        var i = 0;
        foreach (var player in playerObject)
        {
            _players[i] = player.transform;
            i++;
        }
    }

    void InitializeSegments()
    {
        // _lineRenderer = GetComponent<LineRenderer>();
        // _creatureCollider = GetComponent<EdgeCollider2D>();
        _segments = new Segment[_segmentAmount];
        for (int i = 0; i < _segmentAmount; i++)
        {
            Segment segment = new Segment();
            segment._A = Vector3.zero + (Vector3.up * _segmentLength * (i));
            segment._B = segment._A + (Vector3.up * _segmentLength);
            segment._length = _segmentLength;
            _segments[i] = segment;
        }
    }

    private void Update()
    {
        if (Managers.TimeManager.isPaused) return;
        if(_players.Length==0 || !_players[0])FindPlayers();
        
        if (_segmentAmount != _segments.Length)
        {
            InitializeSegments(); // reinitialize if amount of segments changes (for example an creature who gets a leg ripped of)
        }

        if (_followPlayer && _players.Length>0)
        {
            _targetPosition = GetPlayerPosition();
        }

        _linePosition = _lineRenderer.GetPosition(1);

        Follow();
        DrawSegments(_segments);
        UpdateCollider();
    }

    private void UpdateCollider()
    {
        int pointCount = _lineRenderer.positionCount;
        if (pointCount < 2) return;

        List<Vector2> leftSide = new List<Vector2>();
        List<Vector2> rightSide = new List<Vector2>();
        float halfThickness = _segmentLength / 2f;

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 p = _lineRenderer.GetPosition(i);

            // Direction based on surrounding points
            Vector2 dir;
            if (i == 0)
                dir = (_lineRenderer.GetPosition(i + 1) - p).normalized;
            else if (i == pointCount - 1)
                dir = (p - _lineRenderer.GetPosition(i - 1)).normalized;
            else
                dir = (_lineRenderer.GetPosition(i + 1) - _lineRenderer.GetPosition(i - 1)).normalized;

            Vector2 normal = new Vector2(-dir.y, dir.x);

            leftSide.Add((Vector2)p + normal * halfThickness);
            rightSide.Add((Vector2)p - normal * halfThickness);
        }

        rightSide.Reverse();

        List<Vector2> colliderPoints = new List<Vector2>();
        colliderPoints.AddRange(leftSide);
        colliderPoints.AddRange(rightSide);

        // Close the loop by adding the first point again
        colliderPoints.Add(colliderPoints[0]);

        _creatureCollider.points = colliderPoints.ToArray();
    }

    void Follow()
    {
        if ((_linePosition - _targetPosition).magnitude < _range)
        {
            _segments[_segmentAmount - 1].Follow(_targetPosition);
        }

        for (int i = _segmentAmount - 2; i >= 0; i--)
        {
            _segments[i].Follow(_segments[i + 1]);
        }

        if (_isFixed)
        {
            _segments[0].AnchorStartAt(Vector3.zero);
            for (int i = 1; i < _segmentAmount; i++)
            {
                _segments[i].AnchorStartAt(_segments[i - 1]._B);
            }
        }
    }

    void DrawSegments(Segment[] segments)
    {
        _lineRenderer.positionCount = _segmentAmount + 1;
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < _segmentAmount; i++)
        {
            points.Add(segments[i]._A);
        }

        points.Add(segments[_segmentAmount - 1]._B);
        _lineRenderer.SetPositions(points.ToArray());
    }

    Vector2 GetPlayerPosition()
    {
        var closest=_players[0].position;
        foreach (var player in _players )
        {
            var pos = player.position;
            if ((new Vector2(pos.x, pos.y) - _linePosition).magnitude<( new Vector2(closest.x, closest.y) - _linePosition).magnitude)
            {
                closest=pos;
            }
        }
        
        return _lineRenderer.transform.InverseTransformPoint(closest);
    }
}