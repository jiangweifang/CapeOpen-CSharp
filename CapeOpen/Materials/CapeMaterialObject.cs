using System;
using System.Collections.Generic;
using System.Linq;

namespace CapeOpen
{
    /// <summary>
    /// Unified high-level abstraction over a CAPE-OPEN material object, providing
    /// convenient strongly-typed access to temperature, pressure, flow, compounds,
    /// and phase properties. Works with both Thermo 1.0 (<see cref="ICapeThermoMaterialObject"/>)
    /// and Thermo 1.1 (<see cref="ICapeThermoMaterial"/>) material objects.
    /// </summary>
    /// <remarks>
    /// This class wraps the connected material object (obtained from a port) and provides
    /// shortcut properties and methods so that Calculator implementations don't need to
    /// work directly with low-level COM interfaces. The underlying material object is
    /// accessed through the existing wrapper classes.
    /// </remarks>
    [System.Runtime.InteropServices.ComVisible(false)]
    public class CapeMaterialObject : IDisposable
    {
        private readonly object _connectedObject;
        private readonly ICapeThermoMaterialObject _thermo10;
        private readonly ICapeThermoMaterial _thermo11;
        private readonly ICapeThermoCompounds _thermoCompounds;
        private readonly ICapeThermoPhases _thermoPhases;
        private readonly ICapeThermoPropertyRoutine _thermoPropertyRoutine;
        private bool _disposed;

        /// <summary>
        /// Creates a <see cref="CapeMaterialObject"/> from a raw object obtained from 
        /// <see cref="UnitPort.connectedObject"/>. The object may implement 
        /// <see cref="ICapeThermoMaterialObject"/>, <see cref="ICapeThermoMaterial"/>, or both.
        /// </summary>
        /// <param name="connectedObject">The object connected to a material port.</param>
        public CapeMaterialObject(object connectedObject)
        {
            _connectedObject = connectedObject ?? throw new ArgumentNullException(nameof(connectedObject));
            _thermo10 = connectedObject as ICapeThermoMaterialObject;
            _thermo11 = connectedObject as ICapeThermoMaterial;
            _thermoCompounds = connectedObject as ICapeThermoCompounds;
            _thermoPhases = connectedObject as ICapeThermoPhases;
            _thermoPropertyRoutine = connectedObject as ICapeThermoPropertyRoutine;
        }

        /// <summary>
        /// Returns whether the underlying material supports Thermo 1.0.
        /// </summary>
        public bool SupportsThermo10 => _thermo10 != null;

        /// <summary>
        /// Returns whether the underlying material supports Thermo 1.1.
        /// </summary>
        public bool SupportsThermo11 => _thermo11 != null;

        /// <summary>
        /// Gets the raw underlying object for advanced or direct COM usage.
        /// </summary>
        public object RawObject => _connectedObject;

        #region Convenience Properties (Thermo 1.0)

        /// <summary>
        /// Gets the component identifiers in the material object.
        /// Requires Thermo 1.0 support.
        /// </summary>
        public string[] Compounds
        {
            get
            {
                if (_thermo10 != null)
                    return _thermo10.ComponentIds;
                if (_thermoCompounds != null)
                {
                    string[] compIds = null, formulae = null, names = null, casnos = null;
                    double[] boilTemps = null, molwts = null;
                    _thermoCompounds.GetCompoundList(ref compIds, ref formulae, ref names, ref boilTemps, ref molwts, ref casnos);
                    return compIds;
                }
                throw new CapeNoImplException("Material object does not expose component identifiers.");
            }
        }

        /// <summary>
        /// Gets the number of components.
        /// </summary>
        public int CompoundCount => Compounds.Length;

        /// <summary>
        /// Gets the molecular formulae of the compounds (Thermo 1.1 only).
        /// </summary>
        public string[] Formulae
        {
            get
            {
                if (_thermoCompounds != null)
                {
                    string[] compIds = null, formulae = null, names = null, casnos = null;
                    double[] boilTemps = null, molwts = null;
                    _thermoCompounds.GetCompoundList(ref compIds, ref formulae, ref names, ref boilTemps, ref molwts, ref casnos);
                    return formulae;
                }
                throw new CapeNoImplException("ICapeThermoCompounds not available.");
            }
        }

