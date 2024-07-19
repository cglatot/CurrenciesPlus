using Dalamud.Plugin.Services;
using Umbra.Common;

namespace Umbra.SamplePlugin.Services;

[Service]
public class AnotherService(IChatGui chatGui)
{
    /// <summary>
    /// Prints a message to the chat window.
    /// </summary>
    /// <param name="message">The message to print.</param>
    public void Print(string message)
    {
        chatGui.Print(message);
    }
}
