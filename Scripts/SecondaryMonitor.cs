using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;

public class SecondaryMonitor : MonoBehaviour
{
    // デスクトップ左端の座標
    [SerializeField]
    TextMeshProUGUI left;

    // デスクトップ上端の座標
    [SerializeField]
    TextMeshProUGUI top;

    // 仮想デスクトップの幅
    [SerializeField]
    TextMeshProUGUI width;

    // 仮想デスクトップの高さ
    [SerializeField]
    TextMeshProUGUI height;

    [SerializeField]
    TextMeshProUGUI resolution;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(
        IntPtr hWnd,
        int X,
        int Y,
        int nWidth,
        int nHeight,
        bool bRepaint
    );

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        int hWndInsertHandler,
        int X,
        int Y,
        int cx,
        int cy,
        uint wFlags
    );

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(
        IntPtr hdc,
        IntPtr lprcClip,
        EnumMonitorsDelegate lpfnEnum,
        IntPtr dwData
    );

    delegate bool EnumMonitorsDelegate(
        IntPtr hMonitor,
        IntPtr hdcMonitor,
        ref Rect lprcMonitor,
        IntPtr dwData
    );

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

    // ウィンドウの左上角の座標と右下角の座標を格納する構造体
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfoEx
    {
        public int size;

        // デスクトップ単体の解像度
        public RECT Monitor;

        // 仮想デスクトップの解像度
        public RECT workArea;
        public uint Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;

        public void Init()
        {
            this.size = 40 + 2 * 32;
            this.DeviceName = string.Empty;
        }
    }

    private RECT rect;

    int SM_XVIRTUALSCREEN = 76;
    int SM_YVIRTUALSCREEN = 77;
    int SM_CXVIRTUALSCREEN = 78;
    int SM_CYVIRTUALSCREEN = 79;

    uint SWP_SHOWWINDOW = 0x0040;

    private static IntPtr windowHandler;

    void Awake()
    {
        Debug.Log("Wake ME!!");
        /*
        SetParamsを使うために必要
        強制的にボーダーレスウィンドウになり、解除できない（やり方がわからない）(SetWindowLongを使っても直らなかった)
        ウィンドウの位置も強制的にデスクトップの真ん中に戻される（Activateを呼んだ後に移動させても意味なかった）
        */
        // Display.main.Activate(0, 0, 0);
    }

    void Start()
    {
        left.text = "left : " + GetSystemMetrics(SM_XVIRTUALSCREEN).ToString();
        top.text = "top : " + GetSystemMetrics(SM_YVIRTUALSCREEN).ToString();
        width.text = "width : " + GetSystemMetrics(SM_CXVIRTUALSCREEN).ToString();
        height.text = "height : " + GetSystemMetrics(SM_CYVIRTUALSCREEN).ToString();

        windowHandler = GetActiveWindow();

        int style = GetWindowLong(windowHandler, -16);
        SetWindowLong(windowHandler, -16, style & ~0x00C00000);
        SetWindowPos(windowHandler, 0, 100, 100, 480, 360, SWP_SHOWWINDOW);

        EnumMonitorsDelegate emd = (
            IntPtr hMonitor,
            IntPtr hdcMonitor,
            ref Rect lprcMonitor,
            IntPtr dwData
        ) =>
        {
            MonitorInfoEx mi = new MonitorInfoEx();
            mi.size = (int)Marshal.SizeOf(mi);
            bool success = GetMonitorInfo(hMonitor, ref mi);
            if (success)
            {
                Debug.Log("width : " + (mi.Monitor.right - mi.Monitor.left).ToString());
                Debug.Log("height : " + (mi.Monitor.bottom - mi.Monitor.top).ToString());
                Debug.Log("work left : " + (mi.workArea.left).ToString());
                Debug.Log("work top : " + (mi.workArea.top).ToString());
            }
            return true;
        };

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, emd, IntPtr.Zero);
    }

    void Update()
    {
        resolution.text =
            "Width : " + Screen.width.ToString() + '\n' + "Height : " + Screen.height.ToString();
        if (Input.GetKey(KeyCode.H))
        {
            GetWindowRect(windowHandler, out rect);
            MoveWindow(
                windowHandler,
                rect.left - 10,
                rect.top,
                rect.right - rect.left,
                rect.bottom - rect.top,
                true
            );
        }
        if (Input.GetKey(KeyCode.J))
        {
            GetWindowRect(windowHandler, out rect);
            SetWindowPos(windowHandler, 0, rect.left, rect.top + 10, 480, 360, SWP_SHOWWINDOW);
            // MoveWindow(
            //     windowHandler,
            //     rect.left,
            //     rect.top + 10,
            //     rect.right - rect.left,
            //     rect.bottom - rect.top,
            //     true
            // );
        }
        if (Input.GetKey(KeyCode.K))
        {
            GetWindowRect(windowHandler, out rect);
            // Display.main.SetParams(480, 360, rect.left, rect.top - 10);
            MoveWindow(
                windowHandler,
                rect.left,
                rect.top - 10,
                rect.right - rect.left,
                rect.bottom - rect.top,
                true
            );
        }
        if (Input.GetKey(KeyCode.L))
        {
            GetWindowRect(windowHandler, out rect);
            MoveWindow(
                windowHandler,
                rect.left + 10,
                rect.top,
                rect.right - rect.left,
                rect.bottom - rect.top,
                true
            );
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetWindowRect(windowHandler, out rect);
            MoveWindow(
                windowHandler,
                Display.main.systemWidth,
                rect.top,
                rect.right - rect.left,
                rect.bottom - rect.top,
                true
            );
            // Screen.fullScreen = Screen.fullScreen ? false : true;
        }
    }
}
