using System.Collections.Generic;
using Assets.Scripts.Rules;
using Newtonsoft.Json;

namespace VanillaRuleModifierAssembly
{
    public class QueryablePorts
    {
        public static QueryableProperty HasParallelPort = new QueryableProperty
        {
            Name = "portXPresent",
            Text = "there is a parallel port present on the bomb",
            QueryFunc = ((BombComponent comp, Dictionary<string, object> args) => WidgetHelpers.IsPortPresent(comp.Bomb.WidgetManager, PortWidget.PortType.Parallel))
        };

        public static QueryableProperty HasSerialPort = new QueryableProperty
        {
            Name = "portXPresent",
            Text = "there is a serial port present on the bomb",
            QueryFunc = ((BombComponent comp, Dictionary<string, object> args) => WidgetHelpers.IsPortPresent(comp.Bomb.WidgetManager, PortWidget.PortType.Serial))
        };

        public static QueryableProperty HasRJ45Port = new QueryableProperty
        {
            Name = "portXPresent",
            Text = "there is a RJ-45 port present on the bomb",
            QueryFunc = ((BombComponent comp, Dictionary<string, object> args) => WidgetHelpers.IsPortPresent(comp.Bomb.WidgetManager, PortWidget.PortType.RJ45))
        };

        public static QueryableProperty HasPS2Port = new QueryableProperty
        {
            Name = "portXPresent",
            Text = "there is a PS2 port present on the bomb",
            QueryFunc = ((BombComponent comp, Dictionary<string, object> args) => WidgetHelpers.IsPortPresent(comp.Bomb.WidgetManager, PortWidget.PortType.PS2))
        };

        public static QueryableProperty HasDVIPort = new QueryableProperty
        {
            Name = "portXPresent",
            Text = "there is a DVI-D port present on the bomb",
            QueryFunc = ((BombComponent comp, Dictionary<string, object> args) => WidgetHelpers.IsPortPresent(comp.Bomb.WidgetManager, PortWidget.PortType.DVI))
        };

        public static QueryableProperty HasRCAPort = new QueryableProperty
        {
            Name = "portXPresent",
            Text = "there is a Stereo RCA port present on the bomb",
            QueryFunc = ((BombComponent comp, Dictionary<string, object> args) => WidgetHelpers.IsPortPresent(comp.Bomb.WidgetManager, PortWidget.PortType.StereoRCA))
        };

        public static QueryableProperty EmptyPortPlate = new QueryableProperty
        {
            Name = "emptyPlatePresent",
            Text = "there is an empty port plate present on the bomb",
            QueryFunc = ((BombComponent comp, Dictionary<string, object> args) => IsEmptyPlatePresent(comp.Bomb.WidgetManager))
        };

        public static bool IsEmptyPlatePresent(WidgetManager widgetManager)
        {
            List<string> widgetQueryResponses = widgetManager.GetWidgetQueryResponses(KMBombInfo.QUERYKEY_GET_PORTS, null);
            foreach (string value in widgetQueryResponses)
            {
                Dictionary<string, List<string>> dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value);
                try
                {
                    if (dictionary["presentPorts"].Count == 0)
                    {
                        return true;
                    }
                }
                catch (KeyNotFoundException)
                {
                }
            }
            return false;
        }

        public static List<QueryableProperty> PortList = new List<QueryableProperty>
        {
            HasDVIPort,HasPS2Port,HasRJ45Port,HasRCAPort,HasParallelPort,HasSerialPort
        };
    }
}