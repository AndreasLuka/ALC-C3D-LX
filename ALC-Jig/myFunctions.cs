using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;

namespace ALC_Jig
{
        public class BlockMovingRotating : EntityJig
        {
            public int mCurJigFactorNumber = 1;

            private Point3d mPosition;    // Factor #1
            private double mRotation;    // Factor #2

             public BlockMovingRotating(Entity ent)
                : base(ent)
            {
            }

            protected override bool Update()
            {
                switch (mCurJigFactorNumber)
                {
                    case 1:
                        (Entity as BlockReference).Position = mPosition;
                        break;
                    case 2:
                        (Entity as BlockReference).Rotation = mRotation;
                        break;
                    default:
                        return false;
                }

                return true;
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                switch (mCurJigFactorNumber)
                {
                    case 1:
                        JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\nBlock position:");
                        PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
                        if (prResult1.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

                        if (prResult1.Value.Equals(mPosition))
                        {
                            return SamplerStatus.NoChange;
                        }
                        else
                        {
                            mPosition = prResult1.Value;
                            return SamplerStatus.OK;
                        }
                    case 2:
                        JigPromptAngleOptions prOptions2 = new JigPromptAngleOptions("\nBlock rotation angle:");
                        prOptions2.BasePoint = (Entity as BlockReference).Position;
                        prOptions2.UseBasePoint = true;
                        PromptDoubleResult prResult2 = prompts.AcquireAngle(prOptions2);
                        if (prResult2.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

                        if (prResult2.Value.Equals(mRotation))
                        {
                            return SamplerStatus.NoChange;
                        }
                        else
                        {
                            mRotation = prResult2.Value;
                            return SamplerStatus.OK;
                        }
                    default:
                        break;
                }

                return SamplerStatus.OK;
            }


            public static bool Jig(BlockReference ent)
            {
                try
                {
                    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                    BlockMovingRotating jigger = new BlockMovingRotating(ent);
                    PromptResult pr;
                    do
                    {
                        pr = ed.Drag(jigger);
                    }
                    while (pr.Status != PromptStatus.Cancel && pr.Status != PromptStatus.Error && pr.Status != PromptStatus.Keyword && jigger.mCurJigFactorNumber++ <= 2);

                    return pr.Status == PromptStatus.OK;
                }
                catch
                {
                    return false;
                }
            }
        }
}