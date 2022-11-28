using System.Data;

namespace Analyzer {
	using Symbol = Mnemonic;

	public class Parser
	{
		// 唯一实例
		private static Parser? uniqueInstance = null;

		// 获取实例
		public static Parser Instance {
			get {
				uniqueInstance ??= new Parser();
				return uniqueInstance;
			}
		}

		// 词法分析器
		private LexicalAnalyzer lexicalAnalyzer = LexicalAnalyzer.Instance;

		// 预测分析表
		private Dictionary<Mnemonic, Dictionary<Mnemonic, List<Mnemonic>>> PATable;
		
		// 是否遇到文件尾
		private bool bEndOfStream;

		// buffer 行数计数
		private int lineCnt;

		// 符号计数
		private int sgnCnt;
		// 符号栈
		private Stack<Mnemonic> symbolStack = new Stack<Mnemonic>();

		// 非终结符集合
		private HashSet<Mnemonic> nonterminalSet;

		// 终结符集合
		private HashSet<Mnemonic> terminalSet;

		// 语义分析器
		private SemanticAnalyzer semanticAnalyzer;

		// 当前运算对象
		private Operand currOperand;

		// 构造函数
		private Parser()
		{
			lineCnt = 0;
			sgnCnt = 0;
			bEndOfStream = false;
			semanticAnalyzer = SemanticAnalyzer.Instance;

			// 构造预测分析表
			// 倒序装入
			PATable = new Dictionary<Mnemonic, Dictionary<Mnemonic, List<Mnemonic>>> {
				[Mnemonic.E] = new Dictionary<Mnemonic, List<Mnemonic>> {
					[Mnemonic.i]  = new List<Mnemonic> { Mnemonic.E1, Mnemonic.T },
					[Mnemonic.LB] = new List<Mnemonic> { Mnemonic.E1, Mnemonic.T },
				},

				[Mnemonic.E1] = new Dictionary<Mnemonic, List<Mnemonic>>  {
					[Mnemonic.PL]  = new List<Mnemonic>  { Mnemonic.E1, Mnemonic.T, Mnemonic.PL },
					[Mnemonic.MI]  = new List<Mnemonic> { Mnemonic.E1, Mnemonic.T, Mnemonic.MI },
					[Mnemonic.RB]  = new List<Mnemonic> {},
					[Mnemonic.EOF] = new List<Mnemonic> {},
				},

				[Mnemonic.T] = new Dictionary<Mnemonic, List<Mnemonic>> {
					[Mnemonic.i]  = new List<Mnemonic> { Mnemonic.T1, Mnemonic.F },
					[Mnemonic.LB] = new List<Mnemonic> { Mnemonic.T1, Mnemonic.F },
				},

				[Mnemonic.T1] = new Dictionary<Mnemonic, List<Mnemonic>> {
					[Mnemonic.PL]  = new List<Mnemonic> {},
					[Mnemonic.MI]  = new List<Mnemonic> {},
					[Mnemonic.MU]  = new List<Mnemonic> { Mnemonic.T1, Mnemonic.F, Mnemonic.MU },
					[Mnemonic.DI]  = new List<Mnemonic> { Mnemonic.T1, Mnemonic.F, Mnemonic.DI },
					[Mnemonic.RB]  = new List<Mnemonic> {},
					[Mnemonic.EOF] = new List<Mnemonic> {},
				},
				[Mnemonic.F] = new Dictionary<Mnemonic, List<Mnemonic>> {
					[Mnemonic.i]  = new List<Mnemonic> { Mnemonic.i },
					[Mnemonic.LB] = new List<Mnemonic> { Mnemonic.RB, Mnemonic.E, Mnemonic.LB },
				},
			};

			// 非终结符集合
			nonterminalSet = new HashSet<Mnemonic> {
				Mnemonic.E,
				Mnemonic.E1,
				Mnemonic.T,
				Mnemonic.T1,
				Mnemonic.F
			};

			// 终结符集合
			terminalSet = new HashSet<Mnemonic> {
				Mnemonic.i,
				Mnemonic.PL,
				Mnemonic.MI,
				Mnemonic.MU,
				Mnemonic.DI,
				Mnemonic.LB,
				Mnemonic.RB,
				Mnemonic.EOF
			};
		}

		// 获取下一个符号
		private Mnemonic GetNext()
		{
			// 若输入流已空却还需读取，则匹配不成功
			if (bEndOfStream)
			{
				throw new InvalidExpressionException("Illegal input at line " + sgnCnt + " around word \"" + symbolStack.Peek() + "\".");
			}

			// 若没有下一个单词，则遇到文件尾，压入END状态
			if (!lexicalAnalyzer.HasNext)
			{
				return Mnemonic.EOF;
			}

			while (!lexicalAnalyzer.Success)
			{
				currOperand = lexicalAnalyzer.GetNext();
			}

			++sgnCnt;
			lexicalAnalyzer.Success = false;

			if (currOperand.Attribute == Mnemonic.REAL ||
			    currOperand.Attribute == Mnemonic.INT ||
			    currOperand.Attribute == Mnemonic.ID)
			{
				currOperand.Attribute = Mnemonic.Operand;
				return Mnemonic.i;
			}
			else
			{
				return currOperand.Attribute;
			}
		}

		private void PushExp(Mnemonic nonterminal, Mnemonic terminal)
		{
			if (!nonterminalSet.Contains(nonterminal) 
			    || !terminalSet.Contains(terminal)
			    || !PATable[nonterminal].ContainsKey(terminal))
			{
				throw new InvalidExpressionException("Illegal input at line "+sgnCnt+" around word \""+terminal+"\".");
			}
			else
			{
				foreach (var sgn in PATable[nonterminal][terminal])
				{
					symbolStack.Push(sgn);
				}
			}
		}

		// 执行语法分析
		public void Run()
		{
			Symbol currSymbol = GetNext();
			symbolStack.Push(Mnemonic.EOF);
			symbolStack.Push(Mnemonic.E);

			while (symbolStack.Count != 0)
			{
				if (currSymbol == symbolStack.Peek())
				{
					symbolStack.Pop();
					if (currSymbol != Mnemonic.EOF)
					{
						semanticAnalyzer.Send(currOperand);
					}

					// 符号栈非空则继续读取
					if (symbolStack.Count != 0)
					{
						try {
							currSymbol = GetNext();
						}
						catch (InvalidExpressionException ieException) {
							Console.WriteLine(ieException.Message);
							return;
						}
					}
					// 符号栈空则匹配成功
					else
					{
						semanticAnalyzer.Finish();
						return;
					}
				}
				else
				{
					try
					{
						PushExp(symbolStack.Pop(), currSymbol);
					}
					catch (InvalidExpressionException ieException)
					{
						Console.WriteLine(ieException.Message);
						return;
					}
				}
			}
		}

	}
}
