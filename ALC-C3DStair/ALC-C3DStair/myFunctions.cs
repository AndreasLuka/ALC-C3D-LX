using System;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

using Autodesk.Aec.PropertyData.DatabaseServices;


namespace ALC_C3DStair
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

        public static bool IsPlanView()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            return ed.GetCurrentView().ViewDirection.IsParallelTo(Matrix3d.Identity.CoordinateSystem3d.Zaxis);
        }

        public static bool GetStairPropertiesFromAlignment(Alignment align, ref double tread, ref double riser, ref double landing, ref double width, ref int steps, ref bool reverse, ref double elevation)
        {
            bool result = false;

            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                ObjectIdCollection setIds = PropertyDataServices.GetPropertySets(align);
                if (setIds.Count > 0)
                {
                    foreach (ObjectId id in setIds)
                    {
                        PropertySet pset = (PropertySet)id.GetObject(OpenMode.ForRead);
                        if (pset.PropertySetDefinitionName == "ALCStairAlignment")
                        {
                            // Get the ObjectID of the property set definition by name
                            // Get the value of the property definition
                            tread = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("tread")));
                            riser = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("riser")));
                            landing = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("landing")));
                            width = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("width")));
                            steps = Convert.ToInt16(pset.GetAt(pset.PropertyNameToId("steps")));
                            reverse = Convert.ToBoolean(pset.GetAt(pset.PropertyNameToId("reverse")));
                            elevation = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("elevation")));

                            result = true;
                            break;
                        }
                    }
                }
                tr.Commit();
            }
            return result;
        }

        public static bool SetStairPropertiesToAlignment(Alignment align, double tread, double riser,  double landing, double width, int steps, bool reverse, double elevation)
        {
            bool result = false;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            if (!IsStairPropertySetOnAlignment(align))
            {
                AddStairPropertySetToAlignment(align);
                ed.WriteMessage("\n Property set for {0} created", align.Name);
            }

            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                ObjectIdCollection setIds = PropertyDataServices.GetPropertySets(align);
                if (setIds.Count > 0)
                {
                    foreach (ObjectId id in setIds)
                    {
                        PropertySet pset = (PropertySet)id.GetObject(OpenMode.ForWrite);
                        if (pset.PropertySetDefinitionName == "ALCStairAlignment" && pset.IsWriteEnabled)
                        {
                            // Get the ObjectID of the property set definition by name
                            // Get the value of the property definition
                            pset.SetAt(pset.PropertyNameToId("tread"), tread);
                            pset.SetAt(pset.PropertyNameToId("riser"), riser);
                            pset.SetAt(pset.PropertyNameToId("landing"), landing);
                            pset.SetAt(pset.PropertyNameToId("width"), width);
                            pset.SetAt(pset.PropertyNameToId("steps"), steps);
                            pset.SetAt(pset.PropertyNameToId("reverse"), reverse);
                            pset.SetAt(pset.PropertyNameToId("elevation"), elevation);
                            result = true;
                            break;
                        }
                    }
                }
                tr.Commit();
            }
            return result;
        }

        public static bool AddStairPropertySetToAlignment(Alignment align)
        {
            bool result = false;

            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                try
                {
                    Autodesk.AutoCAD.DatabaseServices.DBObject dbobj = tr.GetObject(align.Id, OpenMode.ForWrite);
                    PropertyDataServices.AddPropertySet(dbobj, GetPropertySetDefinitionIdByName("ALCStairAlignment"));
                    result = true;
                }
                catch
                {
                    result = false;
                }
                tr.Commit();
                return result;
            }
        }

        public static bool IsStairPropertySetOnAlignment(Alignment align)
        {
            bool result = false;

            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                ObjectIdCollection setIds = PropertyDataServices.GetPropertySets(align);
                if (setIds.Count > 0)
                {
                    foreach (ObjectId id in setIds)
                    {
                        PropertySet pset = (PropertySet)id.GetObject(OpenMode.ForRead);
                        if (pset.PropertySetDefinitionName == "ALCStairAlignment")
                        {
                            result = true;
                            break;
                        }
                    }
                }
                tr.Commit();
            }
            return result;
        }

        public static ObjectId GetPropertySetDefinitionIdByName(string psdName)
        {
            ObjectId result = ObjectId.Null;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DictionaryPropertySetDefinitions psdDict = new DictionaryPropertySetDefinitions(db);
                if (psdDict.Has(psdName, tr)) result = psdDict.GetAt(psdName);
                tr.Commit();
            }
            return result;
        }

        public static void CreateProfileNoSurface(Alignment oAlignment)
        {
            CivilDocument civDoc = CivilApplication.ActiveDocument;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


            int steps = 10;
            double width = 2;
            double tread = 0.32;
            double landing = 0;
            double riser = 0.16;
            double elevation = 0;
            bool reverse = false;

            double heightEnd;
            Point2d startPoint, endPoint;

            using (Transaction ts = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {

                // use the same layer as the alignment
                ObjectId layerId = oAlignment.LayerId;

                // get stair values
                GetStairPropertiesFromAlignment(oAlignment, ref tread, ref riser, ref landing, ref width, ref steps, ref reverse, ref elevation);

                // get the standard style and label set 
                // these calls will fail on templates without a style named "ALCStair"

                ObjectId styleId = civDoc.Styles.ProfileStyles["ALCStair"];
                ObjectId labelSetId = civDoc.Styles.LabelSetStyles.ProfileLabelSetStyles["ALCStair"];

                // create a new empty profile
                ObjectId oProfileId = Profile.CreateByLayout("My Profile", oAlignment.Id, layerId, styleId, labelSetId);

                // Now add the entities that define the profile.
                Profile oProfile = ts.GetObject(oProfileId, OpenMode.ForRead) as Profile;

                heightEnd = elevation + steps * riser;
                ed.WriteMessage("\n Elevation: {0}, Height: {1}, Endheight: {2}", elevation, steps * riser, heightEnd);
                if (reverse)
                {
                    startPoint = new Point2d(oAlignment.StartingStation, heightEnd);
                    endPoint = new Point2d(oAlignment.EndingStation, elevation);
                }
                else
                {
                    startPoint = new Point2d(oAlignment.StartingStation, elevation);
                    endPoint = new Point2d(oAlignment.EndingStation, heightEnd); 
                }

                ProfileTangent oTangent1 = oProfile.Entities.AddFixedTangent(startPoint, endPoint);

                ts.Commit();
            }
        }

    }
}
