using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace MPS.IIC.Script
{
    class Calculator
    {
        private static Dictionary<string, int> dic = new Dictionary<string, int>
        {
            { "|", 1 }, { "&", 1 },
            { ">", 3 }, { "<", 3 }, { ">=", 3 }, { "<=", 3 }, { "==", 3 },
            { "+", 4 }, { "-", 4 },
            { "*", 5 }, { "/", 5 },
            { "^", 6},
            { "LN", 7},
            { "(", 100}, { ")", 100},
        };

        //RPN and calculate
        /*是否为纯数字。正则表达式实现*/
        public static bool isNumber(string tmp)
        {
            return Regex.IsMatch(tmp, @"[0-9]+[.]{0,1}[0-9]*");
        }


        /*是否为需拆分的运算符+-*^/() */
        public static bool isOp(string tmp)
        {
            bool bRet = false;
            switch (tmp)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "^":
                case "ln":
                case "(":
                case ")":
                case "&":
                case "|":
                case ">":
                case "<":
                case ">=":
                case "<=":
                case "==":
                    bRet = true;
                    break;
                default:
                    bRet = false;
                    break;
            }
            return bRet;
        }

        public static bool isFunc(string tmp)
        {
            bool bRet = false;
            switch (tmp)
            {
                case "LN":
                    bRet = true;
                    break;
                default:
                    bRet = false;
                    break;
            }
            return bRet;
        }


        //比较运算符及函数优先级。函数视作运算符进行操作
        //返回值：1 表示 大于，-1 表示 小于，0 表示 相等   
        public static int compOper(string op1, string op2)
        {
            int iRet = 0;
            if (dic[op1] > dic[op2])
                iRet = 1;
            else if (dic[op1] < dic[op2])
                iRet = -1;
            else
                iRet = 0;
            return iRet;
        }

        //运算符、函数求值
        public static double calValue(string op, string val1, string val2)
        {
            double dRet = 0.0d;
            switch (op)
            {
                case "+":
                    dRet = double.Parse(val1) + double.Parse(val2);
                    break;
                case "-":
                    dRet = double.Parse(val1) - double.Parse(val2);
                    break;
                case "*":
                    dRet = double.Parse(val1) * double.Parse(val2);
                    break;
                case "/":
                    if (double.Parse(val2) != 0)
                        dRet = double.Parse(val1) / double.Parse(val2);
                    else
                        throw new DivideByZeroException();
                    break;
                case "^":
                    dRet = Math.Pow(double.Parse(val1), double.Parse(val2));
                    break;
                case "&":
                    dRet = int.Parse(val1) & int.Parse(val2);
                    break;
                case "|":
                    dRet = int.Parse(val1) | int.Parse(val2);
                    break;
                case ">":
                    dRet = double.Parse(val1) > double.Parse(val2) ? 1 : 0;
                    break;
                case "<":
                    dRet = double.Parse(val1) < double.Parse(val2) ? 1 : 0;
                    break;
                case ">=":
                    dRet = double.Parse(val1) >= double.Parse(val2) ? 1 : 0;
                    break;
                case "<=":
                    dRet = double.Parse(val1) <= double.Parse(val2) ? 1 : 0;
                    break;
                case "==":
                    dRet = double.Parse(val1) == double.Parse(val2) ? 1 : 0;
                    break;
                default:
                    break;
            }
            return dRet;
        }
        public static double calValue(string op, string val1)
        {
            double dRet = 0.0d;
            switch (op)
            {
                case "LN":
                    dRet = Math.Log(double.Parse(val1));
                    break;
                default:
                    break;
            }
            return dRet;
        }


        //按照=+-*^/()&|分隔出元素
        public static string splitFunc(string tmp)
        {
            string sRet = tmp;
            sRet = sRet.Replace("=", "\n=\n");
            sRet = sRet.Replace("+", "\n+\n");
            sRet = sRet.Replace("-", "\n-\n");
            sRet = sRet.Replace("*", "\n*\n");
            sRet = sRet.Replace("/", "\n/\n");
            sRet = sRet.Replace("^", "\n^\n");
            sRet = sRet.Replace("(", "\n(\n");
            sRet = sRet.Replace(")", "\n)\n");
            sRet = sRet.Replace("&", "\n&\n");
            sRet = sRet.Replace("|", "\n|\n");
            sRet = sRet.Replace(">", "\n>\n");
            sRet = sRet.Replace("<", "\n<\n");
            sRet = sRet.Replace(">=", "\n>=\n");
            sRet = sRet.Replace("<=", "\n<=\n");
            sRet = sRet.Replace("==", "\n==\n");
            return sRet;
        }

        //中缀表达式转后缀表达式,tmp为已经添加分隔符的中缀表达式字符串    
        public static string midToRPN(string tmp)
        {
            string sRet = "";                                               //返回值
            string[] strArr = splitFunc(tmp.ToUpper()).Split('\n');         //字符串数组，存放分隔后的中缀表达式元素
            Stack<string> strStk = new Stack<string>();                     //栈，用于临时存放运算符和函数名
            for (int i = 0; i < strArr.Length; i++)
            {
                if (string.IsNullOrEmpty(strArr[i]))                        //分隔后为空的元素剔除
                    continue;
                else if (isNumber(strArr[i]))                     //纯数字直接入队列
                    sRet += strArr[i] + ',';
                else if (isFunc(strArr[i]))                   //一元函数名直接入栈
                    strStk.Push(strArr[i]);
                else if (isOp(strArr[i]))                         //运算符特殊处理
                {
                    if (strStk.Count != 0 && strStk.Peek() == "(" && strArr[i] != ")")      //栈不为空，最上层为"("，则运算符直接入栈
                    {
                        strStk.Push(strArr[i]);
                    }
                    else if (strStk.Count != 0 && strArr[i] == ")")                         //栈不为空，遇")"则pop至"("为止
                    {
                        while (strStk.Peek() != "(")
                            sRet += strStk.Pop() + ',';
                        strStk.Pop();
                    }
                    else if (strStk.Count != 0 && compOper(strArr[i], strStk.Peek()) == -1)
                    {                                                                       //栈不为空，运算符优先级小
                        while (strStk.Count != 0 && strStk.Peek() != "(" && compOper(strArr[i], strStk.Peek()) == -1)
                            sRet += strStk.Pop() + ',';                                     //则一直pop【存疑】
                        if (strStk.Count != 0 && strStk.Peek() != "(")                                              //pop至优先级不小于栈顶运算符则交换位置
                            sRet += strStk.Pop() + ',';                                     //先pop
                        strStk.Push(strArr[i]);                                             //再push
                    }
                    else if (strStk.Count != 0 && compOper(strArr[i], strStk.Peek()) == 0)
                    {                                                                       //运算符优先级相同，先pop再push
                        sRet += strStk.Pop() + ',';
                        strStk.Push(strArr[i]);
                    }
                    else if (strStk.Count != 0 && compOper(strArr[i], strStk.Peek()) == 1)
                    {                                                                       //运算符优先级大，直接入栈
                        strStk.Push(strArr[i]);
                    }
                    else                                                                    //其他情况，入栈【存疑】
                    {
                        strStk.Push(strArr[i]);
                    }
                }
            }
            while (strStk.Count > 0)                //最后栈内元素全部pop出
            {
                sRet += strStk.Pop() + ',';
            }
            return sRet;                            //返回后缀表达式
        }

        //根据传入的后缀表达式，求值, tmp为含,分隔符的后缀表达式
        public static double calRPN(string tmp)
        {
            double dRet = 0.0d;
            string[] strArr = tmp.Split(',');
            Stack<string> strStk = new Stack<string>();
            for (int i = 0; i < strArr.Length - 1; i++)
            {
                if (isNumber(strArr[i]))                //纯数字入栈
                    strStk.Push(strArr[i]);
                else if (isOp(strArr[i]))               //二元运算符，pop两个元素，计算值后压入栈
                {
                    var val1 = strStk.Pop();
                    var val2 = strStk.Pop();
                    var res = calValue(strArr[i], val2, val1);
                    strStk.Push(res.ToString());
                }
                else if (isFunc(strArr[i]))         //一元函数名，pop一个元素，计算后压入栈
                    strStk.Push(calValue(strArr[i], strStk.Pop()).ToString());

            }
            dRet = double.Parse(strStk.Pop());          //取最后栈中元素作为结果值
            if (strStk.Count != 0)
            {
                throw new Exception("Invalid operation");
            }
            return dRet;
        }

        public static double calculate(string str)
        {
            double res;
            var rpnstr = midToRPN(str);
            res = calRPN(rpnstr);
            return res;           
        }


    }
}
