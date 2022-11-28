namespace Analyzer {

	using Operator = Operand;

	internal class SemanticAnalyzer
	{
		// 唯一实例
		private static SemanticAnalyzer? _uniqueInstance = null;

		public static SemanticAnalyzer Instance
		{
			get
			{
				_uniqueInstance ??= new SemanticAnalyzer();
				return _uniqueInstance;
			}
		}

		// 构造函数
		private SemanticAnalyzer()
		{
			operatorStack = new Stack<Operator>();
			operandStack = new Stack<Operand>();
			postfixExpr = new Queue<Operand>();
		}

		// 临时变量计数
		private int tmpVarCnt = 0;

		// 运算符栈 - 转后缀表达式
		private Stack<Operator> operatorStack;

		// 运算数栈 - 计算后缀表达式
		private Stack<Operand> operandStack;

		// 后缀表达式
		private Queue<Operand> postfixExpr;

		// 生成临时变量名称
		public String NewTemp()
		{
			return "T" + (++tmpVarCnt);
		}

		// 输出四元式
		public void Generate(Operator opr, Operand? opd1, Operand? opd2, Operand result)
		{
			Analyzer.streamWriter.WriteLine("("+opr+", "+opd1+", "+opd2+", "+result+")");
		}

		// 将运算对象送入后缀表达式
		public void Send(Operand opd)
		{
			if (opd.Attribute == Mnemonic.Operand)
			{
				postfixExpr.Enqueue(opd);
			}
			else if (opd.Attribute == Mnemonic.RB)
			{
				while (operatorStack.Peek().Attribute != Mnemonic.LB)
				{
					postfixExpr.Enqueue(operatorStack.Pop());
					CalcNext();
				}
				operatorStack.Pop();
			}
			else
			{
				while (operatorStack.Count != 0 && opd < operatorStack.Peek())
				{
					postfixExpr.Enqueue(operatorStack.Pop());
					CalcNext();
				}
				operatorStack.Push(opd);
			}
		}

		// 向后处理一个运算符
		private void CalcNext()
		{
			while (postfixExpr.Peek().Attribute == Mnemonic.Operand)
			{
				operandStack.Push(postfixExpr.Dequeue());
			}

			Operator opr = postfixExpr.Dequeue();
			Operand	opd2 = operandStack.Pop(), 
				opd1 = operandStack.Pop(),
				result = new Operand(Mnemonic.Operand, NewTemp());
			operandStack.Push(result);

			Generate(opr, opd1, opd2, result);
		}

		// 收尾
		public void Finish()
		{
			// 将运算符栈中元素全部送入后缀表达式
			while (operatorStack.Count != 0)
			{
				postfixExpr.Enqueue(operatorStack.Pop());
				CalcNext();
			}
		}
	}
}
