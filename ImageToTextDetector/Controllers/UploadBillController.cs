namespace ImageToTextDetector.Controllers;

[ApiController]
[Route("[controller]")]
public class UploadBillController : ControllerBase
{
    private readonly IConfiguration _configuration;
    public UploadBillController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> IndexAsync(IFormFile billImage)
    {
        #region Using Gemini Api
        var base64String = string.Empty;
        var fileExtension = Path.GetExtension(billImage.FileName).Split(".")[1].ToString();
        fileExtension = fileExtension.ToLower() == "jpg" ? "jpeg" : fileExtension;
        using (MemoryStream memoryStream = new MemoryStream())
        {
            await billImage.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();
            base64String = Convert.ToBase64String(fileBytes);
        }

        var client = new HttpClient();
        var requestPayload = new RequestPayload
        {
            Contents = new[]
            {
                new Content
                {
                    Parts = new[]
                    {
                        new Part {
                            Text = "Give me Total Amount, Bill Date and Predict bill category in single word from given list of category (Online, Shopping, Fuel, Electronic, Market) remember 3 answer seprate by coma and dont use any extra label. single word only and date formate should be yyyy-mm-dd."
                        },
                        new Part
                        {
                            InlineData = new InlineData
                            {
                                MimeType = "image/"+fileExtension+"",
                                Data = base64String
                            }
                        }
                    }
                }
            }
        };

        string jsonPayload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(jsonPayload, null, "application/json");

        string url = string.Concat(_configuration.GetSection("ApiUrl").Value, _configuration.GetSection("ApiKey").Value);
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = content;

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return Ok(new { status = false, data = "", msg = "No data ditected." });
        }
        else
        {
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();
            dynamic jsonObject = JsonConvert.DeserializeObject(responseData);

            if (jsonObject != null)
            {
                string candidateText = jsonObject.candidates[0].content.parts[0].text;
                var model = new BillDto();

                if (!string.IsNullOrEmpty(candidateText.Split(",")[0]))
                {
                    model.Amount = decimal.Parse(candidateText.Split(",")[0]);
                }

                if (!string.IsNullOrEmpty(candidateText.Split(",")[1]))
                {
                    model.BillDate = DateTime.ParseExact(candidateText.Split(",")[1].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    //model.BillDate = DateTime.Parse(candidateText.Split(",")[1]);
                }

                if (!string.IsNullOrEmpty(candidateText.Split(",")[2]))
                {
                    model.BillType = candidateText.Split(",")[2].Trim();
                }

                return Ok(new { status = true, data = model, msg = "Successfully data ditected." });
            }
            else
            {
                return Ok(new { status = false, data = "", msg = "No data ditected." });
            }
        }
        #endregion
    }
}