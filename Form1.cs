using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using System.Activities.Expressions;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Graph;
using System.Threading.Tasks;
using System.Net.Http;

namespace Exercicio01
{
    public partial class Form1 : Form
    {
        
        public class Media
        {

            public string Cidade { get; set; }
            public Double Idade  { get; set; }

        }

        private static readonly char[] s_Diacritics = GetDiacritics();
        private static char[] GetDiacritics()
        {
            char[] accents = new char[256];

            for (int i = 0; i < 256; i++)
                accents[i] = (char)i;

            accents[(byte)'á'] = accents[(byte)'à'] = accents[(byte)'ã'] = accents[(byte)'â'] = accents[(byte)'ä'] = 'a';
            accents[(byte)'Á'] = accents[(byte)'À'] = accents[(byte)'Ã'] = accents[(byte)'Â'] = accents[(byte)'Ä'] = 'A';

            accents[(byte)'é'] = accents[(byte)'è'] = accents[(byte)'ê'] = accents[(byte)'ë'] = 'e';
            accents[(byte)'É'] = accents[(byte)'È'] = accents[(byte)'Ê'] = accents[(byte)'Ë'] = 'E';

            accents[(byte)'í'] = accents[(byte)'ì'] = accents[(byte)'î'] = accents[(byte)'ï'] = 'i';
            accents[(byte)'Í'] = accents[(byte)'Ì'] = accents[(byte)'Î'] = accents[(byte)'Ï'] = 'I';

            accents[(byte)'ó'] = accents[(byte)'ò'] = accents[(byte)'ô'] = accents[(byte)'õ'] = accents[(byte)'ö'] = 'o';
            accents[(byte)'Ó'] = accents[(byte)'Ò'] = accents[(byte)'Ô'] = accents[(byte)'Õ'] = accents[(byte)'Ö'] = 'O';

            accents[(byte)'ú'] = accents[(byte)'ù'] = accents[(byte)'û'] = accents[(byte)'ü'] = 'u';
            accents[(byte)'Ú'] = accents[(byte)'Ù'] = accents[(byte)'Û'] = accents[(byte)'Ü'] = 'U';

            accents[(byte)'ç'] = 'c';
            accents[(byte)'Ç'] = 'C';

            accents[(byte)'ñ'] = 'n';
            accents[(byte)'Ñ'] = 'N';

            accents[(byte)'ÿ'] = accents[(byte)'ý'] = 'y';
            accents[(byte)'Ý'] = 'Y';

            return accents;
        }

        List<string[]> listA = new List<string[]>();
        List<Media> listB = new List<Media>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            openDialog.Filter = "CSV Files (*.csv)|*.csv";
            openDialog.FileName = "Selecione o arquivo CSV";
            openDialog.Title = "Abrir arquivo CSV";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string RemoveDiacritics(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return text;

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] > 255)
                        sb.Append(text[i]);
                    else
                        sb.Append(s_Diacritics[text[i]]);
                }

                return sb.ToString();
            }

            async Task AddPostAsync(String mediassend)
            {
                HttpClient client = new HttpClient();
                try
                {
                    string url = "https://zeit-endpoint-brmaeji.vercel.app/api/avg/";                                  
                    var uri = new Uri(string.Format(url));
                    var data = JsonConvert.SerializeObject(mediassend);
                    var content = new StringContent(data, Encoding.UTF8, "application/json");
                    string url2 = "https://zeit-endpoint-brmaeji.vercel.app/api/avg/";
                    var uri2 = new Uri(string.Format(url2));
                    content.Headers.ContentLocation = uri2;
                    client.DefaultRequestHeaders.TransferEncodingChunked = true;
                    HttpResponseMessage response = null;
                    response = await client.PostAsync(uri, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Erro ao enviar json");
                    }
                    else
                    {
                        MessageBox.Show("Médias Enviadas com Sucesso");
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sr = new StreamReader(openDialog.FileName, Encoding.UTF7);
                    var resul = new string[2];
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        var values = line.Split(',');

                        listA.Add(values);

                    }

                    foreach (var city in listA)
                    {
                        city[1] = RemoveDiacritics(city[1]);
                        city[1] = city[1].ToUpper();

                        Media media = new Media();
                        media.Cidade = city[1];
                        media.Idade = Convert.ToDouble(city[2]);

                        listB.Add(media);

                    }

                    var listaResul = listB.OrderBy(o => o.Idade).GroupBy(x => new { ID = x.Cidade }).Select(g => new
                    {
                        Average = g.Average(p => p.Idade),
                        ID = g.Key.ID
                    }).ToList();

                    var json = @"{""medias"":[";
                    var i = 0;
                    foreach (var medias in listaResul)
                    {
                        json = json + @"{""cidade"" : """ + medias.ID + @""", ""idade"" : " + medias.Average.ToString("F").Replace(",", ".") + @"}";
                        if (!(i == listaResul.Count-1))
                        {
                            json = json + @",";
                        }
                        i++;
                    }

                    json = json + @"]}";

                    Task task = AddPostAsync(json);

                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }
    }
}
