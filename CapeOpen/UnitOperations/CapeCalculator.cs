using System;

namespace CapeOpen
{
    /// <summary>
    /// Abstract base class that separates calculation logic from the unit operation.
    /// Inherit from this class and implement the three calculation phases.
    /// A unit operation can accept different Calculator implementations, allowing
    /// users to choose how the calculation is performed.
    /// </summary>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public abstract class CapeCalculator
    {
        /// <summary>
        /// Reference to the owning unit operation, set automatically when
        /// the calculator is assigned to <see cref="CapeUnitBase.Calculator"/>.
        /// </summary>
        public CapeUnitBase UnitOperation { get; internal set; }

        /// <summary>
        /// Called before the main calculation. Typical tasks include:
        /// obtaining material objects from ports (always Duplicate input materials),
        /// reading parameter values, and preparing intermediate data.
        /// </summary>
        public abstract void BeforeCalculate();

        /// <summary>
        /// Performs the actual calculation (material/energy balance, flash, etc.).
        /// </summary>
        public abstract void Calculate();

        /// <summary>
        /// Called after the main calculation. Typical tasks include:
        /// writing results to output ports, setting result parameters,
        /// and releasing duplicated material objects.
        /// </summary>
        public abstract void OutputResult();
    }
}
