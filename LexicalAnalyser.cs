namespace Analyzer {

	// 助记符
	enum Mnemonic {
		// 关键字
		INVALID,
		IF,
		ELSE,
		FOR,
		DO,
		BEGIN,
		END,

		// 单词类型
		ID ,	// Identifier
		INT,	// Integer
		REAL,	// Real number
		LT,		// Less than		<
		LE,		// Less equal		<=
		EQ,		// Equal			=
		NE,		// Not equal		<>
		GT,		// Greater than	>
		GE,		// Greater equal	>=
		IS,		// Is				:=
		PL,		// Plus				+
		MI,		// Minus			-
		MU,		// Multiply			*
		DI,		// Divide			/
		LB,		// Left bracket		(
		RB,		// Right bracket	)

		// 文法符号
		E,
		E1,
		T,
		T1,
		F,
		i,
		EOF,

		Operand,
	};

	// 词法分析自动机状态
	enum LA_STATE {
		BGN = 0,    // 初态
		IAD = 1,    // Input alpha and digit of ID
		OID = 2,    // Output ID
		IN = 3,		// Input numeric
		ON = 4,     // Output numeric
		ILT = 5,    // Input less than       <
		OLE = 6,    // Output less equal    <=
		ONE = 7,    // Output not equal     <>
		OLT = 8,    // Output less than     <
		OEQ = 9,    // Output equal         =
		IGT = 10,   // Input greater than   >
		OGE = 11,   // Output greater equal >=
		OGT = 12,   // Output greater than  >
		IIS = 13,   // Input is             :=
		OIS = 14,   // Output is            :=
		OPL = 15,   // Output plus          +
		OMI = 16,   // Output minus         -
		OMU = 17,   // Output multiply      *
		ODI = 18,   // Output divide        /
		OLB = 19,	// Output left bracket  (
		ORB = 20,	// Output right bracket )
		END,        // 末态
	};

	// 词法字符类别
	enum LA_TYPE {
		INVALID = 0,// Invalid
		DIGIT = 1,  // Digit
		ALPHA = 2,  // Alpha
		LT = 3,		// Less than		<
		GT = 4,		// Greater than		>
		EQ = 5,		// Equal			=
		CL = 6,		// Colon			:
		PL = 7,		// Plus				+
		MI = 8,		// Minus			-
		MU = 9,		// Multiply			*
		DI = 10,	// Divide			/
		LB = 11,	// Left bracket		(
		RB = 12,	// Right bracket	)
		BLANK,
		NULL
	};

	internal class LexicalAnalyzer
	{
		// 唯一实例
		private static LexicalAnalyzer uniqueInstance = null;

		// 获取单例
		public static LexicalAnalyzer Instance {

			get {
				uniqueInstance ??= new LexicalAnalyzer();
				return uniqueInstance;
			}
		}

		// 构造函数
		private LexicalAnalyzer() {
			InitKeywordTable();
			NumericAnalyzer.SetDelegate(GetChar, CAT, Retract);
		}

		// 当前读入的部分字符
		private String? buffer = String.Empty;

		public bool HasNext
		{
			get
			{
				return !(Analyzer.streamReader.EndOfStream && currIndex == buffer.Length-1);
			}
		}

		// 当前指向字符的角标
		private Int32 currIndex = 0;

		// 当前行数
		private Int32 lineCnt = 0;

		// 当前单词的各个字符
		private String token = String.Empty;

		// 输出的单词
		private Operand word;

		// 关键字表
		private Dictionary<String, Mnemonic> KeywordTable;

		// 数值识别器
		private NumericAnalyzer numericAnalyzer = NumericAnalyzer.Instance;

		// 数值及类型
		private String value;
		private Mnemonic numType;

		// 读到非空单词
		private bool success;

		public bool Success
		{
			get => success;
			set => success = value;
		}

		// 初始化 KeywordTable
		private void InitKeywordTable()
		{
			KeywordTable = new Dictionary<String, Mnemonic> {
				["if"] = Mnemonic.IF,
				["else"] = Mnemonic.ELSE,
				["for"] = Mnemonic.FOR,
				["do"] = Mnemonic.DO,
				["begin"] = Mnemonic.BEGIN,
				["end"] = Mnemonic.END
			};
		}

		// 读入 buffer
		private void ReadBuffer()
		{
			do
			{
				buffer = Analyzer.streamReader.ReadLine();
			} while (buffer is not null && buffer.Trim() == String.Empty);

			if (buffer is not null)
			{
				++lineCnt;
			}
		}

		// 将当前字符送入 currCh 并更新 currIndex
		private Char GetChar()
		{
			if (String.IsNullOrEmpty(buffer) || currIndex == buffer.Length) {
				ReadBuffer();
				if (buffer is not null || !Analyzer.streamReader.EndOfStream)
				{
					currIndex = 0;
					buffer += ' ';
				}
			}

			if (String.IsNullOrEmpty(buffer))
			{
				return '\0';
			}

			buffer.Append(' ');
			
			Char currCh = buffer[currIndex];
			++currIndex;

			return currCh;
		}

		// 将 currCh 拼入 token
		void CAT(char currCh)
		{
			token += currCh;
		}

		// 判断 token 的关键字类型
		Mnemonic LookUp()
		{
			if (KeywordTable.ContainsKey(token))
			{
				return KeywordTable[token];
			}
			else
			{
				return Mnemonic.INVALID;
			}
		}

		// 回退一个字符
		private void Retract()
		{
			--currIndex;
			token = token.Remove(token.Length - 1);
		}

		// 输出单词的二元表达式
		private void Out(Mnemonic type, String str = "")
		{
			word = new Operand(type, str);
		}


		// 识别单词字符类型
		private LA_TYPE LA_GetChType()
		{
			
			char ch = GetChar();
			CAT(ch);
			if (Char.IsDigit(ch)) {
				return LA_TYPE.DIGIT;
			}
			else if (Char.IsLetter(ch)) {
				return LA_TYPE.ALPHA;
			}
			else if ('<' == ch) {
				return LA_TYPE.LT;
			}
			else if ('>' == ch) {
				return LA_TYPE.GT;
			}
			else if ('=' == ch) {
				return LA_TYPE.EQ;
			}
			else if (':' == ch) {
				return LA_TYPE.CL;
			}
			else if ('+' == ch) {
				return LA_TYPE.PL;
			}
			else if ('-' == ch) {
				return LA_TYPE.MI;
			}
			else if ('*' == ch) {
				return LA_TYPE.MU;
			}
			else if ('/' == ch) {
				return LA_TYPE.DI;
			}
			else if ('(' == ch)
			{
				return LA_TYPE.LB;
			}
			else if (')' == ch)
			{
				return LA_TYPE.RB;
			}
			else if (' ' == ch) {
				return LA_TYPE.BLANK;
			}
			else if ('\0' == ch)
			{
				return LA_TYPE.NULL;
			}
			else {
				return LA_TYPE.INVALID;
			}
		}

		// 执行单词识别
		private void LA_Execute(ref LA_STATE currState, LA_TYPE chType)
		{
			switch (currState) {
				case LA_STATE.BGN:
					switch (chType) {
						case LA_TYPE.ALPHA:
							currState = LA_STATE.IAD;
							break;
						case LA_TYPE.DIGIT:
							currState = LA_STATE.IN;
							Retract();
							break;
						case LA_TYPE.LT:
							currState = LA_STATE.ILT;
							break;
						case LA_TYPE.EQ:
							currState = LA_STATE.OEQ;
							break;
						case LA_TYPE.GT:
							currState = LA_STATE.IGT;
							break;
						case LA_TYPE.CL:
							currState = LA_STATE.IIS;
							break;
						case LA_TYPE.PL:
							currState = LA_STATE.OPL;
							break;
						case LA_TYPE.MI:
							currState = LA_STATE.OMI;
							break;
						case LA_TYPE.MU:
							currState = LA_STATE.OMU;
							break;
						case LA_TYPE.DI:
							currState = LA_STATE.ODI;
							break;
						case LA_TYPE.LB:
							currState = LA_STATE.OLB;
							break;
						case LA_TYPE.RB:
							currState = LA_STATE.ORB;
							break;
						case LA_TYPE.BLANK:
							currState = LA_STATE.END;
							token = String.Empty;
							break;
						case LA_TYPE.NULL:
							currState = LA_STATE.END;
							break;
						default:
							currState = LA_STATE.END;
							LA_Fail();
							break;
					}
					break;
				case LA_STATE.IAD:
					switch (chType) {
						case LA_TYPE.ALPHA:
							currState = LA_STATE.IAD;
							break;
						case LA_TYPE.DIGIT:
							currState = LA_STATE.IAD;
							break;
						default:
							currState = LA_STATE.OID;
							Retract();
							break;
					}
					break;
				case LA_STATE.OID: {
					currState = LA_STATE.END;
					Retract();
					Mnemonic TYPE = LookUp();
					if (Mnemonic.INVALID == TYPE) {
						Out(Mnemonic.ID, token);
						success = true;
					}
					else {
						Out((Mnemonic)TYPE);
						success = true;
					}
					token = String.Empty;
				}
				break;
				case LA_STATE.IN:
					currState = LA_STATE.ON;
					numType = Mnemonic.INVALID;
					numericAnalyzer.Run(Convert.ToInt32(token), out value, out numType);
					break;
				case LA_STATE.ON: {
					currState = LA_STATE.END;
					Retract();
					if (Mnemonic.INT == numType) {
						Out(Mnemonic.INT, value);
						success = true;
					}
					else if (Mnemonic.REAL == numType) {
						Out(Mnemonic.REAL, value);
						success = true;
					}
					else {
						LA_Fail();
					}
					token = String.Empty;
				}
				break;
				case LA_STATE.ILT:
					switch (chType) {
						case LA_TYPE.EQ:
							currState = LA_STATE.OLE;
							break;
						case LA_TYPE.GT:
							currState = LA_STATE.ONE;
							break;
						default:
							currState = LA_STATE.OLT;
							Retract();
							break;
					}
					break;
				case LA_STATE.OLE:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.LE, "<=");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.ONE:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.NE, "<>");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.OLT:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.LT, "<");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.OEQ:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.EQ, "=");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.IGT:
					switch (chType) {
						case LA_TYPE.EQ:
							currState = LA_STATE.OGE;
							break;
						default:
							currState = LA_STATE.OGT;
							break;
					}
					break;
				case LA_STATE.OGE:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.GE, ">=");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.OGT:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.GT, ">");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.IIS:
					switch (chType) {
						case LA_TYPE.EQ:
							currState = LA_STATE.OIS;
							break;
						default:
							currState = LA_STATE.END;
							LA_Fail();
							break;
					}
					break;
				case LA_STATE.OIS:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.IS, ":=");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.OPL:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.PL, "+");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.OMI:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.MI, "-");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.OMU:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.MU, "*");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.ODI:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.DI, "/");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.OLB:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.LB, "(");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.ORB:
					currState = LA_STATE.END;
					Retract();
					Out(Mnemonic.RB, ")");
					success = true;
					token = String.Empty;
					break;
				case LA_STATE.END:
					break;
			}
		}

		// 识别失败
		private void LA_Fail()
		{
			Console.WriteLine("ERROR at Line " + lineCnt + 
			                    " Column " + (currIndex+1)+" : 单词 \"" + 
			                    token + "\" 非法");
		}

		public Operand GetNext()
		{
			if (Analyzer.streamReader is null)
			{
				throw new IOException("Input file not opened.");
			}
			
			LA_STATE currState = LA_STATE.BGN;
			while (currState != LA_STATE.END) 
			{
				LA_Execute(ref currState, LA_GetChType());
			}

			return this.word;
		}

	}
}
