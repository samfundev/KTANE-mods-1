using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class PortWidget : Widget
{


    [Flags]
    public enum PortType
    {
        None = 0,
        DVI = 1 << 0,
        Parallel = 1 << 1,
        PS2 = 1 << 2,
        RJ45 = 1 << 3,
        Serial = 1 << 4,
        StereoRCA = 1 << 5,
        ComponentVideo = 1 << 6,
        CompositeVideo = 1 << 7,
        USB = 1 << 8,
        HDMI = 1 << 9,
        VGA = 1 << 10,
        AC = 1 << 11,
        PCMCIA = 1 << 12,

    }

    void Start()
    {
        if (Ports == null)
        {
            Init();
        }
        OnQueryRequest += GetResult;
    }

    private List<string> Ports;
    public List<PortType> portTypes = new List<PortType>();
    
    

    

    public void Init(bool extended = false, List<PortType> unicornPorts = null)
    {
        Ports = new List<string>();
        var portList = PortType.None;
        //PortType portList = PortType.None;
        if (portTypes.Count == 0)
        {
            var portPlates = new List<List<PortType>>
            {
                new List<PortType>
                {
                    PortType.Serial, PortType.Parallel
                },
                new List<PortType>
                {
                    PortType.PS2, PortType.DVI, PortType.RJ45, PortType.StereoRCA
                },
                new List<PortType>
                {
                    PortType.HDMI,
                    PortType.USB,
                    PortType.ComponentVideo,
                    PortType.AC,
                    PortType.PCMCIA,
                    PortType.VGA,
                    PortType.CompositeVideo
                },
                new List<PortType>
                {
                    PortType.DVI,
                    PortType.StereoRCA,
                    PortType.HDMI,
                    PortType.ComponentVideo,
                    PortType.VGA,
                    PortType.CompositeVideo,
                    PortType.AC
                },
                new List<PortType>
                {
                    PortType.Parallel,
                    PortType.Serial,
                    PortType.PCMCIA,
                    PortType.VGA,
                    PortType.PS2,
                    PortType.RJ45,
                    PortType.USB,
                    PortType.AC
                }
            };

            var plate = portPlates[1];
            if (!extended)
            {
                if (Random.value > 0.5)
                {
                    plate = portPlates[0];
                }
            }
            else
            {
                plate = portPlates[Random.Range(0, portPlates.Count)];
            }
            foreach (var port in plate)
            {
                if (!(Random.value > 0.5)) continue;
                Ports.Add(port.ToString());
                portList |= port;
                portTypes.Add(port);
            }
        }
        else
        {
            var portTypeClone = portTypes.Distinct().ToList();
            portTypeClone.Remove(PortType.None);
            portTypes.Clear();
            if (portTypeClone.Count == 0)
            {
                portTypes.Add(PortType.None);
            }
            else
            {
                foreach (var port in portTypeClone)
                {
                    portList |= port;
                    Ports.Add(port.ToString());
                    portTypes.Add(port);
                }
            }
        }

        if (portList == PortType.None)
            Debug.Log("Added port widget: Empty plate");
        else
            Debug.Log("Added port widget: " + portList);
    }

    public string GetResult(string key, string data)
    {
        if (key == KMBombInfo.QUERYKEY_GET_PORTS)
        {
            return JsonConvert.SerializeObject((object)new Dictionary<string, List<string>>()
            {
                {
                    "presentPorts", Ports
                }
            });
        }
        return null;
    }
}