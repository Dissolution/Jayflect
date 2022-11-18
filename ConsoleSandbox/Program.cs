using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using ConsoleSandbox;
using Jayflect;
using Jayflect.Serialization;

#pragma warning disable

// Use something from Reflection to get its dumpers!
RuntimeHelpers.RunClassConstructor(typeof(Reflect).TypeHandle);

try
{
    // var field = typeof(Entity).GetProperty("Id", Reflect.Flags.Instance).GetBackingField().ValidateNotNull();
    //
    // var entity = new Entity(13)
    // {
    //     Name = "Thirteen Isn't Lucky",
    // };
    //
    // var reflection = DynamicReflection.Of(entity);
    // reflection.UpdateName("joe's");
    // var name = reflection.Name;
    // Console.WriteLine(name);
    // name = reflection.get_Name();
    // Console.WriteLine(name);
    // reflection.Name = "Billy Rae";
    // name = reflection.Name();
    // Console.WriteLine(name);

    //var tc = new TestEntity(666);
    //var reflection = DynamicReflection.Of(tc);
    //var reflection = DynamicReflection.Of(147);

    //bool isFour = reflection == 4;

    //var parts = ChopShop.PartOut(tc);

    var text = "This is a test of an advanced encoding system";

    var bytes = Encoding.ASCII.GetBytes(text);
    var output = Dumb.Compress(bytes);
    var outputStr = Encoding.UTF8.GetString(output);

    var input = Dumb.Decompress(output);
    var inputStr = Encoding.ASCII.GetString(input);

    Debug.Assert(inputStr == text);

    var cdmText = string.Concat(text.Prepend('E').Select(ch => Dumb.ConvertToCDM(ch)));
    var asciiText = string.Concat(cdmText.Skip(1).Select(ch => Dumb.ConvertToAscii(ch)));

    Debug.Assert(asciiText == text);

    var eq = outputStr == cdmText;

    Debugger.Break();
}
catch (Exception mainException)
{
    Debugger.Break();
}

namespace ConsoleSandbox
{
    public class Dumb
    {
        // https://unicode-table.com/en/blocks/combining-diacritical-marks/
        private const char cdmStart = (char)0b_00000011_00000000;
        private const char cdmEnd = (char)0b_00000011_01101111;

        private static byte[] cdmStartBytes = Encoding.UTF8.GetBytes(new char[1] { cdmStart });
        private static byte[] cdmEndBytes = Encoding.UTF8.GetBytes(new char[1] { cdmEnd });

        private const char asciiStart = (char)0b00100000;
        private const char asciiEnd = (char)0b01111110;

        static Dumb()
        {
            int len = (cdmEnd - cdmStart) + 1;
            Debug.Assert(len == 112);

            len = (asciiEnd - asciiStart) + 1;
            Debug.Assert(len == 0b01011111);
            //
            // var sStrs = cdmStartBytes.Select(b => Convert.ToString(b, 2)).ToList();
            // var eStrs = cdmEndBytes.Select(b => Convert.ToString(b, 2)).ToList();
            //
            // Debugger.Break();
        }

        public static char ConvertToCDM(char asciiChar)
        {
            if (!TryConvertToCDM(asciiChar, out var cdmChar))
                throw new InvalidOperationException();
            return cdmChar;
        }

        // public static byte[] ConvertToCDMBytes(byte asciiChar)
        // {
        //     if (asciiChar == '\t') // 0x0009
        //     {
        //
        //
        //         cdmChar = (char)(asciiEnd + 1 + cdmStart);
        //     }
        //     else if (asciiChar == '\n') // 0x000A
        //     {
        //         cdmChar = (char)(asciiEnd + 2 + cdmStart);
        //     }
        //     else if (asciiChar == '\r') // 0x000D
        //     {
        //         cdmChar = (char)(asciiEnd + 3 + cdmStart);
        //     }
        //     else if (asciiChar >= asciiStart && asciiChar <= asciiEnd)
        //     {
        //         cdmChar = (char)((asciiChar - asciiStart) + cdmStart);
        //     }
        //     else
        //     {
        //         cdmChar = default;
        //         return false;
        //     }
        //
        //     return true;
        // }

        public static bool TryConvertToCDM(char asciiChar, out char cdmChar)
        {
            if (asciiChar == '\t') // 0x0009
            {
                cdmChar = (char)(asciiEnd + 1 + cdmStart);
            }
            else if (asciiChar == '\n') // 0x000A
            {
                cdmChar = (char)(asciiEnd + 2 + cdmStart);
            }
            else if (asciiChar == '\r') // 0x000D
            {
                cdmChar = (char)(asciiEnd + 3 + cdmStart);
            }
            else if (asciiChar >= asciiStart && asciiChar <= asciiEnd)
            {
                cdmChar = (char)((asciiChar - asciiStart) + cdmStart);
            }
            else
            {
                cdmChar = default;
                return false;
            }

            return true;
        }

        public static char ConvertToAscii(char cdmChar)
        {
            if (!TryConvertToAscii(cdmChar, out var asciiChar))
                throw new InvalidOperationException();
            return asciiChar;
        }

