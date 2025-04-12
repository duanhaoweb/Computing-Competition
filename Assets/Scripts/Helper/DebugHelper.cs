using UnityEngine;
using UnityEngine.UI;

namespace Helper
{
    public static class DebugHelper
    {
        public static void DrawLineOnCanvas(Vector2 screenPoint, Transform parentTransform)
        {
            GameObject lineObj = new GameObject("Line");
            lineObj.transform.SetParent(parentTransform.transform);

            Image lineImage = lineObj.AddComponent<Image>();
            lineImage.color = Color.yellow;

            RectTransform rectTransform = lineObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;

            // Calculate line length and angle
            float length = screenPoint.magnitude;
            float angle = Mathf.Atan2(screenPoint.y, screenPoint.x) * Mathf.Rad2Deg;

            rectTransform.sizeDelta = new Vector2(length, 2f); // 2 pixels thick
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            rectTransform.localScale = Vector3.one;
        }
    }
}