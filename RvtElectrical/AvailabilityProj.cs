using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace RvtElectrical
{
    public class AvailabilityProj : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(
          UIApplication a,
          CategorySet b)
        {
            try
            {
                Document doc = a.ActiveUIDocument.Document;

                if (doc.IsFamilyDocument)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
            
        }
    }
}
