using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ALC_Stair
{
    class myFunctions
    {

        public void SetXrecord(ObjectId id, string key, ResultBuffer rb)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent != null)
                {
                    ent.UpgradeOpen();
                    ent.CreateExtensionDictionary();
                    DBDictionary xd = (DBDictionary)tr.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite);
                    Xrecord xr = new Xrecord();
                    xr.Data = rb;
                    xd.SetAt(key, xr);
                    tr.AddNewlyCreatedDBObject(xr, true);
                }
                tr.Commit();
            }
        }



        public static Solid3d CreateStair3D(int numStairs, double riser, double tread, double landing, double width)
        {
            // Create Polyline to Sweep 
            Polyline pSection = new Polyline();

            pSection.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);

            int iVertex = 1;
            for (int i = 1; i <= numStairs; i++)
            {
                pSection.AddVertexAt(iVertex++, new Point2d((i - 1) * tread, i * riser), 0, 0, 0);
                pSection.AddVertexAt(iVertex++, new Point2d(i * tread, i * riser), 0, 0, 0);
            };

            if (landing > 0)
            {
                pSection.AddVertexAt(iVertex++, new Point2d(numStairs * tread + landing, numStairs * riser), 0, 0, 0);
                pSection.AddVertexAt(iVertex++, new Point2d(numStairs * tread + landing, (numStairs - 1) * riser), 0, 0, 0);
                pSection.AddVertexAt(iVertex++, new Point2d((numStairs * tread), (numStairs - 1) * riser), 0, 0, 0);
            }
            else
            {
                pSection.AddVertexAt(iVertex++, new Point2d((numStairs * tread), (numStairs - 1) * riser), 0, 0, 0);
            }

            pSection.AddVertexAt(iVertex++, new Point2d(tread, 0), 0, 0, 0);
            pSection.Closed = true;

            // rotate polyline around z-Axis
            Matrix3d matrix = Matrix3d.Rotation(Math.PI / 2, Vector3d.XAxis, Point3d.Origin);
            pSection.TransformBy(matrix);

            // Create a Polyline for sweep path
            Polyline pline = new Polyline();
            pline.AddVertexAt(0, new Point2d(0, -width / 2), 0, 0, 0);
            pline.AddVertexAt(1, new Point2d(0, width / 2), 0, 0, 0);

            // createStair3D sweep options 
            SweepOptionsBuilder sob = new SweepOptionsBuilder();
            sob.Align = SweepOptionsAlignOption.AlignSweepEntityToPath;
            sob.BasePoint = pline.StartPoint;
            sob.Bank = true;

            Solid3d stair3d = new Solid3d();
            stair3d.RecordHistory = true;
            stair3d.CreateSweptSolid(pSection, pline, sob.ToSweepOptions());
            return stair3d;
        }

 

        public static Point3d GetStairStartPoint(Solid3d stair)
        {
            GripDataCollection grips = new GripDataCollection();
            GetGripPointsFlags bitFlags = GetGripPointsFlags.GripPointsOnly;

            stair.GetGripPoints(grips, 0, 0, Application.DocumentManager.MdiActiveDocument.Editor.GetCurrentView().ViewDirection, bitFlags);
            return grips[0].GripPoint;
        }

        public static double GetStairAngle(Solid3d stair)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            GripDataCollection grips = new GripDataCollection();
            GetGripPointsFlags bitFlags = GetGripPointsFlags.GripPointsOnly;

            stair.GetGripPoints(grips, 0, 0, doc.Editor.GetCurrentView().ViewDirection, bitFlags);

            Point2d start = new Point2d(grips[2].GripPoint.X, grips[2].GripPoint.Y);
            Point2d end = new Point2d(grips[4].GripPoint.X, grips[4].GripPoint.Y);
            return start.GetVectorTo(end).Angle;
        }


        public static ResultBuffer GetXrecord(ObjectId id, string key)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            ResultBuffer result = new ResultBuffer();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Xrecord xRec = new Xrecord();
                Entity ent = tr.GetObject(id, OpenMode.ForRead, false) as Entity;
                if (ent != null)
                {
                    try
                    {
                        DBDictionary xDict = (DBDictionary)tr.GetObject(ent.ExtensionDictionary, OpenMode.ForRead, false);
                        xRec = (Xrecord)tr.GetObject(xDict.GetAt(key), OpenMode.ForRead, false);
                        return xRec.Data;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                    return null;
            }
        }

        public static Boolean IsStair(Solid3d sol)
        {
            ResultBuffer rb = GetXrecord(sol.ObjectId, "StairCase");
            return (rb != null);
        }

        public static int GetStairNumber (Solid3d sol)
        {
            ResultBuffer rb = GetXrecord(sol.ObjectId, "StairCase");
            TypedValue[] result = rb.AsArray();
            return Convert.ToInt16 (result[0].Value);
        }

        public static double GetStairTread(Solid3d sol)
        {
            ResultBuffer rb = GetXrecord(sol.ObjectId, "StairCase");
            TypedValue[] result = rb.AsArray();
            return Convert.ToDouble(result[1].Value);
        }

    }
}


