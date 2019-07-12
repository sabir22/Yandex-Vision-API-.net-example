using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.IO;

//Newtonsoft.Json
//The MIT License (MIT)
//Copyright (c) 2007 James Newton-King https://www.newtonsoft.com/json

//This project is an example of how to use Yandex.Vision from Yandex.Cloud
//https://cloud.yandex.ru/services/vision
//Please read rules before using Yandex services
//https://yandex.ru/legal/cloud_oferta/
//https://yandex.ru/legal/cloud_aup/

namespace Json.YC.Reader
{
    public static class JYCR
    {
        /// <summary>
        /// Get text lines with words from JSON request
        /// </summary>
        /// <param name="jsonText">JSON result after recognizing</param>
        /// <returns>List of text lines (jLine)</returns>
        public static List<jLine> getTextLines (string jsonText) //read YC json request
        {
            if (jsonText.Contains("Error") || jsonText.Contains("xpired")) throw new InvalidOperationException("Connection problem with Yandex Cloud Vision");
            JObject json = JObject.Parse(jsonText);
            var jtextLines = json.SelectTokens("$.....lines"); //ignoring other segments before lines
            List<jLine> lines = new List<jLine>();
            foreach (JToken jtextLine in jtextLines)
            {
                foreach (var jwordsLine in jtextLine.Children()) //getting all parts of text lines
                {
                    jLine line = new jLine(jwordsLine);
                    lines.Add(line);
                }
            }
            return lines;
        }
        /// <summary>
        /// Get text lines with words from JSON request
        /// </summary>
        /// <param name="jsonText">JSON result after recognizing</param>
        /// <returns>List of text lines (jLine)</returns>
        public static List<jBlock> getTextBlocks(string jsonText) //read YC json request
        {
            if (jsonText.Contains("Error") || jsonText.Contains("xpired")) throw new InvalidOperationException("Connection problem with Yandex Cloud Vision");
            JObject json = JObject.Parse(jsonText);
            var jtextBlocks = json.SelectTokens("$....blocks"); //ignoring other segments before lines
            List<jBlock> blocks = new List<jBlock>();
            foreach (JToken jtextBlock in jtextBlocks)
            {
                foreach (var jLines in jtextBlock.Children()) //getting all parts of text lines
                {
                    jBlock block = new jBlock(jLines);
                    blocks.Add(block);
                }
            }
            return blocks;
        }
        public static Size getPageSize(string jsonText)//page size
        {
            if (jsonText.Contains("Error") || jsonText.Contains("xpired")) throw new InvalidOperationException("Connection problem with Yandex Cloud Vision");
            JObject json = JObject.Parse(jsonText);
            var jPage = json.SelectTokens("$...pages"); //ignoring other segments before pages
            int width = (int)jPage.ElementAt(0).Children().ElementAt(0).SelectToken("width");
            int height = (int)jPage.ElementAt(0).Children().ElementAt(0).SelectToken("height");
            return new Size(width, height);
        }
        #region Options
        //your YC folder
        private const string folderID = "b...g6d...40....."; 
        
        //your YC token (if not using api key)
        private const string token = "";// @"....................q-N70KVnBqiHhSiuNThwn8-ACIHeornEDgbWovxED6..........................................-Yyv4JJ_HH-.....................-TfpU1P6JsEf0L4dyZyZ6c...............GN-7qRw2oxqZEZhmSVnfv3uYttBupCfEKl1MXjXaYvHlGeNE............................................";
        
        //your YC api key (if not using IAM token)
        private const string apikey = @"AQ......NyoK-....";

        //YC api vision URL
        private static Uri url = new Uri("https://vision.api.cloud.yandex.net/vision/v1/batchAnalyze");

        //list of languages https://cloud.yandex.ru/docs/vision/concepts/ocr/supported-languages
        private const string lang = "['ru','en']";
        #endregion

