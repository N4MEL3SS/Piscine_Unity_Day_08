using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float distanceMin;
    public float distanceMax;
    public float sensitivityX;
    public float sensitivityY;
    
    
    private float _currentDistance;

    private float _yMinLimit = -20f;
    private float _yMaxLimit = 80f;
    private float _x;
    private float _y;

    private RaycastHit _hit;
    
    private void Start()
    {
        var angles = transform.eulerAngles;
        _x = angles.y;
        _y = angles.x;
        _currentDistance = 10f;
    }
    
    private void LateUpdate()
    {
        if (!target) return;
        
        if(Input.GetKey("mouse 1"))
        {
            Cursor.visible = false;
            _x += Input.GetAxis("Mouse X") * sensitivityX * _currentDistance * 0.02f;
            _y -= Input.GetAxis("Mouse Y") * sensitivityY * 0.02f;
        }
        else
            Cursor.visible = true;
        
        _y = ClampAngle(_y, _yMinLimit, _yMaxLimit);
        var rotation = Quaternion.Euler(_y, _x, 0);
        _currentDistance = Mathf.Clamp(_currentDistance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);
        RaycastHit hit;
        
        if (Physics.Linecast(target.position, transform.position, out hit))
        {
            if (!hit.transform.CompareTag("Enemy"))
                _currentDistance -= hit.distance;
        }
        
        var position = rotation * new Vector3(0.0f, 0.0f, - _currentDistance) + target.position;
        transform.rotation = rotation;
        transform.position = position;
    }
    
    private static float ClampAngle(float angle, float min,float max) 
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