        /// <summary>
        /// Gets the overall temperature (K) from the material object.
        /// </summary>
        public double T
        {
            get
            {
                if (_thermo10 != null)
                {
                    double[] vals = _thermo10.GetProp("temperature", "Overall", null, null, null);
                    return vals[0];
                }
                if (_thermo11 != null)
                {
                    double temp = 0, pres = 0;
                    double[] comp = null;
                    _thermo11.GetOverallTPFraction(ref temp, ref pres, ref comp);
                    return temp;
                }
                throw new CapeNoImplException("Cannot get temperature.");
            }
        }

        /// <summary>
        /// Gets the overall pressure (Pa) from the material object.
        /// </summary>
        public double P
        {
            get
            {
                if (_thermo10 != null)
                {
                    double[] vals = _thermo10.GetProp("Pressure", "Overall", null, "Mixture", null);
                    return vals[0];
                }
                if (_thermo11 != null)
                {
                    double temp = 0, pres = 0;
                    double[] comp = null;
                    _thermo11.GetOverallTPFraction(ref temp, ref pres, ref comp);
                    return pres;
                }
                throw new CapeNoImplException("Cannot get pressure.");
            }
        }

        /// <summary>
        /// Gets the total molar flow (mol/s) from the material object.
        /// </summary>
        public double TotalFlow
        {
            get
            {
                if (_thermo10 != null)
                {
                    double[] flows = _thermo10.GetProp("totalFlow", "Overall", null, "Mixture", "mole");
                    return flows[0];
                }
                if (_thermo11 != null)
                {
                    double[] vals = null;
                    _thermo11.GetOverallProp("totalFlow", "mole", ref vals);
                    return vals[0];
                }
                throw new CapeNoImplException("Cannot get total flow.");
            }
        }

        /// <summary>
        /// Gets the component molar flows (mol/s) from the material object.
        /// </summary>
        public double[] ComponentFlows
        {
            get
            {
                if (_thermo10 != null)
                    return _thermo10.GetProp("flow", "Overall", null, null, "mole");
                if (_thermo11 != null)
                {
                    double[] vals = null;
                    _thermo11.GetOverallProp("flow", "mole", ref vals);
                    return vals;
                }
                throw new CapeNoImplException("Cannot get component flows.");
            }
        }

        #endregion

        #region Thermo 1.0 Methods

        /// <summary>
        /// Gets a property value using Thermo 1.0 semantics.
        /// </summary>
        public double[] GetProp(string property, string phase, string[] compIds, string calcType, string basis)
        {
            if (_thermo10 == null) throw new CapeNoImplException("Thermo 1.0 not supported.");
            return _thermo10.GetProp(property, phase, compIds, calcType, basis);
        }

        /// <summary>
        /// Sets a property value using Thermo 1.0 semantics.
        /// </summary>
        public void SetProp(string property, string phase, string[] compIds, string calcType, string basis, double[] values)
        {
            if (_thermo10 == null) throw new CapeNoImplException("Thermo 1.0 not supported.");
            _thermo10.SetProp(property, phase, compIds, calcType, basis, values);
        }

        /// <summary>
        /// Calculates properties using Thermo 1.0 semantics.
        /// </summary>
        public void CalcProp(string[] props, string[] phases, string calcType)
        {
            if (_thermo10 == null) throw new CapeNoImplException("Thermo 1.0 not supported.");
            _thermo10.CalcProp(props, phases, calcType);
        }

        /// <summary>
        /// Performs an equilibrium calculation using Thermo 1.0 semantics.
        /// </summary>
        public void CalcEquilibrium(string flashType, string[] props)
        {
            if (_thermo10 == null) throw new CapeNoImplException("Thermo 1.0 not supported.");
            _thermo10.CalcEquilibrium(flashType, props);
        }

