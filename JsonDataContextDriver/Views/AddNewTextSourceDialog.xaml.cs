using JsonDataContextDriver.Inputs;

using Newtonsoft.Json.Linq;

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

namespace JsonDataContextDriver.Views
{
    public partial class AddNewTextSourceDialog : Window
    {
        private readonly SolidColorBrush _goodBrush = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush _badBrush = new SolidColorBrush(Colors.IndianRed) { Opacity = .5 };

        private readonly JsonTextInput _input;

        public JsonTextInput Input { get; set; }

        public AddNewTextSourceDialog()
        {
            _input = new JsonTextInput();

            InitializeComponent();

            // sigh, too bad reactiveui doesn't support .net 4
            Action doValidation = () =>
            {
                var name = NameTextBox.Text;
                var json = JsonTextBox.Text;

                bool validJson = false;
                try
                {
                    var obj = JContainer.Parse(json);
                    validJson = true;
                }
                catch { }

                var nameOk = String.IsNullOrEmpty(name) || char.IsLetter(name.ToCharArray()[0]);
                var jsonOk = validJson || String.IsNullOrEmpty(json);

                OkButton.IsEnabled = (nameOk && jsonOk && !String.IsNullOrEmpty(json) && !string.IsNullOrWhiteSpace(name));

                NameTextBox.Background = nameOk ? _goodBrush : _badBrush;
                JsonTextBox.Background = jsonOk ? _goodBrush : _badBrush;
            };

            NameTextBox.TextChanged += (sender, args) => doValidation();
            JsonTextBox.TextChanged += (sender, args) => doValidation();

            CancelButton.Click += (sender, args) => DialogResult = false;
            OkButton.Click += (sender, args) =>
            {
                _input.Name = NameTextBox.Text;
                _input.Json = JsonTextBox.Text;

                Input = _input;
                DialogResult = true;
            };

            doValidation();

            NameTextBox.Focus();
        }

        public AddNewTextSourceDialog(JsonTextInput input) : this()
        {
            _input = input;

            NameTextBox.Text = _input.Name;
            JsonTextBox.Text = _input.Json;
        }
    }
}
