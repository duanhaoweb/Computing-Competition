using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/TransformsConfig", fileName = "TransformsConfig")]
public class TransformsConfig : ScriptableObject
{
    public List<PosAndAngle> transforms;
}

[System.Serializable]
public struct PosAndAngle
{
    public Vector3 position;
    public Vector3 angle;

    public static PosAndAngle Zero => new PosAndAngle(Vector3.zero, Vector3.zero);

    public PosAndAngle(Vector3 position, Vector3 angle)
    {
        this.position = position;
        this.angle = angle;
    }
}