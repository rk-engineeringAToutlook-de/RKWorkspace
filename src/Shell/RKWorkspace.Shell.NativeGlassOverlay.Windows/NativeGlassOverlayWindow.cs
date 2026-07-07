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
        var bounds = FormsScreen.PrimaryScreen?.Bounds ?? new System.Drawing.Rectangle(0, 0, 1280, 720);

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
        Title = string.Empty;

        _surface = new NativeGlassOverlaySurface(runtime, bounds, options);
        _shaderLayer = new NativeGlassShaderLayer
        {
            Width = bounds.Width,
            Height = bounds.Height
        };

        var root = new Grid
        {
            Width = bounds.Width,
            Height = bounds.Height,
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
}
