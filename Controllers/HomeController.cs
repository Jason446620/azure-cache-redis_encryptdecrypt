using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AzureCacheRedis.Models;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace AzureCacheRedis.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, IConfiguration iConfig)
        {
            _logger = logger;
            _configuration = iConfig;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult RedisCache()
        {
            ViewBag.Message = "A simple example with Azure Cache for Redis on ASP.NET.";


            var lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                string cacheConnection = _configuration.GetSection("CacheConnection").Value;
                return ConnectionMultiplexer.Connect(cacheConnection);
            });


            // Connection refers to a property that returns a ConnectionMultiplexer
            // as shown in the previous example.
            IDatabase cache = lazyConnection.Value.GetDatabase();


            // Perform cache operations using the cache object...


            // Simple PING command
            //ViewBag.command1 = "PING";
            //ViewBag.command1Result = cache.Execute(ViewBag.command1).ToString();


            // Simple get and put of integral data types into the cache
            //ViewBag.command2 = "GET Message";
            //ViewBag.command2Result = cache.StringGet("Message").ToString();


            //ViewBag.command3 = "SET Message \"Hello! The cache is working from ASP.NET!\"";
            //ViewBag.command3Result = cache.StringSet("Message", "Hello! The cache is working from ASP.NET!").ToString();


            // Demonstrate "SET Message" executed as expected...
            //ViewBag.command4 = "GET Message";
            //ViewBag.command4Result = cache.StringGet("Message").ToString();


            // Get the client list, useful to see if connection list is growing...
            //ViewBag.command5 = "CLIENT LIST";
            //ViewBag.command5Result = cache.Execute("CLIENT", "LIST").ToString().Replace(" id=", "\rid=");


            string original = "Here is some data to encrypt!";
            string guid = Guid.NewGuid().ToString();

            byte[] myRijndaelKey;
            byte[] myRijndaelIV;

            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                myRijndael.GenerateKey();
                myRijndael.GenerateIV();
                myRijndaelKey = myRijndael.Key;
                myRijndaelIV = myRijndael.IV;
            }
            byte[] encrypted_original = EncryptandDecrypt.EncryptStringToBytes(original, myRijndaelKey, myRijndaelIV);

            ViewBag.command6 = original;
            ViewBag.command6Result = encrypted_original.ToString();
            //set orginal data
            cache.StringSet(guid, encrypted_original);
            //set key and iv
            cache.StringSet(guid+"Key", myRijndaelKey);
            cache.StringSet(guid+"IV", myRijndaelIV);


            //get data from redis
            byte[] get_encrypted_originalByte = (byte[])cache.StringGet(guid);
            //get data from redis
            byte[] get_Key = (byte[])cache.StringGet(guid+"Key");
            //get data from redis
            byte[] get_IV = (byte[])cache.StringGet(guid+"IV");


            string decrypted_originalString = EncryptandDecrypt.DecryptStringFromBytes(get_encrypted_originalByte, get_Key, get_IV);

            ViewBag.command7 = "Get From Redis:"+ get_encrypted_originalByte.ToString();
            ViewBag.command7Result = "decrypted data:" + decrypted_originalString;

            lazyConnection.Value.Dispose();


            return View();
        }
    }
}
