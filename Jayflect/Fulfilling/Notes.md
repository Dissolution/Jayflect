# Implementer Concepts

I think that `record` is a half-implementation.
While `IStructuralEquality`, `Tuple`, and `ValueTuple` have been around for a while, we finally got `record` to simplify a lot of boilerplate.
Unfortunately, I don't think that `record` is a full implementation, only half of one, and not even the right half.
I find myself writing code for Entities more often than Record types. Where a single value (often an `Id` or `Key` property) is the 'true' identifier for the Entity.
When we compare them, we should be comparing by the Id and not any other fields.

I found myself writing an `IEntity<TKey>` and `abstract Entity<TKEy>` time and time again for my own projects. I saw developers slap together Entities for EF and not
override Equals or GetHashCode and wonder where bugs starting coming from.
`record` doesn't solve that, because _all_ of the Properties in a `record` participate in Equality and HashCode operations.
What we need is something like `record` that allows us to specify which Property(ies) are 'important' and which ones aren't.

I could try to extend the C# language and add my own keyword, but going down the `Fody` route feels like a dead end waiting to be broken by future C# updates.
Hence comes in Implementer.

Implementer is an idea I had years ago: a system that can automatically implement interfaces.
Similar to `record`, one would just define and interface that inherits from any other interface and we build a concrete backing type for it that is as efficient as anything a user could write themselves.

### Equality
- There are three base levels of Equality we can build:
  1. Default/Reference Equality - The default for any `object`, `Equals` compares by reference, and `GetHashCode` refers to a particular reference.
  2. Structural Equality - The same as `record`, every Property participates in Equality and GetHashCode
    - `IStructuralEquatable` forces this behavior.
    - If every Property is marked with `[Equality(true)]`, the implementation will act with Structural Equality
  3. Property Equality - The new feature, Properties can be marked with an `EqualityAttribute` to specify whether they participate in Equality and GetHashCode
    - The default is `false`, to keep the need for attributes to a minimum (likely only Property marked per Entity)
- All implementations implement `Equals<TSelf>`, `Equals(obj?)`, and `GetHashCode()` automatically using the base Equality
- `EqualityAttribute`
  - Only on a Property, it specifies whether the Property does or doesn't participate in Equality operations~~~~
  - The default is non-participation, the same as not having the `Attribute` at all
  - Using this `Attribute` changes the functionality of `Equals(obj?) and `GetHashCode()`
- By default, any Implementation acts as a `class`
  - `Equals<TSelf>` (if defined) is reference-equality
  - `Equals(obj?)` is reference-equality
  - `GetHashCode()` calls `RuntimeHelpers.GetHashCode()` on itself
- If any `Property` has an `EqualityAttribute` defined, Property-specific comparision is enabled for all Properties where it is defined as `true`
  - `Equals<TSelf>` checks for `null`, checks for reference-equality, and then compares each property of each Implementation
  - `Equals(obj?)` checks that `obj is TSelf` and calls `Equals<TSelf>`
  - `GetHashCode()` uses `HashCode` to generate a hash from properties
- `IStructuralEquatable` forces Property-specific comparison
  - If no Properties have an `EqualityAttribute` defined, all will be used

### ToString
- The default of `GetType().ToString()` (aka the `Type.Name`) will be hardcoded as a response
- If non-Default equality is being used, the output will include each key Property's name and value
- `IFormattable`
  - Will use the given values to call `GetType().ToString()` by default
  - Will attempt to use each Properties's values as an IFormattable and pass the args forward
  - 
### Property Notification
- `INotifyPropertyChanged`
- `INotifyPropertyChanging`
- Either interface will alter the behavior of Property set accessors for all implemented Properties in order to implement the interface(s)

### Cloning
- `ICloneable`
- `ICloneable<T>` // custom interface
- We will always implement both if we have either, doing a field-wise deepclone of self (all Properties!)

### Disposal
- `IDisposable`
- While not necessary, dispose will clear any Event handlers (set underlying field to `null`)
- Any Property marked with [DisposeAttribute] will also be Disposed

Think!~?
- `IComparable<T> where T : self`
- `IComparable`
- `IStructuralComparable`