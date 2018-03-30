using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;

namespace ALC_SolidStair
{
    class StairJig : DrawJig
    {

        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


        private Solid3d stair;
        public Point3d insertPoint, secondPoint;
        public Double insertAngle;

        public int steps;
        public Double tread;
        public Double riser;
        public Double landing;
        public Double width;

        public Matrix3d Displacement { get; private set; }
        public Matrix3d Rotation { get; private set; }

        public int jigStatus = 0;  //0  = Insert :: 1 = Rotation
        public Boolean jigUpdate = false;

        public StairJig(int steps, double riser, double tread, double landing, double width)
        {
            this.stair = myFunctions.CreateStair3D(steps, riser, tread, landing, width);
            this.steps = steps;
            this.tread = tread;
            this.riser = riser;
            this.landing = landing;
            this.width = width;
        }


        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (jigStatus)
            {
                case 0:
                    JigPromptPointOptions opts1 = new JigPromptPointOptions("\nSpecify the start point: ");
                    // Define the valid keywords and allow Enter
                    opts1.Keywords.Add("Steps");
                    opts1.Keywords.Add("Tread");
                    opts1.Keywords.Add("Riser");
                    opts1.Keywords.Add("Landing");
                    opts1.Keywords.Add("Width");

                    opts1.UserInputControls = UserInputControls.NullResponseAccepted;

                    PromptPointResult ppr1 = prompts.AcquirePoint(opts1);

                    if (ppr1.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

                    if (ppr1.Value.DistanceTo(insertPoint) < Tolerance.Global.EqualPoint)
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        insertPoint = ppr1.Value;
                        Vector3d disp = Point3d.Origin.GetVectorTo(insertPoint);
                        Displacement = Matrix3d.Displacement(disp);
                        // jigStatus++;
                        return SamplerStatus.OK;
                    }

                case 1:
                    JigPromptPointOptions opts2 = new JigPromptPointOptions("\nSpecify second point:");
                    opts2.Keywords.Add("Steps");
                    opts2.Keywords.Add("Tread");
                    opts2.Keywords.Add("Riser");
                    opts2.Keywords.Add("Landing");
                    opts2.Keywords.Add("Width");

                    opts2.BasePoint = insertPoint;
                    opts2.UseBasePoint = true;
                    opts2.Cursor = CursorType.RubberBand;
                    PromptPointResult ppr2 = prompts.AcquirePoint(opts2);

                    if (ppr2.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

                    if (ppr2.Value.DistanceTo(secondPoint) < Tolerance.Global.EqualPoint)
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        secondPoint = ppr2.Value;
                        Point2d start = new Point2d(insertPoint.X, insertPoint.Y);
                        Point2d end = new Point2d(secondPoint.X, secondPoint.Y);
                        insertAngle = start.GetVectorTo(end).Angle;
                        Rotation = Matrix3d.Rotation(insertAngle, Vector3d.ZAxis, insertPoint);
                        return SamplerStatus.OK;
                    }
                default: break;
            }
            return SamplerStatus.OK;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {

            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                if (jigUpdate)
                {
                    stair = myFunctions.CreateStair3D(steps, riser, tread, landing, width);
                }
                switch (jigStatus)
                {
                    case 0:
                        geo.PushModelTransform(Displacement);
                        geo.Draw(stair);
                        geo.PopModelTransform();
                        break;
                    case 1:
                        geo.PushModelTransform(Displacement.PreMultiplyBy(Rotation));
                        geo.Draw(stair);
                        geo.PopModelTransform();
                        break; ;
                    default: return false;
                }
            }
            return true;
        }
    }

}