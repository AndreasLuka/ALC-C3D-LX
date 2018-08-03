// (C) Copyright 2018 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_PlaneView.MyCommands))]

namespace ALC_PlaneView
{

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an intance member then the enclosing class is 
        // intantiated for each document. If the member is a static member then
        // the enclosing class is NOT intantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpid and
        // context menu.

        // Modal Command with localized name
        [CommandMethod("MyGroup", "ALC-CheckPlanView", CommandFlags.Modal)]
        public void MyCommand() // This method can have any name
        {
            // Put your command code here

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ViewTableRecord view = ed.GetCurrentView();

            ed.WriteMessage("Planview: {0}", view.ViewDirection.IsParallelTo(Matrix3d.Identity.CoordinateSystem3d.Zaxis).ToString());

            

        }



    }

}
