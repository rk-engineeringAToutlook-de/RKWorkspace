using RKWorkspace.Shell;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FormsScreen = System.Windows.Forms.Screen;

namespace RKWorkspace.Shell.NativeGlassOverlay.Windows;

public sealed class NativeGlassOverlayWindow : Window
{
    private readonly WorkspaceShellRuntime _runtime;
    private readonly NativeGlassOverlaySurface _surface;
    private readonly NativeGlassShaderLayer _shaderLayer;
    private readonly NativeGlassOverlayOptions _options;
    private NativePdfPickHotkey? _pickHotkey;
    private NativePdfContextPipeListener? _contextPipeListener;

    public NativeGlassOverlayWindow(WorkspaceShellRuntime runtime, NativeGlassOverlayOptions options)
    {
        _runtime = runtime;
        _options = options;
        var pixelBounds = FormsScreen.PrimaryScreen?.Bounds ?? new System.Drawing.Rectangle(0, 0, 1280, 720);
        var dipBounds = PrimaryScreenDipBounds(pixelBounds);

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        Left = dipBounds.Left;
        Top = dipBounds.Top;
        Width = dipBounds.Width;
        Height = dipBounds.Height;
        Title = string.Empty;

        _surface = new NativeGlassOverlaySurface(runtime, pixelBounds, dipBounds.Width, dipBounds.Height, options);
        _shaderLayer = new NativeGlassShaderLayer
        {
            Width = dipBounds.Width,
            Height = dipBounds.Height
        };

        var root = new Grid
        {
            Width = dipBounds.Width,
            Height = dipBounds.Height,
            Background = System.Windows.Media.Brushes.Transparent
        };
        root.Children.Add(_shaderLayer);
        root.Children.Add(_surface);
        Content = root;

        KeyDown += OnKeyDown;
        _surface.PointerInputModeChanged += (_, _) => UpdatePointerInputMode();
        SourceInitialized += (_, _) =>
        {
            _surface.MarkCaptureExclusion(false);
            _pickHotkey = new NativePdfPickHotkey(this, _surface.Diagnostics);
            _pickHotkey.PickRequested += (_, args) => _surface.PickSelectedPdfFromGesture(args.KeyName);
            _pickHotkey.Register();
            if (_options.ContextListenerEnabled)
            {
                _contextPipeListener = new NativePdfContextPipeListener(
                    _options.ContextPipeName,
                    Dispatcher,
                    _surface.PickPdfFromContextPath,
                    _surface.Diagnostics);
                _contextPipeListener.Start();
            }

            UpdatePointerInputMode();
        };
        Loaded += (_, _) =>
        {
            Activate();
            if (_options.PickImmediately)
            {
                Dispatcher.BeginInvoke(() => _surface.PickLoadedPdfFromContext());
            }

            CompositionTarget.Rendering += OnRendering;
        };
        Closed += (_, _) =>
        {
            CompositionTarget.Rendering -= OnRendering;
            _pickHotkey?.Dispose();
            _contextPipeListener?.Dispose();
        };
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        _surface.Tick();
        _shaderLayer.Tick(_surface.CreateShaderSnapshot());
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
            return;
        }

        if (e.Key == Key.Space && Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            e.Handled = true;
            _surface.ActivatePickAt(Mouse.GetPosition(_surface));
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
        }
    }

    private void UpdatePointerInputMode()
    {
        if (!_options.RealPdfGestureMode)
        {
            NativeGlassWindowInteractivity.SetClickThrough(this, false);
            return;
        }

        NativeGlassWindowInteractivity.SetClickThrough(this, !_surface.WantsPointerInput);
    }

    private static Rect PrimaryScreenDipBounds(System.Drawing.Rectangle pixelBounds)
    {
        var primaryWidth = SystemParameters.PrimaryScreenWidth;
        var primaryHeight = SystemParameters.PrimaryScreenHeight;
        if (primaryWidth <= 1 || primaryHeight <= 1)
        {
            return new Rect(0, 0, pixelBounds.Width, pixelBounds.Height);
        }

        var scaleX = Math.Max(0.01, pixelBounds.Width / primaryWidth);
        var scaleY = Math.Max(0.01, pixelBounds.Height / primaryHeight);
        return new Rect(
            pixelBounds.Left / scaleX,
            pixelBounds.Top / scaleY,
            primaryWidth,
            primaryHeight);
    }
}
