using QRCoder;
using System.Drawing;
using Newtonsoft.Json;

int Main(string[] args)
{
    string url = "thiswebsite.io";
    const string rootPath = @"[rootpath]";
    List<string> clues = new List<string>();
    using (var reader = new StreamReader(@$"{rootPath}\ClueGenerator\clues.csv"))
    {
        while (!reader.EndOfStream)
        {
            clues.Add(reader.ReadLine());
        }
    }

    Console.WriteLine($"There are {clues.Count} clues in clues.csv");
    Console.WriteLine("Would you like to generate a website? [Y/N]");

    string input = Console.ReadLine();

    switch (input.ToLower())
    {
        case "n":
            Console.WriteLine("Exiting Program");
            break;
        case "y":
            Console.WriteLine("Generating Pages ...");
            GeneratePages(clues, url, rootPath);
            Console.WriteLine("Webpage Generation Complete!");
            break;
        default:
            Console.WriteLine("Invalid Input. Shutting Down... :(");
            break;
    }
    return 0;
}

void GeneratePages(List<string> clues, string url, string rootPath)
{
    string date = DateTime.Now.ToString("yyyy-MM-dd-hhmmss");
    string dir = @$"{rootPath}/ScavengerHunt_{date}";
    Directory.CreateDirectory(dir);

    List<Image> qrCodes = new();
    List<string> clueList = new();
    Dictionary<string,string> cluesList = new();

    int clueNum = 1;
    foreach (string clue in clues)
    {

        string guid = Guid.NewGuid().ToString();
        string link = $"{url}/clue/{guid}";

        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            Console.WriteLine("bing bong");
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            //File.WriteAllBytes(filepath, qrCodeImage);
            Image qrImage = (Bitmap)(new ImageConverter()).ConvertFrom(qrCodeImage);
            qrCodes.Add(qrImage);
        }

        cluesList.Add(guid, clue);
        clueNum++;
    }

    string cluesJson = JsonConvert.SerializeObject(cluesList);
    cluesJson.Replace("[", "").Replace("]", "");
    Console.WriteLine(cluesJson);

    string page = "";

    using (var reader = new StreamReader(@$"{rootPath}\ClueGenerator\template.html"))
        {
            string template = "";
            while (!reader.EndOfStream)
            {
                template += reader.ReadLine() + Environment.NewLine;
            }
            page = template;
            
        }

        int cluesMapIdx = page.IndexOf("const cluesMap = {");
        page = page.Insert(cluesMapIdx, cluesJson);

        File.WriteAllText(@$"{dir}/index.html", page);

    int maxPages = qrCodes.Count % 6 == 0 ? qrCodes.Count / 6 : (qrCodes.Count / 6) + 1;
    int x = 0;
    int y = 0;
    int count = 1;
    int pageNum = 1;

    for (int i = 0; i < maxPages; i++)
    {
        bool fullPage = qrCodes.Count >= 6;
        List<Image> qrCodeBatch = fullPage ? qrCodes.GetRange(0, 6) : qrCodes;
        if (fullPage) qrCodes.RemoveRange(0, 6);
        Bitmap bitmap = new(1800, 2700);

        using (Graphics grfx = Graphics.FromImage(bitmap))
        {
            foreach (var item in qrCodeBatch)
            {
                grfx.DrawImage(item, x, y);
                grfx.DrawString(count.ToString(), new Font("Tahoma", 14), Brushes.Black, x + 450, y + 850);
                bitmap.Save(@$"{dir}\clue_QR_CODE_all_{pageNum}.png");
                
                x = x == 0 ? 900 : 0;
                y = x == 0 ? y + 900 : y;
                count++;
            }
            pageNum++;
            y = 0;
        }
    }
}

Main(args);
