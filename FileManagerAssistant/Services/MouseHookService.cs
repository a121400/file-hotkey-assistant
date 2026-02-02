using System.Diagnostics;
using FileManagerAssistant.Helpers;

namespace FileManagerAssistant.Services;

public class MouseHookService : IDisposable
{
    private IntPtr _hookId = IntPtr.Zero;
    private Win32Api.LowLevelMouseProc _proc;
    private bool _disposed;

    // 双击检测相关字段
    private DateTime _lastMiddleClickTime = DateTime.MinValue;
    private int _clickCount = 0;
    private const int DoubleClickThresholdMs = 500; // 双击时间阈值（毫秒）

    public event Action? MiddleButtonDoubleClicked;

    public MouseHookService()
    {
        _proc = HookCallback;
    }

    public void Start()
    {
        _hookId = SetHook(_proc);
    }

    public void Stop()
    {
        if (_hookId != IntPtr.Zero)
        {
            Win32Api.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    private IntPtr SetHook(Win32Api.LowLevelMouseProc proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        return Win32Api.SetWindowsHookEx(
            Win32Api.WH_MOUSE_LL,
            proc,
            Win32Api.GetModuleHandle(curModule.ModuleName),
            0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)Win32Api.WM_MBUTTONDOWN)
        {
            var now = DateTime.Now;
            var elapsed = (now - _lastMiddleClickTime).TotalMilliseconds;

            if (elapsed <= DoubleClickThresholdMs)
            {
                // 在阈值时间内的第二次点击，触发双击事件
                _clickCount++;
                if (_clickCount >= 2)
                {
                    MiddleButtonDoubleClicked?.Invoke();
                    _clickCount = 0;
                    _lastMiddleClickTime = DateTime.MinValue;
                }
            }
            else
            {
                // 超出阈值，重新计数
                _clickCount = 1;
            }

            _lastMiddleClickTime = now;
        }
        return Win32Api.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public static (int X, int Y) GetCursorPosition()
    {
        Win32Api.GetCursorPos(out var point);
        return (point.X, point.Y);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
