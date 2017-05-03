using LiveSplit.TimeFormatters;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class OddsSettings : UserControl
    {

        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }
        public GradientType BackgroundGradient { get; set; }
        public string GradientString {
            get { return BackgroundGradient.ToString(); }
            set { BackgroundGradient = (GradientType)Enum.Parse(typeof(GradientType), value); }
        }

        public Color TextColor { get; set; }
        public bool OverrideTextColor { get; set; }
        public Color OddsColor { get; set; }
        public bool OverrideOddsColor { get; set; }

        public bool DisplayTwoValues { get; set; }
        public int ValueOneIndex { get; set; }
        public int ValueTwoIndex { get; set; }

        public LayoutMode Mode { get; set; }

        public OddsSettings()
        {
            InitializeComponent();

            TextColor = Color.FromArgb(255, 255, 255);
            OverrideTextColor = false;
            OddsColor = Color.FromArgb(255, 255, 255);
            OverrideOddsColor = false;
            BackgroundColor = Color.Transparent;
            BackgroundColor2 = Color.Transparent;
            BackgroundGradient = GradientType.Plain;

            DisplayTwoValues = false;
            ValueOneIndex = 0;
            ValueTwoIndex = 0;
            
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            
            chkOverrideTextColor.DataBindings.Add("Checked", this, "OverrideTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnTextColor.DataBindings.Add("BackColor", this, "TextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            
            chkOverrideOddsColor.DataBindings.Add("Checked", this, "OverrideOddsColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnOddsColor.DataBindings.Add("BackColor", this, "OddsColor", false, DataSourceUpdateMode.OnPropertyChanged);
            
            chkDisplayTwoValues.DataBindings.Add("Checked", this, "DisplayTwoValues", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbValue1.DataBindings.Add("SelectedIndex", this, "ValueOneIndex", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbValue2.DataBindings.Add("SelectedIndex", this, "ValueTwoIndex", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public String ValueToString(int n) {
            
            switch (n) {
                case 0: return "FS"; 
                case 1: return "STS"; 
                case 2: return "FR";
                default: return "--";
            }
        }
        
        void chkOverrideTextColor_CheckedChanged(object sender, EventArgs e)
        {
            label1.Enabled = btnTextColor.Enabled = chkOverrideTextColor.Checked;
        }

        private void chkOverrideOddsColor_CheckedChanged(object sender, EventArgs e) {

            label2.Enabled = btnOddsColor.Enabled = chkOverrideOddsColor.Checked;
        }

        private void chkDisplayTwoValues_CheckedChanged(object sender, EventArgs e) {

            label4.Enabled = cmbValue2.Enabled = chkDisplayTwoValues.Checked;
        }

        void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnColor1.Visible = cmbGradientType.SelectedItem.ToString() != "Plain";
            btnColor2.DataBindings.Clear();
            btnColor2.DataBindings.Add("BackColor", this, btnColor1.Visible ? "BackgroundColor2" : "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            GradientString = cmbGradientType.SelectedItem.ToString();
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            BackgroundColor = SettingsHelper.ParseColor(element["BackgroundColor"]);
            BackgroundColor2 = SettingsHelper.ParseColor(element["BackgroundColor2"]);
            GradientString = SettingsHelper.ParseString(element["BackgroundGradient"]);
            TextColor = SettingsHelper.ParseColor(element["TextColor"]);
            OverrideTextColor = SettingsHelper.ParseBool(element["OverrideTextColor"]);
            OddsColor = SettingsHelper.ParseColor(element["OddsColor"]);
            OverrideOddsColor = SettingsHelper.ParseBool(element["OverrideOddsColor"]);
            DisplayTwoValues = SettingsHelper.ParseBool(element["DisplayTwoValues"]);
            ValueOneIndex = SettingsHelper.ParseInt(element["ValueOneIndex"]);
            ValueTwoIndex = SettingsHelper.ParseInt(element["ValueTwoIndex"]);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsNode(null, null);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "Version", "1.4") ^
            SettingsHelper.CreateSetting(document, parent, "TextColor", TextColor) ^
            SettingsHelper.CreateSetting(document, parent, "OverrideTextColor", OverrideTextColor) ^
            SettingsHelper.CreateSetting(document, parent, "OddsColor", OddsColor) ^
            SettingsHelper.CreateSetting(document, parent, "OverrideOddsColor", OverrideOddsColor) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundColor", BackgroundColor) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundColor2", BackgroundColor2) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundGradient", BackgroundGradient) ^
            SettingsHelper.CreateSetting(document, parent, "DisplayTwoValues", DisplayTwoValues) ^
            SettingsHelper.CreateSetting(document, parent, "ValueOneIndex", ValueOneIndex) ^
            SettingsHelper.CreateSetting(document, parent, "ValueTwoIndex", ValueTwoIndex);
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            SettingsHelper.ColorButtonClick((Button)sender, this);
        }

        private void Color2BtnClick(object sender, EventArgs e) {

            SettingsHelper.ColorButtonClick((Button)sender, this);
        }
    }
}
