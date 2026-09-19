using System;
using System.Collections.Generic;
using System.Linq;

namespace meerkat.Exceptions;

/// <summary>
/// Represents an exception that is thrown when one or more expected indexes
/// could not be created or were not found after creation.
/// </summary>
public sealed class IndexVerificationException : Exception
{
    /// <summary>
    /// Gets the name of the collection whose indexes failed verification.
    /// </summary>
    public string CollectionName { get; }

    /// <summary>
    /// Gets the names (or key patterns) of the indexes that failed verification.
    /// </summary>
    public IReadOnlyList<string> FailedIndexes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexVerificationException"/> class.
    /// </summary>
    /// <param name="collectionName">The name of the collection whose indexes failed verification.</param>
    /// <param name="failedIndexes">The names or key patterns of the missing or failed indexes.</param>
    public IndexVerificationException(string collectionName, IEnumerable<string> failedIndexes)
        : base($"Index verification failed for collection '{collectionName}': {string.Join(", ", failedIndexes)}")
    {
        CollectionName = collectionName;
        FailedIndexes = failedIndexes.ToList();
    }
}
