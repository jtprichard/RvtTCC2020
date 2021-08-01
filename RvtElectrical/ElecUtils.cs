using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RvtElectrical
{
    public static class ElecUtils
    {
        public static IList<Element> CollectDeviceByBoxNumber(IList<Element> devices, int boxnumber)
        //Method that returns a list of elements with the same box number
        //This was used for the original DeviceBox Constructor - No current uses
        {
            Guid guid = TCCElecSettings.BoxIdGuid;
            
            List<Element> returnDevices = new List<Element>();
            foreach (Element ele in devices)
            {
                Parameter param = ele.get_Parameter(guid);
                if (param.AsInteger() == boxnumber)
                {
                    returnDevices.Add(ele);
                }
            }
            return returnDevices;
        }

        public static IList<Element> CollectDeviceByBoxNumber(Document doc, int boxnumber)
        //Method that returns a list of elements with the same box number
        //This was used for the original DeviceBox Constructor - No current uses
        {
            Guid boxIdGuid = TCCElecSettings.BoxIdGuid;
            Guid deviceIdGuid = TCCElecSettings.DeviceIdGuid;

            //Create Filtered Element Collector & Filter for Electrical Fixtures
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementQuickFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);

            //Apply Filter
            IList<Element> devices = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();
            IList<Element> filteredDevices = devices
                .Where(d => d.get_Parameter(boxIdGuid).AsInteger() == boxnumber)
                //.Where(d => d.get_Parameter(deviceIdGuid) != null)
                .ToList();

            return filteredDevices;
        }
        
        public static string CircuitListToString(List<int> circuits, string prefix)
        //Method takes a list of circuits and turns them into a string
        //Using a "-" dash between circuits if sequential, 
        //and a "," comma between circuits if non sequential.
        //duplicate circuit numbers are ignored.
        {
            circuits.Sort();
            string circuitConcat = "";
            
            if(prefix == null || prefix == "")
            {
                circuitConcat = circuits[0].ToString();
            }
            else       
                circuitConcat = prefix + ": " + circuits[0].ToString();

            int prevCircuit = circuits[0];
            bool addDash = false;
            
            foreach (int circuit in circuits.Distinct().Skip(1))
            {
                if (circuit == prevCircuit + 1)
                {
                    if (circuit == circuits.Last())
                    {
                        circuitConcat = circuitConcat + "-" + circuit.ToString();
                    }
                    else
                    {
                        addDash = true;
                        prevCircuit++;
                    }
                }
                else
                {
                    if (addDash)
                    {
                        circuitConcat = circuitConcat + "-" + prevCircuit.ToString();
                    }
                    circuitConcat = circuitConcat + "," + circuit.ToString();
                    prevCircuit = circuit;
                    addDash = false;
                }
            }
            return circuitConcat;
        }
    }
}
