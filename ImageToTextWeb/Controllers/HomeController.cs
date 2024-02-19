using ImageToTextWeb.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ImageToTextWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AnaliseAsync([FromBody] InstructionDto dto)
        {
            if (dto != null)
            {
                try
                {
                    var client = new HttpClient();
                    //var request = new HttpRequestMessage(HttpMethod.Post, "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent?key=AIzaSyDGfYywmvxMqJrPlKryYtinOeIKVg5W4Nw");
                    //var content = new StringContent("{ \"contents\":[\r\n{ \"parts\":[{\"text\": \"" + dto.Text + " give date and total amount.\"}]}\r\n    ]\r\n}", null, "application/json");
                    //request.Content = content;
                    //var response = await client.SendAsync(request);
                    //response.EnsureSuccessStatusCode();
                    //await response.Content.ReadAsStringAsync();
                    //return Json(new { status = true, data = Response });

                    var requestUrl = "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent?key=AIzaSyDGfYywmvxMqJrPlKryYtinOeIKVg5W4Nw";
                    var requestData = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new
                                    {
                                        //text = dto.Text + " give date and total amount."
                                        text = dto.Text + " give date, total amount and bill catagory."
                                    }
                                }
                            }
                        }
                    };

                    var jsonString = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(requestUrl, content);
                    response.EnsureSuccessStatusCode();

                    var responseData = await response.Content.ReadAsStringAsync();

                    dynamic jsonObject = JsonConvert.DeserializeObject(responseData);

                    string candidateText = jsonObject.candidates[0].content.parts[0].text;

                    string extractedDate = ExtractDate(candidateText);
                    string extractedAmount = ExtractAmount(candidateText);


                    return Json(new { status = true, data = candidateText });
                }
                catch (Exception ex)
                {
                    return Json(new { status = false, data = "" });
                }
            }
            return Json(new { status = false, data = "" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        static string ExtractDate(string inputText)
        {
            // Matches date in the format dd-mm-yyyy
            Regex dateRegex = new Regex(@"\b(\d{2}-\d{2}-\d{4})\b");
            Match match = dateRegex.Match(inputText);

            return match.Success ? match.Groups[1].Value : "Date not found";
        }

        static string ExtractAmount(string inputText)
        {
            // Matches amounts in the format xxx.xx
            Regex amountRegex = new Regex(@"\b(\d+\.\d{2})\b");
            Match match = amountRegex.Match(inputText);

            return match.Success ? match.Groups[1].Value : "Amount not found";
        }
    }
}
