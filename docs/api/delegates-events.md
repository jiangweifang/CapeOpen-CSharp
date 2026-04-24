# 委托与事件 (Delegates & Events)

CAPE-OPEN .NET 类库中的事件通过委托 + `*EventArgs` 类配合使用。

## 委托 (Delegates)

共 17 个。

### CollectionDisposed

Represents the method that will handle the disposal of a collection.

### ComponentDescriptionChangedHandler

Represents the method that will handle the changing of the description of a component.

### ComponentNameChangedHandler

Represents the method that will handle the changing the name of a component.

### EquilibriumReactionsChanged

Represents the method that will handle the changing of the Equilibrium Reaction Chemistry of a PMC.

### KineticReactionsChanged

Represents the method that will handle the changing of the Kinetic Reaction Chemistry of a PMC.

### ParameterDefaultValueChanged

Represents the method that will handle the changing of the default value of a parameter.

### ParameterLowerBoundChanged

Represents the method that will handle the changing of the lower bound of a parameter.

### ParameterModeChanged

Represents the method that will handle the changing of the mode of a parameter.

### ParameterOptionListChanged

Represents the method that will handle the changing of the option list of a parameter.

### ParameterOptionsListChanged

Represents the method that will handle the changing of the options list of a parameter.

### ParameterReset

Represents the method that will handle the resetting of a parameter.

### ParameterRestrictedToListChanged

Represents the method that will handle the changing of whether a paratemer's value is restricted to those in the option list.

### ParameterUpperBoundChanged

Represents the method that will handle the changing of the upper bound of a parameter.

### ParameterValidated

Represents the method that will handle the validation of a parameter.

### ParameterValueChanged

Represents the method that will handle the changing of the value of a parameter.

### PropertyPackageChanged

Represents the method that will handle the changing of a property package used in a PMC.

### SimulationContextChanged

Represents the method that will handle the changing of the simualtion context of a PMC.


## 事件参数类 (*EventArgs)

共 2 个。

### DescriptionChangedEventArgs

Provides data for the CapeIdentification.ComponentDescriptionChanged event.

### NameChangedEventArgs

Provides data for the CapeIdentification.ComponentNameChanged event.
