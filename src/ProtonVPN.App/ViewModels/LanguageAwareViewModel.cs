using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Caliburn.Micro;
using ProtonVPN.Core.Settings;
using ProtonVPN.Resources;

namespace ProtonVPN.ViewModels
{
    public class LanguageAwareViewModel : Screen, ISettingsAware
    {
        private readonly List<string> _rightToLeftLanguages = new List<string> {"fa"};

        public FlowDirection FlowDirection =>
            _rightToLeftLanguages.Contains(TranslationSource.Instance.CurrentCulture.TwoLetterISOLanguageName)
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;

        public virtual void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(IAppSettings.Language)))
            {
                NotifyOfPropertyChange(() => FlowDirection);
            }
        }
    }
}