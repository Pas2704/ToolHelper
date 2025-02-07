using System.Reflection;
using HarmonyLib;
using NLog;
using Sandbox.Game.World;
using VRage.Input;
using VRage.Plugins;

namespace ToolHelper
{
    public class Plugin : IHandleInputPlugin
    {
        internal static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Init(object gameInstance)
        {
            Log.Debug("ToolHelper: Patching");
            new Harmony("ToolHelper").PatchAll(Assembly.GetExecutingAssembly());
            Log.Info("ToolHelper: Patches applied");
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }
        public void HandleInput()
        {
            if (!MyInput.Static.IsAnyAltKeyPressed() || !MyInput.Static.IsNewKeyPressed(MyKeys.OemPlus))
                return;

            var comp = MySession.Static?.GetComponent<RenderComponent>();
            if (comp == null)
                return;

            comp.ToggleDrillVisuals();
        }
    }
}
