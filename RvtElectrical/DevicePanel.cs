using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Electrical;

namespace RvtElectrical
{
    public class DevicePanel
    {

        private readonly Document _doc;                                              //Revit Document
        public Element PanelElement { get; private set; }                   //Stores element reference in Panel
        public string PanelName { get; private set; }                       //Panel Name String
        public DevicePanelScheduleData PanelSchedule { get; private set; }  //Panel Schedule View 
        public ElectricalCircuitData CircuitData { get; private set; }      //Panel Circuit Information
        public ElectricalSystemType SystemType { get; private set; }        //Electrical System Type
        public bool HasPanelSchedule { get; private set; }                  //States whether there is a panel schedule associated
        public bool IsPowerPanel { get; private set; }                      //Notes if this is a Power Panel
        public bool IsDataPanel { get; private set; }                       //Notes if this is a Data Panel
        
        public DeviceId DeviceId { get; private set; }                      //Device ID
        public DeviceSystem System                                                //Device Panel System
        {
            get
            {
                return DeviceId.System;
            }
            private set { }
        }
        public Device Device                                               //Device Panel Device
        {
            get
            {
                return DeviceId.Device;
            }
            private set { }
        }
        public int AddressStart { get; private set; }                       //Panel Starting Address
        public int AddressEnd { get; private set; }                         //Panel Ending Address
        public string Venue { get; private set; }                           //Panel Venue

        public DevicePanel(Document doc, Element element)
        {

            //Assign Elements to Panel Property
            PanelElement = element;
            _doc = doc;
            PanelName = element.Name.ToString();
            DeviceId = new DeviceId(DeviceId.GetDeviceId(element, doc));

            CircuitData = new ElectricalCircuitData(element, doc);

            SystemType = CircuitData.SystemType;
            if (CircuitData.IsPowerCircuit)
                IsPowerPanel = true;
            else if (CircuitData.IsDataCircuit)
                IsDataPanel = true;
            else
            {
                IsPowerPanel = false;
                IsDataPanel = false;
            }

            AddressStart = element.get_Parameter(TCCElecSettings.PanelStartAddress).AsInteger();
            AddressEnd = element.get_Parameter(TCCElecSettings.PanelEndAddress).AsInteger();
            Venue = element.get_Parameter(TCCElecSettings.VenueGuid).AsString();
            GetPanelScheduleView();
            
        }

        public static Boolean TestPanelsForErrors(IList<DevicePanel> panels)
        {
            //Ensure each panel has a venue name
            List<string> venueNames = new List<string>();
            List<string> noVenueNames = new List<string>();
            foreach (DevicePanel panel in panels)
            {
                if (panel.Venue != "")
                    venueNames.Add(panel.Venue);
                else
                {
                    noVenueNames.Add(panel.PanelName);
                }
            }

            if (noVenueNames.Count() > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Panel ");
                foreach (string nvn in noVenueNames)
                {
                    sb.Append(nvn + ", ");
                }
                sb.Append("is missing venue name(s).\n Routine Exiting");
                TaskDialog.Show("Venue Error", sb.ToString());
                return false;
            }

            //Ensure panel has a name
            foreach (DevicePanel panel in panels)
            {
                if(panel.PanelName == "" || panel.PanelName == null)
                {
                    TaskDialog.Show("Panel Name Error", string.Format("One or more panel is missing a name.\nRoutine Exiting"));
                    return false;
                }
            }

            venueNames = venueNames.Distinct().ToList();

            //Test Circuits
            foreach(string venueName in venueNames)
            {
                List<int> panelCircuit = new List<int>();

                foreach (DevicePanel panel in panels)
                {
                    if (panel.Venue == venueName)
                    {
                        if(panel.AddressStart<1 || panel.AddressEnd>512)
                        {
                            TaskDialog.Show("Panel Address Error", string.Format("{0} - Start and End Address is Out of Range", panel.PanelName));
                            return false;
                        }
                        if (panel.AddressStart < panel.AddressEnd)
                        {
                            for (int i = panel.AddressStart; i <= panel.AddressEnd; i++)
                            {
                                panelCircuit.Add(i);
                            }
                        }
                        else
                        {
                            TaskDialog.Show("Panel Address Error", string.Format("[0] - Start and End Address of Panels is not Sequential", panel.PanelName));
                            return false;
                        }
                    }
                }
                if (panelCircuit.Count() != panelCircuit.Distinct().Count())
                {
                    TaskDialog.Show("Panel Address Error", string.Format("Venue: {0} - Panels Duplicate Addresses\nAddresses Must Be Discrete Per Venue", venueName));
                    return false;
                }
            }
            return true;
        }

        
        public static IList<DevicePanel> GetDevicePanels(Document doc)
        {
            var devicePanels = FilterAllDevicePanels(doc);
            return devicePanels;
        }

        public static IList<DevicePanel> GetDevicePanels(Document doc, DeviceSystem deviceScope)
        {
            var devicePanels = FilterAllDevicePanels(doc);

            //Use Linq query to filter by System
            List<DevicePanel> filteredDevicePanels = devicePanels
                .Where(dp => dp.System == deviceScope)
                .ToList();

            return filteredDevicePanels;
        }

        public static IList<DevicePanel> GetDevicePanels(Document doc, DeviceSystem deviceScope, ElectricalSystemType systemType)
        {
            var devicePanels = FilterAllDevicePanels(doc);

            //Use Linq query to filter by System
            List<DevicePanel> filteredDevicePanels = devicePanels
                .Where(dp => dp.System == deviceScope)
                .Where(dp => dp.SystemType == systemType)
                .ToList();

            return filteredDevicePanels;
        }

        private static IList<DevicePanel> FilterAllDevicePanels(Document doc)
        {
            //Get GUID Settings from Setting Class
            Guid DeviceIdGuid = TCCElecSettings.DeviceIdGuid;
            
            //Create Filtered Element Collector & Filter for Electrical Fixtures
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementQuickFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalEquipment);

            //Apply Filter
            IList<Element> equipment = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            //Use Linq query to find family instances that include a DeviceID
            var query = from element in collector
                        where element.get_Parameter(DeviceIdGuid) != null
                        select element;

            IList<Element> panels = new List<Element>();
            panels = query.Cast<Element>().ToList();

            IList<DevicePanel> devicePanels = new List<DevicePanel>();
            foreach (Element panel in panels)
            {
                devicePanels.Add(new DevicePanel(doc, panel));
            }

            //Use Linq query to filter by Panels
            List<DevicePanel> filteredDevicePanels = devicePanels
                .Where(dp => dp.Device == Device.Panel)
                .ToList();

            return filteredDevicePanels;
        }
        
        private void GetPanelScheduleView()
        {
            // get all PanelScheduleView instances in the Revit document.
            FilteredElementCollector fec = new FilteredElementCollector(_doc);
            ElementClassFilter PanelScheduleViewsAreWanted = new ElementClassFilter(typeof(PanelScheduleView));
            fec.WherePasses(PanelScheduleViewsAreWanted);
            List<Element> psViews = fec.ToElements() as List<Element>;
            HasPanelSchedule = false;

            foreach (Element psView in psViews)
            {
                ElementId panelId = PanelElement.Id;
                PanelScheduleView psv = psView as PanelScheduleView;
                if (psv.GetPanel() == panelId)
                {
                    PanelSchedule = new DevicePanelScheduleData(_doc, this);
                    HasPanelSchedule = true;
                }
            }
        }
    }
}
