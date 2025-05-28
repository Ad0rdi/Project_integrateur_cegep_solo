/* Author name: Donavan Sirois
 * Date: 2025/04/29
 * Goal: Program the Segment class used in the InverseKinematics one. This code is based on this video:
 * https://youtu.be/df5YwVsekmE?si=F3WbuXtsmIxb3dDf
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment
{
    public Vector3 _A; // Represents the starting point of the line
    public Vector3 _B; // Represents the ending point of the line
    public float _length;
    public float _speed = 10f;
    Vector3 _diff;
    Vector3 _norm;

    public void Follow(Segment targetSegment)
    {
        Follow(targetSegment._A);
    }

    public void Follow(Vector3 targetPosition) // This follow function recalculates the point _A
    {
        _B = Vector3.MoveTowards(_B, targetPosition, _speed * Time.deltaTime); // The target position is generally either the player, or the _A of another segment
        _diff = _B - _A;    
        _norm = _diff.normalized;
        _A = _B - (_norm*_length);
    }

    public void AnchorStartAt(Vector3 targetPosition)
    {
        _diff = _B - _A;
        _A = targetPosition;
        _B = _A + _diff;
    }

    public void RotateTowards(Vector3 targetPosition)
    {
        _diff = targetPosition - _A;
        _B = _A + (_diff.normalized * _length);
    }

    public void AnchorEndAt(Vector3 targetPosition)
    {
        _diff = _B - _A;
        _B = targetPosition;
        _A  = _B - _diff;
    }
}
