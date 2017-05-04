using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public class OddsComponent : IComponent {

        private LiveSplitState state;

        public OddsComponent(LiveSplitState state) {

            VerticalHeight = 10;
            Settings = new OddsSettings();
            Cache = new GraphicsCache();
            this.state = state;
            
            state.OnReset += state_OnReset;
            CurrentState = state;
            CurrentState.RunManuallyModified += CurrentState_RunModified;

            CalculateOdds();
        }
        
        public GraphicsCache Cache { get; set; }
        public OddsSettings Settings { get; set; }
        protected LiveSplitState CurrentState { get; set; }

        public string ComponentName => "Odds";

        public float HorizontalWidth { get; set; }

        public float MinimumHeight { get; set; }

        public float VerticalHeight { get; set; }

        public float MinimumWidth { get { return OddsNameLabel.X + OddsValueLabel.ActualWidth; } }

        public float PaddingTop { get; set; }

        public float PaddingBottom { get; set; }

        public float PaddingLeft { get { return 7f; } }

        public float PaddingRight { get { return 7f; } }

        protected SimpleLabel OddsNameLabel = new SimpleLabel();
        protected SimpleLabel OddsValueLabel = new SimpleLabel();

        private List<Double> completeCurrentSplit = new List<Double>();
        private List<Double> saveTimeCurrentSplit = new List<Double>();
        private List<Double> completeRun = new List<Double>();

        public IDictionary<string, Action> ContextMenuControls { get { return null; } }
        
        // TODO some of the segment history is incorrect at times, most likely due to new splits being added or removed
        void CalculateOdds() {

            ClearOdds();

            int segmentIndex = 0;
            
            IRun run = state.Run;
            int startIndex = 0;
            int endIndex = run.Count;

            int previousAttempts = 0;

            Time pbDelta = Time.Zero;

            // Loop through every split
            for (int i = startIndex; i < endIndex; i++) {

                SegmentHistory history = state.Run[i].SegmentHistory;
                Time pbTime = state.Run[i].PersonalBestSplitTime;

                var attempts = history.Keys;

                // Something is wrong with the splits, due to adding new ones, etc, so just discount it.
                if (attempts.Count > run.AttemptCount || attempts.Count > previousAttempts) {
                    completeCurrentSplit.Add(1.0);
                    ////previousAttempts = attempts.Count + previousAttempts; // Add onto the next one as they were not used in this calculation
                }

                else { 
                    if (i == 0)
                        // Chances of completeing current split
                        completeCurrentSplit.Add((Double)attempts.Count / run.AttemptCount);
                    else
                        // Chances of completeing current split
                        completeCurrentSplit.Add((Double)attempts.Count / previousAttempts);
                }

                int historySavedTime = 0;
                // Calculate chances of saving time on current split
                foreach (int attempt in attempts) {

                    Time time;
                    bool success = history.TryGetValue(attempt, out time);
                    if (success && time.RealTime != null && time.RealTime.Value <= (pbTime-pbDelta).RealTime.Value)
                        // If the previous segment saved time
                        historySavedTime++;
                }

                pbDelta += pbTime;

                if (attempts.Count > historySavedTime) {
                    saveTimeCurrentSplit.Add(1.0);
                    previousAttempts = attempts.Count + historySavedTime; // Add onto the next one as they were not used in this calculation
                }
                else {
                    // Store chances of saving time on current split
                    saveTimeCurrentSplit.Add((Double)historySavedTime / attempts.Count);
                }

                previousAttempts = attempts.Count;

                segmentIndex++;
            }

            Double previousOdds = 1;

            // Completing the run, combined probability of completing current individual splits.
            for (int i = completeCurrentSplit.Count - 1; i >= 0; i--) {

                completeRun.Insert(0, previousOdds * completeCurrentSplit[i]);
                previousOdds = previousOdds * completeCurrentSplit[i];
            }
        }

        void ClearOdds() {

            completeCurrentSplit.Clear();
            saveTimeCurrentSplit.Clear();
            completeRun.Clear();
        }

        void CurrentState_RunModified(object sender, EventArgs e) {
            CalculateOdds();
        }

        // Reset the odds calculations
        void state_OnReset(object sender, TimerPhase e) {
            CalculateOdds();
        }

        protected Font OddsFont { get; set; }

        private void DrawGeneral(Graphics g, Model.LiveSplitState state, float width, float height, LayoutMode mode) {

            // Set Background colour.
            if (Settings.BackgroundColor.ToArgb() != Color.Transparent.ToArgb()
                || Settings.BackgroundGradient != GradientType.Plain
                && Settings.BackgroundColor2.ToArgb() != Color.Transparent.ToArgb()) {
                var gradientBrush = new LinearGradientBrush(
                            new PointF(0, 0),
                            Settings.BackgroundGradient == GradientType.Horizontal
                            ? new PointF(width, 0)
                            : new PointF(0, height),
                            Settings.BackgroundColor,
                            Settings.BackgroundGradient == GradientType.Plain
                            ? Settings.BackgroundColor
                            : Settings.BackgroundColor2);

                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }

            // Set Font.
            OddsFont = state.LayoutSettings.TextFont;

            // Calculate Height from Font.
            var textHeight = g.MeasureString("A", OddsFont).Height;
            VerticalHeight = 1.2f * textHeight;
            MinimumHeight = MinimumHeight;

            PaddingTop = Math.Max(0, ((VerticalHeight - 0.75f * textHeight) / 2f));
            PaddingBottom = PaddingTop;

            // Measure width of max odds
            float fourCharWidth = g.MeasureString("99.99% / 99.99%", OddsFont).Width;
            HorizontalWidth = OddsNameLabel.X + OddsNameLabel.ActualWidth + 
                (fourCharWidth > OddsValueLabel.ActualWidth ? fourCharWidth : OddsValueLabel.ActualWidth) + 5;

            // Set Odds Name Label
            OddsNameLabel.HorizontalAlignment = mode == LayoutMode.Horizontal ? StringAlignment.Near : StringAlignment.Near;
            OddsNameLabel.VerticalAlignment = StringAlignment.Center;
            OddsNameLabel.X = 5;
            OddsNameLabel.Y = 0;
            OddsNameLabel.Width = (width - fourCharWidth - 5);
            OddsNameLabel.Height = height;
            OddsNameLabel.Font = OddsFont;
            OddsNameLabel.Brush = new SolidBrush(Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor);
            OddsNameLabel.HasShadow = state.LayoutSettings.DropShadows;
            OddsNameLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            OddsNameLabel.Draw(g);

            // Set Odds Value Label.
            OddsValueLabel.HorizontalAlignment = mode == LayoutMode.Horizontal ? StringAlignment.Far : StringAlignment.Far;
            OddsValueLabel.VerticalAlignment = StringAlignment.Center;
            OddsValueLabel.X = 5;
            OddsValueLabel.Y = 0;
            OddsValueLabel.Width = (width - 10);
            OddsValueLabel.Height = height;
            OddsValueLabel.Font = OddsFont;
            OddsValueLabel.Brush = new SolidBrush(Settings.OverrideOddsColor ? Settings.OddsColor : state.LayoutSettings.TextColor);
            OddsValueLabel.HasShadow = state.LayoutSettings.DropShadows;
            OddsValueLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            OddsValueLabel.Draw(g);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) {

            DrawGeneral(g, state, HorizontalWidth, height, LayoutMode.Horizontal);
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) {

            DrawGeneral(g, state, width, VerticalHeight, LayoutMode.Vertical);
        }

        public XmlNode GetSettings(XmlDocument document) {

            return Settings.GetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode) {

            return Settings;
        }

        public void SetSettings(XmlNode settings) {

            Settings.SetSettings(settings);
        }

        private String RetrieveOdds(int selectedIndex, int splitIndex) {
            
            switch (selectedIndex) {

                case 0: return completeCurrentSplit[splitIndex].ToString("P");
                case 1: return saveTimeCurrentSplit[splitIndex].ToString("P");
                case 2: return completeRun[splitIndex].ToString("P");
                default: return "??.??%";
            }
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode) {

            this.state = state;
            int currentSplitIndex = state.CurrentSplitIndex;

            // Create Header
            String header = "Odds (";
            header += Settings.ValueToString(Settings.ValueOneIndex);
            if (Settings.DisplayTwoValues)
                header += " / " + Settings.ValueToString(Settings.ValueTwoIndex);

            header += ")";
            OddsNameLabel.Text = header;

            // Display the odds
            if (currentSplitIndex == -1 || currentSplitIndex >= completeCurrentSplit.Count)
                OddsValueLabel.Text = "-- / --";

            else {
                String values = RetrieveOdds(Settings.ValueOneIndex, currentSplitIndex);
                if (Settings.DisplayTwoValues)
                    values += " / " + RetrieveOdds(Settings.ValueTwoIndex, currentSplitIndex);
                OddsValueLabel.Text = values;
            }
            
            Cache.Restart();
            Cache["OddsNameLabel"] = OddsNameLabel.Text;
            Cache["OddsValueLabel"] = OddsValueLabel.Text;

            if (invalidator != null && Cache.HasChanged) {
                invalidator.Invalidate(0, 0, width, height);
            }
        }

        public void Dispose() {
            
            state.OnReset -= state_OnReset;
        }
    }
}