        /// <summary>
        /// Duplicates the material object (Thermo 1.0).
        /// </summary>
        public CapeMaterialObject Duplicate()
        {
            if (_thermo10 != null)
            {
                ICapeThermoMaterialObject dup = _thermo10.Duplicate();
                return new CapeMaterialObject(dup);
            }
            if (_thermo11 != null)
            {
                ICapeThermoMaterial created = _thermo11.CreateMaterial();
                created.CopyFromMaterial(_thermo11);
                return new CapeMaterialObject(created);
            }
            throw new CapeNoImplException("Cannot duplicate material.");
        }

        #endregion

        #region Thermo 1.1 Methods

        /// <summary>
        /// Gets an overall property using Thermo 1.1 semantics.
        /// </summary>
        public double[] GetOverallProp(string property, string basis)
        {
            if (_thermo11 == null) throw new CapeNoImplException("Thermo 1.1 not supported.");
            double[] results = null;
            _thermo11.GetOverallProp(property, basis, ref results);
            return results;
        }

        /// <summary>
        /// Sets an overall property using Thermo 1.1 semantics.
        /// </summary>
        public void SetOverallProp(string property, string basis, double[] values)
        {
            if (_thermo11 == null) throw new CapeNoImplException("Thermo 1.1 not supported.");
            _thermo11.SetOverallProp(property, basis, values);
        }

        /// <summary>
        /// Gets a single-phase property using Thermo 1.1 semantics.
        /// </summary>
        public double[] GetSinglePhaseProp(string property, string phaseLabel, string basis)
        {
            if (_thermo11 == null) throw new CapeNoImplException("Thermo 1.1 not supported.");
            double[] results = null;
            _thermo11.GetSinglePhaseProp(property, phaseLabel, basis, ref results);
            return results;
        }

        /// <summary>
        /// Sets a single-phase property using Thermo 1.1 semantics.
        /// </summary>
        public void SetSinglePhaseProp(string property, string phaseLabel, string basis, double[] values)
        {
            if (_thermo11 == null) throw new CapeNoImplException("Thermo 1.1 not supported.");
            _thermo11.SetSinglePhaseProp(property, phaseLabel, basis, values);
        }

        /// <summary>
        /// Gets a two-phase property using Thermo 1.1 semantics.
        /// </summary>
        public double[] GetTwoPhaseProp(string property, string[] phaseLabels, string basis)
        {
            if (_thermo11 == null) throw new CapeNoImplException("Thermo 1.1 not supported.");
            double[] results = null;
            _thermo11.GetTwoPhaseProp(property, phaseLabels, basis, ref results);
            return results;
        }

        /// <summary>
        /// Sets a two-phase property using Thermo 1.1 semantics.
        /// </summary>
        public void SetTwoPhaseProp(string property, string[] phaseLabels, string basis, double[] values)
        {
            if (_thermo11 == null) throw new CapeNoImplException("Thermo 1.1 not supported.");
            _thermo11.SetTwoPhaseProp(property, phaseLabels, basis, values);
        }

        /// <summary>
        /// Gets the overall temperature, pressure, and composition (Thermo 1.1).
        /// </summary>
        public (double Temperature, double Pressure, double[] Composition) GetOverallTPFraction()
        {
            if (_thermo11 == null) throw new CapeNoImplException("Thermo 1.1 not supported.");
            double temp = 0, pres = 0;
            double[] comp = null;
            _thermo11.GetOverallTPFraction(ref temp, ref pres, ref comp);
            return (temp, pres, comp);
        }

        /// <summary>
        /// Gets the present phases and their statuses (Thermo 1.1).
        /// </summary>
        public (string[] PhaseLabels, CapePhaseStatus[] PhaseStatus) GetPresentPhases()
        {
            if (_thermo11 == null) throw new CapeNoImplException("Thermo 1.1 not supported.");
            string[] labels = null;
            CapePhaseStatus[] statuses = null;
            _thermo11.GetPresentPhases(ref labels, ref statuses);
            return (labels, statuses);
        }

        /// <summary>
        /// Sets the present phases and their statuses (Thermo 1.1).
        /// </summary>
        public void SetPresentPhases(string[] phaseLabels, CapePhaseStatus[] phaseStatus)
        {
            if (_thermo11 == null) throw new CapeNoImplException("Thermo 1.1 not supported.");
            _thermo11.SetPresentPhases(phaseLabels, phaseStatus);
        }

