using HarmonyLib;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Gui;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using System;
using System.Reflection;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Utils;
using VRageMath;

namespace ToolHelper
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class RenderComponent : MySessionComponentBase
    {
        private readonly MyConcurrentHashSet<MyShipDrill> _drills = new MyConcurrentHashSet<MyShipDrill>();
        private readonly MyConcurrentHashSet<MyShipToolBase> _tools = new MyConcurrentHashSet<MyShipToolBase>();
        private bool _useRightClickDrillRadius;

        private static readonly MyStringId LineMaterial = MyStringId.GetOrCompute("WeaponLaser");
        private static readonly FieldInfo DrillWantsToCollect = AccessTools.Field(typeof(MyShipDrill), "m_wantsToCollect");

        public void Register(MyEntity entity)
        {
            var drill = entity as MyShipDrill;
            if (drill != null)
            {
                if (_drills.Add(drill))
                {
                    drill.OnClose += UnRegister;
                }
                return;
            }

            var tool = entity as MyShipToolBase;
            if (tool != null)
            {
                if (_tools.Add(tool))
                {
                    tool.OnClose += UnRegister;
                }
                return;
            }

            throw new InvalidOperationException($"Attempted to register entity with invalid type: {entity.GetType().Name}");
        }

        public void UnRegister(MyEntity entity)
        {
            var drill = entity as MyShipDrill;
            if (drill != null)
            {
                if (_drills.Remove(drill))
                {
                    drill.OnClose -= UnRegister;
                }
                return;
            }

            var tool = entity as MyShipToolBase;
            if (tool != null)
            {
                if (_tools.Remove(tool))
                {
                    tool.OnClose -= UnRegister;
                }
                return;
            }

            throw new InvalidOperationException($"Attempted to unregister entity with invalid type: {entity.GetType().Name}");
        }

        public bool IsRegistered(MyShipDrill drill) => _drills.Contains(drill);
        public bool IsRegistered(MyShipToolBase tool) => _tools.Contains(tool);
        public void ToggleDrillVisuals()
        {
            _useRightClickDrillRadius = !_useRightClickDrillRadius;
            ShowMessage(
                $"Drill visuals switched to {(_useRightClickDrillRadius ? "right-click" : "normal")} radius");
        }

        public void Clear()
        {
            foreach (var drill in _drills)
            {
                drill.OnClose -= UnRegister;
            }
            _drills.Clear();

            foreach (var tool in _tools)
            {
                tool.OnClose -= UnRegister;
            }
            _tools.Clear();
        }

        public override void Draw()
        {
            foreach (var drill in _drills)
            {
                var sphere = _useRightClickDrillRadius ? drill.DrillBase.GetCutoutSphere()
                    : drill.DrillBase.CutOut.Sphere;

                sphere.Radius++;

                var isDrilling = drill.DrillBase.IsDrilling
                    && (((bool)DrillWantsToCollect.GetValue(drill) || drill.Enabled) != _useRightClickDrillRadius);

                DrawSphere(sphere, drill, isDrilling);
            }

            foreach (var tool in _tools)
            {
                var relativeSphere = tool.DetectorSphere;

                DrawSphere(new BoundingSphereD(
                    Vector3D.Transform(relativeSphere.Center, tool.CubeGrid.WorldMatrix),
                    relativeSphere.Radius), tool, tool.Enabled);
            }
        }

        public override void LoadData()
        {
            MyAPIUtilities.Static.MessageEntered += OnMessageEntered;
        }

        protected override void UnloadData()
        {
            Clear();
            MyAPIUtilities.Static.MessageEntered -= OnMessageEntered;
        }
        private void OnMessageEntered(string message, ref bool sendToOthers)
        {
            if (!message.StartsWith("/th ", StringComparison.InvariantCultureIgnoreCase))
                return;

            sendToOthers = false;

            var command = message.Substring(4);

            if (command.Equals("clear", StringComparison.InvariantCultureIgnoreCase))
                Clear();
            else if (command.Equals("toggle drills", StringComparison.InvariantCultureIgnoreCase))
                ToggleDrillVisuals();
        }

        private static void DrawSphere(BoundingSphereD sphere, MyTerminalBlock block, bool running)
        {
            Color color;

            if (!block.IsFunctional)
                color = Color.Red;
            else if (running)
                color = Color.Yellow;
            else
                color = Color.Green;

            var matrix = MatrixD.CreateWorld(sphere.Center,
                block.WorldMatrix.Forward,
                block.WorldMatrix.Up);

            MySimpleObjectDraw.DrawTransparentSphere(ref matrix,
                (float)sphere.Radius,
                ref color,
                MySimpleObjectRasterizer.Wireframe,
                8,
                lineMaterial:LineMaterial);
        }
        private static void ShowMessage(string message) =>
            MyHud.Notifications.Add(new MyHudNotification(MyStringId.GetOrCompute(message)));
    }
}
