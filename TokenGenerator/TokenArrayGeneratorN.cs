using System.Text;

namespace TokenGenerator
{
    class TokenArrayGeneratorN
    {
        static void Main(string[] args)
        {
            Console.Write("Enter your GitHub PAT (ghp_...): ");
            var pat = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(pat))
            {
                Console.WriteLine("No token provided. Exiting.");
                return;
            }

            //Note: Change NUM_PARTS and xorKey to generate different arrays
            const int NUM_PARTS = 4;
            byte[] xorKey = [0x5A, 0xC3, 0x1F, 0x77];

            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(pat));
            var b = Encoding.UTF8.GetBytes(base64);
            for (int i = 0; i < b.Length; i++)
                b[i] = (byte)(b[i] ^ xorKey[i % xorKey.Length]);

            int chunkSize = (int)Math.Ceiling((double)b.Length / NUM_PARTS);

            Console.WriteLine("\n// ===== COPY INTO GetToken() =====");
            Console.WriteLine("byte[] xorKey = new byte[] { " +
                string.Join(", ", xorKey.Select(x => "0x" + x.ToString("X2"))) + " };");

            for (int i = 0; i < NUM_PARTS; i++)
            {
                var part = b.Skip(i * chunkSize).Take(chunkSize).ToArray();
                Console.WriteLine($"int[] part{i + 1} = new int[] {{ {string.Join(", ", part.Select(x => x.ToString()))} }};");
            }
            Console.WriteLine("// =================================");
        }
    }
}
