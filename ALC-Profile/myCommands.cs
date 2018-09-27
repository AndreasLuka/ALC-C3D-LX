
// (C) Copyright 2018 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_Profile.MyCommands))]

namespace ALC_Profile
{

    public class MyCommands
    {
        // Illustrates creating a new profile without elevation data, then adding the elevation
        // via the entities collection

        public static void CreateProfileNoSurface(Alignment oAlignment)
        {
            CivilDocument civDoc = CivilApplication.ActiveDocument;

            using (Transaction ts = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {

                // use the same layer as the alignment
                ObjectId layerId = oAlignment.LayerId;

                // get the standard style and label set 
                // these calls will fail on templates without a style named "Stair"
                ObjectId styleId = civDoc.Styles.ProfileStyles["Stair"];
                ObjectId labelSetId = civDoc.Styles.LabelSetStyles.ProfileLabelSetStyles["Stair"];

                // create a new empty profile
                ObjectId oProfileId = Profile.CreateByLayout("My Profile", oAlignment.Id, layerId, styleId, labelSetId);

                // Now add the entities that define the profile.
                Profile oProfile = ts.GetObject(oProfileId, OpenMode.ForRead) as Profile;

                Point2d startPoint = new Point2d(oAlignment.StartingStation, 40);
                Point2d endPoint = new Point2d(oAlignment.EndingStation, -70);
                ProfileTangent oTangent1 = oProfile.Entities.AddFixedTangent(startPoint, endPoint);

                ts.Commit();
            }
        }

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


        [CommandMethod("ALC-CreateProfileNoSurface")]
        public static void ALCCreateProfileNoSurface()
        {
            // CivilDocument doc = CivilApplication.ActiveDocument;

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Alignment oAlignment; 
            using (Transaction ts = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                ObjectId alignID = SelectAlignment();

                oAlignment = ts.GetObject(alignID, OpenMode.ForRead) as Alignment;

                CreateProfileNoSurface(oAlignment);


                ts.Commit();
            }

        }
    }

}
