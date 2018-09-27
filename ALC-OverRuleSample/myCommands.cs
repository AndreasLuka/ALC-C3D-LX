
// (C) Copyright 2018 by Andreas Luka
//
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;


using System;
using System.Collections.Generic;

namespace ALC_OverRuleSample
{
    public class PtTransOverrule : TransformOverrule
    {
        
        static public PtTransOverrule theOverrule = new PtTransOverrule(); // A static pointer to our overrule instance
        static internal List<ObjectId> _curves = new List<ObjectId>(); // A list of the curves that have had points attached to
        static bool overruling = false; // A flag to indicate whether we're overruling
        public PtTransOverrule() { }
        
        // Ouu primary overruled function
        public override void TransformBy(Entity e, Matrix3d mat)
        {
            // We only care about points
            DBPoint pt = e as DBPoint;
            if (pt != null)
            {
                Database db = HostApplicationServices.WorkingDatabase;

                // For each curve, let's check whether our point is on it

                bool found = false;

                // We're using an Open/Close transaction, to avoid problems
                // with us using transactions in an event handler

                OpenCloseTransaction tr = db.TransactionManager.StartOpenCloseTransaction();
                using (tr)
                {
                    foreach (ObjectId curId in _curves)
                    {
                        DBObject obj = tr.GetObject(curId, OpenMode.ForRead);
                        Curve cur = obj as Curve;
                        if (cur != null)
                        {
                            Point3d ptOnCurve = cur.GetClosestPointTo(pt.Position, false);
                            Vector3d dist = ptOnCurve - pt.Position;
                            if (dist.IsZeroLength(Tolerance.Global))
                            {
                                Point3d pos = cur.GetClosestPointTo(pt.Position.TransformBy(mat), false);
                                pt.Position = pos;
                                found = true;
                                break;
                            }
                        }
                    }

                    // If the point isn't on any curve, let the standard
                    // TransformBy() do its thing

                    if (!found)
                    {
                        base.TransformBy(e, mat);
                    }
                }
            }
        }


        [CommandMethod("POC")]
        public void CreatePointOnCurve()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Ask the user to select a curve
            PromptEntityOptions opts = new PromptEntityOptions( "\nSelect curve at the point to create: ");
            opts.SetRejectMessage("\nEntity must be a curve.");
            opts.AddAllowedClass(typeof(Curve), false);

            PromptEntityResult per = ed.GetEntity(opts);
            ObjectId curId = per.ObjectId;
            if (curId != ObjectId.Null)
            {

                // Let's make sure we'll be able to see our point
                db.Pdmode = 97;  // square with a circle
                db.Pdsize = -10; // relative to the viewport size

                Transaction tr =  doc.TransactionManager.StartTransaction();
                using (tr)
                {

                    DBObject obj = tr.GetObject(curId, OpenMode.ForRead);
                    Curve cur = obj as Curve;
                    if (cur != null)
                    {
                        // Out initial point should be the closest point
                        // on the curve to the one picked
                        Point3d pos = cur.GetClosestPointTo(per.PickedPoint, false);
                        DBPoint pt = new DBPoint(pos);

                        // Add it to the same space as the curve
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(cur.BlockId, OpenMode.ForWrite );
                        ObjectId ptId = btr.AppendEntity(pt);
                        tr.AddNewlyCreatedDBObject(pt, true);
                    }
                    tr.Commit();

                    // And add the curve to our central list
                    _curves.Add(curId);
                }

                // Turn on the transform overrule if it isn't already
                if (!overruling)
                {
                    ObjectOverrule.AddOverrule( RXClass.GetClass(typeof(DBPoint)),  PtTransOverrule.theOverrule, true);
                    overruling = true;
                    TransformOverrule.Overruling = true;
                }
            }
        }

    }
    public class OverrulePIAlignment : DrawableOverrule
    {
       static public OverrulePIAlignment theOverrule = new OverrulePIAlignment();


        public override bool WorldDraw(Drawable drawable, WorldDraw wd)
        {
            double easting = 0;
            double northing = 0;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            // draw the base class
            bool ret = base.WorldDraw(drawable, wd);

            // get alignment startpoint and endpoint
            Alignment align = (Alignment)drawable;

            int maxSteps = 10;
            if (align.Length > 0.32)
            {
                    maxSteps = (int)(align.Length / 0.32);
                    // ed.WriteMessage(maxSteps.ToString());
                for (int i = 0; i <= maxSteps; i++)
                {
                    align.PointLocation(i * 0.32, 1, ref easting, ref northing);
                    Point3d ptLeft = new Point3d(easting, northing, 0);
                    align.PointLocation(i * 0.32, -1, ref easting, ref northing);
                    Point3d ptRight = new Point3d(easting, northing, 0);
                    wd.Geometry.WorldLine(ptLeft, ptRight);

                };
            };

            Solid3d solidStep = new Solid3d();
            solidStep.CreateBox(1, 2, 1);

            solidStep.WorldDraw(wd);
            solidStep.Dispose();

            // return the base
            return ret;
        }


        public override bool IsApplicable(RXObject overruledSubject)
        {
            Alignment align = overruledSubject as Alignment;
            if (align == null) return false;
            else return (align.StyleName == "Stair");
        }
    }
    public class Commands

    {

        public void Overrule(bool enable)
        {
            // Regen to see the effect
            // (turn on/off Overruling and LWDISPLAY)
            DrawableOverrule.Overruling = enable;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("REGEN3\n", true, false, false);
            doc.Editor.Regen();
        }

        [CommandMethod("OVERRULE1")]
        public void OverruleStart()
        {
            ObjectOverrule.AddOverrule (RXClass.GetClass(typeof(Alignment)), OverrulePIAlignment.theOverrule, true);
            Overrule(true);
        }


        [CommandMethod("OVERRULE0")]
        public void OverruleEnd()
        {
            Overrule(false);
        }
    }

}
