# Breaking Changes & Release Notes

This page details major release changes and migration guidance across Meerkat versions.

---

## v3.0.0

### Key Enhancements & Changes

- **Ambient Multi-Document Transactions**:
  Introduced `Meerkat.WithTransaction` and `Meerkat.WithTransactionAsync`. Reads and writes inside transaction callbacks automatically participate in the active session without requiring manual session parameters.
- **Index Verification at Startup**:
  Added `Meerkat.EnsureIndexes` and `Meerkat.EnsureIndexesAsync` to idempotently create and verify attribute indexes upfront. Throws `IndexVerificationException` if an expected index cannot be confirmed.
- **TTL Indexes**:
  Added `ExpireAfter` on `[SingleFieldIndex]` supporting human-readable duration strings (e.g., `"30d"`, `"12h"`, `"15m"`) and ISO-8601 durations on `DateTime`/`DateTime?` properties.
- **Compound Unique Indexes**:
  Added `Unique` property to `[CompoundIndex]` to enforce compound uniqueness across grouped fields.
- **Soft Delete Support**:
  Added opt-in soft delete via `[Collection(SoftDelete = true)]`. Automatically excludes soft-deleted records from queries, counts, and updates; provides `Restore*` and `HardRemove*` for restoring and permanently deleting documents.
- **Fluent Atomic Partial Updates**:
  Added fluent `Update`, `UpdateOne`, `UpdateMany`, and `UpdateByFilter` builders supporting `$set`, `$unset`, `$push`, `$pull`, `$addToSet`, and `$inc`.
- **Dependency Upgrades**:
  Upgraded `MongoDB.Driver` to 3.12.0 and replaced `mongo-url-parser` with `ciu-parser`.

---

## v2.0.0

### Breaking Changes

- **MongoDB Driver 3.x Upgrade**:
  The underlying `MongoDB.Driver` was upgraded to 3.x.
- **Generic `Schema<TId>` Base Class**:
  The non-generic `Schema` base class was replaced by the generic `Schema<TId>`, requiring an explicit ID type parameter on all models and static `Meerkat` methods (e.g., `Meerkat.FindByIdAsync<Student, ObjectId>(id)`).
