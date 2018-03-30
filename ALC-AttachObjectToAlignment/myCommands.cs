// (C) Copyright 2018 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;


[assembly: CommandClass(typeof(ALC_AttachObjectToAlignment.MyCommands))]

namespace ALC_AttachObjectToAlignment
{
    public class MyCommnds { }

    public class PtTransOverrule : TransformOverrule { }

}
