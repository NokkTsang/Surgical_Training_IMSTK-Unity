using System;
using System.Collections.Generic;

namespace ImstkUnity
{
    public static class Constraints
    {
        /// <summary>
        /// Remove all constraints that exceeds the maxStrain. Should only be used for constraints
        /// that are between bodies. The strain is either calculated as current value divided by restlength
        /// or just the current value if the restlength == 0
        /// </summary>
        /// <param name="constraints">List of constraints to check</param>
        /// <param name="maxStrain">If the strain is higher than maxStrain the constraint will be removed</param>
        public static void RemoveStrainedConstraints(List<Imstk.PbdConstraint> constraints, double maxStrain)
        {
            var toRemove = new List<Imstk.PbdConstraint>();
            var constraintContainer = ImstkUnity.SimulationManager.pbdModel.getConstraints();
            foreach (var constraint in constraints)
            {
                var c = constraint.getConstraintC();
                var r = constraint.getRestValue();

                double strain = (Math.Abs(r) < 1e-7) ? c : c / r;
                
                if (strain > maxStrain)
                {
                    toRemove.Add(constraint);
                }
            }
            
            //Debug.Log($"{strainMax}, removing {toRemove.Count}");
            foreach (var constraint in toRemove)
            {
                constraintContainer.removeConstraint(constraint);
                constraints.Remove(constraint);
            }
        }
    }
}