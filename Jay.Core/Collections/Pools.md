## Object Pooling
- `ObjectPool<T>` and `AsyncObjectPool<T>` are generic implementations of an `object`-pooling pattern.
- Their main purpose is the re-use of a limited number of objects rather than continuously new-ing them up.
- It is not the goal to keep all returned `T` values.
  - The `Pool` is not meant for storage (short nor long).
  - If there is no space in the `Pool`, extra returned values will be disposed.
- It is implied that if a value is obtained from a pool, the caller will return it back in a relatively short time.
  - Keeping checked out values for long durations is _ok_, but it reduces the usefulness of pooling.
  - Not returning values to the pool in not detrimental to its work, but is a bad practice.
  - If there is no intent to return or re-use the value, do not use a `Pool`.