// (C) Copyright 2018 by Andreas Luka (Lu An Jie)
//

using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.Civil.DatabaseServices;
using acEntity = Autodesk.AutoCAD.DatabaseServices.Entity; //alias for AutoCAD entities to avoid ambigues reference between civil3d and autocad db

using static LX_Anchor.MyFunctions;

namespace LX_Anchor
{
    class MyOverrules
    {
        public class DrawableOverruleAnchor : DrawableOverrule
        {
            public override bool WorldDraw(Drawable d, WorldDraw wd)
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                //Circle c = (Circle)d;
                //wd.Geometry.Circle(c.Center, c.Radius * 0.5, c.Normal);
                return base.WorldDraw(d, wd);
            }
        }


        public class DrawableOverruleAlignment : DrawableOverrule
        {
            public override bool WorldDraw(Drawable d, WorldDraw wd)
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                string entryName = "anchor";

                Alignment a = (Alignment)d;

                using (OpenCloseTransaction tr = db.TransactionManager.StartOpenCloseTransaction())
                {
                    try
                    {
                        ResultBuffer rbHost = ReadXRecord(a.Id, entryName);
                        foreach (TypedValue tv in rbHost.AsArray())
                        {
                            ObjectId idEntity = (ObjectId)tv.Value;
                            ResultBuffer rbEntity = ReadXRecord(idEntity, entryName);
                            // get entity 
                            acEntity enAnchored = (acEntity)tr.GetObject(idEntity, OpenMode.ForWrite);
                            Point3d ptAnchored = Point3d.Origin;
                            Point2d ptHost = new Point2d(Double.NaN, Double.NaN);

                            Double offset = 0;
                            Double station = (Double)rbEntity.AsArray()[1].Value;
                            if (enAnchored is Circle c)
                            {
                                ptAnchored = c.Center;
                                if (InRangePoint(a, ref ptHost, station, offset))
                                {
                                    Vector3d dist = new Point3d(ptHost.X, ptHost.Y, 0) - new Point3d(ptAnchored.X, ptAnchored.Y, 0);
                                    c.TransformBy(Matrix3d.Displacement(dist));
                                }
                                else ed.WriteMessage("\n Error: Object not in range of the alignment");

                            }
                            if (enAnchored is CogoPoint cp)
                            {

                                // ptAnchored = new Point3d(cp.Northing, cp.Easting, 0);
                                // ed.WriteMessage(cp.Location.ToString());
                                if (InRangePoint(a, ref ptHost, station, offset))
                                {
                                    // move CogoPoint
                                    cp.Northing = ptHost.Y;
                                    cp.Easting = ptHost.X;
                                }
                                else ed.WriteMessage("\n Error: Object not in range of the alignment");
                            }
                        };
                        tr.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        ed.WriteMessage("\n Error in overrule: {0}", ex.Message);
                    }
                }
                return base.WorldDraw(d, wd); ;
            }
        }
    }
}

