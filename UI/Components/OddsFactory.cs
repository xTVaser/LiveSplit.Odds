using LiveSplit.Model;
using System;

namespace LiveSplit.UI.Components
{
    public class OddsFactory : IComponentFactory
    {
        public string ComponentName => "Odds";

        public string Description => "Displays the current odds.";

        public ComponentCategory Category => ComponentCategory.Information; 

        public IComponent Create(LiveSplitState state) => new OddsComponent(state);

        public string UpdateName => ComponentName;

        public string UpdateURL => "http://livesplit.org/update/";

        public Version Version => Version.Parse("1.0.0");

        public string XMLURL => string.Empty;
    }
}
