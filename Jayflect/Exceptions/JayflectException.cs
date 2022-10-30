using System.Collections;
using Jay.Comparison;
using Jay.Dumping;
using Jayflect.Building;

namespace Jayflect.Exceptions;

/// <summary>
/// An <see cref="Exception"/> thrown during Jayflect operations
/// </summary>
public class JayflectException : Exception
{
    private static readonly Action<Exception, string?> _setExceptionMessage;
    private static readonly Action<Exception, Exception?> _setExceptionInnerException;

    static JayflectException()
    {
        var exMessageField = Reflect.FindField(typeof(Exception),
            "_message",
            BindingFlags.NonPublic | BindingFlags.Instance,
            typeof(string));
        _setExceptionMessage = RuntimeBuilder.CreateDelegate<Action<Exception, string?>>(
            "Exception_set_Message",
            emitter => emitter
                .Ldarg(0)
                .Ldarg(1)
                .Stfld(exMessageField)
                .Ret());

        var exInnerExField = Reflect.FindField(typeof(Exception),
            "_innerException",
            BindingFlags.NonPublic | BindingFlags.Instance,
            typeof(Exception));
        _setExceptionInnerException = RuntimeBuilder.CreateDelegate<Action<Exception, Exception?>>(
            "Exception_set_InnerException",
            emitter => emitter
                .Ldarg(0)
                .Ldarg(1)
                .Stfld(exInnerExField)
                .Ret());
    }

    public new string Message
    {
        get => base.Message;
        init => _setExceptionMessage(this, value);
    }

    public new Exception? InnerException
    {
        get => base.InnerException;
        init => _setExceptionInnerException(this, value);
    }

    private sealed class DictAdapter : IDictionary<string, object?>
    {
        private readonly IDictionary _dictionary;

        public object? this[string key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        ICollection<string> IDictionary<string, object?>.Keys => _dictionary.Keys.Cast<string>().ToList();

        public IReadOnlySet<string> Keys => _dictionary.Keys.Cast<string>().ToHashSet();

        ICollection<object?> IDictionary<string, object?>.Values => _dictionary.Values.Cast<object?>().ToList();

        public IReadOnlyCollection<object?> Values => _dictionary.Values.Cast<object?>().ToList();

        public int Count => _dictionary.Count;

        bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => _dictionary.IsReadOnly;

        public DictAdapter(IDictionary dictionary)
        {
            _dictionary = dictionary;
        }

        public void Add(KeyValuePair<string, object?> pair) => _dictionary.Add(pair.Key, pair.Value);
        
        public void Add(string key, object? value) => _dictionary.Add(key, value);

        public bool ContainsKey(string key)
        {
            return _dictionary.Contains(key);
        }
        
        public bool Contains(KeyValuePair<string, object?> pair)
        {
            return _dictionary.Contains(pair.Key) &&
                   DefaultComparers.Instance.Equals(pair.Value, _dictionary[pair.Key]);
        }
        
        public bool TryGetValue(string key, out object? value)
        {
            if (_dictionary.Contains(key))
            {
                value = _dictionary[key];
                return true;
            }
            value = default;
            return false;
        }

        void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);
            if ((uint)arrayIndex + Count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            
            foreach (DictionaryEntry entry in _dictionary)
            {
                array[arrayIndex++] = new KeyValuePair<string, object?>(entry.Key.ToString() ?? "", entry.Value);
            }
        }
        
        public bool Remove(string key)
        {
            if (_dictionary.Contains(key))
            {
                _dictionary.Remove(key);
                return true;
            }
            return false;
        }
        
        public bool Remove(KeyValuePair<string, object?> pair)
        {
            if (Contains(pair))
            {
                _dictionary.Remove(pair.Key);
                return true;
            }
            return false;
        }
        
        public void Clear() => _dictionary.Clear();
       
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return _dictionary.Cast<DictionaryEntry>()
                .Select(entry => new KeyValuePair<string, object?>((entry.Key as string)!, entry.Value))
                .GetEnumerator();
        }
    }

    private DictAdapter? _data = null;
    
    public new IDictionary<string, object?> Data => _data ??= new DictAdapter(base.Data);

    public JayflectException()
        : base()
    {

    }
    
    public JayflectException(
        ref DumpStringHandler message, 
        Exception? innerException = null)
        : base(message.ToStringAndDispose(), innerException)
    {

    }
    
    public JayflectException(
        string? message = null,
        Exception? innerException = null)
        : base(message, innerException)
    {

    }

    public override string ToString()
    {
        if (base.Data.Count == 0)
        {
            return base.ToString();
        }
        
        var dumper = new DumpStringHandler();
        dumper.Write(base.ToString());
        foreach (var pair in this.Data)
        {
            dumper.Write(Environment.NewLine);
            dumper.Write(pair.Key);
            dumper.Write(": ");
            dumper.Dump(pair.Value);
        }
        return dumper.ToStringAndDispose();
    }
}