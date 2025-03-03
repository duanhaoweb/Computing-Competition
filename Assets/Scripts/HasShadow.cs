using System;
using UnityEngine;

public class HasShadow : MonoBehaviour
{
    private enum Direction
    {
        XAxis,
        YAxis,
        ZAxis
    }

    [SerializeField] private Direction direction;
    private GameObject _shadow;


    private Vector3 GetForward()
    {
        if (this.direction == Direction.XAxis)
            return this.transform.right;
        else if (this.direction == Direction.YAxis)
            return this.transform.up;
        else
            return this.transform.forward;
    }

    private Vector3 GetShadowScale()
    {
        var forward = this.GetForward();
        var x = Mathf.Abs(Vector3.Dot(forward, Vector3.right));
        var z = Mathf.Abs(Vector3.Dot(forward, Vector3.forward));
        var xScale = Mathf.Lerp(0.105f, 0.505f, x);
        var zScale = Mathf.Lerp(0.105f, 0.505f, z);
        return new Vector3(xScale, 1, zScale);
    }

    private void OnEnable()
    {
        this._shadow = ShadowController.Instance.GetShadow();
        this._shadow.SetActive(true);
        this._shadow.transform.localScale = this.GetShadowScale();
        this._shadow.transform.position = this.transform.position;
    }

    private void Update()
    {
        this._shadow.transform.position = new Vector3(this.transform.position.x, -0.1f, this.transform.position.z);
        this._shadow.transform.localScale = this.GetShadowScale();
    }

    private void OnDisable()
    {
        this._shadow.SetActive(false);
    }
}