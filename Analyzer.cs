namespace Analyzer
{
	public class Analyzer
	{
		internal static StreamReader? streamReader = null;

		internal static StreamWriter? streamWriter = null;

		public static void Main(string[] args)
		{
			InitStreamReader("input.txt");
			InitStreamWriter("output.txt");
			Run();
			Console.WriteLine("Successful.");
			Console.WriteLine();
			Console.Write("请按任意键继续...");
			Console.ReadKey();
		}

		public static void InitStreamReader(String path)
		{
			if (streamReader is not null)
			{
				streamReader.Close();
			}
			streamReader = new StreamReader(path);
		}

		public static void InitStreamWriter(String path) {
			if (streamWriter is not null) {
				streamWriter.Close();
			}
			streamWriter = new StreamWriter(path);
			streamWriter.AutoFlush = true;
		}

		public static void Run()
		{
			Parser parser = Parser.Instance;
			parser.Run();
		}
	}
}