        private static async Task<string> PostRec(string value) //POST JSON request for recognition 
        {
            string text ="";
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                }
                else if (!string.IsNullOrEmpty(apikey))
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Api-Key " + apikey);
                }
                else
                {
                    throw new InvalidOperationException("No keys to access!");
                }
                var content = new StringContent(value, Encoding.UTF8, "application/json");
                var result = await client.PostAsync(url, content);
                text = await result.Content.ReadAsStringAsync();
            }
            return text;
        }
        public static async Task<string> PostImage(string fileName) //bulding body of JSON request
        {
            Byte[] bytes = File.ReadAllBytes(fileName); //you can also convert stream from Bitmap (below)
            return await PostRec(jSONbody(bytes));
        }
        public static async Task<string> PostImage(Bitmap bmp) //bulding body of JSON request
        {
            Byte[] bytes = ImageToByte(bmp); 
            return await PostRec(jSONbody(bytes));
        }
        private static string jSONbody(Byte[] bytes) //body of request
        {
            string imgString64 = Convert.ToBase64String(bytes); //image into base64
            return @"{
                'folderId': '" + folderID + @"',
                'analyze_specs': [{
                    'content':" + @" '" + imgString64 + @"'," +
                                      @"'features': [{
                        'type': 'TEXT_DETECTION',
                        'text_detection_config': {
                            'language_codes': " + lang + @"
                        }
                    }]
                }]
            }";
        } 
        public static byte[] ImageToByte(Bitmap img) //bitmap to byte array
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Close();
                byteArray = stream.ToArray();
            }
            return byteArray;
        }
        /// <summary>
        /// recognize image request (async)
        /// </summary>
        /// <param name="fileName">image file name</param>
        /// <returns>JSON result</returns>
        public static async Task<string> ycRecognizeImageASYNC(string fileName) //async operation
        {
            return await JYCR.PostImage(fileName);
        }
        /// <summary>
        /// recognize image request (async)
        /// </summary>
        /// <param name="bmp">Bitmap image</param>
        /// <returns>JSON result</returns>
        public static async Task<string> ycRecognizeImageASYNC(Bitmap bmp) //async operation
        {
            return await JYCR.PostImage(bmp);
        }
        /// <summary>
        /// recognize image request (sync)
        /// </summary>
        /// <param name="fileName">image file name</param>
        /// <returns>JSON result</returns>
        public static string ycRecognizeImageSYNC(string fileName) //sync operation for waiting of result
        {
            return Task.Run(async () => { return await ycRecognizeImageASYNC(fileName); }).Result;
        }
        /// <summary>
        /// recognize image request (sync)
        /// </summary>
        /// <param name="bmp">Bitmap image</param>
        /// <returns>JSON result</returns>
        public static string ycRecognizeImageSYNC(Bitmap bmp) //sync operation for waiting of result
        {
            return Task.Run(async () => { return await ycRecognizeImageASYNC(bmp); }).Result;
        }

    }
    public class jLine //text line
    {
        public Rectangle Rect { get; set; }
        public List<jWord> Words { get; set; }
        public string Text { get; set; }
        public decimal Confidense { get; set; }
        public jLine (Rectangle rect, List<jWord> words, decimal conf = 0M)
        {
            this.Rect = rect;
            this.Words = words;
            this.Confidense = conf;
        }
        internal jLine(JToken jtLine) //collecting lines with coordinates
        {
            int x1, x2, y1, y2;
            //try-catch used for missing coordinates
            try { x1 = (int)jtLine.SelectToken("boundingBox.vertices[0].x", true); }
            catch { x1 = 0; }
            try { y1 = (int)jtLine.SelectToken("boundingBox.vertices[0].y", true); }
            catch { y1 = 0; } 
            try { x2 = (int)jtLine.SelectToken("boundingBox.vertices[2].x", true); }
            catch { x2 = 0; }
            try { y2 = (int)jtLine.SelectToken("boundingBox.vertices[2].y", true); }
            catch { y2 = 0; }
            this.Rect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            var jwords = jtLine.SelectTokens("words");
            Confidense = (decimal)jtLine.SelectToken("confidence");
            List<jWord> words = new List<jWord>();
            foreach (var jword in jwords) //collecting words with coordinates
            {
                foreach (var jw in jword.Children())//all parts
                {
                    //try-catch used for missing coordinates
                    try { x1 = (int)jw.SelectToken("boundingBox.vertices[0].x", true); }
                    catch { x1 = 0; }
                    try { y1 = (int)jw.SelectToken("boundingBox.vertices[0].y", true); }
                    catch { y1 = 0; }
                    try { x2 = (int)jw.SelectToken("boundingBox.vertices[2].x", true); }
                    catch { x2 = 0; }
                    try { y2 = (int)jw.SelectToken("boundingBox.vertices[2].y", true); }
                    catch { y2 = 0; }
                    Rectangle rect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                    string text = (string)jw.SelectToken("text");
                    decimal confW = (decimal)jw.SelectToken("confidence");
                    words.Add(new jWord(rect, text, confW));
                }
            }
            this.Words = words;
            this.Text = String.Join(" ", words.Select(p => p.Text).ToArray());
        }
    }
    public class jWord //word
    {
        public Rectangle Rect { get; set; }
        public string Text { get; set; }
        public decimal Confidense { get; set; }
        public jWord(Rectangle rect, string text, decimal conf = 0M)
        {
            this.Rect = rect;
            this.Text = text;
            this.Confidense = conf;
        }
    }

    public class jBlock
    {
        public Rectangle Rect { get; set; }
        public List<jLine> Lines { get; set; }
        public string Text { get; set; }
        public jBlock (Rectangle rect, List<jLine> lines)
        {
            this.Rect = rect;
            this.Lines = lines;
        }
        internal jBlock(JToken jtBlock) //collecting lines with coordinates
        {
            int x1, x2, y1, y2;
            //try-catch used for missing coordinates
            try { x1 = (int)jtBlock.SelectToken("boundingBox.vertices[0].x", true); }
            catch { x1 = 0; }
            try { y1 = (int)jtBlock.SelectToken("boundingBox.vertices[0].y", true); }
            catch { y1 = 0; }
            try { x2 = (int)jtBlock.SelectToken("boundingBox.vertices[2].x", true); }
            catch { x2 = 0; }
            try { y2 = (int)jtBlock.SelectToken("boundingBox.vertices[2].y", true); }
            catch { y2 = 0; }
            this.Rect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            var jlines = jtBlock.SelectTokens("lines");
            List<jLine> lines = new List<jLine>();
            foreach (var jline in jlines) //collecting strings with coordinates
            {
                foreach (var jw in jline.Children())//all parts
                {                   
                    lines.Add(new jLine(jw));
                }
            }
            this.Lines = lines;
            this.Text = String.Join("\r\n", lines.Select(p => p.Text).ToArray());
        }
    }
}
