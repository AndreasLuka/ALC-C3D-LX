// (C) Copyright 2018 by  
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using aGi = Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(LX_GetStationOffset.MyCommands))]

namespace LX_GetStationOffset
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
        [CommandMethod("MyGroup", "MyCommand", "MyCommandLocal", CommandFlags.Modal)]
        public void MyCommand() // This method can have any name
        {
            // Put your command code here
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed;
            if (doc != null)
            {
                ed = doc.Editor;
                ed.WriteMessage("Hello, this is your first command.");

            }
        }

        // Modal Command with pickfirst selection
        [CommandMethod("MyGroup", "MyPickFirst", "MyPickFirstLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void MyPickFirst() // This method can have any name
        {
            PromptSelectionResult result = Application.DocumentManager.MdiActiveDocument.Editor.GetSelection();
            if (result.Status == PromptStatus.OK)
            {
                // There are selected entities
                // Put your command using pickfirst set code here
            }
            else
            {
                // There are no selected entities
                // Put your command code here
            }
        }

        // Application Session Command with localized name
        [CommandMethod("MyGroup", "MySessionCmd", "MySessionCmdLocal", CommandFlags.Modal | CommandFlags.Session)]
        public void MySessionCmd() // This method can have any name
        {
            // Put your command code here
        }

        // LispFunction is similar to CommandMethod but it creates a lisp 
        // callable function. Many return types are supported not just string
        // or integer.
        [LispFunction("MyLispFunction", "MyLispFunctionLocal")]
        public int MyLispFunction(ResultBuffer args) // This method can have any name
        {
            // Put your command code here

            // Return a value to the AutoCAD Lisp Interpreter
            return 1;
        }


        private IntegerCollection intColl = new IntegerCollection();
        private DBObjectCollection m_mrkers = new DBObjectCollection();
        private Editor ed;
        private Point3d curPt;
        private Alignment align = null;

        [CommandMethod("StaOffTest")]
        public void staoffcommand()
        {
            if (CivilApplication.ActiveDocument.GetAlignmentIds().Count > 0) runcommand();
        }

        private void runcommand()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            ed = doc.Editor;
            Database db = doc.Database;
            CivilDocument civdoc = CivilApplication.ActiveDocument;
            PromptEntityOptions entOpts = new PromptEntityOptions("Select alignment:");
            entOpts.SetRejectMessage("...not an ALignment object, try again.");
            entOpts.AddAllowedClass(typeof(Alignment), true);
            PromptEntityResult entRes = ed.GetEntity(entOpts);
            if (entRes.Status != PromptStatus.OK) return;            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                align = (Alignment)entRes.ObjectId.GetObject(OpenMode.ForRead);
                ed.PointMonitor += new PointMonitorEventHandler(StaOffPointMonitor);
                PromptPointOptions ptPrmpt = new PromptPointOptions("\nSelect point to get Station & Offset of:");
                ptPrmpt.AllowNone = true;
                PromptPointResult ptResult;
                while (true) //with the PointMonitor running it will update until user picks a point
                {
                    ptPrmpt.Message = "\nMove along alignment to track the Station & Offset:";
                    ptResult = ed.GetPoint(ptPrmpt);
                    if (ptResult.Status != PromptStatus.Other)
                        break;
                }
                ed.PointMonitor -= new PointMonitorEventHandler(StaOffPointMonitor);
                ClearMarkers();
                tr.Commit();
            }
        }

        private void StaOffPointMonitor(object sender, PointMonitorEventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            curPt = e.Context.RawPoint;
            double scale = 5; // This needs to have code to get it to scale up/down as the view is zoomed out/in
            Point3d pt2 = new Point3d();
            pt2 = align.GetClosestPointTo(curPt, false);
            showLine_and_X(curPt, pt2, scale);
        }

        private void showLine_and_X(Point3d curPt, Point3d pt3, double scale)
        {
            ClearMarkers();
            Line line1 = new Line(curPt, pt3);
            Line line2 = new Line(Utility.PolarPoint(pt3, line1.Angle + (Math.PI * 0.25), 0.15 * scale), Utility.PolarPoint(pt3, line1.Angle + (Math.PI * 1.25), 0.15 * scale));
            Line line3 = new Line();
            line3.StartPoint = line2.StartPoint;
            line3.EndPoint = line2.EndPoint;
            line3.TransformBy(Matrix3d.Rotation(Math.PI * 0.5, Vector3d.ZAxis, pt3));
            line1.ColorIndex = 1;
            line2.ColorIndex = 3;
            line3.ColorIndex = 3;
            aGi.TransientManager.CurrentTransientManager.AddTransient(line1, aGi.TransientDrawingMode.DirectShortTerm, 128, intColl);
            aGi.TransientManager.CurrentTransientManager.AddTransient(line2, aGi.TransientDrawingMode.DirectShortTerm, 128, intColl);
            aGi.TransientManager.CurrentTransientManager.AddTransient(line3, aGi.TransientDrawingMode.DirectShortTerm, 128, intColl);
            m_mrkers.Add(line1);
            m_mrkers.Add(line2);
            m_mrkers.Add(line3);
        }


        private void ClearMarkers()
        {
            for (int i = 0; i < m_mrkers.Count; i++)
            {
                aGi.TransientManager.CurrentTransientManager.EraseTransient(m_mrkers[i], intColl);
                m_mrkers[i].Dispose();
            }
            m_mrkers.Clear();
        }
    }
}