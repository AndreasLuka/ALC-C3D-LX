using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using Autodesk.AutoCAD.Windows;

namespace ALC_Jig
{
    // Class that Inherits from EntityJig. 
    class stairJig : EntityJig
    {   
        // Inputs, the startpoint and the rotation. 
        private Point3d startPnt;
        private double rotation;

        // Keep track of the input number. (used to determine which value we are getting). 
        private int currentInputValue;

        // Default constructor. Pass in an Entity variable named ent, Derive from the base class and also pass in the ent passed into the constructor. 
        public stairJig(Entity ent) : base(ent)
        {}

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {

            switch (currentInputValue)
            {
                // 0 (zero) for the case. (getting startpoint) 
                case 0:

                    // Test to see if the cursor has moved during the jig to avoid flickering
                    Point3d oldPnt = startPnt;

                    // AcquirePoint method of the JigPrompts oject passed into the Sampler function. 
                    PromptPointResult jigPromptResult = prompts.AcquirePoint("Pick start point : ");

                    if (jigPromptResult.Status == PromptStatus.OK)
                    {

                        startPnt = jigPromptResult.Value;

                        // Check to see if the cursor has moved. 
                        if ((oldPnt.DistanceTo(startPnt) < 0.001))
                        {
                            return SamplerStatus.NoChange;
                        }
                    }
                    return SamplerStatus.OK;

                // 1 for the case. (getting rotation) 
                case 1:
                    double oldRotation = rotation;
                    JigPromptAngleOptions jigPromptAngleOpts = new JigPromptAngleOptions("Pick direction : ");

                    // getting startpoint
                    jigPromptAngleOpts.UseBasePoint = true;
                    jigPromptAngleOpts.BasePoint = startPnt;

                    // AquireAngle method of yje JigPrompts object passed into the Sampler function
                    PromptDoubleResult jigPromptDblResult = prompts.AcquireAngle(jigPromptAngleOpts);

                    if ((jigPromptDblResult.Status == PromptStatus.OK))
                    {
                        rotation = jigPromptDblResult.Value;
                        // 26. Check to see if the cursor has moved. 
                        if ((System.Math.Abs(oldRotation - rotation) < 0.001))
                        {
                             return SamplerStatus.NoChange;
                        }
                    }
                    return SamplerStatus.OK;
            }
            // if switch break
            return SamplerStatus.NoChange;
        }


        // Override the Update function. 
        protected override bool Update()
        {

            // For every input, we need to update the entity 
            switch (currentInputValue)
            {
                // Updating startpoint  
                case 0:

                    ((Circle)this.Entity).Center = startPnt;
                    break;

                // Updating rotation 
                case 1:
                    ((Circle)this.Entity).Radius = rotation;
                    break;

            }
            return true;
        }


    }
}