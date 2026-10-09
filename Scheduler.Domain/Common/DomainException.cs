namespace Scheduler.Domain.Common;

/// <summary>Thrown when an operation would violate a domain invariant.</summary>
public class DomainException(string message) : Exception(message);
