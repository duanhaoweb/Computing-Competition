using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class ScenesShoot  : MonoBehaviour
{
    public Texture2D CaptureScreenshot() 
    {
        //获取屏幕尺寸
        int width = Screen.width;
        int height = Screen.height;
        //创建临时渲染
        RenderTexture rt = RenderTexture.GetTemporary(width,height,24);
        //检查主摄像机是否存在
        Camera mainCamera = Camera.main;

        if (mainCamera == null) 
        {
            Debug.LogError(Constants.CAMERA_NOT_FOUND);
            return null;
        }

        //设置摄像机渲染目标
        mainCamera.targetTexture = rt;
        RenderTexture.active = rt;
        mainCamera.Render();

        //读取像素数据
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0,0,width,height),0,0);
        screenshot.Apply();

        //重置渲染目标并释放资源
        mainCamera.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        //缩小截图
        Texture2D resizedScreenshot = ResizeTexture(screenshot,width/6,height/6);
        Destroy(screenshot);

        return resizedScreenshot;
    }

    //辅助方法
    private Texture2D ResizeTexture(Texture2D original,int newWidth,int newHeight) 
    {
        // 创建渲染纹理
        RenderTexture rt = RenderTexture.GetTemporary(newWidth,newHeight,24);
        RenderTexture.active = rt;

        //使用GPU缩放
        Graphics.Blit(original, rt);

        //读取缩放后的数据
        Texture2D resized = new Texture2D(newWidth,newHeight,TextureFormat.RGB24,false);
        resized.ReadPixels(new Rect(0,0,newWidth,newHeight),0,0);
        resized.Apply();

        //释放资源
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return resized;
    }
}
