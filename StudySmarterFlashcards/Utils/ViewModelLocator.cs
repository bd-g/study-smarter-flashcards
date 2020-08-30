using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using StudySmarterFlashcards.Menus;
using StudySmarterFlashcards.Sets;
using StudySmarterFlashcards.Study;

namespace StudySmarterFlashcards.Utils
{
  // Found at http://joeljoseph.net/navigation-between-pages-using-mvvm-light-in-universal-windows-platform-uwp/
  public class ViewModelLocator
  {
    public const string MainMenuPageKey = "MainMenuPage";
    public const string SetPageKey = "SetPage";
    public const string EditSetPageKey = "EditSetPage";
    public const string BasicStudyPageKey = "BasicStudyPage";
    public const string FillBlankStudyPageKey = "FillBlankStudyPage";
    public const string MultipleChoiceStudyPageKey = "MultipleChoiceStudyPage";
    public const string SettingsPageKey = "SettingsPage";

    public ViewModelLocator()
    {
      ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
      var nav = new NavigationService();
      nav.Configure(MainMenuPageKey, typeof(MainMenuPage));
      nav.Configure(SetPageKey, typeof(SetPage));
      nav.Configure(EditSetPageKey, typeof(EditSetPage));
      nav.Configure(BasicStudyPageKey, typeof(BasicStudyPage));
      nav.Configure(FillBlankStudyPageKey, typeof(FillBlankStudyPage));
      nav.Configure(MultipleChoiceStudyPageKey, typeof(MultipleChoiceStudyPage));
      nav.Configure(SettingsPageKey, typeof(SettingsPage));

      SimpleIoc.Default.Register<INavigationService>(() => nav);
      SimpleIoc.Default.Register<MainMenuViewModel>();
      SimpleIoc.Default.Register<SetViewModel>();
      SimpleIoc.Default.Register<EditSetViewModel>();
      SimpleIoc.Default.Register<BasicStudyViewModel>();
      SimpleIoc.Default.Register<FillBlankStudyViewModel>();
      SimpleIoc.Default.Register<MultipleChoiceStudyViewModel>();
      SimpleIoc.Default.Register<SettingsViewModel>();
    }

    public MainMenuViewModel MainMenuInstance
    {
      get
      {
        return ServiceLocator.Current.GetInstance<MainMenuViewModel>();
      }
    }

    public SetViewModel SetInstance
    {
      get
      {
        return ServiceLocator.Current.GetInstance<SetViewModel>();
      }
    }

    public EditSetViewModel EditSetInstance
    {
      get
      {
        return ServiceLocator.Current.GetInstance<EditSetViewModel>();
      }
    }

    public BasicStudyViewModel BasicStudyInstance
    {
      get
      {
        return ServiceLocator.Current.GetInstance<BasicStudyViewModel>();
      }
    }
    public FillBlankStudyViewModel FillBlankStudyInstance
    {
      get
      {
        return ServiceLocator.Current.GetInstance<FillBlankStudyViewModel>();
      }
    }
    public MultipleChoiceStudyViewModel MultipleChoiceStudyInstance
    {
      get
      {
        return ServiceLocator.Current.GetInstance<MultipleChoiceStudyViewModel>();
      }
    }
    public SettingsViewModel SettingsInstance
    {
      get
      {
        return ServiceLocator.Current.GetInstance<SettingsViewModel>();
      }
    }
  }
}

