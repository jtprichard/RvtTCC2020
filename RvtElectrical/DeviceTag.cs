using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace RvtElectrical
{
    public class DeviceTag
    {
        public string TagFamily { get; set; }
        public string TagFamilyType { get; set; }
        public string Circuit { get; set; }
        public BuiltInCategory TagCategory { get; set; }
        public bool HasLeader { get; set; }
        public TagOrientation TagOrient { get; set; }
        public TagMode Mode { get; set; }
        public XYZ Location { get; set; }
        public Reference TagReference { get; set; }



        public DeviceTag() { }

        public bool TagDeviceBox(Document doc, View view)
        {
            try
            {
                //Desired tag info
                IndependentTag newTag = IndependentTag.Create(doc, view.Id, TagReference, HasLeader, Mode, TagOrient, Location);
                Element desiredTagType = FamilyLocateUtils.FindFamilyType(doc, typeof(FamilySymbol), TagFamily, TagFamilyType, TagCategory);

                if (desiredTagType != null)
                {
                    newTag.ChangeTypeId(desiredTagType.Id);
                }
                return true;
            }

            catch
            {
                return false;
            }



        }

        public static DeviceTag DevicePowerTag()
        {
            var deviceTag = new DeviceTag()
            {
                TagFamily = "Power Tag Multicategory",
                TagFamilyType = "Power",
                HasLeader = true,
                TagCategory = BuiltInCategory.OST_MultiCategoryTags,
                TagOrient = TagOrientation.Horizontal,
                Mode = TagMode.TM_ADDBY_MULTICATEGORY,

            };

            return deviceTag;
        }

        public static DeviceTag DeviceIGPowerTag()
        {
            var deviceTag = new DeviceTag()
            {
                TagFamily = "Power Tag Multicategory",
                TagFamilyType = "Power_IG",
                HasLeader = true,
                TagCategory = BuiltInCategory.OST_MultiCategoryTags,
                TagOrient = TagOrientation.Horizontal,
                Mode = TagMode.TM_ADDBY_MULTICATEGORY,

            };

            return deviceTag;
        }

        public static DeviceTag GeneralPowerTag()
        {
            var deviceTag = new DeviceTag()
            {
                TagFamily = "Power Tag Multicategory",
                TagFamilyType = "Power_NoBox",
                HasLeader = false,
                TagCategory = BuiltInCategory.OST_MultiCategoryTags,
                TagOrient = TagOrientation.Horizontal,
                Mode = TagMode.TM_ADDBY_MULTICATEGORY,

            };

            return deviceTag;
        }

        public static DeviceTag SpecialtyDeviceTagNormal()
        {
            var deviceTag = new DeviceTag()
            {
                TagFamily = "Device Tag",
                TagFamilyType = "Normal",
                HasLeader = false,
                TagCategory = BuiltInCategory.OST_MultiCategoryTags,
                TagOrient = TagOrientation.Horizontal,
                Mode = TagMode.TM_ADDBY_MULTICATEGORY,

            };

            return deviceTag;
        }

        public static DeviceTag SpecialtyDeviceTagPower()
        {
            var deviceTag = new DeviceTag()
            {
                TagFamily = "Device Tag",
                TagFamilyType = "Power",
                HasLeader = false,
                TagCategory = BuiltInCategory.OST_MultiCategoryTags,
                TagOrient = TagOrientation.Horizontal,
                Mode = TagMode.TM_ADDBY_MULTICATEGORY,

            };

            return deviceTag;
        }

        public static DeviceTag SpecialtyDeviceTagPowerIG()
        {
            var deviceTag = new DeviceTag()
            {
                TagFamily = "Device Tag",
                TagFamilyType = "Power_IG",
                HasLeader = false,
                TagCategory = BuiltInCategory.OST_MultiCategoryTags,
                TagOrient = TagOrientation.Horizontal,
                Mode = TagMode.TM_ADDBY_MULTICATEGORY,

            };

            return deviceTag;
        }
    }
}
