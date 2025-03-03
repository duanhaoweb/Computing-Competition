using QFramework;
using UnityEngine;

public class ShadowController : MonoSingleton<ShadowController>
{
    [SerializeField] private GameObject shadow;
    /*[SerializeField] private GameObject yAxisShadow;
    [SerializeField] private GameObject zAxisShadow;*/

    /*public GameObject GetShadow(ShadowType type)
    {
        return type switch
        {
            ShadowType.XAxis => this.xAxisShadow,
            ShadowType.YAxis => this.yAxisShadow,
            ShadowType.ZAxis => this.zAxisShadow,
            _ => null
        };
    }*/

    public GameObject GetShadow()
    {
        return this.shadow;
    }
}

/*public enum ShadowType
{
        XAxis,
        YAxis,
        ZAxis
}*/