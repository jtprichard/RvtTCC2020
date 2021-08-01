using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;

namespace IUpdateRvtElec
{

    public class CircuitLabeWatcherUpdater : IUpdater
    {

        static CircuitLabeWatcherUpdater _updater = null;
        
        // DMU updater to call class to update device box concat label to new entry

        static AddInId _appId;
        static UpdaterId _updaterId;

        public CircuitLabeWatcherUpdater(AddInId id)
        {
            _appId = id;
            _updaterId = new UpdaterId(_appId, new Guid(
                "77128A86-8102-4605-B2F9-A1939D193A00"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Application app = doc.Application;

            foreach (ElementId eid in data.GetModifiedElementIds())
            {
                Element ele = doc.GetElement(eid);

                TaskDialog.Show("Element Changed", "MEP Fixture " + ele.Name.ToString() + " has changed");
                //Finish search for element and provide feedback
            }
        }

        public string GetAdditionalInformation()
        {
            return "Entertainment BIM Solutions";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.MEPFixtures;
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public string GetUpdaterName()
        {
            return "CircuitLabelWatcherUpdater";
        }


    }

}
