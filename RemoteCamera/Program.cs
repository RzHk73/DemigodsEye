using Emgu.CV;
using Emgu.CV.Structure;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace SendImage
{
    internal class Program
    {
        private static string endpoint = "";
        private static float interval = 600; 

        static void Main(string[] args)
        {
            try
            {
                endpoint = (Environment.GetEnvironmentVariable("ENDPOINT") ?? "http://localhost:5000") + "/Home/SetImage";
                Console.WriteLine($"Endpoint is {endpoint}");

                string strInterval = Environment.GetEnvironmentVariable("INTERVAL") ?? interval.ToString();
                bool gotInterval = float.TryParse(strInterval, out interval);

                if (!gotInterval)
                {
                    Console.WriteLine("Fps not valid");
                }
                else
                {
                    Console.WriteLine($"FPS set to {strInterval}");
                }

                StartTakingImages().Wait();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());

                Console.WriteLine("\n\n Press any key to exit....");
                Console.ReadKey(true);
            }
        }


        static async Task StartTakingImages()
        {
            await Console.Out.WriteLineAsync("Starting up... ");

            using (HttpClient client = new HttpClient())
            using (VideoCapture vc = new VideoCapture())
            {
                Mat imageTensor = new Mat();
                HttpResponseMessage response = null;

                vc.Start(); // Start vc subsystem

                if (vc.IsOpened)
                {
                    await Console.Out.WriteLineAsync("Starting Loop");
                    await Console.Out.WriteLineAsync();

                    while (true)
                    {
                        vc.Read(imageTensor); // Take picture;

                        var image = imageTensor.ToImage<Bgr, byte>();
                        byte[] data = image.ToJpegData(100); // Convert to jpeg

                        try
                        {
                            // Send the image to the listing server
                            var content = new ByteArrayContent(data);
                            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                            response = await client.PostAsync(endpoint, content);
                            await Console.Out.WriteLineAsync("Sending Packet..");
                            await Console.Out.WriteLineAsync($"Response was: {response.StatusCode}\n");
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync("Error sending image \n");
                            await Console.Out.WriteLineAsync(ex.ToString());
                        }

                        Thread.Sleep((int)(1000 * interval));
                    }
                }

                vc.Stop();
            }
        }
    }
}
