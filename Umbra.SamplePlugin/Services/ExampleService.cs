using Umbra.Common;
using Umbra.Game;

namespace Umbra.SamplePlugin.Services;

/// <summary>
/// This class demonstrates how to create a service in Umbra.
/// </summary>
[Service]
public class ExampleService
{
    public ExampleService(IZoneManager zoneManager, AnotherService anotherService)
    {
        // This constructor is invoked when the service is instantiated.
        // You can inject other services by adding them as parameters to the
        // constructor. Umbra will automatically resolve and inject the
        // dependencies for you.

        if (zoneManager.HasCurrentZone) {
            anotherService.Print($"You are currently in zone {zoneManager.CurrentZone.Name}.");
        }
    }

    /// <summary>
    /// Invoked when Umbra's core framework is being initialized and
    /// immediately before services are instantiated.
    /// </summary>
    [WhenFrameworkCompiling]
    private static void OnFrameworkCompiling()
    {
        // Do something when the framework is compiling, such as scanning for
        // types through reflection or other tasks that need to be done before
        // services are instantiated.

        // Note that services (even Dalamud ones) are unavailable at this point.
    }

    [WhenFrameworkDisposing]
    private static void OnFrameworkDisposing()
    {
        // Do something when the framework is disposing, such as cleaning up
        // resources or other tasks that need to be done before the framework
        // is disposed.
    }

    /// <summary>
    /// Invoked on every frame on the render thread.
    /// </summary>
    [OnDraw(executionOrder: 100)]
    private static void OnDraw()
    {
        // Do something when the game is drawing, such as rendering UI elements
        // or other tasks that need to be done during the draw phase.
        // Use the executionOrder parameter to specify the order in which this
        // method should be executed relative to other methods with the same
        // attribute.
    }

    /// <summary>
    /// Invoked on the framework thread every {interval} milliseconds.
    /// </summary>
    [OnTick(interval: 1000)]
    private static void OnTick()
    {
        // Do something every 1000 milliseconds (1 second), such as updating
        // internal state or other tasks that need to be done on a regular
        // interval. Use the interval parameter to specify the number of
        // milliseconds between each invocation of this method.
    }
}
