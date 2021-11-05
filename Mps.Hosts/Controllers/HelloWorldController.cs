using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Mps.Hosts.Controllers
{
    //Route 特性即设置路由特性
    //有以下四种东西组成：
    //1、[area]   特殊标识，表示 Controller 类上方 Area特性中的字符。
    //2、[controller]   特殊标识，表示 Controller 类的名称
    //3、[action]   特殊标识，表示方法名字
    //4、常量 随便写什么东西  比如下面的ImIPA  一般会使用  api
    //以上四项先后随便打乱使用
    //最后是 特性 HttpGet 等  里面的字符又是一个路径，加在 Route 特性字符的结尾
    [Route("ImIPA/[area]/[action]/[controller]")]
    [Area("TestArea")]
    public class HelloWorldController : Controller
    {
        // 
        // GET: /HelloWorld/
        [HttpGet("ts/tb")]
        public string Index()
        {
            return "This is my default action...";
        }

        // 
        // GET: /HelloWorld/Welcome/ 

        public string Welcome(string name,int numTimes)
        {
            return HtmlEncoder.Default.Encode($"Hello {name}, NumTimes is: {numTimes}");
        }
    }
}