        public static bool TryConvertToAscii(char cdmChar, out char asciiChar)
        {
            if (cdmChar >= cdmStart && cdmChar <= cdmEnd)
            {
                asciiChar = (char)((cdmChar - cdmStart) + asciiStart);
                if (asciiChar >= asciiStart && asciiChar <= asciiEnd)
                    return true;
                if (asciiChar == asciiEnd + 1)
                {
                    asciiChar = '\t';
                    return true;
                }
                if (asciiChar == asciiEnd + 2)
                {
                    asciiChar = '\n';
                    return true;
                }
                if (asciiChar == asciiEnd + 3)
                {
                    asciiChar = '\r';
                    return true;
                }
            }

            asciiChar = default;
            return false;
        }



        //https: //github.com/DaCoolOne/DumbIdeas/blob/main/reddit_ph_compressor/compress.py
        //https://www.reddit.com/r/ProgrammerHumor/comments/yqof9f/the_most_upvoted_comment_picks_the_next_line_of/#ivrd9ur

        /* # Compress algorithm
    def unicode_compress(bytes):
        o = b'E'
        for c in bytes:
            # Skip carriage returns
            if c == 13:
                continue
            # Check for invalid code points
            if (c < 20 or c > 126) and c != 10:
                raise Exception("Cannot encode character with code point " + str(c))
            # Code point translation
            v = (c-11)%133-21
            o += ((v >> 6) & 1 | 0b11001100).to_bytes(1,'big')
            o += ((v & 63) | 0b10000000).to_bytes(1,'big')
        return o*/
        public static byte[] Compress(ReadOnlySpan<byte> asciiBytes)
        {
            List<byte> output = new(asciiBytes.Length * 2) { (byte)'E' };
            foreach (byte c in asciiBytes)
            {
                // Special handling


                // Check for invalid code points
                if (c != 10 && (c < 20 || c > 126))
                    throw new InvalidOperationException($"Cannot encode character with code point '{c}'");
                // code point translation
                int v = ((c - 11) % 133) - 21;

                int h = c - 32;
                Debug.Assert(v == h);

                byte lower = (byte)(((v >> 6) & 0b00000001) | 0b11001100);
                output.Add(lower);

                byte higher = (byte)((v & 0b00111111) | 0b10000000);
                output.Add(higher);

                var lh = Encoding.UTF8.GetString(new byte[2] { lower, higher });
                Debug.Assert(lh.Length == 1);
                Debug.Assert(lh[0] is >= cdmStart and <= cdmEnd);

                //Debugger.Break();
            }

            return output.ToArray();
        }

/*# Decompress algorithm (Code golfed)
def unicode_decompress(b):
    return ''.join([chr(((h<<6&64|c&63)+22)%133+10)for h,c in zip(b[1::2],b[2::2])])
*/
        public static byte[] Decompress(IReadOnlyList<byte> bytes)
        {
            var output = new List<byte>();
            var first = bytes.Skip(1).Where((b, i) => i % 2 == 0);
            var second = bytes.Skip(2).Where((b, i) => i % 2 == 0);
            foreach (var (h, c) in Enumerable.Zip(first, second))
            {
                byte x = (byte)(((((h << 6) & 0b01000000) | (c & 0b00111111)) + 22) % 133 + 10);
                output.Add(x);
            }

            return output.ToArray();
        }
    }


    public class TestEntity
    {
        public int Id { get; init; }
        public string Name { get; set; }

        public TestEntity(int id)
        {
            this.Id = id;
        }
    }

    public class TestClass
    {
        public static bool operator false(TestClass testClass) => false;
        public static bool operator true(TestClass testClass) => true;

        public static bool operator &(TestClass left, bool right) => right;
        public static bool operator |(TestClass left, bool right) => true;
        public static bool operator ^(TestClass left, bool right) => !right;

        public static bool operator ==(TestClass left, TestClass right) => left.Equals(right);
        public static bool operator !=(TestClass left, TestClass right) => !left.Equals(right);

        private readonly Dictionary<string, object?> _dict = new();

        public object? this[string key]
        {
            get => _dict[key];
            set => _dict[key] = value;
        }
    }

    public static class TestStaticClass
    {
        private static readonly Dictionary<string, object?> _dict = new();

        private static object? get_Item(string key)
        {
            return _dict[key];
        }

        private static void set_Item(string key, object? value)
        {
            _dict[key] = value;
        }

        public static Entity NewEntity() => new Entity(Random.Shared.Next(), Guid.NewGuid().ToString("N"));

        public static void WriteNonsense()
        {
            Console.WriteLine(Guid.NewGuid().ToString("N"));
        }

        public static object? Invoke(params object?[] args)
        {
            Debugger.Break();
            return null;
        }
    }

    public class Entity : IEquatable<Entity>
    {
        public int Id { get; init; }

        public string Name { get; set; } = "";

        public Entity(int id)
        {
            this.Id = id;
        }

        public Entity(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public void UpdateName(string? name)
        {
            this.Name = name ?? "";
        }

        public override string ToString()
        {
            return $"{Name} #{Id}";
        }

        public bool Equals(Entity? entity)
        {
            return this.Id == entity.Id;
        }

        public override bool Equals(object? obj)
        {
            return obj is Entity entity && Equals(entity);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}