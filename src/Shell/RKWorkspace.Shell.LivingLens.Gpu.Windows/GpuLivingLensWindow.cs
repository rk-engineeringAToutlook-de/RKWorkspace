using RKWorkspace.Shell;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FormsScreen = System.Windows.Forms.Screen;

namespace RKWorkspace.Shell.LivingLens.Gpu.Windows;

public sealed class GpuLivingLensWindow : Window
{
    private readonly WorkspaceShellRuntime _runtime;
    private readonly GpuLivingLensSurface _surface;

    public GpuLivingLensWindow(WorkspaceShellRuntime runtime)
    {
        _runtime = runtime;
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

        _surface = new GpuLivingLensSurface(runtime, bounds);
        Content = _surface;
        KeyDown += OnKeyDown;
        Loaded += (_, _) =>
        {
            Activate();
            CompositionTarget.Rendering += OnRendering;
        };
        Closed += (_, _) => CompositionTarget.Rendering -= OnRendering;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        _surface.Tick();
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
            var position = Mouse.GetPosition(_surface);
            _surface.ActivatePickAt(position);
            _runtime.Shell.UpdateCarryState(WorkspaceCarryState.Picked, "HX-001A");
            return;
        }

        if (e.Key is Key.D1 or Key.NumPad1)
        {
            e.Handled = true;
            _surface.SetLensLook(GpuLivingLensLook.GlassBubble);
            return;
        }

        if (e.Key is Key.D2 or Key.NumPad2)
        {
            e.Handled = true;
            _surface.SetLensLook(GpuLivingLensLook.Wormhole);
            return;
        }

        if (e.Key is Key.D3 or Key.NumPad3)
        {
            e.Handled = true;
            _surface.SetLensLook(GpuLivingLensLook.Hybrid);
        }
    }
}
