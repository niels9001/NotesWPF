using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NotesWPF
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            var jsonstring = "{\r\n  \"settings\": [\r\n    {\r\n      \"category\": \"Image Settings\",\r\n      \"options\": [\r\n        {\r\n          \"name\": \"Resolution\",\r\n          \"type\": \"dropdown\",\r\n          \"values\": [\"720p\", \"1080p\", \"4K\"],\r\n          \"default\": \"1080p\",\r\n          \"description\": \"Select the resolution for capturing images.\"\r\n        }\r\n      ]\r\n    }\r\n  ]\r\n}";
            var res = FormatJson(jsonstring);
            jsonsettings.Text = res;
        }
        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
