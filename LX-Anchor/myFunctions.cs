// (C) Copyright 2018 by  
// Lu An Jie (Andreas Luka)


using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using System;
using Autodesk.Civil;

using System.Collections.Generic;


namespace LX_Anchor
{
    class MyFunctions
    {
        public static ObjectId SelectAlignment()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ObjectId result = ObjectId.Null;

            PromptEntityOptions entOpts = new PromptEntityOptions("\nSelect alignment: ");
            entOpts.SetRejectMessage("...not an Alignment, try again!:");
            entOpts.AddAllowedClass(typeof(Alignment), true);

            PromptEntityResult entRes = ed.GetEntity(entOpts);

            if (entRes.Status == PromptStatus.OK) result = entRes.ObjectId;

            return result;
        }

        public static ObjectId SelectEntity(HashSet<Type> validTypes)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ObjectId result = ObjectId.Null;

            PromptEntityOptions entOpts = new PromptEntityOptions("\nSelect valid entity: ");
            entOpts.SetRejectMessage("...not an valid entity, try again!:");
            foreach (Type valid in validTypes) entOpts.AddAllowedClass(valid, true);

            PromptEntityResult entRes = ed.GetEntity(entOpts);

            if (entRes.Status == PromptStatus.OK) result = entRes.ObjectId;

            return result;
        }



        public static void MoveCP(CogoPoint cp, double easting, double northing)
        {
            cp.Easting = easting;
            cp.Northing = northing;
        }


        // returns true and if true the point perpendicular on the alignment
        public static bool InRangePerpendicularPoint(Alignment align, Point2d ptOffAlignment, ref Point2d ptOnAlignment, ref double station)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            double easting = ptOffAlignment.X;
            double northing = ptOffAlignment.Y;
            double offset = Double.NaN;
            ptOnAlignment = new Point2d(Double.NaN, Double.NaN);

            try
            {
                align.StationOffset(easting, northing, ref station, ref offset);
                align.PointLocation(station, 0, ref easting, ref northing);
                ptOnAlignment = new Point2d(easting, northing);
                return true;
            }
            catch (PointNotOnEntityException)  // The point is outside of the alignment boundaries
            {
                return false;
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                ed.WriteMessage(ex.Message);
                return false;
            }
        }


        // returns true and if true the point perpendicular on the alignment
        public static bool InRangePoint(Alignment align, ref Point2d pt, double station, double offset)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            double easting = Double.NaN;
            double northing = Double.NaN;

            try
            {
                align.PointLocation(station, offset, ref easting, ref northing);
                pt = new Point2d(easting, northing);
                return true;
            }
            catch (PointNotOnEntityException)  // The point is outside of the alignment boundaries
            {
                return false;
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                ed.WriteMessage(ex.Message);
                return false;
            }
        }

        public static void WriteXRecord(ObjectId id, string key, ResultBuffer rb)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Autodesk.AutoCAD.DatabaseServices.Entity ent = tr.GetObject(id, OpenMode.ForWrite) as Autodesk.AutoCAD.DatabaseServices.Entity;
                if (ent != null)
                {
                    if (ent.ExtensionDictionary == ObjectId.Null)
                    {
                        ent.CreateExtensionDictionary();
                    }
                    DBDictionary xDict = (DBDictionary)tr.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite);
                    Xrecord xRec = new Xrecord
                    {
                        Data = rb
                    };
                    xDict.SetAt(key, xRec);
                    tr.AddNewlyCreatedDBObject(xRec, true);
                }
                tr.Commit();
            }
        }


        public static ResultBuffer ReadXRecord(ObjectId id, string key)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Xrecord xRec = new Xrecord();
                    Autodesk.AutoCAD.DatabaseServices.Entity ent = (Autodesk.AutoCAD.DatabaseServices.Entity)tr.GetObject(id, OpenMode.ForRead, false);
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
                    else return null;
                }
                catch 
                {
                    return null;
                }
            }

        }

        
        public static void DeleteXRecord(ObjectId id,  string key)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Xrecord xRec = new Xrecord();
                Autodesk.AutoCAD.DatabaseServices.Entity ent = tr.GetObject(id, OpenMode.ForRead, false) as Autodesk.AutoCAD.DatabaseServices.Entity;
                if (ent != null)
                {
                    try
                    {
                        DBDictionary xDict = (DBDictionary)tr.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite);
                        if ((xDict != null) && (xDict.Contains(key))) xDict.Remove(key);

                        tr.Commit();
                    }
                    catch { }
                }
            }
        }
    }
}
