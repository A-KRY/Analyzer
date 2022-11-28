namespace Analyzer {

	// 数值识别状态
	enum NA_STATE {
		BGN,	// begin            初态
		US,		// unsigned         无符号数
		DF,		// decimal fraction 十进制小数
		FR,		// fraction         小数
		EXP,	// exponent         指数
		IE,		// integer exponent 整指数
		RIE,	// residual int exp 余留整指数
		END,	// end state        末态
	};

	// 数值字符类别
	enum NA_TYPE {
		INVALID = 0,  //          非法字符
		DIGIT = 1,  // [0-9]    数字
		POINT = 2,  // .        小数点
		EXP = 3,  // E, e     乘方
		PLUS = 4,  // +        加号
		MINUS = 5,  // -        减号
		ALPHA = 6,  // E, e 以外的其他字母
		BLANK,
		NULL,
	};

	internal class NumericAnalyzer
	{
		// 唯一实例
		private static NumericAnalyzer? uniqueInstance = null;

		// 获取单例
		public static NumericAnalyzer Instance {
			get {
				uniqueInstance ??= new NumericAnalyzer();
				return uniqueInstance;
			}
		}

		// 构造函数
		private NumericAnalyzer() {

		}

		public delegate Char GetCharHandler();
		public delegate void CATHandler(Char currCh);
		public delegate void RetractHandler();

		private static GetCharHandler GetChar;
		private static CATHandler CAT;
		private static RetractHandler Retract;

		// 设置委托
		public static void SetDelegate(GetCharHandler getchar, CATHandler cat, RetractHandler retract) {
			GetChar = new GetCharHandler(getchar);
			CAT = new CATHandler(cat);
			Retract = new RetractHandler(retract);
		}

		// n: 尾数位 p: 指数 sgn: 指数符号 w: 基数 d:当前字符数值
		private Int32 n = 0, p = 0, sgn = 0, w = 0, d =0;
		private Double value = 0.0;
		private NA_STATE currState;
		private Mnemonic numericType;

		private char tmp;

		// 识别数值字符类型
		private NA_TYPE NA_GetChType()
		{
			char ch = GetChar();
			CAT(ch);
			tmp = ch;

			if (Char.IsDigit(ch)) {
				d = ch - '0';
				return NA_TYPE.DIGIT;
			}
			else if ('.' == ch) {
				return NA_TYPE.POINT;
			}
			else if ('e' == ch || 'E' == ch) {
				return NA_TYPE.EXP;
			}
			else if ('+' == ch) {
				return NA_TYPE.PLUS;
			}
			else if ('-' == ch) {
				return NA_TYPE.MINUS;
			}
			else if (Char.IsLetter(ch)) {
				return NA_TYPE.ALPHA;
			}
			else if (' ' == ch) {
				return NA_TYPE.BLANK;
			}
			else if ('\0' == ch)
			{
				return NA_TYPE.NULL;
			}
			else {
				return NA_TYPE.INVALID;
			}
		}

		private void NA_Execute(NA_TYPE chType)
		{
			switch (currState) {
				case NA_STATE.BGN:   // 0
					switch (chType) {
						case NA_TYPE.DIGIT:
							currState = NA_STATE.US;
							n = 0; p = 0; sgn = 1; w = w*10+d;
							break;
						case NA_TYPE.POINT:
							currState = NA_STATE.FR;
							w = d; p = 0; sgn = 1; n = 0;
							break;
						case NA_TYPE.BLANK:
							currState = NA_STATE.END;
							Retract();
							numericType = Mnemonic.INT;
							value = d;
							break;
						case NA_TYPE.ALPHA:
							currState = NA_STATE.END;
							Retract();
							break;
						default:
							currState = NA_STATE.END;
							Retract();
							numericType = Mnemonic.INT;
							value = w;
							//throw new Exception("数值识别失败");
							break;
					}
					break;
				case NA_STATE.US:   // 1
					switch (chType) {
						case NA_TYPE.DIGIT:
							currState = NA_STATE.US;
							w = w * 10 + d;
							break;
						case NA_TYPE.POINT:
							currState = NA_STATE.DF;
							break;
						case NA_TYPE.EXP:
							currState = NA_STATE.EXP;
							break;
						default:
							currState = NA_STATE.END;
							Retract();
							numericType = Mnemonic.INT;
							value = w;
							break;
					}
					break;
				case NA_STATE.DF:   // 2
					switch (chType) {
						case NA_TYPE.DIGIT:
							currState = NA_STATE.DF;
							++n; w = w * 10 + d;
							break;
						case NA_TYPE.EXP:
							currState = NA_STATE.EXP;
							break;
						default:
							currState = NA_STATE.END;
							Retract();
							numericType = Mnemonic.REAL;
							value = w * Math.Pow(10, sgn * p - n);
							break;
					}
					break;
				case NA_STATE.FR:   // 3
					switch (chType) {
						case NA_TYPE.DIGIT:
							currState = NA_STATE.DF;
							++n; w = w * 10 + d;
							break;
						default:
							currState = NA_STATE.END;
							throw new Exception("数值识别失败");
					}
					break;
				case NA_STATE.EXP:   // 4
					switch (chType) {
						case NA_TYPE.DIGIT:
							currState = NA_STATE.RIE;
							p = p * 10 + d;
							break;
						case NA_TYPE.PLUS:
							currState = NA_STATE.IE;
							break;
						case NA_TYPE.MINUS:
							currState = NA_STATE.IE;
							sgn = -1;
							break;
						default:
							currState = NA_STATE.END;
							throw new Exception("数值识别失败");
					}
					break;
				case NA_STATE.IE:   // 5
					switch (chType) {
						case NA_TYPE.DIGIT:
							currState = NA_STATE.RIE;
							p = p * 10 + d;
							break;
						default:
							currState = NA_STATE.END;
							throw new Exception("数值识别失败");
					}
					break;
				case NA_STATE.RIE:
					switch (chType) {
						case NA_TYPE.DIGIT:
							currState = NA_STATE.RIE;
							p = p * 10 + d;
							break;
						default:
							currState = NA_STATE.END;
							Retract();
							numericType = Mnemonic.REAL;
							value = w * Math.Pow(10, sgn * p - n);
							break;
					}
					break;
				default:
					break;
			}
		}

		public void Run(Int32 inVal, out String valStr, out Mnemonic numType)
		{
			currState = NA_STATE.BGN;
			numericType = Mnemonic.INVALID;
			w = d = inVal;
			while (currState != NA_STATE.END)
			{
				NA_Execute(NA_GetChType());
			}
			
			numType = numericType;
			if (numericType == Mnemonic.INT)
			{
				valStr = Convert.ToInt32(value).ToString();
			}
			else
			{
				Int32 cnt = n - sgn * p;
				if (cnt < 0)
				{
					cnt = 0;
				}

				valStr = String.Format("{0:F" + cnt + "}", value);
			} 
		}
	}
}
