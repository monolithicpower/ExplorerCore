using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LanguageImprove
{
    class Program
    {
        //static async Task Main(string[] args)
        static void Main(string[] args)
        {
            //这个项目为了学习新的C#语法而建立。
            //概念是啥不重要，重要是怎么写怎么用
            #region C# 7.0   &    7.3

            #region 元组和弃元
            //可以用来声明参数，当作一个匿名类使用
            (string Alpha, string Beta) namedLetters = ("a", "b");
            Console.WriteLine($"输出了元组的两个属性：{namedLetters.Alpha}, {namedLetters.Beta}");
            //可以在构造函数中快速赋值使用  详情见 Class 元组
            var tmp元组 = new 元组(1, 2);
            //也可以在方法返回值中使用，当作一个匿名类,或者直接赋值变量,若变量名是 _ 则代表弃元（放弃参数）
            var (a, b, c) = tmp元组.元组方法();
            var (d, _, e) = tmp元组.元组方法();
            var f = tmp元组.元组方法();
            Console.WriteLine($"{f.a}");
            #endregion

            #region 模式匹配
            //这个就比较简单了，直接看代码吧
            object inputA = 1;
            string inputB = "2";
            if (inputA is int count)
            {
                Console.WriteLine($"此时 变量count以及用inputA赋值了 :{count}");
            }
            //牛逼的来了，在switch中使用：
            static int SumPositiveNumbers(IEnumerable<object> sequence)
            {
                int sum = 0;
                foreach (var i in sequence)
                {
                    switch (i)//此时i 的类型是object
                    {
                        case 0://常量模式，即当i==0时，匹配成功
                            break;
                        case IEnumerable<int> childSequence://模式匹配，当i是IEnumerate<int> 是， 赋值给变量 childSequence 并且进入此处
                            {
                                foreach (var item in childSequence)
                                    sum += (item > 0) ? item : 0;
                                break;
                            }
                        case int n when n > 0: //模式匹配， 当 i是int类型时，赋值给变量 n ,  并且n>0 时，此处匹配成功。
                            sum += n;
                            break;
                        case null:
                            throw new NullReferenceException("Null found in sequence");
                        default:
                            throw new InvalidOperationException("Unrecognized type");
                    }
                }
                return sum;
            }
            #endregion
            //异步 Main  方法   看看这个 Main 方法  (被注释的部分）
            //本地函数  就是方法里面套方法， 抬头往上看。 SumPositiveNumbers
            //现在属性也可以用lambda表达式了，仔细看一下class  元组  里面的属性A
            //给引用类型赋值default终于不用括号了。
            string sdw = default;
            Func<string, bool> whereClause = default;
            object obj = default;
            int valuetype = default;
            #region 数字文本优化
            //可以用二进制来表达数字了
            var binaryNumber = 0b1101_0101;
            var bn = 0b_11111111;// _ 可以出现在0b后的任意地方，方便查看是多少。
            var whatA_ = 123_456_789.123_456;//double int 长一点的数字都可以拿  _  分割了
            #endregion
            //out 可以直接赋值一个变量了
            int.TryParse(sdw, out var intSdw);
            #region ref可以当指针声明了
            //声明值变量的时候可以在变量名前添加ref了，那这个变量就已经变成了引用类型，可以赋值ref指针，也可以赋值值类型
            var ints = new int[2] { 1, 2 };
            static ref int GetFirst(int[] inputInts) => ref inputInts[0];
            ref int ri = ref GetFirst(ints);
            ref int ri2 = ref ints[1];
            ri = 3;
            ri2 = 4;
            Console.WriteLine($"这个时候 ints 变量里面的数字都变化了： {ints[0]}  ;  {ints[1]}");
            #endregion
            //in 修饰符（和 ref out 同种用法）  in修饰符是确保入参不会被变化
            static void inParaNotChange(in 元组 inPara, in int inPara2)
            {
                inPara.A = 1234;
                //下面两行代码就报错了，我去这玩意吊毛意义没有
                //inPara2 = 1234;
                //inPara = new 元组();
            }
            inParaNotChange(tmp元组, ri);
            Console.WriteLine($"此时 tmp元组.A 的值 ： {tmp元组.A}");
            //stackalloc声明数组
            unsafe
            {
                var pArr = stackalloc int[3] { 1, 2, 3 };
                int* pArr2 = stackalloc int[] { 1, 2, 3 };
            }
            Span<int> arr = stackalloc[] { 1, 2, 3 };
            //TBD  Span<int> 用法 干嘛的
            #endregion

            #region C# 8.0
            #region switch 表达式
            //感觉好垃圾啊 根本不好使。  把 case  xx ： break;  改成了  xx  =>  ,
            ConsoleColor cc = ConsoleColor.Red;
            static string FromCC(ConsoleColor consoleColor)
            {
                var tmp = consoleColor switch
                {
                    ConsoleColor.Red => "Red",
                    _ => "default",//弃元 _ 代表 default
                };
                return tmp;
            }
            //新的switch的写法只支持有返回值的，如果没有返回值需要回到老版写法，但是匹配模式都是支持的（没有选词也太差了）
            switch (cc)
            {
                case ConsoleColor ccRed when ccRed == ConsoleColor.Black:
                    break;
                default:
                    break;
            };
            //加上属性模式后
            static string FromObj(object objMode)
                => objMode switch
                {
                    Point pointObj => "Point Obj",//如果是 Point 类型就匹配
                    PointF pfObj =>
                    pfObj switch
                    {
                        //下面这种用法需要class里面有  public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);
                        //(0, 0) => "位置模式匹配了00",
                        //var (x, y) when x > 0 && y > 0 => "",
                    },
                    FileStream fs =>// 如果是 fs 类型就匹配
                    fs switch
                    {
                        { CanRead: true, Length: > 100 } => "More than 100 length and can read fs",  //如果可以读，并且长度属性大于100，则匹配
                        _ => (fs.SafeFileHandle, fs.CanWrite) switch//defalut
                        {
                            //真的离谱，真真离谱,但是switch现在终于可以用 && 条件了，虽然语法面目全非
                            ({ IsInvalid: true, IsInvalid: true }, true) => "这个就是元组模式的switch结果了"
                        }
                    }
                };
            #endregion
            #region using 终于不用再用两个括号了 ，泪奔
            static int WriteLinesToFile(IEnumerable<string> lines)
            {
                using var file = new System.IO.StreamWriter("WriteLines2.txt");
                int skippedLines = 0;
                foreach (string line in lines)
                {
                    if (!line.Contains("Second"))
                    {
                        file.WriteLine(line);
                    }
                    else
                    {
                        skippedLines++;
                    }
                }
                // Notice how skippedLines is in scope here.
                return skippedLines;
                // file is disposed here   这里就会调用disposed
            }
            #endregion
            #region    异步流，其实就是yield return 可以异步去执行
            static async System.Collections.Generic.IAsyncEnumerable<int> GenerateSequence()
            {
                for (int i = 0; i < 20; i++)
                {
                    await Task.Delay(100);
                    yield return i;
                }
            }
            static async void TestFunc()//创建这个方法是因为 Main 方法不能为异步的，因为上面的一些语法
            {
                await foreach (var number in GenerateSequence())
                {
                    Console.WriteLine(number);
                }
            }
            #endregion
            #region 索引和范围
            var indexs = new string[] { "a", "b", "c", "d", "e", "f" };
            Console.WriteLine(indexs[^1]);//^n 的意思是  indexs.Length - n ,所以 ^1 的意思是取倒数第一个
            var taked = indexs[1..4];//等价于下面 也就是取了 index 1~3的内容  { "b", "c", "d" }
            var sameTaked = indexs.Skip(1).Take(3);
            #endregion
            #endregion

            #region C# 9.0  
            #region record 记录类型
            //用法一般是作为一个不可变的class，节约声明这个class的代码
            Person teacher = new Teacher("Nancy", "Davolio", 3);
            Person student = new Student("Nancy", "Davolio", 3);
            //类型不对 ，属性相等做不到相等
            Console.WriteLine(teacher == student); // output: False
            Student student2 = new Student("Nancy", "Davolio", 3);
            //类型，属性都对上才能相等，可以节约开发时间。
            Console.WriteLine(student2 == student); // output: True
            //复制一个对象之后修改值， with的用法  ，此时student3 和 student2 是在内存的不同地址
            var student3 = student2 with { FirstName = "Shaun" };
            var pns = new string[2];
            var ts1 = new Person("Shaun", "Chen", pns);
            //复制的过程中不修改任何东西
            var ts2 = ts1 with { };
            Console.WriteLine($"ts1==ts2{ts1 == ts2}");
            ts1.PhoneNumbers[0] = "111";
            //下面这个输出仍旧是 true的，说明在复制副本的过程中，引用类型的属性没有完全得到复制
            Console.WriteLine($"after change pn,ts1.PhoneNumbers[0]:{ts1.PhoneNumbers[0]},ts2.PhoneNumbers[0]:{ts2.PhoneNumbers[0]},ts1==ts2{ts1 == ts2}");
            //内置tostring 有一定格式 ，用起来很方便
            Console.WriteLine($"record class tostring: {ts1.ToString()}");
            Console.WriteLine(ts2);
            #endregion
            #region init
            //这玩意儿可以替代 private set  ，当你的类的属性的set访问器是init的时候，在外部new时，可以跟大括号赋值，之后都是相当于private set 的了（在类内部直接设置私有字段，而非设置属性）。
            var testInit1 = new TestInit();//此时 InitStr 取值构造函数的赋值
            var testInit2 = new TestInit() { InitStr = "Init in new" };
            testInit2.FixInitStr("fix para not property in function");
            #endregion
            //匹配模式增强
            object improved = 1;
            if (improved is double or >= 1 and (<= 3 or 4) or not Enum)
                Console.WriteLine("nothing happen");
            #endregion

            #region C# 10.0
            // record struct 或 readonly record struct 声明声明值类型记录,可以通过 record class 声明阐明 record 是引用类型(默认是class）
            #endregion

            Console.WriteLine("Hello World!");
        }

        public class 元组
        {
            private int a;
            public int A
            {
                get => a;
                set => a = value;
            }

            private int b;
            public int B
            {
                get { return b; }
                set { b = value; }
            }

            public 元组(int a, int b) => (A, B) = (a, b);

            public (string a, int b, double c) 元组方法() => ("string", 1, 2);
        }

        public /*abstract*/ record Person(string FirstName, string LastName, string[] PhoneNumbers);

        public record Teacher(string FirstName, string LastName, int Grade) : Person(FirstName, LastName, null);

        public record Student(string FirstName, string LastName, int Grade) : Person(FirstName, LastName, null);

        public class TestInit
        {
            private string initStr;
            public string InitStr
            {
                get => initStr;
                init => initStr = value;
            }

            public TestInit()
            {
                InitStr = "init in 构造函数";
            }

            public void FixInitStr(string input)
            {
                //注意这里也会报错
                //InitStr = "fix in public function";
                //只能这样
                initStr = input;
            }
        }
    }
}
