//
// (C) Copyright 2003-2019 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;

namespace RvtElectrical
{
    /// <summary>
    /// An electrical system item contains the name and id of an electrical system. 
    /// </summary>
    public class ElectricalSystemItem
    {
        /// <summary>
        /// Name of an electrical system
        /// </summary>
        private String _name;

        /// <summary>
        /// Id of an electrical system
        /// </summary>
        private ElementId _id;

        /// <summary>
        /// Id of an electrical system
        /// </summary>
        public ElementId Id
        {
            get
            {
                return _id;
            }
            private set { }
        }

        /// <summary>
        /// Name of an electrical system
        /// </summary>
        public String Name
        {
            get
            {
                return _name;
            }
            private set { }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="es"></param>
        public ElectricalSystemItem(ElectricalSystem es)
        {
            _name = es.Name;
            _id = es.Id;
        }
    }
}
