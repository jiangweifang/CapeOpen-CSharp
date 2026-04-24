# 异常 (Exceptions)

CAPE-OPEN 标准异常由两部分组成：

- **`ECape*` 接口**：COM 侧标准错误接口，跨语言错误契约。
- **`Cape*Exception` 类**：.NET 侧对应的异常类，实现上述接口。

## 异常接口 (ECape*)

共 31 个。

### ECapeBadArgument

An invalid argument value was passed.

### ECapeBadArgument093

An invalid argument value was passed.

### ECapeBadCOParameter

A parameter, which is an object from the Parameter Common Interface, has an invalid status.

### ECapeBadInvOrder

The necessary pre-requisite operation has not been called prior to the operation request.

### ECapeBoundaries

This interface provides information about error that result from values that are outside of their bounds. It can be raised to indicate that the value of either a method argument or the value of a object parameter is out of range.

### ECapeComputation

The base interface of the errors hierarchy related to calculations.

### ECapeData

The base interface for the errors hierarchy related to any data.

### ECapeErrorDummy

The ECapeErrorDummy interface is not intended to be used.

### ECapeFailedInitialisation

This exception is thrown when necessary initialisation has not been performed or has failed.

### ECapeHessianInfoNotAvailable

Exception thrown when the Hessian for the MINLP problem is not available.

### ECapeIllegalAccess

The access to something within the persistence system is not authorised.

### ECapeImplementation

The base class of the errors hierarchy related to the current implementation.

### ECapeInvalidArgument

An invalid argument value was passed. For instance the passed name of the phase does not belong to the CO Phase List.

### ECapeInvalidOperation

This operation is not valid in the current context.

### ECapeLicenceError

An operation can not be completed because the licence agreement is not respected.

### ECapeLimitedImpl

The limit of the implementation has been violated.

### ECapeNoImpl

An exception that indicates that the requested operation has not been implemented by the current object.

### ECapeNoMemory

An exception that indicates that the memory required for this operation is not available.

### ECapeOutOfBounds

An argument value is outside of the bounds..

### ECapeOutOfResources

An exception that indicates that the resources required by this operation are not available.

### ECapeOutsideSolverScope

Exception thrown when the problem is outside the scope of the solver.

### ECapePersistence

An exception that indicates that the a persistence-related error has occurred.

### ECapePersistenceNotFound

An exception that indicates that the persistence was not found.

### ECapePersistenceOverflow

An exception that indicates an overflow of internal persistence system.

### ECapePersistenceSystemError

An exception that indicates a severe error occurred within the persistence system.

### ECapeRoot

The root CAPE-OPEN Exception interface.

### ECapeSolvingError

An exception that indicates a numerical algorithm failed for any reason.

### ECapeThrmPropertyNotAvailable

An exception that indicates the requested thermodynamic property was not available.

### ECapeTimeOut

Exception thrown when the time-out criterion is reached.

### ECapeUnknown

This exception is raised when other error(s), specified by the operation, do not suit.

### ECapeUser

The base interface of the CO errors hierarchy.


## 异常类 (*Exception)

共 26 个。

### CapeBadArgumentException

An argument value of the operation is not correct.

### CapeBadInvOrderException

The necessary pre-requisite operation has not been called prior to the operation request.

### CapeBoundariesException

This is an abstract class that allows derived classes to provide information about error that result from values that are outside of their bounds. It can be raised to indicate that the value of either a method argument or the value of a object parameter is out of range.

### CapeComputationException

The base class of the errors hierarchy related to calculations.

### CapeDataException

The base class of the errors hierarchy related to any data.

### CapeFailedInitialisationException

This exception is thrown when necessary initialisation has not been performed or has failed.

### CapeHessianInfoNotAvailableException

Exception thrown when the Hessian for the MINLP problem is not available.

### CapeIllegalAccessException

The access to something within the persistence system is not authorised.

### CapeImplementationException

The base class of the errors hierarchy related to the current implementation.

### CapeInvalidArgumentException

An invalid argument value was passed. For instance the passed name of the phase does not belong to the CO Phase List.

### CapeInvalidOperationException

This operation is not valid in the current context.

### CapeLicenceErrorException

An operation can not be completed because the licence agreement is not respected.

### CapeLimitedImplException

The limit of the implementation has been violated.

### CapeNoImplException

An exception class that indicates that the requested operation has not been implemented by the current object.

### CapeNoMemoryException

An exception class that indicates that the memory required for this operation is not available.

### CapeOutOfBoundsException

An argument value is outside of the bounds..

### CapeOutOfResourcesException

An exception class that indicates that the resources required by this operation are not available.

### CapePersistenceException

An exception class that indicates that the a persistence-related error has occurred.

### CapePersistenceNotFoundException

An exception class that indicates that the persistence was not found.

### CapePersistenceOverflowException

An exception class that indicates an overflow of internal persistence system.

### CapePersistenceSystemErrorException

An exception class that indicates a severe error occurred within the persistence system.

### CapeSolvingErrorException

An exception class that indicates a numerical algorithm failed for any reason.

### CapeThrmPropertyNotAvailableException

Exception thrown when a requested theromdynamic property is not available.

### CapeTimeOutException

Exception thrown when the time-out criterion is reached.

### CapeUnknownException

This exception is raised when other error(s), specified by the operation, do not suit.

### CapeUserException

This is the abstract base class for all .Net based CAPE-OPEN exception classes.
