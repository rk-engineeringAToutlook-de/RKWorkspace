using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public sealed class NativePdfPickHotkey : IDisposable
{
    private const int CtrlAltPHotkeyId = 0x524B5050;
    private const int F8HotkeyId = 0x524B5046;
    private const int F9HotkeyId = 0x524B5047;
    private const int CtrlAltSpaceHotkeyId = 0x524B5053;
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModNoRepeat = 0x4000;
    private const uint VirtualKeyP = 0x50;
    private const uint VirtualKeyF8 = 0x77;
    private const uint VirtualKeyF9 = 0x78;
    private const uint VirtualKeySpace = 0x20;

    private readonly Window _window;
    private readonly NativeGlassOverlayDiagnostics _diagnostics;
    private HwndSource? _source;
    private bool _registeredCtrlAltP;
    private bool _registeredF8;
    private bool _registeredF9;
    private bool _registeredCtrlAltSpace;

    public NativePdfPickHotkey(Window window, NativeGlassOverlayDiagnostics diagnostics)
    {
        _window = window;
        _diagnostics = diagnostics;
    }

    public event EventHandler<NativePdfPickHotkeyEventArgs>? PickRequested;

    public void Register()
    {
        var handle = new WindowInteropHelper(_window).Handle;
        if (handle == IntPtr.Zero)
        {
            _diagnostics.Set("Hotkeys", "nicht registriert: Fenster-Handle fehlt");
            return;
        }

        _source = HwndSource.FromHwnd(handle);
        _source?.AddHook(OnMessage);
        _registeredCtrlAltSpace = TryRegister(handle, CtrlAltSpaceHotkeyId, ModControl | ModAlt | ModNoRepeat, VirtualKeySpace, "Strg+Alt+Leertaste");
        _registeredF9 = TryRegister(handle, F9HotkeyId, ModNoRepeat, VirtualKeyF9, "F9");
        _registeredF8 = TryRegister(handle, F8HotkeyId, ModNoRepeat, VirtualKeyF8, "F8");
        _registeredCtrlAltP = TryRegister(handle, CtrlAltPHotkeyId, ModControl | ModAlt | ModNoRepeat, VirtualKeyP, "Strg+Alt+P");

        _diagnostics.Set(
            "Hotkeys",
            $"Strg+Alt+Leertaste={Format(_registeredCtrlAltSpace)}, F9={Format(_registeredF9)}, F8={Format(_registeredF8)}, Strg+Alt+P={Format(_registeredCtrlAltP)}");
    }

    public void Dispose()
    {
        var handle = new WindowInteropHelper(_window).Handle;
        if (_registeredCtrlAltP && handle != IntPtr.Zero)
        {
            UnregisterHotKey(handle, CtrlAltPHotkeyId);
        }

        if (_registeredF8 && handle != IntPtr.Zero)
        {
            UnregisterHotKey(handle, F8HotkeyId);
        }

        if (_registeredF9 && handle != IntPtr.Zero)
        {
            UnregisterHotKey(handle, F9HotkeyId);
        }

        if (_registeredCtrlAltSpace && handle != IntPtr.Zero)
        {
            UnregisterHotKey(handle, CtrlAltSpaceHotkeyId);
        }

        _source?.RemoveHook(OnMessage);
    }

    private IntPtr OnMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WmHotkey)
        {
            return IntPtr.Zero;
        }

        var keyName = wParam.ToInt32() switch
        {
            CtrlAltSpaceHotkeyId => "Strg+Alt+Leertaste",
            F9HotkeyId => "F9",
            F8HotkeyId => "F8",
            CtrlAltPHotkeyId => "Strg+Alt+P",
            _ => null
        };

        if (keyName is not null)
        {
            handled = true;
            PickRequested?.Invoke(this, new NativePdfPickHotkeyEventArgs(keyName));
        }

        return IntPtr.Zero;
    }

    private static string Format(bool registered) => registered ? "OK" : "BLOCKIERT";

    private bool TryRegister(IntPtr handle, int id, uint modifiers, uint virtualKey, string label)
    {
        if (RegisterHotKey(handle, id, modifiers, virtualKey))
        {
            return true;
        }

        var error = Marshal.GetLastWin32Error();
        _diagnostics.Set("Hotkeys", $"{label} blockiert oder nicht verfuegbar (Win32 {error})");
        return false;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

public sealed class NativePdfPickHotkeyEventArgs : EventArgs
{
    public NativePdfPickHotkeyEventArgs(string keyName)
    {
        KeyName = keyName;
    }

    public string KeyName { get; }
}
