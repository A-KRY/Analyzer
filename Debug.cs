using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer {
#if DEBUG
	internal static class Debug {
		private static StreamWriter? streamWriter = null;

		// 初始化输出流
		public static void InitStreamWriter(String path) {
			if (streamWriter is not null) {
				streamWriter.Close();
			}
			streamWriter = new StreamWriter(path);
			streamWriter.AutoFlush = true;
		}

		// 输出栈内元素
		public static void ShowStack<T>(Stack<T> stack) {
			foreach (var item in stack.Reverse()) {
				Console.Write(item + " ");
			}
			Console.Write('\n');
		}

		public static void LogShowStack<T>(Stack<T> stack) {
			foreach (var item in stack.Reverse()) {
				streamWriter.Write(item + " ");
			}
			streamWriter.Write('\n');
		}

		// 输出
		public static void LogWrite(String str) {
			streamWriter.Write(str);
		}
		public static void LogWriteLine() {
			streamWriter.WriteLine();
		}

		public static void LogWriteLine(String str) {
			streamWriter.WriteLine(str);
		}

		public static void Close() {
			streamWriter.Close();
		}
	}
#endif
}
