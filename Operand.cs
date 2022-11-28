using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace Analyzer {

	using Operator = Operand;
	internal class Operand
	{
		// 符号优先级，行是否小于列
		private static Dictionary<Mnemonic, Dictionary<Mnemonic, bool>> priorityTable;

		private void InitPriorityTable()
		{
			priorityTable = new Dictionary<Mnemonic, Dictionary<Mnemonic, bool>> {
				[Mnemonic.PL] = new Dictionary<Mnemonic, bool> {
					[Mnemonic.PL] = true,
					[Mnemonic.MI] = true,
					[Mnemonic.MU] = true,
					[Mnemonic.DI] = true,
					[Mnemonic.LB] = false
				},

				[Mnemonic.MI] = new Dictionary<Mnemonic, bool> {
					[Mnemonic.PL] = true,
					[Mnemonic.MI] = true,
					[Mnemonic.MU] = true,
					[Mnemonic.DI] = true,
					[Mnemonic.LB] = false
				},

				[Mnemonic.MU] = new Dictionary<Mnemonic, bool> {
					[Mnemonic.PL] = false,
					[Mnemonic.MI] = false,
					[Mnemonic.MU] = true,
					[Mnemonic.DI] = true,
					[Mnemonic.LB] = false
				},

				[Mnemonic.DI] = new Dictionary<Mnemonic, bool> {
					[Mnemonic.PL] = false,
					[Mnemonic.MI] = false,
					[Mnemonic.MU] = true,
					[Mnemonic.DI] = true,
					[Mnemonic.LB] = false
				},

				[Mnemonic.LB] = new Dictionary<Mnemonic, bool> {
					[Mnemonic.PL] = false,
					[Mnemonic.MI] = false,
					[Mnemonic.MU] = false,
					[Mnemonic.DI] = false,
					[Mnemonic.LB] = false
				}
			};
		}

		public Operand()
		{
			name = "";
			InitPriorityTable();
		}

		public Operand(Mnemonic attribute, String name)
		{
			this.attribute = attribute;
			this.name = name;
			InitPriorityTable();
		}

		// 运算对象属性
		private Mnemonic attribute;

		public Mnemonic Attribute
		{
			get => attribute;
			set => attribute = value;
		}

		// 运算对象字符
		private String name;

		public String Name
		{
			get => name;
			set => name = value;
		}

		// 重载 toString() 方法
		public override string ToString()
		{
			return name;
		}

		// 重载比较方法

		public static bool operator> (Operand op1, Operand op2)
		{
			return !priorityTable[op1.attribute][op2.attribute];
		}

		public static bool operator<(Operand op1, Operand op2)
		{
			return priorityTable[op1.attribute][op2.attribute];
		}
	}
}
