using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Properties;
using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Livet;
using Livet.EventListeners;

namespace Grabacr07.KanColleViewer.ViewModels
{
	public class GamePageViewModel : ViewModel
	{
		public GamePageViewModel()
		{
			this.CompositeDisposable.Add(new PropertyChangedEventListener(KanColleClient.Current.Homeport.GamePage)
			{
				{ "PageName", (sender, args) => this.UpdatePageName() },
			});
		}

		private void UpdatePageName()
		{
			int prefix;
			switch (KanColleClient.Current.Homeport.GamePage.PageName)
			{
				case "nyukyo":
					prefix = 5;
					break;
				case "practice":
					prefix = 3;
					break;
				case "map":
					prefix = 2;
					break;
				case "mission":
					prefix = 4;
					break;
				case "kousyou":
					prefix = 6;
					break;
				default:
					return;
			}

			List<Quest> Quests = KanColleClient.Current.Homeport.Quests.All
				.Where(x => x.Type == QuestType.Daily || x.Type == QuestType.Weekly)
				.Where(x => x.State == QuestState.None && x.Id / 100 == prefix)
				.ToList();
			if (Quests.Count > 0)
			{
				WindowsNotification.Notifier.Show(
					Resources.Quests,
					string.Format("以下の任務が未着手です - {0}", Quests[0].Title),
					() => App.ViewModelRoot.Activate());
			}
		}
	}
}
