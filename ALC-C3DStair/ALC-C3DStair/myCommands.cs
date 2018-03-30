using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;


[assembly: CommandClass(typeof(ALC_C3DStair.MyCommands))]

namespace ALC_C3DStair
{

    public class MyCommands
    {
        // Modal Command with pickfirst selection
        [CommandMethod("ALCCreateStairOnAlignment", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ALCStairOnAlignment()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CivilDocument civdoc = CivilApplication.ActiveDocument;

            double tread = 0.32;
            double riser = 0.16;
            double landing = 0;
            double width = 2;
            int steps = 5;
            bool reverse = false;
            double elevation = 5;

            ObjectId alignId = MyFunctions.SelectAlignment();

            if (alignId == ObjectId.Null) return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // get the alignment from alignId
                    Alignment align = (Alignment)alignId.GetObject(OpenMode.ForRead);

                    // create the jig
                    StationJig jig = new StationJig(align);
                    PromptResult pr;
                    do
                    {
                        pr = ed.Drag(jig);
                    }
                    while (pr.Status != PromptStatus.Cancel && pr.Status != PromptStatus.Error && jig.jigStatus++ <= 1);


                    // create centerline alignment and change offset to 0 presuming style "Stair"exist
                    string name = Alignment.GetNextUniqueName("Stair-<[Next Counter(CP)]>");

                    ObjectId styleId = civdoc.Styles.AlignmentStyles["Stair"];
                    if (jig.stationStart > jig.stationEnd)
                    {
                        // swap
                        double temp = jig.stationStart;
                        jig.stationStart = jig.stationEnd;
                        jig.stationEnd = temp;
                        reverse = true;
                    }

                    ObjectId ctLineId = Alignment.CreateOffsetAlignment(name, alignId, 1, styleId, jig.stationStart, jig.stationEnd);
                    Alignment stairAlignment = ctLineId.GetObject(OpenMode.ForWrite) as Alignment;

                    OffsetAlignmentInfo offsetInfo = stairAlignment.OffsetAlignmentInfo;
                    offsetInfo.NominalOffset = 0;
                    offsetInfo.LockToStartStation = false;
                    offsetInfo.LockToEndStation = false;

                    // Create two new alignment with an offset of width/2 and lock to start/end to parent alignment start/end:
                    // styleId = civdoc.Styles.AlignmentStyles["Offsets"];
                    // ObjectId leftLineId = Alignment.CreateOffsetAlignment(String.Format("Stair Offset {0} Left", width / 2), ctLineId, -width / 2, styleId);
                    // Alignment leftLine = leftLineId.GetObject(OpenMode.ForWrite) as Alignment;
                    // OffsetAlignmentInfo leftOffsetInfo = leftLine.OffsetAlignmentInfo;
                    // leftOffsetInfo.LockToStartStation = true;
                    // leftOffsetInfo.LockToEndStation = true;

                    // ObjectId rightLineId = Alignment.CreateOffsetAlignment(String.Format("Stair Offset {0} Right", width / 2), ctLineId, width / 2, styleId);
                    // Alignment rightLine = rightLineId.GetObject(OpenMode.ForWrite) as Alignment;
                    // OffsetAlignmentInfo rightOffsetInfo = rightLine.OffsetAlignmentInfo;
                    // rightOffsetInfo.LockToStartStation = true;
                    // rightOffsetInfo.LockToEndStation = true;

                    bool result = MyFunctions.SetPropertiesToAlignment(stairAlignment, tread, riser, landing, width, steps, reverse, elevation);

                    MyFunctions.CreateProfileNoSurface(stairAlignment);

                    // use the same layer as the alignment
                    // ObjectId layerId = stairAlignment.LayerId;
                    // get the standard style and label set 
                    // these calls will fail on templates without a style named "Stair"
                    // ObjectId profileStyleId = civdoc.Styles.ProfileStyles["Stair"];
                    // ObjectId labelSetId = civdoc.Styles.LabelSetStyles.ProfileLabelSetStyles["Stair"];

                    // ObjectId oProfileId = Profile.CreateByLayout("StairLine", align.Id, layerId, profileStyleId, labelSetId);

                    // Now add the entities that define the profile.


                    tr.Commit();
                }

                catch
                {
                    // add exception handling
                }

            }
        }

 
        [CommandMethod("ALCCreateStairProfile", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ALCCreateProfileNoSurface()
        {
            // CivilDocument doc = CivilApplication.ActiveDocument;

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Alignment oAlignment;
            using (Transaction ts = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                ObjectId alignId = MyFunctions.SelectAlignment();

                if (alignId == ObjectId.Null) return;

                oAlignment = ts.GetObject(alignId, OpenMode.ForRead) as Alignment;
                MyFunctions.CreateProfileNoSurface(oAlignment);
                ts.Commit();
            }

        }

    }

}