        /// <summary>
        /// Clears all stored property values (Thermo 1.1).
        /// </summary>
        public void ClearAllProps()
        {
            if (_thermo11 == null) throw new CapeNoImplException("Thermo 1.1 not supported.");
            _thermo11.ClearAllProps();
        }

        #endregion

        #region Property Availability (Thermo 1.1)

        /// <summary>
        /// Gets the list of available single-phase properties (Thermo 1.1).
        /// Returns null if ICapeThermoPropertyRoutine is not supported.
        /// </summary>
        public string[] AvailableSinglePhaseProps => _thermoPropertyRoutine?.GetSinglePhasePropList();

        /// <summary>
        /// Gets the list of available two-phase properties (Thermo 1.1).
        /// Returns null if ICapeThermoPropertyRoutine is not supported.
        /// </summary>
        public string[] AvailableTwoPhaseProps => _thermoPropertyRoutine?.GetTwoPhasePropList();

        /// <summary>
        /// Gets the phase list (Thermo 1.1).
        /// </summary>
        public string[] GetPhaseList()
        {
            if (_thermoPhases == null && _thermo10 != null)
                return _thermo10.PhaseIds;
            if (_thermoPhases != null)
            {
                string[] labels = null, stateOfAgg = null, keyCompound = null;
                _thermoPhases.GetPhaseList(ref labels, ref stateOfAgg, ref keyCompound);
                return labels;
            }
            throw new CapeNoImplException("Cannot get phase list.");
        }

        #endregion

        #region Compound Constants (Thermo 1.1)

        /// <summary>
        /// Gets compound constant property values for all compounds (Thermo 1.1).
        /// </summary>
        public void GetCompoundConstant(string[] props, string[] compIds, ref double[] propVals)
        {
            if (_thermoCompounds == null) throw new CapeNoImplException("ICapeThermoCompounds not supported.");
            // The CAPE-OPEN GetCompoundConstant returns objects, so use GetTDependentProperty etc. for doubles
            // For constant properties, use the ICapeThermoMaterialObject interface if available
            if (_thermo10 != null)
            {
                object[] results = _thermo10.GetComponentConstant(props, compIds);
                propVals = results.Select(r => Convert.ToDouble(r)).ToArray();
                return;
            }
            throw new CapeNoImplException("GetCompoundConstant requires Thermo 1.0 support for typed access.");
        }

        /// <summary>
        /// Gets temperature-dependent property values (Thermo 1.1).
        /// </summary>
        public double[] GetTDependentProperty(string[] props, double temperature, string[] compIds)
        {
            if (_thermoCompounds == null) throw new CapeNoImplException("ICapeThermoCompounds not supported.");
            double[] results = null;
            _thermoCompounds.GetTDependentProperty(props, temperature, compIds, ref results);
            return results;
        }

        /// <summary>
        /// Gets pressure-dependent property values (Thermo 1.1).
        /// </summary>
        public double[] GetPDependentProperty(string[] props, double pressure, string[] compIds)
        {
            if (_thermoCompounds == null) throw new CapeNoImplException("ICapeThermoCompounds not supported.");
            double[] results = null;
            _thermoCompounds.GetPDependentProperty(props, pressure, compIds, ref results);
            return results;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Searches for a property name in the available property list that contains the given
        /// search term (case-insensitive). Returns the default name if not found.
        /// </summary>
        /// <param name="availableProps">Array of available property names to search.</param>
        /// <param name="searchTerm">Partial name to search for (case-insensitive).</param>
        /// <param name="defaultName">Value to return if not found. If null, returns searchTerm.</param>
        public static string SearchPropName(string[] availableProps, string searchTerm, string defaultName = null)
        {
            if (availableProps != null)
            {
                string found = availableProps.FirstOrDefault(x => x.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
                if (found != null) return found;
            }
            return defaultName ?? searchTerm;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            try
            {
                return $"T:{T} P:{P} TotalFlow:{TotalFlow}";
            }
            catch
            {
                return base.ToString();
            }
        }

        /// <summary>
        /// Releases the underlying COM material object if applicable.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_connectedObject != null && _connectedObject.GetType().IsCOMObject)
                {
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(_connectedObject);
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
