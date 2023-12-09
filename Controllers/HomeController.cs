using countries.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Components;



namespace countries.Controllers
{
    // Контоллер
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        //читаем данные и передаем их списком
        CountyList list = new CountyList(null);
        List<MyItemCity> data = null;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

            Task<List<MyItemCity>> t = list.GetList();
            t.Wait();
            data = t.Result;

        }

        //Индекс
        [Microsoft.AspNetCore.Mvc.Route("")]
        [Microsoft.AspNetCore.Mvc.Route("Home")]
        [Microsoft.AspNetCore.Mvc.Route("Home/Index")]
        public IActionResult Index()
        {
           return this.View(data.AsQueryable()); //View();
        }

        //Возврат деталей
        [Microsoft.AspNetCore.Mvc.Route("Home/Details/{Index?}")]
        public IActionResult Details(string? Index)
        {
            var ix = data.Where(i => i.Index ==  Index);

            MyItemCity item = null;

            if (ix == null)
            {

                foreach (MyItemCity i in data)
                {
                    if (i.Index == Index)
                    {
                        item = i;
                        break;
                    }
                }
            }
            else
            { 
                item=ix.First();
            }


            //var send = new List<MyItemCity>() { item };

            return this.View(item);
                //send.AsQueryable());
           
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
