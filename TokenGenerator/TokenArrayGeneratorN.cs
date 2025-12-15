using System;
using System.Linq;
using System.Text;

namespace TokenGenerator
{
    class TokenArrayGeneratorN
    {
        static void Main()
        {
            Console.Write("Enter your GitHub PAT (ghp_...): ");
            var pat = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(pat))
            {
                Console.WriteLine("No token provided. Exiting.");
                return;
            }

            const int NUM_PARTS = 4;

            // Generate randomized XOR key (4 bytes)
            var rng = new Random();
            byte[] xorKey = [.. Enumerable.Range(0, 4).Select(_ => (byte)rng.Next(0x10, 0xF0))];

            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(pat));
            var b = Encoding.UTF8.GetBytes(base64);

            for (int i = 0; i < b.Length; i++)
                b[i] ^= xorKey[i % xorKey.Length];

            int chunkSize = (int)Math.Ceiling((double)b.Length / NUM_PARTS);

            Console.WriteLine("\n// ===== COPY INTO GetToken() =====");
            Console.WriteLine("byte[] xorKey = new byte[] { " +
                string.Join(", ", xorKey.Select(x => "0x" + x.ToString("X2"))) + " };");

            for (int i = 0; i < NUM_PARTS; i++)
            {
                var part = b.Skip(i * chunkSize).Take(chunkSize).ToArray();
                Console.WriteLine($"int[] part{i + 1} = new int[] {{ {string.Join(", ", part)} }};");
            }

            Console.WriteLine("\n// ===== Decoding logic =====");
            Console.WriteLine(@"// Combine and decode:
var all = part1.Concat(part2).Concat(part3).Concat(part4)
    .Select((x,i)=> (byte)(x ^ xorKey[i % xorKey.Length])).ToArray();
return Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(all)));");
            Console.WriteLine("// =================================");
        }
    }
}
