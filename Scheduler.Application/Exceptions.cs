namespace Scheduler.Application;

public class NotFoundException(string message) : Exception(message);

/// <summary>Thrown when the caller's copy of an event is stale (someone else changed it first).</summary>
public class ConcurrencyException(string message) : Exception(message);
