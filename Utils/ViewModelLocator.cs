using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using CommonServiceLocator;
using StudySmarterFlashcards.Menus;
using StudySmarterFlashcards.Sets;

namespace StudySmarterFlashcards.Utils
{
  // Found at http://joeljoseph.net/navigation-between-pages-using-mvvm-light-in-universal-windows-platform-uwp/
  public class ViewModelLocator
  {
    public const string MainMenuPageKey = "MainMenuPage";
    public const string SetPageKey = "SetPage";
    public const string EditSetPageKey = "EditSetPage";

    public ViewModelLocator()
    {
      ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
      var nav = new NavigationService();
      nav.Configure(MainMenuPageKey, typeof(MainMenuPage));
      nav.Configure(SetPageKey, typeof(SetPage));
      nav.Configure(EditSetPageKey, typeof(EditSetPage));

      SimpleIoc.Default.Register<INavigationService>(() => nav);
      SimpleIoc.Default.Register<MainMenuViewModel>();
      SimpleIoc.Default.Register<SetViewModel>();
      SimpleIoc.Default.Register<EditSetViewModel>();
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
  }
}

