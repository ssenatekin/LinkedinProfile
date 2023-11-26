using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using System;
using GiveAChance.Models;
using iText.Kernel.Pdf;
using Document = iText.Layout.Document;
using iText.IO.Image;
using System.Text;
using iText.Kernel.Font;
using iText.IO.Font;
using iTextSharp.text;
using Paragraph = iText.Layout.Element.Paragraph;
using Image = iText.Layout.Element.Image;

namespace GiveAChance.Controllers
{
    public class LinkedInController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public LinkedInController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public ActionResult LinkedinLogin()
        {
            string clientId = _configuration["Client:client_id"];
            string clientSecret = _configuration["Client:client_secret"];
            string redirectUri = _configuration["Client:redirectUri"];
            string scope = _configuration["Client:scope"];                            
            string state = "YOUR_STATE_VALUE"; // Güvenlik için değer

            string authorizationUrl = $"https://www.linkedin.com/oauth/v2/authorization?" +
            $"response_type=code&" +
            $"client_id={clientId}&" +
            $"redirect_uri={redirectUri}&" +
            $"state={state}&" +
            $"scope={scope}";

            ViewBag.auth = authorizationUrl;

            return View();
        }

        public ActionResult LinkedINAuth(string code, string state)
        {
            //This method path is your return URL  
            try
            {
                //Get Accedd Token  
                var client = new RestClient("https://www.linkedin.com/oauth/v2/accessToken");
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AddParameter("grant_type", "authorization_code");
                request.AddParameter("code", code);
                request.AddParameter("redirect_uri", "https://localhost:7108/LinkedIn/LinkedINAuth");
                request.AddParameter("client_id", _configuration["Client:client_id"]);
                request.AddParameter("client_secret", _configuration["Client:client_secret"]);
                RestResponse response = client.Execute(request);
                var content = response.Content;
                var res = (JObject)JsonConvert.DeserializeObject(content);

                JToken jToken = JToken.Parse(content);
                // JToken içindeki belirli bir değeri çekme
                string sst = jToken.SelectToken("access_token")?.ToString();

                //var st= res.Root.First;
                var linkedINView = JsonConvert.DeserializeObject<LinkedINViewModel>(content);

                //Get Profile Details
                client = new RestClient("https://api.linkedin.com/v2/userinfo?oauth2_access_token=" + linkedINView.access_token);
                //client = new RestClient("https://api.linkedin.com/v2/me"  /* ?oauth2_access_token=" + sst  */ );
                request = new RestRequest();
                request.Method = Method.Get;
                //request.AddHeader("Authorization", $"Bearer{sst}");
                response = client.Execute(request);
                content = response.Content;

                //jsonSerializer = new JavaScriptSerializer();
                var linkedINResVM = JsonConvert.DeserializeObject<LinkedINResVM>(content);

                string imagePath = null;

                if (linkedINResVM.picture != null)
                {
                    imagePath = linkedINResVM.picture;

                }

                byte[] jsonDataBytes = Encoding.Latin1.GetBytes(content);
                //byte[] pdfBytes = ConvertJsonToPdf(linkedINResVM.given_name + " " + linkedINResVM.family_name, imagePath);


                using (MemoryStream stream = new MemoryStream())
                {
                    using (var pdfWriter = new PdfWriter(stream))
                    {
                        using (var pdf = new PdfDocument(pdfWriter))
                        {
                            using (var document = new Document(pdf))
                            {
                                PdfFont font = PdfFontFactory.CreateFont(FontFactory.TIMES_ROMAN, PdfEncodings.UTF8);
                                if (imagePath != null)
                                {
                                    Image img = new Image(ImageDataFactory.Create(new Uri(imagePath)));
                                    document.Add(img);
                                }
                                document.Add(new Paragraph(Encoding.Latin1.GetString(jsonDataBytes)).SetFont(font));
                            }
                        }
                    }
                    // MemoryStream kapatılmaz, bu nedenle pdfBytes değişkenine kopyalanır.
                    var pdfbytes= stream.ToArray();
                    return File(pdfbytes, "application/pdf", "LinkedinCV.pdf");
                }

            }
            catch
            {
                throw;
            }

        }
    }
}