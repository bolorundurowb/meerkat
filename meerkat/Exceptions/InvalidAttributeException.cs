using System;

namespace meerkat.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an invalid attribute is encountered.
/// </summary>
/// <param name="exceptionMessage">The error message describing the invalid attribute usage.</param>
public sealed class InvalidAttributeException(string exceptionMessage) : Exception(exceptionMessage);
