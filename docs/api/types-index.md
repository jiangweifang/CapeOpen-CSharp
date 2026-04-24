# 类型索引

按字母顺序列出 `CapeOpen` 命名空间下所有公开类型。

| 名称 | 类别 | 简介 |
| --- | --- | --- |
| `BaseUnitEditor` | Class | Base class for a unit operation editor. |
| `CapeAboutAttribute` | Class | Provides text about information for the registration of a CAPE-OPEN object. |
| `CapeBadArgumentException` | Class | An argument value of the operation is not correct. |
| `CapeBadCOParameter` | Class | A parameter, which is an object from the Parameter Common Interface, has an invalid status. |
| `CapeBadInvOrderException` | Class | The necessary pre-requisite operation has not been called prior to the operation request. |
| `CapeBoundariesException` | Class | This is an abstract class that allows derived classes to provide information about error that result from values that are outside of their bounds. It can be raised to indicate that the value of either a method argument o… |
| `CapeCalculationCode` | Enumeration | A flag that indicates the desired calculations for the ICapeThermoPropertyRoutine.CalcAndGetLnPhi method. |
| `CapeCompoundType` | Enumeration | The type of compound for use in petroleum fractions |
| `CapeComputationException` | Class | The base class of the errors hierarchy related to calculations. |
| `CapeDataException` | Class | The base class of the errors hierarchy related to any data. |
| `CapeDescriptionAttribute` | Class | Provides a text description for the registration of a CAPE-OPEN object. |
| `CapeErrorInterfaceHR` | Enumeration | Enumeration of the various HRESULT values for the CAPE-OPEN error handling interfaces. |
| `CapeFailedInitialisationException` | Class | This exception is thrown when necessary initialisation has not been performed or has failed. |
| `CapeHelpURLAttribute` | Class | Provides a help URL for the registration of a CAPE-OPEN object. |
| `CapeHessianInfoNotAvailableException` | Class | Exception thrown when the Hessian for the MINLP problem is not available. |
| `CapeIdentification` | Class | Provides methods to identify and describe a CAPE-OPEN component. |
| `CapeIllegalAccessException` | Class | The access to something within the persistence system is not authorised. |
| `CapeImplementationException` | Class | The base class of the errors hierarchy related to the current implementation. |
| `CapeInvalidArgumentException` | Class | An invalid argument value was passed. For instance the passed name of the phase does not belong to the CO Phase List. |
| `CapeInvalidOperationException` | Class | This operation is not valid in the current context. |
| `CapeLicenceErrorException` | Class | An operation can not be completed because the licence agreement is not respected. |
| `CapeLimitedImplException` | Class | The limit of the implementation has been violated. |
| `CapeNameAttribute` | Class | Provides a text name for the registration of a CAPE-OPEN object. |
| `CapeNoImplException` | Class | An exception class that indicates that the requested operation has not been implemented by the current object. |
| `CapeNoMemoryException` | Class | An exception class that indicates that the memory required for this operation is not available. |
| `CapeOutOfBoundsException` | Class | An argument value is outside of the bounds.. |
| `CapeOutOfResourcesException` | Class | An exception class that indicates that the resources required by this operation are not available. |
| `CapeParamMode` | Enumeration | Modes of parameters. |
| `CapeParamType` | Enumeration | Gets the type of the parameter for which this is a specification: |
| `CapePersistenceException` | Class | An exception class that indicates that the a persistence-related error has occurred. |
| `CapePersistenceNotFoundException` | Class | An exception class that indicates that the persistence was not found. |
| `CapePersistenceOverflowException` | Class | An exception class that indicates an overflow of internal persistence system. |
| `CapePersistenceSystemErrorException` | Class | An exception class that indicates a severe error occurred within the persistence system. |
| `CapePhaseStatus` | Enumeration | Status of the phases present in the material object. |
| `CapePortDirection` | Enumeration | The direction that objects or information connected to the port is expected to flow (e.g. material, energy or information objects). |
| `CapePortType` | Enumeration | The type of objects or information that can flow into the unit operation from the connected object. |
| `CapeReactionRateBasis` | Enumeration | Enumeration for the rate basis for the reaction. |
| `CapeReactionType` | Enumeration | Enumeration for the type of reaction. |
| `CapeSolutionStatus` | Enumeration | Indicates solution status of the monitored flowsheet. |
| `CapeSolvingErrorException` | Class | An exception class that indicates a numerical algorithm failed for any reason. |
| `CapeThermoMaterialWrapper` | Class | A wrapper class for the ICapeThermoMaterial interface. |
| `CapeThermoSystem` | Class | A class that implements the interface and provides access to COM and .Net-based property packages available on the current computer. |
| `CapeThrmPropertyNotAvailableException` | Class | Exception thrown when a requested theromdynamic property is not available. |
| `CapeTimeOutException` | Class | Exception thrown when the time-out criterion is reached. |
| `CapeUnknownException` | Class | This exception is raised when other error(s), specified by the operation, do not suit. |
| `CapeUserException` | Class | This is the abstract base class for all .Net based CAPE-OPEN exception classes. |
| `CapeValidationStatus` | Enumeration | Enumeration flag to indicate parameter validation status. |
| `CapeVendorURLAttribute` | Class | Provides a vendor URL for the registration of a CAPE-OPEN object. |
| `CapeVersionAttribute` | Class | Provides the CAPE-OPEN version number for the registration of a CAPE-OPEN object. |
| `CBoolParameter` | Class | Boolean-Valued parameter for use in the CAPE-OPEN parameter collection. |
| `CCapeObject` | Class | Abstract base class that implements ICapeIdentification and ICapeUtilities. |
| `CCapeReactionChemistry` | Class | The reaction chemistry class. |
| `CIntParameter` | Class | Intger-Valued parameter for use in the CAPE-OPEN parameter collection. |
| `CMixerExample` | Class | This is a mixer eample class that models an adiabtic mixer. |
| `CMixerExample110` | Class | This is a mixer eample class that models an adiabtic mixer. |
| `CollectionDisposed` | Delegate | Represents the method that will handle the disposal of a collection. |
| `COMCapeOpenExceptionWrapper` | Class | A wrapper class for COM-based exceptions. |
| `COMExceptionHandler` | Class | A helper class for handling exceptions from COM-based CAPE-OPEN components. |
| `ComponentDescriptionChangedHandler` | Delegate | Represents the method that will handle the changing of the description of a component. |
| `ComponentNameChangedHandler` | Delegate | Represents the method that will handle the changing the name of a component. |
| `COptionParameter` | Class | String Parameter class that implements the ICapeParameter and ICapeOptionParameterSpec CAPE-OPEN interfaces. |
| `CParameterCollection` | Class | A type-safe collection of ICapeParameter objects. |
| `CPortCollection` | Class | A type-safe collection of ICapeUnitPort objects. |
| `CPropertyPackage` | Class | Summary for CPropertyPackage |
| `CReactionCollection` | Class | Initializes a new instance of the CReactionCollection class |
| `CReactionPackageManager` | Class | Initializes a new instance of the CReactionPackageManager class |
| `CRealParameter` | Class | Real-Valued parameter for use in the CAPE-OPEN parameter collection. |
| `CThermoSystem` | Class | Summary for CThermoSystem |
| `CUnitBase` | Class | Abstract base class to be used to develop unit operation models. |
| `CUnitPort` | Class | Summary for CUnitPort |
| `DescriptionChangedEventArgs` | Class | Provides data for the CapeIdentification.ComponentDescriptionChanged event. |
| `ECapeBadArgument` | Interface | An invalid argument value was passed. |
| `ECapeBadArgument093` | Interface | An invalid argument value was passed. |
| `ECapeBadCOParameter` | Interface | A parameter, which is an object from the Parameter Common Interface, has an invalid status. |
| `ECapeBadInvOrder` | Interface | The necessary pre-requisite operation has not been called prior to the operation request. |
| `ECapeBoundaries` | Interface | This interface provides information about error that result from values that are outside of their bounds. It can be raised to indicate that the value of either a method argument or the value of a object parameter is out … |
| `ECapeComputation` | Interface | The base interface of the errors hierarchy related to calculations. |
| `ECapeData` | Interface | The base interface for the errors hierarchy related to any data. |
| `ECapeErrorDummy` | Interface | The ECapeErrorDummy interface is not intended to be used. |
| `ECapeFailedInitialisation` | Interface | This exception is thrown when necessary initialisation has not been performed or has failed. |
| `ECapeHessianInfoNotAvailable` | Interface | Exception thrown when the Hessian for the MINLP problem is not available. |
| `ECapeIllegalAccess` | Interface | The access to something within the persistence system is not authorised. |
| `ECapeImplementation` | Interface | The base class of the errors hierarchy related to the current implementation. |
| `ECapeInvalidArgument` | Interface | An invalid argument value was passed. For instance the passed name of the phase does not belong to the CO Phase List. |
| `ECapeInvalidOperation` | Interface | This operation is not valid in the current context. |
| `ECapeLicenceError` | Interface | An operation can not be completed because the licence agreement is not respected. |
| `ECapeLimitedImpl` | Interface | The limit of the implementation has been violated. |
| `ECapeNoImpl` | Interface | An exception that indicates that the requested operation has not been implemented by the current object. |
| `ECapeNoMemory` | Interface | An exception that indicates that the memory required for this operation is not available. |
| `ECapeOutOfBounds` | Interface | An argument value is outside of the bounds.. |
| `ECapeOutOfResources` | Interface | An exception that indicates that the resources required by this operation are not available. |
| `ECapeOutsideSolverScope` | Interface | Exception thrown when the problem is outside the scope of the solver. |
| `ECapePersistence` | Interface | An exception that indicates that the a persistence-related error has occurred. |
| `ECapePersistenceNotFound` | Interface | An exception that indicates that the persistence was not found. |
| `ECapePersistenceOverflow` | Interface | An exception that indicates an overflow of internal persistence system. |
| `ECapePersistenceSystemError` | Interface | An exception that indicates a severe error occurred within the persistence system. |
| `ECapeRoot` | Interface | The root CAPE-OPEN Exception interface. |
| `ECapeSolvingError` | Interface | An exception that indicates a numerical algorithm failed for any reason. |
| `ECapeThrmPropertyNotAvailable` | Interface | An exception that indicates the requested thermodynamic property was not available. |
| `ECapeTimeOut` | Interface | Exception thrown when the time-out criterion is reached. |
| `ECapeUnknown` | Interface | This exception is raised when other error(s), specified by the operation, do not suit. |
| `ECapeUser` | Interface | The base interface of the CO errors hierarchy. |
| `EquilibriumReactionsChanged` | Delegate | Represents the method that will handle the changing of the Equilibrium Reaction Chemistry of a PMC. |
| `IATCapeXRealParameterSpec` | Interface | Aspen interface for providing dimension for a real-valued parameter. |
| `ICapeArrayParameterSpec` | Interface | This interface is for a parameter specification when the parameter is an array of values (maybebe integers,reals, booleans or arrays again, which represents. |
| `ICapeBooleanParameterSpec` | Interface | This interface is for a parameter specification when the parameter is a boolean |
| `ICapeCollection` | Interface | This interface provides the behaviour for a read-only collection. It can be used for storing ports or parameters. |
| `ICapeCOSEUtilities` | Interface | Provides a mechanism for the PMC to obtain a free FORTRAN channel from the PME. |
| `ICapeDiagnostic` | Interface | Provides a mechanism to provide verbose messages to the user. |
| `ICapeElectrolyteReactionContext` | Interface | Provides access to the properties of a set of electrolyte reactions. |
| `ICapeFlowsheetMonitoring` | Interface | This interface provides information about error that result from values that are outside of their bounds. It can be raised to indicate that the value of either a method argument or the value of a object parameter is out … |
| `ICapeFlowsheetMonitoringOld` | Interface | This interface provides information about error that result from values that are outside of their bounds. It can be raised to indicate that the value of either a method argument or the value of a object parameter is out … |
| `ICapeIdentification` | Interface | Provides methods to identify and describe a CAPE-OPEN component. |
| `ICapeIntegerParameterSpec` | Interface | This interface is for a parameter specification when the parameter is an integer value. |
| `ICapeKineticReactionContext` | Interface | Provides access to the properties of a set of kinetic reactions. |
| `ICapeMaterialTemplateSystem` | Interface | Creates a new thermo material template of the specified type. |
| `ICapeOptionParameterSpec` | Interface | This interface is for a parameter specification when the parameter is an option, which represents a list of strings from which one is selected. |
| `ICapeParameter` | Interface | Interface defining the actual Parameter quantity. |
| `ICapeParameterSpec` | Interface | Gets the dimensionality of the parameter. |
| `ICapePetroFractions` | Interface | ICapePetroFractions interface Provides methods to identify a CAPE-OPEN component. |
| `ICapeReactionChemistry` | Interface | Provides information about the reactions in the reaction package. |
| `ICapeReactionProperties` | Interface | Provides access to the properties of a particular reaction. |
| `ICapeReactionsPackageManager` | Interface | Similar in scope to the . These interfaces will be implemented by a Reactions Package Manager component. |
| `ICapeReactionsRoutine` | Interface | Calculates the values of reaction (or reaction related) properties. |
| `ICapeRealParameterSpec` | Interface | This interface is for a parameter specification when the parameter has a double-precision floating point value. |
| `ICapeSimulationContext` | Interface | Encloses the diagnostic functionality. |
| `ICapeThermoCalculationRoutine` | Interface | ICapeThermoCalculationRoutine is a mechanism for adding foreign calculation routines to a physical property package. |
| `ICapeThermoCompounds` | Interface | When implemented by a Property Package, this interface is used to access the list of Compounds that the Property Package can deal with, as well as the Compounds Physical Properties. When implemented by a Material Object,… |
| `ICapeThermoContext` | Interface | Provides a material object for physical property calculations. |
| `ICapeThermoEquilibriumRoutine` | Interface | Implemented by any component or object that can perform an Equilibrium Calculation. |
| `ICapeThermoEquilibriumServer` | Interface |  |
| `ICapeThermoMaterial` | Interface | A Material Object is a container of information that describes a Material stream. Calculations of thermophysical and thermodynamic properties are performed by a Property Package using information stored in a Material Obj… |
| `ICapeThermoMaterialContext` | Interface | This interface should be implemented by all Thermodynamic and Physical Properties components that need an ICapeThermoMaterial interface in order to set and get a Material’s property values. |
| `ICapeThermoMaterialObject` | Interface | Material object interface |
| `ICapeThermoMaterialTemplate` | Interface | Material Template interface |
| `ICapeThermoPhases` | Interface | Provides information about the number and types of Phases supported by the component that implements it. |
| `ICapeThermoPropertyPackage` | Interface | Calculate some equilibrium values |
| `ICapeThermoPropertyPackageManager` | Interface | The ICapeThermoPropertyPackageManager interface should only be implemented by a Property Package Manager component. This interface is used to access the Property Packages managed by such a component. |
| `ICapeThermoPropertyRoutine` | Interface | This method is used to calculate the natural logarithm of the fugacity coefficients (and optionally their derivatives) in a single Phase mixture. The values of temperature, pressure and composition are specified in the a… |
| `ICapeThermoReliability` | Interface | Interface for the reliability of the Thermo Object. |
| `ICapeThermoSystem` | Interface | Interface class that provides access to property packages supported by a Thermodynamics Package. |
| `ICapeThermoUniversalConstant` | Interface | Implemented by a component that can return the value of a Universal Constant. |
| `ICapeUnit` | Interface | This interface handles most of the interaction with the Flowsheet Unit. |
| `ICapeUnitPort` | Interface | Connects an object to the port. For a material port it must be an object implementing the ICapeThermoMaterialObject interface, for Energy and Information ports it must be an object implementing the ICapeParameter interfa… |
| `ICapeUnitPortVariables` | Interface | Port variables for equation-oriented simulators. |
| `ICapeUnitReport` | Interface | This interface provides access to the active unit report and the available list of options. |
| `ICapeUtilities` | Interface | Interface that exposes a PMC's parameters, controls the PMC's lifecycle, provides access to the PME through the simulation context, and provides a means for the PME to edit the PMC. |
| `Installer` | Class | Summary for Installer |
| `IPersist` | Interface |  |
| `IPersistStream` | Interface |  |
| `IPersistStreamInit` | Interface |  |
| `KineticReactionsChanged` | Delegate | Represents the method that will handle the changing of the Kinetic Reaction Chemistry of a PMC. |
| `MaterialObjectWrapper` | Class | Wrapper class for COM-based CAPE-OPEN ICapeThermoMaterialObject material object. |
| `MixerEditor` | Class | Summary for MixerEditor WARNING: If you change the name of this class, you will need to change the 'Resource File Name' property for the managed resource compiler tool associated with all .resx files this class depends o… |
| `NameChangedEventArgs` | Class | Provides data for the CapeIdentification.ComponentNameChanged event. |
| `ParameterDefaultValueChanged` | Delegate | Represents the method that will handle the changing of the default value of a parameter. |
| `ParameterLowerBoundChanged` | Delegate | Represents the method that will handle the changing of the lower bound of a parameter. |
| `ParameterModeChanged` | Delegate | Represents the method that will handle the changing of the mode of a parameter. |
| `ParameterOptionListChanged` | Delegate | Represents the method that will handle the changing of the option list of a parameter. |
| `ParameterOptionsListChanged` | Delegate | Represents the method that will handle the changing of the options list of a parameter. |
| `ParameterReset` | Delegate | Represents the method that will handle the resetting of a parameter. |
| `ParameterRestrictedToListChanged` | Delegate | Represents the method that will handle the changing of whether a paratemer's value is restricted to those in the option list. |
| `ParameterUpperBoundChanged` | Delegate | Represents the method that will handle the changing of the upper bound of a parameter. |
| `ParameterValidated` | Delegate | Represents the method that will handle the validation of a parameter. |
| `ParameterValueChanged` | Delegate | Represents the method that will handle the changing of the value of a parameter. |
| `PropertyPackageChanged` | Delegate | Represents the method that will handle the changing of a property package used in a PMC. |
| `reactant` | Structure | Indicates whether this instance and a specified object are equal. |
| `Reaction` | Class | Initializes a new instance of the Reaction class |
| `SimulationContextChanged` | Delegate | Represents the method that will handle the changing of the simualtion context of a PMC. |
| `UnitOperationManager` | Class | This class provides access to unit operations based upon .Net-based assembly location rules. |
| `UnitSelector` | Class | Summary for UnitSelector WARNING: If you change the name of this class, you will need to change the 'Resource File Name' property for the managed resource compiler tool associated with all .resx files this class depends … |
| `WARAddIn` | Class | Summary for WAR |
| `warAddUserData` | Class | Summary for warAddUserData WARNING: If you change the name of this class, you will need to change the 'Resource File Name' property for the managed resource compiler tool associated with all .resx files this class depend… |
| `WARalgorithm` | Class | Summary for WARalgorithm WARNING: If you change the name of this class, you will need to change the 'Resource File Name' property for the managed resource compiler tool associated with all .resx files this class depends … |