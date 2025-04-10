using UnityEngine;

namespace World
{
    /// <summary>
    /// Расчет точек спирали и управление высотой
    /// </summary>
    public class SpiralCalculator
    {
        private readonly float _radius;
        private readonly float _angleStep;
        private readonly float _heightPerRotation;
        
        private float _currentAngle = 0f;
        private float _currentHeight = 0f;

        public float CurrentHeight => _currentHeight;

        public SpiralCalculator(float radius, float angleStep, float heightPerRotation)
        {
            _radius = radius;
            _angleStep = angleStep;
            _heightPerRotation = heightPerRotation;
        }

        public void Reset()
        {
            _currentAngle = 0f;
            _currentHeight = 0f;
        }

        public void IncrementAngle(float amount)
        {
            _currentAngle += amount;
        }

        public void UpdateCurrentHeight(float height)
        {
            _currentHeight = Mathf.Max(_currentHeight, height);
        }

        public Vector3 GetNextPosition(Vector3 lastPosition, float maxJumpDistance, float maxJumpHeight)
        {
            float t = _currentAngle * Mathf.Deg2Rad;
            
            Vector3 spiralPoint = new Vector3(
                _radius * Mathf.Cos(t),
                (t * _heightPerRotation) / (2 * Mathf.PI),
                _radius * Mathf.Sin(t)
            );
            
            if (lastPosition == Vector3.zero)
            {
                _currentAngle += _angleStep;
                return spiralPoint;
            }
            
            Vector3 lastPositionXZ = new Vector3(lastPosition.x, 0, lastPosition.z);
            Vector3 spiralPointXZ = new Vector3(spiralPoint.x, 0, spiralPoint.z);
            
            float horizontalDistance = Vector3.Distance(lastPositionXZ, spiralPointXZ);
            float heightDifference = spiralPoint.y - lastPosition.y;
            
            if (horizontalDistance > maxJumpDistance * 0.8f || heightDifference > maxJumpHeight * 0.8f)
            {
                _currentAngle -= _angleStep * 0.3f;
                t = _currentAngle * Mathf.Deg2Rad;
                
                spiralPoint = new Vector3(
                    _radius * Mathf.Cos(t),
                    (t * _heightPerRotation) / (2 * Mathf.PI),
                    _radius * Mathf.Sin(t)
                );
                
                lastPositionXZ = new Vector3(lastPosition.x, 0, lastPosition.z);
                spiralPointXZ = new Vector3(spiralPoint.x, 0, spiralPoint.z);
                horizontalDistance = Vector3.Distance(lastPositionXZ, spiralPointXZ);
                heightDifference = spiralPoint.y - lastPosition.y;
                
                if (heightDifference > maxJumpHeight * 0.7f)
                {
                    spiralPoint.y = lastPosition.y + maxJumpHeight * 0.7f;
                }
            }
            
            _currentAngle += _angleStep;
            
            return spiralPoint;
        }
    }
}