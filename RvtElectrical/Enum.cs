using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtElectrical
{
    public enum System
    // An enumeration type for Specialty Device System
    // Integer keys match Device ID standards
    {
        Unknown = 0,
        PerfLighting = 10000000,
        ArchLighting = 20000000,
        PerfSVC = 30000000,
        CablePasses = 60000000,
        PerfMachinery = 70000000,
        SFX = 80000000,
        General = 90000000
    }

    public enum SystemCode
    //An enumeration type for Specialty Device System for Plate Coding
    //Integer keys match Device ID standards
    {
          U = 0,
          L = 10000000,
          A = 20000000,
          S = 30000000,
          C = 60000000,
          M = 70000000,
          F = 80000000,
          G = 90000000
    }

    public enum Device
    //An enumaration type for Specialty Device Types
    //Integer keys match Device ID standards
    {
        Connector = 0,
        Faceplate = 1000000,
        Panel = 2000000,
        Rack = 3000000
    }

    public enum Voltage
    //An enumaration type for Specialty Device Types
    //Integer keys match Device ID standards
    {
        LowVoltage = 0,
        High120V1Ph = 100000,
        High208V1Ph = 200000,
        High208V3Ph = 300000,
        High277V1Ph = 400000,
        High480V3Ph = 500000,
        MixedVoltage = 900000
    }

}
