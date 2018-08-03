// (C) Copyright 2018 by AL
//

using System;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_StairJig.MyCommands))]

namespace ALC_StairJig
{

    public class MyCommands
    {


        // Modal Command with pickfirst selection
        [CommandMethod("MyGroup", "MySelectAlignment", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void AlignmentTest()
        {
            // Document doc = Application.DocumentManager.MdiActiveDocument;

            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


            ObjectId alignId = MyFunctions.SelectAlignment();

            if (alignId == ObjectId.Null) return;


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Alignment align = (Alignment)alignId.GetObject(OpenMode.ForRead);
                ed.WriteMessage(align.Name);
            }

        }
    }
}

