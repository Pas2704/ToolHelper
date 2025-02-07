using HarmonyLib;
using Sandbox.Game.Gui;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using VRage.Utils;

namespace ToolHelper.Patches
{
    [HarmonyPatch(typeof(MyShipToolBase), "CreateTerminalControls")]
    internal static class CreateToolControlsPatch
    {
        public static List<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();

            var index = list.FindLastIndex(i => i.opcode == OpCodes.Ret);
            list.Insert(index, CodeInstruction.CallClosure<Action>(AddToolControls));

            return list;
        }

        private static void AddToolControls()
        {
            var controlSwitch = new MyTerminalControlOnOffSwitch<MyShipToolBase>("THShowRadius",
                MyStringId.GetOrCompute("Show Radius"),
                MyStringId.GetOrCompute("Whether to show the radius of this tool"))
            {
                Getter = x => MySession.Static?.GetComponent<RenderComponent>()?.IsRegistered(x) ?? false,

                Setter = (x, value) =>
                {
                    var comp = MySession.Static?.GetComponent<RenderComponent>();

                    if (value)
                        comp.Register(x);
                    else
                        comp.UnRegister(x);
                }
            };

            controlSwitch.EnableToggleAction();
            MyTerminalControlFactory.AddControl(controlSwitch);
        }
    }
}
